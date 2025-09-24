using System.Collections.Concurrent;

namespace TrackEddi {
   internal static class CompassExt {

      class UserData {
         public readonly double MinDelta;
         public double LastSendedHeadingMagneticNorth;
         public Action<int, double> Action;

         public UserData(double mindelta, Action<int, double> action) {
            MinDelta = mindelta;
            LastSendedHeadingMagneticNorth = 400;
            Action = action;
         }

      }

      static ConcurrentDictionary<int, UserData> userData = new ConcurrentDictionary<int, UserData>();



      /// <summary>
      /// Gerät unterstützt einen Kompass (ist vorhanden)
      /// </summary>
      public static bool IsSupported => Compass.IsSupported;

      /// <summary>
      /// Liefert der Geräte-Kompass akt. Daten?
      /// </summary>
      public static bool IsRunning => Compass.IsSupported && Compass.IsMonitoring;


      /// <summary>
      /// startet den Kompass
      /// </summary>
      /// <param name="action">Benachrichtigungsfkt.</param>
      /// <param name="minDelta4HeadingMagneticNorth">min. Abweichung vom letzten gemeldeten Wert für die Auslösung der Benachrichtigungsfkt.</param>
      /// <returns>ID; negativ, wenn ein Start nicht möglich ist</returns>
      public static int Register(Action<int, double> action, double minDelta4HeadingMagneticNorth = 0) {
         int result = -1;

         if (IsSupported) {
            int id = 1 + (userData.Count == 0 ? 0 : userData.Keys.Max());
            while (!userData.TryAdd(id, new UserData(minDelta4HeadingMagneticNorth, action)))
               id++;
            if (!IsRunning) {
               Compass.ReadingChanged += compass_ReadingChanged;
               Compass.Start(SensorSpeed.Default, true);
            }
            result = id;
         }
         return result;
      }

      public static bool UnRegister(int user) {
         bool result = false;

         if (userData.ContainsKey(user)) {
            userData.TryRemove(user, out _);
            if (userData.IsEmpty) {
               if (IsRunning) {
                  Compass.Stop();
                  Compass.ReadingChanged -= compass_ReadingChanged;
               }
            }
            result = true;
         }
         return result;
      }

      static void compass_ReadingChanged(object? sender, CompassChangedEventArgs e) {
         foreach (var user in userData) {
            double delta = e.Reading.HeadingMagneticNorth - user.Value.LastSendedHeadingMagneticNorth;
            if (delta < 0)
               delta = -delta;
            while (delta > 360)
               delta -= 360;
            if (delta >= user.Value.MinDelta) {
               user.Value.Action(user.Key, e.Reading.HeadingMagneticNorth);
               user.Value.LastSendedHeadingMagneticNorth = e.Reading.HeadingMagneticNorth;
            }
         }

      }

   }
}
