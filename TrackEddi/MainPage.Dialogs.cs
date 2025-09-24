using Bumptech.Glide;
using FSofTUtils.Geography.DEM;
using FSofTUtils.Geography.GeoCoding;
using FSofTUtils.Geometry;
using GMap.NET.FSofTExtented.MapProviders;
using SpecialMapCtrl;
using System.Text;

namespace TrackEddi {
   partial class MainPage {
      public class Dialogs {

         MainPage mainPage;


         public Dialogs(MainPage mainPage) {
            this.mainPage = mainPage;

         }

         /// <summary>
         /// einfache Anzeige der Track-Eigenschaften
         /// </summary>
         /// <param name="track"></param>
         /// <param name="point"></param>
         /// <returns></returns>
         async public Task ShowShortTrackProps(Track track, Point point) =>
            await showTrackPointInfo(track, point);

         /// <summary>
         /// zeigt Infos zum nächstliegenden zum Point liegenden <see cref="Track"/>-Punkt des <see cref="Track"/>
         /// (analog GpxViewer)
         /// </summary>
         /// <param name="track"></param>
         /// <param name="ptclient"></param>
         async Task showTrackPointInfo(Track track, Point ptclient) {
            int idx = track.GetNearestPtIdx(Helper2.Maui2LatLon(ptclient, mainPage.map));
            if (idx >= 0) {
               FSofTUtils.Geography.PoorGpx.GpxTrackPoint? pt = track.GetGpxPoint(idx);
               if (pt != null) {
                  StringBuilder sb = new StringBuilder();

                  sb.AppendFormat("nächstliegender Trackpunkt:");
                  sb.AppendLine();
                  sb.AppendFormat("Lng {0:F6}°, Lat {1:F6}°", pt.Lon, pt.Lat);
                  if (pt.Elevation != FSofTUtils.Geography.PoorGpx.BaseElement.NOTVALID_DOUBLE)
                     sb.AppendFormat(", Höhe {0:F0} m", pt.Elevation);
                  sb.AppendLine();
                  double length = track.Length(0, idx);
                  sb.AppendFormat("Streckenlänge bis zum Punkt: {0:F1} km ({1:F0} m)", length / 1000, length);
                  sb.AppendLine();
                  if (pt.Time != FSofTUtils.Geography.PoorGpx.BaseElement.NOTVALID_TIME) {
                     sb.AppendLine(pt.Time.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"));
                  }
                  sb.AppendLine();

                  sb.AppendFormat("Gesamtlänge: {0:F1} km ({1:F0} m)", track.StatLength / 1000, track.StatLength);
                  sb.AppendLine();
                  if (track.StatMinDateTimeIdx >= 0 &&
                     track.StatMaxDateTimeIdx > track.StatMinDateTimeIdx) {
                     TimeSpan ts = track.StatMaxDateTime.Subtract(track.StatMinDateTime);
                     sb.AppendFormat("Gesamt Datum/Zeit: {0} .. {1} (Dauer: {2} Stunden)",
                                 track.StatMinDateTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"),
                                 track.StatMaxDateTime.ToString(@"ddd, d. MMMM yyyy, H:mm:ss U\hr (UTC)"),
                                 ts.ToString(@"h\:mm\:ss"));
                     sb.AppendLine();
                     sb.AppendFormat("Durchschnittsgeschwindigkeit: {0:F1} km/h", track.StatLengthWithTime / ts.TotalSeconds * 3.6);
                     sb.AppendLine();
                  }

                  await Helper2.ShowInfoMessage(mainPage, sb.ToString(), track.VisualName);
               }
            }
         }

         /// <summary>
         /// ev. einen Marker an dieser Bildschirmpos. setzen
         /// </summary>
         /// <param name="clientpoint"></param>
         /// <returns></returns>
         async public Task SetNewMarker(System.Drawing.Point clientpoint) {
            if (await Helper2.ShowMessageBoxDefFalse(mainPage,
                                     "Neuer Marker",
                                     "Einen neuen Marker an dieser Position setzen?",
                                     "ja", "nein")) {
               mainPage.gpxWorkbench?.MarkerStartEdit(true, null);
               mainPage.gpxWorkbench?.MarkerEndEdit(clientpoint, false);
            }
         }

         /// <summary>
         /// ev. Marker löschen
         /// </summary>
         /// <param name="marker"></param>
         /// <returns></returns>
         async public Task RemoveMarker(Marker marker) {
            if (await Helper2.ShowMessageBoxDefFalse(mainPage,
                                     "Marker löschen",
                                     "'" + marker.Waypoint.Name + "'" + Environment.NewLine + Environment.NewLine +
                                     "löschen?",
                                     "ja", "nein"))
               mainPage.gpxWorkbench?.MarkerRemove(marker);
         }

         /// <summary>
         /// ev. Marker verschieben (starten)
         /// </summary>
         /// <param name="marker"></param>
         /// <returns></returns>
         async public Task StartMoveMarker(Marker marker) {
            if (await Helper2.ShowMessageBoxDefFalse(mainPage,
                                     "Marker verschieben",
                                     "Neue Position für" + Environment.NewLine + Environment.NewLine +
                                     "'" + marker.Waypoint.Name + "'" + Environment.NewLine + Environment.NewLine +
                                     "setzen?",
                                     "ja", "nein")) {
               mainPage.gpxWorkbench?.MarkerStartEdit(true, marker);
               mainPage.map.M_Refresh(true, false, false, false);     // ???
            }
         }

         /// <summary>
         /// ev. Track löschen
         /// </summary>
         /// <param name="track"></param>
         /// <returns></returns>
         async public Task RemoveTrack(Track track) {
            if (await Helper2.ShowMessageBoxDefFalse(mainPage,
                                     "Track löschen",
                                     "'" + track.VisualName + "'" + Environment.NewLine + Environment.NewLine +
                                     "löschen?",
                                     "ja", "nein"))
               mainPage.gpxWorkbench?.TrackRemove(track);
         }

         async public Task Info4LonLatAsync(Point xamarinpoint, bool onlyosm = false) {
            PointD geopt = mainPage.map.M_Client2LonLat(Helper2.Maui2ClientPoint(xamarinpoint));

            string txt = getDistanceText(mainPage.geoLocation, geopt.X, geopt.Y);
            if (txt != "")
               txt = "Entfernung " + txt;

            int height = DEM1x1.DEMNOVALUE;
            try {
               height = mainPage.dem != null ? mainPage.dem.GetHeight(geopt.X, geopt.Y) : DEM1x1.DEMNOVALUE;
            } catch (Exception ex) {
               // Ein bisher noch nicht näher bekannter Fehler scheint manchmal zu defekten Cachedateien zu führen.
               await Helper2.ShowExceptionMessage(mainPage, "Fehler beim Ermitteln der DEM-Daten für lon=" + geopt.X + "° lat=" + geopt.Y + "° (" + nameof(Info4LonLatAsync) + "())", ex);
            }
            if (height != DEM1x1.DEMNOVALUE)
               txt += (txt != "" ? ", Höhe " : "") + height.ToString() + "m";

            txt += txt != "" ?
                   System.Environment.NewLine + System.Environment.NewLine + "Soll nach weiteren " :
                   "Soll nach ";

            if (await Helper2.ShowMessageBoxDefFalse(mainPage,
                                     "Info suchen",
                                     txt + "Informationen zu diesem Punkt gesucht werden?",
                                     "ja", "nein"))
               await showInfo4LonLatAsync(geopt.X, geopt.Y, onlyosm);
         }

         /// <summary>
         /// suchen und anzeigen von Infos zum Punkt (Garmin- oder OSM-Suche)
         /// </summary>
         /// <param name="lon"></param>
         /// <param name="lat"></param>
         /// <param name="onlyosm"></param>
         /// <returns></returns>
         async Task showInfo4LonLatAsync(double lon, double lat, bool onlyosm = false) {
            int providx = mainPage.map.M_ActualMapIdx;

            string distanceText = getDistanceText(mainPage.geoLocation, lon, lat);

            int height = mainPage.dem != null ? mainPage.dem.GetHeight(lon, lat) : DEM1x1.DEMNOVALUE;
            string heightText = height != DEM1x1.DEMNOVALUE ? height.ToString() + "m" : "";

            string preText = "";
            if (distanceText != "")
               preText = preText + "Entfernung " + distanceText + System.Environment.NewLine;
            if (heightText != "")
               preText = preText + "Höhe " + heightText + System.Environment.NewLine;

            preText = preText + (lat >= 0 ? lat.ToString("f6") + "° N" : (-lat).ToString("f6") + "° S") + System.Environment.NewLine;
            preText = preText + (lon >= 0 ? lon.ToString("f6") + "° E" : (-lon).ToString("f6") + "° W") + System.Environment.NewLine;

            if (!onlyosm &&
               0 <= providx && providx < mainPage.map.M_ProviderDefinitions.Count) {
               if (mainPage.map.M_ProviderDefinitions[providx].Provider is GarminProvider) { // falls Garminkarte ...
                  int delta = mainPage.config != null ? (Math.Min(mainPage.map.Height, mainPage.map.Width) * mainPage.config.DeltaPercent4Search) / 100 : 0;
                  List<GarminImageCreator.SearchObject> info = await mainPage.map.M_GetGarminObjectInfosAsync(mainPage.map.M_LonLat2Client(lon, lat), delta, delta);

                  if (info.Count > 0) {
                     if (mainPage.subpages != null)
                        await mainPage.subpages.ShowGarminInfo4LonLat(info, preText);
                  } else
                     await Helper2.ShowInfoMessage(mainPage, preText + "Keine Infos für diesen Punkt vorhanden.", "Garmin-Info");
               } else {
                  GeoCodingReverseResultOsm[] geoCodingReverseResultOsms = await GeoCodingReverseResultOsm.GetAsync(lon, lat);
                  if (geoCodingReverseResultOsms.Length > 0) {
                     string[] names = new string[geoCodingReverseResultOsms.Length];
                     for (int i = 0; i < geoCodingReverseResultOsms.Length; i++)
                        names[i] = geoCodingReverseResultOsms[i].Name;
                     string txt = names.Length > 0 ?
                                       string.Join(Environment.NewLine, names) :
                                       "Keine Info für diesen Punkt vorhanden.";
                     await Helper2.ShowInfoMessage(mainPage, preText + txt, "OSM-Info");
                  }
               }
            }
         }

         /// <summary>
         /// liefert eine leere Zeichenkette wenn keine akt. Position bekannt ist, sonst eine in m bzw. km
         /// </summary>
         /// <param name="geoLocation"></param>
         /// <param name="lon"></param>
         /// <param name="lat"></param>
         /// <returns></returns>
         string getDistanceText(GeoLocation? geoLocation, double lon, double lat) {
            if (geoLocation != null &&
                geoLocation.GetLastPosition(out double mylon, out double mylat, out double myheight, out DateTime mydatetime)) {
               double distance = GeoLocation.Distance(mylon, mylat, lon, lat);
               if (distance < 10000)
                  return distance.ToString("f0") + "m";
               return (distance / 1000).ToString("f1") + "km";
            }
            return "";
         }

      }
   }
}
