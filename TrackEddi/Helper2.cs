using FSofTUtils.Geometry;
using FSofTUtils.OSInterface;
using FSofTUtils.OSInterface.Storage;
using SpecialMapCtrl;
using TrackEddi.Common;

namespace TrackEddi {
   class Helper2 {

      #region Punktumrechnungen Maui - Client - LatLon

      public static Point Client2MauiPoint(System.Drawing.Point client) =>
         new Point(SpecialMapCtrl.SpecialMapCtrl.SkiaX2MauiX(client.X),
                   SpecialMapCtrl.SpecialMapCtrl.SkiaY2MauiY(client.Y));

      public static Point Client2MauiPoint(int clientx, int clienty) =>
         new Point(SpecialMapCtrl.SpecialMapCtrl.SkiaX2MauiX(clientx),
                   SpecialMapCtrl.SpecialMapCtrl.SkiaY2MauiY(clienty));

      public static System.Drawing.Point Maui2ClientPoint(Point maui) =>
         new System.Drawing.Point((int)SpecialMapCtrl.SpecialMapCtrl.MauiX2SkiaX(maui.X),
                                  (int)SpecialMapCtrl.SpecialMapCtrl.MauiY2SkiaY(maui.Y));

      public static PointD Maui2LatLon(Point point, SpecialMapCtrl.SpecialMapCtrl map) =>
         map.M_Client2LonLat((int)SpecialMapCtrl.SpecialMapCtrl.MauiX2SkiaX(point.X),
                             (int)SpecialMapCtrl.SpecialMapCtrl.MauiY2SkiaY(point.Y));

      #endregion

      #region Ausgabe Text in Logdatei

      static object loglocker = new object();

      static string logtext = string.Empty;

      /// <summary>
      /// hängt einen Text an die Logdatei an
      /// </summary>
      /// <param name="txt"></param>
      /// <param name="firstVolumePath"></param>
      /// <param name="datapath"></param>
      public static void Log(string txt, string firstVolumePath, string datapath) {
         try {
            lock (loglocker) {
               string logfile = string.IsNullOrEmpty(firstVolumePath) ?
                                             "" :
                                             Path.Combine(firstVolumePath, datapath, "log.txt");
               txt = DateTime.Now.ToString("G") + " " + txt + Environment.NewLine;
               if (logfile.Length == 0)
                  logtext += txt;
               else {
                  if (logtext.Length > 0) {
                     File.AppendAllText(logfile, logtext);
                     logtext = "";
                  }
                  File.AppendAllText(logfile, txt);
               }
            }
         } catch { }
      }

      #endregion


      /// <summary>
      /// um Mehrfachstart zu vermeiden
      /// </summary>
      static bool isInShowInfoMessage = false;

      /// <summary>
      /// zeigt einen Info-Text an
      /// </summary>
      /// <param name="parentpage"></param>
      /// <param name="txt"></param>
      /// <param name="caption"></param>
      public static async Task ShowInfoMessage(Page parentpage, string txt, string caption = "Info") {
         if (isInShowInfoMessage)
            return;
         isInShowInfoMessage = true;
         await UIHelper.ShowInfoMessage(parentpage, txt, caption);
         isInShowInfoMessage = false;
      }

      /// <summary>
      /// Exception im Haupthread anzeigen
      /// </summary>
      /// <param name="parentpage"></param>
      /// <param name="caption"></param>
      /// <param name="ex"></param>
      /// <param name="exit"></param>
      /// <returns></returns>
      public static async Task ShowExceptionMessage(Page parentpage, string caption, Exception ex, bool exit = false) {
         if (MainThread.IsMainThread) {
            await UIHelper.ShowExceptionMessage(parentpage, caption, ex, null, exit);
         } else {
            await MainThread.InvokeOnMainThreadAsync(async () => 
               await UIHelper.ShowExceptionMessage(parentpage, caption, ex, null, exit));
         }
      }

      /// <summary>
      /// um Mehrfachstart zu vermeiden
      /// </summary>
      static bool isInMessageBoxDefFalse = false;

      /// <summary>
      /// wartet eine Anwort ab (nur bei 'accept' wird true geliefert)
      /// </summary>
      /// <param name="parentpage"></param>
      /// <param name="title"></param>
      /// <param name="msg"></param>
      /// <param name="accept"></param>
      /// <param name="cancel"></param>
      /// <returns></returns>
      public static async Task<bool> ShowMessageBoxDefFalse(Page parentpage, string title, string msg, string accept, string cancel) {
         if (isInMessageBoxDefFalse)
            return false;
         isInMessageBoxDefFalse = true;
         var answer = await Helper.MessageBox(parentpage, title, msg, accept, cancel);
         isInMessageBoxDefFalse = false;
         return answer;
      }



