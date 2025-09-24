#define USESTDGESTURES

using FSofTUtils.Geography.DEM;
using FSofTUtils.Geography.Garmin;
using FSofTUtils.OSInterface;
using GMap.NET.FSofTExtented.MapProviders;
using SkiaSharp;
using SpecialMapCtrl;
using System.Diagnostics;
using TrackEddi.Common;
using TrackEddi.Gnns;
using MapCtrl = SpecialMapCtrl.SpecialMapCtrl;

namespace TrackEddi {
   public partial class MainPage : ContentPage {

      #region Definitionen

      //const string TITLE = "TrackEddi, © by FSofT";

      /// <summary>
      /// Standard-Datenverzeichnis der App
      /// </summary>
      const string DATAPATH = "TrackEddi";

      /// <summary>
      /// Name der Konfigurationsdatei (im <see cref="DATAPATH"/>)
      /// </summary>
      const string CONFIGFILE = "config.xml";

      /// <summary>
      /// Logdatei für Exceptions (im <see cref="DATAPATH"/>)
      /// </summary>
      const string ERRORLOGFILE = "error.txt";

      /// <summary>
      /// normale Logdatei (im <see cref="DATAPATH"/>)
      /// </summary>
      const string LOGFILE = "log.txt";

      /// <summary>
      /// (private) Datei für die Workbench-Daten
      /// </summary>
      const string WORKBENCHGPXFILE = "persistent.gpx";

      readonly Color MainMenuBackcolorStd = Colors.LightGreen;

      readonly Color MainMenuBackcolorEdit = Color.FromRgb(255, 128, 128);

      /// <summary>
      /// spätestens nach dieser Anzahl neuer Livetrackpoints wird die Workbench gespeichert
      /// </summary>
      const int MAXUNSAVEDLIVETRACKPOINTS = 10;

      static string oslogfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                             "TrackEddiErrorLog.txt");

      #endregion

      /// <summary>
      /// Ist die Initialisierung erfolgreich gewesen? (Rechte)
      /// </summary>
      bool initisok = false;

      /// <summary>
      /// zum direkten Zugriff auf die Karte
      /// </summary>
      public MapCtrl Map => map;

      /// <summary>
      /// Läuft gerade das Tracking?
      /// </summary>
      bool IsOnTracking => geoLocation != null && geoLocation.LocationTracking;

      /// <summary>
      /// persistente Programmdaten
      /// </summary>
      AppData? appData;

      GpxWorkbench? gpxWorkbench;

      /// <summary>
      /// Konfigurationsdaten
      /// </summary>
      Config? config;

      /// <summary>
      /// für die Ermittlung der Höhendaten
      /// </summary>
      DemData? dem = null;

      /// <summary>
      /// Liste der registrierten Garmin-Symbole
      /// </summary>
      List<GarminSymbol> garminMarkerSymbols = new List<GarminSymbol>();

      /// <summary>
      /// wenn Tiles geladen werden 1, sonst 0 (threadsichere Abfrage!)
      /// </summary>
      long tileLoadIsRunning = 0;

      bool centerTargetIsVisible {
         get => map != null ? map.M_ShowCenter : false;
         set {
            if (map != null)
               if (map.M_ShowCenter != value) {
                  map.M_ShowCenter = value;
                  map.M_Refresh(false, false, false, false);
               }
         }
      }

      /// <summary>
      /// Kartenmitte in Maui-Koordinaten
      /// </summary>
      Point mauiMapCenter => new Point(MapCtrl.SkiaX2MauiX(map.Width) / 2,
                                       MapCtrl.SkiaY2MauiY(map.Height) / 2);

      /// <summary>
      /// Kartenmitte in Client-Koordinaten
      /// </summary>
      internal System.Drawing.Point ClientMapCenter => new System.Drawing.Point(map.Width / 2, map.Height / 2);

      GeoLocation? geoLocation;

      GnssData? gnssData;


      /// <summary>
      /// Bedeutung für Fingertips
      /// </summary>
      enum TappingType {
         Standard,
         Mapinfo,
         DeleteObjects,
      }

      /// <summary>
      /// akt. Bedeutung des Fingertips
      /// </summary>
      TappingType TapType = TappingType.Standard;

      List<int[]> providxpaths = new List<int[]>();

      /// <summary>
      /// Standardeigenschaft überschreiben, damit <see cref="BusyIndicator"/> "von außen" verwendet werden kann
      /// </summary>
      new bool IsBusy {
         get => BusyIndicator.IsRunning;
         set {
            if (MainThread.IsMainThread)
               BusyIndicator.IsRunning = value;
            else
               MainThread.BeginInvokeOnMainThread(() => BusyIndicator.IsRunning = value);
         }
      }

