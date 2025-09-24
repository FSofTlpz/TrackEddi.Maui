using Android.OS;
using Android.OS.Storage;
using Android.Runtime;
using static FSofTUtils.OSInterface.Storage.StorageHelper;

namespace FSofTUtils.OSInterface.Storage {

   /// <summary>
   /// liefert Hilfsfunktionen für Volumes und Dateioperationen
   /// </summary>
   class Volume {

      /// <summary>
      /// ext. StorageVolume-Pfade beim letzten <see cref="RefreshVolumes"/>, z.B. "/storage/emulated/0" oder "/storage/19F4-0903"
      /// </summary>
      public List<string> StorageVolumePaths { get; protected set; }

      /// <summary>
      /// ext. StorageVolume-Namen beim letzten <see cref="RefreshVolumes"/>, z.B. "primary" oder "19F4-0903"
      /// </summary>
      public List<string> StorageVolumeNames { get; protected set; }

      /// <summary>
      /// Anzahl der Volumes
      /// </summary>
      public int Volumes => StorageVolumePaths.Count;


      public Volume() {
         if (Build.VERSION.SdkInt < BuildVersionCodes.N)  // "Nougat", 7.0
            throw new Exception("Need Version 7.0 ('Nougat', API 24) or higher.");
         StorageVolumePaths = new List<string>();
         StorageVolumeNames = new List<string>();
         RefreshVolumes();
      }

      VolumeState GetVolumeState(string? strstate) {
         VolumeState state = VolumeState.MEDIA_UNKNOWN;
         try {
            /*
             MEDIA_UNKNOWN            Unknown storage state, such as when a path isn't backed by known storage media.  "unknown"  
             MEDIA_REMOVED            Storage state if the media is not present.  "removed" 
             MEDIA_UNMOUNTED          Storage state if the media is present but not mounted.  "unmounted"
             MEDIA_CHECKING           Storage state if the media is present and being disk-checked. "checking"
             MEDIA_EJECTING           Storage state if the media is in the process of being ejected. "ejecting"
             MEDIA_NOFS               Storage state if the media is present but is blank or is using an unsupported filesystem.  "nofs"
             MEDIA_MOUNTED            Storage state if the media is present and mounted at its mount point with read/write access. "mounted" 
             MEDIA_MOUNTED_READ_ONLY  Storage state if the media is present and mounted at its mount point with read-only access.  "mounted_ro" 
             MEDIA_SHARED             Storage state if the media is present not mounted, and shared via USB mass storage.  "shared"
             MEDIA_BAD_REMOVAL        Storage state if the media was removed before it was unmounted. "bad_removal"
             MEDIA_UNMOUNTABLE        Storage state if the media is present but cannot be mounted. Typically this happens if the file system on the media is corrupted.  "unmountable" 
            */
            if (strstate != null) {
               if (strstate == global::Android.OS.Environment.MediaRemoved)
                  state = VolumeState.MEDIA_REMOVED;
               else if (strstate == global::Android.OS.Environment.MediaUnmounted)
                  state = VolumeState.MEDIA_UNMOUNTED;
               else if (strstate == global::Android.OS.Environment.MediaChecking)
                  state = VolumeState.MEDIA_CHECKING;
               else if (strstate == global::Android.OS.Environment.MediaEjecting)
                  state = VolumeState.MEDIA_EJECTING;
               else if (strstate == global::Android.OS.Environment.MediaNofs)
                  state = VolumeState.MEDIA_NOFS;
               else if (strstate == global::Android.OS.Environment.MediaMounted)
                  state = VolumeState.MEDIA_MOUNTED;
               else if (strstate == global::Android.OS.Environment.MediaMountedReadOnly)
                  state = VolumeState.MEDIA_MOUNTED_READ_ONLY;
               else if (strstate == global::Android.OS.Environment.MediaShared)
                  state = VolumeState.MEDIA_SHARED;
               else if (strstate == global::Android.OS.Environment.MediaBadRemoval)
                  state = VolumeState.MEDIA_BAD_REMOVAL;
               else if (strstate == global::Android.OS.Environment.MediaUnmountable)
                  state = VolumeState.MEDIA_UNMOUNTABLE;
            } else
               state = VolumeState.MEDIA_UNKNOWN;
         } catch {
            state = VolumeState.MEDIA_UNKNOWN;
         }
         return state;
      }

