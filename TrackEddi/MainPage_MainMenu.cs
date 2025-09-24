using TrackEddi.Common;

namespace TrackEddi {
   public partial class MainPage : ContentPage {

      // Reaktionen auf das Hauptmenü

      /// <summary>
      /// Menüaussehen an Programmstatus anpassen
      /// </summary>
      /// <param name="progState"></param>
      void fitMainMenu(ProgState progState) {
         if (progState.ProgramState == ProgState.State.Viewer) {
            EditButtonsGroup.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
            ExtEditButtons.IsVisible = false;
         } else {
            EditButtonsGroup.BackgroundColor = Color.FromRgba(1, .8, .8, 1);
            ExtEditButtons.IsVisible = true;
         }

         switch (progState.ProgramState) {
            case ProgState.State.Viewer:
            default:
               centerTargetIsVisible = false;
               Helper2.ShowImageButton(ButtonEditTarget, false);
               Helper2.ShowImageButton(ButtonEditMinus, false);
               Helper2.ShowImageButton(ButtonEditEnd, false);
               Helper2.ShowImageButton(ButtonEditCancel, false);
               break;

            case ProgState.State.Edit_Marker:
               centerTargetIsVisible = true;
               Helper2.ShowImageButton(ButtonEditTarget);
               Helper2.ShowImageButton(ButtonEditMinus, false);
               Helper2.ShowImageButton(ButtonEditEnd, false);
               Helper2.ShowImageButton(ButtonEditCancel);
               break;

            case ProgState.State.Edit_TrackDraw:
               ExtEditButtons.IsVisible = true;
               centerTargetIsVisible = true;
               Helper2.ShowImageButton(ButtonEditTarget);
               Helper2.ShowImageButton(ButtonEditMinus);
               Helper2.ShowImageButton(ButtonEditEnd);
               Helper2.ShowImageButton(ButtonEditCancel);
               break;

            case ProgState.State.Edit_TrackPointremove:
               ExtEditButtons.IsVisible = true;
               centerTargetIsVisible = true;
               Helper2.ShowImageButton(ButtonEditTarget);
               Helper2.ShowImageButton(ButtonEditEnd);
               Helper2.ShowImageButton(ButtonEditCancel);
               break;

            case ProgState.State.Edit_TrackSplit:
               ExtEditButtons.IsVisible = true;
               centerTargetIsVisible = true;
               Helper2.ShowImageButton(ButtonEditTarget);
               Helper2.ShowImageButton(ButtonEditMinus, false);
               Helper2.ShowImageButton(ButtonEditEnd);
               Helper2.ShowImageButton(ButtonEditCancel);
               break;

            case ProgState.State.Edit_TrackConcat:
               ExtEditButtons.IsVisible = true;
               centerTargetIsVisible = true;
               Helper2.ShowImageButton(ButtonEditTarget);
               Helper2.ShowImageButton(ButtonEditMinus, false);
               Helper2.ShowImageButton(ButtonEditEnd);
               Helper2.ShowImageButton(ButtonEditCancel);
               break;
         }
      }

      #region Hauptmenübuttons

      private void ZoomIn_Clicked(object? sender, EventArgs e) {
         double zoom = map.M_Zoom;
         if (zoom % 1.0 == 0)
            zoom++;
         else
            zoom = Math.Ceiling(zoom);     // auf "ganzzahlig einrasten"
         Task.Run(async () => await map.M_SetZoomAsync(zoom));
      }

      private void ZoomOut_Clicked(object? sender, EventArgs e) {
         double zoom = map.M_Zoom;
         if (zoom % 1.0 == 0)
            zoom--;
         else
            zoom = Math.Floor(zoom);       // auf "ganzzahlig einrasten"
         Task.Run(async () => await map.M_SetZoomAsync(zoom));
      }

      private void ChooseMap_Clicked(object? sender, EventArgs e) => ToolbarItem_ChooseMap_Clicked(sender, e);

