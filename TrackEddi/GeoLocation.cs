//#define SAMPLEACCTESTDATA

using FSofTUtils.Geography.DEM;
using FSofTUtils.Geography.PoorGpx;
using SkiaSharp;
using SpecialMapCtrl;
using TrackEddi.Common;
#if SAMPLEACCTESTDATA
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
#endif

namespace TrackEddi {
   public class GeoLocation : IDisposable {

      /// <summary>
      /// ungültiger Wert
      /// </summary>
      const double NOTVALID_DOUBLE = FSofTUtils.Geography.PoorGpx.BaseElement.NOTVALID_DOUBLE;

      public string logfile = string.Empty;

      /// <summary>
      /// wird ausgelöst, wenn ein neuer Punkt an <see cref="LiveTrack"/> angehängt wurde
      /// </summary>
      public event EventHandler? AppendPoint;


      SpecialMapCtrl.SpecialMapCtrl mapControl;

      Location? lastLocation = null;

      string lastLocationProvider = string.Empty;

      object locationLocker = new object();

      long _locationSelfCentering = 0;

      public bool LocationSelfCentering {
         get => Interlocked.Read(ref _locationSelfCentering) != 0;
         set => Interlocked.Exchange(ref _locationSelfCentering, value ? 1 : 0);
      }

      FSofTUtils.Threading.ThreadSafeBoolVariable _locationServiceIsStarted = new FSofTUtils.Threading.ThreadSafeBoolVariable(false);

      /// <summary>
      /// der Service wurde gestartet, ist aber ev. noch nicht aktiv (siehe <see cref="LocationServiceIsActiv"/>)
      /// </summary>
      public bool LocationServiceIsStarted {
         get => _locationServiceIsStarted.Value;
         protected set => _locationServiceIsStarted.Value = value;
      }

      /// <summary>
      /// läuft der Service (kann noch eine kurze Zeit false nach <see cref="StartGeoLocationService"/> liefern!)
      /// </summary>
      public bool LocationServiceIsActiv => serviceCtrl.ServiceIsActive();

      FSofTUtils.Threading.ThreadSafeBoolVariable _locationIsShowing = new FSofTUtils.Threading.ThreadSafeBoolVariable(false);

      /// <summary>
      /// Wird die akt. Position angezeigt?
      /// </summary>
      public bool LocationIsShowing {
         get => LocationServiceIsActiv && _locationIsShowing.Value;
         set {
            if (_locationIsShowing.Value != value) {
               _locationIsShowing.Value = value;
               if (value)
                  lastLocation = null;
               mapControl.M_Refresh(false, false, false, false);
            }
         }
      }

      /// <summary>
      /// Wird die akt. Bewegung als Track aufgezeichnet?
      /// <para>Start mit <see cref="StartTracking"/></para>
      /// <para>Stop mit <see cref="EndTracking"/></para>
      /// </summary>
      public bool LocationTracking =>
         LocationServiceIsActiv && LiveTrack != null;

      public bool LocationTrackingPausing { get; protected set; } = false;

      long _screenActualisationIsOn = 1;

      /// <summary>
      /// Soll der Bildschirm akt. werden?
      /// </summary>
      public bool ScreenActualisationIsOn {
         get => Interlocked.Read(ref _screenActualisationIsOn) != 0;
         set => Interlocked.Exchange(ref _screenActualisationIsOn, value ? 1 : 0);
      }

      Track? _liveTrack;

      public Track? LiveTrack {
         get => Interlocked.Exchange(ref _liveTrack, _liveTrack);
         protected set => Interlocked.Exchange(ref _liveTrack, value);
      }

      double mindistance = 0, mindeltaheight = 0;

      /// <summary>
      /// die Richtung (gemessen in Grad) relativ zum magnetischen Norden (threadsicher)
      /// </summary>
      FSofTUtils.Threading.ThreadSafeDoubleVariable headingMagneticNorth = new FSofTUtils.Threading.ThreadSafeDoubleVariable(0);

      FSofTUtils.Threading.ThreadSafeIntVariable compassUserId = new FSofTUtils.Threading.ThreadSafeIntVariable(-1);
      FSofTUtils.Threading.ThreadSafeIntVariable compassUserId4Service = new FSofTUtils.Threading.ThreadSafeIntVariable(-1);

