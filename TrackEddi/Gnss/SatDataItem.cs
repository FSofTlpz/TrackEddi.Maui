using System.ComponentModel;
using static TrackEddi.Gnns.GnssData;

namespace TrackEddi.Gnss {
   public class SatDataItem : INotifyPropertyChanged {

      public event PropertyChangedEventHandler? PropertyChanged;


      /// <summary>
      /// Satelliten-ID 
      /// <para>This svid is pseudo-random number for most constellations. It is FCN and OSN number for Glonass. </para>
      /// <para>GPS: 1-32</para>
      /// <para>SBAS: 120-151, 183-192</para>
      /// <para>GLONASS: One of: OSN, or FCN+100 (1-25 as the orbital slot number (OSN) (preferred, if known); 
      /// 93-106 as the frequency channel number (FCN) (-7 to +6) plus 100. i.e. encode FCN of -7 as 93, 0 as 100, and +6 as 106</para>
      /// <para>QZSS: 183-206</para>
      /// <para>Galileo: 1-36</para>
      /// <para>Beidou: 1-63</para>
      /// <para>IRNSS: 1-14</para>
      /// </summary>
      public int SvID {
         get => _SvID;
         set { if (_SvID != value) { _SvID = value; Notify4PropChanged(nameof(SvID)); } }
      }
      /// <summary>
      /// true if the satellite was used by the GPS engine when calculating the most recent GPS fix.
      /// </summary>
      public bool UsedInFix {
         get => _UsedInFix;
         set {
            if (_UsedInFix != value) { _UsedInFix = value; Notify4PropChanged(nameof(UsedInFix)); }
         }
      }

      /// <summary>
      /// true if the GPS engine has almanac data for the satellite.
      /// </summary>
      public bool HasAlmanacData {
         get => _HasAlmanacData;
         set {
            if (_HasAlmanacData != value) { _HasAlmanacData = value; Notify4PropChanged(nameof(HasAlmanacData)); }
         }
      }
      /// <summary>
      /// true if the GPS engine has ephemeris data for the satellite.
      /// </summary>
      public bool HasEphemerisData {
         get => _HasEphemerisData;
         set {
            if (_HasEphemerisData != value) { _HasEphemerisData = value; Notify4PropChanged(nameof(HasEphemerisData)); }
         }
      }
      /// <summary>
      /// Reports whether a valid getBasebandCn0DbHz(int) is available. (>= API30)
      /// </summary>
      public bool HasBasebandCn0DbHz {
         get => _HasBasebandCn0DbHz;
         set {
            if (_HasBasebandCn0DbHz != value) { _HasBasebandCn0DbHz = value; Notify4PropChanged(nameof(HasBasebandCn0DbHz)); }
         }
      }
      /// <summary>
      /// Reports whether a valid getCarrierFrequencyHz(int) is available. (>= API26)
      /// </summary>
      public bool HasCarrierFrequencyHz {
         get => _HasCarrierFrequencyHz;
         set {
            if (_HasCarrierFrequencyHz != value) { _HasCarrierFrequencyHz = value; Notify4PropChanged(nameof(HasCarrierFrequencyHz)); }
         }
      }

      /// <summary>
      /// azimuth/elevation of the satellite in degrees
      /// </summary>
      public string Position {
         get => _Position;
         set {
            if (_Position != value) { _Position = value; Notify4PropChanged(nameof(Position)); }
         }
      }
      /// <summary>
      /// carrier frequency of the signal; (>= API26)
      /// For example it can be the GPS central frequency for L1 = 1575.45 MHz, or L2 = 1227.60 MHz, 
      /// L5 = 1176.45 MHz, varying GLO channels, etc. 
      /// </summary>
      public string CarrierFrequencyHz {
         get => _CarrierFrequencyHz;
         set {
            if (_CarrierFrequencyHz != value) { _CarrierFrequencyHz = value; Notify4PropChanged(nameof(CarrierFrequencyHz)); }
         }
      }
      /// <summary>
      /// Retrieves the baseband carrier-to-noise density of the satellite at the specified index in dB-Hz. (>= API30)
      /// </summary>
      public string BasebandCn0DbHz {
         get => _BasebandCn0DbHz;
         set {
            if (_BasebandCn0DbHz != value) { _BasebandCn0DbHz = value; Notify4PropChanged(nameof(BasebandCn0DbHz)); }
         }
      }
      /// <summary>
      /// Retrieves the carrier-to-noise density at the antenna of the satellite at the specified index in dB-Hz.
      /// </summary>
      public string Cn0DbHz {
         get => _Cn0DbHz;
         set {
            if (_Cn0DbHz != value) { _Cn0DbHz = value; Notify4PropChanged(nameof(Cn0DbHz)); }
         }
      }

