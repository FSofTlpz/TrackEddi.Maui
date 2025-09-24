namespace FSofTUtils.OSInterface {
   public class AndroidConstant {
#pragma warning disable CA1416

      /// <summary>
      /// ab API 30, Android 11, R
      /// </summary>
      public const string ActionManageAllFilesAccessPermission = Android.Provider.Settings.ActionManageAllFilesAccessPermission;

      /// <summary>
      /// ab API 30, Android 11, R
      /// </summary>
      public const string ActionManageAppAllFilesAccessPermission = Android.Provider.Settings.ActionManageAppAllFilesAccessPermission;

      /// <summary>
      /// ab API 23, Android 6, M
      /// </summary>
      public const string ActionManageOverlayPermission = Android.Provider.Settings.ActionManageOverlayPermission;

      /// <summary>
      /// ab API 26, Android 8, O
      /// </summary>
      public const string DocumentsContractExtraInitialUri = Android.Provider.DocumentsContract.ExtraInitialUri;

      public const string IntentActionOpenDocumentTree = Android.Content.Intent.ActionOpenDocumentTree;

      /// <summary>
      /// ab API 24, Android 7.0, N
      /// </summary>
      public const string ExtraStorageVolume = Android.OS.Storage.StorageVolume.ExtraStorageVolume;

      /// <summary>
      /// ab API 30, Android 11, R
      /// </summary>
      public const string ManifestPermissionManageExternalStorage = Android.Manifest.Permission.ManageExternalStorage;

      public const string AuthorityExternalStorageDocuments = "com.android.externalstorage.documents";

      public const string AndroidOsStorageActionOpenExternalDirectory = "android.os.storage.action.OPEN_EXTERNAL_DIRECTORY";

      public const string AndroidOsStorageExtraDirectoryName = "android.os.storage.extra.DIRECTORY_NAME";

      public const string ContentExternalStorageTree = "content://com.android.externalstorage.documents/tree/";

      public const string RootIdPrimaryEmulated = "primary";
      //const string ROOT_ID_HOME = "home";

      public const string ContentExternalstorageTree = "content://com.android.externalstorage.documents/tree/";

   }

}
