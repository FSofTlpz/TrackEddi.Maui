using Android.Locations;
using Android.OS;

namespace TrackEddi.Gnns {
   /// <summary>
   /// GNSS-Infos: globale Navigationssatellitensysteme
   /// </summary>
   public class GnssInfo {

      LocationManager? lm = null;

      /// <summary>
      /// ab API 28
      /// </summary>
      public string GnssHardwareModelName => lm != null && System.OperatingSystem.IsAndroidVersionAtLeast(28) ?
         lm.GnssHardwareModelName ?? string.Empty : string.Empty;

      // Antenne

      /// <summary>
      /// ab API 31
      /// </summary>
      public int GnssAntennaInfos => lm != null && System.OperatingSystem.IsAndroidVersionAtLeast(31) && lm.GnssAntennaInfos != null ?
         lm.GnssAntennaInfos.Count : 0;

      /// <summary>
      /// ab API 30
      /// </summary>
      public double GnssAntennaCarrierFrequencyMHz(int no) =>
         lm != null &&
         System.OperatingSystem.IsAndroidVersionAtLeast(31) &&
         lm.GnssAntennaInfos != null && 0 <= no && no < GnssAntennaInfos ?
               lm.GnssAntennaInfos[no].CarrierFrequencyMHz :
               0;

      // nur ein Bruchteil (!) der Android GnssCapabilities:

      /// <summary>
      /// ab API 31
      /// </summary>
      public bool GnssCapabilitiesHasAntennaInfo => lm != null && System.OperatingSystem.IsAndroidVersionAtLeast(31) ?
         lm.GnssCapabilities.HasAntennaInfo : false;

      /// <summary>
      /// ab API 31
      /// </summary>
      public bool GnssCapabilitiesHasMeasurements => lm != null && System.OperatingSystem.IsAndroidVersionAtLeast(31) ?
         lm.GnssCapabilities.HasMeasurements : false;

      /// <summary>
      /// ab API 31
      /// </summary>
      public bool GnssCapabilitiesHasNavigationMessages => lm != null && System.OperatingSystem.IsAndroidVersionAtLeast(31) ?
         lm.GnssCapabilities.HasNavigationMessages : false;

      class MyGnssStatusCallback : GnssStatus.Callback {

         public event EventHandler? OnGnssStatusStart;

         public event EventHandler? OnGnssStatusEnd;

         public event EventHandler<int>? OnGnssFirstFix;

         public event EventHandler<GnssData.SatelliteStatus>? OnGnssStatusChanged;

         /// <summary>
         /// Called when the GNSS system has received its first fix since starting. (seit <see cref="OnStarted"/>!)
         /// </summary>
         /// <param name="ttffMillis">the time from start to first fix in milliseconds</param>
         public override void OnFirstFix(int ttffMillis) {
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
            // ab API 24
            base.OnFirstFix(ttffMillis);
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
            OnGnssFirstFix?.Invoke(this, ttffMillis);
         }

