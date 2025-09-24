using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.OS.Storage;
using Android.Provider;
using AndroidX.Activity.Result;
using FSofTUtils.OSInterface.Platforms.Android;
using System.Text;

namespace FSofTUtils.OSInterface {
   public static partial class PermissionsRequest {

      /// <summary>
      /// für die Definition der Rechte
      /// </summary>
      public class OSPermission {

         /// <summary>
         /// Android-Name der Permission
         /// </summary>
         public string Name { get; protected set; }

         /// <summary>
         /// unbedingt nötig oder nur wünschenswert
         /// </summary>
         public bool Necessary { get; protected set; }

         public string Description { get; protected set; } = string.Empty;


         public OSPermission(string name, string description, bool necessary = true) {
            Name = name;
            Necessary = necessary;
            Description = description;
         }

         public OSPermission(OSPermission perm) {
            Name = perm.Name;
            Necessary = perm.Necessary;
            Description = perm.Description;
         }

         public override string ToString() {
            return Name + " " + Necessary;
         }

      }

      #region PermissionRequest

      public class PermissionRequest {

         class MyActivityWithResult : Java.Lang.Object, IActivityResultCallback {
            readonly Action<ActivityResult>? callback_ar = null;
            readonly Action<Dictionary<string, bool>>? callback_dict = null;

            /// <summary>
            /// für StartActivityForResult() mit Intent
            /// </summary>
            /// <param name="callback"></param>
            public MyActivityWithResult(Action<ActivityResult> callback) => callback_ar = callback;

            /// <summary>
            /// für RequestMultiplePermissions() mit string[]
            /// <para>(RequestPermission() mit string ist damit auch abgedeckt)</para>
            /// </summary>
            /// <param name="callback"></param>
            public MyActivityWithResult(Action<Dictionary<string, bool>> callback) => callback_dict = callback;

            public void OnActivityResult(Java.Lang.Object? result) {

               if (result is ActivityResult) {
                  if (callback_ar != null && result is ActivityResult activityResult)
                     callback_ar(activityResult);

               } else if (result is Android.Runtime.JavaDictionary dictionary) {

                  Dictionary<string, bool> netdict = [];
                  Android.Runtime.JavaDictionary dict = dictionary;
                  foreach (var key in dict.Keys)
                     if (key != null) {
                        if (key != null) {
                           string? k = key.ToString();
                           object? v = dict[key];
                           if (k != null && v != null)
                              netdict.Add(k, (bool)v);
                        }
                     }

                  callback_dict?.Invoke(netdict);

               } else if (result is Java.Util.LinkedHashMap) {
                  Dictionary<string, bool> netdict = [];
                  if (result is Java.Util.LinkedHashMap lhm)
                     foreach (var item in lhm.KeySet()) {
                        if (item != null) {
                           string? k = item.ToString();
                           Java.Lang.Object? v = lhm.Get(k);
                           if (k != null && v != null)
                              netdict.Add(k, (bool)v);
                        }
                     }

                  callback_dict?.Invoke(netdict);

               } else if (result is Java.Util.AbstractMap) {
                  Dictionary<string, bool> netdict = [];
                  if (result is Java.Util.AbstractMap am)
                     foreach (var item in am.KeySet()) {
                        if (item != null) {
                           string? k = item.ToString();
                           Java.Lang.Object? v = am.Get(k);
                           if (k != null && v != null)
                              netdict.Add(k, (bool)v);
                        }
                     }

                  callback_dict?.Invoke(netdict);


               } else {

                  callback_dict?.Invoke([]);

               }

            }
         }


         readonly ActivityResultLauncher _activityResultLauncher;

         Dictionary<string, bool>? permissionsresult = null;

         readonly AndroidX.Activity.ComponentActivity activity;

         readonly ManualResetEvent manualResetEvent4Request = new(false);


         public PermissionRequest(Activity componentActivity) {
            activity = (AndroidX.Activity.ComponentActivity)componentActivity;
            _activityResultLauncher = registerActivityResultLauncher();
         }

         // new AndroidX.Activity.Result.Contract.ActivityResultContracts.StartActivityForResult();      // Intent für Start nötig
         // new AndroidX.Activity.Result.Contract.ActivityResultContracts.RequestMultiplePermissions();  // string[] für Start nötig
         // new AndroidX.Activity.Result.Contract.ActivityResultContracts.RequestPermission();           // string für Start nötig


         ActivityResultLauncher registerActivityResultLauncher(Action<Dictionary<string, bool>> callback) {
            return activity.RegisterForActivityResult(
               new AndroidX.Activity.Result.Contract.ActivityResultContracts.RequestMultiplePermissions(),
               new MyActivityWithResult(callback)
            );
         }

         ActivityResultLauncher registerActivityResultLauncher() {
            /* Note: You must call registerForActivityResult() before the fragment or activity is created, 
             * but you can't launch the ActivityResultLauncher until the fragment or activity's Lifecycle 
             * has reached CREATED.
             */
            return activity.RegisterForActivityResult(
               new AndroidX.Activity.Result.Contract.ActivityResultContracts.RequestMultiplePermissions(),  // string[] nötig
               new MyActivityWithResult((Dictionary<string, bool> dict) => {
                  permissionsresult = dict;
                  manualResetEvent4Request.Set();     // Signal, dass die Activity jetzt abgeschlossen ist
               })
            );
         }

         /// <summary>
         /// damit nur 1 Request gleichzeitig efolgen kann
         /// </summary>
         long requestIsBusy = 0;

