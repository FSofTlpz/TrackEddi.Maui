namespace TrackEddi {

   /// <summary>
   /// Interface zum Steuern des Service
   /// </summary>
   public partial class GeoLocationServiceCtrl {

      public class LocationChangedArgs {
         public Location? Location;
         public string Provider = string.Empty;
      }

      public event EventHandler<LocationChangedArgs>? LocationChanged;

      /// <summary>
      /// versucht den Service zu startet und liefert true, wenn der Start erfolgreich initiiert wurde
      /// <para>ACHTUNG: Damit läuft der Service noch nicht sofort und er kann sogar ganz fehlschlagen!</para>
      /// </summary>
      /// <param name="updateintervall"></param>
      /// <param name="updatedistance"></param>
      /// <returns></returns>
      public bool StartService(int updateintervall, double updatedistance) => startService(updateintervall, updatedistance);

      /// <summary>
      /// stopt den ev. laufenden Service
      /// </summary>
      /// <returns></returns>
      public bool StopService() => stopService();

      /// <summary>
      /// liefert den Status (aber leider nicht sofort nach <see cref="StartService(int, double)"/>, da
      /// der Start des Service einige Zeit benötigt)
      /// </summary>
      /// <returns></returns>
      public bool ServiceIsActive() => serviceIsActive();

      /// <summary>
      /// Ist der vom Service gelieferte Wert ein gültiger Wert?
      /// </summary>
      /// <param name="v"></param>
      /// <returns></returns>
      public bool IsValid(double v) => isValid(v);

   }
}
