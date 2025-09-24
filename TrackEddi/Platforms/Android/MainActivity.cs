using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using FSofTUtils.OSInterface;
using FSofTUtils.OSInterface.Platforms.Android;

namespace TrackEddi {

   // https://learn.microsoft.com/en-us/previous-versions/xamarin/android/platform/android-manifest

   [Activity(Label = "TrackEddi",
             Icon = "@mipmap/icon",
             Theme = "@style/Maui.SplashTheme",
             MainLauncher = true,
             LaunchMode = LaunchMode.SingleInstance,  //SingleTop SingleInstance

             ConfigurationChanges = ConfigChanges.ScreenSize |
                                    ConfigChanges.Orientation |
                                    ConfigChanges.UiMode |
                                    ConfigChanges.ScreenLayout |
                                    ConfigChanges.SmallestScreenSize |
                                    //ConfigChanges.KeyboardHidden |
                                    ConfigChanges.Density)]

   /*
   https://developer.android.com/guide/topics/manifest/intent-filter-element?hl=de

   intent-filter
      muss Folgendes enthalten:  <action>
      kann Folgendes enthalten:  <category>
                                 <data>
                                 <uri-relative-filter-group>


   ACTION_VIEW    Verwenden Sie diese Aktion, wenn Sie Informationen haben, die in einer Aktivität für den Nutzer angezeigt werden können, 
                  z. B. ein Foto, das in einer Galerie-App angezeigt werden soll, oder eine Adresse, die in einer Karten-App angezeigt werden soll.
   ACTION_SEND    Diesen Intent sollten sie verwenden, wenn Sie Daten haben, die der Nutzer über eine andere App wie eine E-Mail-App oder 
                  eine App zum Teilen in sozialen Netzwerken teilen kann. 

   Some examples of action/data pairs are:
      ACTION_VIEW   content://contacts/people/1 -- Display information about the person whose identifier is "1".
      ACTION_DIAL   content://contacts/people/1 -- Display the phone dialer with the person filled in.
      ACTION_VIEW   tel:123 -- Display the phone dialer with the given number filled in. Note how the VIEW action does what is considered the most reasonable thing for a particular URI.
      ACTION_DIAL   tel:123 -- Display the phone dialer with the given number filled in.
      ACTION_EDIT   content://contacts/people/1 -- Edit information about the person whose identifier is "1".
      ACTION_VIEW   content://contacts/people/ -- Display a list of people, which the user can browse through. This example is a typical top-level entry into the Contacts application, showing you the list of people. Selecting a particular person to view would result in a new intent { ACTION_VIEW content://contacts/people/N } being used to start an activity to display that person.

   CATEGORY_BROWSABLE   Die Zielaktivität kann von einem Webbrowser gestartet werden, um Daten anzuzeigen, auf die über einen Link verwiesen wird, 
                        z. B. ein Bild oder eine E‑Mail-Nachricht. 
   CATEGORY_LAUNCHER    Die Aktivität ist die erste Aktivität einer Aufgabe und wird im Anwendungs-Launcher des Systems aufgeführt. 


   https://developer.android.com/guide/topics/manifest/data-element?hl=de
   Wenn das Daten-Tag das unmittelbare untergeordnete Element eines <intent-filter> ist:
   <data android:scheme="string"
         android:host="string"
         android:port="string"
         android:path="string"
         android:pathPattern="string"
         android:pathPrefix="string"
         android:pathSuffix="string"
         android:pathAdvancedPattern="string"
         android:mimeType="string" />

        <data android:mimeType="text/plain"/>
        <data android:mimeType="application/vnd.google.panorama360+jpg"/>
        <data android:mimeType="image/*"/>
        <data android:mimeType="video/*"/>

   android:path
   android:pathPrefix
   android:pathSuffix
   android:pathPattern
   android:pathAdvancedPattern
      Pfadteil eines URI, der i.A. mit einem / beginnen muss. 
   
   Das path-Attribut gibt einen vollständigen Pfad an, der mit dem vollständigen Pfad in einem Intent-Objekt abgeglichen wird. 
   
   Das pathPrefix-Attribut gibt einen Teilpfad an, der nur mit dem Anfangsteil des Pfads im Intent-Objekt abgeglichen wird.

   Das pathSuffix-Attribut wird genau mit dem Endteil des Pfads im Intent-Objekt abgeglichen. Dieses Attribut muss nicht mit dem Zeichen / beginnen.

   Das pathPattern-Attribut gibt einen vollständigen Pfad an, der mit dem vollständigen Pfad im Intent-Objekt abgeglichen wird. 
   Es kann jedoch die folgenden Platzhalter enthalten:
      Ein Punkt (.) entspricht jedem Zeichen.
      Ein Sternchen (*) entspricht einer Folge von null bis vielen Vorkommen des unmittelbar vorausgehenden Zeichens.
      Ein Punkt gefolgt von einem Sternchen (.*) entspricht einer beliebigen Zeichenfolge mit null bis vielen Zeichen.

   Das pathAdvancedPattern-Attribut gibt einen vollständigen Pfad an, der mit dem vollständigen Pfad des Intent-Objekts abgeglichen wird. 
   Es unterstützt die folgenden regulären Ausdrucksmuster:
      Ein Punkt (.) entspricht einem beliebigen Zeichen.
      Ein Satz ([...]) entspricht einem Zeichenbereich. Beispiel: [0-5] stimmt beispielsweise mit einer einzelnen Ziffer zwischen 0 und 5 überein , aber nicht mit 6 bis 9. [a-zA-Z] stimmt mit einem beliebigen Buchstaben überein, unabhängig von der Groß- und Kleinschreibung. Für Sets wird auch der Modifikator „nicht“ ^ unterstützt.
      Der Modifikator „Sternchen“ (*) entspricht null oder mehrmals dem vorangehenden Muster.
      Der Modifizierer „Plus“ (+) entspricht dem vorangehenden Muster einmal oder mehrmals.
      Mit dem Modifikator „Bereich“ ({...}) wird angegeben, wie oft ein Muster übereinstimmen kann.
   Der pathAdvancedPattern-Abgleich ist eine Bewertungsimplementierung, bei der der Abgleich in Echtzeit ohne Backtracking-Unterstützung mit dem Muster erfolgt.

   Da \ als Escape-Zeichen verwendet wird, wenn der String aus XML gelesen wird, bevor er als Muster geparst wird, müssen Sie das Zeichen doppelt maskieren. 
   Beispiel: Das Literal * wird als \\* geschrieben und das Literal \ als \\\. Das entspricht dem, was Sie schreiben, wenn Sie den String in Java-Code erstellen. 



   FUNKTIONIERT NICHT !!!

      DataSchemes = ["file", "content"],
      DataPathSuffixes = [".gpx", ".kml", ".kmz"],
      AutoVerify = true)]

      DataHost = "*",
      DataSchemes = ["file", "content"],
      DataPathSuffix = ".gpx",
      AutoVerify = true)]

   OK

      DataHost = "*",
      DataMimeType = "* /*",
      DataSchemes = ["file", "content"],
      DataPathSuffix = ".gpx",
      AutoVerify = true)]

      DataHost = "*",
      DataMimeType = "* /*",
      DataSchemes = ["file", "content"],
      DataPathSuffixes = [".gpx", ".kml", ".kmz"],
      AutoVerify = true)]

   DataHost und DataMimeType scheinen NOTWENDIG zu sein.


    */
   [IntentFilter(
      [Intent.ActionView],
      // https://developer.android.com/reference/android/content/Intent#CATEGORY_DEFAULT
      Categories = [
         Intent.CategoryDefault,
         Intent.CategoryBrowsable     // Activities that can be safely invoked from a browser must support this category. 
      ],
      // https://developer.android.com/guide/topics/manifest/data-element.html#path
      DataHost = "*",
      DataMimeType = "*/*",
      DataSchemes = ["file", "content"],
      DataPathSuffixes = [".gpx", ".kml", ".kmz"],
      AutoVerify = true)]
   public partial class MainActivity : MauiAppCompatActivity {