      #region bindable Props für Bildschirmorientierung

      public static readonly BindableProperty StackOrientationVertical4PortraitProperty = BindableProperty.Create(
               nameof(StackOrientationVertical4Portrait),
               typeof(StackOrientation),
               typeof(MainPage),
               StackOrientation.Vertical);

      public StackOrientation StackOrientationVertical4Portrait {
         get => (StackOrientation)GetValue(StackOrientationVertical4PortraitProperty);
         set => SetValue(StackOrientationVertical4PortraitProperty, value);
      }

      public static readonly BindableProperty StackOrientationHorizontal4PortraitProperty = BindableProperty.Create(
         nameof(StackOrientationHorizontal4Portrait),
         typeof(StackOrientation),
         typeof(MainPage),
         StackOrientation.Horizontal);

      public StackOrientation StackOrientationHorizontal4Portrait {
         get => (StackOrientation)GetValue(StackOrientationHorizontal4PortraitProperty);
         set => SetValue(StackOrientationHorizontal4PortraitProperty, value);
      }

      public static readonly BindableProperty ScrollOrientationVertical4PortraitProperty = BindableProperty.Create(
         nameof(ScrollOrientationVertical4Portrait),
         typeof(ScrollOrientation),
         typeof(MainPage),
         ScrollOrientation.Vertical);

      public ScrollOrientation ScrollOrientationVertical4Portrait {
         get => (ScrollOrientation)GetValue(ScrollOrientationVertical4PortraitProperty);
         set => SetValue(ScrollOrientationVertical4PortraitProperty, value);
      }

      public static readonly BindableProperty ScrollOrientationHorizontal4PortraitProperty = BindableProperty.Create(
         nameof(ScrollOrientationHorizontal4Portrait),
         typeof(ScrollOrientation),
         typeof(MainPage),
         ScrollOrientation.Horizontal);

      public ScrollOrientation ScrollOrientationHorizontal4Portrait {
         get => (ScrollOrientation)GetValue(ScrollOrientationHorizontal4PortraitProperty);
         set => SetValue(ScrollOrientationHorizontal4PortraitProperty, value);
      }

      #endregion

      public ProgState? progState = null;

      #region Funktionsgruppen

      Subpages? subpages;

      public Dialogs? dialogs;

      Init? init;

      #endregion


      public MainPage() {
         InitializeComponent();
         App.MyMainPage = this;  // für die App-Events
      }

      /// <summary>
      /// Auswertung der App-Events
      /// </summary>
      /// <param name="ev"></param>
      public void AppEvent(App.AppEvent ev) {
         switch (ev) {
            case App.AppEvent.OnStart:    // nach Konstruktor und base.OnAppearing()
               onAppStart();
               break;

            case App.AppEvent.OnResume:   // nicht unbedingt mit OnAppearing() verbunden
               onAppResume();
               break;

            case App.AppEvent.OnSleep:    // danach ev. OnDisappearing()
               onAppSleep();
               break;
         }
      }

      async void onAppStart() {
         // für Codepage 1252 usw.
         System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

         subpages = new Subpages(this);
         dialogs = new Dialogs(this);
         init = new Init(this);

         initisok = false;
         await Task.Run(async () => {
            init.IsRunning = true;
            initisok = await init.InitAllAsync();
            if (initisok)
               config?.LoadData();      // falls aktualisiert
            init.IsRunning = false;
            if (initisok && gnssData == null) {
               gnssData = new GnssData();
               MainThread.BeginInvokeOnMainThread(() => gnssData.RegisterGnss());
            }
         });
      }

      void onAppResume() {
         if (geoLocation != null)
            geoLocation.ScreenActualisationIsOn = true;
      }

      async void onAppSleep() {
         if (geoLocation != null)
            geoLocation.ScreenActualisationIsOn = false;
         await Task.Run(async () => {
            try {
               if (appData != null) {
                  appData.LastZoom = map.M_Zoom;
                  appData.LastLatitude = map.M_CenterLat;
                  appData.LastLongitude = map.M_CenterLon;
                  int idx = map.M_ActualMapIdx;
                  if (idx >= 0)
                     appData.LastMapname = map.M_ProviderDefinitions[idx].MapName;

                  if (gpxWorkbench != null) {
                     appData.VisibleStatusTrackList = gpxWorkbench.VisibleStatusTrackList;
                     appData.VisibleStatusMarkerList = gpxWorkbench.VisibleStatusMarkerList;
                     appData.GpxDataChanged = gpxWorkbench.DataChanged;
                  }

                  appData.Save();
               }

               if (gpxWorkbench?.Gpx != null &&
                   gpxWorkbench.DataChanged)
                  await gpxWorkbench.SaveAsync(true);

            } catch (Exception ex) {   // Anzeige erst bei der "Wiedererweckung" der App
               await Helper2.ShowExceptionMessage(this, "Fehler bei " + nameof(AppEvent), ex);
            }
         });
      }