      FSofTUtils.Threading.ThreadSafeBoolVariable _compassIsShowing = new FSofTUtils.Threading.ThreadSafeBoolVariable(false);
      /// <summary>
      /// der Service wurde gestartet, ist aber ev. noch nicht aktiv (siehe <see cref="LocationServiceIsActiv"/>)
      /// </summary>
      public bool CompassIsShowing {
         get => _compassIsShowing.Value;
         set {
            if (_compassIsShowing.Value != value) {
               if (value) {
                  compassUserId.Value = CompassExt.Register(
                     (id, degree) => {
                        headingMagneticNorth.Value = degree;
                        mapControl.M_Refresh();
                     },
                     0);
               } else {
                  CompassExt.UnRegister(compassUserId.Value);
                  compassUserId.Value = -1;
               }
               _compassIsShowing.Value = value;
               mapControl.M_Refresh();
            }
         }
      }


      GeoLocationServiceCtrl serviceCtrl { get; set; }

      /// <summary>
      /// ev. zur Höhenkorrektur ??? (z.Z. NICHT verwendet)
      /// </summary>
      DemData? dem = null;


      public GeoLocation(SpecialMapCtrl.SpecialMapCtrl map, DemData? dem = null) {
         mapControl = map;
         serviceCtrl = new GeoLocationServiceCtrl();
         serviceCtrl.LocationChanged += locationChanged;
         this.dem = dem;
      }

      /// <summary>
      /// startet den Geo-Service
      /// </summary>
      /// <param name="updateintervall">Zeit in ms für Update</param>
      /// <param name="updatedistance">Positionsänderung in m für Update</param>
      /// <returns>true, wenn der Service arbeitet</returns>
      public bool StartGeoLocationService(int updateintervall = 1000, double updatedistance = 1) {
         if (!LocationServiceIsActiv) {
            compassUserId4Service.Value = CompassExt.Register((id, degree) => {
               headingMagneticNorth.Value = degree;
            },
                                                              0);
            // ACHTUNG: Der Start des Service wurde initiiert, aber er läuft ev. erst etwas verzögert an!
            return LocationServiceIsStarted = serviceCtrl.StartService(updateintervall, updatedistance);
         }
         return LocationServiceIsActiv;
      }

      /// <summary>
      /// stopt den Geo-Service
      /// </summary>
      /// <returns>true, wenn der Service nicht (mehr) arbeitet</returns>
      public bool StopGeoLocationService() {
         CompassExt.UnRegister(compassUserId4Service.Value);
         compassUserId4Service.Value = -1;
         if (LocationServiceIsActiv) {
            EndTracking();
            LocationServiceIsStarted = false;
            return serviceCtrl.StopService();
         }
         return !LocationServiceIsActiv;
      }

      /// <summary>
      /// falls <see cref="LocationIsShowing"/>==true wird die akt. Position gezeichnet
      /// </summary>
      /// <param name="g"></param>
      /// <param name="symbolradius">Symbolradius in Clientkoordinaten</param>
      public async Task ShowPosition(System.Drawing.Graphics g, float symbolradius = 50F) {
         if (LocationIsShowing) {
            Location? location = GetLastLocation(out _);
            if (location != null)
               if (drawLocationSymbol(g, symbolradius, location, (int)symbolradius) &&
                   LocationSelfCentering)
                  await mapControl.M_SetLocationAndZoomAsync(mapControl.M_Zoom, location.Longitude, location.Latitude);
         }
      }

      public void ShowCompass(System.Drawing.Graphics g, float symbolradius = 50F) {
         if (CompassIsShowing)
            drawCompass(g,
                        360 - headingMagneticNorth.Value,
                        new System.Drawing.Point((int)(1.3F * symbolradius), (int)(1.3F * symbolradius)),
                        symbolradius);
      }

