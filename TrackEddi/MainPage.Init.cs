using FSofTUtils.Geography.DEM;
using FSofTUtils.Geography.Garmin;
using FSofTUtils.OSInterface;
using FSofTUtils.OSInterface.Storage;
using GMap.NET.FSofTExtented.MapProviders;
using TrackEddi.Common;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;

namespace TrackEddi {
   public partial class MainPage : ContentPage {

      class Init {

         long _isRunning = 1;

         /// <summary>
         /// Init läuft?
         /// </summary>
         public bool IsRunning {
            get => Interlocked.Read(ref _isRunning) != 0;
            set => Interlocked.Exchange(ref _isRunning, value ? 1 : 0);
         }

         long _isOnInitMap = 1;

         /// <summary>
         /// App ist in <see cref="InitMapAsync(string, bool)"/> bzw. beim Start noch davor
         /// </summary>
         public bool IsOnInitMap {
            get => Interlocked.Read(ref _isOnInitMap) != 0;
            set => Interlocked.Exchange(ref _isOnInitMap, value ? 1 : 0);
         }

         public StorageHelper? StorageHelper;


         MainPage mainPage;

         /// <summary>
         /// Pfad des 1. (ext.) Volumes (z.B. "/storage/self/primary" oder im Emulator "/mnt/user/0/primary")
         /// </summary>
         string firstVolumePath = string.Empty;

         string dataPath = string.Empty;

         /// <summary>
         /// normale Logdatei
         /// </summary>
         public string logfile { get; protected set; } = string.Empty;

         /// <summary>
         /// GPX-Datei für die Workbench
         /// </summary>
         string gpxworkbenchfile = string.Empty;


         public Init(MainPage mainPage) {
            this.mainPage = mainPage;
         }

         /// <summary>
         /// Karte oder Infobereich anzeigen
         /// </summary>
         /// <param name="visible"></param>
         public void ShowMapOrStartInfo(bool visible = true) =>
            MainThread.BeginInvokeOnMainThread(async () => {
               await mainPage.pushEditModeButton(ProgState.State.Viewer);
               mainPage.startInfo.IsVisible = !visible;
               mainPage.map.IsVisible = visible;
               mainPage.mainMenu.IsVisible = visible;
               mainPage.InvalidateMeasure();
            });