      /// <summary>
      /// Hilfsfunktion für <see cref="StorageVolume"/>: die Methode getPath() ist z.Z. noch nicht umgesetzt und wird per JNI realisiert (API level 24 / Nougat / 7.0)
      /// </summary>
      /// <param name="sv"></param>
      /// <returns></returns>
      string Path4StorageVolume(StorageVolume? sv) {
         string path = "";
         try {
            if (sv != null)
               if (Build.VERSION.SdkInt < BuildVersionCodes.R) {
                  // http://journals.ecs.soton.ac.uk/java/tutorial/native1.1/implementing/method.html
                  IntPtr methodID = JNIEnv.GetMethodID(sv.Class.Handle, "getPath", "()Ljava/lang/String;");    // getPath() ex. in Android 11 nicht mehr
                  IntPtr lref = JNIEnv.CallObjectMethod(sv.Handle, methodID);
                  using (var value = new Java.Lang.Object(lref, JniHandleOwnership.TransferLocalRef)) {
                     path = value.ToString();
                  }
               } else
                  path = AndroidFunction.StorageVolumeDirectory(sv).AbsolutePath;
         } catch (Exception ex) {
            path = "";
            System.Diagnostics.Debug.WriteLine("Exception in Volume.Path4StorageVolume " + AndroidFunction.StorageMediaStoreVolumeName(sv) + ": " + ex.Message);
         }
         return path;
      }

      /// <summary>
      /// setzt die Pfade und Namen der akt. ext. StorageVolumes, z.B. "/storage/emulated/0" und 
      /// "/storage/19F4-0903" (API level 24 / Nougat / 7.0)
      /// </summary>
      /// <returns>Anzahl der Volumes</returns>
      public int RefreshVolumes() {
         StorageVolumePaths.Clear();
         StorageVolumeNames.Clear();
         StorageManager? sm = AndroidFunction.GetStorageManager();
         if (sm == null)
            return 0;
         // StorageVolumes und StorageVolume erst ab API 24 (Nougat)
         for (int i = 0; i < AndroidFunction.GetStorageVolumeCount(sm); i++) {
            StorageVolumePaths.Add(Path4StorageVolume(AndroidFunction.GetStorageVolume(sm, i)));
            if (AndroidFunction.GetStorageVolumeIsPrimary(sm, i))
               StorageVolumeNames.Add(AndroidConstant.RootIdPrimaryEmulated);
            else {
               string? uuid = AndroidFunction.GetStorageVolumeUuid(sm, i);
               if (uuid != null)
                  StorageVolumeNames.Add(uuid);
            }
         }
         return StorageVolumePaths.Count;
      }