      /// <summary>
      /// startet einen neuen Track der aufgezeichnet wird
      /// <para>Ein neuer Trackpunkt muss entweder den Mindestabstand zum letzten Trackpunkt ODER die Mindesthöhenänderung haben um
      /// registriert zu werden.</para>
      /// </summary>
      /// <param name="gpx"></param>
      /// <param name="mindistance">Mindestabstand zum letzten Punkt</param>
      /// <param name="mindeltaheight">Mindesthöhenänderung zum letzten Punkt</param>
      /// <param name="track"></param>
      public void StartTracking(GpxData gpx, double mindistance, double mindeltaheight, Track? track) {
         EndTracking();
         this.mindistance = mindistance;
         this.mindeltaheight = mindeltaheight;
         if (LocationServiceIsStarted) {
            LocationTrackingPausing = false;
            if (track != null)
               LiveTrack = track;
            else {
               LiveTrack = gpx.TrackInsertCopyWithLock(
                  new Track(Array.Empty<GpxTrackPoint>(),
                            "Livetrack " + DateTime.Now.ToString("G")) {
                               LineColor = VisualTrack.LiveDrawColor,
                               LineWidth = VisualTrack.LiveDrawWidth,
                            },
                  0,
                  true);
               LiveTrack.IsOnLiveDraw = true;
               LiveTrack.IsVisible = true;
            }
#if SAMPLEACCTESTDATA
            if (!string.IsNullOrEmpty(gpx.GpxFilename))
               startExtraData(gpx.GpxFilename, LiveTrack.Trackname);
#endif
         }
      }

      public void PausingTracking(bool on) {
         if (LocationTracking)
            LocationTrackingPausing = on;
      }


      /// <summary>
      /// Wenn <see cref="LocationServiceIsActiv"/>==true kann die letzte bekannte Position geliefert werden. Kann keine
      /// Position ermittelt werden, wird false geliefert
      /// </summary>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      /// <param name="height"></param>
      /// <param name="datetime"></param>
      /// <returns></returns>
      public bool GetLastPosition(out double lon, out double lat, out double height, out DateTime datetime) {
         if (LocationServiceIsActiv)
            return getLastPosition(out lon, out lat, out height, out datetime);
         else {
            lon = lat = height = 0;
            datetime = DateTime.Now;
         }
         return false;
      }

      public bool GetLastPosition(out double lon,
                                  out double lat,
                                  out double height,
                                  out DateTime datetime,
                                  out double accuracy,
                                  out double vaccuracy,
                                  out double speed,
                                  out double course) {
         if (LocationServiceIsActiv)
            return getLastPosition(out lon, out lat, out height, out datetime, out accuracy, out vaccuracy, out speed, out course);
         else {
            lon = lat = height = accuracy = vaccuracy = speed = course = 0;
            datetime = DateTime.Now;
         }
         return false;
      }

      /// <summary>
      /// beendet die Trackaufzeichnung
      /// </summary>
      public void EndTracking() {
         if (LocationTracking &&
             !LocationTrackingPausing &&
             LiveTrack != null) {
            GpxTrackSegment? segment = LiveTrack.GpxSegment;
            if (segment != null && segment.Points.Count > 1) {       // Trackaufzeichnung beenden und Track im Container speichern
               LiveTrack.IsOnLiveDraw = false;
               LiveTrack.RefreshBoundingbox();
               LiveTrack.CalculateStats();
            } else {       // zu wenig Punkte
               showTrack(LiveTrack, false);                    // Sichtbarkeit ausschalten
               if (LiveTrack.GpxDataContainer != null)
                  LiveTrack.GpxDataContainer.TrackRemoveWithLock(LiveTrack);
            }
#if SAMPLEACCTESTDATA
               endExtraData();
#endif
            LiveTrack = null;
         }
      }

      /// <summary>
      /// liefert threadsicher eine Kopie der letzten Position (oder null)
      /// </summary>
      /// <param name="provider"></param>
      /// <returns></returns>
      public Location? GetLastLocation(out string provider) {
         lock (locationLocker) {
            provider = lastLocationProvider;
            return lastLocation != null ? new Location(lastLocation) : null;
         }
      }

      /// <summary>
      /// berechnet die Entfernung in Meter
      /// </summary>
      /// <param name="fromlon"></param>
      /// <param name="fromlat"></param>
      /// <param name="tolon"></param>
      /// <param name="tolat"></param>
      /// <returns></returns>
      public static double Distance(double fromlon, double fromlat, double tolon, double tolat) =>
         new Location(fromlat, fromlon).CalculateDistance(tolat, tolon, DistanceUnits.Kilometers) * 1000;