         async public Task<bool> InitAllAsync() {
            //mainPage.Title = TITLE + " (" + AppInfo.VersionString + ")";
            mainPage.Title = AppInfo.Name;

#if !DEBUG
            mainPage.ctrls4Debug.IsVisible = false;
#else
            mainPage.ctrls4Debug.IsVisible = true;
#endif
            bool initIsOk = true;

            await mainPage.showMainpageBusy();

            int trackcount = 0, markercount = 0;
            GpxWorkbench.LoadInfoEvent += async (sender, e) => {
               string txt = "";
               switch (e.LoadReason) {
                  case GpxWorkbench.LoadEventArgs.Reason.ReadXml:
                     txt = "GPX einlesen";
                     trackcount = markercount = 0;
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.ReadGDB:
                     txt = "GDB einlesen";
                     trackcount = markercount = 0;
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.ReadKml:
                     txt = "KML/KMZ einlesen";
                     trackcount = markercount = 0;
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.InsertWaypoints:
                     txt = "Marker einfügen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.InsertTracks:
                     txt = "Tracks einfügen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.InsertWaypoint:
                     if (mainPage.gpxWorkbench != null)
                        txt = " " + mainPage.gpxWorkbench.MarkerCount + " Marker gelesen";
                     else {
                        markercount++;
                        txt = " " + markercount + " Marker gelesen";
                     }
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.InsertTrack:
                     if (mainPage.gpxWorkbench != null)
                        txt = " " + mainPage.gpxWorkbench.TrackCount + " Track" + (mainPage.gpxWorkbench.TrackCount != 1 ? "s" : "") + " gelesen";
                     else {
                        trackcount++;
                        txt = " " + trackcount + " Track" + (trackcount != 1 ? "s" : "") + " gelesen";
                     }
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.SplitMultiSegmentTracks:
                     txt = "MultiSegmentTracks aufteilen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.RemoveEmptyTracks:
                     txt = "leere Tracks entfernen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.RebuildTrackList:
                     txt = "Trackliste erzeugen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.RebuildMarkerList:
                     txt = "Markerliste erzeugen";
                     break;
                  case GpxWorkbench.LoadEventArgs.Reason.ReadIsReady:
                     txt = "Workbench eingelesen";
                     break;
               }
               await appendStartInfoAsync("  " + txt);
            };

            mainPage.progState = new ProgState(mainPage.map);

            ShowMapOrStartInfo(false);

            await appendStartInfoAsync(AppInfo.Name +
                                       ", Version " + AppInfo.VersionString +
                                       ", Build " + AppInfo.BuildString + Environment.NewLine +
                                       "PackageName " + AppInfo.PackageName +
                                       Environment.NewLine);

            if (DirtyGlobalVars.AndroidActivity != null &&
                await PermissionsRequest.Request())
               await Task.Run(async () => {
                  try {
                     await appendStartInfoAsync("Init ...");

                     await appendStartInfoAsync("initTouchHandling() ...");
                     mainPage.initTouchHandling();

                     StorageHelper = new StorageHelper(DirtyGlobalVars.AndroidActivity);

                     // wird später noch geändert
                     gpxworkbenchfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), WORKBENCHGPXFILE);

                     MapCtrl.M_ThreadPoolSize = 2;

                     // Bis auf Ausnahmen muss die gesamte Init-Prozedur fehlerfrei laufen. Sonst erfolgt ein Prog-Abbruch.
                     if (await initDepTools(DirtyGlobalVars.AndroidActivity)) {
                        dataPath = Path.Combine(firstVolumePath, DATAPATH);
                        mainPage.appData = new AppData(string.Empty, dataPath);

                        App.ErrorFilename = UIHelper.ExceptionLogfile = Path.Combine(dataPath, ERRORLOGFILE);
                        App.LogFilename = logfile = Path.Combine(dataPath, LOGFILE);

                        gpxworkbenchfile = Path.Combine(dataPath, WORKBENCHGPXFILE);

                        if (initDataPath(dataPath)) {
                           //string currentpath = Directory.GetCurrentDirectory();
                           Directory.SetCurrentDirectory(dataPath); // Directory.GetCurrentDirectory() liefert z.B.: /storage/emulated/0/TrackEddi

                           // Wenn im Android-ErrorLog etwas steht, wird es übernommen und das Android-ErrorLog wird gelöscht.
                           if (File.Exists(oslogfile) &&
                               new System.IO.FileInfo(oslogfile).Length > 0) {
                              File.AppendAllLines(UIHelper.ExceptionLogfile, File.ReadAllLines(oslogfile));
                              File.Delete(oslogfile);
                           }

                           await appendStartInfoAsync(nameof(initConfig) + "(" + Path.Combine(dataPath, CONFIGFILE) + ") ...");
                           mainPage.config = initConfig(Path.Combine(dataPath, CONFIGFILE)).Result;

                           await InitMapAsync(dataPath, true);

                           await appendStartInfoAsync(nameof(initVisualTrackData) + "() ...");
                           initVisualTrackData(mainPage.config);

                           try {
                              await appendStartInfoAsync(nameof(initGarminMarkerSymbols) + "() ...");
                              mainPage.garminMarkerSymbols = initGarminMarkerSymbols(dataPath, mainPage.config);
                              await appendStartInfoAsync(" " + mainPage.garminMarkerSymbols.Count + " Symbole");
                              SpecialMapCtrl.VisualMarker.RegisterExternSymbols(mainPage.garminMarkerSymbols);
                           } catch (Exception ex) {
                              mainPage.IsBusy = false;
                              await Helper2.ShowExceptionMessage(mainPage, "Fehler beim Lesen der Garmin-Symbole", ex);
                              await mainPage.showMainpageBusy();
                           }

                           await appendStartInfoAsync(nameof(initWorkbenchAsync) + "() ...");
                           mainPage.gpxWorkbench = await initWorkbenchAsync(mainPage.config,
                                                                       mainPage.appData,
                                                                       gpxworkbenchfile,
                                                                       mainPage.map,
                                                                       mainPage.dem);

                           //map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
                           //map.Map_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.ViewCenter;
                           mainPage.map.M_MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;

                           if (mainPage.geoLocation == null) {
                              mainPage.geoLocation = new GeoLocation(mainPage.map, mainPage.dem) {
                                 //logfile = Path.Combine(FirstVolumePath, DATAPATH, "location.txt")
                              };

                              mainPage.geoLocation.AppendPoint += async (s1, e1) => {  // wenn ein neuer Punkt getrackt wurde

                                 // wenn eine bestimmte Seite gerade aktiv ist, wird sie informiert
                                 Page page = mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 1];

                                 if (page is WorkbenchContentPage &&
                                     mainPage.geoLocation.LiveTrack != null)
                                    ((WorkbenchContentPage)page).LiveTrackAppendPoint(mainPage.geoLocation.LiveTrack);

                                 // ev. den Livetrack speichern
                                 if (mainPage.gpxWorkbench != null &&
                                     ++mainPage.gpxWorkbench.UnsavedLivetrackPoints >= MAXUNSAVEDLIVETRACKPOINTS) {
                                    try {
                                       mainPage.appData?.Save();
                                       await mainPage.gpxWorkbench.SaveAsync(true);
                                       mainPage.gpxWorkbench.UnsavedLivetrackPoints = 0;
                                    } catch (Exception ex) {
                                       await Helper2.ShowExceptionMessage(mainPage, "Fehler bei GeoLocation.AppendPoint", ex);
                                    }
                                 }
                              };
                           }

                        } else
                           throw new Exception("Kein Zugriff auf das Konfigurationsverzeichnis '" + dataPath + "' möglich.");
                     } else
                        throw new Exception("Kein Zugriff auf das Dateisystem möglich.");
                  } catch (Exception ex) {
                     initIsOk = false;
                     mainPage.IsBusy = false;
                     await Helper2.ShowExceptionMessage(mainPage, "Fehler mit App-Abbruch", ex, true);  // Abbruch
                     return;
                  } finally {
                     mainPage.IsBusy = false;
                  }

