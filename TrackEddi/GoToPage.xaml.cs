using FSofTUtils.Geometry;
using System.Collections.ObjectModel;
using TrackEddi.Common;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class GoToPage : ContentPage {

      /// <summary>
      /// Map-Control
      /// </summary>
      SpecialMapCtrl.SpecialMapCtrl map;

      AppData appData;

      ObservableCollection<PlaceItem> posItems = new ObservableCollection<PlaceItem>();

      /// <summary>
      /// Positionsname
      /// </summary>
      public string PosName { get; set; } = string.Empty;

      #region bindable Vars für die Location

      public static BindableProperty DecLonProperty =
            BindableProperty.Create(nameof(DecLon), typeof(string), typeof(GoToPage), "", BindingMode.TwoWay);

      public string DecLon {
         get => (string)GetValue(DecLonProperty);
         set => SetValue(DecLonProperty, value);
      }

      public static BindableProperty DecLonEWProperty =
            BindableProperty.Create(nameof(DecLonEW), typeof(string), typeof(GoToPage), "", BindingMode.TwoWay);

      public string DecLonEW {
         get => (string)GetValue(DecLonEWProperty);
         set => SetValue(DecLonEWProperty, value);
      }

      public static BindableProperty DecLatProperty =
            BindableProperty.Create(nameof(DecLat), typeof(string), typeof(GoToPage), "", BindingMode.TwoWay);

      public string DecLat {
         get => (string)GetValue(DecLatProperty);
         set => SetValue(DecLatProperty, value);
      }

      public static BindableProperty DecLatNSProperty =
            BindableProperty.Create(nameof(DecLatNS), typeof(string), typeof(GoToPage), "", BindingMode.TwoWay);

      public string DecLatNS {
         get => (string)GetValue(DecLatNSProperty);
         set => SetValue(DecLatNSProperty, value);
      }

      public static BindableProperty DegreeLonProperty =
            BindableProperty.Create(nameof(DegreeLon), typeof(string), typeof(GoToPage), "", BindingMode.TwoWay);

      public string DegreeLon {
         get => (string)GetValue(DegreeLonProperty);
         set => SetValue(DegreeLonProperty, value);
      }

      public static BindableProperty MinuteLonProperty =
            BindableProperty.Create(nameof(MinuteLon), typeof(string), typeof(GoToPage), "", BindingMode.TwoWay);

      public string MinuteLon {
         get => (string)GetValue(MinuteLonProperty);
         set => SetValue(MinuteLonProperty, value);
      }

      public static BindableProperty SecondeLonProperty =
            BindableProperty.Create(nameof(SecondeLon), typeof(string), typeof(GoToPage), "", BindingMode.TwoWay);

      public string SecondeLon {
         get => (string)GetValue(SecondeLonProperty);
         set => SetValue(SecondeLonProperty, value);
      }

      public static BindableProperty DegreeLatProperty =
            BindableProperty.Create(nameof(DegreeLat), typeof(string), typeof(GoToPage), "", BindingMode.TwoWay);

      public string DegreeLat {
         get => (string)GetValue(DegreeLatProperty);
         set => SetValue(DegreeLatProperty, value);
      }

      public static BindableProperty MinuteLatProperty =
            BindableProperty.Create(nameof(MinuteLat), typeof(string), typeof(GoToPage), "", BindingMode.TwoWay);

      public string MinuteLat {
         get => (string)GetValue(MinuteLatProperty);
         set => SetValue(MinuteLatProperty, value);
      }

      public static BindableProperty SecondeLatProperty =
            BindableProperty.Create(nameof(SecondeLat), typeof(string), typeof(GoToPage), "", BindingMode.TwoWay);

      public string SecondeLat {
         get => (string)GetValue(SecondeLatProperty);
         set => SetValue(SecondeLatProperty, value);
      }

      #endregion

      //public Command<PlaceItem> PosCmd { get; private set; }
      //public Command<PlaceItem> PosDeleteCmd { get; private set; }
      //public Command<PlaceItem> MoveDownPlaceCommand { get; private set; }
      //public Command<PlaceItem> MoveUpPlaceCommand { get; private set; }


      public GoToPage(SpecialMapCtrl.SpecialMapCtrl map,
                      AppData appData) {
         InitializeComponent();
         this.map = map;
         this.appData = appData;

         //PosCmd = new Command<PlaceItem>(onPosCmd);
         //PosDeleteCmd = new Command<PlaceItem>(onPosDeleteCmd);
         //MoveDownPlaceCommand = new Command<PlaceItem>(onMoveDownPlace);
         //MoveUpPlaceCommand = new Command<PlaceItem>(onMoveUpPlace);

         ListViewPos.ItemsSource = PlaceItem.ConvertPlaceList(posItems, appData.PositionList);

         initLocation(map.M_CenterLon, map.M_CenterLat);

         BindingContext = this;
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

      private void PosTapped(object sender, TappedEventArgs e) => onPosCmd(getItem<PlaceItem>(e));
      private void PosDeleteClicked(object sender, EventArgs e) => onPosDeleteCmd(getItem<PlaceItem>(sender));
      private void MoveDownPlaceClicked(object sender, EventArgs e) => onMoveDownPlace(getItem<PlaceItem>(sender));
      private void MoveUpPlaceClicked(object sender, EventArgs e) => onMoveUpPlace(getItem<PlaceItem>(sender));

      #region Kartenpositionen

      async private void buttonPos_Clicked(object sender, EventArgs e) {
         try {
            string name = PosName.Trim();
            if (!string.IsNullOrEmpty(name)) {
               PlaceItem pi = new PlaceItem(name, map.M_CenterLon, map.M_CenterLat, map.M_Zoom);
               if (posItems.Count == 0)
                  posItems.Add(pi);
               else
                  posItems.Insert(0, pi);
               appData.PositionList = PlaceItem.ConvertPlaceList(posItems);
            }
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onPosCmd(PlaceItem item) {
         try {
            await map.M_SetLocationAndZoomAsync(item.Zoom, item.Longitude, item.Latitude);
            appData.LastZoom = map.M_Zoom;
            appData.LastLongitude = map.M_CenterLon;
            appData.LastLatitude = map.M_CenterLat;

            await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onPosDeleteCmd(PlaceItem item) {
         try {
            if (await FSofTUtils.OSInterface.Helper.MessageBox(this, "Achtung", "Soll der Ort '" + item.Name + "' wirklich gelöscht werden?", "ja", "nein")) {
               posItems.Remove(item);
               appData.PositionList = PlaceItem.ConvertPlaceList(posItems);
            }
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onMoveDownPlace(PlaceItem item) {
         try {
            int idx = posItems.IndexOf(item);
            if (0 <= idx && idx < posItems.Count - 1) {
               posItems.RemoveAt(idx);
               posItems.Insert(idx + 1, item);
               ListViewPos.ScrollTo(item, ScrollToPosition.MakeVisible, true);
               appData.PositionList = PlaceItem.ConvertPlaceList(posItems);
            }
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onMoveUpPlace(PlaceItem item) {
         try {
            int idx = posItems.IndexOf(item);
            if (0 < idx) {
               posItems.RemoveAt(idx);
               posItems.Insert(idx - 1, item);
               ListViewPos.ScrollTo(item, ScrollToPosition.MakeVisible, true);
               appData.PositionList = PlaceItem.ConvertPlaceList(posItems);
            }
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      #endregion

      #region Wechsel zur Location

      void initLocation(double lon, double lat) {
         setDecLon(lon);
         setDecLat(lat);

         setDMSLon(lon);
         setDMSLat(lat);
      }

      string cleanDigitString(string txt) {
         int p;
         while ((p = txt.IndexOfAny(new char[] { ' ', '-' })) >= 0)
            txt = txt.Substring(0, p) + txt.Substring(p + 1);
         return txt;
      }

      double digitStringAsPosDouble(string txt) {
         txt = cleanDigitString(txt);
         return txt == "" ?
                     0 :
                     Convert.ToDouble(txt);
      }

      int digitStringAsPosInt(string txt) {
         txt = cleanDigitString(txt);
         return txt == "" ?
                     0 :
                     Convert.ToInt32(txt);
      }

      double getDecLon() {
         return digitStringAsPosDouble(DecLon) * (DecLonEW.ToUpper() == "W" ? -1 : 1);
      }

      double getDecLat() {
         return digitStringAsPosDouble(DecLat) * (DecLatNS.ToUpper() == "S" ? -1 : 1);
      }

      void setDecLon(double v) {
         DecLon = Math.Abs(v).ToString("f8");
         DecLonEW = v < 0 ? "W" : "O";
      }

      void setDecLat(double v) {
         DecLat = Math.Abs(v).ToString("f8");
         DecLatNS = v < 0 ? "S" : "N";
      }

      double getDMSLon() {
         int d = digitStringAsPosInt(DegreeLon);
         int m = digitStringAsPosInt(MinuteLon);
         double s = digitStringAsPosDouble(SecondeLon);
         if (180 < d)
            d = 0;
         if (59 < m)
            m = 0;
         if (60 <= s)
            s = 0;
         return dms2d(d, m, (int)s, s - (int)s, DecLonEW.ToUpper() == "W");
      }

      double getDMSLat() {
         int d = digitStringAsPosInt(DegreeLat);
         int m = digitStringAsPosInt(MinuteLat);
         double s = digitStringAsPosDouble(SecondeLat);
         if (180 < d)
            d = 0;
         if (59 < m)
            m = 0;
         if (60 <= s)
            s = 0;
         return dms2d(d, m, (int)s, s - (int)s, DecLatNS.ToUpper() == "S");
      }

      void setDMSLon(double v) {
         d2dms(v, out int d, out int m, out int s, out double ss, out bool negativ);
         DegreeLon = d.ToString();
         MinuteLon = m.ToString("d2");
         SecondeLon = (s + ss).ToString("00.00");
         DecLonEW = negativ ? "W" : "O";
      }

      void setDMSLat(double v) {
         d2dms(v, out int d, out int m, out int s, out double ss, out bool negativ);
         DegreeLat = d.ToString();
         MinuteLat = m.ToString("d2");
         SecondeLat = (s + ss).ToString("00.00");
         DecLatNS = negativ ? "S" : "N";
      }

      /// <summary>
      /// wandelt einen Dezimalgradwert in die Bestandteile Grad-Minute-Sekunde um
      /// </summary>
      /// <param name="v"></param>
      /// <param name="d"></param>
      /// <param name="m"></param>
      /// <param name="s"></param>
      /// <param name="ss">Sekundenbruchteile (dez.)</param>
      /// <param name="negativ"></param>
      void d2dms(double v, out int d, out int m, out int s, out double ss, out bool negativ) {
         negativ = v < 0;
         if (negativ)
            v = -v;
         v *= 3600;     // in s
         ss = v - (int)v;

         d = (int)v / 3600;
         v -= d * 3600;

         m = (int)v / 60;
         v -= m * 60;

         s = (int)v;
      }

      /// <summary>
      /// wandelt einen Wert in Grad-Minute-Sekunde in Dezimalgrad um
      /// </summary>
      /// <param name="d"></param>
      /// <param name="m"></param>
      /// <param name="s"></param>
      /// <param name="ss">Sekundenbruchteile (dez.)</param>
      /// <param name="negativ"></param>
      /// <returns></returns>
      double dms2d(int d, int m, int s, double ss, bool negativ) {
         return (negativ ? -1 : 1) * (d + m / 60.0 + (s + ss) / 3600.0);
      }

      private void EntryLocation_Completed(object sender, EventArgs e) {
         Entry? entry = sender as Entry;
         if (entry != null) {
            string txt = entry.Text;

            if (EntryDecLonEW1.Equals(entry) ||
                EntryDecLonEW2.Equals(entry)) {

               txt = txt.ToUpper();
               if (txt != "O" && txt != "W")
                  txt = "O";
               DecLonEW = txt;

            } else if (EntryDecLatNS1.Equals(entry) ||
                       EntryDecLatNS2.Equals(entry)) {

               txt = txt.ToUpper();
               if (txt != "N" && txt != "S")
                  txt = "N";
               DecLatNS = txt;

            } else if (EntryDecLon.Equals(entry)) {

               double lon = getDecLon();
               setDecLon(lon);
               setDMSLon(lon);

            } else if (EntryDecLat.Equals(entry)) {

               double lat = getDecLat();
               setDecLat(lat);
               setDMSLat(lat);

            } else if (EntryDegreeLon.Equals(entry) ||
                       EntryMinuteLon.Equals(entry) ||
                       EntrySecondeLon.Equals(entry)) {

               double lon = getDMSLon();
               setDMSLon(lon);
               setDecLon(lon);

            } else if (EntryDegreeLat.Equals(entry) ||
                       EntryMinuteLat.Equals(entry) ||
                       EntrySecondeLat.Equals(entry)) {

               double lat = getDMSLat();
               setDMSLat(lat);
               setDecLat(lat);

            }

         }
      }

      async private void buttonLocation_Clicked(object sender, EventArgs e) {
         try {
            await map.M_ZoomToRangeAsync(new PointD(getDecLon(), getDecLat()),
                                         new PointD(getDecLon(), getDecLat()),
                                         true);
            appData.LastZoom = map.M_Zoom;
            appData.LastLongitude = map.M_CenterLon;
            appData.LastLatitude = map.M_CenterLat;

            appData.LastLocationLongitude = appData.LastLongitude;
            appData.LastLocationLatitude = appData.LastLatitude;

            await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }

      }

      #endregion
   }
}