      /// <summary>
      /// fügt einen neuen Punkt zum akt. Track hinzu, wenn sein Abstand zum letzten Punkt 
      /// ODER die Höhenänderung groß genug ist
      /// </summary>
      /// <param name="mindistance"></param>
      /// <param name="mindeltaheight"></param>
      void appendTrackPoint(double mindistance, double mindeltaheight) {
         if (LiveTrack != null &&
             !LocationTrackingPausing &&
#if SAMPLEACCTESTDATA
             GetLastPosition(out double lon,
                             out double lat,
                             out double height,
                             out DateTime datetime,
                             out double accuracy,
                             out double vaccuracy,
                             out double speed,
                             out double course)) {
#else
             GetLastPosition(out double lon, out double lat, out double height, out DateTime datetime)) {
#endif

            //if (dem != null) {
            //   double h = dem.GetHeight(lon, lat);
            //   if (h != DEM1x1.DEMNOVALUE) {





            //   }
            //}

            double distance = 0;
            double deltaheigth = 0;
            GpxTrackSegment? segment = LiveTrack.GpxSegment;
            if (segment != null) {
               ListTS<GpxTrackPoint> Points = segment.Points;

               GpxTrackPoint? lastTP = null;
               if (0 < Points.Count) {
                  // letzte registrierte Position:
                  lastTP = LiveTrack.GetGpxPoint(Points.Count - 1);
                  if (lastTP != null) {
                     distance = Distance(lon, lat, lastTP.Lon, lastTP.Lat);
                     deltaheigth = lastTP.Elevation != NOTVALID_DOUBLE &&
                                   height != NOTVALID_DOUBLE ?
                                          height - lastTP.Elevation :
                                          0;
                  }
               }

               if (Points.Count == 0 ||   // 1. Punkt oder
                   distance >= mindistance ||                  // Abstand groß genug oder
                   deltaheigth >= mindeltaheight) {            // Höhendiff. groß genug

                  GpxTrackPoint newpt = new GpxTrackPoint(lon, lat, height, datetime);

                  const int TESTPOINTS = 5;
                  bool removespike = false;

                  try {
                     Points.EnterWriteLock();
                     if (lastTP != null &&
                         Points.Count >= TESTPOINTS) {  // der Spikealgorithmus benötigt min. 5 Punkte
                        List<GpxTrackPoint> lastpts = new List<GpxTrackPoint>();
                        for (int i = 0; i < TESTPOINTS - 1; i++)
#pragma warning disable CS8604 // Mögliches Nullverweisargument.
                           lastpts.Add(LiveTrack.GetGpxPoint(Points.Count - (TESTPOINTS - 1) + i));
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
                        lastpts.Add(newpt);

                        if (FSofTUtils.Geography.GpxSimplification.RemoveSpikes(lastpts).Length > 0)
                           removespike = true;
                     }

                     showTrack(LiveTrack, false);
                     GpxData? gpx = LiveTrack.GpxDataContainer;
                     int trackno = LiveTrack.GpxDataContainerIndex;
                     if (removespike)
                        Points.RemoveAt(Points.Count - 2); // betrifft den z.Z. vorletzten Punkt
                     Points.Add(newpt);
                  } catch (Exception ex) {
                     string msg = UIHelper.GetExceptionMessage(ex);
                     UIHelper.Message2Logfile(nameof(GeoLocation) + "." + nameof(appendTrackPoint), msg, null);
                  } finally {
                     Points.ExitWriteLock();
                  }

                  LiveTrack.CalculateStats();
                  if (LiveTrack.GpxDataContainer != null)
                     LiveTrack.GpxDataContainer.GpxDataChanged = true;
                  LiveTrack.UpdateVisualTrack(mapControl);
                  showTrack(LiveTrack, true);
                  AppendPoint?.Invoke(this, EventArgs.Empty);  // Event auslösen
#if SAMPLEACCTESTDATA
                  writeGpsData(lon, lat, height, datetime, accuracy, vaccuracy, speed, course);
#endif

               }
            }
         }
      }