      private void TrackMarkerList_Clicked(object? sender, EventArgs e) => ToolbarItem_GpxContent_Clicked(sender, e);

      /// <summary>
      /// schaltet zum nächsten Button-Modus
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Tap_Clicked(object sender, EventArgs e) {
         if (sender != null && sender is ImageButton) {
            ImageButton? srcbtn = sender as ImageButton;
            ButtonTap1.IsVisible = false;
            ButtonTap2.IsVisible = false;
            ButtonTap3.IsVisible = false;

            if (ButtonTap1 == srcbtn) {

               ButtonTap2.IsVisible = true;
               TapType = TappingType.Mapinfo;

            } else if (ButtonTap2 == srcbtn) {

               if (progState != null)
                  switch (progState.ProgramState) {
                     case ProgState.State.Edit_Marker:
                     case ProgState.State.Edit_TrackDraw:
                        //case ProgState.State.Edit_TrackConcat:
                        //case ProgState.State.Edit_TrackSplit:
                        ButtonTap3.IsVisible = true;
                        TapType = TappingType.DeleteObjects;
                        break;

                     default:
                        ButtonTap1.IsVisible = true;
                        TapType = TappingType.Standard;
                        break;
                  }

            } else if (ButtonTap3 == srcbtn) {

               ButtonTap1.IsVisible = true;
               TapType = TappingType.Standard;

            }
         }
      }

      private async void GeoLocationStart_Clicked(object? sender, EventArgs e) {
         if (config != null &&
             geoLocation != null &&
             !geoLocation.LocationIsShowing &&
             geoLocation.StartGeoLocationService(config.LocationUpdateIntervall, config.LocationUpdateDistance)) {
            ButtonGeoLocationStart.IsVisible =
            ButtonGeoLocationWithoutCenter.IsVisible = false;
            ButtonGeoLocationStop.IsVisible = !ButtonGeoLocationStart.IsVisible;
            geoLocation.LocationIsShowing = true;
            geoLocation.LocationSelfCentering = true;

            await checkLocationService();
         }
      }

      private void GeoLocationStop_Clicked(object? sender, EventArgs e) {
         if (geoLocation != null &&
             geoLocation.LocationIsShowing &&
             !IsOnTracking) {
            ButtonGeoLocationStart.IsVisible = true;
            ButtonGeoLocationWithoutCenter.IsVisible = false;
            ButtonGeoLocationStop.IsVisible = !ButtonGeoLocationStart.IsVisible;
            geoLocation.LocationIsShowing = false;
            geoLocation.LocationSelfCentering = false;
            if (!geoLocation.LocationTracking)
               geoLocation.StopGeoLocationService();
         }
      }

      private void GeoLocationWithoutCenter_Clicked(object? sender, EventArgs e) {
         if (geoLocation != null &&
             geoLocation.LocationIsShowing &&
             !geoLocation.LocationSelfCentering) {
            ButtonGeoLocationStart.IsVisible =
            ButtonGeoLocationWithoutCenter.IsVisible = false;
            ButtonGeoLocationStop.IsVisible = true;
            geoLocation.LocationIsShowing = true;
            geoLocation.LocationSelfCentering = true;
         }
      }

      private async void TrackingStart_Clicked(object? sender, EventArgs e) {
         bool rejectcancel = false;
         if (progState != null) {
            switch (progState.ProgramState) {
               case ProgState.State.Edit_TrackDraw:
               case ProgState.State.Edit_TrackPointremove:
               case ProgState.State.Edit_TrackSplit:
               case ProgState.State.Edit_TrackConcat:
                  rejectcancel = await endEditCancelRejectAsync();
                  if (!rejectcancel)
                     await pushEditModeButton(ProgState.State.Viewer);
                  break;
            }

            if (!rejectcancel) {    // sonst läuft noch eine Editieraktion, die NICHT abgebrochen werden soll
               if (gpxWorkbench != null &&
                   config != null &&
                   geoLocation != null &&
                   !geoLocation.LocationTracking &&
                   geoLocation.StartGeoLocationService(config.LocationUpdateIntervall, config.LocationUpdateDistance)) {
                  ButtonTrackingStart.IsVisible = false;
                  ButtonTrackingStop.IsVisible = !ButtonTrackingStart.IsVisible;
                  geoLocation.StartTracking(gpxWorkbench.Gpx,
                                            config.TrackingMinimalPointdistance,
                                            config.TrackingMinimalHeightdistance,
                                            null);

                  await checkLocationService();
               }
            }
         }
      }

