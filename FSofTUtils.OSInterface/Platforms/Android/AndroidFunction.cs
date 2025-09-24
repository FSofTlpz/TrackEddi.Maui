namespace FSofTUtils.OSInterface {

   /// <summary>
   /// "Interface" für Android-Funktionen (alle Abhängigkeiten sollten hier gebündelt sein)
   /// </summary>
   public class AndroidFunction {

      /// <summary>
      /// Android.App.Application.Context.GetSystemService(Android.Content.Context.StorageService)
      /// </summary>
      /// <returns></returns>
      static public Java.Lang.Object? GetStorageService() =>
         Android.App.Application.Context.GetSystemService(Android.Content.Context.StorageService);   // STORAGE_SERVICE  "storage" 

      /// <summary>
      /// Android.App.Application.Context.GetSystemService(Android.Content.Context.StorageService) as Android.OS.Storage.StorageManager
      /// </summary>
      /// <returns></returns>
      static public Android.OS.Storage.StorageManager? GetStorageManager() =>
         GetStorageService() as Android.OS.Storage.StorageManager;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// </summary>
      /// <param name="storageManager"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      static bool storageVolumeIdxIsValid(Android.OS.Storage.StorageManager storageManager, int idx) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) &&
                0 <= idx && idx < storageManager.StorageVolumes.Count;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// <para>Android.OS.Storage.StorageManager.StorageVolumes[idx]</para>
      /// </summary>
      /// <param name="storageManager"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      static public Android.OS.Storage.StorageVolume? GetStorageVolume(Android.OS.Storage.StorageManager storageManager, int idx) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) &&
                storageVolumeIdxIsValid(storageManager, idx) ?
                     storageManager.StorageVolumes[idx] :
                     null;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// <para>Android.OS.Storage.StorageManager.StorageVolumes.Count</para>
      /// </summary>
      /// <param name="storageManager"></param>
      /// <returns></returns>
      static public int GetStorageVolumeCount(Android.OS.Storage.StorageManager storageManager) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) ?
                     storageManager.StorageVolumes.Count :
                     0;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// <para>Android.OS.Storage.StorageManager.StorageVolumes[idx].Uuid</para>
      /// </summary>
      /// <param name="storageManager"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      static public string? GetStorageVolumeUuid(Android.OS.Storage.StorageManager storageManager, int idx) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) &&
                storageVolumeIdxIsValid(storageManager, idx) ?
                     storageManager.StorageVolumes[idx].Uuid :
                     null;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// <para>Android.OS.Storage.StorageManager.StorageVolumes[idx].State</para>
      /// </summary>
      /// <param name="storageManager"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      static public string? GetStorageVolumeState(Android.OS.Storage.StorageManager storageManager, int idx) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) &&
                storageVolumeIdxIsValid(storageManager, idx) ?
                     storageManager.StorageVolumes[idx].State :
                     null;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// <para>Android.OS.Storage.StorageManager.StorageVolumes[idx].GetDescription(...)</para>
      /// </summary>
      /// <param name="storageManager"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      static public string? GetStorageVolumeDescription(Android.OS.Storage.StorageManager storageManager, int idx) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) &&
                storageVolumeIdxIsValid(storageManager, idx) ?
                     storageManager.StorageVolumes[idx].GetDescription(ApplicationContext) :
                     null;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// <para>Android.OS.Storage.StorageManager.StorageVolumes[idx].IsPrimary</para>
      /// </summary>
      /// <param name="storageManager"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      static public bool GetStorageVolumeIsPrimary(Android.OS.Storage.StorageManager storageManager, int idx) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) &&
                storageVolumeIdxIsValid(storageManager, idx) ?
                     storageManager.StorageVolumes[idx].IsPrimary :
                     false;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// <para>Android.OS.Storage.StorageManager.StorageVolumes[idx].IsRemovable</para>
      /// </summary>
      /// <param name="storageManager"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      static public bool GetStorageVolumeIsRemovable(Android.OS.Storage.StorageManager storageManager, int idx) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) &&
                storageVolumeIdxIsValid(storageManager, idx) ?
                     storageManager.StorageVolumes[idx].IsRemovable :
                     false;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// <para>Android.OS.Storage.StorageManager.StorageVolumes[idx].IsEmulated</para>
      /// </summary>
      /// <param name="storageManager"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      static public bool GetStorageVolumeIsEmulated(Android.OS.Storage.StorageManager storageManager, int idx) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) &&
                storageVolumeIdxIsValid(storageManager, idx) ?
                     storageManager.StorageVolumes[idx].IsEmulated :
                     false;

      /// <summary>
      /// Android.OS.Environment.ExternalStorageDirectory.AbsolutePath
      /// </summary>
      static public string ExternalStorageDirectory =>
         Android.OS.Environment.ExternalStorageDirectory != null ?
            Android.OS.Environment.ExternalStorageDirectory.AbsolutePath :
            string.Empty;

      /// <summary>
      /// ab API 30, Android 11, R
      /// <para>Android.OS.Environment.IsExternalStorageManager</para>
      /// </summary>
      static public bool IsExternalStorageManager =>
         OperatingSystem.IsAndroidVersionAtLeast(30) ?
            Android.OS.Environment.IsExternalStorageManager :
            false;


      /// <summary>
      /// Hilfsfunktion für <see cref="Android.OS.Storage.StorageVolume"/>: 
      /// die Methode getPath() ist z.Z. noch nicht umgesetzt und wird per JNI realisiert (API level 24 / Nougat / 7.0)
      /// </summary>
      /// <param name="sv"></param>
      /// <returns></returns>
      static public string GetStorageVolumePath(Android.OS.Storage.StorageVolume sv) {
         string path = string.Empty;
         try {
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.R) {
               // http://journals.ecs.soton.ac.uk/java/tutorial/native1.1/implementing/method.html
               IntPtr methodID = Android.Runtime.JNIEnv.GetMethodID(sv.Class.Handle, "getPath", "()Ljava/lang/String;");    // getPath() ex. in Android 11 nicht mehr
               IntPtr lref = Android.Runtime.JNIEnv.CallObjectMethod(sv.Handle, methodID);
               using (var value = new Java.Lang.Object(lref, Android.Runtime.JniHandleOwnership.TransferLocalRef)) {
                  path = value.ToString();
               }
            } else {
               if (System.OperatingSystem.IsAndroidVersionAtLeast(30))
                  path = sv.Directory != null ? sv.Directory.AbsolutePath : string.Empty;
            }

         } catch (Exception ex) {
            path = string.Empty;
            System.Diagnostics.Debug.WriteLine("Exception in " +
                                               nameof(GetStorageVolumePath) + "() " +
                                               (System.OperatingSystem.IsAndroidVersionAtLeast(30) ? 
                                                         sv.MediaStoreVolumeName : 
                                                         string.Empty) +
                                               ": " +
                                               ex.Message);
         }
         return path;
      }



      /// <summary>
      /// Android.App.Application.Context
      /// </summary>
      static public Android.Content.Context? ApplicationContext =>
         Android.App.Application.Context;

      /// <summary>
      /// Android.App.Activity.ApplicationContext
      /// </summary>
      /// <param name="activity"></param>
      /// <returns></returns>
      static public Android.Content.Context? GetActivityContext(Android.App.Activity activity) =>
          activity.ApplicationContext;

      /// <summary>
      /// Android.App.Activity.ContentResolver
      /// </summary>
      /// <param name="activity"></param>
      /// <returns></returns>
      static public Android.Content.ContentResolver? GetActivityContentResolver(Android.App.Activity activity) =>
          activity.ContentResolver;

      /// <summary>
      /// Android.App.Application.Context.ContentResolver
      /// </summary>
      static public Android.Content.ContentResolver? GetContentResolver =>
         Android.App.Application.Context.ContentResolver;

      /// <summary>
      /// Android.Provider.DocumentsContract.BuildTreeDocumentUri(...)
      /// </summary>
      /// <param name="authority"></param>
      /// <param name="docid"></param>
      /// <returns></returns>
      static public Android.Net.Uri? DocumentsContractBuildTreeDocumentUri(string authority, string docid) =>
          Android.Provider.DocumentsContract.BuildTreeDocumentUri(authority, docid);

      /// <summary>
      /// Android.Provider.DocumentsContract.BuildDocumentUriUsingTree(...)
      /// </summary>
      /// <param name="treeuri"></param>
      /// <param name="docid"></param>
      /// <returns></returns>
      static public Android.Net.Uri? DocumentsContractBuildDocumentUriUsingTree(Android.Net.Uri treeuri, string docid) =>
          Android.Provider.DocumentsContract.BuildDocumentUriUsingTree(treeuri, docid);

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// <para>Android.Provider.DocumentsContract.MoveDocument(...)</para>
      /// </summary>
      /// <param name="contentResolver"></param>
      /// <param name="srcuri"></param>
      /// <param name="dstpathuri"></param>
      /// <param name="dstdocuri"></param>
      /// <returns></returns>
      static public Android.Net.Uri? DocumentsContractMoveDocument(Android.Content.ContentResolver contentResolver,
                                                                   Android.Net.Uri srcuri,
                                                                   Android.Net.Uri dstpathuri,
                                                                   Android.Net.Uri dstdocuri) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) ?
                  Android.Provider.DocumentsContract.MoveDocument(contentResolver, srcuri, dstpathuri, dstdocuri) :
                  null;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// <para>Android.Provider.DocumentsContract.CopyDocument(...)</para>
      /// </summary>
      /// <param name="contentResolver"></param>
      /// <param name="srcuri"></param>
      /// <param name="dstpathuri"></param>
      /// <returns></returns>
      static public Android.Net.Uri? DocumentsContractCopyDocument(Android.Content.ContentResolver contentResolver,
                                                                   Android.Net.Uri srcuri,
                                                                   Android.Net.Uri dstpathuri) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(24) ?
                  Android.Provider.DocumentsContract.CopyDocument(contentResolver, srcuri, dstpathuri) :
                  null;

      /// <summary>
      /// ab API 30, Android 11, R
      /// <para>Android.OS.Storage.StorageVolume.Directory</para>
      /// </summary>
      /// <param name="sv"></param>
      /// <returns></returns>
      static public Java.IO.File StorageVolumeDirectory(Android.OS.Storage.StorageVolume sv) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(30) &&
                sv.Directory != null ?
                   sv.Directory :
                   new Java.IO.File(string.Empty);

      /// <summary>
      /// ab API 30, Android 11, R
      /// <para>Android.OS.Storage.StorageVolume.MediaStoreVolumeName</para>
      /// </summary>
      /// <param name="sv"></param>
      /// <returns></returns>
      static public string StorageMediaStoreVolumeName(Android.OS.Storage.StorageVolume? sv) =>
         System.OperatingSystem.IsAndroidVersionAtLeast(30) &&
                sv != null &&
                sv.MediaStoreVolumeName != null ?
                  sv.MediaStoreVolumeName :
                  string.Empty;

      /// <summary>
      /// Android.Net.Uri.Parse(...)
      /// </summary>
      /// <param name="uri"></param>
      /// <returns></returns>
      static public Android.Net.Uri? UriParse(string? uri) =>
         uri != null ? Android.Net.Uri.Parse(uri) : null;

      /// <summary>
      /// ab API 29, Android 10, Q
      /// <para>Android.Provider.MediaStore.GetExternalVolumeNames(...)</para>
      /// </summary>
      /// <param name="context"></param>
      /// <returns></returns>
      static public ICollection<string>? MediaStoreGetExternalVolumeNames(Android.Content.Context context) {
         return System.OperatingSystem.IsAndroidVersionAtLeast(29) ?
                     Android.Provider.MediaStore.GetExternalVolumeNames(context) :
                     null;
      }

      /// <summary>
      /// Android.Provider.MediaStore.Files.GetContentUri(...)
      /// </summary>
      /// <param name="uri"></param>
      /// <returns></returns>
      static public Android.Net.Uri? MediaStoreGetContentUri(string? uri) =>
         uri != null ? Android.Provider.MediaStore.Files.GetContentUri(uri) : null;

   }

}