         /// <summary>
         /// sammelt zunächst die noch nicht vorhandenen Rechte ein und fordert diese an
         /// </summary>
         /// <param name="permissions"></param>
         /// <returns>Liste der nicht erteilten Rechte</returns>
         public async Task<List<string>> Start(IList<string> permissions) {
            while (Interlocked.CompareExchange(ref requestIsBusy, 1, 0) == 1)    // warten bis kein Request mehr in Arbeit ist
               Thread.Sleep(100);

            List<string> errors = [];
            List<string> neededPermissions = [];
            if (System.OperatingSystem.IsAndroidVersionAtLeast(23))
               foreach (var item in permissions) {
                  if (activity.CheckSelfPermission(item) == Permission.Denied)   // wenn noch nicht vorhanden, dann anforden
                     neededPermissions.Add(item);
               }

            if (0 < neededPermissions.Count) {
               await Task.Run(() => {
                  manualResetEvent4Request.Reset();
                  _activityResultLauncher.Launch(neededPermissions.ToArray());
                  manualResetEvent4Request.WaitOne();    // Wenn das Event gesetzt wird ist auch permissionsOK neu gesetzt.
                  if (permissionsresult != null) {
                     foreach (var key in permissionsresult.Keys)
                        if (!permissionsresult[key])
                           errors.Add(key);
                  } else {
                     errors.Add("internal error");
                  }
               });
            }

            Interlocked.Exchange(ref requestIsBusy, 0);  // Requestsperre entfernen
            return errors;
         }

         /// <summary>
         /// fordert falls nötig das Recht an
         /// </summary>
         /// <param name="permission"></param>
         /// <returns>Liste der nicht erteilten Rechte</returns>
         public async Task<List<string>> Start(string permission) => await Start([permission]);

      }

      #endregion

      #region PermissionRequest_ManageExternalStorage

      /// <summary>
      /// zur Anforderung von MANAGE_EXTERNAL_STORAGE bzw. der persistenten Permissions VOR API 30/Android 11/R
      /// <para>eingeführt mit API 30/Android 11/R</para>
      /// <para>Fkt. wegen StorageVolumes erst ab API 24/Android 7/N !</para>
      /// </summary>
      class PermissionRequest_ManageExternalStorage {

         class MyActivityWithResult {

            class MyActivityResultCallback : Java.Lang.Object, AndroidX.Activity.Result.IActivityResultCallback {
               readonly Action<AndroidX.Activity.Result.ActivityResult> _callback;

               public MyActivityResultCallback(Action<AndroidX.Activity.Result.ActivityResult> callback) => _callback = callback;

               public void OnActivityResult(Java.Lang.Object? result) {
                  if (result != null &&
                      result is AndroidX.Activity.Result.ActivityResult)
                     _callback((AndroidX.Activity.Result.ActivityResult)result);
               }
            }

            readonly AndroidX.Activity.Result.ActivityResultLauncher? result;

            /// <summary>
            /// Registrierung des Callbacks (i.A. in OnCreate)
            /// </summary>
            /// <param name="resultaction"></param>
            public MyActivityWithResult(Action<AndroidX.Activity.Result.ActivityResult> resultaction) {
               AndroidX.Activity.ComponentActivity? activity = (AndroidX.Activity.ComponentActivity?)Platform.CurrentActivity;
               if (activity != null) {
                  result = activity.RegisterForActivityResult(
                                             new AndroidX.Activity.Result.Contract.ActivityResultContracts.StartActivityForResult(),
                                             new MyActivityResultCallback(resultaction));
               }
            }

            /// <summary>
            /// startet die Activity und kehrt sofort zurück
            /// </summary>
            /// <param name="intent"></param>
            public void Start(Intent intent) => result?.Launch(intent);

         }


         readonly Activity activity;

         /// <summary>
         /// Ex. das Recht?
         /// </summary>
         public bool IsGranted { get; protected set; } = false;

         /// <summary>
         /// zum warten auf das Ergebnis
         /// </summary>
         readonly ManualResetEvent manualResetEvent4Request;

         readonly MyActivityWithResult? myActivityWithResult;


         public PermissionRequest_ManageExternalStorage(Activity activity) {
            this.activity = activity;
            manualResetEvent4Request = new ManualResetEvent(false);
            myActivityWithResult = new MyActivityWithResult((AndroidX.Activity.Result.ActivityResult result) => {
               IsGranted = FSofTUtils.OSInterface.AndroidFunction.IsExternalStorageManager;
               manualResetEvent4Request.Set();     // Signal, dass die Activity jetzt abgeschlossen ist
            });
         }

         public bool PermissionExists(Activity activity) =>
            OperatingSystem.IsAndroidVersionAtLeast(30) ?
                        FSofTUtils.OSInterface.AndroidFunction.IsExternalStorageManager :
                        false;

