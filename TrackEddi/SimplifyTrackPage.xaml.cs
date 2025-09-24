using FSofTUtils.Geography;
using FSofTUtils.Geography.PoorGpx;
using SpecialMapCtrl;
using System.Collections.ObjectModel;
using System.Text;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class SimplifyTrackPage : ContentPage {

      /// <summary>
      /// Dialog mit Auswahl beendet
      /// </summary>
      public event EventHandler<EventArgs>? EndWithOk;

      public Track? NewTrack { get; protected set; }

      Track? orgTrack;

      Common.AppData? appData;

      readonly ObservableCollection<SimplifyTrackPage_ListViewObjectItem>? simplificationdatalst;

      public class SimplificationData {

         /// <summary>
         /// einige einfache Standarddefinitionen
         /// </summary>
         public static string[] StdDefs = [
               new SimplificationData() {
               Name = "Wandern",
               AscendOutlier = 40,
               AscendOutlierLength = 50,
               SpeedOutlier = 10,
               GapFill4Time = true,
               HSimplification = GpxSimplification.HSimplification.Douglas_Peucker,
               HSimplificationWidth = 0.2,
               VSimplification = GpxSimplification.VSimplification.SlidingIntegral,
               VSimplificationWidth = 100,
               VSimplificationFractionalDigits = 1,
            }.AsString(),
               new SimplificationData() {
               Name = "Wandern (Smartphon)",
               AscendOutlier = 40,
               AscendOutlierLength = 50,
               RemoveSpikes = true,
               SpeedOutlier = 10,
               GapFill4Time = true,
               HSimplification = GpxSimplification.HSimplification.Douglas_Peucker,
               HSimplificationWidth = 0.2,
               VSimplification = GpxSimplification.VSimplification.SlidingIntegral,
               VSimplificationWidth = 400,
               VSimplificationFractionalDigits = 1,
            }.AsString(),
            new SimplificationData() {
               Name = "Radfahren",
               AscendOutlier = 25,
               AscendOutlierLength = 50,
               SpeedOutlier = 60,
               GapFill4Time = true,
               HSimplification = GpxSimplification.HSimplification.Douglas_Peucker,
               HSimplificationWidth = 0.2,
               VSimplification = GpxSimplification.VSimplification.SlidingIntegral,
               VSimplificationWidth = 100,
               VSimplificationFractionalDigits = 1,
            }.AsString(),
            new SimplificationData() {
               Name = "Radfahren (Smartphon)",
               AscendOutlier = 25,
               AscendOutlierLength = 50,
               RemoveSpikes = true,
               SpeedOutlier = 60,
               GapFill4Time = true,
               HSimplification = GpxSimplification.HSimplification.Douglas_Peucker,
               HSimplificationWidth = 0.2,
               VSimplification = GpxSimplification.VSimplification.SlidingIntegral,
               VSimplificationWidth = 400,
               VSimplificationFractionalDigits = 1,
            }.AsString(),
         ];

         public string Name = string.Empty;

         public GpxSimplification.HSimplification HSimplification = GpxSimplification.HSimplification.Nothing;
         public double HSimplificationWidth = 0.2;

         public GpxSimplification.VSimplification VSimplification = GpxSimplification.VSimplification.Nothing;
         public double VSimplificationWidth = 100;
         public int VSimplificationWidthPt = 50;
         public double VSimplificationLowPassFreq = 0.00140;
         public int VSimplificationLowPassSamplerate = 10;
         public double VSimplificationLowPassDelay = 0.023;
         public int VSimplificationFractionalDigits = 1;
         public int VSimplificationRRWidth = 50;
         public int VSimplificationRROverlap = 5;
         public double VSimplificationRRLambda = 0.0001;

         /// <summary>
         /// max. Geschwindigkeit in km/h
         /// </summary>
         public double SpeedOutlier = 0;

         /// <summary>
         /// max. An-/Abstieg in Prozent
         /// </summary>
         public double AscendOutlier = 0;
         /// <summary>
         /// Testwegstrecke in m
         /// </summary>
         public int AscendOutlierLength = 0;

         public bool RemoveTimestamps = false;
         public bool RemoveHeights = false;

         public bool MinimalHeightIsActiv = false;
         public double MinimalHeight = 0;

         public bool MaximalHeightIsActiv = false;
         public double MaximalHeight = 0;

         public bool HSimplificationIsActiv => HSimplification != GpxSimplification.HSimplification.Nothing &&
                                                      0 < HSimplificationWidth;
         public bool VSimplificationIsActiv => (VSimplification == GpxSimplification.VSimplification.SlidingIntegral &&
                                                      0 < VSimplificationWidth) ||
                                               (VSimplification == GpxSimplification.VSimplification.SlidingMean &&
                                                      0 < VSimplificationWidth) ||
                                               (VSimplification == GpxSimplification.VSimplification.LowPassFilter &&
                                                      0 < VSimplificationLowPassFreq &&
                                                      0 < VSimplificationLowPassSamplerate &&
                                                      0 <= VSimplificationLowPassDelay) ||
                                               (VSimplification == GpxSimplification.VSimplification.RidgeRegression &&
                                                      0 < VSimplificationRRLambda &&
                                                      0 <= VSimplificationRROverlap &&
                                                      2 < VSimplificationRRWidth);
         public bool SpeedOutlierIsActiv => 0 < SpeedOutlier;
         public bool RemoveSpikes = false;
         public bool AscendOutlierIsActiv => 0 < AscendOutlier && 1 < AscendOutlierLength;

         public bool PointRangeIsActiv = false;
         public double PointRangeHeight = 0;
         public int PointRangeStart = 0;
         public int PointRangeCount = 0;

         public bool GapFill4Time = false;

         public bool GapFill4Height = false;


         public SimplificationData() { }

         public SimplificationData(string name) {
            Name = name;
         }

         public string AsString() => AsString(this);

         // Muss mit FromString() korrespondieren.

         public static string AsString(SimplificationData sd) {
            // Neue Parameter immer an das Ende!
            StringBuilder sb = new StringBuilder(sd.Name);
            sb.Append("\t");
            sb.Append((int)sd.HSimplification);
            sb.Append("\t");
            sb.Append(sd.HSimplificationWidth);
            sb.Append("\t");
            sb.Append((int)sd.VSimplification);
            sb.Append("\t");
            sb.Append(sd.VSimplificationWidth);
            sb.Append("\t");
            sb.Append(sd.SpeedOutlier);
            sb.Append("\t");
            sb.Append(sd.AscendOutlier);
            sb.Append("\t");
            sb.Append(sd.AscendOutlierLength);
            sb.Append("\t");
            sb.Append(sd.RemoveTimestamps);
            sb.Append("\t");
            sb.Append(sd.RemoveHeights);
            sb.Append("\t");
            sb.Append(sd.MinimalHeightIsActiv);
            sb.Append("\t");
            sb.Append(sd.MinimalHeight);
            sb.Append("\t");
            sb.Append(sd.MaximalHeightIsActiv);
            sb.Append("\t");
            sb.Append(sd.MaximalHeight);
            sb.Append("\t");
            sb.Append(sd.PointRangeIsActiv);
            sb.Append("\t");
            sb.Append(sd.PointRangeHeight);
            sb.Append("\t");
            sb.Append(sd.PointRangeStart);
            sb.Append("\t");
            sb.Append(sd.PointRangeCount);
            sb.Append("\t");
            sb.Append(sd.GapFill4Time);
            sb.Append("\t");
            sb.Append(sd.GapFill4Height);
            sb.Append("\t");
            sb.Append(sd.VSimplificationLowPassFreq);
            sb.Append("\t");
            sb.Append(sd.VSimplificationLowPassSamplerate);
            sb.Append("\t");
            sb.Append(sd.VSimplificationLowPassDelay);
            sb.Append("\t");
            sb.Append(sd.VSimplificationFractionalDigits);
            sb.Append("\t");
            sb.Append(sd.RemoveSpikes);
            sb.Append("\t");
            sb.Append(sd.VSimplificationRRWidth);
            sb.Append("\t");
            sb.Append(sd.VSimplificationRROverlap);
            sb.Append("\t");
            sb.Append(sd.VSimplificationRRLambda);
            sb.Append("\t");
            sb.Append(sd.VSimplificationWidthPt);

            return sb.ToString();
         }

         // Muss mit AsString() korrespondieren.

         public static SimplificationData FromString(string txt) {
            SimplificationData sd = new SimplificationData();
            string[] tmp = txt.Split('\t');
            int i = 0;
            if (i < tmp.Length)
               sd.Name = tmp[i++];
            if (i < tmp.Length)
               sd.HSimplification = (GpxSimplification.HSimplification)Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.HSimplificationWidth = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplification = (GpxSimplification.VSimplification)Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationWidth = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.SpeedOutlier = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.AscendOutlier = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.AscendOutlierLength = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.RemoveTimestamps = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.RemoveHeights = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.MinimalHeightIsActiv = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.MinimalHeight = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.MaximalHeightIsActiv = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.MaximalHeight = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeIsActiv = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeHeight = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeStart = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.PointRangeCount = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.GapFill4Time = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.GapFill4Height = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationLowPassFreq = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationLowPassSamplerate = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationLowPassDelay = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationFractionalDigits = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.RemoveSpikes = Convert.ToBoolean(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationRRWidth = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationRROverlap = Convert.ToInt32(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationRRLambda = Convert.ToDouble(tmp[i++]);
            if (i < tmp.Length)
               sd.VSimplificationWidthPt = Convert.ToInt32(tmp[i++]);

            return sd;
         }

      }

      //public Command<SimplifyTrackPage_ListViewObjectItem>? SimplificationDataUse { get; private set; }
      //public Command<SimplifyTrackPage_ListViewObjectItem>? SimplificationDataDelete { get; private set; }
      //public Command<SimplifyTrackPage_ListViewObjectItem>? SimplificationDataMoveDown { get; private set; }
      //public Command<SimplifyTrackPage_ListViewObjectItem>? SimplificationDataMoveUp { get; private set; }


      public SimplifyTrackPage() {
         InitializeComponent();
      }

      public SimplifyTrackPage(Track track, Common.AppData appdata) : this() {
         BindingContext = this;

         orgTrack = track;
         NewTrack = null;

         appData = appdata;

         simplificationdatalst = new ObservableCollection<SimplifyTrackPage_ListViewObjectItem>();

         //SimplificationDataUse = new Command<SimplifyTrackPage_ListViewObjectItem>(onSimplificationDataUse);
         //SimplificationDataDelete = new Command<SimplifyTrackPage_ListViewObjectItem>(onSimplificationDataDelete);
         //SimplificationDataMoveDown = new Command<SimplifyTrackPage_ListViewObjectItem>(onSimplificationDataMoveDown);
         //SimplificationDataMoveUp = new Command<SimplifyTrackPage_ListViewObjectItem>(onSimplificationDataMoveUp);

         loadSimplificationData();
         ListViewSimplificationData.ItemsSource = simplificationdatalst;
         if (simplificationdatalst.Count > 0)
            setActualData(simplificationdatalst[0].SimplificationData);
         else
            setActualData(null);

         Title = "Track vereinfachen: " + track.VisualName;
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

      private void SimplificationDataUseTapped(object sender, TappedEventArgs e) => 
         onSimplificationDataUse(getItem<SimplifyTrackPage_ListViewObjectItem>(e));
      private void SimplificationDataDeleteClicked(object sender, EventArgs e) => 
         onSimplificationDataDelete(getItem<SimplifyTrackPage_ListViewObjectItem>(sender));
      private void SimplificationDataMoveDownClicked(object sender, EventArgs e) => 
         onSimplificationDataMoveDown(getItem<SimplifyTrackPage_ListViewObjectItem>(sender));
      private void SimplificationDataMoveUpClicked(object sender, EventArgs e) => 
         onSimplificationDataMoveUp(getItem<SimplifyTrackPage_ListViewObjectItem>(sender));

      void saveSimplificationData() {
         List<string> tmp = new List<string>();
         if (simplificationdatalst != null)
            foreach (var item in simplificationdatalst)
               tmp.Add(item.SimplificationData.AsString());
         if (appData != null)
            appData.SimplifyDatasetList = tmp;
      }

      void loadSimplificationData() {
         if (appData != null) {
            List<string> tmp = new List<string>(appData.SimplifyDatasetList);
            if (simplificationdatalst != null &&
                simplificationdatalst.Count == 0 &&
                tmp.Count == 0)
               tmp.AddRange(SimplificationData.StdDefs);

            if (simplificationdatalst != null)
               foreach (string item in tmp) {
                  SimplificationData sd = SimplificationData.FromString(item);
                  simplificationdatalst.Add(new SimplifyTrackPage_ListViewObjectItem(sd.Name, sd));
               }
         }
      }

      /// <summary>
      /// liefert ein <see cref="SimplificationData"/>-Objekt entsprechend der akt. Daten im Form
      /// </summary>
      /// <returns></returns>
      SimplificationData getActualData() {
         return new SimplificationData() {
            Name = entryDatasetName.Text == null ? "" : entryDatasetName.Text.Trim(),

            RemoveTimestamps = cbDeleteTimestamps.IsChecked,
            RemoveHeights = cbDeleteHeights.IsChecked,

            MinimalHeightIsActiv = cbMinHeight.IsChecked,
            MinimalHeight = getEntryValue(entryMinHeight, -15000, 15000, 0),

            MaximalHeightIsActiv = cbMaxHeight.IsChecked,
            MaximalHeight = getEntryValue(entryMaxHeight, -15000, 15000, 5000),

            PointRangeIsActiv = cbPointRangeHeight.IsChecked,
            PointRangeHeight = getEntryValue(entryPointRangeHeight, -15000, 15000, 0),
            PointRangeStart = getEntryValue(entryPointRangeStart, 1, 999999, 1),
            PointRangeCount = getEntryValue(entryPointRangeCount, 1, 999999, 10),

            RemoveSpikes = cbSpikes.IsChecked,
            SpeedOutlier = cbSpeedOulier.IsChecked ? getEntryValue(entrySpeedOulier, 1, 500, 10) : -1,

            AscendOutlier = cbAscentOulier.IsChecked ? getEntryValue(entryAscentOulier, 1, 100, 25) : -1,
            AscendOutlierLength = cbAscentOulier.IsChecked ? getEntryValue(entryAscentOulierLength, 1, 1000, 50) : -1,

            GapFill4Time = cbRemoveGaps4Time.IsChecked,

            GapFill4Height = cbRemoveGaps4Height.IsChecked,

            HSimplification = HSimpDP.IsChecked ? GpxSimplification.HSimplification.Douglas_Peucker :
                              HSimpRW.IsChecked ? GpxSimplification.HSimplification.Reumann_Witkam :
                                                  GpxSimplification.HSimplification.Nothing,
            HSimplificationWidth = getEntryValue(entryHSimplWidth, 0.01, 50, 0.2),

            VSimplification = VSimpSI.IsChecked ? GpxSimplification.VSimplification.SlidingIntegral :
                              VSimpSM.IsChecked ? GpxSimplification.VSimplification.SlidingMean :
                              VSimpLP.IsChecked ? GpxSimplification.VSimplification.LowPassFilter :
                                                  GpxSimplification.VSimplification.Nothing,
            VSimplificationWidthPt = getEntryValue(entryVSimplWidthPt, 3, 1000, 100),
            VSimplificationWidth = getEntryValue(entryVSimplWidth, 1, 1000, 100),
            VSimplificationLowPassFreq = getEntryValue(entryVSimplLPFreq, .0001, 10, 0.0035),
            VSimplificationLowPassSamplerate = getEntryValue(entryVSimplLPSamplerate, 1, 1000, 10),
            VSimplificationLowPassDelay = getEntryValue(entryVSimplLPDelay, .001, 0.005, 0.023),
            VSimplificationFractionalDigits = getEntryValue(entryVSimplFractionalDigits, 0, 6, 3),
            VSimplificationRRWidth = getEntryValue(entryVSimplRRWidth, 3, 1000, 50),
            VSimplificationRROverlap = getEntryValue(entryVSimplRROverlap, 0, 40, 5),
            VSimplificationRRLambda = getEntryValue(entryVSimplRRLambda, 0.0000001, 2, 0.0001),
         };
      }

      /// <summary>
      /// setzt die Daten entsprechend des <see cref="SimplificationData"/>
      /// </summary>
      /// <param name="sd"></param>
      void setActualData(SimplificationData? sd) {
         if (sd != null) {
            entryDatasetName.Text = sd.Name;

            cbDeleteTimestamps.IsChecked = sd.RemoveTimestamps;
            cbDeleteHeights.IsChecked = sd.RemoveHeights;

            cbMinHeight.IsChecked = sd.MinimalHeightIsActiv;
            entryMinHeight.Text = sd.MinimalHeight.ToString();

            cbMaxHeight.IsChecked = sd.MaximalHeightIsActiv;
            entryMaxHeight.Text = sd.MaximalHeight.ToString();

            cbPointRangeHeight.IsChecked = sd.PointRangeIsActiv;
            entryPointRangeHeight.Text = sd.PointRangeHeight.ToString();
            entryPointRangeStart.Text = sd.PointRangeStart.ToString();
            entryPointRangeCount.Text = sd.PointRangeCount.ToString();

            cbSpeedOulier.IsChecked = sd.SpeedOutlierIsActiv;
            entrySpeedOulier.Text = sd.SpeedOutlier.ToString();

            cbSpikes.IsChecked = sd.RemoveSpikes;

            cbAscentOulier.IsChecked = sd.AscendOutlierIsActiv;
            entryAscentOulier.Text = sd.AscendOutlier.ToString();
            entryAscentOulierLength.Text = sd.AscendOutlierLength.ToString();

            cbRemoveGaps4Time.IsChecked = sd.GapFill4Time;

            cbRemoveGaps4Height.IsChecked = sd.GapFill4Height;

            switch (sd.HSimplification) {
               case GpxSimplification.HSimplification.Douglas_Peucker: HSimpDP.IsChecked = true; break;
               case GpxSimplification.HSimplification.Reumann_Witkam: HSimpRW.IsChecked = true; break;
               default: HSimpNo.IsChecked = true; break;
            }
            entryHSimplWidth.Text = sd.HSimplificationWidth.ToString();

            switch (sd.VSimplification) {
               case GpxSimplification.VSimplification.SlidingMean: VSimpSM.IsChecked = true; break;
               case GpxSimplification.VSimplification.SlidingIntegral: VSimpSI.IsChecked = true; break;
               case GpxSimplification.VSimplification.LowPassFilter: VSimpLP.IsChecked = true; break;
               default: VSimpNo.IsChecked = true; break;
            }
            entryVSimplWidthPt.Text = sd.VSimplificationWidthPt.ToString();
            entryVSimplWidth.Text = sd.VSimplificationWidth.ToString();
            entryVSimplLPFreq.Text = sd.VSimplificationLowPassFreq.ToString();
            entryVSimplLPSamplerate.Text = sd.VSimplificationLowPassSamplerate.ToString();
            entryVSimplLPDelay.Text = sd.VSimplificationLowPassDelay.ToString();
            entryVSimplFractionalDigits.Text = sd.VSimplificationFractionalDigits.ToString();
            entryVSimplRRWidth.Text = sd.VSimplificationRRWidth.ToString();
            entryVSimplRROverlap.Text = sd.VSimplificationRROverlap.ToString();
            entryVSimplRRLambda.Text = sd.VSimplificationRRLambda.ToString();

         } else {

            entryDatasetName.Text = "";

            cbDeleteTimestamps.IsChecked = false;
            cbDeleteHeights.IsChecked = false;

            cbMinHeight.IsChecked = false;
            entryMinHeight.Text = "0";

            cbMaxHeight.IsChecked = false;
            entryMaxHeight.Text = "5000";

            cbPointRangeHeight.IsChecked = false;
            entryPointRangeHeight.Text = "0";
            entryPointRangeStart.Text = "1";
            entryPointRangeCount.Text = "10";

            cbSpikes.IsChecked = false;

            cbSpeedOulier.IsChecked = false;
            entrySpeedOulier.Text = "10";

            cbAscentOulier.IsChecked = false;
            entryAscentOulier.Text = "25";
            entryAscentOulierLength.Text = "50";

            cbRemoveGaps4Time.IsChecked = false;

            cbRemoveGaps4Height.IsChecked = false;

            HSimpNo.IsChecked = true;
            entryHSimplWidth.Text = 0.2.ToString();

            VSimpNo.IsChecked = true;
            entryVSimplWidthPt.Text = "100";
            entryVSimplWidth.Text = "100";
            entryVSimplLPFreq.Text = 0.0018.ToString();
            entryVSimplLPSamplerate.Text = "10";
            entryVSimplLPDelay.Text = 0.023.ToString();
            entryVSimplRRWidth.Text = "50";
            entryVSimplRROverlap.Text = "5";
            entryVSimplRRLambda.Text = "0,0001";
         }
      }

      double getEntryValue(Entry entry, double min, double max, double def) {
         double v = def;
         try {
            v = Math.Max(min, Math.Min(Convert.ToDouble(entry.Text == null ? "" : entry.Text), max));
         } catch { }
         return v;
      }

      double getEntryValue(Entry entry, double min, double max) => getEntryValue(entry, min, max, min);

      int getEntryValue(Entry entry, int min, int max, int def) {
         int v = def;
         try {
            v = Math.Max(min, Math.Min(Convert.ToInt32(entry.Text == null ? "" : entry.Text), max));
         } catch { }
         return v;
      }

      async void onSimplificationDataUse(SimplifyTrackPage_ListViewObjectItem item) {
         try {
            if (simplificationdatalst != null) {
               int idx = simplificationdatalst.IndexOf(item);
               if (0 <= idx)
                  setActualData(simplificationdatalst[idx].SimplificationData);
            }
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void onSimplificationDataDelete(SimplifyTrackPage_ListViewObjectItem item) {
         if (simplificationdatalst != null) {
            if (await FSofTUtils.OSInterface.Helper.MessageBox(this, "Achtung", "Soll der Track '" + item.Name + "' wirklich gelöscht werden?", "ja", "nein")) {
               int idx = simplificationdatalst.IndexOf(item);
               simplificationdatalst.RemoveAt(idx);
               //if (idx > 0)
               //   ListViewSimplificationData.SelectedItem = simplificationdatalst[idx];
               //else if (simplificationdatalst.Count > 0)
               //   ListViewSimplificationData.SelectedItem = 0;
               saveSimplificationData();
            }
         }
      }

      void onSimplificationDataMoveUp(SimplifyTrackPage_ListViewObjectItem item) {
         if (simplificationdatalst != null) {
            int idx = simplificationdatalst.IndexOf(item);
            if (0 < idx) {
               simplificationdatalst.RemoveAt(idx);
               simplificationdatalst.Insert(idx - 1, item);
               ListViewSimplificationData.ScrollTo(item, ScrollToPosition.MakeVisible, true);
               saveSimplificationData();
            }
         }
      }

      void onSimplificationDataMoveDown(SimplifyTrackPage_ListViewObjectItem item) {
         if (simplificationdatalst != null) {
            int idx = simplificationdatalst.IndexOf(item);
            if (0 <= idx && idx < simplificationdatalst.Count - 1) {
               simplificationdatalst.RemoveAt(idx);
               simplificationdatalst.Insert(idx + 1, item);
               ListViewSimplificationData.ScrollTo(item, ScrollToPosition.MakeVisible, true);
               saveSimplificationData();
            }
         }
      }

      private void btnSave_Clicked(object sender, EventArgs e) {
         string name = entryDatasetName.Text.Trim();
         if (!string.IsNullOrEmpty(name) && simplificationdatalst != null) {
            SimplificationData sd = getActualData();
            sd.Name = name;
            simplificationdatalst.Add(new SimplifyTrackPage_ListViewObjectItem(sd.Name, sd));
            //ListViewSimplificationData.SelectedItem = simplificationdatalst[simplificationdatalst.Count - 1];
            saveSimplificationData();
         }
      }

      private void entryDatasetName_TextChanged(object sender, TextChangedEventArgs e) =>
         btnSave.IsEnabled = ((Entry)sender).Text.Trim().Length > 0;

      private void cbMinHeight_CheckedChanged(object sender, CheckedChangedEventArgs e) =>
         entryMinHeight.IsEnabled = ((CheckBox)sender).IsChecked;

      private void cbMaxHeight_CheckedChanged(object sender, CheckedChangedEventArgs e) =>
         entryMaxHeight.IsEnabled = ((CheckBox)sender).IsChecked;

      private void cbPointRangeHeight_CheckedChanged(object sender, CheckedChangedEventArgs e) =>
         entryPointRangeHeight.IsEnabled =
         entryPointRangeStart.IsEnabled =
         entryPointRangeCount.IsEnabled = ((CheckBox)sender).IsChecked;

      private void cbSpeedOulier_CheckedChanged(object sender, CheckedChangedEventArgs e) =>
         entrySpeedOulier.IsEnabled = ((CheckBox)sender).IsChecked;

      private void cbAscentOulier_CheckedChanged(object sender, CheckedChangedEventArgs e) =>
         entryAscentOulier.IsEnabled =
         entryAscentOulierLength.IsEnabled = ((CheckBox)sender).IsChecked;

      private void rbHSimp_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         if (((RadioButton)sender).IsChecked) {
            RadioButton rb = (RadioButton)sender;
            if (rb.Equals(HSimpNo))
               entryHSimplWidth.IsEnabled = false;
            else if (rb.Equals(HSimpDP) || rb.Equals(HSimpRW))
               entryHSimplWidth.IsEnabled = true;
         }
      }

      private void rbVSimp_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         if (((RadioButton)sender).IsChecked) {
            RadioButton rb = (RadioButton)sender;

            SetEnableStatus4Layout(slVSimplFractionalDigits, true);
            SetEnableStatus4Layout(slVSimplWidthPt, false);
            SetEnableStatus4Layout(slVSimplWidth, false);
            SetEnableStatus4Layout(slVSimplLPFreq, false);
            SetEnableStatus4Layout(slVSimplLPSamplerate, false);
            SetEnableStatus4Layout(slVSimplLPDelay, false);
            SetEnableStatus4Layout(slVSimplRRWidth, false);
            SetEnableStatus4Layout(slVSimplRROverlap, false);
            SetEnableStatus4Layout(slVSimplRRLambda, false);

            if (rb.Equals(VSimpNo)) {

               SetEnableStatus4Layout(slVSimplFractionalDigits, false);
               //SetEnableStatus4Layout(slVSimplWidthPt, false);
               //SetEnableStatus4Layout(slVSimplWidth, false);
               //SetEnableStatus4Layout(slVSimplLPFreq, false);
               //SetEnableStatus4Layout(slVSimplLPSamplerate, false);
               //SetEnableStatus4Layout(slVSimplLPDelay, false);
               //SetEnableStatus4Layout(slVSimplRRWidth, false);
               //SetEnableStatus4Layout(slVSimplRROverlap, false);
               //SetEnableStatus4Layout(slVSimplRRLambda, false);

            } else if (rb.Equals(VSimpSM)) {

               //SetEnableStatus4Layout(slVSimplFractionalDigits, true);
               SetEnableStatus4Layout(slVSimplWidthPt, true);
               //SetEnableStatus4Layout(slVSimplWidth, false);
               //SetEnableStatus4Layout(slVSimplLPFreq, false);
               //SetEnableStatus4Layout(slVSimplLPSamplerate, false);
               //SetEnableStatus4Layout(slVSimplLPDelay, false);
               //SetEnableStatus4Layout(slVSimplRRWidth, false);
               //SetEnableStatus4Layout(slVSimplRROverlap, false);
               //SetEnableStatus4Layout(slVSimplRRLambda, false);

            } else if (rb.Equals(VSimpSI)) {

               //SetEnableStatus4Layout(slVSimplFractionalDigits, true);
               //SetEnableStatus4Layout(slVSimplWidthPt, false);
               SetEnableStatus4Layout(slVSimplWidth, true);
               //SetEnableStatus4Layout(slVSimplLPFreq, false);
               //SetEnableStatus4Layout(slVSimplLPSamplerate, false);
               //SetEnableStatus4Layout(slVSimplLPDelay, false);
               //SetEnableStatus4Layout(slVSimplRRWidth, false);
               //SetEnableStatus4Layout(slVSimplRROverlap, false);
               //SetEnableStatus4Layout(slVSimplRRLambda, false);

            } else if (rb.Equals(VSimpLP)) {

               //SetEnableStatus4Layout(slVSimplFractionalDigits, true);
               //SetEnableStatus4Layout(slVSimplWidthPt, false);
               //SetEnableStatus4Layout(slVSimplWidth, false);
               SetEnableStatus4Layout(slVSimplLPFreq, true);
               SetEnableStatus4Layout(slVSimplLPSamplerate, true);
               SetEnableStatus4Layout(slVSimplLPDelay, true);
               //SetEnableStatus4Layout(slVSimplRRWidth, false);
               //SetEnableStatus4Layout(slVSimplRROverlap, false);
               //SetEnableStatus4Layout(slVSimplRRLambda, false);

            } else if (rb.Equals(VSimpRR)) {

               //SetEnableStatus4Layout(slVSimplFractionalDigits, true);
               //SetEnableStatus4Layout(slVSimplWidthPt, false);
               //SetEnableStatus4Layout(slVSimplWidth, false);
               //SetEnableStatus4Layout(slVSimplLPFreq, false);
               //SetEnableStatus4Layout(slVSimplLPSamplerate, false);
               //SetEnableStatus4Layout(slVSimplLPDelay, false);
               SetEnableStatus4Layout(slVSimplRRWidth, true);
               SetEnableStatus4Layout(slVSimplRROverlap, true);
               SetEnableStatus4Layout(slVSimplRRLambda, true);

            }
         }
      }

      private void cbDeleteTimestamps_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         foreach (StackLayout sl in new StackLayout[] { slSpeedOulier,
                                                        /*slRemoveGaps4Time*/})
            SetEnableStatus4Layout(sl, !cbDeleteTimestamps.IsChecked);
         cbRemoveGaps4Time.IsEnabled =
         cbSpeedOulier.IsEnabled = !cbDeleteTimestamps.IsChecked;
      }

      private void cbDeleteHeights_CheckedChanged(object sender, CheckedChangedEventArgs e) {
         foreach (StackLayout sl in new StackLayout[] { slAscentOulier,
                                                        slMinHeight,
                                                        slMaxHeight,
                                                        slPointRangeHeight,
                                                        slVSimpl})
            SetEnableStatus4Layout(sl, !cbDeleteHeights.IsChecked);
         cbRemoveGaps4Height.IsEnabled =
         cbAscentOulier.IsEnabled =
         cbMinHeight.IsEnabled =
         cbMaxHeight.IsEnabled =
         cbPointRangeHeight.IsEnabled = !cbDeleteHeights.IsChecked;
      }

      /// <summary>
      /// Die Umschaltung des visuellen Stils muss leider expliziet ausgelöst werden.
      /// </summary>
      /// <param name="layout"></param>
      /// <param name="enabled"></param>
      static void SetEnableStatus4Layout(Layout layout, bool enabled) {
         layout.IsEnabled = enabled;
         foreach (var child in layout.Children) {
            if (child is Label ||
                child is Entry ||
                child is CheckBox)
               ((View)child).IsEnabled = layout.IsEnabled;
            else
               if (child is Layout) SetEnableStatus4Layout((Layout)child, enabled);
         }
      }

      async private void Button_Clicked(object sender, EventArgs e) {
         int removedtimestamps = 0;
         int removedheights = 0;
         int setminheights = 0;
         int setmaxheights = 0;
         int setheights = 0;
         int spikes = 0;
         int speedoutliers = 0;
         int heightoutliers = 0;
         int gapfilledheights = 0;
         int gapfilledtimestamps = 0;
         int removedhsimpl = 0;
         int changedvsimpl = 0;

         try {

            SimplificationData sd = getActualData();

            List<GpxTrackPoint> gpxTrackPoints = new List<GpxTrackPoint>();
            if (orgTrack != null && orgTrack.GpxSegment != null)
               for (int i = 0; i < orgTrack.GpxSegment.Points.Count; i++)
                  gpxTrackPoints.Add(new GpxTrackPoint(orgTrack.GpxSegment.Points[i]));

            if (sd.RemoveTimestamps)
               removedtimestamps = GpxSimplification.RemoveTimestamp(gpxTrackPoints);

            if (sd.RemoveHeights)
               removedheights = GpxSimplification.RemoveHeight(gpxTrackPoints);

            if (!sd.RemoveHeights && sd.MinimalHeightIsActiv)
               GpxSimplification.SetHeight(gpxTrackPoints, out setminheights, out _, sd.MinimalHeight);

            if (!sd.RemoveHeights && sd.MaximalHeightIsActiv)
               GpxSimplification.SetHeight(gpxTrackPoints, out _, out setmaxheights, double.MinValue, sd.MaximalHeight);

            if (sd.PointRangeIsActiv)
               setheights = GpxSimplification.SetHeight(gpxTrackPoints, sd.PointRangeHeight, sd.PointRangeStart, sd.PointRangeCount);

            if (sd.SpeedOutlierIsActiv)
               speedoutliers = GpxSimplification.RemoveSpeedOutlier(gpxTrackPoints, sd.SpeedOutlier / 3.6).Length;

            if (sd.RemoveSpikes)
               spikes = GpxSimplification.RemoveSpikes(gpxTrackPoints).Length;

            if (!sd.RemoveHeights && sd.AscendOutlierIsActiv)
               heightoutliers = GpxSimplification.RemoveHeigthOutlier(gpxTrackPoints, sd.AscendOutlierLength, sd.AscendOutlier).Length;

            if (!sd.RemoveTimestamps && sd.GapFill4Time)
               gapfilledtimestamps = GpxSimplification.GapFill4Time(gpxTrackPoints).Length;

            if (!sd.RemoveHeights && sd.GapFill4Height)
               gapfilledheights = GpxSimplification.GapFill4Height(gpxTrackPoints).Length;

            if (!sd.RemoveHeights && sd.HSimplificationIsActiv)
               removedhsimpl = GpxSimplification.HorizontalSimplification(gpxTrackPoints, sd.HSimplification, sd.HSimplificationWidth).Length;

            if (sd.VSimplificationIsActiv) {
               double[] vparams =
                   sd.VSimplification == GpxSimplification.VSimplification.SlidingMean ?
                     [
                        sd.VSimplificationWidthPt,
                        sd.VSimplificationFractionalDigits,
                     ] :
                   sd.VSimplification == GpxSimplification.VSimplification.SlidingIntegral ?
                     [
                        sd.VSimplificationWidth,
                        sd.VSimplificationFractionalDigits,
                     ] :
                   sd.VSimplification == GpxSimplification.VSimplification.LowPassFilter ?
                     [
                        sd.VSimplificationLowPassFreq,
                        sd.VSimplificationLowPassSamplerate,
                        sd.VSimplificationLowPassDelay,
                        sd.VSimplificationFractionalDigits,
                     ] :
                     [
                        sd.VSimplificationRRWidth,
                        sd.VSimplificationRROverlap,
                        sd.VSimplificationRRLambda,
                        sd.VSimplificationFractionalDigits,
                     ];

               changedvsimpl = GpxSimplification.VerticalSimplification(gpxTrackPoints, sd.VSimplification, vparams).Length;

            }

            if (orgTrack != null &&
                (removedtimestamps > 0 ||
                removedheights > 0 ||
                setminheights > 0 ||
                setmaxheights > 0 ||
                setheights > 0 ||
                spikes > 0 ||
                speedoutliers > 0 ||
                heightoutliers > 0 ||
                gapfilledheights > 0 ||
                gapfilledtimestamps > 0 ||
                removedhsimpl > 0 ||
                changedvsimpl > 0)) {
               NewTrack = new Track(gpxTrackPoints, orgTrack.VisualName + " (vereinfacht)");
               StringBuilder sb = new StringBuilder();

               if (removedtimestamps > 0)
                  sb.AppendLine("* " + removedtimestamps + " Zeitstempel entfernt");
               if (removedheights > 0)
                  sb.AppendLine("* " + removedheights + " Höhen entfernt");
               if (setminheights > 0)
                  sb.AppendLine("* " + setminheights + " Höhen auf Minimum " + sd.MinimalHeight + "m gesetzt");
               if (setmaxheights > 0)
                  sb.AppendLine("* " + setmaxheights + " Höhen auf Maximum " + sd.MaximalHeight + "m gesetzt");
               if (setheights > 0)
                  sb.AppendLine("* " + setmaxheights + " Höhen auf " + sd.PointRangeHeight + "m gesetzt");
               if (spikes > 0)
                  sb.AppendLine("* " + spikes + " Punkte als Spikes entfernt");
               if (speedoutliers > 0)
                  sb.AppendLine("* " + speedoutliers + " Punkte wegen Überschreitung der Maximalgeschwindigkeit " + sd.SpeedOutlier + "km/h entfernt");
               if (heightoutliers > 0)
                  sb.AppendLine("* " + heightoutliers + " Höhen wegen Überschreitung der max. Anstiegs " + sd.AscendOutlier + "% angepasst");
               if (gapfilledheights > 0)
                  sb.AppendLine("* " + gapfilledheights + " Punkte ohne Höhe mit interpolierter Höhe gesetzt");
               if (gapfilledtimestamps > 0)
                  sb.AppendLine("* " + gapfilledtimestamps + " Punkte ohne Zeitstempel mit interpolierten Zeitstempel gesetzt");
               if (removedhsimpl > 0)
                  sb.AppendLine("* " + removedhsimpl + " Punkte bei horizontaler Vereinfachung entfernt");
               if (changedvsimpl > 0)
                  sb.AppendLine("* " + changedvsimpl + " Punkte bei vertikaler Vereinfachung geändert");

               await FSofTUtils.OSInterface.Helper.MessageBox(this, "Ergebnis", sb.ToString());
            } else
               await FSofTUtils.OSInterface.Helper.MessageBox(this, "Ergebnis", "Es gab keine Veränderungen am Track.");
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }

         EndWithOk?.Invoke(this, EventArgs.Empty);
         await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
      }

   }
}