      /// <summary>
      /// Die Fenstergröße (die -ausrichtung) hat sich geändert
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      protected override void OnSizeAllocated(double width, double height) {
         base.OnSizeAllocated(width, height); //must be called

         if (width < height) {   // Portrait
            StackOrientationVertical4Portrait = StackOrientation.Vertical;
            StackOrientationHorizontal4Portrait = StackOrientation.Horizontal;
            ScrollOrientationVertical4Portrait = ScrollOrientation.Vertical;
            ScrollOrientationHorizontal4Portrait = ScrollOrientation.Horizontal;
         } else {                // Landscape
            StackOrientationVertical4Portrait = StackOrientation.Horizontal;
            StackOrientationHorizontal4Portrait = StackOrientation.Vertical;
            ScrollOrientationVertical4Portrait = ScrollOrientation.Horizontal;
            ScrollOrientationHorizontal4Portrait = ScrollOrientation.Vertical;
         }
      }

      /// <summary>
      /// Eine URI wurd per AppLink an die App geliefert.
      /// </summary>
      /// <param name="uri"></param>
      /// <returns></returns>
      public async Task ReceiveAppLink(Uri uri) {
         if (init != null) {
            if (init.IsRunning)
               await Task.Run(() => {
                  while (init.IsRunning)
                     Thread.Sleep(200);      // abwarten bis Start beendet
               });

            if (init.StorageHelper != null) {
               string filename = await Helper2.uriLink2Filename(this, init.StorageHelper, uri);
               if (!string.IsNullOrEmpty(filename)) {
                  try {
                     if (await UIHelper.ShowYesNoQuestion_RealYes(this,
                                                                  "Soll die Datei '" + filename + "' hinzugefügt werden? "/* + uri.OriginalString*/,
                                                                  "Achtung")) {
                        await showMainpageBusy();
                        if (gpxWorkbench != null)
                           await Loadfile2gpxworkbench(gpxWorkbench, filename, true, false);
                     }

                  } catch (Exception ex) {
                     IsBusy = false;
                     await Helper2.ShowExceptionMessage(this, "Fehler beim Hinzufügen der Datei '" + filename + "' (" + nameof(ReceiveAppLink) + "())", ex);
                  } finally {
                     IsBusy = false;
                  }
               } else
                  UIHelper.Message2Logfile("ReceiveAppLink",
                                           "FEHLER bei Dateinamenerkennung: '" + uri.ToString() + "'",
                                           App.LogFilename); //init.logfile
            }
         }
      }

      #region Reaktion auf Map-Events

      // ACHTUNG
      // Es muss damit gerechnet werden, dass alle Map-Events NICHT im UI-Thread geliefert werden. Deshalb sicherheitshalber immer: 
      //    Device.BeginInvokeOnMainThread(()=> { ... });

      private void map_OnPositionChanged(object? sender, MapCtrl.PositionChangedEventArgs e) {
#if DEBUG
         MainThread.BeginInvokeOnMainThread(() => labelPos.Text = string.Format("{0:F6}° {1:F6}°", e.Point.Lng, e.Point.Lat));
#endif
      }

      private void map_ZoomChanged(object? sender, EventArgs e) {
#if DEBUG
         MainThread.BeginInvokeOnMainThread(() => labelInfo.Text = string.Format("Zoom {0:F3}, linear {1:F1}", map.M_Zoom, map.M_ZoomLinear));
#endif
         if (dem != null)
            dem.IsActiv = map.M_Zoom >= dem.MinimalZoom;
      }


      /// <summary>
      /// nur für die Trackbearbeitung: 2. Track für ein Concat
      /// </summary>
      Track? MarkedTrack4Concat;


