using FSofTUtils.Geometry;
using FSofTUtils.OSInterface;
using FSofTUtils.OSInterface.Control;
using FSofTUtils.OSInterface.Page;
using SpecialMapCtrl;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TrackEddi.Common;
using TrackEddi.ConfigEdit;

namespace TrackEddi {
   public partial class MainPage : ContentPage {

      bool isNotOnInitMap => init != null && !init.IsOnInitMap;


      async private void ToolbarItem_ChooseMap_Clicked(object? sender, EventArgs e) {
         if (subpages != null)
            await subpages.MapChoosing();
      }

      async private void ToolbarItem_ReloadMap_Clicked(object? sender, EventArgs e) {
         if (isNotOnInitMap) {
            bool clearpartial = false;
            if (await UIHelper.ShowYesNoQuestion_RealYes(this, "Soll auch der Cache für den angezeigten Bereich gelöscht werden?", "Reload"))
               clearpartial = true;
            map.M_Refresh(true, false, false, clearpartial);
         }
      }

      #region Load and Save

      private void ToolbarItem_GPXOpen_Clicked(object? sender, EventArgs e) => toolbarItem_GPXOpen(false);

      private void ToolbarItem_GPXAppendClicked(object? sender, EventArgs e) => toolbarItem_GPXOpen(true);

      async private void toolbarItem_GPXOpen(bool append) {
         if (isNotOnInitMap &&
             appData != null &&
             gpxWorkbench != null)
            try {
               ChooseFilePage chooseFilePage = new ChooseFilePage() {
                  AndroidActivity = DirtyGlobalVars.AndroidActivity,
                  Path = appData.LastLoadSavePath,
                  Filename = "",
                  OnlyExistingFile = true,   // ohne Eingabefeld für Namen
                  Match4Filenames = new Regex(@"\.(gpx|kml|kmz)$", RegexOptions.IgnoreCase),
                  Title = "GPX-Datei auswählen",
               };
               chooseFilePage.ChooseFileReadyEvent += async (object? sender,
                                                             ChooseFile.ChoosePathAndFileEventArgs e) => {
                                                                if (e.OK) {
                                                                   appData.LastLoadSavePath = e.Path;
                                                                   await Loadfile2gpxworkbench(gpxWorkbench,
                                                                                               Path.Combine(e.Path, e.Filename),
                                                                                               append,
                                                                                               false);
                                                                }
                                                             };

               await FSofTUtils.OSInterface.Helper.GoTo(chooseFilePage);
            } catch (Exception ex) {
               await Helper2.ShowExceptionMessage(this, "Fehler beim Lesen einer GPX-Datei", ex);
            }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="gpxWorkbench"></param>
      /// <param name="file">GPX-Datei</param>
      /// <param name="append">alten Inhalt löschen oder zusätzlich einfügen</param>
      /// <param name="atendoflist">vor oder nach dem alten Inhalt einfügen</param>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      internal async Task<bool> Loadfile2gpxworkbench(GpxWorkbench gpxWorkbench, string file, bool append, bool atendoflist) {
         int tracksold = gpxWorkbench.TrackCount;
         int markersold = gpxWorkbench.MarkerCount;
         bool result = true;

         try {
            await showMainpageBusy();

            // akt. Tracks und Marker merken
            Track[] oldtracks = new Track[gpxWorkbench.TrackList.Count];
            gpxWorkbench.TrackList.CopyTo(oldtracks);
            Marker[] oldmarker = new Marker[gpxWorkbench.MarkerList.Count];
            gpxWorkbench.MarkerList.CopyTo(oldmarker);

            if (config != null) {
               if (!await IOHelper.Load(this,
                                        gpxWorkbench.Gpx,
                                        file,
                                        append,
                                        atendoflist,
                                        config.StandardTrackWidth,
                                        config.SymbolZoomfactor,
                                        VisualTrack.EditableColor))
                  return false;
            }

            // neue Tracks und Marker anzeigen
            foreach (Track track in gpxWorkbench.TrackList) {
               if (Array.IndexOf(oldtracks, track) >= 0)
                  continue;
               Helper2.ShowTrack(map, track);
            }
            foreach (Marker marker in gpxWorkbench.MarkerList) {
               if (Array.IndexOf(oldmarker, marker) >= 0)
                  continue;
               Helper2.ShowMarker(map, marker);
            }

            gpxWorkbench.Gpx.VisualRefresh();

            WorkbenchContentPage? workbenchContentPage = null;
            if (Navigation.NavigationStack != null)
               foreach (var item in Navigation.NavigationStack)
                  if (item is WorkbenchContentPage) {
                     workbenchContentPage = (WorkbenchContentPage)item;
                     break;
                  }
            if (workbenchContentPage != null)
               await workbenchContentPage.ActualizeContent();

         } catch (Exception ex) {
            result = false;
            throw new Exception(ex.Message);
         } finally {
            IsBusy = false;
         }
         return result;
      }

