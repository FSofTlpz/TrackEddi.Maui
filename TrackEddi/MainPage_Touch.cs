#define USESTDGESTURES        // ACHTUNG: MainPage.xaml entsprechend anpassen!

#if DEBUG
//#define DEBUGTHISPAGE
//#define DEBUGZOOM
#endif

using FSofTUtils.Geometry;
using GMap.NET;

#if !USESTDGESTURES
using FSofTUtils.OSInterface;
#endif
using SpecialMapCtrl;
using System.Collections.Concurrent;
using System.Diagnostics;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;

namespace TrackEddi {
   public partial class MainPage {

      // ACHTUNG: interne Klasse analog "friend" in C++


      /// <summary>
      /// Es können entweder die 3 Touch-Gesture-Funktionen vom GestureRecognizer direkt verwendet werden oder 
      /// die internen Eigenbau-Funktionen
      /// </summary>
      internal static class TouchInterface {

         const double WAITINGPERIOD = 0.5;      // Karenzzeit für Drag nach Zoom

         /// <summary>
         /// Pan läuft gerade
         /// </summary>
         static bool panIsRunning = false;
         /// <summary>
         /// akt. Gesamtverschiebung des Pan
         /// </summary>
         static Point panDelta = Point.Zero;

         /// <summary>
         /// Pinch läuft gerade
         /// </summary>
         static bool pinchIsRunning = false;
         /// <summary>
         /// Zeitpunkt des letzten Zooms
         /// </summary>
         static DateTime dtLastPinch = DateTime.MinValue;
         /// <summary>
         /// "Zentrum" für Pinch
         /// </summary>
         static PointD pinchOrigin = PointD.Empty;
         /// <summary>
         /// gesamter zusätzlicher Zoom beim Pinch
         /// </summary>
         static double totalPinchScale = 1;

         public static double StartZoomLinear = 1;


         /// <summary>
         /// 
         /// </summary>
         /// <param name="mainpage"></param>
         /// <param name="mymap"></param>
         /// <param name="position">Maui-Koordinaten</param>
         /// <returns></returns>
         public static async Task Tapped(MainPage mainpage, MapCtrl mymap, Point? position) {
            resetZoomData(mymap);
            stopPan(mainpage, mymap);
            if (position != null) {
#if DEBUGTHISPAGE
               Debug.WriteLine(">>> TouchInterface.Tapped: position=" + position);
#endif
               await mainpage.mapTapped((Point)position, false);
            }
         }

         /// <summary>
         /// Pan / Verschieben
         /// </summary>
         /// <param name="mainpage"></param>
         /// <param name="mymap"></param>
         /// <param name="status">Status der Verschiebung</param>
         /// <param name="deltaxtotal">horizontale Gesamtverschiebung in Maui-Koordinaten</param>
         /// <param name="deltaytotal">vertikale Gesamtverschiebung in Maui-Koordinaten</param>
         public static void Pan(MainPage mainpage,
                                MapCtrl mymap,
                                GestureStatus status,
                                double deltaxtotal,
                                double deltaytotal) {
            bool end = status == GestureStatus.Completed;
            panDelta = new Point(deltaxtotal, deltaytotal);
#if DEBUGTHISPAGE
            Debug.WriteLine(">>> TouchInterface.Pan: end=" + end + " panDelta=" + panDelta);
#endif
            resetZoomData(mymap);
            switchOffSelfCentering(mainpage);

            if (MainThread.IsMainThread)
               touchDrag(mainpage, mymap, panDelta, end);
            else
               MainThread.BeginInvokeOnMainThread(() => touchDrag(mainpage, mymap, panDelta, end));
         }