         /// <summary>
         /// führt zur Anfrage an den Nutzer, ob die Permissions erteilt werden sollen (API level 24 / Nougat / 7.0; deprecated in API level Q) 
         /// (nur für secondary external storage sinnvoll)
         /// </summary>
         /// <returns></returns>
         public async Task<bool> Start() {
            // https://developer.android.com/training/data-storage/manage-all-files

            int volidx = 1;
            IsGranted = setPersistentPermissions(volidx);

            /*
               IsExternalStorageManager:                    ab API 30/Android 11/R
               Returns whether the calling app has All Files Access on the primary shared/external storage media.
               Declaring the permission Manifest.permission.MANAGE_EXTERNAL_STORAGE isn't enough to gain the access.
               To request access, use Settings.ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION.
             */
            if (!IsGranted) {

               Intent? intent = getAccessIntent(volidx);
               if (intent != null) {

                  /*
                  ACHTUNG
                  Android 14:
                  Schaltet man in der Activity das Recht zunächst ein und dann gleich wieder aus, wird die MainActivity auf jeden Fall "gekillt".
                  (auch mit "Unexpected activity event reported!" wie bei Android 15).

                  ACHTUNG
                  Ab Android 15 neuese Verhalten:
                  Nach etwa 3 Sekunden ohne Interaktion mit der gestarteten Activity wird die App (NICHT die Activity!) abgebrochen.

--------- beginning of system
01-26 17:19:37.063   875   910 V WindowManagerShell: Transition requested (#189): android.os.BinderProxy@5cd9664 TransitionRequestInfo { type = OPEN, triggerTask = TaskInfo{userId=0 taskId=87 displayId=0 isRunning=true baseIntent=Intent { act=android.intent.action.MAIN cat=[android.intent.category.LAUNCHER] flg=0x10000000 cmp=com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity } baseActivity=ComponentInfo{com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity} topActivity=ComponentInfo{com.android.settings/com.android.settings.Settings$AppManageExternalStorageActivity} origActivity=null realActivity=ComponentInfo{com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity} numActivities=2 lastActiveTime=21445459 supportsMultiWindow=true resizeMode=1 isResizeable=true minWidth=-1 minHeight=-1 defaultMinSize=220 token=WCT{android.window.IWindowContainerToken$Stub$Proxy@1b3b0cd} topActivityType=1 pictureInPictureParams=null shouldDockBigOverlays=false launchIntoPipHostTaskId=-1 lastParentTaskIdBeforePip=-1 displayCutoutSafeInsets=Rect(0, 136 - 0, 0) topActivityInfo=ActivityInfo{4eeed82 com.android.settings.Settings$AppManageExternalStorageActivity} launchCookies=[] positionInParent=Point(0, 0) parentTaskId=-1 isFocused=true isVisible=true isVisibleRequested=true isSleeping=false locusId=null displayAreaFeatureId=1 isTopActivityTransparent=false appCompatTaskInfo=AppCompatTaskInfo { topActivityInSizeCompat=false topActivityEligibleForLetterboxEducation= falseisLetterboxEducationEnabled= true isLetterboxDoubleTapEnabled= false topActivityEligibleForUserAspectRatioButton= false topActivityBoundsLetterboxed= false isFromLetterboxDoubleTap= false topActivityLetterboxVerticalPosition= -1 topActivityLetterboxHorizontalPosition= -1 topActivityLetterboxWidth=1080 topActivityLetterboxHeight=2400 isUserFullscreenOverrideEnabled=false isSystemFullscreenOverrideEnabled=false cameraCompatTaskInfo=CameraCompatTaskInfo { cameraCompatControlState=hidden freeformCameraCompatMode=inactive}}}, pipTask = null, remoteTransition = null, displayChange = null, flags = 0, debugId = 189 }
01-26 17:19:37.701   875   910 V WindowManagerShell: onTransitionReady (#189) android.os.BinderProxy@5cd9664: {id=189 t=OPEN f=0x0 trk=0 opt={t=FROM_STYLE} r=[0@Point(0, 0)] c=[{null m=OPEN f=FILLS_TASK leash=Surface(name=ActivityRecord{cd4e1cf u0 com.android.settings/.spa.SpaActivity)/@0xaa4ec93 sb=Rect(0, 0 - 0, 0) eb=Rect(0, 0 - 1080, 2400) d=-1->0 r=-1->0:-1 bc=fff1f0f7 component=com.android.settings/.spa.SpaActivity},{null m=TO_BACK f=FILLS_TASK leash=Surface(name=ActivityRecord{a5d1bc5 u0 com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity)/@0x4ffb4d0 sb=Rect(0, 0 - 1080, 2400) eb=Rect(0, 0 - 1080, 2400) d=0 bc=fff1f0f7 component=com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity}]}
01-26 17:19:37.710   875   910 V WindowManagerShell: Transition doesn't have explicit remote, search filters for match for {id=189 t=OPEN f=0x0 trk=0 opt={t=FROM_STYLE} r=[0@Point(0, 0)] c=[{null m=OPEN f=FILLS_TASK leash=Surface(name=ActivityRecord{cd4e1cf u0 com.android.settings/.spa.SpaActivity)/@0xaa4ec93 sb=Rect(0, 0 - 0, 0) eb=Rect(0, 0 - 1080, 2400) d=-1->0 r=-1->0:-1 bc=fff1f0f7 component=com.android.settings/.spa.SpaActivity},{null m=TO_BACK f=FILLS_TASK leash=Surface(name=ActivityRecord{a5d1bc5 u0 com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity)/@0x4ffb4d0 sb=Rect(0, 0 - 1080, 2400) eb=Rect(0, 0 - 1080, 2400) d=0 bc=fff1f0f7 component=com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity}]}
01-26 17:19:37.714   875   910 V WindowManagerShell: start default transition animation, info = {id=189 t=OPEN f=0x0 trk=0 opt={t=FROM_STYLE} r=[0@Point(0, 0)] c=[{null m=OPEN f=FILLS_TASK leash=Surface(name=ActivityRecord{cd4e1cf u0 com.android.settings/.spa.SpaActivity)/@0xaa4ec93 sb=Rect(0, 0 - 0, 0) eb=Rect(0, 0 - 1080, 2400) d=-1->0 r=-1->0:-1 bc=fff1f0f7 component=com.android.settings/.spa.SpaActivity},{null m=TO_BACK f=FILLS_TASK leash=Surface(name=ActivityRecord{a5d1bc5 u0 com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity)/@0x4ffb4d0 sb=Rect(0, 0 - 1080, 2400) eb=Rect(0, 0 - 1080, 2400) d=0 bc=fff1f0f7 component=com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity}]}
01-26 17:19:37.725   594   628 V WindowManager: Sent Transition (#189) createdAt=01-26 17:19:37.032 via request=TransitionRequestInfo { type = OPEN, triggerTask = TaskInfo{userId=0 taskId=87 displayId=0 isRunning=true baseIntent=Intent { act=android.intent.action.MAIN cat=[android.intent.category.LAUNCHER] flg=0x10000000 cmp=com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity } baseActivity=ComponentInfo{com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity} topActivity=ComponentInfo{com.android.settings/com.android.settings.Settings$AppManageExternalStorageActivity} origActivity=null realActivity=ComponentInfo{com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity} numActivities=2 lastActiveTime=21445459 supportsMultiWindow=true resizeMode=1 isResizeable=true minWidth=-1 minHeight=-1 defaultMinSize=220 token=WCT{RemoteToken{53fb958 Task{779091a #87 type=standard A=10215:com.fsoft.trackeddi2}}} topActivityType=1 pictureInPictureParams=null shouldDockBigOverlays=false launchIntoPipHostTaskId=-1 lastParentTaskIdBeforePip=-1 displayCutoutSafeInsets=Rect(0, 136 - 0, 0) topActivityInfo=ActivityInfo{497dcbf com.android.settings.Settings$AppManageExternalStorageActivity} launchCookies=[] positionInParent=Point(0, 0) parentTaskId=-1 isFocused=true isVisible=true isVisibleRequested=true isSleeping=false locusId=null displayAreaFeatureId=1 isTopActivityTransparent=false appCompatTaskInfo=AppCompatTaskInfo { topActivityInSizeCompat=false topActivityEligibleForLetterboxEducation= falseisLetterboxEducationEnabled= true isLetterboxDoubleTapEnabled= false topActivityEligibleForUserAspectRatioButton= false topActivityBoundsLetterboxed= false isFromLetterboxDoubleTap= false topActivityLetterboxVerticalPosition= -1 topActivityLetterboxHorizontalPosition= -1 topActivityLetterboxWidth=1080 topActivityLetterboxHeight=2400 isUserFullscreenOverrideEnabled=false isSystemFullscreenOverrideEnabled=false cameraCompatTaskInfo=CameraCompatTaskInfo { cameraCompatControlState=hidden freeformCameraCompatMode=inactive}}}, pipTask = null, remoteTransition = null, displayChange = null, flags = 0, debugId = 189 }
01-26 17:19:37.729   594   628 V WindowManager:     info={id=189 t=OPEN f=0x0 trk=0 opt={t=FROM_STYLE} r=[0@Point(0, 0)] c=[
01-26 17:19:37.729   594   628 V WindowManager:         {null m=OPEN f=FILLS_TASK leash=Surface(name=ActivityRecord{cd4e1cf u0 com.android.settings/.spa.SpaActivity)/@0x710cd60 sb=Rect(0, 0 - 0, 0) eb=Rect(0, 0 - 1080, 2400) d=-1->0 r=-1->0:-1 bc=fff1f0f7 component=com.android.settings/.spa.SpaActivity},
01-26 17:19:37.729   594   628 V WindowManager:         {null m=TO_BACK f=FILLS_TASK leash=Surface(name=ActivityRecord{a5d1bc5 u0 com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity)/@0xcffcd63 sb=Rect(0, 0 - 1080, 2400) eb=Rect(0, 0 - 1080, 2400) d=0 bc=fff1f0f7 component=com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity}
01-26 17:19:37.729   594   628 V WindowManager:     ]}
01-26 17:19:43.560   594  2154 I ActivityManager: Force stopping com.fsoft.trackeddi2 appid=10215 user=0: from pid 25784
01-26 17:19:43.562   594  2154 I ActivityManager: Killing 25446:com.fsoft.trackeddi2/u0a215 (adj 700): stop com.fsoft.trackeddi2 due to from pid 25784
01-26 17:19:43.564   594  2154 W ActivityTaskManager: Force removing ActivityRecord{a5d1bc5 u0 com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity t87 f}}: app died, no saved state
--------- beginning of main
01-26 17:19:43.565   594  2154 W InputManager-JNI: Input channel object 'd00c61b com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity (client)' was disposed without first being removed with the input manager!
01-26 17:19:43.567   594   704 W UsageStatsService: Unexpected activity event reported! (com.fsoft.trackeddi2/crc6421371f0c5b9a65b2.MainActivity event : 23 instanceId : 57343051)
01-26 17:19:43.573   594  2154 I AppsFilter: interaction: PackageSetting{6891b74 com.android.microdroid.empty_payload/10194} -> PackageSetting{bcf6266 com.fsoft.trackeddi2/10215} BLOCKED
01-26 17:19:43.589  1053  1053 D CarrierSvcBindHelper: onHandleForceStop: [com.fsoft.trackeddi2]
01-26 17:19:43.593   594   989 V ActivityManager: Got obituary of 25446:com.fsoft.trackeddi2
01-26 17:19:43.601   594   806 I AppWidgetServiceImpl: Updating package stopped masked state for uid 10215 package com.fsoft.trackeddi2 isStopped true
                  */

                  await Task.Run(() => {
                     myActivityWithResult?.Start(intent);
                     manualResetEvent4Request.WaitOne();    // warten auf Abschluss der Activity
                  });
               }
            }
            return IsGranted;
         }