      bool getLastPosition(out double lon, out double lat, out double heigth, out DateTime datetime) {
         lon = lat = heigth = double.MinValue;
         datetime = DateTime.MinValue;
         Location? location = GetLastLocation(out _);
         if (location != null) {
            lon = location.Longitude;
            lat = location.Latitude;
            if (location.Altitude != null)
               heigth = (double)location.Altitude;
            else
               heigth = NOTVALID_DOUBLE;
            datetime = new DateTime(location.Timestamp.Ticks);
            return true;
         }
         return false;
      }

      bool getLastPosition(out double lon,
                           out double lat,
                           out double heigth,
                           out DateTime datetime,
                           out double accuracy,
                           out double vaccuracy,
                           out double speed,
                           out double course) {
         lon = lat = heigth = NOTVALID_DOUBLE;
         datetime = DateTime.MinValue;
         accuracy = vaccuracy = speed = course = NOTVALID_DOUBLE;
         Location? location = GetLastLocation(out _);
         if (location != null) {
            lon = location.Longitude;
            lat = location.Latitude;
            heigth = location.Altitude != null ? (double)location.Altitude : NOTVALID_DOUBLE;
            accuracy = location.Accuracy != null ? (double)location.Accuracy : NOTVALID_DOUBLE;
            vaccuracy = location.VerticalAccuracy != null ? (double)location.VerticalAccuracy : NOTVALID_DOUBLE;
            speed = location.Speed != null ? (double)location.Speed : NOTVALID_DOUBLE;
            course = location.Course != null ? (double)location.Course : NOTVALID_DOUBLE;
            datetime = new DateTime(location.Timestamp.Ticks);
            return true;
         }
         return false;
      }

      /// <summary>
      /// die Geo-Position wurde verändert
      /// <para>Bei <see cref="LocationIsShowing"/> oder <see cref="LocationTracking"/> wird im MainThread (!) ein
      /// <see cref="SpecialMapCtrl.SpecialMapCtrl.M_Refresh(bool, bool, bool)"/> ausgelöst. 
      /// </para>
      /// <para>Bei <see cref="LocationIsShowing"/> wird zusätzlich <see cref="appendTrackPoint(double, double)"/> 
      /// im MainThread (!) ausgeführt.</para>
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void locationChanged(object? sender, GeoLocationServiceCtrl.LocationChangedArgs e) {
         lock (locationLocker) {
            if (e.Location != null) {
               if (e.Provider == "network" &&
                   e.Location.Accuracy > 15)
                  return;

               lastLocation = new Location(e.Location);
               lastLocationProvider = e.Provider;

               if (e.Location.Altitude != null && !serviceCtrl.IsValid((double)e.Location.Altitude))
                  lastLocation.Altitude = null;
               if (e.Location.Course != null && !serviceCtrl.IsValid((double)e.Location.Course))
                  lastLocation.Course = null;
               if (e.Location.Speed != null && !serviceCtrl.IsValid((double)e.Location.Speed))
                  lastLocation.Speed = null;
               if (e.Location.Accuracy != null && !serviceCtrl.IsValid((double)e.Location.Accuracy))
                  lastLocation.Accuracy = null;
               if (e.Location.VerticalAccuracy != null && !serviceCtrl.IsValid((double)e.Location.VerticalAccuracy))
                  lastLocation.VerticalAccuracy = null;

               MainThread.BeginInvokeOnMainThread(() => {
                  if ((LocationIsShowing || LocationTracking) && ScreenActualisationIsOn)
                     mapControl.M_Refresh(false, false, false, false);

                  if (LocationTracking)
                     appendTrackPoint(mindistance, mindeltaheight);
               });
            }
         }


         //System.IO.File.AppendAllText(logfile,
         //                             lastLocation.Timestamp.ToString("G") + " " + lastLocationProvider + ": "
         //                             + lastLocation.Longitude.ToString("f6") + "° "
         //                             + lastLocation.Latitude.ToString("f6") + "° "
         //                             + (lastLocation.Altitude != null ? lastLocation.Altitude.Value.ToString("f1") + "m " : " ")
         //                             + (lastLocation.Course != null ? lastLocation.Course.Value.ToString("f1") + "° " : " ")
         //                             + (lastLocation.Speed != null ? lastLocation.Speed.Value.ToString("f1") + "m/s " : " ")
         //                             + (lastLocation.Accuracy != null ? lastLocation.Accuracy.Value.ToString("f1") + "m " : " ")
         //                             + (lastLocation.VerticalAccuracy != null ? lastLocation.VerticalAccuracy.Value.ToString("f1") + "m " : " ")
         //                             + System.Environment.NewLine);


         //Debug.WriteLine("POS: " + lastLocation.Longitude.ToString("f6") + "° " + lastLocation.Latitude.ToString("f6") + "° ");

      }