         /// <summary>
         /// Pinch / Zoom
         /// </summary>
         /// <param name="mainpage"></param>
         /// <param name="mymap"></param>
         /// <param name="status"></param>
         /// <param name="scale">Faktor bezüglich des letzten (!) Pinch</param>
         /// <param name="scaleorigin">Bezugspunkt als Bruchteil zur Höhe und Breite des Controls</param>
         public static async Task Pinch(MainPage mainpage,
                                        MapCtrl mymap,
                                        GestureStatus status,
                                        double scale,
                                        Point scaleorigin) {
            stopPan(mainpage, mymap);
            switch (status) {
               case GestureStatus.Started:
               case GestureStatus.Running:
               case GestureStatus.Completed:
                  if (status == GestureStatus.Started || !pinchIsRunning) {
                     resetZoomData(mymap);
                     setPinchOrigin(mymap, scaleorigin);
                     pinchIsRunning = true;
                  }
                  if (scale != 1) {
                     switchOffSelfCentering(mainpage);
                     totalPinchScale *= scale;
                     if (await touchZoomAsync(mymap, pinchOrigin, StartZoomLinear, totalPinchScale))
                        dtLastPinch = DateTime.Now;
#if DEBUGTHISPAGE
                     Debug.WriteLine(">>> TouchInterface.Pinch: pinchorigin=" + pinchorigin + " totalscale=" + totalscale + " status=" + status);
#endif
                  }
                  if (status == GestureStatus.Completed)
                     resetZoomData(mymap);
                  break;
               default:
                  resetZoomData(mymap);
                  break;
            }
         }

         static void setPinchOrigin(MapCtrl mymap, Point scaleorigin) =>
            pinchOrigin = Helper2.Maui2LatLon(new Point(scaleorigin.X * mymap.ControlWidth,
                                                        scaleorigin.Y * mymap.ControlHeight),
                                              mymap);

         static void stopPan(MainPage mainpage, MapCtrl mymap) {
            if (panIsRunning) {
               Pan(mainpage, mymap, GestureStatus.Completed, panDelta.X, panDelta.Y);
               panDelta = Point.Zero;
               panIsRunning = false;
            }
         }

         static void resetZoomData(MapCtrl mymap) {
            StartZoomLinear = mymap.M_ZoomLinear;
            pinchIsRunning = false;
            pinchOrigin = PointD.Empty;
            totalPinchScale = 1;
         }

         /// <summary>
         /// falls in <see cref="MainPage"/> das <see cref="GeoLocation.LocationSelfCentering"/> aktiv ist, 
         /// wird es ausgeschaltet
         /// </summary>
         /// <param name="mainpage"></param>
         static void switchOffSelfCentering(MainPage mainpage) {
            if (mainpage.geoLocation != null &&
                mainpage.geoLocation.LocationIsShowing &&
                mainpage.geoLocation.LocationSelfCentering) {
               mainpage.geoLocation.LocationSelfCentering = false;
               mainpage.ButtonGeoLocationStart.IsVisible = false;
               mainpage.ButtonGeoLocationWithoutCenter.IsVisible = true;
               mainpage.ButtonGeoLocationStop.IsVisible = false;
            }
         }