         /// <summary>
         /// liefert einen Intent für den Zugriff
         /// </summary>
         /// <param name="volidx">Volumeindex (nur VOR Android 11 nötig)</param>
         /// <returns></returns>
         Intent? getAccessIntent(int volidx) {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Q)             // vor Android 10

               return getAccessIntent_PreQ(volidx);

            else if (Build.VERSION.SdkInt == BuildVersionCodes.Q)       // Android 10

               return getAccessIntent_Q(volidx);

            else

               return getAccessIntent_R();                              // ab Android 11
         }

         /// <summary>
         /// ab Android 11
         /// </summary>
         /// <returns></returns>
         Intent? getAccessIntent_R() {
            Intent? intent = null;
            /*
            Request All files access
            An app can request All files access from the user by doing the following:

               Declare the MANAGE_EXTERNAL_STORAGE permission in the manifest.
               Use the ACTION_MANAGE_ALL_FILES_ACCESS_PERMISSION intent action to direct users to a system settings page 
               where they can enable the following option for your app: Allow access to manage all files.

            To determine whether your app has been granted the MANAGE_EXTERNAL_STORAGE permission, 
            call Environment.isExternalStorageManager().


               ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION
               Activity Action: Show screen for controlling if the app specified in the data URI of the intent can manage external storage.

               Launching the corresponding activity requires the permission Manifest.permission#MANAGE_EXTERNAL_STORAGE.
               In some cases, a matching Activity may not exist, so ensure you safeguard against this.
               Input: The Intent's data URI MUST specify the application package name whose ability of managing external storage 
               you want to control. 

               ACTION_MANAGE_ALL_FILES_ACCESS_PERMISSION
               Added in API level 30
               Activity Action: Show screen for controlling which apps have access to manage external storage.
               In some cases, a matching Activity may not exist, so ensure you safeguard against this.
               If you want to control a specific app's access to manage external storage, use 
               ACTION_MANAGE_APP_ALL_FILES_ACCESS_PERMISSION instead. 
             */
            Android.Content.Context? context = FSofTUtils.OSInterface.AndroidFunction.GetActivityContext(activity);

            if (context != null &&
                !FSofTUtils.OSInterface.AndroidFunction.IsExternalStorageManager) // sonst ex. die Rechte schon
               intent = new Intent(FSofTUtils.OSInterface.AndroidConstant.ActionManageAppAllFilesAccessPermission,
                                   Android.Net.Uri.Parse("package:" + context.PackageName));

            //intent = new Intent(Android.Provider.Settings.ActionManageAllFilesAccessPermission);

            // 		resultCode	Android.App.Result.Canceled	Android.App.Result
            // ABER: Recht wurde gesetzt
            // Nur hierüber kann das Recht vom Nutzer auch wieder zurückgesetzt werden.
            // z.B. Einstellunge -> Datenschutz -> Berechtigungsmanager -> Dateien und Medien -> dann bestimmte App auswählen usw.
            return intent;
         }