      async private void ToolbarItem_SaveAsClicked(object sender, EventArgs e) => await _toolbarItem_SaveAsClicked(false);

      async private void ToolbarItem_SaveAsMultiClicked(object sender, EventArgs e) => await _toolbarItem_SaveAsClicked(true);

      async private Task _toolbarItem_SaveAsClicked(bool multi) {
         if (isNotOnInitMap)
            try {
               string? path = appData != null ?
                                 string.IsNullOrEmpty(appData.LastFullSaveFilename) ?
                                          appData.LastLoadSavePath :
                                          Path.GetDirectoryName(appData.LastFullSaveFilename) :
                                 null;
               // "TrackEddi-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".gpx";
               //string? filename = appData != null ?
               //                  string.IsNullOrEmpty(appData.LastFullSaveFilename) ?
               //                           string.Empty :
               //                           Path.GetFileName(appData.LastFullSaveFilename) :
               //                  null;

               string? filename = DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".gpx";

               ChooseFilePage chooseFilePage = new ChooseFilePage() {
                  AndroidActivity = DirtyGlobalVars.AndroidActivity,
                  Path = path ?? string.Empty,
                  Filename = filename ?? string.Empty,
                  Match4Filenames = new Regex(@"\.(gpx|kml|kmz)$", RegexOptions.IgnoreCase),
                  OnlyExistingFile = false,   // mit Eingabefeld für Namen
                  Title = "Zieldatei auswählen",
               };

               await showMainpageBusy();

               if (multi)
                  chooseFilePage.ChooseFileReadyEvent += async (object? sender, ChooseFile.ChoosePathAndFileEventArgs e) => {
                     if (e.OK)
                        await saveGpxWorkbench(Path.Combine(e.Path, e.Filename), true);
                  };
               else
                  chooseFilePage.ChooseFileReadyEvent += async (object? sender, ChooseFile.ChoosePathAndFileEventArgs e) => {
                     if (e.OK)
                        await saveGpxWorkbench(Path.Combine(e.Path, e.Filename), false);
                  };

               await FSofTUtils.OSInterface.Helper.GoTo(chooseFilePage);
            } catch (Exception ex) {
               await Helper2.ShowExceptionMessage(this, "Fehler beim Speichern", ex);
            } finally {
               IsBusy = false;
            }
      }

      async Task saveGpxWorkbench(string filename, bool multi) {
         try {
            if (gpxWorkbench != null &&
                appData != null) {
               appData?.Save();
               if (gpxWorkbench.DataChanged)
                  await gpxWorkbench.SaveAsync(false);

               if (await IOHelper.SaveGpx(this,
                                          gpxWorkbench.Gpx,
                                          filename,
                                          multi,
                                          AppInfo.Name,
                                          true,
                                          gpxWorkbench.GetTrackColors())) {
                  if (multi) {
                     string? path = Path.GetDirectoryName(filename);
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
                     appData.LastLoadSavePath = path ?? string.Empty;
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
                  } else
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
                     appData.LastFullSaveFilename = filename;
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
               }
            }
         } catch (Exception ex) {
            throw new Exception(ex.Message);
         } finally {
            IsBusy = false;
         }
      }

      #endregion

      async private void ToolbarItem_OsmSearchClicked(object? sender, EventArgs e) {
         if (subpages != null)
            await subpages.OsmSearch();
      }


      async private void ToolbarItem_GoToClicked(object? sender, EventArgs e) {
         if (subpages != null)
            await subpages.GoTo();
      }

      async private void ToolbarItem_GpxContent_Clicked(object? sender, EventArgs e) {
         if (subpages != null)
            await subpages.ShowTracklistAndMarkerlist();
      }

      bool isOnGPXSearch = false;
      List<string> gpxSearchFiles = new List<string>();
      CheckRouteCrossing checkRouteCrossing = new CheckRouteCrossing();