      /// <summary>
      /// <see cref="Track"/> anzeigen oder verbergen
      /// </summary>
      /// <param name="map"></param>
      /// <param name="track"></param>
      /// <param name="visible"></param>
      public static void ShowTrack(SpecialMapCtrl.SpecialMapCtrl map, Track? track, bool visible = true) {
         if (track != null && track.IsVisible != visible)
            map.M_ShowTrack(track,
                                 visible,
                                 visible ? track.GpxDataContainer?.NextVisibleTrack(track) : null);
      }

      /// <summary>
      /// <see cref="Marker"/> anzeigen oder verbergen
      /// </summary>
      /// <param name="map"></param>
      /// <param name="marker"></param>
      /// <param name="visible"></param>
      /// <returns></returns>
      public static int ShowMarker(SpecialMapCtrl.SpecialMapCtrl map, Marker? marker, bool visible = true) {
         if (marker == null || marker.IsVisible == visible)
            return -1;
         map.M_ShowMarker(marker,
                               visible,
                               visible ? marker.GpxDataContainer?.NextVisibleMarker(marker) : null);

         //editableTracklistControl1.ShowMarker(marker, visible);

         return marker.GpxDataContainer != null ? marker.GpxDataContainer.MarkerIndex(marker) : -1;
      }


      ///// <summary>
      ///// erst NACH <see cref="initDepTools(object)"/>
      ///// </summary>
      //async void test_uriLink2Filename() {
      //   Debug.WriteLine(await uriLink2Filename(new Uri("file:///storage/emulated/0/Download/!notes.abc")));
      //   Debug.WriteLine(await uriLink2Filename(new Uri("file:///storage/190E-1F12/Download/!notes.abc")));
      //   Debug.WriteLine(await uriLink2Filename(new Uri("content://com.android.externalstorage.documents/document/primary%3ADownload%2F!notes.abc")));
      //   Debug.WriteLine(await uriLink2Filename(new Uri("content://com.android.externalstorage.documents/document/0FFE-051F%3ADocuments%2Fabc.gpx")));
      //   Debug.WriteLine(await uriLink2Filename(new Uri("content://com.ghisler.files/tree/primary%3A/document/primary%3Astorage%2Femulated%2F0%2FDownload%2Ftest.gpx")));
      //   Debug.WriteLine(await uriLink2Filename(new Uri("content://com.ghisler.files/tree/primary%3A/document/primary%3Astorage%2F2F057C-181B%2FDownload%2Ftest.gpx")));
      //   Debug.WriteLine(await uriLink2Filename(new Uri("content://com.google.android.apps.nbu.files.provider/1/file%3A%2F%2F%2Fstorage%2F057C-181B%2FDaten%2Ftest.gpx")));
      //}