         /// <summary>
         /// für Android 10
         /// </summary>
         /// <param name="volidx"></param>
         /// <returns></returns>
         static Intent? getAccessIntent_Q(int volidx) {
            Intent? intent = null;
            List<string> storagenames = getStorageVolumeNames();

            if (0 <= volidx && volidx < storagenames.Count) {
               /* Dieser Intent führt leider auch zum Öffnen des Android-Explorers (mit der SDCARD):
               EXTRA_INITIAL_URI
                  Sets the desired initial location visible to user when file chooser is shown.
                  Applicable to Intent with actions:

                      Intent#ACTION_OPEN_DOCUMENT
                      Intent#ACTION_CREATE_DOCUMENT
                      Intent#ACTION_OPEN_DOCUMENT_TREE

                  Location should specify a document URI or a tree URI with document ID. If this URI identifies a non-directory, document navigator will attempt to 
                  use the parent of the document as the initial location.

                  The initial location is system specific if this extra is missing or document navigator failed to locate the desired initial location.
                */
               intent = new Intent(Intent.ActionOpenDocumentTree);
               intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
               intent.PutExtra(FSofTUtils.OSInterface.AndroidConstant.DocumentsContractExtraInitialUri,
                               FSofTUtils.OSInterface.AndroidConstant.ContentExternalstorageTree + storagenames[volidx] + "%3A");
            }

            return intent;
         }

         /// <summary>
         /// vor Android 10
         /// </summary>
         /// <param name="volidx"></param>
         /// <returns></returns>
         static Intent? getAccessIntent_PreQ(int volidx) {
            Intent? intent = null;
            StorageVolume? sv = getStorageVolume(volidx);
            if (sv != null) {
               /* Versuche:
                     // -- NO WAY
                     Android.Net.Uri uri;
                     uri = null;
                     uri = GetTreeDocumentUri("primary");
                     uri = GetDocumentUriUsingTree("primary", global::Android.OS.Environment.DirectoryMusic);
                     intent = new Intent(Intent.ActionOpenDocumentTree, uri);   // Constant Value: "android.intent.action.OPEN_DOCUMENT_TREE" 
                     if (intent == null)
                        return false;
                     intent.AddCategory(Intent.CategoryOpenable);
                     intent.SetType("* /*");

                     // -- OK, öffnet Android-Auswahl
                     intent = new Intent(Intent.ActionOpenDocument);       // Constant Value: "android.intent.action.OPEN_DOCUMENT" 
                     intent.AddCategory(Intent.CategoryOpenable);
                     intent.SetType("* /*");

                     // -- OK
                     string pextdir = global::Android.OS.Environment.DirectoryMusic;
                     pextdir = global::Android.OS.Environment.DirectoryDocuments;
                     intent = sm.StorageVolumes[0].CreateAccessIntent(pextdir);

                     // -- NO WAY
                     intent = new Intent();
                     intent.AddFlags(ActivityFlags.GrantPersistableUriPermission | ActivityFlags.GrantPrefixUriPermission | ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
                     Android.Net.Uri voluri = GetTreeDocumentUri("primary");
                     intent.SetData(voluri);

              StorageVolume
              -------------

              public Intent createAccessIntent (String directoryName)                 Added in API level 24 (N), Deprecated in API level 29 (Q)

              This method was deprecated in API level 29.
              Callers should migrate to using Intent#ACTION_OPEN_DOCUMENT_TREE instead. 
              Launching this Intent on devices running Build.VERSION_CODES.Q or higher, will immediately finish with a result code of Activity.RESULT_CANCELED.           !!!! MIST !!!!

              Builds an intent to give access to a standard storage directory or entire volume after obtaining the user's approval.

              When invoked, the system will ask the user to grant access to the requested directory (and its descendants). The result of the request will be returned 
              to the activity through the onActivityResult method. 

              To gain access to descendants (child, grandchild, etc) documents, use DocumentsContract#buildDocumentUriUsingTree(Uri, String), or 
              DocumentsContract#buildChildDocumentsUriUsingTree(Uri, String) with the returned URI.

              If your application only needs to store internal data, consider using Context.getExternalFilesDirs, Context#getExternalCacheDirs(), 
              or Context#getExternalMediaDirs(), which require no permissions to read or write.

              Access to the entire volume is only available for non-primary volumes (for the primary volume, apps can use the 
              Manifest.permission.READ_EXTERNAL_STORAGE and Manifest.permission.WRITE_EXTERNAL_STORAGE permissions) and should be used with caution, 
              since users are more likely to deny access when asked for entire volume access rather than specific directories.
               */

               // CreateAccessIntent(null) ist veraltet. Intern erfolgt in StorageVolume:
               intent = new Intent(FSofTUtils.OSInterface.AndroidConstant.AndroidOsStorageActionOpenExternalDirectory);
               intent.PutExtra(FSofTUtils.OSInterface.AndroidConstant.DocumentsContractExtraInitialUri, sv);
               intent.PutExtra(FSofTUtils.OSInterface.AndroidConstant.AndroidOsStorageExtraDirectoryName, null as string);

            }
            return intent;
         }

