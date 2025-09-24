#if USE_STORAGEACCESSFRAMEWORK
namespace FSofTUtils.OSInterface.Storage {

   /// <summary>
   /// <see cref="StorageHelper.StorageItem"/> auf Basis des Storage Access Framework
   /// </summary>
   class SafStorageItem : StorageHelper.StorageItem {

      public SafStorageItem(AndroidX.DocumentFile.Provider.DocumentFile file) {
         Name = file.Name != null ? file.Name : "";
         IsDirectory = file.IsDirectory;
         IsFile = file.IsFile;
         MimeType = file.Type != null ? file.Type : "";
         CanRead = file.CanRead();
         CanWrite = file.CanWrite();
         LastModified = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime().AddMilliseconds(file.LastModified()); // Bezug auf den 1.1.1970
         Length = file.Length();
      }

   }
}
#endif