      /// <summary>
      /// versucht, aus der URI den vollständigen Dateinamen zu extrahieren
      /// </summary>
      /// <param name="parentpage"></param>
      /// <param name="storageHelper"></param>
      /// <param name="uri"></param>
      /// <returns></returns>
      public static async Task<string> uriLink2Filename(Page parentpage, StorageHelper storageHelper, Uri uri) {
         //StringBuilder sb = new StringBuilder();
         //if (uri != null) {
         //   if (!string.IsNullOrEmpty(uri.OriginalString)) {
         //      sb.Append("OriginalString: " + uri.OriginalString);
         //      sb.AppendLine();
         //      sb.AppendLine();
         //   }
         //   if (!string.IsNullOrEmpty(uri.AbsolutePath)) {
         //      sb.Append("AbsolutePath: " + uri.AbsolutePath);
         //      sb.AppendLine();
         //      sb.AppendLine();
         //   }
         //   if (!string.IsNullOrEmpty(uri.Scheme)) {
         //      sb.Append("Scheme: " + uri.Scheme);
         //      sb.AppendLine();
         //      sb.AppendLine();
         //   }
         //}
         //await UIHelper.ShowInfoMessage(this, sb.ToString(), "URI");

         string filename = "";
         if (uri != null &&
             !string.IsNullOrEmpty(uri.AbsolutePath))
            try {

               string absdecodesfilename = System.Web.HttpUtility.UrlDecode(uri.AbsolutePath);

               if (uri.Scheme == "file") {
                  // z.B.
                  //		uri.OriginalString = "file:///storage/emulated/0/Download/!notes.abc"
                  //		uri.AbsolutePath = "/storage/emulated/0/Download/!notes.abc"
                  // oder
                  //		uri.AbsolutePath = "/storage/190E-1F12/Documents/!notes.abc"

                  filename = absdecodesfilename;

               } else if (uri.Scheme == "content") {

                  // Folgende Varianten wurden beobachtet:
                  // - Ist "/file:///" enthalten, kommt danach der vollständige Pfad mit Volume.
                  // - Ist ein einzelnes ":" enthalten folgt nach dem ":" der abs. Pfad ohne führendes "/" und ohne Volume.
                  //   Das Volume steht direkt vor dem ":".
                  // - Sind 2 ":" enthalten, folgt nach dem letzten ":" der vollständige Pfad mit Volume ohne führendes "/".

                  // "Explorer" (Files- / Dateien-App)
                  //		uri.OriginalString = "content://com.android.externalstorage.documents/document/primary%3ADownload%2F!notes.abc"
                  //		uri.AbsolutePath = "/document/primary%3ADownload%2F!notes.abc"
                  //		uri.AbsolutePath = "/document/0FFE-051F:Documents/abc.gpx"
                  //
                  //	z.B. Total Comander (wenn nicht als file-URI)
                  //		uri.OriginalString = content://com.ghisler.files/tree/primary%3A/document/primary%3Astorage%2Femulated%2F0%2FDownload%2F!notes.abc"
                  //		uri.AbsolutePath = "/tree/primary%3A/document/primary%3Astorage%2Femulated%2F0%2FDownload%2F!notes.abc"
                  //		uri.OriginalString = content://com.ghisler.files/tree/primary%3A/document/primary%3Astorage%2FF2F057C-181B%2FDownload%2F!notes.abc"
                  //		uri.AbsolutePath = "/tree/primary%3A/document/primary%3Astorage%2FF2F057C-181B%2FDownload%2F!notes.abc"
                  //
                  // uri.OriginalString = content://com.google.android.apps.nbu.files.provider/1/file%3A%2F%2F%2Fstorage%2F057C-181B%2FDaten ...
                  // 	uri.AbsolutePath = "/1/file:///storage/057C-181B/Daten/GPX-Touren/201004%20Wander ....
                  // 	uri.AbsolutePath = "/1/file%3A%2F%2F%2Fstorage%2F057C-181B%2FDaten%2FGPX-Touren%2F202403%2520Vietnam%252C%2520Kambodscha%2F04.gpx

                  int p = absdecodesfilename.LastIndexOf(":");
                  if (p >= 0) {
                     int pf;
                     if ((pf = absdecodesfilename.IndexOf("/file:///")) >= 0) {  // z.B.: "/1/file:///storage/057C-181B/Daten/GPX-Touren/201004%20Wander ...."
                        filename = absdecodesfilename.Substring(pf + 8);         //                 "/storage/057C-181B/Daten/GPX-Touren/201004%20Wander ...."
                     } else {                                                    // z.B.: "/document/primary:Download/!notes.abc"
                        filename = "/" + absdecodesfilename.Substring(p + 1);    //                        "/Download/!notes.abc"

                        if (absdecodesfilename.IndexOf(":") < p) {               // min. 2x ":"
                                                                                 // alles ok
                        } else {
                           // Volume-Name ermitteln
                           string volume = absdecodesfilename.Substring(0, p);   //       "/document/primary"
                           p = volume.LastIndexOf("/");
                           if (p >= 0)
                              volume = volume.Substring(p + 1);                  //                 "primary"

                           // Volume-Path zum Volume-Name suchen
                           string volumepath = "";
                           for (int i = 0; i < storageHelper.Volumes; i++) {
                              if (volume == storageHelper.VolumeNames[i]) {
                                 volumepath = storageHelper.VolumePaths[i];
                                 break;
                              }
                           }

                           if (!filename.StartsWith(volumepath))
                              filename = volumepath + filename;
                        }
                     }
                  }
               }

               if (!string.IsNullOrEmpty(filename)) {
                  for (int i = 0; i < 3; i++)         // wegen ev. Mehrfachencodierung ...
                     if (File.Exists(filename))
                        break;
                     else
                        filename = System.Web.HttpUtility.UrlDecode(filename);
               }
            } catch (Exception ex) {
               filename = "";
               await Helper2.ShowExceptionMessage(parentpage, "Fehler bei Analyse der URI '" + uri.OriginalString + "' (" + nameof(uriLink2Filename) + "())", ex);
            }
         return filename;
      }


      /// <summary>
      /// Workaround für ImageButton: falls bei "IsVisible = true" der Button nicht richtig angezeigt wird
      /// </summary>
      /// <param name="ib"></param>
      /// <param name="show"></param>
      public static void ShowImageButton(ImageButton ib, bool show = true) {
         ib.IsVisible = show;
         if (show) {
            Thickness pad = ib.Padding;
            ib.Padding = new Thickness(0);
            ib.Padding = pad;
         }
      }



      public static void WriteDebugLine(string txt) {
         if (MainThread.IsMainThread)
            File.AppendAllText("/storage/emulated/0/TrackEddi/debuginfo.txt", DateTime.Now.ToString("G") + ": " + txt + Environment.NewLine);
         else
            MainThread.BeginInvokeOnMainThread(() =>
                  File.AppendAllText("/storage/emulated/0/TrackEddi/debuginfo.txt", DateTime.Now.ToString("G") + ": " + txt + Environment.NewLine)
               //   File.AppendAllText("./debuginfo.txt", DateTime.Now.ToString("G") + ": " + txt);
               );
      }



   }
}