         /// <summary>
         /// liefert die Namen aller akt. Volumes
         /// </summary>
         /// <returns></returns>
         static List<string> getStorageVolumeNames() {
            List<string> storageVolumeNames = [];
            StorageManager? sm = FSofTUtils.OSInterface.AndroidFunction.GetStorageManager();
            if (sm != null &&
                System.OperatingSystem.IsAndroidVersionAtLeast(24))
               foreach (StorageVolume sv in sm.StorageVolumes) {
                  if (sv.IsPrimary)
                     storageVolumeNames.Add("primary");
                  else
                     if (sv.Uuid != null) storageVolumeNames.Add(sv.Uuid);
               }
            return storageVolumeNames;
         }

         /// <summary>
         /// liefert das StorageVolume für den Index oder null
         /// </summary>
         /// <param name="volidx"></param>
         /// <returns></returns>
         static StorageVolume? getStorageVolume(int volidx) {
            StorageManager? sm = FSofTUtils.OSInterface.AndroidFunction.GetStorageManager();
            return sm != null ?
                        FSofTUtils.OSInterface.AndroidFunction.GetStorageVolume(sm, volidx) :
                        null;
         }

         /// <summary>
         /// versucht die Schreib- und Leserechte für dieses Volume zu setzen (API level 19 / Kitkat / 4.4) 
         /// (nur für secondary external storage nötig)
         /// <para>Ab BuildVersionCodes.R wird nur noch der akt. Status geholt.</para>
         /// </summary>
         /// <param name="volumeidx"></param>
         /// <returns>true, wenn die Rechte ex.</returns>
         bool setPersistentPermissions(int volumeidx) {
            /*
               https://developer.xamarin.com/api/type/Android.Content.ActivityFlags/

               GrantReadUriPermission	
               If set, the recipient of this Intent will be granted permission to perform read operations on the Uri in the Intent's data and any URIs specified in its ClipData. 
               When applying to an Intent's ClipData, all URIs as well as recursive traversals through data or other ClipData in Intent items will be granted; only the grant flags 
               of the top-level Intent are used. 

               GrantWriteUriPermission	
               If set, the recipient of this Intent will be granted permission to perform write operations on the Uri in the Intent's data and any URIs specified in its ClipData. 
               When applying to an Intent's ClipData, all URIs as well as recursive traversals through data or other ClipData in Intent items will be granted; only the grant flags 
               of the top-level Intent are used. 

               GrantPersistableUriPermission
               When combined with FLAG_GRANT_READ_URI_PERMISSION and/or FLAG_GRANT_WRITE_URI_PERMISSION, the URI permission grant can be persisted across device reboots until 
               explicitly revoked with Context.revokeUriPermission(Uri, int). 
               This flag only offers the grant for possible persisting; the receiving application must call ContentResolver.takePersistableUriPermission(Uri, int) to actually persist.

               GrantPrefixUriPermission
               When combined with FLAG_GRANT_READ_URI_PERMISSION and/or FLAG_GRANT_WRITE_URI_PERMISSION, the URI permission grant applies to any URI that is a prefix match against 
               the original granted URI. (Without this flag, the URI must match exactly for access to be granted.) 
               Another URI is considered a prefix match only when scheme, authority, and all path segments defined by the prefix are an exact match. 
             */
            ActivityFlags flags = ActivityFlags.GrantReadUriPermission |
                                  ActivityFlags.GrantWriteUriPermission;

            bool ok = false;
            try {

               if (Build.VERSION.SdkInt < BuildVersionCodes.R) {
                  List<string> volumenames = getStorageVolumeNames();
                  if (0 <= volumeidx && volumeidx < volumenames.Count) {

                     // Build URI representing access to descendant documents of the given Document#COLUMN_DOCUMENT_ID. (API level 21 / Lollipop / 5.0)
                     // z.B. "primary" oder "19F4-0903"
                     Android.Net.Uri? voluri = DocumentsContract.BuildTreeDocumentUri(FSofTUtils.OSInterface.AndroidConstant.AuthorityExternalStorageDocuments,
                                                                                      volumenames[volumeidx] + ":");
                     if (voluri != null) {
                        /* API level 19 / Kitkat / 4.4
                         *
                         * Take a persistable URI permission grant that has been offered. 
                         * Once taken, the permission grant will be remembered across device reboots. Only URI permissions granted with Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION can be 
                         * persisted. If the grant has already been persisted, taking it again will touch UriPermission.PersistedTime.
                         * 
                         * Value is either 0 or combination of FLAG_GRANT_READ_URI_PERMISSION or FLAG_GRANT_WRITE_URI_PERMISSION.
                         */

                        Android.Content.ContentResolver? resolver = FSofTUtils.OSInterface.AndroidFunction.GetActivityContentResolver(activity);
                        if (resolver != null) {
                           if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                              // Exception z.B.: No persistable permission grants found for UID 10084 and Uri 0 @ content://com.android.externalstorage.documents/tree/19F4-0903:
                              /* https://stackoverflow.com/questions/67810037/takepersistableuripermission-via-action-open-document-fails-on-a-custom-document
                               *    ... it's the expected behavior on API 19-25 when working with SAF.
                               */
                              try {
                                 resolver.TakePersistableUriPermission(voluri, flags);
                              } catch { } // ignore
                           else
                              resolver.TakePersistableUriPermission(voluri, flags);

                           /* Return list of all URI permission grants that have been persisted by the calling app. That is, the returned permissions have been granted to the calling app. 
                            * Only persistable grants taken with ContentResolver.TakePersistableUriPermission(Uri,ActivityFlags) are returned.
                            * Note: Some of the returned URIs may not be usable until after the user is unlocked.
                            */
                           foreach (UriPermission perm in resolver.PersistedUriPermissions) {
                              if (voluri.Equals(perm.Uri))
                                 ok = true;
                           }
                        }
                     }
                  }
               } else {
                  ok = FSofTUtils.OSInterface.AndroidFunction.IsExternalStorageManager; // Ex. die Rechte?
               }
            } catch (Exception ex) {
               System.Diagnostics.Debug.WriteLine("Exception in SetPersistentPermissions(" + volumeidx + "): " + ex.Message);
            }
            return ok;
         }

      }

