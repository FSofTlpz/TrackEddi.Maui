//#define WORKSPACEDIRECTSAVE      // nach JEDER Änderung wird der Workspace sofort gespeichert (sonst erst beim Disappearing)

using FSofTUtils.Geography.Garmin;
using FSofTUtils.Geometry;
using SpecialMapCtrl;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using TrackEddi.Common;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class WorkbenchContentPage : ContentPage {


      /// <summary>
      /// alle akt. GPX-Daten
      /// </summary>
      readonly GpxData gpx;

      /// <summary>
      /// Map-Control
      /// </summary>
      readonly SpecialMapCtrl.SpecialMapCtrl map;

      readonly GpxWorkbench gpxWorkbench;

      /// <summary>
      /// Garmin-Symbole
      /// </summary>
      List<GarminSymbol> garminMarkerSymbols;

      readonly AppData appData;

      /// <summary>
      /// Standardeigenschaft überschreiben, damit <see cref="IsBusy"/> "von außen" verwendet werden kann
      /// </summary>
      new bool IsBusy {
         get => BusyIndicator.IsRunning;
         set => BusyIndicator.IsRunning = value;
      }

      /// <summary>
      /// TabPage Defs
      /// </summary>
      public ObservableCollection<WorkbenchContentPage_TabPageItem> TabPageDef { get; set; } =
         new ObservableCollection<WorkbenchContentPage_TabPageItem>{
                  new WorkbenchContentPage_TabPageItem() { HeaderName = "Tracks", Id = "1", },
                  new WorkbenchContentPage_TabPageItem() { HeaderName = "Marker", Id = "2", }
      };

      /// <summary>
      /// Toolbaritems für die Track-Tabpage
      /// </summary>
      readonly List<ToolbarItem> tbi4Tracks = new List<ToolbarItem>();

      /// <summary>
      /// Toolbaritems für die Marker-Tabpage
      /// </summary>
      readonly List<ToolbarItem> tbi4Markers = new List<ToolbarItem>();

      /// <summary>
      /// Itemliste für die Tracks
      /// </summary>
      readonly ObservableCollection<WorkbenchContentPage_ListViewObjectItem> tracklst;

      /// <summary>
      /// Itemliste für die Marker
      /// </summary>
      readonly ObservableCollection<WorkbenchContentPage_ListViewObjectItem> markerlst;

      bool firstOnAppearing = true;



      public WorkbenchContentPage(SpecialMapCtrl.SpecialMapCtrl map,
                                  GpxWorkbench gpxWorkbench,
                                  List<GarminSymbol> garminMarkerSymbols,
                                  AppData appdata) {
         InitializeComponent();

         BindingContext = this;

         this.map = map;
         this.gpxWorkbench = gpxWorkbench;
         this.gpx = gpxWorkbench.Gpx;
         this.garminMarkerSymbols = garminMarkerSymbols;
         appData = appdata;

         tracklst = new ObservableCollection<WorkbenchContentPage_ListViewObjectItem>();
         markerlst = new ObservableCollection<WorkbenchContentPage_ListViewObjectItem>();

         tbi4Tracks.Add(tbiTracksViewAll);
         tbi4Tracks.Add(tbiTracksHideAll);
         tbi4Tracks.Add(tbiTracksDelete);

         tbi4Markers.Add(tbiMarkersViewAll);
         tbi4Markers.Add(tbiMarkersHideAll);
         tbi4Markers.Add(tbiMarkersDelete);
      }

      T getItem<T>(object sender) {
         if (sender is ImageButton)
            return (T)((ImageButton)sender).CommandParameter;

         if (sender is Button)
            return (T)((Button)sender).CommandParameter;

         throw new Exception(nameof(WorkbenchContentPage) + "." +
                             nameof(getItem) +
                             "(): Falscher Parametertyp: " +
                             sender.GetType().Name);
      }

      T getItem<T>(TappedEventArgs e) {
         if (e.Parameter != null)
            if (e.Parameter is T)
               return (T)e.Parameter;

         throw new Exception(nameof(WorkbenchContentPage) + "." +
                             nameof(getItem) +
                             "(): Falscher Parametertyp: " +
                             e.Parameter?.GetType().Name);
      }

      private void TrackColorClicked(object sender, EventArgs e) =>
         onTrackColor(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void TrackZoomClicked(object sender, EventArgs e) =>
         onTrackZoom(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void TrackTurnBackClicked(object sender, EventArgs e) =>
         onTrackTurnBack(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void TrackSimplClicked(object sender, EventArgs e) =>
         onTrackSimpl(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void TrackDeleteClicked(object sender, EventArgs e) =>
         onTrackDelete(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void TrackMoveDownClicked(object sender, EventArgs e) =>
         onTrackMoveDown(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void TrackMoveUpClicked(object sender, EventArgs e) =>
         onTrackMoveUp(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void MarkerSymbolClicked(object sender, EventArgs e) =>
         onMarkerSymbol(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void MarkerZoomClicked(object sender, EventArgs e) =>
         onMarkerZoom(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void MarkerDeleteClicked(object sender, EventArgs e) =>
         onMarkerDelete(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void MarkerMoveDownClicked(object sender, EventArgs e) =>
         onMarkerMoveDown(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));
      private void MarkerMoveUpClicked(object sender, EventArgs e) =>
         onMarkerMoveUp(getItem<WorkbenchContentPage_ListViewObjectItem>(sender));

      private void TrackPropsTapped(object sender, TappedEventArgs e) =>
         onTrackProps(getItem<WorkbenchContentPage_ListViewObjectItem>(e));
      private void MarkerPropsTapped(object sender, TappedEventArgs e) =>
         onMarkerProps(getItem<WorkbenchContentPage_ListViewObjectItem>(e));
      //private void MarkerPropsClicked(object sender, EventArgs e) => onMarkerProps(getItem(sender));


      protected override void OnAppearing() {
         base.OnAppearing();
         if (firstOnAppearing) {
            firstOnAppearing = false;
            activateTabPage("1");         // die Listen sind noch NICHT gefüllt
         }
      }

      void activateTabPage(string id) {
         RadioButton? rb = null;
         foreach (var child in StackLayout4Tabs.GetVisualTreeDescendants()) { // ALLE untergeordneten Childs werden geliefert (vermutlich in der Reihenfolge der internen rekursiven Suche?)
            if (child is RadioButton) {
               RadioButton? test = child as RadioButton;
               if (test != null && id == test.Value.ToString()) {
                  rb = test;
                  break;
               }
            }
         }
         if (rb != null) {
            rb.IsChecked = true;


         }

         setToolbarItems(id == TabPageDef[0].Id);
      }

      private async void TabBar_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         if (sender != null &&
             sender is RadioButton) {
            RadioButton? rb = (RadioButton)sender;
            if (rb != null && rb.IsChecked) {
               await showPageBusy();

               PseudoPage1.IsVisible =
               PseudoPage2.IsVisible = false;

               string? id = rb.Value.ToString();
               if (id != null) {
                  if (id == "1") {
                     PseudoPage1.IsVisible = true;
                  } else if (id == "2") {
                     PseudoPage2.IsVisible = true;
                  }
               }

               setToolbarItems(PseudoPage1.IsVisible);

               IsBusy = false;
            }
         }
      }

      /* Die Funktion läuft schnell "durch" aber danach dauert es noch eine längere Zeit bevor das Listview auf dem Bildschirm
       * erscheint.
       * Dadurch läuft auch das PageBusy ins Leere.
       */

      public async Task ActualizeContent() {
         await showPageBusy();

         ListViewTracks.ItemsSource = null;
         ListViewMarker.ItemsSource = null;

         tracklst.Clear();
         markerlst.Clear();

         await Task.Run(() => {
            for (int i = 0; i < gpx.TrackList.Count; i++) {
               WorkbenchContentPage_ListViewObjectItem item = new WorkbenchContentPage_ListViewObjectItem(gpx.TrackList[i]);
               item.PropertyChanged += Tracklist_PropertyChanged;
               tracklst.Add(item);
            }

            for (int i = 0; i < gpx.MarkerList.Count; i++) {
               WorkbenchContentPage_ListViewObjectItem item = new WorkbenchContentPage_ListViewObjectItem(gpx.MarkerList[i]);
               item.PropertyChanged += Marker_PropertyChanged;
               markerlst.Add(item);
            }
         });

         setContentPageTitels();

         ListViewTracks.ItemsSource = tracklst;
         ListViewMarker.ItemsSource = markerlst;

         IsBusy = false;
      }

      protected override async void OnDisappearing() {
         base.OnDisappearing();
         if (gpxWorkbench.DataChanged) {
            await showPageBusy();
            appData?.Save();
            await gpxWorkbench.SaveAsync(true);
            IsBusy = false;
         }
      }

      /// <summary>
      /// spez. Behandlung von Track.IsVisible ist nötig
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Tracklist_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
         if (e.PropertyName == nameof(WorkbenchContentPage_ListViewObjectItem.IsVisible)) {
            WorkbenchContentPage_ListViewObjectItem? listViewObjectItem = (WorkbenchContentPage_ListViewObjectItem?)sender;
            if (listViewObjectItem != null) {
               Track? track = listViewObjectItem.Track;
               if (track != null) {
                  map.M_ShowTrack(track,
                                  track.IsVisible,
                                  track.GpxDataContainer?.NextVisibleTrack(track));
                  setContentPageTitels();
               }
            }
         }
      }

      /// <summary>
      /// spez. Behandlung von Marker.IsVisible ist nötig
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Marker_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
         if (e.PropertyName == nameof(WorkbenchContentPage_ListViewObjectItem.IsVisible)) {
            WorkbenchContentPage_ListViewObjectItem? listViewObjectItem = (WorkbenchContentPage_ListViewObjectItem?)sender;
            if (listViewObjectItem != null) {
               Marker? marker = listViewObjectItem.Marker;
               if (marker != null) {
                  map.M_ShowMarker(marker,
                                   marker.IsVisible,
                                   marker.GpxDataContainer?.NextVisibleMarker(marker));
                  setContentPageTitels();
               }
            }
         }
      }

      void setContentPageTitels() {
         int markedmarker = 0, markedtracks = 0;
         for (int i = 0; i < tracklst.Count; i++)
            if (tracklst[i].IsVisible)
               markedtracks++;
         for (int i = 0; i < markerlst.Count; i++)
            if (markerlst[i].IsVisible)
               markedmarker++;

         TabPageDef[0].HeaderName = "Tracks (" + markedtracks + " von " + tracklst.Count + ")";
         TabPageDef[1].HeaderName = "Marker (" + markedmarker + " von " + markerlst.Count + ")";

         //await UIHelper.ShowInfoMessage(this, TabPageDef[0].HeaderName + " / " + TabPageDef[1].HeaderName);

      }

      #region Toolbaritems

      void setToolbarItems(bool fortracks) {
         ToolbarItems.Clear();
         if (fortracks)
            foreach (var item in tbi4Tracks)
               ToolbarItems.Add(item);
         else
            foreach (var item in tbi4Markers)
               ToolbarItems.Add(item);
      }

      private void ToolbarItem_ShowAllTracks_Clicked(object sender, EventArgs e) {
         foreach (var t in tracklst)
            if (!t.IsVisible)
               t.IsVisible = true;
      }

      private void ToolbarItem_HideAllTracks_Clicked(object sender, EventArgs e) {
         foreach (var t in tracklst)
            if (t.IsVisible)
               t.IsVisible = false;
      }

      async private void ToolbarItem_DeleteAllVisibleTracks_Clicked(object sender, EventArgs e) {
         try {
            int count = 0;
            foreach (var t in tracklst)
               if (t.IsVisible)
                  count++;
            if (count > 0) {
               if (await UIHelper.ShowYesNoQuestion_RealYes(this,
                                                            "Sollen wirklich ALLE angezeigten Tracks (" + count + ") gelöscht werden?",
                                                            "Achtung")) {
                  await showPageBusy();

                  await Task.Run(() => { // Arbeit darf wegen Busy NICHT im MainThread erfolgen
                     for (int i = tracklst.Count - 1; i >= 0; i--)
                        if (tracklst[i].IsVisible)
                           trackDelete(tracklst[i]);
#if WORKSPACEDIRECTSAVE
                     gpxWorkbench.Save();
#else
                     gpxWorkbench.DataChanged = true;
#endif
                  });

                  IsBusy = false;
                  setContentPageTitels();
               }
            }
         } catch (Exception ex) {
            await UIHelper.ShowExceptionMessage(this, ex, null, false);
         }
      }

      private void ToolbarItem_ShowAllMarker_Clicked(object sender, EventArgs e) {
         foreach (var wp in markerlst)
            if (!wp.IsVisible)
               wp.IsVisible = true;
      }

      private void ToolbarItem_HideAllMarker_Clicked(object sender, EventArgs e) {
         foreach (var wp in markerlst)
            if (wp.IsVisible)
               wp.IsVisible = false;
      }

      async private void ToolbarItem_DeleteAllVisibleMarker_Clicked(object sender, EventArgs e) {
         try {
            int count = 0;
            foreach (var td in markerlst)
               if (td.IsVisible)
                  count++;
            if (count > 0) {
               if (await UIHelper.ShowYesNoQuestion_RealYes(this,
                                                            "Sollen wirklich ALLE angezeigten Marker (" + count + ") gelöscht werden?",
                                                            "Achtung")) {
                  await showPageBusy();

                  await Task.Run(() => { // Arbeit darf wegen Busy NICHT im MainThread erfolgen
                     for (int i = markerlst.Count - 1; i >= 0; i--)
                        if (markerlst[i].IsVisible)
                           markerDelete(markerlst[i]);
#if WORKSPACEDIRECTSAVE
                     gpxWorkbench.Save();
#else
                     gpxWorkbench.DataChanged = true;
#endif
                  });

                  IsBusy = false;
                  setContentPageTitels();
               }
            }
         } catch (Exception ex) {
            await UIHelper.ShowExceptionMessage(this, ex, null, false);
         }
      }

      #endregion

      #region Track-Bearbeitung

      async void onTrackProps(WorkbenchContentPage_ListViewObjectItem td) {
         try {
            Track? track = td.Track;
            if (track != null && !track.IsOnLiveDraw) {
               Track trackcopy = Track.CreateCopy(track);
               EditTrackPage page = new EditTrackPage(trackcopy);
               page.EndWithOk += (object? sender2, EventArgs e2) => {
                  track.LineColor = trackcopy.LineColor;
                  track.GpxTrack.Name = trackcopy.GpxTrack.Name;
                  track.VisualName = track.GpxTrack.Name;
                  track.GpxTrack.Description = trackcopy.GpxTrack.Description;
                  track.GpxTrack.Comment = trackcopy.GpxTrack.Comment;
                  track.GpxTrack.Source = trackcopy.GpxTrack.Source;
                  td.Notify4PropChanged(nameof(WorkbenchContentPage_ListViewObjectItem.Text1));
#if WORKSPACEDIRECTSAVE
               gpxWorkbench.Save();
#else
                  gpxWorkbench.DataChanged = true;
#endif
               };
               await FSofTUtils.OSInterface.Helper.GoTo(page);
            }
         } catch (Exception ex) {
            await UIHelper.ShowExceptionMessage(this, ex, null, false);
         }
      }

      async void onTrackColor(WorkbenchContentPage_ListViewObjectItem td) {
         try {
            Track? track = td.Track;
            if (track != null && !track.IsOnLiveDraw) {
               ColorChoosingPage page = new ColorChoosingPage() {
                  ActualColor = td.TrackColor,
               };
               page.EndWithOk += (object? sender, EventArgs e) => {
                  td.TrackColor = page.ActualColor;
#if WORKSPACEDIRECTSAVE
               gpxWorkbench.Save();
#else
                  gpxWorkbench.DataChanged = true;
#endif
               };
               await FSofTUtils.OSInterface.Helper.GoTo(page);
            }
         } catch (Exception ex) {
            await UIHelper.ShowExceptionMessage(this, ex, null, false);
         }
      }

      async void onTrackTurnBack(WorkbenchContentPage_ListViewObjectItem td) {
         if (td.Track != null &&
             !td.Track.IsOnLiveDraw &&
             await UIHelper.ShowYesNoQuestion_RealYes(this,
                                                      "Soll die Richtung des Tracks '" + td.Text1 + "' umgekehrt werden?",
                                                      "Achtung")) {
            map.M_ShowTrack(td.Track, false, null);
            td.Track.ChangeDirection();
            map.M_ShowTrack(td.Track, true, null);
#if WORKSPACEDIRECTSAVE
            gpxWorkbench.Save();
#else
            gpxWorkbench.DataChanged = true;
#endif
         }
      }

      async void onTrackSimpl(WorkbenchContentPage_ListViewObjectItem td) {
         if (td.Track != null && !td.Track.IsOnLiveDraw)
            try {
               SimplifyTrackPage page = new SimplifyTrackPage(td.Track, appData);
               page.EndWithOk += (object? sender2, EventArgs e2) => {
                  if (page.NewTrack != null &&
                      page.NewTrack.GpxTrack.Segments[0].Points.Count > 1) {
                     int pos = tracklst.IndexOf(td);
                     Track newtrack = gpx.TrackInsertCopyWithLock(page.NewTrack, pos);      // "über" dem Originaltrack
                     newtrack.Trackname = gpx.GetUniqueTrackname(newtrack.Trackname);
                     newtrack.LineColor = td.Track.LineColor;
                     newtrack.LineWidth = td.Track.LineWidth;

                     map.M_ShowTrack(newtrack, true, pos == 0 ? null : gpx.TrackList[pos - 1]);

                     tracklst.Insert(pos, new WorkbenchContentPage_ListViewObjectItem(newtrack) { IsVisible = true });

                     if (!td.Track.IsVisible)
                        tracklst[pos].IsVisible = false;

                     // Die Anzeige fkt. manchmal im Emulator NICHT. ?????

                     ListViewTracks.ScrollTo(tracklst[pos], ScrollToPosition.Center, true);
#if WORKSPACEDIRECTSAVE
                  gpxWorkbench.Save();
#else
                     gpxWorkbench.DataChanged = true;
#endif
                  }
               };
               await FSofTUtils.OSInterface.Helper.GoTo(page);
            } catch (Exception ex) {
               await UIHelper.ShowExceptionMessage(this, ex, null, false);
            }
      }

      async void onTrackZoom(WorkbenchContentPage_ListViewObjectItem td) => await zoomAsync(td);

      async void onTrackDelete(WorkbenchContentPage_ListViewObjectItem td) => await deleteAsync(td);

      async void onTrackMoveUp(WorkbenchContentPage_ListViewObjectItem td) => await moveAsync(td, false);

      async void onTrackMoveDown(WorkbenchContentPage_ListViewObjectItem td) => await moveAsync(td, true);

      #endregion

      #region Marker-Bearbeitung

      async void onMarkerProps(WorkbenchContentPage_ListViewObjectItem td) {
         if (td.Marker != null) {
            await showPageBusy();
            try {
               EditMarkerPage page = new EditMarkerPage(td.Marker, garminMarkerSymbols);
               page.EndWithOk += (object? sender, EventArgs e) => {
                  td.SetMarkerPicture(page.Marker.Symbolname);      // ev. geändert
                  int idx = gpx.MarkerIndex(page.Marker);
                  if (idx >= 0) {
                     gpx.GpxDataChanged = true;
                     gpx.Waypoints[idx] = page.Marker.Waypoint;
                     gpxWorkbench.RefreshOnMap(page.Marker);
                     td.Notify4PropChanged(nameof(WorkbenchContentPage_ListViewObjectItem.Text1));
                     td.Notify4PropChanged(nameof(WorkbenchContentPage_ListViewObjectItem.Picture));
#if WORKSPACEDIRECTSAVE
                     gpxWorkbench.Save();
#else
                     gpxWorkbench.DataChanged = true;
#endif
                  }
               };
               await FSofTUtils.OSInterface.Helper.GoTo(page);
            } catch (Exception ex) {
               IsBusy = false;
               await UIHelper.ShowExceptionMessage(this, ex, null, false);
            }
            IsBusy = false;
         }
      }

      async void onMarkerSymbol(WorkbenchContentPage_ListViewObjectItem td) {
         if (td.Marker != null) {
            await showPageBusy();
            try {
               SymbolChoosingPage page = new SymbolChoosingPage(garminMarkerSymbols, td.Marker.Waypoint.Symbol);
               page.EndWithOk += (object? sender, EventArgs e) => {
                  if (page.ActualGarminSymbol != null) {
                     td.Marker.Waypoint.Symbol = page.ActualGarminSymbol.Name;
                     td.SetMarkerPicture(page.ActualGarminSymbol.Name);

                     int idx = gpx.MarkerIndex(td.Marker);
                     if (idx >= 0) {
                        gpx.GpxDataChanged = true;
                        gpx.Waypoints[idx] = td.Marker.Waypoint;
                        gpxWorkbench.RefreshOnMap(td.Marker);
#if WORKSPACEDIRECTSAVE
                  gpxWorkbench.Save();
#else
                        gpxWorkbench.DataChanged = true;
#endif
                     }
                  }
               };
               await FSofTUtils.OSInterface.Helper.GoTo(page);
            } catch (Exception ex) {
               IsBusy = false;
               await UIHelper.ShowExceptionMessage(this, ex, null, false);
            }
         }
         IsBusy = false;
      }

      async void onMarkerZoom(WorkbenchContentPage_ListViewObjectItem td) => await zoomAsync(td);

      async void onMarkerDelete(WorkbenchContentPage_ListViewObjectItem td) => await deleteAsync(td);

      async void onMarkerMoveUp(WorkbenchContentPage_ListViewObjectItem td) => await moveAsync(td, false);

      async void onMarkerMoveDown(WorkbenchContentPage_ListViewObjectItem td) => await moveAsync(td, true);

      #endregion

      void trackDelete(WorkbenchContentPage_ListViewObjectItem td) {
         if (td.Track != null && !td.Track.IsOnLiveDraw) {
            gpxWorkbench.TrackRemove(td.Track);
            MainThread.BeginInvokeOnMainThread(() => tracklst.Remove(td));
         }
      }

      void markerDelete(WorkbenchContentPage_ListViewObjectItem td) {
         if (td.Marker != null) {
            gpxWorkbench.MarkerRemove(td.Marker);
            MainThread.BeginInvokeOnMainThread(() => markerlst.Remove(td));
         }
      }

      async Task moveAsync(WorkbenchContentPage_ListViewObjectItem td, bool down) {
         await showPageBusy();

         ObservableCollection<WorkbenchContentPage_ListViewObjectItem> lst = td.IsTrack ? tracklst : markerlst;
         int idx = lst.IndexOf(td);
         ListView lv = td.IsTrack ? ListViewTracks : ListViewMarker;

         if (down) { // Bewegung nach unten: Index in der Liste erhöht sich; aber Zeichnen-Ebene 1 tiefer

            if (0 <= idx && idx < lst.Count - 1) {

               await Task.Run(() => { // Arbeit darf NICHT im MainThread erfolgen
                  bool visible = td.IsVisible;
                  if (td.IsTrack) {
                     gpx.TrackOrderChangeWithLock(idx, idx + 1);
                     refreshTrack(td.Track);
                  } else {
                     gpx.MarkerOrderChangeWithLock(idx, idx + 1);
                     refreshMarker(td.Marker);
                  }
                  lst.RemoveAt(idx);
                  lst.Insert(idx + 1, td);
#if WORKSPACEDIRECTSAVE
                  gpxWorkbench.Save();
#else
                  gpxWorkbench.DataChanged = true;
#endif
               });

               lv.ScrollTo(td, ScrollToPosition.MakeVisible, true);
            }

         } else {

            if (0 < idx) {

               await Task.Run(() => { // Arbeit darf NICHT im MainThread erfolgen
                  if (td.IsTrack) {
                     gpx.TrackOrderChangeWithLock(idx, idx - 1);
                     refreshTrack(td.Track);
                  } else {
                     gpx.MarkerOrderChangeWithLock(idx, idx - 1);
                     refreshMarker(td.Marker);
                  }
                  lst.RemoveAt(idx);
                  lst.Insert(idx - 1, td);
#if WORKSPACEDIRECTSAVE
                  gpxWorkbench.Save();
#else
                  gpxWorkbench.DataChanged = true;
#endif
               });

               lv.ScrollTo(td, ScrollToPosition.MakeVisible, true);
            }

         }
         IsBusy = false;
      }

      async Task deleteAsync(WorkbenchContentPage_ListViewObjectItem td) {
         if (await FSofTUtils.OSInterface.Helper.MessageBox(this,
                                                        "Achtung",
                                                        "Soll der " + (td.IsTrack ? "Track" : "Marker") + " '" + td.Text1 + "' wirklich gelöscht werden?",
                                                        "ja",
                                                        "nein")) {
            await showPageBusy();
            await Task.Run(() => { // Arbeit darf NICHT im MainThread erfolgen
               if (td.IsTrack)
                  trackDelete(td);
               else
                  markerDelete(td);
#if WORKSPACEDIRECTSAVE
               gpxWorkbench.Save();
#else
               gpxWorkbench.DataChanged = true;
#endif
            });

            IsBusy = false;
            setContentPageTitels();
         }
      }

      async Task zoomAsync(WorkbenchContentPage_ListViewObjectItem td) {
         if (td.IsTrack)
#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
            await map.M_ZoomToRangeAsync(new PointD(td.Track.Bounds.MinLon, td.Track.Bounds.MaxLat),
                                         new PointD(td.Track.Bounds.MaxLon, td.Track.Bounds.MinLat),
                                         true);
#pragma warning restore CS8602 // Dereferenzierung eines möglichen Nullverweises.
         else {
            if (td.Marker != null)
               await map.M_SetLocationAndZoomAsync(map.M_Zoom, td.Marker.Longitude, td.Marker.Latitude);
         }
         appData.LastZoom = map.M_Zoom;
         appData.LastLongitude = map.M_CenterLon;
         appData.LastLatitude = map.M_CenterLat;
         await FSofTUtils.OSInterface.Helper.GoBack();
      }


      /// <summary>
      /// Neuanzeige (basiert auf "aus-" und "einschalten") z.B. bei Ebenenwechsel
      /// </summary>
      /// <param name="track"></param>
      void refreshTrack(Track? track) {
         if (track != null && track.IsVisible) {
            map.M_ShowTrack(track, false, null);
            map.M_ShowTrack(track,
                            true,
                            track.GpxDataContainer?.NextVisibleTrack(track));
         }
      }

      /// <summary>
      /// Neuanzeige (basiert auf "aus-" und "einschalten") z.B. bei Ebenenwechsel
      /// </summary>
      /// <param name="marker"></param>
      void refreshMarker(Marker? marker) {
         if (marker != null && marker.IsVisible) {
            map.M_ShowMarker(marker, false, null);
            map.M_ShowMarker(marker,
                            true,
                            marker.GpxDataContainer?.NextVisibleMarker(marker));
         }
      }

      /// <summary>
      /// informiert diese Seite darüber, dass sich der Livetrack verändert hat
      /// </summary>
      public void LiveTrackAppendPoint(Track livetrack) {
         int idx = gpx.TrackIndex(livetrack);
         if (idx >= 0)
            try {
               MainThread.BeginInvokeOnMainThread(() =>
                  tracklst[idx].Notify4PropChanged(nameof(WorkbenchContentPage_ListViewObjectItem.Text2)));
            } catch (Exception ex) {
               string msg = UIHelper.GetExceptionMessage(ex);
               UIHelper.Message2Logfile(nameof(WorkbenchContentPage_ListViewObjectItem.Text2), msg, null);
            }
      }

      async Task showPageBusy() {
         IsBusy = true;
         await Task.Delay(10);      // "Trick": der ActivityIndicator erscheint schneller
      }
   }
}