      private async void TrackingStop_Clicked(object? sender, EventArgs e) {
         if (IsOnTracking) {

            switch (await FSofTUtils.OSInterface.Helper.MessageBox(
                                 this,
                                 "Soll die Trackaufzeichnung ...",
                                 [
                                    "... beendet werden?",
                                    "... pausieren?",
                                 ],
                                 "zurück",
                                 null)) {
               case 0:
                  ButtonTrackingStart.IsVisible = true;
                  ButtonTrackingPause.IsVisible = 
                  ButtonTrackingStop.IsVisible = false;
                  if (geoLocation != null) {
                     geoLocation.EndTracking();
                     if (!geoLocation.LocationIsShowing)
                        geoLocation.StopGeoLocationService();
                  }
                  break;

               case 1:
                  ButtonTrackingPause.IsVisible = true;
                  ButtonTrackingStart.IsVisible =
                  ButtonTrackingStop.IsVisible = false;
                  if (geoLocation != null)
                     geoLocation.PausingTracking(true);
                  break;
            }
         }
      }

      private async void TrackingPause_Clicked(object? sender, EventArgs e) {
         if (IsOnTracking) {

            switch (await FSofTUtils.OSInterface.Helper.MessageBox(
                                 this,
                                 "Soll die Trackaufzeichnung ...",
                                 [
                                    "... fortgesetzt werden?",
                                    "... beendet werden?",
                                 ],
                                 "zurück",
                                 null)) {
               case 0:
                  ButtonTrackingPause.IsVisible = 
                  ButtonTrackingStart.IsVisible = false;
                  ButtonTrackingStop.IsVisible = true;
                  if (geoLocation != null)
                     geoLocation.PausingTracking(false);
                  break;

               case 1:
                  ButtonTrackingStart.IsVisible = true;
                  ButtonTrackingPause.IsVisible =
                  ButtonTrackingStop.IsVisible = false;
                  if (geoLocation != null) {
                     geoLocation.EndTracking();
                     if (!geoLocation.LocationIsShowing)
                        geoLocation.StopGeoLocationService();
                  }
                  break;
            }
         }
      }

      #endregion

      #region "Radiobutton" Editiermodus

      async void EditModeNothing_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.State.Viewer);