      /// <summary>
      /// zeichnet ein Symbol für die akt. Position
      /// </summary>
      /// <param name="g"></param>
      /// <param name="symbolradius">Größe des Symbols in Clientkoordinaten</param>
      /// <param name="geplocation">Geo-Position</param>
      /// <param name="selfcenteringdistance">Distanz in Clientkoordinaten ab der eine neue Zentrierung der Karte erfolgen sollte</param>
      /// <returns>liefert true, wenn eine Zentrierung erfolgen sollte</returns>
      bool drawLocationSymbol(System.Drawing.Graphics g,
                              float symbolradius = 50,
                              Location? geplocation = null,
                              int selfcenteringdistance = 50) {
         if (geplocation != null) {
            System.Drawing.Point ptClient = mapControl.M_LonLat2Client(geplocation.Longitude, geplocation.Latitude);

            // von Xamarin.Essentials:
            //    Requires a high accuracy query of location and may not be returned by Geolocation.GetLastKnownLocationAsync
            double? course = geplocation.Course;
            if (course == null)
               course = headingMagneticNorth.Value;

            float radians = course != null ?
                                 (float)((course - 90) / 180 * Math.PI) :
                                 float.MinValue;

            using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(180, System.Drawing.Color.Red))) {
               using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 3)) {

                  float radius = symbolradius;
                  g.DrawEllipse(pen,
                                ptClient.X - radius,
                                ptClient.Y - radius,
                                2 * radius,
                                2 * radius);

#if GRAPHICS2
                  SKMatrix orgMatrix = g.TotalMatrix;
                  if (radians != float.MinValue) {
                     SKMatrix rotMatrix = SKMatrix.CreateRotation(radians, ptClient.X, ptClient.Y);
                     g.SetMatrix(g.TotalMatrix.PreConcat(rotMatrix));    // jetzt neues KS: Linie kommt "von links" und endet in (xend, yend)
                  }
#else
                  SKMatrix orgMatrix = g.SKCanvas.TotalMatrix;
                  if (radians != float.MinValue) {
                     SKMatrix rotMatrix = SKMatrix.CreateRotation(radians, ptClient.X, ptClient.Y);
                     g.SKCanvas.SetMatrix(g.SKCanvas.TotalMatrix.PreConcat(rotMatrix));    // jetzt neues KS: Linie kommt "von links" und endet in (xend, yend)
                  }
#endif

                  if (radians != float.MinValue) {
                     g.DrawLine(pen,
                                ptClient.X - symbolradius,
                                ptClient.Y,
                                ptClient.X,
                                ptClient.Y);
                     drawArrow(g,
                               brush,
                               ptClient.X,
                               ptClient.Y,
                               ptClient.X - 0.7F * symbolradius,
                               symbolradius / 3);
                  } else {
                     radius = symbolradius / 10;
                     g.FillEllipse(brush,
                                   ptClient.X - radius,
                                   ptClient.Y - radius,
                                   2 * radius,
                                   2 * radius);
                  }

#if GRAPHICS2
                  g.SetMatrix(orgMatrix);
#else
                  g.SKCanvas.SetMatrix(orgMatrix);
#endif
               }
            }
            if (ptClient.Y < mapControl.Height / 2 - selfcenteringdistance || mapControl.Height / 2 + selfcenteringdistance < ptClient.Y ||
                ptClient.X < mapControl.Width / 2 - selfcenteringdistance || mapControl.Width / 2 + selfcenteringdistance < ptClient.X)
               return true;
         }
         return false;
      }

      /// <summary>
      /// einfache Pfeilspitze von links nach rechts
      /// </summary>
      /// <param name="g"></param>
      /// <param name="brush"></param>
      /// <param name="peakx"></param>
      /// <param name="peaky"></param>
      /// <param name="basex"></param>
      /// <param name="basey"></param>
      void drawArrow(System.Drawing.Graphics g, System.Drawing.SolidBrush brush, float peakx, float peaky, float basex, float basey) =>
         g.FillPolygon(brush, new System.Drawing.PointF[] { new System.Drawing.PointF(peakx, peaky),
                                                            new System.Drawing.PointF(basex, peaky + basey),
                                                            new System.Drawing.PointF(basex + (peakx - basex) / 3, peaky),
                                                            new System.Drawing.PointF(basex, peaky - basey),
                                                            new System.Drawing.PointF(peakx, peaky)});

      void drawCompass(System.Drawing.Graphics g, double degree, System.Drawing.Point ptClientCenter, float symbolradius) {
         using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(150, System.Drawing.Color.Red))) {
            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 3)) {
               float radius = symbolradius;
               // Kreis
               g.DrawEllipse(pen,
                             ptClientCenter.X - radius,
                             ptClientCenter.Y - radius,
                             2 * radius,
                             2 * radius);

               float radians = (float)((degree - 180) / 180 * Math.PI);
               if (radians != float.MinValue) {
                  SKMatrix orgMatrix = g.SKCanvas.TotalMatrix;

                  // KS mit Nullpunkt ptClientCenter und nach rechts und oben gerichtet
                  g.SKCanvas.SetMatrix(g.SKCanvas.TotalMatrix.PreConcat(SKMatrix.CreateRotation(radians, ptClientCenter.X, ptClientCenter.Y)));

                  g.FillPolygon(brush, [ new System.Drawing.PointF(ptClientCenter.X, ptClientCenter.Y + symbolradius),
                                         new System.Drawing.PointF(ptClientCenter.X + symbolradius * .7F, ptClientCenter.Y - symbolradius * .7F),
                                         new System.Drawing.PointF(ptClientCenter.X, ptClientCenter.Y - symbolradius * .3F),
                                         new System.Drawing.PointF(ptClientCenter.X - symbolradius * .7F, ptClientCenter.Y - symbolradius * .7F),
                                         new System.Drawing.PointF(ptClientCenter.X, ptClientCenter.Y + symbolradius)]);

                  using (System.Drawing.Font font = new System.Drawing.Font(
                              mapControl.Font != null ? mapControl.Font.FontFamilyname : string.Empty,
                              35,
                              System.Drawing.FontStyle.Regular,
                              System.Drawing.GraphicsUnit.Pixel)) {
                     using (System.Drawing.Brush txtbrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(180, 255, 255, 255)))
                        g.DrawString("N",
                                     font,
                                     txtbrush,
                                     ptClientCenter.X,
                                     ptClientCenter.Y,
                                     new System.Drawing.StringFormat() {
                                        Alignment = System.Drawing.StringAlignment.Center,
                                        LineAlignment = System.Drawing.StringAlignment.Center
                                     });
                  }

                  g.SKCanvas.SetMatrix(orgMatrix);
               }
            }
         }

      }

      void showTrack(Track track, bool visible) => mapControl.M_ShowTrack(track,
                                                                               visible,
                                                                               visible && track.GpxDataContainer != null ?
                                                                                    track.GpxDataContainer.NextVisibleTrack(track) :
                                                                                    null);

      #region ================ experimentell

