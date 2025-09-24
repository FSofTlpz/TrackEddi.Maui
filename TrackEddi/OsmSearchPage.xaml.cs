using FSofTUtils.Geography.GeoCoding;
using FSofTUtils.Geometry;
using System.Collections.ObjectModel;
using TrackEddi.Common;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class OsmSearchPage : ContentPage {

      /// <summary>
      /// Map-Control
      /// </summary>
      SpecialMapCtrl.SpecialMapCtrl? map;

      AppData? appData;

      /// <summary>
      /// zum Sortieren der <see cref="GeoCodingResultOsm"/>-Ergebnisse (nach "Entfernung" zur Kartenmitte)
      /// </summary>
      public class GeoCodingResultOsmDistanceComparer : Comparer<GeoCodingResultOsm> {

         public double centerlon, centerlat;

         public GeoCodingResultOsmDistanceComparer(double centerlon, double centerlat) {
            this.centerlon = centerlon;
            this.centerlat = centerlat;
         }

         public override int Compare(GeoCodingResultOsm? x, GeoCodingResultOsm? y) {
            return (x != null ? pseudodistance(centerlon, x.Longitude, centerlat, x.Latitude) : 0) <
                   (y != null ? pseudodistance(centerlon, y.Latitude, centerlat, y.Longitude) : 0) ? -1 : 1;
         }

         double pseudodistance(double lon1, double lon2, double lat1, double lat2) {
            return (lon1 - lon2) * (lon1 - lon2) + (lat1 - lat2) * (lat1 - lat2);
         }

      }

      ObservableCollection<PlaceItem> osmItems = new ObservableCollection<PlaceItem>();

      ObservableCollection<PlaceItem> osmReverseItems = new ObservableCollection<PlaceItem>();

      /// <summary>
      /// Text für die OSM-Suche
      /// </summary>
      public string OsmPlacePattern {
         get => appData != null ? appData.LastSearchPattern : string.Empty;
         set {
            if (appData != null)
               appData.LastSearchPattern = value;
         }
      }

      //public Command<PlaceItem> OsmPlaceCmd { get; private set; }


      public OsmSearchPage() {
         InitializeComponent();

         //OsmPlaceCmd = new Command<PlaceItem>(onOsmPlaceCmd);
      }

      public OsmSearchPage(SpecialMapCtrl.SpecialMapCtrl map,
                           AppData appData) : this() {
         this.map = map;
         this.appData = appData;

         BindingContext = this;
      }

      bool firstOnAppearing = true;

      protected override void OnAppearing() {
         base.OnAppearing();
         if (firstOnAppearing) {
            firstOnAppearing = false;
            if (appData != null)
               ListViewOsm.ItemsSource = PlaceItem.ConvertPlaceList(osmItems, appData.LastSearchResults);
         }
      }


      T getItem<T>(object sender) {
         if (sender is ImageButton)
            return (T)((ImageButton)sender).CommandParameter;

         if (sender is Button)
            return (T)((Button)sender).CommandParameter;

         //if (sender is TapGestureRecognizer) {
         //   TapGestureRecognizer g = (TapGestureRecognizer)sender;
         //   return g.CommandParameter != null ?
         //               (WorkbenchContentPage_ListViewObjectItem)g.CommandParameter :
         //               new WorkbenchContentPage_ListViewObjectItem(new Track("FEHLER"));
         //}

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

      private void OsmPlaceTapped(object sender, TappedEventArgs e) => onOsmPlaceCmdAsync(getItem<PlaceItem>(e));

      #region OSM-Suche

      async private void buttonOsm_Clicked(object sender, EventArgs e) {
         try {
            string pattern = OsmPlacePattern.Trim();
            if (!string.IsNullOrEmpty(pattern)) {
               osmItems.Clear();
               GeoCodingResultOsm[] geoCodingResultOsm = await GeoCodingResultOsm.GetAsync(pattern);
               if (geoCodingResultOsm.Length > 0) {
                  if (map != null)
                     Array.Sort(geoCodingResultOsm, new GeoCodingResultOsmDistanceComparer(map.M_CenterLon, map.M_CenterLat));
                  foreach (GeoCodingResultOsm result in geoCodingResultOsm)
                     osmItems.Add(new PlaceItem(string.Format("{0} ({1:N6}° {2:N6}°)",
                                                            result.Name,
                                                            result.Longitude,
                                                            result.Latitude),
                                                  result.Longitude,
                                                  result.Latitude));
                  if (appData != null)
                     appData.LastSearchResults = PlaceItem.ConvertPlaceList(osmItems);
               }
            }
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onOsmPlaceCmdAsync(PlaceItem item) {
         try {
            if (map != null) {
               await map.M_ZoomToRangeAsync(new PointD(item.Longitude, item.Latitude),
                                            new PointD(item.Longitude, item.Latitude),
                                            true);
               if (appData != null) {
                  appData.LastZoom = map.M_Zoom;
                  appData.LastLongitude = map.M_CenterLon;
                  appData.LastLatitude = map.M_CenterLat;
               }
            }

            await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      #endregion

      #region OSM-Rückwärtssuche für den Kartenmittelpunkt

      async void buttonOsmBack_Clicked(object sender, EventArgs e) {
         try {
            if (map != null) {
               GeoCodingReverseResultOsm[] geoCodingReverseResultOsms = await GeoCodingReverseResultOsm.GetAsync(map.M_CenterLon, map.M_CenterLat);
               if (geoCodingReverseResultOsms.Length > 0) {
                  osmReverseItems.Clear();
                  foreach (var item in geoCodingReverseResultOsms) {
                     osmReverseItems.Add(new PlaceItem(item.Name, 0, 0));
                  }
                  ListViewOsmBack.ItemsSource = osmReverseItems;
               }
            }
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      #endregion

   }
}