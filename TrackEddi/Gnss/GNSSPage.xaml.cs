using System.Collections.ObjectModel;
using System.Text;
using TrackEddi.Gnns;
using static TrackEddi.Gnns.GnssData;

namespace TrackEddi.Gnss {

   public partial class GNNSPage : ContentPage {

      public GNNSPage() {
         InitializeComponent();
      }


      #region bindable Vars

      public static BindableProperty GnssHardwareModelNameProperty = BindableProperty.Create(
         nameof(GnssHardwareModelName), typeof(string), typeof(GeoLocationPage), "");

      public string GnssHardwareModelName {
         get => (string)GetValue(GnssHardwareModelNameProperty);
         set => SetValue(GnssHardwareModelNameProperty, value);
      }

      public static BindableProperty GnssStatusOnProperty = BindableProperty.Create(
         nameof(GnssStatusOn), typeof(string), typeof(GeoLocationPage), "");

      public string GnssStatusOn {
         get => (string)GetValue(GnssStatusOnProperty);
         set => SetValue(GnssStatusOnProperty, value);
      }

      public static BindableProperty GnssFixTimeProperty = BindableProperty.Create(
         nameof(GnssFixTime), typeof(string), typeof(GeoLocationPage), "");

      public string GnssFixTime {
         get => (string)GetValue(GnssFixTimeProperty);
         set => SetValue(GnssFixTimeProperty, value);
      }

      public static BindableProperty HasAntennaInfoProperty = BindableProperty.Create(
         nameof(HasAntennaInfo), typeof(string), typeof(GeoLocationPage), "");

      public string HasAntennaInfo {
         get => (string)GetValue(HasAntennaInfoProperty);
         set => SetValue(HasAntennaInfoProperty, value);
      }

      public static BindableProperty AntennaInfosProperty = BindableProperty.Create(
         nameof(AntennaInfos), typeof(string), typeof(GeoLocationPage), "");

      public string AntennaInfos {
         get => (string)GetValue(AntennaInfosProperty);
         set => SetValue(AntennaInfosProperty, value);
      }

      public static BindableProperty HasMeasurementsProperty = BindableProperty.Create(
         nameof(HasMeasurements), typeof(string), typeof(GeoLocationPage), "");

      public string HasMeasurements {
         get => (string)GetValue(HasMeasurementsProperty);
         set => SetValue(HasMeasurementsProperty, value);
      }

      public static BindableProperty HasNavigationMessagesProperty = BindableProperty.Create(
         nameof(HasNavigationMessages), typeof(string), typeof(GeoLocationPage), "");

      public string HasNavigationMessages {
         get => (string)GetValue(HasNavigationMessagesProperty);
         set => SetValue(HasNavigationMessagesProperty, value);
      }

      public static BindableProperty cProperty = BindableProperty.Create(
         nameof(c), typeof(string), typeof(GeoLocationPage), "");

      public string c {
         get => (string)GetValue(cProperty);
         set => SetValue(cProperty, value);
      }

      #endregion

      GnssData? gnssData;


      readonly ObservableCollection<SatDataItem> satlst = new ObservableCollection<SatDataItem>();


      public GNNSPage(GnssData? gnssData) {
         InitializeComponent();
         this.gnssData = gnssData;
         BindingContext = this;

         ListViewSat.ItemsSource = satlst;

      }

      protected override void OnAppearing() {
         base.OnAppearing();
         if (gnssData != null)
            gnssData.OnGnssStatusChanged += GnssData_OnGnssStatusChanged;
      }

      protected override void OnDisappearing() {
         base.OnDisappearing();
         if (gnssData != null)
            gnssData.OnGnssStatusChanged -= GnssData_OnGnssStatusChanged;
      }

      DateTime dtLast = DateTime.MinValue;

      long _isBuildSatList = 0;

      /// <summary>
      /// Wird Sat-Liste erzeugt?
      /// </summary>
      public bool IsBuildSatList {
         get => Interlocked.Read(ref _isBuildSatList) != 0;
         set => Interlocked.Exchange(ref _isBuildSatList, value ? 1 : 0);
      }