      #endregion

      #region PermissionRequest_ManageOverlayPermission 

      class PermissionRequest_ManageOverlayPermission {

         class MyActivityWithResult {

            class MyActivityResultCallback : Java.Lang.Object, AndroidX.Activity.Result.IActivityResultCallback {
               readonly Action<AndroidX.Activity.Result.ActivityResult> _callback;

               public MyActivityResultCallback(Action<AndroidX.Activity.Result.ActivityResult> callback) => _callback = callback;

               public void OnActivityResult(Java.Lang.Object? result) {
                  if (result != null &&
                      result is AndroidX.Activity.Result.ActivityResult result1)
                     _callback(result1);
               }

            }

            readonly AndroidX.Activity.Result.ActivityResultLauncher? result;

            /// <summary>
            /// Registrierung des Callbacks (i.A. in OnCreate)
            /// </summary>
            /// <param name="resultaction"></param>
            public MyActivityWithResult(Action<AndroidX.Activity.Result.ActivityResult> resultaction) {
               AndroidX.Activity.ComponentActivity? activity = (AndroidX.Activity.ComponentActivity?)Platform.CurrentActivity;
               if (activity != null) {
                  result = activity.RegisterForActivityResult(
                                             new AndroidX.Activity.Result.Contract.ActivityResultContracts.StartActivityForResult(),
                                             new MyActivityResultCallback(resultaction));
               }
            }

            /// <summary>
            /// startet die Activity und kehrt sofort zurück
            /// </summary>
            /// <param name="intent"></param>
            public void Start(Intent intent) => result?.Launch(intent);

         }


         readonly Activity activity;

         /// <summary>
         /// Ex. das Recht?
         /// </summary>
         public bool IsGranted { get; protected set; } = false;

         /// <summary>
         /// zum warten auf das Ergebnis
         /// </summary>
         readonly ManualResetEvent manualResetEvent4Request;

         readonly MyActivityWithResult? myActivityWithResult;


         public PermissionRequest_ManageOverlayPermission(Activity activity) {
            this.activity = activity;
            manualResetEvent4Request = new ManualResetEvent(false);
            myActivityWithResult = new MyActivityWithResult((AndroidX.Activity.Result.ActivityResult result) => {
               IsGranted = PermissionExists(activity);
               manualResetEvent4Request.Set();     // Signal, dass die Activity jetzt abgeschlossen ist
            });
         }

         public static bool PermissionExists(Activity activity) => !OperatingSystem.IsAndroidVersionAtLeast(23) || Settings.CanDrawOverlays(activity);


         /// <summary>
         /// führt zur Anfrage an den Nutzer, ob die Permissions erteilt werden sollen
         /// </summary>
         /// <returns></returns>
         public async Task<bool> Start() {
            IsGranted = PermissionExists(activity);

            if (!IsGranted) {

               Intent? intent = getAccessIntent();
               if (intent != null) {
                  await Task.Run(() => {
                     myActivityWithResult?.Start(intent);
                     manualResetEvent4Request.WaitOne();    // warten auf Abschluss der Activity
                  });
               }
            }
            return IsGranted;
         }

         /// <summary>
         /// liefert einen Intent für den Zugriff
         /// </summary>
         /// <returns></returns>
         static Intent? getAccessIntent() {
            Intent? intent = null;
            if (Android.App.Application.Context != null)
               intent = new Intent(FSofTUtils.OSInterface.AndroidConstant.ActionManageOverlayPermission,
                                   Android.Net.Uri.Parse("package:" + Android.App.Application.Context.PackageName));
            return intent;
         }

      }

      #endregion


      public const string SPECIALPERM_MANAGEEXTERNALSTORAGE = Manifest.Permission.ManageExternalStorage;
      public const string SPECIALPERM_MANAGEOVERLAYPERMISSION = Settings.ActionManageOverlayPermission;