         /// <summary>
         /// 
         /// </summary>
         /// <param name="mainpage"></param>
         /// <param name="mymap"></param>
         /// <param name="delta">akt. (Gesamt-)Verschiebung in Maui-Koordinaten</param>
         /// <param name="islastpt">true wenn Ende der Verschiebung</param>
         static async void touchDrag(MainPage mainpage, MapCtrl mymap, Point delta, bool islastpt) {
#if DEBUGTHISPAGE
            Debug.WriteLine(">>> TouchInterface.touchDrag: delta=" + delta + " islastpt=" + islastpt);
#endif
            //StartZoomLinear = 0;


            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine("");
            //sb.AppendLine("org M_IsDragging=" + mymap.M_IsDragging
            //               + " islastpt=" + islastpt
            //               + " delta=" + delta
            //               + " deltatime=" + DateTime.Now.Subtract(dtLastPinch).TotalSeconds);

            //if (mymap.logtxt.Length > 0) {
            //   sb.Append(mymap.logtxt.ToString());
            //   mymap.logtxt.Clear();
            //}

            //Common.UIHelper.Message2Logfile("touchDrag", sb.ToString(), App.LogFilename);




            /* Nach einem Zoom kommt oft noch ein unbeabsichtigtes Drag (weitere Bewegung nur noch mit 1 Finger).
             * Deshalb wird hier eine kurze "Karenzzeit" von 0.2s verwendet.
             * */
            if (DateTime.Now.Subtract(dtLastPinch).TotalSeconds > WAITINGPERIOD) {
               panIsRunning = true;
#if DEBUGTHISPAGE
               Debug.WriteLine(">>> TouchInterface.touchDrag: used");
#endif
               if (!mymap.M_IsDragging)
                  await mymap.MapDragStart(delta);
               if (islastpt)
                  await mymap.MapDragEnd(delta);
               else {
                  await mymap.MapDrag(delta);
                  // falls das LocationSelfCentering aktiv ist: ausschalten
                  if (mainpage.geoLocation != null &&
                      mainpage.geoLocation.LocationIsShowing &&
                      mainpage.geoLocation.LocationSelfCentering) {
                     mainpage.geoLocation.LocationSelfCentering = false;
                     mainpage.ButtonGeoLocationStart.IsVisible = false;
                     mainpage.ButtonGeoLocationWithoutCenter.IsVisible = true;
                     mainpage.ButtonGeoLocationStop.IsVisible = false;
                  }
               }
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="mymap"></param>
         /// <param name="origin">Mittelpunkt für den Zoom</param>
         /// <param name="startZoomLinear">linearer Zoom beim Start des Zoomens</param>
         /// <param name="factor">Zoomfaktor (gesamt)</param>
         /// <returns>true wenn Zoom erfolgt ist</returns>
         static async Task<bool> touchZoomAsync(MapCtrl mymap, PointD origin, double startZoomLinear, double factor) {
            double newZoomLinear = Math.Min(Math.Max(mymap.M_MinZoomLinear, startZoomLinear * factor), mymap.M_MaxZoomLinear);
            if (startZoomLinear > 0 &&
                mymap.M_ZoomLinear != newZoomLinear) {
               factor = newZoomLinear / startZoomLinear;    // falls eingegrenzt
#if DEBUGZOOM
               Debug.WriteLine("ZOOM: origin=" + origin);

               double actDeltaY = mymap.M_CenterLat - origin.Y;
               double actDeltaX = mymap.M_CenterLon - origin.X;
               Debug.WriteLine("ZOOM: actDeltaX=" + actDeltaX + " actDeltaY=" + actDeltaY);

               // wegen map.Map_ZoomLinear = startzoomlinear * factor:
               double actfactor = mymap.M_ZoomLinear / StartZoomLinear;
               Debug.WriteLine("ZOOM: actfactor=" + actfactor);

               double newDeltaY = actDeltaY * actfactor / factor;
               double newDeltaX = actDeltaX * actfactor / factor;
               Debug.WriteLine("ZOOM: newDeltaX=" + newDeltaX + " newDeltaY=" + newDeltaY);

               double newDeltaDeltaY = newDeltaY - actDeltaY;
               double newDeltaDeltaX = newDeltaX - actDeltaX;
               Debug.WriteLine("ZOOM: newDeltaDeltaX=" + newDeltaDeltaX + " newDeltaDeltaY=" + newDeltaDeltaY);

               Debug.WriteLine("ZOOM: M_Position=" + mymap.M_Position);
               mymap.M_SetLocation(mymap.M_Position.Lng + newDeltaDeltaX,
                                   mymap.M_Position.Lat + newDeltaDeltaY);
               Debug.WriteLine("ZOOM: new M_Position=" + mymap.M_Position);

               mymap.M_SetZoom(Math.Log(StartZoomLinear * factor, 2) + mymap.M_MinZoom);
#endif
               // analog aber etwas optimiert:
               double k = mymap.M_ZoomLinear / startZoomLinear / factor;
#if DEBUGZOOM

               StringBuilder sb = new StringBuilder();
               sb.AppendLine("");
               sb.Append("org origin=" + new PointLatLng(origin.Y, origin.X) + " factor=" + factor
                         + " M_Position=" + mymap.M_Position + " M_Zoom=" + mymap.M_Zoom + " M_ZoomLinear=" + mymap.M_ZoomLinear
                         + " set " + new PointLatLng(mymap.M_Position.Lat * k - origin.Y * (k - 1),
                                                  mymap.M_Position.Lng * k - origin.X * (k - 1)) + " "
                         + startZoomLinear * factor
                         + " M_IsDragging=" + mymap.M_IsDragging);
#endif

               await mymap.M_SetLocationAndZoomLinearAsync(newZoomLinear, //startZoomLinear * factor,
                                                     mymap.M_Position.Lng * k - origin.X * (k - 1),
                                                     mymap.M_Position.Lat * k - origin.Y * (k - 1));

#if DEBUGZOOM
               sb.AppendLine(" res M_Position=" + mymap.M_Position + " M_Zoom=" + mymap.M_Zoom + " M_ZoomLinear=" + mymap.M_ZoomLinear);

               Common.UIHelper.Message2Logfile("touchZoom", sb.ToString(), App.LogFilename);
#endif
#if DEBUGTHISPAGE
               Debug.WriteLine(">>> TouchInterface.touchZoom: StartZoomLinear=" + StartZoomLinear +
                                                                  " factor=" + factor +
                                                                  " Map_ZoomLinear=" + mymap.M_ZoomLinear +
                                                                  " Map_Position=" + mymap.M_Position);
#endif
               return true;
            }
            return false;
         }
      }

      #region Touch-Reaktionen

#if !USESTDGESTURES

      private void mapTouchAction(object sender, TouchEffect.TouchActionEventArgs args) =>
        touchHandling.MapTouchAction(sender, args);

      TouchHandling touchHandling;

      void initTouchHandling() {
         touchHandling = new TouchHandling();

         GestureStatus pinchstatus = GestureStatus.Completed;
         GestureStatus panstatus = GestureStatus.Completed;

         Point ptDragStart = Point.Zero;
         Point ptScalePinchOrigin = Point.Zero;
         double lastpinchzoom = 1;

         touchHandling.TapDown += (sender, e) => {
            // alle ev. noch "laufenden" Gesten beenden
            if (pinchstatus != GestureStatus.Completed) {
               pinchstatus = GestureStatus.Completed;
               TouchInterface.Pinch(map, pinchstatus, map.Map_ZoomLinear, ptScalePinchOrigin);
            }
            if (panstatus != GestureStatus.Completed) {
               panstatus = GestureStatus.Completed;
               TouchInterface.Pan(this, map, panstatus, e.Point.X - ptDragStart.X, e.Point.Y - ptDragStart.Y);
            }

            if (e.Fingers == 2) {
               pinchstatus = GestureStatus.Started;
            }
         };

         touchHandling.StdTap += async (sender, e) => await TouchInterface.Tapped(this, map, e.Point);

         //touchHandling.Move += (sender, e) => TouchInterface.TouchDrag(this, map, !map.Map_IsDragging ? e.From : e.To, e.Last);

         touchHandling.Move += (sender, e) => {
            if (!map.Map_IsDragging) {
               if (e.Last) // ignorieren
                  return;
               ptDragStart = e.From;
               panstatus = GestureStatus.Started;
            } else {
               if (e.Last)
                  panstatus = GestureStatus.Completed;
               else
                  panstatus = GestureStatus.Running;
            }

            Debug.WriteLine(">>> Pan: " + e.To + " " + panstatus);

            TouchInterface.Pan(this, map, panstatus, e.To.X - ptDragStart.X, e.To.Y - ptDragStart.Y);
         };

         touchHandling.Zoom += (sender, e) => {
            if (pinchstatus != GestureStatus.Completed) {
               if (e.Ended) {
                  pinchstatus = GestureStatus.Completed;
               } else {
                  if (pinchstatus == GestureStatus.Started) {
                     TouchInterface.StartZoomLinear = map.Map_ZoomLinear;
                     lastpinchzoom = 1;
                     ptScalePinchOrigin = new Point(e.Center.X / map.ControlWidth, e.Center.Y / map.ControlHeight);
                  } else
                     pinchstatus = GestureStatus.Running;
               }
               TouchInterface.Pinch(map, pinchstatus, e.Zoom / lastpinchzoom, ptScalePinchOrigin);
               lastpinchzoom = e.Zoom;

               if (pinchstatus == GestureStatus.Started)
                  pinchstatus = GestureStatus.Running;
            }
         };
      }

#else

      void initTouchHandling() { }

      private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
         //Debug.WriteLine("Tapped: GetPosition=" + e?.GetPosition(map).ToString());
         MapCtrl mymap = (MapCtrl)sender;
         Point? pt = e?.GetPosition(mymap);
         if (pt != null)
            await TouchInterface.Tapped(this, mymap, (Point)pt);
      }