      static string logfile = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                                                     "TrackEddiErrorLog.txt");

      protected override void OnCreate(Bundle? savedInstanceState) {
         addExceptionCatcher();
         //writeIntentData(Intent, "OnCreate");

         //if (DirtyGlobalVars.AndroidActivity != null) {
         //   MainActivity ma = (MainActivity)DirtyGlobalVars.AndroidActivity;

         //}
         DirtyGlobalVars.AndroidActivity = this;
         base.OnCreate(savedInstanceState);

         // Hardware-Backbutton umlenken auf ContentPage.OnBackButtonPressed()
         //OnBackPressedDispatcher.AddCallback(this, new BackPress(this));

         /* 
           * "https://nominatim.openstreetmap.org/search?q={0}&format=xml";
           * führt zu einer Exception:
           *       Error: TrustFailure (Authentication failed, see inner exception.)
           *       Ssl error:1000007d:SSL routines:OPENSSL_internal:CERTIFICATE_VERIFY_FAILED
           *       
           * deshalb:
          */
         System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

         /* Versions:
IceCreamSandwich     14       October 2011: Android 4.0.
IceCreamSandwichMr1  15       December 2011: Android 4.0.3.
JellyBean            16       June 2012: Android 4.1.
JellyBeanMr1         17       Android 4.2.x
JellyBeanMr2         18       Android 4.3: Jelly Bean MR2, the revenge of the beans.
Kitkat               19       Android 4.4, 4.4.x
KitkatWatch          20       Android 4.4W
Lollipop             21       Android 5.0, 5.0.x
LollipopMr1          22       Android 5.1, 5.1.1
M                    23       Android 6.0, 6.0.1
N                    24       Android 7.0
NMr1                 25       Android 7.1, 7.1.x
O                    26       Android 8.0
OMr1                 27       Android 8.1
P                    28       Android 9
Q                    29       Android 10
R                    30       Android 11
S                    31       Android 12
SV2                  32       Android 12.1
Tiramisu             33       Android 13           8/2022
UpsideDownCake       34
VanillaIceCream      35


https://developer.android.com/about/versions/15/behavior-changes-all

expl.
<=30     WRITE_EXTERNAL_STORAGE     Allows an application to write to external storage.
<=33     READ_EXTERNAL_STORAGE      Allows an application to read from external storage. 
>=33     READ_MEDIA_AUDIO           Allows an application to read audio files from external storage.
>=33     READ_MEDIA_IMAGES          Allows an application to read image files from external storage.
>=33     READ_MEDIA_VIDEO           Allows an application to read video files from external storage. 
ACCESS_FINE_LOCATION       Allows an app to access precise location. Alternatively, you might want ACCESS_COARSE_LOCATION.
ACCESS_COARSE_LOCATION     Allows an app to access approximate location. Alternatively, you might want ACCESS_FINE_LOCATION.

nur AndroidManifest.xml
>=30     MANAGE_EXTERNAL_STORAGE    Allows an application a broad access to external storage in scoped storage. 
                        extra anfordern mit spez. Intent

>=29     ACCESS_BACKGROUND_LOCATION Allows an app to access location in the background. 

 */

         const string reasonManageExtStor = "Dieses Recht ist für den Zugriff auf die Konfigurationsdaten, GPX-Dateien, den Cache für die Karten und lokale Karten nötig.";
         const string reasonRead = "Dieses Recht ist für den Zugriff auf die Konfigurationsdaten, GPX-Dateien, den Cache für die Karten und lokale Karten nötig.";
         const string reasonWrite = "Dieses Recht ist für den Zugriff auf die Konfigurationsdaten, GPX-Dateien, den Cache für die Karten und lokale Karten nötig.";
         const string reasonNotify = "Dieses Recht ist für die Anzeige des Standortzugriffs nötig.";
         const string reasonLocation = "Dieses Recht ist für die Standortanzeige und die Trackaufzeichnung nötig.";

         List<PermissionsRequest.OSPermission> permissions = new List<PermissionsRequest.OSPermission> {
               new PermissionsRequest.OSPermission(Manifest.Permission.AccessCoarseLocation,      // ungefährer Standort
                                                   reasonLocation),
               new PermissionsRequest.OSPermission(Manifest.Permission.AccessFineLocation,        // GPS-Standort
                                                   reasonLocation),
               //Manifest.Permission.AccessBackgroundLocation,                  // NUR im Manifest festlegen!!!
            };

         if (Build.VERSION.SdkInt <= BuildVersionCodes.R) {                // ... 30

            if (System.OperatingSystem.IsAndroidVersionAtLeast(30))
               permissions.Add(new PermissionsRequest.OSPermission(PermissionsRequest.SPECIALPERM_MANAGEEXTERNALSTORAGE, reasonManageExtStor)); // Sonderfall bei Behandlung
            permissions.Add(new PermissionsRequest.OSPermission(Manifest.Permission.WriteExternalStorage, reasonWrite));
            permissions.Add(new PermissionsRequest.OSPermission(Manifest.Permission.ReadExternalStorage, reasonRead));

         } else if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu) {   // 31 ... 32

            if (System.OperatingSystem.IsAndroidVersionAtLeast(30))
               permissions.Add(new PermissionsRequest.OSPermission(PermissionsRequest.SPECIALPERM_MANAGEEXTERNALSTORAGE, reasonManageExtStor)); // Sonderfall bei Behandlung
            permissions.Add(new PermissionsRequest.OSPermission(Manifest.Permission.ReadExternalStorage, reasonRead));

         } else {                                                          // 33 ...

            if (System.OperatingSystem.IsAndroidVersionAtLeast(30))
               permissions.Add(new PermissionsRequest.OSPermission(PermissionsRequest.SPECIALPERM_MANAGEEXTERNALSTORAGE, reasonManageExtStor)); // Sonderfall bei Behandlung
            if (System.OperatingSystem.IsAndroidVersionAtLeast(33)) {
               permissions.Add(new PermissionsRequest.OSPermission(Manifest.Permission.ReadMediaAudio, reasonRead)); // Ersatz für ReadExternalStorage
               permissions.Add(new PermissionsRequest.OSPermission(Manifest.Permission.ReadMediaVideo, reasonRead));
               permissions.Add(new PermissionsRequest.OSPermission(Manifest.Permission.ReadMediaImages, reasonRead));
               permissions.Add(new PermissionsRequest.OSPermission(Manifest.Permission.PostNotifications, reasonNotify));
            }

         }

