//#define FUSEDLOCATION

using Android.Content;

#if FUSEDLOCATION
using Android.Gms.Location;
#else
using Android.OS;
#endif
using Android.Locations;

namespace TrackEddi {

   /// <summary>
   /// zur Ermittlung der akt. Position
   /// <para>Sowohl der LocationManager von Android als auch Xamarin.Essentials.Location liefern z.B. (auch in einem Service) keine Daten mehr, 
   /// wenn die App nicht mehr auf dem Bilschirm sichtbar ist. Aktuell scheint der FusedLocationProviderClient aus den GooglePlayServices
   /// die einzige Möglichkeit zu bieten, ein Tracking auch im Hintergund zu ermöglichen.</para>
   /// </summary>
   internal class LocationTracker
#if !FUSEDLOCATION
                                  : Java.Lang.Object, ILocationListener
#endif
      {

      /// <summary>
      /// ungültiger Wert
      /// </summary>
      public const double NOTVALID_DOUBLE = double.MinValue;

      public event EventHandler<GeoLocationServiceCtrl.LocationChangedArgs>? LocationChanged;

#if FUSEDLOCATION
      FusedLocationProviderClient fusedLocationProviderClient;

      LocationCallback locationCallback = null;
#else
      public readonly LocationManager? LocationManager = null;
#endif

      bool isActiv = false;


      public LocationTracker(Context context) {
#if FUSEDLOCATION
         fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(context);
#else
         LocationManager = (LocationManager?)context.GetSystemService(Context.LocationService);
         //IList<string> acceptableLocationProviders = locationManager.GetProviders(false);
#endif
      }

      /// <summary>
      /// akt. das Tracking (vermutlich sind die beiden Bedingungen OR-verknüpft)
      /// <para>
      /// Requests location updates with the given request and results delivered to the given 
      /// callback on the specified Looper. A previous request for location updates for the same 
      /// callback will be replaced by this request. If the location request has a priority higher 
      /// than Priority.PRIORITY_PASSIVE, a wakelock may be held on the client's behalf while 
      /// delivering locations. A wakelock will not be held while delivering availability updates.
      /// </para>
      /// <para>
      /// The frequency of notification may be controlled using the
      /// minTime and minDistance parameters.If minTime is greater than 0,
      /// the LocationManager could potentially rest for minTime milliseconds
      /// between location updates to conserve power. If minDistance is greater than 0,
      /// a location will only be broadcasted if the device moves by minDistance meters.
      /// To obtain notifications as frequently as possible, set both parameters to 0.
      /// </para>
      /// </summary>
      /// <param name="intervalmillies">minTime the minimum time interval for notifications, in milliseconds. 
      /// This field is only used as a hint to conserve power, and actual time between location updates may be 
      /// greater or lesser than this value.</param>
      /// <param name="mindistance">minDistance the minimum distance interval for notifications in meters</param>
      public void ActivateTracking(long intervalmillies = 1000, float mindistance = 1) {

#if FUSEDLOCATION
         if (fusedLocationProviderClient != null) {
            if (isActiv)
               DeactivateTracking();

            Android.Gms.Location.LocationRequest req = Android.Gms.Location.LocationRequest.Create();
            req.SetPriority(Priority.PriorityHighAccuracy);
            //req.SetPriority(Priority.PriorityBalancedPowerAccuracy);
            req.SetInterval(intervalmillies);
            req.SetSmallestDisplacement(mindistance);

            locationCallback = new LocationCallback();
            locationCallback.LocationResult += locationCallback_LocationResult;
            fusedLocationProviderClient.RequestLocationUpdates(req, locationCallback, null);

            isActiv = true;
         }
#else
         if (LocationManager != null) {
            if (isActiv)
               DeactivateTracking();

            /*

               PRIORITY_BALANCED_POWER_ACCURACY - Requests a tradeoff that is balanced between location accuracy and power usage.
               PRIORITY_HIGH_ACCURACY - Requests a tradeoff that favors highly accurate locations at the possible expense of additional power usage.
               PRIORITY_LOW_POWER - Requests a tradeoff that favors low power usage at the possible expense of location accuracy.
               PRIORITY_PASSIVE - Ensures that no extra power will be used to derive locations. This enforces that the request will act as a passive listener that will only receive "free" locations calculated on behalf of other clients, and no locations will be calculated on behalf of only this request.


               LocationManager.FusedProvider
                     Standard name of the fused location provider.
                     If present, this provider may combine inputs from several other location providers to provide the best possible location fix. 
                     It is implicitly used for all requestLocationUpdates APIs that involve a Criteria.
                     Constant Value: "fused"                
            
               LocationManager.GpsProvider
                     Standard name of the GNSS location provider.
                     If present, this provider determines location using GNSS satellites. The responsiveness and accuracy of location fixes may depend 
                     on GNSS signal conditions.
                     Locations returned from this provider are with respect to the primary GNSS antenna position within the device. getGnssAntennaInfos() may be used 
                     to determine the GNSS antenna position with respect to the Android Coordinate System, and convert between them if necessary. This is generally 
                     only necessary for high accuracy applications.
                     The extras Bundle for locations derived by this location provider may contain the following key/value pairs:
                         satellites - the number of satellites used to derive the fix 
                     Constant Value: "gps" 

               LocationManager.NetworkProvider
                     Standard name of the network location provider.
                     If present, this provider determines location based on nearby of cell tower and WiFi access points. 
                     Operation of this provider may require a data connection.
                     Constant Value: "network"                
            
               LocationManager.PassiveProvider      A special location provider for receiving locations without actively initiating a location fix.
                     A special location provider for receiving locations without actively initiating a location fix. This location provider is always present.
                     This provider can be used to passively receive location updates when other applications or services request them without actually requesting 
                     the locations yourself. This provider will only return locations generated by other providers.
                     Constant Value: "passive" 
             */
            /*
            Prior to Jellybean, the minTime parameter was only a hint, and some location provider implementations ignored it. 
            For Jellybean and onwards however, it is mandatory for Android compatible devices to observe both 
            the minTime and minDistance parameters.
            Requires Manifest.permission.ACCESS_COARSE_LOCATION or Manifest.permission.ACCESS_FINE_LOCATION


            Only one request can be registered for each unique listener/provider pair, so any subsequent requests with the 
            same provider and listener will overwrite all associated arguments. The same listener may be used across 
            multiple providers with different requests for each provider.

            It may take some time to receive the first location update depending on the conditions the device finds itself in. 
            ...

            See LocationRequest documentation for an explanation of various request parameters and how they can affect the
            received locations.

            If your application wants to passively observe location updates from all providers, then use the PASSIVE_PROVIDER. 
            This provider does not turn on or modify active location providers, so you do not need to be as careful about 
            minimum time and minimum distance parameters. However, if your application performs heavy work on a location update 
            (such as network activity) then you should set an explicit fastest interval on your location request in case 
            another application enables a location provider with extremely fast updates.

            In case the provider you have selected is disabled, location updates will cease, and a provider availability update 
            will be sent. As soon as the provider is enabled again, another provider availability update will be sent and 
            location updates will resume.

            Locations returned from GPS_PROVIDER are with respect to the primary GNSS antenna position within the device. 
            getGnssAntennaInfos() may be used to determine the GNSS antenna position with respect to the 
            Android Coordinate System, and convert between them if necessary. This is generally only necessary for 
            high accuracy applications. 
            ...
             */
            LocationManager.RequestLocationUpdates(
               LocationManager.NetworkProvider,
               intervalmillies,
               mindistance,
               this);

            LocationManager.RequestLocationUpdates(
               LocationManager.GpsProvider,
               intervalmillies,                 // minimum time interval between location updates in milliseconds
               mindistance,                     // minimum distance between location updates in meters
               this);

            isActiv = true;
         }
#endif
      }

      /// <summary>
      /// stopt das Tracking
      /// </summary>
      public void DeactivateTracking() {
         if (isActiv) {
#if FUSEDLOCATION
            fusedLocationProviderClient.RemoveLocationUpdates(locationCallback);
            locationCallback.Dispose();
            locationCallback = null;
#else
            LocationManager?.RemoveUpdates(this);
#endif
            isActiv = false;
         }
      }

#if FUSEDLOCATION
      void locationCallback_LocationResult(object sender, LocationCallbackResultEventArgs e) {
         if (e.Result != null)
            foreach (var item in e.Result.Locations)
               LocationChanged?.Invoke(this, Convert(item));
      }
#endif

      /*
      Accuracy          
         the estimated horizontal accuracy radius in meters of this location at the 68th percentile confidence level
         This means that there is a 68% chance that the true location of the device is within a distance 
         of this uncertainty of the reported location. Another way of putting this is that if a circle 
         with a radius equal to this accuracy is drawn around the reported location, there is a 68% chance 
         that the true location falls within this circle.

      VerticalAccuracyMeters  
         analog

      Altitude
         The altitude of this location in meters above the WGS84 reference ellipsoid. 

      Bearing
         Returns the bearing at the time of this location in degrees. Bearing is the horizontal direction of travel 
         of this device and is unrelated to the device orientation. The bearing is guaranteed to be in the range [0, 360). 

      BearingAccuracyDegrees
         Returns the estimated bearing accuracy in degrees of this location at the 68th percentile confidence level. 
         This means that there is 68% chance that the true bearing at the time of this location falls within 
         getBearing() ()} +/- this uncertainty. 

      MslAltitudeMeters
         Returns the Mean Sea Level altitude of this location in meters. 

      MslAltitudeAccuracyMeters
         Returns the estimated Mean Sea Level altitude accuracy in meters of this location at the 68th percentile 
         confidence level. This means that there is 68% chance that the true Mean Sea Level altitude of this location 
         falls within getMslAltitudeMeters() +/- this uncertainty. 
   
      Speed
         the speed at the time of this location in meters per second. Note that the speed returned here may be 
         more accurate than would be obtained simply by calculating distance / time for sequential positions, 
         such as if the Doppler measurements from GNSS satellites are taken into account. 

      SpeedAccuracyMetersPerSecond
         Returns the estimated speed accuracy in meters per second of this location at the 68th percentile confidence level.

       */


#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
      static Microsoft.Maui.Devices.Sensors.Location convert(Android.Locations.Location location) =>
         new Microsoft.Maui.Devices.Sensors.Location(location.Latitude, location.Longitude) {
            Altitude = location.HasAltitude && location.Altitude != double.MaxValue ? location.Altitude : NOTVALID_DOUBLE,
            Course = location.HasBearing ? location.Bearing : NOTVALID_DOUBLE,
            Speed = location.HasSpeed && location.Speed != double.MaxValue ? location.Speed : NOTVALID_DOUBLE,
            Accuracy = location.HasAccuracy ? location.Accuracy : NOTVALID_DOUBLE,
            VerticalAccuracy = location.HasVerticalAccuracy ? location.VerticalAccuracyMeters : NOTVALID_DOUBLE,  // ab API 26
            Timestamp = System.DateTimeOffset.FromUnixTimeMilliseconds(location.Time),
         };
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen

#if !FUSEDLOCATION
      #region Interface Android.Locations.ILocationListener 

      Android.Locations.Location? lastLoction = null;


      public void OnProviderDisabled(string provider) { }

      public void OnProviderEnabled(string provider) { }

      void ILocationListener.OnStatusChanged(string? provider, Availability status, Bundle? extras) { }

      void ILocationListener.OnLocationChanged(Android.Locations.Location location) {
         if (lastLoction != null)
            if (location.Time == lastLoction.Time &&
                location.Longitude == lastLoction.Longitude &&
                location.Longitude == lastLoction.Longitude &&
                location.Provider == lastLoction.Provider)
               return;
         lastLoction = location;

         LocationChanged?.Invoke(this,
                                 new GeoLocationServiceCtrl.LocationChangedArgs() {
                                    Location = convert(location),
                                    Provider = location.Provider ?? string.Empty,
                                 });
      }

      #endregion
#endif

   }
}