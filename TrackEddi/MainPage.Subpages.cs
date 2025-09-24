using SpecialMapCtrl;
using System.Diagnostics;
using TrackEddi.Common;
using TrackEddi.Gnss;

namespace TrackEddi {
   public partial class MainPage : ContentPage {

      class Subpages {

         bool isCallingSubpage = false;

         bool isSubPageAllowed =>
                     !isCallingSubpage &&
                     (mainPage.init != null && !mainPage.init.IsOnInitMap) &&
                     mainPage.appData != null &&
                     mainPage.gpxWorkbench != null;

         MainPage mainPage;


         public Subpages(MainPage mainPage) {
            this.mainPage = mainPage;
         }

         async Task showMainpageBusy() {
            mainPage.IsBusy = true;
            await Task.Delay(10);      // "Trick": der ActivityIndicator erscheint schneller
         }

         /// <summary>
         /// Auswahl der Karte
         /// </summary>
         /// <returns></returns>
         async public Task MapChoosing() {
            if (isSubPageAllowed) {
               isCallingSubpage = true;
               int orgidx = -1;

               try {
                  await showMainpageBusy();

                  MapChoosingPage page = new MapChoosingPage() {
                     MapControl = mainPage.map,
                     ProvIdxPaths = mainPage.providxpaths,
                     Config = mainPage.config,
                     AppData = mainPage.appData,
                  };
                  page.MapChoosingEvent += async (s, e) => {
                     if (e.NewIdx >= 0) {
                        orgidx = e.ActualIdx;
                        await setMap4Idx(e.NewIdx);
                     }
                     isCallingSubpage = false;
                  };
                  await FSofTUtils.OSInterface.Helper.GoTo(page);
               } catch (Exception ex) {
                  mainPage.IsBusy = false;
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler bei " + nameof(MapChoosing), ex);

                  if (orgidx >= 0) { // nach Möglichkeit zurücksetzen
                     try {
                        await setMap4Idx(orgidx);
                     } catch { }
                  }
               } finally {    // wird NACH der Beendigung des await (Start der Subpage) ausgeführt
                  mainPage.IsBusy = false;
                  isCallingSubpage = false;
               }
            }
         }

         async Task setMap4Idx(int newidx) {
            mainPage.appData!.LastMapname = mainPage.map.M_ProviderDefinitions[newidx].MapName;
            await mainPage.setProviderZoomPositionAsync(newidx,
                                                        mainPage.map.M_Zoom,
                                                        mainPage.map.M_CenterLon,
                                                        mainPage.map.M_CenterLat);
         }

         /// <summary>
         /// (bearbeitbare) Track- und Markerliste anzeigen
         /// </summary>
         /// <returns></returns>
         async public Task ShowTracklistAndMarkerlist() {
            if (isSubPageAllowed) {
               isCallingSubpage = true;

               try {
                  await showMainpageBusy();
                  WorkbenchContentPage page = new WorkbenchContentPage(mainPage.map,
                                                                       mainPage.gpxWorkbench!,
                                                                       mainPage.garminMarkerSymbols,
                                                                       mainPage.appData!);
                  page.Disappearing += (s, e) => {
                     mainPage.IsBusy = false;         // Die Busy-Anzeige wird trotzdem zu schnell abgeschaltet!!
                  };
                  await FSofTUtils.OSInterface.Helper.GoTo(page);
                  await page.ActualizeContent();
               } catch (Exception ex) {
                  mainPage.IsBusy = false;
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler bei " + nameof(ShowTracklistAndMarkerlist), ex);
               } finally {
                  //mainPage.IsBusy = false;
                  isCallingSubpage = false;
               }
            }
         }

         /// <summary>
         /// geografische Suche per OSM (nur online)
         /// </summary>
         /// <returns></returns>
         async public Task OsmSearch() {
            if (isSubPageAllowed) {
               isCallingSubpage = true;

               try {
                  await showMainpageBusy();
                  OsmSearchPage page = new OsmSearchPage(mainPage.map,
                                                         mainPage.appData!);
                  await FSofTUtils.OSInterface.Helper.GoTo(page);
               } catch (Exception ex) {
                  mainPage.IsBusy = false;
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler bei OSM-Suche", ex);
               } finally {
                  mainPage.IsBusy = false;
                  isCallingSubpage = false;
               }
            }
         }

         async public Task ShowGarminInfo4LonLat(IList<GarminImageCreator.SearchObject> infos, string pretext) {
            if (isSubPageAllowed) {
               isCallingSubpage = true;

               try {
                  await showMainpageBusy();
                  ShowGarminInfo4LonLat page = new ShowGarminInfo4LonLat(infos, pretext);
                  await FSofTUtils.OSInterface.Helper.GoTo(page);
               } catch (Exception ex) {
                  mainPage.IsBusy = false;
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler bei GarminInfo-Anzeige", ex);
               } finally {
                  mainPage.IsBusy = false;
                  isCallingSubpage = false;
               }
            }
         }

