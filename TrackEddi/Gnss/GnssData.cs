namespace TrackEddi.Gnns {
   public partial class GnssData {

      public event EventHandler? GnssStatusStart;
      public event EventHandler? GnssStatusEnd;
      public event EventHandler<int>? GnssFirstFix;
      public event EventHandler<SatelliteStatus>? GnssStatusChanged;

      /// <summary>
      /// Daten je Satellit (i.A. für Android ab >= API24)
      /// </summary>
      public class SatelliteStatus {

         /// <summary>
         /// Satellitentyp
         /// </summary>
         public enum ConstellationType {
            Unknown,
            Gps,
            Sbas,
            Glonass,
            Qzss,
            Beidou,
            Galileo,
            Irnss,
         }

         // https://de.wikipedia.org/wiki/GNSS-Navigation

         public class SatItem {

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
            public int SvID;
            /// <summary>
            /// true if the satellite was used by the GPS engine when calculating the most recent GPS fix.
            /// </summary>
            public bool UsedInFix;

            /// <summary>
            /// true if the GPS engine has almanac data for the satellite.
            /// <para>
            /// Als Almanach bezeichnet man bei einem Globalen Navigationssatellitensystem (GNSS, z. B. GPS) eine Liste 
            /// mit Bahndaten der Satelliten, die weniger genau, aber länger gültig sind als die Ephemeriden 
            /// (ergänzt um Informationen über die Integrität der ausgestrahlten Signale).
            /// </para>
            /// <para>
            /// Die Almanach-Daten sind nicht notwendig, aber nützlich, denn sie beschleunigen beim Einschalten des Empfängers 
            /// die Suche nach den Satellitensignalen, indem der zu durchsuchende Bereich eingegrenzt wird.
            /// Dabei geht es nicht um eine Suche der Satelliten am Himmel, sondern im zweidimensionalen Raum der Frequenzabweichung. 
            /// Dieser ergibt sich einerseits durch den Dopplereffekt und andererseits durch die zeitliche Verschiebung zwischen 
            /// gesendeten und im Empfänger generierten pseudozufälligen Codefolgen.
            /// </para>
            /// <para>
            /// Des Weiteren wird der Almanach, der auch im Internet zur Verfügung steht, zur Planung von GNSS-Sessions benutzt, 
            /// z.B.um zu ermitteln, ob und wann auf einem Punkt der Erdoberfläche besonders gute Ergebnisse erzielt werden können.
            /// </para>
            /// <para>
            /// Jeder Satellit sendet neben den zur Verwendung seiner Signale nötigen hochgenauen eigenen Bahndaten, 
            /// den Ephemeriden, auch die Almanach-Daten aller Satelliten des Systems, allerdings in einem längeren Zyklus.
            /// Moderne GNSS-Empfänger speichern diese Daten und verringern so die Zeit, bis die erste Positionsbestimmung 
            /// verfügbar ist. Der Vorteil hängt ab von der Dauer, in der das Gerät keinen Empfang hatte. 
            /// </para>
            /// </summary>
            public bool HasAlmanacData;
            /// <summary>
            /// true if the GPS engine has ephemeris data for the satellite.
            /// <para>
            /// eigener Standort des Satelliten
            /// </para>
            /// <para>
            /// Die Ephemeriden (von altgriechisch ἐφήμερος ephḗmeros „für einen Tag“, aus ἐπί epi „auf“ und ἡμέρα hēméra „Tag“) 
            /// sind die Positionswerte sich bewegender astronomischer Objekte bezogen auf ein jeweils zweckmäßiges astronomisches 
            /// Koordinatensystem. Ihr Name drückt aus, dass solche Positionsangaben in der Regel jeweils für einen Tag gemacht 
            /// werden.
            /// </para>
            /// </summary>
            public bool HasEphemerisData;
            /// <summary>
            /// Reports whether a valid getBasebandCn0DbHz(int) is available. (>= API30)
            /// </summary>
            public bool HasBasebandCn0DbHz;
            /// <summary>
            /// Reports whether a valid getCarrierFrequencyHz(int) is available. (>= API26)
            /// </summary>
            public bool HasCarrierFrequencyHz;

            /// <summary>
            /// azimuth of the satellite in degrees
            /// </summary>
            public float AzimuthDegrees;
            /// <summary>
            /// elevation of the satellite in degrees
            /// </summary>
            public float ElevationDegrees;
            /// <summary>
            /// carrier frequency of the signal; (>= API26)
            /// For example it can be the GPS central frequency for L1 = 1575.45 MHz, or L2 = 1227.60 MHz, 
            /// L5 = 1176.45 MHz, varying GLO channels, etc. 
            /// </summary>
            public float CarrierFrequencyHz;
            /// <summary>
            /// Retrieves the baseband carrier-to-noise density of the satellite at the specified index in dB-Hz. (>= API30)
            /// </summary>
            public float BasebandCn0DbHz;
            /// <summary>
            /// Retrieves the carrier-to-noise density at the antenna of the satellite at the specified index in dB-Hz.
            /// </summary>
            public float Cn0DbHz;

            /// <summary>
            /// Retrieves the constellation type of the satellite at the specified index.
            /// </summary>
            public ConstellationType ConstellationType;


            public SatItem() { }

            public SatItem(
                                 int SvID,
                                 bool UsedInFix,
                                 bool HasAlmanacData,
                                 bool HasEphemerisData,
                                 bool HasBasebandCn0DbHz,
                                 bool HasCarrierFrequencyHz,
                                 float AzimuthDegrees,
                                 float ElevationDegrees,
                                 float CarrierFrequencyHz,
                                 float BasebandCn0DbHz,
                                 float Cn0DbHz,
                                 ConstellationType ConstellationType) {

               this.SvID = SvID;
               this.UsedInFix = UsedInFix;
               this.HasAlmanacData = HasAlmanacData;
               this.HasEphemerisData = HasEphemerisData;
               this.HasBasebandCn0DbHz = HasBasebandCn0DbHz;
               this.HasCarrierFrequencyHz = HasCarrierFrequencyHz;
               this.AzimuthDegrees = AzimuthDegrees;
               this.ElevationDegrees = ElevationDegrees;
               this.CarrierFrequencyHz = CarrierFrequencyHz;
               this.BasebandCn0DbHz = BasebandCn0DbHz;
               this.Cn0DbHz = Cn0DbHz;
               this.ConstellationType = ConstellationType;
            }

         }

         public readonly int Count;

         public readonly SatItem[] Sat;


         public SatelliteStatus(int satelliteCount) {
            Count = satelliteCount;
            Sat = new SatItem[Count];
            for (int i = 0; i < Count; ++i)
               Sat[i] = new SatItem();
         }

         public SatelliteStatus(SatelliteStatus org) : this(org.Count) => Array.Copy(org.Sat, Sat, Count);

         public void Sort0() => Array.Sort(Sat, Comparer0);

         public void Sort1() => Array.Sort(Sat, Comparer1);

         public void Sort1Reverse() => Array.Sort(Sat, Comparer1reverse);

         static int Comparer1(SatItem x, SatItem y) {
            // UsedInFix, Cn0DbHz, SvID
            if (x == null) {
               if (y == null)    // If x is null and y is null, they're equal.
                  return 0;
               else              // If x is null and y is not null, y is greater.
                  return -1;
            } else {             // If x is not null...
               if (y == null)    // ...and y is null, x is greater.
                  return 1;
               else {

                  // Standard
                  if (x.UsedInFix == y.UsedInFix)
                     if (x.Cn0DbHz == y.Cn0DbHz)
                        return x.SvID > y.SvID ? 1 : x.SvID < y.SvID ? -1 : 0;
                     else return x.Cn0DbHz > y.Cn0DbHz ? 1 : x.Cn0DbHz < y.Cn0DbHz ? -1 : 0;
                  else return x.UsedInFix ? 1 : y.UsedInFix ? -1 : 0;

               }
            }
         }

         static int Comparer1reverse(SatItem x, SatItem y) => -Comparer1(x, y);

         static int Comparer0(SatItem x, SatItem y) {
            // ConstellationType, SvID
            if (x == null) {
               if (y == null)    // If x is null and y is null, they're equal.
                  return 0;
               else              // If x is null and y is not null, y is greater.
                  return -1;
            } else {             // If x is not null...
               if (y == null)    // ...and y is null, x is greater.
                  return 1;
               else {

                  // Standard
                  if (x.ConstellationType == y.ConstellationType)
                     if (x.SvID == y.SvID)
                        return 0;
                     else return x.SvID > y.SvID ? 1 : -1;
                  else return (int)x.ConstellationType > (int)y.ConstellationType ? 1 : -1;

               }
            }
         }
      }



      public GnssData() {

      }

      public bool GnssInit() => gnssStart();

      public void GnssDeinit() => gnssEnd();

      public string GnssHardwareModelName() => gnssHardwareModelName();

      // Antenne

      public int GnssAntennaInfos() => gnssAntennaInfos();

      public double GnssAntennaCarrierFrequencyMHz(int no) => gnssAntennaCarrierFrequencyMHz(no);

      // nur ein Bruchteil (!) der Android GnssCapabilities:

      public bool GnssCapabilitiesHasAntennaInfo() => gnssCapabilitiesHasAntennaInfo();

      public bool GnssCapabilitiesHasMeasurements() => gnssCapabilitiesHasMeasurements();

      public bool GnssCapabilitiesHasNavigationMessages() => gnssCapabilitiesHasNavigationMessages();

      // Status

      /// <summary>
      /// startet oder beendet die GNSS-Beobachtung
      /// </summary>
      /// <param name="on"></param>
      public void SetGnssStatus(bool on) => setGnssStatus(on);

      public bool RegisterGnss() {
         if (GnssInit()) {
            GnssStatusStart += (s, e) => {
               GnssStatusIsOn = true;
               OnGnssStatusStart?.Invoke(this, e);
            };
            GnssStatusEnd += (s, e) => {
               GnssStatusIsOn = false;
               GnssFirstFixVar = 0;
               OnGnssStatusEnd?.Invoke(this, e);
            };
            GnssFirstFix += (s, e) => {
               GnssFirstFixVar = e;
               OnGnssFirstFix?.Invoke(this, e);
            };
            GnssStatusChanged += (s, e) => {
               MyGnssStatusVar = e;
               OnGnssStatusChanged?.Invoke(this, e);
            };
            SetGnssStatus(true);
            return true;
         }
         return false;
      }

      public void DeregisterGnss() {
         SetGnssStatus(false);
         GnssDeinit();
      }


      long _gnssFirstFix = 0;
      long _gnssStatusOn = 0;
      SatelliteStatus? _gnssStatus = null;

      public long GnssFirstFixVar {
         get => Interlocked.Read(ref _gnssFirstFix);
         set => Interlocked.Exchange(ref _gnssFirstFix, value);
      }

      public bool GnssStatusIsOn {
         get => Interlocked.Read(ref _gnssStatusOn) != 0;
         protected set => Interlocked.Exchange(ref _gnssStatusOn, value ? 1 : 0);
      }

      public SatelliteStatus? MyGnssStatusVar {
         get => Interlocked.Exchange(ref _gnssStatus, _gnssStatus);
         protected set => Interlocked.Exchange(ref _gnssStatus, value);
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
      /// </summary>
      public event EventHandler<int>? OnGnssFirstFix;

      /// <summary>
      /// Event sent periodically to report GPS satellite status. 
      /// </summary>
      public event EventHandler<SatelliteStatus>? OnGnssStatusChanged;


   }
}