      private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e) {
         //Debug.WriteLine("PanUpdated: GestureId=" + e.GestureId);
         //Debug.WriteLine("PanUpdated: StatusType=" + e.StatusType.ToString());
         //Debug.WriteLine("PanUpdated: TotalX=" + e.TotalX);
         //Debug.WriteLine("PanUpdated: TotalY=" + e.TotalY);
         TouchInterface.Pan(this, (MapCtrl)sender, e.StatusType, e.TotalX, e.TotalY);
      }

      private async void PinchGestureRecognizer_PinchUpdated(object sender, PinchGestureUpdatedEventArgs e) {
         //Debug.WriteLine("PinchUpdated: Scale=" + e.Scale);
         //Debug.WriteLine("PinchUpdated: ScaleOrigin=" + e.ScaleOrigin);
         //Debug.WriteLine("PinchUpdated: Status=" + e.Status);
         await TouchInterface.Pinch(this, (MapCtrl)sender, e.Status, e.Scale, e.ScaleOrigin);
      }

#endif

      #endregion

      /// <summary>
      /// ein langer oder kurzer Tap auf die Karte ist erfolgt
      /// </summary>
      /// <param name="point"></param>
      /// <param name="longtap"></param>
      /// <returns></returns>
      async Task mapTapped(Point point, bool longtap) {
         PointD platlon = Helper2.Maui2LatLon(point, map);
         labelPos.Text = string.Format("Tap {0:F6}° {1:F6}°", platlon.X, platlon.Y);

         // weiterleiten auf Marker- und Track-Events:
         //    Map_SpecMapMouseEvent(), Map_SpecMapMarkerEvent() und Map_SpecMapTrackEvent()
         // und von dort auf userTapAction()
         // Da in Map_SpecMapMouseEvent() aber noch nicht klar ist, ob danach noch Map_SpecMapMarkerEvent() und/oder Map_SpecMapTrackEvent() folgt
         // muss diese Auswertung von Map_SpecMapMouseEvent() entfallen und hier erfolgen!

         List<Marker>? markerlst = null;
         List<Track>? tracklst = null;
         if (TapType == TappingType.Standard ||
             TapType == TappingType.DeleteObjects)
            map.M_DoMouseClick((int)MapCtrl.MauiX2SkiaX(point.X),
                                    (int)MapCtrl.MauiY2SkiaY(point.Y),
                                    true,
                                    !longtap ?
                                       System.Windows.Forms.MouseButtons.Left :
                                       System.Windows.Forms.MouseButtons.Right,
                                    out markerlst,
                                    out tracklst);

         if ((markerlst == null || markerlst.Count == 0) &&
             (tracklst == null || tracklst.Count == 0))
            await userTapAction(TapType, null, null, point);   // Tap in den "freien Raum"
      }


   }
}