         PermissionsRequest.Init(this, permissions);

      }

      #region extended

      /// <summary>
      /// für threadsichere Logdatei
      /// </summary>
      static object loglocker = new object();

      //public class EventArgsActivityResult {
      //   public readonly int RequestCode;
      //   public readonly Result ResultCode;
      //   public readonly Android.Content.Intent? Data;

      //   public EventArgsActivityResult(int requestCode, Result resultCode, Android.Content.Intent? data) {
      //      RequestCode = requestCode;
      //      ResultCode = resultCode;
      //      Data = data;
      //   }
      //}

      //public event EventHandler<EventArgsActivityResult>? ActivityResultExt;


      public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
         Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

         if (System.OperatingSystem.IsAndroidVersionAtLeast(23))
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
      }

      /// <summary>
      /// reagiert auf den Software-Backbutton (fkt. NUR mit SetSupportActionBar() in OnCreate())
      /// </summary>
      /// <param name="item"></param>
      /// <returns></returns>
      public override bool OnOptionsItemSelected(Android.Views.IMenuItem item) {
         // check if the current item id is equals to the back button id
         if (item.ItemId == Android.Resource.Id.Home) {
            Microsoft.Maui.Controls.Application? myapplication = Microsoft.Maui.Controls.Application.Current;
            if (myapplication != null) {
               Window? win = myapplication.Windows.Count > 0 ? myapplication.Windows[0] : null;
               if (win != null &&
                   win.Page != null &&
                   win.Page.SendBackButtonPressed())
                  return false;
            }
         }
         return base.OnOptionsItemSelected(item);
      }

      ///// <summary>
      ///// Hardware-Backbutton umlenken auf ContentPage.OnBackButtonPressed()
      ///// </summary>
      //public override void OnBackPressed() {
      //   // this is not necessary, but in Android user has both Nav bar back button and physical back button its safe to cover the both events
      //   Microsoft.Maui.Controls.Application? myapplication = Microsoft.Maui.Controls.Application.Current;
      //   if (myapplication != null) {
      //      Window? win = myapplication.Windows.Count > 0 ? myapplication.Windows[0] : null;
      //      if (win != null &&
      //          win.Page != null &&
      //          win.Page.SendBackButtonPressed())
      //         return;
      //   }
      //   base.OnBackPressed();
      //}

      ///// <summary>
      ///// Event "weiterreichen"
      ///// </summary>
      ///// <param name="requestCode"></param>
      ///// <param name="resultCode"></param>
      ///// <param name="data"></param>
      //protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent? data) {
      //   ActivityResultExt?.Invoke(this, new EventArgsActivityResult(requestCode, resultCode, data));
      //}

      #region EXCEPTIONCATCHER

      void addExceptionCatcher() {
         AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
         TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
         AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidEnvironmentUnhandledExceptionRaiser;
         activity4exception = this;
      }

      void OnAndroidEnvironmentUnhandledExceptionRaiser(object? sender, RaiseThrowableEventArgs unhandledExceptionEventArgs) {
         var newExc = new Exception("OnAndroidEnvironmentUnhandledExceptionRaiser", unhandledExceptionEventArgs.Exception);
         LogUnhandledException(newExc);
      }

      static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs) {
         var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
         LogUnhandledException(newExc);
      }

      static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs) {
         var newExc = new Exception("CurrentDomainOnUnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception);
         LogUnhandledException(newExc);
      }

      static void LogUnhandledException(Exception exception) {
         ErrorLog(exception.ToString());
      }

      static Activity? activity4exception = null;

      static void ErrorLog(string txt) {
         try {
            lock (loglocker) {
               System.IO.File.AppendAllText(logfile, DateTime.Now.ToString("G") + " " + txt + System.Environment.NewLine);
            }

            if (activity4exception != null) {
               Android.App.AlertDialog? dlg = SimpleDialogs.GetBaseDialog(activity4exception, "Android-Exception", txt);
               if (dlg != null) {
                  dlg.SetButton("Bye bye ...", (c, ev) => activity4exception.Finish());
                  dlg.Show();
               }
            }
         } catch { }
      }

      #endregion

      #endregion

      //void writeIntentData(Intent? intent, string theme) {
      //   if (intent != null) {
      //      List<string> lines = [theme + ":"];
      //      if (intent.Action != null)
      //         lines.Add("   Action: " + intent.Action);

      //      if (intent.Categories != null)
      //         foreach (var item in intent.Categories)
      //            lines.Add("   Categories: " + item);

      //      if (intent.Component != null) {
      //         lines.Add("   Component.PackageName: " + intent.Component.PackageName);
      //         lines.Add("   Component.ClassName: " + intent.Component.ClassName);
      //      }

      //      if (intent.DataString != null)
      //         lines.Add("   DataString: " + intent.DataString);

      //      if (intent.Extras != null && intent.Extras.KeySet() != null)
      //         foreach (var item in intent.Extras.KeySet())
      //            lines.Add("   Extras-Key: " + item);

      //      lines.Add("   Flags: " + intent.Flags.ToString());

      //      if (intent.Identifier != null)
      //         lines.Add("   Identifier: " + intent.Identifier);

      //      if (intent.Package != null)
      //         lines.Add("   Package: " + intent.Package);

      //      //PackageManager pm;

      //      //if (Intent.ResolveActivity(pm)!= null)
      //      //   lines.Add("   ResolveActivityInfo: " + Intent.ResolveActivityInfo);

      //      if (intent.Scheme != null)
      //         lines.Add("   Scheme: " + intent.Scheme);

      //      if (intent.Type != null)
      //         lines.Add("   Type: " + intent.Type);

      //      File.AppendAllLines("/storage/emulated/0/TrackEddi/intent.txt", lines);
      //   }

      //}





      //internal class BackPress : OnBackPressedCallback {
      // in AndroidManifest.xml:    android:enableOnBackInvokedCallback="true"
      //   private readonly Activity activity;

      //   public BackPress(Activity activity) : base(true) {
      //      this.activity = activity;
      //      Enabled = true;     // false -> Standardverhalten
      //   }

      //   public override void HandleOnBackPressed() {
      //      // FKT. ALLES NICHT WIE IM ORIGINAL !!!!!
      //      Page? page = Microsoft.Maui.Controls.Application.Current?.Windows[0].Page;
      //      page?.Navigation.PopAsync();
      //      bool ret = page != null ? page.SendBackButtonPressed() : false;
      //      //Microsoft.Maui.Controls.Application.Current?.Windows[0].Page?.SendBackButtonPressed();
      //   }
      //}

   }

}