#if SAMPLEACCTESTDATA

      string extraDataFilename = string.Empty;
      long startticks;
      bool accelerometerReadingChanged = false;
      bool orientationSensorReadingChanged = false;

      void startExtraData(string gpxallfile, string trackname) {
         Regex r = new Regex("[" + "\"*/:<>?\\|\x7F\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0A\x0B\x0C\x0D\x0E\x0F\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\x1B\x1C\x1D\x1E\x1F" + "]");    // nur sichere Zeichen zulassen
         trackname = r.Replace(trackname, "_");
         extraDataFilename = Path.Combine(Path.GetDirectoryName(gpxallfile) ?? string.Empty, trackname) + ".dat";
         startticks = 0;

         if (!Accelerometer.IsMonitoring && !accelerometerReadingChanged) {
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            accelerometerReadingChanged = true;
            Accelerometer.Start(SensorSpeed.UI);
         }

         if (!OrientationSensor.IsMonitoring && !orientationSensorReadingChanged) {
            OrientationSensor.ReadingChanged += Orientation_ReadingChanged;
            orientationSensorReadingChanged = true;
            OrientationSensor.Start(SensorSpeed.UI);
         }
      }


      void endExtraData() {
         if (Accelerometer.IsMonitoring || accelerometerReadingChanged) {
            Accelerometer.Stop();
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            accelerometerReadingChanged = false;
         }
         if (OrientationSensor.IsMonitoring || orientationSensorReadingChanged) {
            OrientationSensor.Stop();
            orientationSensorReadingChanged = false;
            OrientationSensor.ReadingChanged -= Orientation_ReadingChanged;
         }
      }

      private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e) {
         writeExtraDataLine(extraDataFilename, e.Reading.Acceleration, DateTime.UtcNow);
      }

      private void Orientation_ReadingChanged(object? sender, OrientationSensorChangedEventArgs e) {
         writeExtraDataLine(extraDataFilename, e.Reading.Orientation, DateTime.UtcNow);
      }

      void writeExtraDataLine(string filename, Vector3 rawAcc, DateTime dt) {
         StringBuilder sb = new StringBuilder();
         sb.AppendFormat("a\t{0:F3}\t{1:F3}\t{2:F3}",
                         rawAcc.X,
                         rawAcc.Y,
                         rawAcc.Z);
         appendDateTime(sb, dt);
         writeExtraData2File(filename, sb);
      }

      void writeExtraDataLine(string filename, Quaternion rawOrient, DateTime dt) {
         StringBuilder sb = new StringBuilder();
         sb.AppendFormat("o\t{0:F3}\t{1:F3}\t{2:F3}\t{3:F3}",
                         rawOrient.X,
                         rawOrient.Y,
                         rawOrient.Z,
                         rawOrient.W);
         appendDateTime(sb, dt);
         writeExtraData2File(filename, sb);
      }

      void writeGpsData(double lon,
                        double lat,
                        double height,
                        DateTime dt,
                        double accuracy,
                        double vaccuracy,
                        double speed,
                        double course) {
         StringBuilder sb = new StringBuilder();
         // 12,35146358	51,31920609	153,0	5,360	8,000	5,5	77,3	-7166,758
         // 12,35419274	51,31978506	154,7	10,720	8,000	10,3	71,1	-7127,767
         // 12,35681791	51,32052435	156,0	10,720	6,000	0,0		-7088,781
         // 12,35993854	51,32098850	145,9	10,720	6,000	9,9	75,8	-7052,741

         // 12,34322889	51,30726356	163,1	6,432	16,000	0,0		-7199,017            0,961
         // ...
         // 12,34356471	51,30705383	145,4	8,576	12,000	1,1	321,3	-5128,240         2071,752

         sb.AppendFormat("g\t{0:F8}\t{1:F8}", lon, lat);
         sb.Append("\t");
         if (height != NOTVALID_DOUBLE)
            sb.AppendFormat("{0:F1}", height);
         sb.Append("\t");
         if (accuracy != NOTVALID_DOUBLE)
            sb.AppendFormat("{0:F3}", accuracy);
         sb.Append("\t");
         if (vaccuracy != NOTVALID_DOUBLE)
            sb.AppendFormat("{0:F3}", vaccuracy);
         sb.Append("\t");
         if (speed != NOTVALID_DOUBLE)
            sb.AppendFormat("{0:F1}", speed);
         sb.Append("\t");
         if (course != NOTVALID_DOUBLE)
            sb.AppendFormat("{0:F1}", course);
         appendDateTime(sb, dt);

         writeExtraData2File(extraDataFilename, sb);
      }

      void appendDateTime(StringBuilder sb, DateTime dt) {
         sb.Append("\t");
         TimeSpan ts = new TimeSpan(dt.Ticks - startticks);
         sb.AppendLine(ts.TotalSeconds.ToString("f3"));
      }

      object objlocker = new object();

      void writeExtraData2File(string filename, StringBuilder sb) {
         lock (objlocker) {
            File.AppendAllText(filename, sb.ToString());
         }
      }
#endif


      #endregion

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben

            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)
            LocationIsShowing = false;
            StopGeoLocationService();
            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion
   }
}