      async private void ToolbarItem_GPXSearch(object? sender, EventArgs e) {
         if (!isOnGPXSearch &&
             appData != null &&
             gpxWorkbench != null) {
            isOnGPXSearch = true;

            PointD topleftLatLon = map.M_Client2LonLat(0, 0);
            PointD bottomrightLatLon = map.M_Client2LonLat(map.Width, map.Height);

            try {
               ChooseFilePage chooseFilePage = new ChooseFilePage() {
                  AndroidActivity = DirtyGlobalVars.AndroidActivity,
                  Path = appData.LastGpxSearchPath,
                  OnlyExistingDirectory = true,
                  Title = "GPX-Verzeichnis auswählen",
               };
               chooseFilePage.ChooseFileReadyEvent += async (object? s, ChooseFile.ChoosePathAndFileEventArgs ea) => {
                  if (ea.OK) {
                     if (appData.LastGpxSearchPath != ea.Path)
                        appData.LastGpxSearchPath = ea.Path;

                     showGpxSearchInfo(0, 0);
                     gpxSearchFiles.Clear();
                     await checkRouteCrossing.TestpathsAsync([appData.LastGpxSearchPath],
                                                             gpxSearchFiles,
                                                             topleftLatLon.X,
                                                             bottomrightLatLon.X,
                                                             bottomrightLatLon.Y,
                                                             topleftLatLon.Y,
                                                             showGpxSearchInfo);
                     isOnGPXSearch = false;
                     showGpxSearchInfo(-1, 0);
                     ToolbarItem_GPXLastSearch(null, EventArgs.Empty);
                  } else
                     isOnGPXSearch = false;
               };

               await FSofTUtils.OSInterface.Helper.GoTo(chooseFilePage);
            } catch (Exception ex) {
               showGpxSearchInfo(-1, 0);
               await UIHelper.ShowExceptionMessage(this,
                                                   "Fehler beim Lesen des GPX-Verzeichnis",
                                                   ex,
                                                   null,
                                                   false);
               isOnGPXSearch = false;
            } finally {
            }
         }
      }

      void ToolbarItem_GPXCancelSearch(object? sender, EventArgs e) {
         checkRouteCrossing.CancelTest();
      }

      async void ToolbarItem_GPXLastSearch(object? sender, EventArgs e) {
         if (!isOnGPXSearch &&
             gpxWorkbench != null &&
             gpxSearchFiles.Count > 0) {
            try {
               GPXSearchPage page = new GPXSearchPage(this, gpxWorkbench, gpxSearchFiles);
               await FSofTUtils.OSInterface.Helper.GoTo(page);
            } catch (Exception ex) {
               await Helper2.ShowExceptionMessage(this, "Fehler bei der Gpx-Dateianzeige", ex);
            }
         }
      }

      private void ToolbarItem_Compass_Clicked(object? sender, EventArgs e) {
         if (CompassExt.IsSupported)
            if (geoLocation != null) {
               geoLocation.CompassIsShowing = !geoLocation.CompassIsShowing;
            }
      }

      async private void ToolbarItem_LastLocation(object? sender, EventArgs e) {
         if (geoLocation != null &&
             isNotOnInitMap &&
             subpages != null)
            await subpages.ShowGeoLocation();
      }

      async private void ToolbarItem_GNSS(object sender, EventArgs e) {
         if (gnssData != null &&
             isNotOnInitMap &&
             subpages != null)
            await subpages.ShowGNSS();
      }

      async private void ToolbarItem_Config_Clicked(object? sender, EventArgs e) {
         if (isNotOnInitMap &&
             DirtyGlobalVars.AndroidActivity != null &&
             config != null)
            try {
               ConfigPage page = new ConfigPage(map, config, providxpaths);
               page.Disappearing += async (s, ea) => {
                  if (!page.IsOnOpeningOrClosingSubPage) {  // Page beendet oder Taskwechsel steht bevor
                     if (page.ConfigChanged && page.SaveButtonPressed) { // die Konfigurationsdatei wurde geändert
                        await showMainpageBusy();
                        page.ConfigEdit.SaveData();
                        if (config.XmlFilename != null)
                           config.Load(config.XmlFilename);

                        if (init != null) {
                           init.ShowMapOrStartInfo(false);
                           startInfoArea.Text = string.Empty;
                           init.IsRunning = true;
                           await init.ReInitMapAsync();
                           init.IsRunning = false;
                           init.ShowMapOrStartInfo(true);
                        }
                        IsBusy = false;

                        await UIHelper.ShowInfoMessage(this,
@"Je nach veränderten Daten der Konfiguration muss ev. der Kartencache für eine oder alle Karten gelöscht werden!

Sonst werden die Änderungen ev. nicht wirksam.",
"Achtung");
                     }
                  }
               };
               await FSofTUtils.OSInterface.Helper.GoTo(page);
            } catch (Exception ex) {
               await Helper2.ShowExceptionMessage(this, "Fehler bei " + nameof(ConfigPage), ex);
            }
      }

      async private void ToolbarItem_DeleteCache_Clicked(object? sender, EventArgs e) {
         if (subpages != null)
            await subpages.ShowCacheManagementPage(); // deleteCache(map.M_ActualMapIdx);
      }

      async private void ToolbarItem_Help_Clicked(object? sender, EventArgs e) {
         if (isNotOnInitMap)
            try {
               await FSofTUtils.OSInterface.Helper.GoTo(new HelpPage());
            } catch (Exception ex) {
               await Helper2.ShowExceptionMessage(this, "Fehler bei " + nameof(HelpPage), ex);
            }
      }

   }
}