      private void GnssData_OnGnssStatusChanged(object? sender, SatelliteStatus status) {
         if (gnssData != null) {
            // ----------------------------
            // ab Android 9
            GnssHardwareModelName = gnssData.GnssHardwareModelName();
            // ----------------------------
            // ab Android 12
            AntennaInfos = gnssData.GnssAntennaInfos().ToString();
            bool hasAntennaInfos = gnssData.GnssCapabilitiesHasAntennaInfo();
            HasAntennaInfo = hasAntennaInfos ? "ja" : "nein";
            antennaInfos.IsVisible = hasAntennaInfos;
            if (hasAntennaInfos) {
               StringBuilder sb = new StringBuilder();
               for (int i = 0; i < gnssData.GnssAntennaInfos(); i++)
                  sb.AppendLine("CarrierFrequencyMHz (" + i + "): " + gnssData.GnssAntennaCarrierFrequencyMHz(i));
               antennaInfos.Text = sb.ToString();
            }
            HasMeasurements = gnssData.GnssCapabilitiesHasMeasurements() ? "ja" : "nein";
            HasNavigationMessages = gnssData.GnssCapabilitiesHasNavigationMessages() ? "ja" : "nein";
            // ----------------------------

            GnssStatusOn = gnssData.GnssStatusIsOn ? "ein" : "aus";
            GnssFixTime = gnssData.GnssFirstFixVar > 0 ? gnssData.GnssFirstFixVar / 1000F + "s" : "";

            DateTime now = DateTime.Now;
            if (now.Subtract(dtLast).TotalSeconds >= 5 && !IsBuildSatList) {
               IsBuildSatList = true;
               dtLast = now;
               status.Sort1Reverse();
               MainThread.BeginInvokeOnMainThread(() => {
                  int oldcount = satlst.Count;
                  for (int s = 0; s < status.Count; s++) {
                     if (s < oldcount)
                        satlst[s].NewSet(status.Sat[s]);
                     else
                        satlst.Add(new SatDataItem(status.Sat[s]));
                  }
                  int lines = oldcount - status.Count;
                  while (lines > 0) {
                     satlst.RemoveAt(satlst.Count - 1);
                     lines--;
                  }
                  IsBuildSatList = false;
               });
            }


            //status.Sort1();
            //StringBuilder statustxt = new StringBuilder();
            //statustxt.AppendLine("Satelliten: " + status.Count);
            //for (int s = 0; s < status.Count; s++) {
            //   statustxt.AppendLine("Sat " + s);
            //   statustxt.AppendLine("   " + status.Sat[s].ConstellationType + " " + status.Sat[s].SvID + ", UsedInFix=" + status.Sat[s].UsedInFix);
            //   statustxt.AppendLine("   Azimuth/Elevation " + status.Sat[s].AzimuthDegrees + "°/" + status.Sat[s].ElevationDegrees + "°");
            //   if (status.Sat[s].HasCarrierFrequencyHz)
            //      statustxt.AppendLine("   CarrierFreq " + (status.Sat[s].CarrierFrequencyHz / 1000000).ToString("f3") + " MHz");
            //   if (status.Sat[s].HasBasebandCn0DbHz)
            //      statustxt.AppendLine("   BasebandCn0 " + status.Sat[s].BasebandCn0DbHz + " DbHz");
            //   statustxt.AppendLine("   Cn0DbHz=" + status.Sat[s].Cn0DbHz + " dB");
            //   statustxt.AppendLine("   AlmanacData=" + status.Sat[s].HasAlmanacData + ", EphemerisData=" + status.Sat[s].HasEphemerisData);
            //}

            //MainThread.BeginInvokeOnMainThread(() => {
            //   if (gnsstxt.Text != statustxt.ToString()) {
            //      gnsstxt.Text = statustxt.ToString();
            //   }
            //});
         }
      }

   }
}