                  ShowMapOrStartInfo();

               });

            // Die Beendigung ist hier ungünstig da die Fehleranzeige aus dem Androidteil dann nicht mehr sichtbar wird!
            //else
            //   Application.Current?.Quit();  // Android
            //// Windows: Application.Current?.CloseWindow(Application.Current.MainPage.Window);

            // Buttonstatus rekonstruieren
            if (initIsOk && mainPage.geoLocation != null) {
               mainPage.ButtonGeoLocationStart.IsVisible = !mainPage.geoLocation.LocationIsShowing;
               mainPage.ButtonGeoLocationStop.IsVisible = !mainPage.ButtonGeoLocationStart.IsVisible;

               mainPage.ButtonTrackingStart.IsVisible = !mainPage.geoLocation.LocationTracking;
               mainPage.ButtonTrackingStop.IsVisible = !mainPage.ButtonTrackingStart.IsVisible;

               mainPage.geoLocation.ScreenActualisationIsOn = true;
            }

            mainPage.IsBusy = false;

            return initIsOk;
         }

         public async Task ReInitMapAsync() => await InitMapAsync(dataPath, false);

         /// <summary>
         /// entweder aus <see cref="InitAllAsync()"/> aufgerufen oder nach der Änderung der Konfiguration
         /// </summary>
         /// <param name="datapath"></param>
         /// <param name="firstcall"></param>
         async public Task InitMapAsync(string datapath, bool firstcall) {
            if (mainPage.config != null && mainPage.appData != null) {
               IsOnInitMap = true;
               ShowMapOrStartInfo(false);
               //map.MapServiceEnd();

               await appendStartInfoAsync(nameof(initDEM) + "() ...");
               mainPage.dem = initDEM(mainPage.config);
               await appendStartInfoAsync("   DemPath " + mainPage.config.DemPath + System.Environment.NewLine +
                                          "   DemCachesize " + mainPage.config.DemCachesize + System.Environment.NewLine +
                                          "   DemCachePath " + mainPage.config.DemCachePath);

               mainPage.map.M_CacheLocation = Path.Combine(datapath, mainPage.config.CacheLocation);
               await initMapProvider(mainPage.map, mainPage.config, mainPage.dem);

               await appendStartInfoAsync(nameof(initAndStartMap) + "() ...");
               initAndStartMap(mainPage.map, mainPage.config, firstcall);

               await appendStartInfoAsync(nameof(setProviderZoomPositionAsync) + "() ...");
               int idx = mainPage.config.StartProvider;
               for (int i = 0; i < mainPage.map.M_ProviderDefinitions.Count; i++) {
                  if (mainPage.map.M_ProviderDefinitions[i].MapName == mainPage.appData.LastMapname) {
                     idx = i;
                     break;
                  }
               }

               await setProviderZoomPosition(idx,                 // entweder config.StartProvider oder entsprechend appData.LastMapname
                                             mainPage.appData.LastZoom,
                                             mainPage.appData.LastLongitude,
                                             mainPage.appData.LastLatitude);
               IsOnInitMap = false;
            }
         }

         #region interne Funktionen

         /// <summary>
         /// Infotext zum Text im <see cref="startInfoArea"/> anhängen
         /// </summary>
         /// <param name="txt"></param>
         async Task appendStartInfoAsync(string txt) {
            if (MainThread.IsMainThread)
               await appendWithScroll(txt);
            else
               await MainThread.InvokeOnMainThreadAsync(async () => {
                  await appendWithScroll(txt);
               });
         }

         async Task appendWithScroll(string txt, int delayms = 1) {
            mainPage.startInfoArea.Text += txt + System.Environment.NewLine;
            if (delayms > 0)
               await Task.Delay(delayms);
            await mainPage.startInfo.ScrollToAsync(mainPage.StartInfoEnd, ScrollToPosition.Start, false);
         }

         async Task<bool> initDepTools(object androidactivity) {
            await appendStartInfoAsync(nameof(initDepTools) + "() ...");
            StorageHelper = new StorageHelper(androidactivity);
            List<string> volumenpaths = StorageHelper.VolumePaths;

            if (volumenpaths.Count < 1) {
               throw new Exception("Kein external Storage vorhanden.");
            } else {
               firstVolumePath = volumenpaths[0];
            }

            bool[] results = new bool[volumenpaths.Count];
            for (int v = 0; v < StorageHelper.Volumes; v++)
               //if (!StorageHelper.SetAndroidPersistentPermissions(v))
               //   results[v] = await StorageHelper.Ask4AndroidPersistentPermissonAndWait(v);
               //else
               results[v] = true;

            return results[0];
         }

         bool initDataPath(string datapath) {
            if (StorageHelper == null)
               return false;
            if (!StorageHelper.DirectoryExists(datapath))
               if (!StorageHelper.CreateDirectory(datapath))
                  return false;
            return true;
         }

         async Task<Config> initConfig(string configfile) {
            if (StorageHelper != null &&
                !StorageHelper.FileExists(configfile)) {      // wenn noch keine Config-Datei ex. wird die Dummy-Datei in das Verzeichnis geschrieben
               // eine als MauiAsset im Projekt eingefügte Datei lesen:
               using var stream = await FileSystem.OpenAppPackageFileAsync("configdummy.xml");
               using var reader = new StreamReader(stream);
               string contents = reader.ReadToEnd();
               using var streamr = StorageHelper.OpenFile(configfile, "rw");
               if (streamr != null)
                  using (StreamWriter sw = new StreamWriter(streamr)) {
                     if (sw != null)
                        sw.Write(contents);
                  }
            }
            return new Config(configfile, null);
         }

         DemData initDEM(Config cfg) => ConfigHelper.ReadDEMDefinition(cfg);

         async Task initMapProvider(MapCtrl map, Config cfg, DemData dem) {
            await appendStartInfoAsync(nameof(initMapProvider) + "() ...");
            List<MapProviderDefinition> provdefs = ConfigHelper.ReadProviderDefinitions(cfg,
                                                                                        out mainPage.providxpaths,
                                                                                        out List<string> providernames,
                                                                                        dem);
            for (int i = 0; i < provdefs.Count; i++)
               await appendStartInfoAsync("   " + provdefs[i].MapName + " (" + provdefs[i].ProviderName + ")");
            map.M_RegisterProviders(providernames, provdefs);
         }

         async void initAndStartMap(MapCtrl map, Config cfg, bool firstcall) {
            double maxdisplaysize = Math.Max(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);

            if (firstcall) {
               map.M_TileLoadChange += (s, e) => mainPage.showTilesInWork(e.Count);
               map.M_PositionChanged += mainPage.map_OnPositionChanged;
               map.M_InnerExceptionThrown += async (s, e) => {
                  if (e.ShouldShow)
                     await Helper2.ShowExceptionMessage(mainPage, "Fehler bei " + nameof(map.M_InnerExceptionThrown), e.Exception);
                  else {
                     UIHelper.Message2Logfile("Fehler bei " + nameof(map.M_InnerExceptionThrown),
                                               UIHelper.GetExceptionMessage(e.Exception),
                                               UIHelper.ExceptionLogfile);
                  }
               };
               map.M_ZoomChanged += mainPage.map_ZoomChanged;
               map.M_Mouse += mainPage.map_Mouse;
               map.M_Marker += mainPage.map_Marker;
               map.M_Track += mainPage.map_Track;
               map.M_DrawOnTop += mainPage.map_DrawOnTop;
            }

            map.M_ShowTileGridLines =
#if DBEUG
                  true;                 // mit EmptyTileBorders gezeichnet
#else
               false;
#endif
            map.M_ShowCenter = false;                        // shows a little red cross on the map to show you exactly where the center is

            map.M_EmptyMapBackgroundColor = WinHelper.ConvertColor(Colors.LightYellow);   // Tile (noch) ohne Daten
            map.M_EmptyTileText = "keine Daten";            // Hinweistext für "Tile ohne Daten"
            map.M_EmptyTileColor = WinHelper.ConvertColor(Colors.LightGray);        // Tile (endgültig) ohne Daten

            map.M_ScaleAlpha = 150;
            map.M_ScaleKind = SpecialMapCtrl.Scale4Map.ScaleKind.Around;

            MapCtrl.M_CacheIsActiv = !cfg.ServerOnly;
            MapCtrl.M_SetProxy(cfg.WebProxyName,
                                    cfg.WebProxyPort,
                                    cfg.WebProxyUser,
                                    cfg.WebProxyPassword);

            mainPage.centerTargetIsVisible = false;
            map.M_ClickTolerance4Tracks = (float)(cfg.ClickTolerance4Tracks * maxdisplaysize / 100.0);

            List<MapProviderDefinition> provdefs = map.M_ProviderDefinitions;

            if (mainPage.config != null && mainPage.appData != null) {
               int startprovider = mainPage.config.StartProvider;       // EmptyProvider.Instance, GoogleMapProvider.Instance
               if (!mainPage.appData.IsCreated) {     // wurde noch nie verwendet
                  mainPage.appData.LastLatitude = mainPage.config.StartLatitude;
                  mainPage.appData.LastLongitude = mainPage.config.StartLongitude;
                  mainPage.appData.LastZoom = mainPage.config.StartZoom;
                  mainPage.appData.IsCreated = true;
               } else {
                  string mapname = mainPage.appData.LastMapname;
                  for (int i = 0; i < provdefs.Count; i++) {
                     if (provdefs[i].MapName == mapname) {
                        startprovider = i;
                        break;
                     }
                  }
               }
               if (startprovider >= provdefs.Count)
                  startprovider = -1;

               if (firstcall)
                  await map.MapServiceStartAsync(mainPage.appData.LastLongitude,
                                                 mainPage.appData.LastLatitude,
                                                 IOHelper.GetFullPath(mainPage.config.CacheLocation),
                                                 (int)mainPage.appData.LastZoom,
                                                 MapCtrl.ScaleModes.Fractional);
               string cache = mainPage.config.CacheLocation;
               if (!Path.IsPathRooted(cache))
                  cache = Path.GetFullPath(cache);
               map.M_CacheLocation = cache;

               //map.Map_ShowTileGridLines = false; // auch bei DEBUG

               if (startprovider >= 0)
                  await setProviderZoomPosition(startprovider,
                                                mainPage.appData.LastZoom,
                                                mainPage.appData.LastLongitude,
                                                mainPage.appData.LastLatitude);

#if DEBUG
               mainPage.map_ZoomChanged(null, EventArgs.Empty);  // löst die korrekte Zoomanzeige aus
#endif
            }
         }

         private void Map_M_InnerExceptionThrown(object? sender, MapCtrl.ExceptionThrownEventArgs e) {
            throw new NotImplementedException();
         }

         async Task setProviderZoomPosition(int mapidx, double zoom, double lon, double lat) {
            if (mapidx >= 0)
               try {
                  await mainPage.setProviderZoomPositionAsync(mapidx, zoom, lon, lat);
               } catch (Exception ex) {
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler beim Verwenden der Karte", ex);
               }
         }

         void initVisualTrackData(Config cfg) => ConfigHelper.ReadVisualTrackDefinitions(cfg);

         async Task<GpxWorkbench> initWorkbenchAsync(Config config, AppData appData, string gpxworkbenchfile, MapCtrl map, DemData? dem) {
            UIHelper.SetBusyStatusEvent += showWorkbenchBusyStatus;
            GpxWorkbench wb = new GpxWorkbench(mainPage,
                                               map,
                                               dem,
                                               gpxworkbenchfile,
                                               config.HelperLineColor,
                                               config.HelperLineWidth,
                                               config.StandardTrackColor,
                                               config.StandardTrackWidth,
                                               config.SymbolZoomfactor,
                                               appData.GpxDataChanged);
            await mainPage.showMainpageBusy();
            if (map != null) {
               // Nach dem Einlesen sind alle Tracks "unsichtbar".
               List<bool> tmp = appData.VisibleStatusTrackList;
               for (int i = 0; i < tmp.Count && i < wb.TrackCount; i++)
                  if (tmp[i])
                     Helper2.ShowTrack(map, wb.GetTrack(i));

               tmp = appData.VisibleStatusMarkerList;
               for (int i = 0; i < tmp.Count && i < wb.MarkerCount; i++)
                  if (tmp[i])
                     Helper2.ShowMarker(map, wb.GetMarker(i));
            }

            //wb.Gpx.TracklistChanged += gpxWorkbench_TracklistChanged;
            //wb.Gpx.MarkerlistlistChanged += gpxWorkbench_MarkerlistlistChanged;
            //wb.Gpx.ChangeIsSet += gpxWorkbench_ChangeIsSet;
            wb.MarkerShouldInsertEvent += mainPage.gpxWorkbench_MarkerShouldInsertEvent;
            //wb.TrackEditShowEvent += gpxWorkbench_TrackEditShowEvent;

            return wb;
         }

         async void showWorkbenchBusyStatus(object? sender, UIHelper.BusyEventArgs e) {
            if (e.Page == mainPage ||
                e.Page == null) {
               if (e.Busy)
                  await mainPage.showMainpageBusy();
               else
                  mainPage.IsBusy = e.Busy;
            }
         }

         List<GarminSymbol> initGarminMarkerSymbols(string datapath, Config cfg) => ConfigHelper.ReadGarminMarkerSymbols(cfg, datapath);

         #endregion
      }

   }
}