         /// <summary>
         /// zu einem "benannten" Ort oder zu geografischen Koordinaten gehen
         /// </summary>
         /// <returns></returns>
         async public Task GoTo() {
            if (isSubPageAllowed) {
               isCallingSubpage = true;

               try {
                  await showMainpageBusy();
                  GoToPage page = new GoToPage(mainPage.map,
                                               mainPage.appData!);
                  await FSofTUtils.OSInterface.Helper.GoTo(page);
               } catch (Exception ex) {
                  mainPage.IsBusy = false;
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler bei " + nameof(GoTo), ex);
               } finally {
                  mainPage.IsBusy = false;
                  isCallingSubpage = false;
               }
            }
         }

         /// <summary>
         /// zeigt die (editierbare) Seite der Marker-Eigenschaften an
         /// </summary>
         /// <param name="marker"></param>
         /// <returns></returns>
         async public Task EditMarkerProps(Marker marker) {
            if (isSubPageAllowed) {
               isCallingSubpage = true;

               try {
                  await showMainpageBusy();
                  EditMarkerPage page = new EditMarkerPage(marker, mainPage.garminMarkerSymbols);
                  page.EndWithOk += (object? sender, EventArgs e) => mainPage.gpxWorkbench?.RefreshMarkerWaypoint(page.Marker);
                  await FSofTUtils.OSInterface.Helper.GoTo(page);
               } catch (Exception ex) {
                  mainPage.IsBusy = false;
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler bei " + nameof(EditMarkerProps), ex);
               } finally {
                  mainPage.IsBusy = false;
                  isCallingSubpage = false;
               }
            }
         }

         /// <summary>
         /// einfache Anzeige der Marker-Eigenschaften
         /// </summary>
         /// <param name="marker"></param>
         /// <returns></returns>
         async public Task ShowShortMarkerProps(Marker marker) => await Helper2.ShowInfoMessage(mainPage, marker.Waypoint.Name, "Marker");

         /// <summary>
         /// zeigt die (editierbare) Seite der Track-Eigenschaften an
         /// </summary>
         /// <param name="track"></param>
         /// <returns></returns>
         async public Task EditTrackProps(Track track) {
            if (isSubPageAllowed) {
               isCallingSubpage = true;

               try {
                  await showMainpageBusy();
                  Track trackcopy = Track.CreateCopy(track);
                  EditTrackPage page = new EditTrackPage(trackcopy);
                  page.EndWithOk += (object? sender2, EventArgs e2) => {
                     track.LineColor = trackcopy.LineColor;
                     track.GpxTrack.Name = trackcopy.GpxTrack.Name;
                     track.GpxTrack.Description = trackcopy.GpxTrack.Description;
                     track.GpxTrack.Comment = trackcopy.GpxTrack.Comment;
                     track.GpxTrack.Source = trackcopy.GpxTrack.Source;
                  };
                  await FSofTUtils.OSInterface.Helper.GoTo(page);
               } catch (Exception ex) {
                  mainPage.IsBusy = false;
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler bei " + nameof(EditTrackProps), ex);
               } finally {
                  mainPage.IsBusy = false;
                  isCallingSubpage = false;
               }
            }
         }

         async public Task ShowGeoLocation() {
            if (isSubPageAllowed && mainPage.geoLocation != null) {
               isCallingSubpage = true;

               try {
                  await showMainpageBusy();
                  GeoLocationPage page = new GeoLocationPage(mainPage.geoLocation);
                  await FSofTUtils.OSInterface.Helper.GoTo(page);
               } catch (Exception ex) {
                  mainPage.IsBusy = false;
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler bei " + nameof(ShowGeoLocation), ex);
               } finally {
                  mainPage.IsBusy = false;
                  isCallingSubpage = false;
               }
            }
         }

         async public Task ShowGNSS() {
            if (isSubPageAllowed &&
                mainPage.geoLocation != null &&
                mainPage.geoLocation.LocationServiceIsActiv &&
                mainPage.gnssData != null) {
               isCallingSubpage = true;

               try {
                  await showMainpageBusy();
                  GNNSPage page = new GNNSPage(mainPage.gnssData);
                  await FSofTUtils.OSInterface.Helper.GoTo(page);
               } catch (Exception ex) {
                  mainPage.IsBusy = false;
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler bei " + nameof(ShowGNSS), ex);
               } finally {
                  mainPage.IsBusy = false;
                  isCallingSubpage = false;
               }
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <returns></returns>
         async public Task ShowCacheManagementPage() {
            if (isSubPageAllowed) {
               isCallingSubpage = true;

               try {
                  await showMainpageBusy();
                  CacheManagePage page = new CacheManagePage(mainPage.map, mainPage.map.M_ActualMapIdx);
                  page.Disappearing += (s, e) => {
                     mainPage.IsBusy = false;         // Die Busy-Anzeige wird trotzdem zu schnell abgeschaltet!!
                  };
                  await FSofTUtils.OSInterface.Helper.GoTo(page);
                  await page.ActualizeContent();
               } catch (Exception ex) {
                  mainPage.IsBusy = false;
                  await Helper2.ShowExceptionMessage(mainPage, "Fehler bei " + nameof(ShowTracklistAndMarkerlist), ex);
               } finally {
                  //mainPage.IsBusy = false;
                  isCallingSubpage = false;
               }
            }
         }

      }
   }
}