      static PermissionRequest? permissionRequest = null;
      static PermissionRequest_ManageExternalStorage? prManageExternalStorage = null;
      static PermissionRequest_ManageOverlayPermission? prManageOverlayPermission = null;

      static Activity? activity;

      static List<OSPermission> neededPermissions = [];


      /// <summary>
      /// muss unbedingt in OnCreate() erfolgen
      /// <para>sonst fkt. RegisterForActivityResult() NICHT:</para>
      /// <para>Note: You must call registerForActivityResult() before the fragment or activity is created, 
      /// but you can't launch the ActivityResultLauncher until the fragment or activity's Lifecycle
      /// has reached CREATED.</para>
      /// </summary>
      /// <param name="mainactivity"></param>
      public static void Init(Activity mainactivity, List<OSPermission> permissions) {
         activity = mainactivity;
         neededPermissions = permissions;

         permissionRequest = new PermissionRequest(activity);

         // für Sonderfälle:
         foreach (var perm in permissions) {
            if (perm.Name == SPECIALPERM_MANAGEEXTERNALSTORAGE)
               prManageExternalStorage = new PermissionRequest_ManageExternalStorage(activity);
            if (perm.Name == SPECIALPERM_MANAGEOVERLAYPERMISSION)
               prManageOverlayPermission = new PermissionRequest_ManageOverlayPermission(activity);
         }
      }

      /// <summary>
      /// fordert alle notwendigen Rechte an
      /// </summary>
      /// <returns>true wenn alle notwendigen(!) Rechte erteilt sind</returns>
      public async static Task<bool> Request() => await request();

      async static Task<bool> request(Activity activity) => await request(activity, neededPermissions);

      async static Task<bool> request() => activity != null && await request(activity, neededPermissions);

      async static Task<bool> request(Activity activity, List<OSPermission> permissions) {
         bool ok = permissions.Count == 0;

         if (permissions.Count > 0) {

            /* Nach dem einmaligen Ablehnen der Rechte AccessCoarseLocation und AccessFineLocation wird der Abfragedialog
             * von Android NICHT mehr aufgerufen.
             * Dann muss man manuell in die Einstellungen gehen!
             */

            List<string> errors = [];

            if (permissionRequest != null)
               foreach (var perm in permissions) {
                  string pname = perm.Name;
                  if (pname != SPECIALPERM_MANAGEEXTERNALSTORAGE &&                // Sonderfälle
                      pname != SPECIALPERM_MANAGEOVERLAYPERMISSION) {

                     if (System.OperatingSystem.IsAndroidVersionAtLeast(23)) {
                        if (activity.CheckSelfPermission(pname) != Permission.Granted) {   // wenn noch nicht vorhanden, dann anforden
                           if (!string.IsNullOrEmpty(perm.Description) &&
                               activity.ShouldShowRequestPermissionRationale(pname))
                              /* ShouldShowRequestPermissionRationale()
                               * 
                               * true, wenn
                               *    When the user has denied the permission previously but has not checked the "Never Ask Again" checkbox.
                               * false
                               *    When the user has denied the permission previously AND never ask again checkbox was selected. 
                               *    When the user is requesting permission for the first time.
                               */
                              //await SimpleDialogs.ShowInfoDialog(activity, perm.Name, perm.Description);
                              await Shell.Current.DisplayAlert(pname, perm.Description, "weiter");
                           // normalerweise z.B.: await Permissions.RequestAsync<Permissions.LocationAlways>();
                           // aber es fehlt eine einfache Verbindung zwischen den Permissions. ... und den Strings aus Manifest.Permission. ...
                           List<string> e = await permissionRequest.Start(pname);
                           if (e != null && e.Count > 0)
                              errors.AddRange(e);
                        }
                     }

                  } else {    // Sonderfälle

                     if (pname == SPECIALPERM_MANAGEEXTERNALSTORAGE &&
                         prManageExternalStorage != null &&
                         !prManageExternalStorage.PermissionExists(activity)) {

                        //await Shell.Current.DisplayAlert(pname, perm.Description, "weiter");  // kann leider zu einer Arg_TargetInvocationException führen, deshalb ...
                        await SimpleDialogs.ShowInfoDialog(activity, pname, perm.Description);
                        if (!await prManageExternalStorage.Start())                    // wenn das Recht benötigt wird aber nicht erteilt wurde ...
                           errors.Add(pname);                                      // wird es zur Fehlerliste hinzugefügt
                        prManageExternalStorage = null;

                     } else if (pname == SPECIALPERM_MANAGEOVERLAYPERMISSION &&
                                prManageOverlayPermission != null &&
                                !PermissionRequest_ManageOverlayPermission.PermissionExists(activity)) {

                        await Shell.Current.DisplayAlert(pname, perm.Description, "weiter");
                        if (!await prManageOverlayPermission.Start())                  // wenn das Recht benötigt wird aber nicht erteilt wurde ...
                           errors.Add(pname);                                      // wird es zur Fehlerliste hinzugefügt
                        prManageOverlayPermission = null;

                     }

                  }
               }

            // ev. nicht unbedingt nötige Perms aus der Liste entfernen
            for (int i = errors.Count - 1; i >= 0; i--) {
               OSPermission? p = permissions.Find(item => item.Name == errors[i]);
               if (p != null && !p.Necessary)
                  errors.RemoveAt(i);
            }

            if (errors.Count > 0) {
               StringBuilder sb = new("Der App fehlen die für ihre Ausführung notwendigen Rechte:");
               sb.AppendLine();
               sb.AppendLine();
               foreach (string perm in errors)
                  sb.AppendLine(perm);
               await SimpleDialogs.ShowFinishDialog(activity, "Sorry", sb.ToString());
            } else
               ok = true;
         }

         return ok;
      }

   }
}