         /// <summary>
         /// Called periodically to report GNSS satellite status.
         /// </summary>
         /// <param name="status">the current status of all satellites</param>
         public override void OnSatelliteStatusChanged(GnssStatus status) {
            // ab API 24
            if (System.OperatingSystem.IsAndroidVersionAtLeast(24)) {
               base.OnSatelliteStatusChanged(status);

               if (OnGnssStatusChanged != null) {
                  GnssData.SatelliteStatus gnssStatus = new GnssData.SatelliteStatus(status.SatelliteCount);         // ab API 24
                  for (int i = 0; i < status.SatelliteCount; ++i) {
                     gnssStatus.Sat[i].SvID = status.GetSvid(i);                                 // ab API 24
                     gnssStatus.Sat[i].UsedInFix = status.UsedInFix(i);                          // ab API 24
                     gnssStatus.Sat[i].HasAlmanacData = status.HasAlmanacData(i);                // ab API 24
                     if (System.OperatingSystem.IsAndroidVersionAtLeast(30)) {
                        gnssStatus.Sat[i].HasBasebandCn0DbHz = status.HasBasebandCn0DbHz(i);        // ab API 30
                        gnssStatus.Sat[i].BasebandCn0DbHz = status.GetBasebandCn0DbHz(i);           // ab API 30
                     }
                     if (System.OperatingSystem.IsAndroidVersionAtLeast(26)) {
                        gnssStatus.Sat[i].HasCarrierFrequencyHz = status.HasCarrierFrequencyHz(i);  // ab API 26
                        gnssStatus.Sat[i].CarrierFrequencyHz = status.GetCarrierFrequencyHz(i);     // ab API 26
                     }
                     gnssStatus.Sat[i].HasEphemerisData = status.HasEphemerisData(i);            // ab API 24
                     gnssStatus.Sat[i].AzimuthDegrees = status.GetAzimuthDegrees(i);             // ab API 24
                     gnssStatus.Sat[i].ElevationDegrees = status.GetElevationDegrees(i);         // ab API 24
                     gnssStatus.Sat[i].Cn0DbHz = status.GetCn0DbHz(i);                           // ab API 24

                     switch (status.GetConstellationType(i)) {                               // ab API 24
                        case GnssConstellationType.Gps:
                           gnssStatus.Sat[i].ConstellationType = GnssData.SatelliteStatus.ConstellationType.Gps;
                           break;

                        case GnssConstellationType.Beidou:
                           gnssStatus.Sat[i].ConstellationType = GnssData.SatelliteStatus.ConstellationType.Beidou;
                           break;

                        case GnssConstellationType.Galileo:
                           gnssStatus.Sat[i].ConstellationType = GnssData.SatelliteStatus.ConstellationType.Galileo;
                           break;

                        case GnssConstellationType.Glonass:
                           gnssStatus.Sat[i].ConstellationType = GnssData.SatelliteStatus.ConstellationType.Glonass;
                           break;

                        case GnssConstellationType.Qzss:
                           gnssStatus.Sat[i].ConstellationType = GnssData.SatelliteStatus.ConstellationType.Qzss;
                           break;

                        case GnssConstellationType.Sbas:
                           gnssStatus.Sat[i].ConstellationType = GnssData.SatelliteStatus.ConstellationType.Sbas;
                           break;

                        default:
                           gnssStatus.Sat[i].ConstellationType = GnssData.SatelliteStatus.ConstellationType.Unknown;
                           break;
                     }

                     if (System.OperatingSystem.IsAndroidVersionAtLeast(29)) {
                        switch (status.GetConstellationType(i)) {
                           case GnssConstellationType.Irnss:
                              gnssStatus.Sat[i].ConstellationType = GnssData.SatelliteStatus.ConstellationType.Irnss;
                              break;

                        }
                     }

                  }
                  OnGnssStatusChanged?.Invoke(this, gnssStatus);
               }
            }
         }

         /// <summary>
         /// Called when GNSS system has started.
         /// </summary>
         public override void OnStarted() {
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
            base.OnStarted();    // ab API 24
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
            OnGnssStatusStart?.Invoke(this, EventArgs.Empty);
         }

         /// <summary>
         /// Called when GNSS system has stopped.
         /// </summary>
         public override void OnStopped() {
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
            base.OnStopped();    // ab API 24
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
            OnGnssStatusEnd?.Invoke(this, EventArgs.Empty);
         }

      }

      /// <summary>
      /// Event sent when the GPS system has started. 
      /// </summary>
      public event EventHandler? OnGnssStatusStart;

      /// <summary>
      /// Event sent when the GPS system has stopped. 
      /// </summary>
      public event EventHandler? OnGnssStatusEnd;

      /// <summary>
      /// Event sent when the GPS system has received its first fix since starting. 
      /// <para>Time to first fix (TTFF) is a measure of the time required for a GPS navigation device to acquire 
      /// satellite signals and navigation data, and calculate a position solution (called a fix).</para>
      /// </summary>
      public event EventHandler<int>? OnGnssFirstFix;

      /// <summary>
      /// Event sent periodically to report GPS satellite status. 
      /// </summary>
      public event EventHandler<GnssData.SatelliteStatus>? OnGnssStatusChanged;

      MyGnssStatusCallback? gnssstatuscb = null;


      public GnssInfo(LocationManager locationManager) => lm = locationManager;

      /// <summary>
      /// startet oder beendet die GNSS-Beobachtung
      /// </summary>
      /// <param name="on"></param>
      public void SetGnssStatus(bool on) {
         if (lm != null) {
            if (on && gnssstatuscb == null) {
               gnssstatuscb = new MyGnssStatusCallback();
               Handler? h1 = null;

               if (OnGnssStatusStart != null)
                  gnssstatuscb.OnGnssStatusStart += OnGnssStatusStart;
               if (OnGnssStatusEnd != null)
                  gnssstatuscb.OnGnssStatusEnd += OnGnssStatusEnd;
               if (OnGnssFirstFix != null)
                  gnssstatuscb.OnGnssFirstFix += OnGnssFirstFix;
               if (OnGnssStatusChanged != null)
                  gnssstatuscb.OnGnssStatusChanged += OnGnssStatusChanged;

#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
               lm.RegisterGnssStatusCallback(gnssstatuscb, h1);         // ab API 24
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
            }
            if (!on && gnssstatuscb != null) {
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
               lm.RegisterGnssStatusCallback(gnssstatuscb, null);       // ab API 24
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
               gnssstatuscb.Dispose();
               gnssstatuscb = null;
            }
         }
      }

   }
}