      /// <summary>
      /// Retrieves the constellation type of the satellite at the specified index.
      /// </summary>
      public string ConstellationType {
         get => _ConstellationType;
         set {
            if (_ConstellationType != value) { _ConstellationType = value; Notify4PropChanged(nameof(ConstellationType)); }
         }
      }

      int _SvID;
      bool _UsedInFix;
      bool _HasAlmanacData;
      bool _HasEphemerisData;
      bool _HasBasebandCn0DbHz;
      bool _HasCarrierFrequencyHz;
      string _Position = string.Empty;
      string _ElevationDegrees = string.Empty;
      string _CarrierFrequencyHz = string.Empty;
      string _BasebandCn0DbHz = string.Empty;
      string _Cn0DbHz = string.Empty;
      string _ConstellationType = SatelliteStatus.ConstellationType.Unknown.ToString();


      public SatDataItem() { }

      public SatDataItem(SatelliteStatus.SatItem sat) {
         _SvID = sat.SvID;
         _UsedInFix = sat.UsedInFix;
         _HasAlmanacData = sat.HasAlmanacData;
         _HasEphemerisData = sat.HasEphemerisData;
         //if (sat.HasEphemerisData)
         _Position = sat.AzimuthDegrees.ToString() + "°/" + sat.ElevationDegrees.ToString() + "°";
         if (sat.HasBasebandCn0DbHz)
            _Cn0DbHz = sat.Cn0DbHz.ToString(); // + " dB";
         _HasCarrierFrequencyHz = sat.HasCarrierFrequencyHz;
         if (sat.HasCarrierFrequencyHz)
            _CarrierFrequencyHz = (sat.CarrierFrequencyHz / 1000000).ToString("f3"); // + " MHz";
         _HasBasebandCn0DbHz = sat.HasBasebandCn0DbHz;
         if (sat.HasBasebandCn0DbHz)
            _BasebandCn0DbHz = sat.BasebandCn0DbHz.ToString(); // + " DbHz";
         _ConstellationType = sat.ConstellationType.ToString();
      }

      public void NewSet(SatelliteStatus.SatItem sat) {
         SvID = sat.SvID;
         UsedInFix = sat.UsedInFix;
         HasAlmanacData = sat.HasAlmanacData;
         HasEphemerisData = sat.HasEphemerisData;
         //if (sat.HasEphemerisData)
         Position = sat.AzimuthDegrees.ToString() + "°/" + sat.ElevationDegrees.ToString() + "°";
         if (sat.HasBasebandCn0DbHz)
            Cn0DbHz = sat.Cn0DbHz.ToString(); // + " dBHz";
         HasCarrierFrequencyHz = sat.HasCarrierFrequencyHz;
         if (sat.HasCarrierFrequencyHz)
            CarrierFrequencyHz = (sat.CarrierFrequencyHz / 1000000).ToString("f3"); // + " MHz";
         HasBasebandCn0DbHz = sat.HasBasebandCn0DbHz;
         if (sat.HasBasebandCn0DbHz)
            BasebandCn0DbHz = sat.BasebandCn0DbHz.ToString(); // + " dBHz";
         ConstellationType = sat.ConstellationType.ToString();
      }

      /// <summary>
      /// zum Auslösen eines <see cref="PropertyChanged"/>-Events (auch "extern")
      /// </summary>
      /// <param name="propname"></param>
      public void Notify4PropChanged(string propname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));

   }
}
