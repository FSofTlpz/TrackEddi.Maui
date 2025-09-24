using Android.Content;
#if FUSEDLOCATION
using Android.Gms.Common;
#endif

namespace TrackEddi.Gnns {
   public partial class GnssData {

      GnssInfo? gnssInfo;

      bool gnssStart() {
         gnssEnd();
         Android.Locations.LocationManager? lm =
            (Android.Locations.LocationManager?)Android.App.Application.Context.GetSystemService(Context.LocationService);
         gnssInfo = lm != null ? new GnssInfo(lm) : null;
         if (gnssInfo != null) {
            gnssInfo.OnGnssStatusChanged += GnssInfo_OnGnssStatusChanged;
            gnssInfo.OnGnssFirstFix += GnssInfo_OnGnssFirstFix;
            gnssInfo.OnGnssStatusStart += GnssInfo_OnGnssStatusStart;
            gnssInfo.OnGnssStatusEnd += GnssInfo_OnGnssStatusEnd;
         }
         return gnssInfo != null;
      }

      void gnssEnd() {
         if (gnssInfo != null) {
            gnssInfo.SetGnssStatus(false);
            gnssInfo.OnGnssStatusChanged -= GnssInfo_OnGnssStatusChanged;
            gnssInfo.OnGnssFirstFix -= GnssInfo_OnGnssFirstFix;
            gnssInfo.OnGnssStatusStart -= GnssInfo_OnGnssStatusStart;
            gnssInfo.OnGnssStatusEnd -= GnssInfo_OnGnssStatusEnd;
            gnssInfo = null;
         }
      }

      private void GnssInfo_OnGnssStatusStart(object? sender, EventArgs e) =>
         GnssStatusStart?.Invoke(this, EventArgs.Empty);

      private void GnssInfo_OnGnssStatusEnd(object? sender, EventArgs e) =>
         GnssStatusEnd?.Invoke(this, EventArgs.Empty);

      private void GnssInfo_OnGnssFirstFix(object? sender, int e) =>
         GnssFirstFix?.Invoke(this, e);

      private void GnssInfo_OnGnssStatusChanged(object? sender, SatelliteStatus e) =>
          GnssStatusChanged?.Invoke(this, e);


      /// <summary>
      /// ab Android 9
      /// </summary>
      /// <returns></returns>
      string gnssHardwareModelName() => gnssInfo != null ? gnssInfo.GnssHardwareModelName : string.Empty;

      /// <summary>
      /// ab Android 12
      /// </summary>
      /// <returns></returns>
      int gnssAntennaInfos() => gnssInfo != null ? gnssInfo.GnssAntennaInfos : 0;

      /// <summary>
      /// ab Android 11
      /// </summary>
      /// <returns></returns>
      double gnssAntennaCarrierFrequencyMHz(int no) => gnssInfo != null ? gnssInfo.GnssAntennaCarrierFrequencyMHz(no) : 0;

      /// <summary>
      /// ab Android 12
      /// </summary>
      /// <returns></returns>
      bool gnssCapabilitiesHasAntennaInfo() => gnssInfo != null && gnssInfo.GnssCapabilitiesHasAntennaInfo;

      /// <summary>
      /// ab Android 12
      /// </summary>
      /// <returns></returns>
      bool gnssCapabilitiesHasMeasurements() => gnssInfo != null && gnssInfo.GnssCapabilitiesHasMeasurements;

      /// <summary>
      /// ab Android 12
      /// </summary>
      /// <returns></returns>
      bool gnssCapabilitiesHasNavigationMessages() => gnssInfo != null && gnssInfo.GnssCapabilitiesHasNavigationMessages;

      void setGnssStatus(bool on) => gnssInfo?.SetGnssStatus(on);

   }
}