      /// <summary>
      /// nach der Karte, den Tracks usw. ev. noch etwas zusätzlich zeichnen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void map_DrawOnTop(object? sender, MapCtrl.DrawExtendedEventArgs e) {
         if (gpxWorkbench != null && progState != null)
            switch (progState.ProgramState) {
               case ProgState.State.Edit_Marker:
                  gpxWorkbench.TrackDrawDestinationLine(e.Graphics, ClientMapCenter);
                  break;

               case ProgState.State.Edit_TrackDraw:
                  gpxWorkbench.TrackDrawDestinationLine(e.Graphics, ClientMapCenter);
                  break;

               case ProgState.State.Edit_TrackPointremove:
                  gpxWorkbench.TrackDrawNextTrackPoint(e.Graphics, ClientMapCenter);
                  break;

               case ProgState.State.Edit_TrackSplit:
                  gpxWorkbench.TrackDrawNextTrackPoint(e.Graphics, ClientMapCenter);
                  break;

               case ProgState.State.Edit_TrackConcat:
                  if (MarkedTrack4Concat != null)
                     gpxWorkbench.TrackDrawConcatLine(e.Graphics, MarkedTrack4Concat);
                  break;
            }

         if (config != null) {
            geoLocation?.ShowPosition(e.Graphics, config.LocationSymbolsize);
            geoLocation?.ShowCompass(e.Graphics, config.LocationSymbolsize);
         }