      async void EditModeMarkerSet_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.State.Edit_Marker);

      async void EditModeTrackDraw_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.State.Edit_TrackDraw);

      async void EditModeTrackPointremove_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.State.Edit_TrackPointremove);

      async void EditModeTrackSplit_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.State.Edit_TrackSplit);

      async void EditModeTrackConcat_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.State.Edit_TrackConcat);

      /// <summary>
      /// einer der Editier-Buttons wurde betätigt
      /// </summary>
      /// <param name="newProgState"></param>
      /// <returns></returns>
      async Task pushEditModeButton(ProgState.State newProgState) {
         if (progState != null &&
             progState.ProgramState != newProgState) {
            //if (!firstOnAppearing ||                       // ohne Rückfrage
            //    await endEditCancelAsync()) {

            if (IsOnTracking && (
                  newProgState == ProgState.State.Edit_TrackDraw ||
                  newProgState == ProgState.State.Edit_TrackPointremove ||
                  newProgState == ProgState.State.Edit_TrackSplit ||
                  newProgState == ProgState.State.Edit_TrackConcat))
               return;

            bool rejectcancel = false;
            if (init != null && !init.IsRunning)
               rejectcancel = await endEditCancelRejectAsync();

            if (!rejectcancel) {    // sonst läuft noch eine Editieraktion, die NICHT abgebrochen werden soll
               showEditInfoText();

               // "gedrückte Buttons" ausschalten, Standardbuttons "einschalten"
               if (EditModeNothingPressed.IsVisible) {
                  EditModeNothing.IsVisible = true;
                  EditModeNothingPressed.IsVisible = false;
               }
               if (EditModeMarkerSetPressed.IsVisible) {
                  EditModeMarkerSet.IsVisible = true;
                  EditModeMarkerSetPressed.IsVisible = false;
               }
               if (EditModeTrackDrawPressed.IsVisible) {
                  EditModeTrackDraw.IsVisible = true;
                  EditModeTrackDrawPressed.IsVisible = false;
               }
               if (EditModeTrackPointremovePressed.IsVisible) {
                  EditModeTrackPointremove.IsVisible = true;
                  EditModeTrackPointremovePressed.IsVisible = false;
               }
               if (EditModeTrackSplitPressed.IsVisible) {
                  EditModeTrackSplit.IsVisible = true;
                  EditModeTrackSplitPressed.IsVisible = false;
               }
               if (EditModeTrackConcatPressed.IsVisible) {
                  EditModeTrackConcat.IsVisible = true;
                  EditModeTrackConcatPressed.IsVisible = false;
               }

               ImageButton? ib = null;
               ImageButton? ibpressed = null;
               string modetxt = "";
               switch (newProgState) {
                  case ProgState.State.Viewer:
                     ib = EditModeNothing;
                     ibpressed = EditModeNothingPressed;
                     modetxt = "Standardansicht";
                     break;
                  case ProgState.State.Edit_Marker:
                     ib = EditModeMarkerSet;
                     ibpressed = EditModeMarkerSetPressed;
                     modetxt = "Marker setzen/verschieben";
                     break;
                  case ProgState.State.Edit_TrackDraw:
                     ib = EditModeTrackDraw;
                     ibpressed = EditModeTrackDrawPressed;
                     modetxt = "Track zeichnen";
                     break;
                  case ProgState.State.Edit_TrackPointremove:
                     ib = EditModeTrackPointremove;
                     ibpressed = EditModeTrackPointremovePressed;
                     modetxt = "Trackpunkte löschen";
                     break;
                  case ProgState.State.Edit_TrackSplit:
                     ib = EditModeTrackSplit;
                     ibpressed = EditModeTrackSplitPressed;
                     modetxt = "Track trennen";
                     break;
                  case ProgState.State.Edit_TrackConcat:
                     ib = EditModeTrackConcat;
                     ibpressed = EditModeTrackConcatPressed;
                     modetxt = "Tracks verbinden";
                     break;
               }

               if (ib != null)
                  ib.IsVisible = false;
               if (ibpressed != null)
                  ibpressed.IsVisible = true;
               EditModeText.Text = modetxt;

               progState.ProgramState = newProgState;

               switch (progState.ProgramState) {
                  case ProgState.State.Edit_TrackSplit:
                  case ProgState.State.Edit_TrackConcat:
                  case ProgState.State.Edit_TrackDraw:
                  case ProgState.State.Edit_TrackPointremove:
                  case ProgState.State.Edit_Marker:
                     if (!ButtonTap1.IsVisible)                      // erstmal in den Standardmodus schalten
                        Tap_Clicked(ButtonTap3, EventArgs.Empty);
                     break;

                  case ProgState.State.Viewer:
                     if (ButtonTap3.IsVisible)
                        Tap_Clicked(ButtonTap3, EventArgs.Empty);
                     break;
               }

               fitMainMenu(progState);
            }
         }
      }

      /// <summary>
      /// akt. Editieraktion ev. mit Abbruch beenden (oder weiterlaufen lassen)
      /// </summary>
      async Task<bool> endEditCancelRejectAsync() {
         if (gpxWorkbench != null && gpxWorkbench.InWork)
            return !await Cancel();   // InWork aber NICHT abgebrochen -> true
         return false;
      }

      /// <summary>
      /// Wenn eine Editieraktion läuft, wird gefragt ob diese abgebrochen werden soll.
      /// </summary>
      /// <returns>true wenn eine Editieraktion abgebrochen wurde</returns>
      async Task<bool> Cancel() {
         bool canceled = false;
         if (gpxWorkbench != null)
            if (gpxWorkbench.MarkerIsInWork) {
               if (await UIHelper.ShowYesNoQuestion_RealYes(this,
                                                            "Abbrechen?",
                                                            "Marker setzen/bearbeiten")) {
                  gpxWorkbench.MarkerEndEdit(ClientMapCenter, true);
                  canceled = true;
               }
            } else if (gpxWorkbench.TrackIsInWork) {
               if (await UIHelper.ShowYesNoQuestion_RealYes(this,
                                                            "Abbrechen?",
                                                            "Track bearbeiten")) {
                  if (gpxWorkbench.TrackInEdit != null)
                     gpxWorkbench.TrackEndEdit(true);
                  canceled = true;
               }
            }
         return canceled;
      }

      /// <summary>
      /// akt. Editieraktion beenden mit OK
      /// </summary>
      /// <returns></returns>
      async Task endEditOKAsync() {
         if (progState != null) {
            switch (progState.ProgramState) {
               case ProgState.State.Viewer: break;
               case ProgState.State.Edit_Marker: gpxWorkbench?.MarkerEndEdit(ClientMapCenter, false); break;
               case ProgState.State.Edit_TrackDraw: gpxWorkbench?.TrackEndEdit(false); break;
               case ProgState.State.Edit_TrackPointremove: gpxWorkbench?.TrackEndEdit(false); break;
               case ProgState.State.Edit_TrackSplit: gpxWorkbench?.TrackEndEdit(ClientMapCenter, false); break;
               case ProgState.State.Edit_TrackConcat:
                  if (MarkedTrack4Concat != null) {
                     gpxWorkbench?.TrackEndEdit(MarkedTrack4Concat, false);
                     MarkedTrack4Concat = null;
                  }
                  break;
            }
            await pushEditModeButton(ProgState.State.Viewer);
         }
      }

      #endregion

      #region Editierbuttons

      /// <summary>
      /// "Ziel"-Button für Track: unterschiedliche Funktion je nach Editierfunktion
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private async void ButtonEditTarget_Clicked(object sender, EventArgs e) => await mapTapped(mauiMapCenter, false);

      /// <summary>
      /// letzten Trackpunkt entfernen
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ButtonEditMinus_Clicked(object sender, EventArgs e) {
         if (progState != null &&
             progState.ProgramState == ProgState.State.Edit_TrackDraw) {
            gpxWorkbench?.TrackRemoveLastPoint();
            showEditInfoText(gpxWorkbench?.TrackInEdit);
         }
      }

      /// <summary>
      /// Ende einer Editierfunktion
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private async void ButtonEditEnd_Clicked(object sender, EventArgs e) => await endEditOKAsync();

      /// <summary>
      /// Abbruch einer Editierfunktion
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private async void ButtonEditCancel_Clicked(object sender, EventArgs e) => await pushEditModeButton(ProgState.State.Viewer);

      #endregion

      async Task checkLocationService() {
         if (geoLocation != null) {
            bool isStarted = geoLocation.LocationServiceIsStarted;
            if (!isStarted) {
               await Task.Run(() => {
                  for (int i = 0; i < 20; i++) {
                     Task.Delay(1000);
                     if (geoLocation.LocationServiceIsStarted) {
                        isStarted = true;
                        break;
                     }
                  }
               });

               if (!isStarted) {
                  GeoLocationStop_Clicked(null, EventArgs.Empty);
                  TrackingStop_Clicked(null, EventArgs.Empty);
               }
            }
         }
      }

   }
}