      /// <summary>
      /// liefert die akt. Daten des Volumes (API level 24 / Nougat / 7.0)
      /// </summary>
      /// <param name="volumeno">Nummer des gewünschten Volumes (wenn nicht vorhanden, wird nur die Volumeanzahl geliefert)</param>
      /// <returns></returns>
      public VolumeData GetVolumeData(int volumeno) {
         VolumeData data = new VolumeData {
            VolumeNo = volumeno
         };
         if (volumeno >= 0) {

            StorageManager? sm = AndroidFunction.GetStorageManager();
            if (sm != null) {
               if (Build.VERSION.SdkInt >= BuildVersionCodes.N) { // "Nougat", 7.0
                  data.Volumes = AndroidFunction.GetStorageVolumeCount(sm);
                  if (0 <= volumeno && volumeno < data.Volumes) {
                     data.IsPrimary = AndroidFunction.GetStorageVolumeIsPrimary(sm, volumeno);
                     data.IsRemovable = AndroidFunction.GetStorageVolumeIsRemovable(sm, volumeno);
                     data.IsEmulated = AndroidFunction.GetStorageVolumeIsEmulated(sm, volumeno);
                     data.Path = Path4StorageVolume(AndroidFunction.GetStorageVolume(sm, volumeno));
                     data.State = GetVolumeState(AndroidFunction.GetStorageVolumeState(sm, volumeno));
                     string? tmp = AndroidFunction.GetStorageVolumeDescription(sm, volumeno);
                     if (tmp != null)
                        data.Description = tmp;
                     if (data.IsPrimary)
                        data.Name = AndroidConstant.RootIdPrimaryEmulated;
                     else {
                        string? uuid = AndroidFunction.GetStorageVolumeUuid(sm, volumeno);
                        if (uuid != null)
                           data.Name = uuid;
                     }

                     try {
                        StatFs statfs = new StatFs(data.Path);
                        data.AvailableBytes = statfs.AvailableBytes;
                        data.TotalBytes = statfs.TotalBytes;
                     } catch {
                        data.TotalBytes = data.AvailableBytes = 0;
                     }
                  }
               }
            }

         } else { // Daten des internal Storage holen

            data.IsPrimary = false;
            data.IsRemovable = false;
            data.IsEmulated = false;
            data.Path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile); // == Xamarin.Essentials.FileSystem.AppDataDirectory;
            data.Path = data.Path.Substring(0, data.Path.LastIndexOf(Path.DirectorySeparatorChar));
            data.State = VolumeState.MEDIA_MOUNTED;
            data.Description = "intern";
            data.Name = "intern";
            try {
               StatFs statfs = new StatFs(global::Android.OS.Environment.RootDirectory.CanonicalPath);
               data.AvailableBytes = statfs.AvailableBytes;
               data.TotalBytes = statfs.TotalBytes;
            } catch {
               data.TotalBytes = data.AvailableBytes = 0;
            }

         }
         return data;
      }

      /// <summary>
      /// liefert die Nummer, den Namen und Pfad des StorageVolumes zum Pfad
      /// <para>-1, nicht gefunden</para>
      /// <para>0, internal</para>
      /// <para>1, primary external</para>
      /// <para>2 usw., secondary external</para>
      /// </summary>
      /// <param name="fullpath"></param>
      /// <param name="volpath"></param>
      /// <param name="volname"></param>
      /// <returns></returns>
      public int GetKnownElems4FullPath(string fullpath, out string volpath, out string volname) {
         int v = GetVolumeNo4Path(fullpath);
         if (v >= 0) {
            volpath = StorageVolumePaths[v];
            volname = StorageVolumeNames[v];
         } else {
            volpath = volname = "";
         }
         return v;
      }

      /// <summary>
      /// liefert die Nummer des StorageVolumes zum Pfad
      /// <para>-1, nicht gefunden</para>
      /// <para>0, internal</para>
      /// <para>1, primary external</para>
      /// <para>2 usw., secondary external</para>
      /// </summary>
      /// <param name="fullpath"></param>
      /// <returns></returns>
      public int GetVolumeNo4Path(string fullpath) {
         if (!string.IsNullOrEmpty(fullpath))
            for (int i = 0; i < StorageVolumePaths.Count; i++) {
               if (fullpath.StartsWith(StorageVolumePaths[i])) {
                  if (fullpath.Length == StorageVolumePaths[i].Length ||
                      fullpath[StorageVolumePaths[i].Length] == '/')
                     return i;
               }
            }
         return -1;
      }

      /// <summary>
      /// liefert die Nummer des StorageVolumes zum Namen
      /// <para>-1, nicht gefunden</para>
      /// <para>0, internal</para>
      /// <para>1, primary external</para>
      /// <para>2 usw., secondary external</para>
      /// </summary>
      /// <param name="volumename"></param>
      /// <returns></returns>
      public int GetVolumeNo4Name(string volumename) {
         for (int i = 0; i < StorageVolumePaths.Count; i++)
            if (volumename == StorageVolumeNames[i])
               return i;
         return -1;
      }

   }

}