         showTilesInWork(map.M_WaitingTiles());
      }

      private async void map_Track(object? sender, MapCtrl.TrackEventArgs e) {
         if (e.Eventtype == MapCtrl.MapMouseEventArgs.EventType.Click)                 // Click => Tapped
            await userTapAction(//e.Button != System.Windows.Forms.MouseButtons.Left,    // long-Tap -> Right
                                TapType,
                                null,
                                e.Track,
                                Helper2.Client2MauiPoint(map.M_LonLat2Client(e.Lon, e.Lat)));
      }

      private async void map_Marker(object? sender, MapCtrl.MarkerEventArgs e) {
         if (e.Eventtype == MapCtrl.MapMouseEventArgs.EventType.Click)                 // Click => Tapped
            await userTapAction(//e.Button != System.Windows.Forms.MouseButtons.Left,    // long-Tap -> Right
                                TapType,
                                e.Marker,
                                null,
                                Helper2.Client2MauiPoint(map.M_LonLat2Client(e.Lon, e.Lat)));
      }

      private void map_Mouse(object? sender, MapCtrl.MapMouseEventArgs e) {

         // fkt. NICHT, weil danach noch zusätzlich Map_SpecMapMarkerEvent() oder Map_SpecMapTrackEvent() ausgelöst werden kann

         //if (e.Eventtype == MapCtrl.MapMouseEventArgs.EventType.Click) // Click => Tapped
         //   await userTapAction(e.Button != System.Windows.Forms.MouseButtons.Left,
         //                       null,
         //                       null,
         //                       Client2XamarinPoint(e.Location));
      }

      #endregion

      #region Events der GpxWorkbench

      /// <summary>
      /// ev. Aufname eines neuen <see cref="Marker"/>; wird über <see cref="Dialogs.SetNewMarker"/> ausgelöst
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      async void gpxWorkbench_MarkerShouldInsertEvent(object? sender, EditHelper.MarkerEventArgs e) {
         try {
            if (string.IsNullOrEmpty(e.Marker.Symbolname))
               e.Marker.Symbolname = "Flag, Green";            // <--> passend zum VisualMarker für editierbare Marker

            if (gpxWorkbench != null) {
               string[]? names = null;
               try {
                  names = await gpxWorkbench.GetNamesForGeoPointAsync(e.Marker.Longitude, e.Marker.Latitude);
               } catch (Exception ex) {
                  await Helper2.ShowExceptionMessage(this,
                                                     "Fehler bei " + nameof(gpxWorkbench_MarkerShouldInsertEvent) + " (get names)",
                                                     ex);
               }
               EditMarkerPage page = new EditMarkerPage(e.Marker, garminMarkerSymbols, names);
               page.EndWithOk += (object? sender2, EventArgs e2) => {
                  if (string.IsNullOrEmpty(e.Marker.Waypoint.Name))
                     e.Marker.Waypoint.Name = string.Format("M Lon={0:F6}°/Lat={1:F6}°", e.Marker.Waypoint.Lon, e.Marker.Waypoint.Lat);    // autom. Name
                  Marker? marker = gpxWorkbench?.MarkerInsertCopy(e.Marker);
                  if (marker != null) {
                     marker.Symbolzoom = config != null ? config.SymbolZoomfactor : 1;
                     Helper2.ShowMarker(map, marker);
                  }
               };
               await Helper.GoTo(page);
            }
         } catch (Exception ex) {
            await Helper2.ShowExceptionMessage(this, "Fehler bei " + nameof(gpxWorkbench_MarkerShouldInsertEvent), ex);
         }
      }

      #endregion

      /// <summary>
      /// setzt die gewünschte Karte, den Zoom und die Position oder liefert eine Exception
      /// </summary>
      /// <param name="mapidx">Index für die <see cref="SpecialMapCtrl.SpecialMapCtrl.M_ProviderDefinitions"/></param>
      /// <param name="zoom"></param>
      /// <param name="lon"></param>
      /// <param name="lat"></param>
      async Task setProviderZoomPositionAsync(int mapidx, double zoom, double lon, double lat) {
         // Zoom und Pos. einstellen
         await map.M_SetLocationAndZoomAsync(zoom, lon, lat);

         if (mapidx != map.M_ActualMapIdx) {                 // andere Karte anzeigen
            if (0 <= mapidx &&
                mapidx < providxpaths.Count &&
                mapidx < map.M_ProviderDefinitions.Count) {  // erlaubter Index
               bool hillshade = false;
               byte hillshadealpha = 0;
               bool hillshadeisactiv = dem != null && map.M_Zoom >= dem.MinimalZoom;

               if (0 <= mapidx) {
                  map.M_ClearWaitingTaskList();

                  MapProviderDefinition mapProviderDefinition = map.M_ProviderDefinitions[mapidx];
                  if (mapProviderDefinition.ProviderName == "Garmin") {
                     map.M_CancelTileBuilds();
                     hillshadealpha = ((GarminProvider.GarminMapDefinition)mapProviderDefinition).HillShadingAlpha;
                     hillshade = ((GarminProvider.GarminMapDefinition)mapProviderDefinition).HillShading;
                  } else if (mapProviderDefinition.ProviderName == "GarminKMZ") {
                     map.M_CancelTileBuilds();
                     hillshadealpha = ((GarminKmzProvider.KmzMapDefinition)mapProviderDefinition).HillShadingAlpha;
                     hillshade = ((GarminKmzProvider.KmzMapDefinition)mapProviderDefinition).HillShading;
                  }
               }
               if (dem != null) {
                  dem.WithHillshade = hillshade;
                  dem.IsActiv = hillshadeisactiv;
                  await map.M_SetActivProviderAsync(mapidx, hillshadealpha, dem, config != null ? config.Zoom4Displayfactor : 1);
               }
            } else
               throw new Exception("Der Kartenindex " + mapidx + " ist nicht vorhanden. (max. " + (map.M_ProviderDefinitions.Count - 1) + ")");
         }
      }

      /// <summary>
      /// Zusammenführung aller Tap-Aktionen (Tap auf <see cref="Marker"/>, <see cref="Track"/> oder in den "freien Raum"
      /// <para>ACHTUNG: Ein einzelner Tap kann mehrere <see cref="userTapAction(TappingType, Marker?, Track?, Point)"/>-Aufrufe auslösen!
      /// Das passiert immer dann, wenn mehrere Tracks und/oder Marker im Toleranzbereich liegen.
      /// </para>
      /// <para><see cref="TappingType.Mapinfo"/>: Info für diesen geografischen Punkt</para>
      /// <para><see cref="TappingType.DeleteObjects"/>; <see cref="ProgState.State.Edit_Marker"/>; auf Marker: (ev.) diesen Marker löschen</para>
      /// <para><see cref="TappingType.DeleteObjects"/>; <see cref="ProgState.State.Edit_TrackDraw"/>; auf Track: (ev.) diesen Track löschen</para>
      /// <para><see cref="TappingType.Standard"/>; <see cref="ProgState.State.Viewer"/>; auf Marker: (kurz oder lange) Anzeige der Markereigenschaften</para>
      /// <para><see cref="TappingType.Standard"/>; <see cref="ProgState.State.Viewer"/>; auf Track: (Kurz oder lange) Anzeige der Trackeigenschaften</para>
      /// <para><see cref="TappingType.Standard"/>; <see cref="ProgState.State.Edit_Marker"/>; auf Marker: Start Markerverschiebung</para>
      /// <para><see cref="TappingType.Standard"/>; <see cref="ProgState.State.Edit_Marker"/>: neuen Marker setzen</para>
      /// <para><see cref="TappingType.Standard"/>; <see cref="ProgState.State.Edit_TrackDraw"/>; auf Track: Track für Veränderung aktivieren/markieren</para>
      /// <para><see cref="TappingType.Standard"/>; <see cref="ProgState.State.Edit_TrackDraw"/>: Start eines neuen Tracks oder an aktivierten/markierten Track Punkt anhängen</para>
      /// <para><see cref="TappingType.Standard"/>; <see cref="ProgState.State.Edit_TrackConcat"/>; auf 1. Track: Start der Aktion für diesen Track</para>
      /// <para><see cref="TappingType.Standard"/>; <see cref="ProgState.State.Edit_TrackConcat"/>; auf 2. Track: 2. Track an 1. Track anhängen</para>
      /// <para><see cref="TappingType.Standard"/>; <see cref="ProgState.State.Edit_TrackSplit"/>; auf Track: Start der Aktion für diesen Track</para>
      /// </summary>
      /// <param name="tappingType">Tap-Typ</param>
      /// <param name="marker">Tap auf <see cref="Marker"/></param>
      /// <param name="track">Tap auf <see cref="Track"/></param>
      /// <param name="point">Tap weder auf <see cref="Marker"/> oder <see cref="Track"/></param>
      async Task userTapAction(TappingType tappingType, Marker? marker, Track? track, Point point) {
         // mögliche Varianten:  [5xProgState] *
         //                      [3xTaptyp] *
         //                      [3xObject(Marker, Track, -)] = 45

         switch (tappingType) {
            case TappingType.Mapinfo:
               if (dialogs != null)
                  await dialogs.Info4LonLatAsync(point);
               break;

            case TappingType.DeleteObjects:
               if (progState != null &&
                   gpxWorkbench != null &&
                   !gpxWorkbench.InWork) {
                  switch (progState.ProgramState) {
                     case ProgState.State.Edit_Marker:
                        if (marker != null) {                           // marker tapped
                           showEditInfoText(marker.Text);
                           if (dialogs != null)
                              await dialogs.RemoveMarker(marker);                  // remove this marker
                           showEditInfoText();
                        }
                        break;

                     case ProgState.State.Edit_TrackDraw:
                        if (track != null)
                           if (gpxWorkbench.TrackInEdit != track)       // another track tapped
                              if (dialogs != null)
                                 await dialogs.RemoveTrack(track);                 // remove this track
                        break;
                  }
               }
               break;

            case TappingType.Standard:
               if (progState != null)
                  switch (progState.ProgramState) {
                     case ProgState.State.Viewer:
                        showEditInfoText();
                        if (marker != null) {               // marker tapped
                           switch (await Helper.MessageBox(this,
                                                           "Marker: " + marker.Text,
                                                           ["weitere Info anzeigen", "ausführliche Info anzeigen", "zurück"],
                                                           null)) {
                              case 0:
                                 if (subpages != null)
                                    await subpages.ShowShortMarkerProps(marker);
                                 break;
                              case 1:
                                 if (subpages != null)
                                    await subpages.EditMarkerProps(marker);
                                 break;
                           }
                        } else if (track != null) {         // track tapped
                           switch (await Helper.MessageBox(this,
                                                           "Track: " + track.VisualName,
                                                           ["weitere Info anzeigen", "ausführliche Info anzeigen", "zurück"],
                                                           null)) {
                              case 0:
                                 if (dialogs != null)
                                    await dialogs.ShowShortTrackProps(track, point);
                                 break;
                              case 1:
                                 if (subpages != null)
                                    await subpages.EditTrackProps(track);
                                 break;
                           }
                        }
                        break;

                     case ProgState.State.Edit_Marker:
                        if (gpxWorkbench != null) {
                           if (gpxWorkbench.MarkerIsInWork) { // 1 Marker wurde schon (für das Verschieben) ausgewählt
                              gpxWorkbench?.MarkerEndEdit(Helper2.Maui2ClientPoint(point), false);     // falls noch kein Marker "InWork" neuer Marker, sonst neue Position für den Maker "InWork"
                              showEditInfoText();
                              await pushEditModeButton(ProgState.State.Viewer);
                           } else {
                              if (marker != null) {                           // marker tapped
                                 showEditInfoText(marker.Text);
                                 if (dialogs != null)
                                    await dialogs.StartMoveMarker(marker);               // start move this marker
                                 showEditInfoText();
                              } else
                                 if (dialogs != null)
                                 await dialogs.SetNewMarker(Helper2.Maui2ClientPoint(point)); // set new marker
                           }
                        }
                        break;

                     case ProgState.State.Edit_TrackDraw:
                        if (gpxWorkbench != null) {
                           if (!gpxWorkbench.TrackIsInWork) {  // Bearbeitung starten
                              if (track != null) {
                                 switch (await Helper.MessageBox(this,
                                                                 "Track bearbeiten",
                                                                 ["Track: " + track.VisualName, "neuen Track erzeugen", "zurück"],
                                                                 null)) {
                                    case 0:
                                       gpxWorkbench.TrackStartEdit(true, track);          // track extend
                                       break;

                                    case 1:
                                       gpxWorkbench.TrackStartEdit(true, null);           // new track
                                       gpxWorkbench.TrackAddPoint(Helper2.Maui2ClientPoint(point));
                                       break;

                                    case 2:
                                       return;
                                 }
                              } else {
                                 gpxWorkbench.TrackStartEdit(true, null);           // new track
                                 gpxWorkbench.TrackAddPoint(Helper2.Maui2ClientPoint(point));
                              }
                           } else
                              gpxWorkbench.TrackAddPoint(Helper2.Maui2ClientPoint(point));
                           showEditInfoText(gpxWorkbench.TrackInEdit);
                        }
                        break;

                     case ProgState.State.Edit_TrackPointremove:
                        if (gpxWorkbench != null) {
                           if (marker == null &&                                                                                 // NICHT für einen Marker
                               (track == null ||                                                                                 // NICHT für einen Track oder
                                (track != null && !gpxWorkbench.TrackIsInWork) ||                                                // für Track und noch kein Track in Arbeit
                                (track != null && gpxWorkbench.TrackIsInWork && track.Equals(gpxWorkbench.TrackInEdit)))) {      // für Track der in Arbeit ist
                              if (track != null && !gpxWorkbench.TrackIsInWork) {
                                 gpxWorkbench.TrackStartEdit(true, track);
                                 showEditInfoText(track);
                                 break;
                              }
                              if (gpxWorkbench.TrackIsInWork)
                                 gpxWorkbench.TrackRemoveNextPoint(Helper2.Maui2ClientPoint(point));
                           }
                        }
                        break;

                     case ProgState.State.Edit_TrackSplit:
                        if (gpxWorkbench != null) {
                           if (track != null && !gpxWorkbench.TrackIsInWork) {
                              gpxWorkbench.TrackStartEdit(true, track);
                              showEditInfoText(track);
                           }
                        }
                        break;

                     case ProgState.State.Edit_TrackConcat:
                        if (gpxWorkbench != null) {
                           if (track != null) {           // track tapped
                              if (!gpxWorkbench.TrackIsInWork) {  // 1. Tap
                                 MarkedTrack4Concat = null;
                                 gpxWorkbench.TrackStartEdit(true, track);
                                 showEditInfoText(track, "1. Track: ");
                              } else {                            // 2. oder mehr Tap
                                 if (!track.Equals(gpxWorkbench.TrackInEdit)) {  // ein anderer Track
                                    MarkedTrack4Concat = track;
                                    showEditInfoText(track, "2. Track: ");
                                    map.M_Refresh(false, false, false, false);
                                 }
                              }
                           }
                        }
                        break;
                  }
               break;
         }
      }

      /// <summary>
      /// zeigt einen Infotext beim Editieren an
      /// </summary>
      /// <param name="txt"></param>
      void showEditInfoText(string? txt = null) {
         if (string.IsNullOrEmpty(txt))
            EditInfoText.IsVisible = false;
         else {
            EditInfoText.Text = txt;
            EditInfoText.IsVisible = true;
            mainMenu.ScrollToAsync(EditInfoText, ScrollToPosition.End, true);
         }
      }

      /// <summary>
      /// zeigt einen Infotext beim Editieren eines Tracks an
      /// </summary>
      /// <param name="track"></param>
      /// <param name="pretxt"></param>
      void showEditInfoText(Track? track, string? pretxt = null) {
         if (track != null) {
            double length = track.Length();
            showEditInfoText((pretxt != null ? pretxt : "") +
                             track.VisualName + " (" +
                             (length < 1000 ? string.Format("{0:F0}m", length) : string.Format("{0:F1}km", length / 1000)) + ")");
         }
      }

      void showTilesInWork(int count) {

         Debug.WriteLine(">>> showTilesInWork(): " + count);

         if (count <= 0) {
            mainMenu.BackgroundColor = MainMenuBackcolorStd;
            TilesInWork.IsVisible = false;
         } else {
            if (mainMenu.BackgroundColor != MainMenuBackcolorEdit)
               mainMenu.BackgroundColor = MainMenuBackcolorEdit;
            TilesInWork.IsVisible = true;
            TilesInWork.Text = count.ToString();
         }
      }

      /// <summary>
      /// "Callback"
      /// </summary>
      /// <param name="filecount"></param>
      /// <param name="filefound"></param>
      /// <param name="found"></param>
      /// <param name="filename"></param>
      void showGpxSearchInfo(int filecount, int filefound, bool found, string filename) =>
         MainThread.BeginInvokeOnMainThread(() => showGpxSearchInfo(filefound, filecount));

      void showGpxSearchInfo(int count, int all) {
         if (count < 0) {
            GpxSearch.IsVisible = GpxSearchCount.IsVisible = false;
         } else {
            GpxSearch.IsVisible = GpxSearchCount.IsVisible = true;
            GpxSearchCount.Text = count + "/" + all;
         }
      }





      async Task deleteCache(int mapidx = -1) {
         if (init != null && init.IsOnInitMap)
            return;
         bool delete = false;
         if (mapidx >= 0) {
            if (await UIHelper.ShowYesNoQuestion_RealYes(
                           this,
                           "Soll der Cache für '" +
                              map.M_ProviderDefinitions[mapidx].MapName +
                              "' wirklich gelöscht werden?" +
                              Environment.NewLine +
                              Environment.NewLine +
                              "Das Löschen kann einige Zeit in Anspruch nehmen.",
                           "Achtung")) {
               delete = true;
            }
         } else {
            if (await UIHelper.ShowYesNoQuestion_RealYes(
                           this,
                           "Soll der Cache wirklich für ALLE Karten gelöscht werden?" +
                              Environment.NewLine +
                              Environment.NewLine +
                              "Das Löschen kann einige Zeit in Anspruch nehmen.",
                           "Achtung")) {
               delete = true;
            }
         }
         if (delete) {
            await showMainpageBusy();
            map.M_ClearMemoryCache();
            int tiles = await map.M_ClearCacheAsync(mapidx);
            IsBusy = false;
            if (0 <= tiles) {
               await Helper2.ShowInfoMessage(this, tiles + " Kartenteile gelöscht", "Cache gelöscht");
               map.M_Refresh(true, false, false, false);
            }
         }
      }

      async Task showMainpageBusy() {
         IsBusy = true;
         await Task.Delay(10);      // "Trick": der ActivityIndicator erscheint schneller
      }


      private void buttonTestA_Clicked(object sender, EventArgs e) {
         //map_OnExceptionThrown(new Exception("Test"));

         //startMapSettingsPage();

         //map.Map_Zoom -= .5;

         //geoLocation?.StartGeoLocationService();

         //ToolbarItem_Config_Clicked(null, EventArgs.Empty);
      }

      private void buttonTestB_Clicked(object sender, EventArgs e) {
         //map.Map_Zoom += .5;

         //geoLocation?.StopGeoLocationService();
      }



      async private void buttonTestC_Clicked(object? sender, EventArgs e) {
#if DEBUG

         MapProviderDefinition? mpd = map.M_ProviderDefinitions[2];
         if (mpd != null) {
            await showMainpageBusy();
            ConfigEdit.EditMapDefPage page = new ConfigEdit.EditMapDefPage(mpd, false);
            page.Disappearing += (s, ea) => {
               if (page.Ok) {

               }
            };
            await FSofTUtils.OSInterface.Helper.GoTo(page);
            IsBusy = false;
         }





         //try {
         //   Random r = new Random();

         //   const int STEPS = 100;
         //   double scale = 1;
         //   Point originstart = new Point(0.2, 0.2);
         //   Point originend = new Point(0.8, 0.8);
         //   MapCtrl mymap = map;

         //   await Task.Run(async () => {

         //      double lon = map.M_CenterLon;
         //      double lat = map.M_CenterLat;
         //      double orgzoom = mymap.M_Zoom;
         //      double lastzoom = orgzoom;

         //      PinchGestureRecognizer_PinchUpdated(mymap, new PinchGestureUpdatedEventArgs(GestureStatus.Started, scale, originstart));
         //      for (int i = 0; i < STEPS; i++) {
         //         double newzoom = orgzoom - 5 * (2 * r.NextDouble() - 1);
         //         lastzoom = scale = newzoom / mymap.M_Zoom;

         //         double x = (originend.X - originstart.X) * r.NextDouble();
         //         double y = (originend.Y - originstart.Y) * r.NextDouble();

         //         PinchGestureRecognizer_PinchUpdated(mymap, new PinchGestureUpdatedEventArgs(GestureStatus.Running,
         //                                                                                    scale,
         //                                                                                    new Point(x, y)));
         //         Thread.Sleep(100);

         //      }
         //      PinchGestureRecognizer_PinchUpdated(mymap, new PinchGestureUpdatedEventArgs(GestureStatus.Completed,
         //                                                                                  orgzoom / lastzoom,
         //                                                                                  originstart));

         //      Thread.Sleep(1000);
         //      await mymap.M_SetLocationAndZoomAsync(orgzoom, lon, lat);

         //   });


         //} catch (Exception ex) {
         //   await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         //}
#endif
      }
   }

}
