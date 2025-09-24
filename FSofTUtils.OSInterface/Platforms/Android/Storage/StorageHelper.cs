using Android.App;
#if USE_STORAGEACCESSFRAMEWORK
using AndroidX.DocumentFile.Provider;
#endif

namespace FSofTUtils.OSInterface.Storage {

   /// <summary>
   /// StorageVolumes und StorageVolume erst ab API 24 (Nougat, 7.0)
   /// </summary>
   public partial class StorageHelper {

      /* Für das Volume 0 werden immer die Standarddateifunktionen verwendet.
       * 
       * Für die weiteren Volumes (z.Z. max. 1, "external Storage") werden
       *    vor Android Q auch die Standarddateifunktionen
       *    bei Android Q je nach USESTD4Q entweder die Standarddateifunktionen oder das Storage Access Framework
       *    ab Android R die Funktionen des Storage Access Framework 
       * verwendet.
       * 
       * Es muss in AndroidManifest.xml
       * 	<application ... android:requestLegacyExternalStorage="true"></application>
       * aktiviert sein!
       * 
       */

      readonly Activity? activity;

      readonly Volume vol;

#if USE_STORAGEACCESSFRAMEWORK
      readonly SafStorageHelper saf;
      public const bool UseAndroidStorageAccessFrameWork = true;
#endif
      public const bool UseAndroidStorageAccessFrameWork = false;

      /// <summary>
      /// Version VOR Android-R (11)
      /// </summary>
      readonly bool ispre_r = false;

      /// <summary>
      /// Version VOR Android-Q (10)
      /// </summary>
      readonly bool ispre_q = false;

      /*    Achtung
       *    
       *    Auf dem dem primary external storage (emuliert; interne SD-Karte) werden die Funktionen mit URI, DocumentsContract und DocumentFile NICHT benötigt 
       *    (und funktionieren auch nicht). Hier sollten alle normalen .Net-Funktionen funktionieren.
       *    
       *    Erst auf dem secondary external storage (echte SD-Karte und/oder USB-Stick) sind diese Funktionen nötig (Storage Access Framework). Dafür sind auch nochmal zusätzliche Rechte erforderlich.
       *    "PersistentPermissions".
       *    Das zentrale Element ist der DocumentsProvider über den der Zugriff auf die "Dokumente" erfolgt.
       */


      public StorageHelper(object activity) {
         ispre_q = Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Q;  // 10
         ispre_r = Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.Q;

         if (activity != null && activity is Activity) {
            this.activity = activity as Activity;
            vol = new Volume();
#if USE_STORAGEACCESSFRAMEWORK
            saf = new SafStorageHelper(this.activity);
#endif

            RefreshVolumesAndSpecPaths();
         } else
            throw new Exception("'activity' must be a valid Activity.");
      }

#if USE_STORAGEACCESSFRAMEWORK
      #region Android-Permissions (Storage-Access-Framework)

      /// <summary>
      /// versucht die Schreib- und Leserechte für das Volume zu setzen (API level 19 / Kitkat / 4.4) (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="storagename">z.B. "19F4-0903"</param>
      /// <returns></returns>
      public partial bool SetSafAndroidPersistentPermissions(string storagename) =>
         SetSafAndroidPersistentPermissions(idx4VolumeName(storagename));

      /// <summary>
      /// versucht die Schreib- und Leserechte für das Volume zu setzen (API level 19 / Kitkat / 4.4) (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="volidx">Volume-Index (min. 1)</param>
      /// <returns></returns>
      public partial bool SetSafAndroidPersistentPermissions(int volidx) {
         return 0 <= volidx ?
                     saf.SetPersistentPermissions(VolumeNames[volidx], volidx) :
                     false;
      }

      /// <summary>
      /// gibt die persistenten Schreib- und Leserechte frei (API level 19 / Kitkat / 4.4) (für primäres externes Volume nicht geeignet!)
      /// <param name="storagename">z.B. "19F4-0903"</param>
      /// </summary>
      public partial void ReleaseSafAndroidPersistentPermissions(string storagename) =>
         ReleaseSafAndroidPersistentPermissions(idx4VolumeName(storagename));

      /// <summary>
      /// gibt die persistenten Schreib- und Leserechte frei (API level 19 / Kitkat / 4.4) (für primäres externes Volume nicht geeignet!)
      /// <param name="volidx">Volume-Index (min. 1)</param>
      /// </summary>
      public partial void ReleaseSafAndroidPersistentPermissions(int volidx) {
         if (0 <= volidx)
            saf.ReleasePersistentPermissions(VolumeNames[volidx], volidx);
      }

      /// <summary>
      /// führt zur Anfrage an den Nutzer, ob die Permissions erteilt werden sollen (API level 24 / Nougat / 7.0; deprecated in API level Q) 
      /// (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="storagename">z.B. "19F4-0903"</param>
      /// <param name="requestid">ID für OnActivityResult() der activity</param>
      public partial void Ask4SafAndroidPersistentPermisson(string storagename, int requestid) =>
         Ask4SafAndroidPersistentPermisson(idx4VolumeName(storagename), requestid);

      /// <summary>
      /// führt zur Anfrage an den Nutzer, ob die Permissions erteilt werden sollen (API level 24 / Nougat / 7.0; deprecated in API level Q) 
      /// (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="volidx">Volume-Index (min. 1)</param>
      /// <param name="requestid">ID für OnActivityResult() der activity</param>
      public partial void Ask4SafAndroidPersistentPermisson(int volidx, int requestid) {
         if (0 <= volidx)
            saf.Ask4PersistentPermisson(VolumeNames[volidx], volidx, requestid);
      }

      /// <summary>
      /// führt zur Anfrage an den Nutzer, ob die Permissions erteilt werden sollen (API level 24 / Nougat / 7.0; deprecated in API level Q) 
      /// (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="storagename">z.B. "19F4-0903"</param>
      /// <param name="requestid">ID für OnActivityResult() der activity</param>
      public partial async Task<bool> Ask4SafAndroidPersistentPermissonAndWait(string storagename, int requestid) {
         bool ok = await Ask4SafAndroidPersistentPermissonAndWait(vol.GetVolumeNo4Name(storagename), requestid);
         return ok;
      }

      /// <summary>
      /// führt zur Anfrage an den Nutzer, ob die Permissions erteilt werden sollen (API level 24 / Nougat / 7.0; deprecated in API level Q) 
      /// (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="volidx">Volume-Index (min. 1)</param>
      /// <param name="requestid">ID für OnActivityResult() der activity</param>
      public partial async Task<bool> Ask4SafAndroidPersistentPermissonAndWait(int volidx, int requestid) {
         //if (useStd(volidx))
         //   return true;
         bool ok = false;
         if (0 <= volidx && volidx < Volumes)
            ok = await saf.Ask4PersistentPermissonAndWait4Result(vol.StorageVolumeNames[volidx], volidx, requestid);
         return ok;
      }

      #endregion
#endif

      /// <summary>
      /// Volumenamen und spez. Pfadangaben aktualisieren
      /// </summary>
      public partial void RefreshVolumesAndSpecPaths() {
         getSpecPaths(out List<string> datapaths, out List<string> cachepaths);

         for (int i = 0; i < datapaths.Count; i++)
            if (datapaths[i].EndsWith('/'))
               datapaths[i] = datapaths[i].Substring(0, datapaths[i].Length - 1);

         for (int i = 0; i < cachepaths.Count; i++)
            if (cachepaths[i].EndsWith('/'))
               cachepaths[i] = cachepaths[i].Substring(0, cachepaths[i].Length - 1);

         AppDataPaths = new List<string>(datapaths);
         AppTmpPaths = new List<string>(cachepaths);

         vol.RefreshVolumes();
         VolumePaths = new List<string>(vol.StorageVolumePaths);
         VolumeNames = new List<string>(vol.StorageVolumeNames);
      }

      /// <summary>
      /// liefert die akt. Daten für ein Volume (oder null)
      /// </summary>
      /// <param name="volidx">Volume-Index</param>
      /// <returns></returns>
      public partial VolumeData? GetVolumeData(int volidx) => volidx < Volumes ?
                                                                           vol.GetVolumeData(volidx) :
                                                                           null;

      /// <summary>
      /// liefert die akt. Daten für ein Volume (oder null)
      /// </summary>
      /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
      /// <returns></returns>
      public partial VolumeData GetVolumeData(string storagename) => vol.GetVolumeData(vol.GetVolumeNo4Name(storagename));

      public partial bool DirectoryExists(string fullpath) =>
         exists(fullpath, true
#if USE_STORAGEACCESSFRAMEWORK
                              , vol.GetVolumeNo4Path(fullpath)
#endif
            );

      public partial bool FileExists(string fullpath) =>
         exists(fullpath, false
#if USE_STORAGEACCESSFRAMEWORK
                               , vol.GetVolumeNo4Path(fullpath)
#endif
            );

      /// <summary>
      /// liefert eine Liste <see cref="StorageItem"/>; das 1. Item gehört zu fullpath
      /// </summary>
      /// <param name="fullpath"></param>
      /// <param name="onlyfolders"></param>
      /// <returns></returns>
      public partial List<StorageItem> StorageItemList(string fullpath, bool onlyfolders) {
         List<StorageItem> lst = new();
         try {
#if USE_STORAGEACCESSFRAMEWORK
            int v = vol.GetVolumeNo4Path(fullpath);
            if (useStd(v)) {
               //if (true) { // IMMER für die Listenabfrage (ist auch deutlich schneller!!!)

               if (exists(fullpath, true, 0)) { // ex. und ist Verzeichnis

                  //DirectoryInfo di = new DirectoryInfo(fullpath);
                  //StdStorageItem si = new StdStorageItem(di);
                  //lst.Add(si);

                  if (Directory.Exists(fullpath))
                     lst.Add(new StdStorageItem(new DirectoryInfo(fullpath)));
                  else if (File.Exists(fullpath))
                     lst.Add(new StdStorageItem(new FileInfo(fullpath)));

                  foreach (string item in Directory.GetDirectories(fullpath))
                     lst.Add(new StdStorageItem(new DirectoryInfo(item)));
                  if (!onlyfolders)
                     foreach (string item in Directory.GetFiles(fullpath))
                        lst.Add(new StdStorageItem(new FileInfo(item)));
               }

            } else {

               lst = saf.StorageItemList(vol.StorageVolumeNames[v],
                                         fullpath.Substring(vol.StorageVolumePaths[v].Length)); // Objektnamen ohne Pfad!

            }
#else
            if (exists(fullpath, true)) { // ex. und ist Verzeichnis
               if (Directory.Exists(fullpath))
                  lst.Add(new StdStorageItem(new DirectoryInfo(fullpath)));
               else if (File.Exists(fullpath))
                  lst.Add(new StdStorageItem(new FileInfo(fullpath)));

               foreach (string item in Directory.GetDirectories(fullpath))
                  lst.Add(new StdStorageItem(new DirectoryInfo(item)));
               if (!onlyfolders)
                  foreach (string item in Directory.GetFiles(fullpath))
                     lst.Add(new StdStorageItem(new FileInfo(item)));
            }
#endif
         } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine("StorageItemList(" + fullpath + ") Exception: " + ex.Message);
         }
         return lst;
      }

      public partial List<string> Files(string fullpath) => objectList(fullpath, false);

      public partial List<string> Directories(string fullpath) => objectList(fullpath, true);

      /// <summary>
      /// löscht das Verzeichnis
      /// </summary>
      /// <param name="fullpath">Pfad einschließlich Volumepfad</param>
      /// <returns>true falls erfolgreich</returns>
      /// <returns></returns>
      public partial bool DeleteDirectory(string fullpath) {
#if USE_STORAGEACCESSFRAMEWORK
         int v = vol.GetVolumeNo4Path(fullpath);
         if (exists(fullpath, true, v))
            try {
               if (useStd(v)) {

                  Directory.Delete(fullpath, true);
                  return true;

               } else {

                  return saf.Delete(vol.StorageVolumeNames[v],
                                    fullpath.Substring(vol.StorageVolumePaths[v].Length));

               }
            } catch (Exception ex) {
               System.Diagnostics.Debug.WriteLine("DeleteDirectory(" + fullpath + ") Exception: " + ex.Message);
            }
#else
         if (exists(fullpath, true))
            try {
               Directory.Delete(fullpath, true);
               return true;
            } catch (Exception ex) {
               System.Diagnostics.Debug.WriteLine("DeleteDirectory(" + fullpath + ") Exception: " + ex.Message);
            }
#endif
         return false;
      }

      /// <summary>
      /// erzeugt ein Verzeichnis
      /// </summary>
      /// <param name="fullpath">Pfad einschließlich Volumepfad</param>
      /// <returns>true falls erfolgreich oder schon ex.</returns>
      public partial bool CreateDirectory(string fullpath) {
#if USE_STORAGEACCESSFRAMEWORK
         int v = vol.GetVolumeNo4Path(fullpath);
         if (!exists(fullpath, true, v))
            try {
               if (useStd(v)) {

                  Directory.CreateDirectory(fullpath);
                  return true;

               } else {

                  return saf.CreateDirectory(vol.StorageVolumeNames[v], fullpath.Substring(vol.StorageVolumePaths[v].Length));

               }
            } catch (Exception ex) {
               System.Diagnostics.Debug.WriteLine("CreateDirectory(" + fullpath + ") Exception: " + ex.Message);
            }
#else
         if (!exists(fullpath, true))
            try {
               Directory.CreateDirectory(fullpath);
               return true;
            } catch (Exception ex) {
               System.Diagnostics.Debug.WriteLine("CreateDirectory(" + fullpath + ") Exception: " + ex.Message);
            }
#endif
         return false;
      }

      /// <summary>
      /// löscht die Datei
      /// </summary>
      /// <param name="fullpath">Pfad einschließlich Volumepfad</param>
      /// <returns>true falls erfolgreich</returns>
      public partial bool DeleteFile(string fullpath) {
#if USE_STORAGEACCESSFRAMEWORK
         int v = vol.GetVolumeNo4Path(fullpath);
         if (exists(fullpath, false, v))
            try {
               if (useStd(v)) {

                  File.Delete(fullpath);
                  return true;

               } else {

                  return saf.Delete(vol.StorageVolumeNames[v],
                                    fullpath.Substring(vol.StorageVolumePaths[v].Length));

               }
            } catch (Exception ex) {
               System.Diagnostics.Debug.WriteLine("DeleteFile(" + fullpath + ") Exception: " + ex.Message);
            }
#else
         if (exists(fullpath, false))
            try {
               File.Delete(fullpath);
               return true;
            } catch (Exception ex) {
               System.Diagnostics.Debug.WriteLine("DeleteFile(" + fullpath + ") Exception: " + ex.Message);
            }
#endif
         return false;
      }

      /// <summary>
      /// liefert einen Dateistream
      /// <para>Falls die Datei nicht ex. wird sie erzeugt. Auch das Verzeichnis wird bei Bedarf erzeugt.</para>
      /// <para>I.A. wird damit ein StreamWriter bzw. ein StreamReader erzeugt.</para>
      /// <para>ACHTUNG: Der Stream erfüllt nur primitivste Bedingungen. Er ist nicht "seekable", es wird keine Position oder Länge geliefert.
      /// Auch ein "rwt"-Stream scheint nur beschreibbar zu sein.</para>
      /// </summary>
      /// <param name="fullpath">Pfad einschließlich Volumepfad</param>
      /// <param name="mode"><para>"w" (write-only access and erasing whatever data is currently in the file),</para>
      ///                    <para>"wa" (write-only access to append to existing data),</para>
      ///                    <para>"rw" (read and write access), or</para>
      ///                    <para>"rwt" (read and write access that truncates any existing file)</para>
      ///                    <para>["r" (read-only)] ???,</para></param>
      /// <returns></returns>
      public partial Stream? OpenFile(string fullpath, string mode) {
         if (mode == "r" ||
             mode == "w" ||
             mode == "wa" ||
             mode == "rw" ||
             mode == "rwt") {

#if USE_STORAGEACCESSFRAMEWORK
            int v = vol.GetVolumeNo4Path(fullpath);

            if (!exists(fullpath, true, v)) // es gibt kein gleichnamiges Verzeichnis
               try {
                  FileMode fmode = FileMode.OpenOrCreate;
                  FileAccess access = FileAccess.ReadWrite; // analog rw
                  if (mode == "rwt") {
                     fmode = FileMode.Create;
                  } else if (mode == "wa") {
                     fmode = FileMode.Append;
                     access = FileAccess.Write;
                  } else if (mode == "w") {
                     fmode = FileMode.Create;
                     access = FileAccess.Write;
                  } else if (mode == "r") {
                     fmode = FileMode.Open;
                     access = FileAccess.Read;
                  }

                  if (useStd(v)) {

                     string? parentpath = Path.GetDirectoryName(fullpath);
                     if (parentpath == null)
                        return null;
                     if (!Directory.Exists(parentpath))
                        if (!CreateDirectory(parentpath))
                           return null;
                     return new FileStream(fullpath, fmode, access);

                  } else {

                     return saf.CreateOpenFile(vol.StorageVolumeNames[v],
                                               fullpath.Substring(vol.StorageVolumePaths[v].Length),
                                               mode);

                  }
               } catch (Exception ex) {
                  System.Diagnostics.Debug.WriteLine("OpenFile(" + fullpath + ", " + mode + ") Exception: " + ex.Message);
               }
#else
            if (!exists(fullpath, true)) // es gibt kein gleichnamiges Verzeichnis
               try {
                  FileMode fmode = FileMode.OpenOrCreate;
                  FileAccess access = FileAccess.ReadWrite; // analog rw
                  if (mode == "rwt") {
                     fmode = FileMode.Create;
                  } else if (mode == "wa") {
                     fmode = FileMode.Append;
                     access = FileAccess.Write;
                  } else if (mode == "w") {
                     fmode = FileMode.Create;
                     access = FileAccess.Write;
                  } else if (mode == "r") {
                     fmode = FileMode.Open;
                     access = FileAccess.Read;
                  }

                  string? parentpath = Path.GetDirectoryName(fullpath);
                  if (parentpath == null)
                     return null;
                  if (!Directory.Exists(parentpath))
                     if (!CreateDirectory(parentpath))
                        return null;
                  return new FileStream(fullpath, fmode, access);
               } catch (Exception ex) {
                  System.Diagnostics.Debug.WriteLine("OpenFile(" + fullpath + ", " + mode + ") Exception: " + ex.Message);
               }
#endif
         }
         return null;
      }

      /// <summary>
      /// verschiebt eine Datei (ev. als Kombination aus Kopie und Löschen)
      /// </summary>
      /// <param name="fullpathsrc">Quelldatei: Pfad einschließlich Volumepfad</param>
      /// <param name="fullpathdst">Zieldatei: Pfad einschließlich Volumepfad</param>
      /// <returns></returns>
      public partial bool Move(string fullpathsrc, string fullpathdst) {
#if USE_STORAGEACCESSFRAMEWORK
         int v1 = vol.GetVolumeNo4Path(fullpathsrc);
         int v2 = vol.GetVolumeNo4Path(fullpathdst);
         if (!exists(fullpathdst, true, v2) &&
             !exists(fullpathdst, false, v2))
            try {
               if (useStd(v1) &&
                   useStd(v2)) {
                  File.Move(fullpathsrc, fullpathdst);
                  return true;
               } else {
                  if (Path.GetDirectoryName(fullpathsrc) == Path.GetDirectoryName(fullpathdst))  // nur ein Rename
                     if (saf.Rename(vol.StorageVolumeNames[v1],
                                    fullpathsrc.Substring(vol.StorageVolumePaths[v1].Length),
                                    Path.GetFileName(fullpathdst)))
                        return true;

                  if (!useStd(v1) && !useStd(v2)) {
                     string filesrc = fullpathsrc.Substring(vol.StorageVolumePaths[v1].Length);
                     string? pathdst = Path.GetDirectoryName(fullpathdst.Substring(vol.StorageVolumePaths[v2].Length));
                     if (pathdst == null)
                        return false;
                     string filetmp = Path.Combine(pathdst, Path.GetFileName(fullpathsrc));
                     if (saf.Move(vol.StorageVolumeNames[v1],
                                  filesrc,
                                  vol.StorageVolumeNames[v2],
                                  pathdst)) {
                        if (saf.Rename(vol.StorageVolumeNames[v2],
                                       filetmp,
                                       Path.GetFileName(fullpathdst)))
                           return true;
                     }
                  }

                  // Sollte Move auch über Volume-Grenzen hinweg fkt.???


                  if (copy(fullpathsrc, fullpathdst))
                     if (DeleteFile(fullpathsrc))
                        return true;
               }
            } catch (Exception ex) {
               System.Diagnostics.Debug.WriteLine("Move(" + fullpathsrc + ", " + fullpathdst + ") Exception: " + ex.Message);
            }
         return false;
#else
         File.Move(fullpathsrc, fullpathdst);
         return true;
#endif
      }

      /// <summary>
      /// erzeugt eine Dateikopie
      /// </summary>
      /// <param name="fullpathsrc">Quelldatei: Pfad einschließlich Volumepfad</param>
      /// <param name="fullpathdst">Zieldatei: Pfad einschließlich Volumepfad</param>
      /// <returns></returns>
      public partial bool Copy(string fullpathsrc, string fullpathdst) => copy(fullpathsrc, fullpathdst);

      /// <summary>
      /// liefert die Länge einer Datei, Lese- und Schreibberechtigung und den Zeitpunkt der letzten Änderung
      /// </summary>
      /// <param name="fullpath"></param>
      /// <param name="pathisdir"></param>
      /// <param name="canread">wenn nicht secondary external storage, dann immer true (?)</param>
      /// <param name="canwrite"></param>
      /// <param name="lastmodified">letzte Änderung einer Datei bzw. Zeitpunkt der Erzeugung eines Verzeichnisses</param>
      /// <returns>Dateilänge</returns>
      public partial long GetFileAttributes(string fullpath,
                                            bool pathisdir,
                                            out bool canread,
                                            out bool canwrite,
                                            out DateTime lastmodified) {
         int v = vol.GetVolumeNo4Path(fullpath);
         long len = -1;

         if (useStd(v)) {

            if (pathisdir) {
               DirectoryInfo di = new(fullpath);
               lastmodified = di.CreationTime;
               canread = canwrite = true;
               try {
                  di.GetFiles();
               } catch {
                  canread = canwrite = false;
               }
            } else {
               FileInfo fi = new(fullpath);
               len = fi.Length;
               canwrite = !fi.IsReadOnly;
               lastmodified = fi.LastWriteTime;
               canread = false;
               using StreamReader sr = new(fullpath); canread = true;
            }

         } else {

            canread = canwrite = false;
            lastmodified = DateTime.MinValue;

#if USE_STORAGEACCESSFRAMEWORK
            DocumentFile? doc = saf.GetExistingDocumentFile(vol.StorageVolumeNames[v],
                                                            fullpath.Substring(vol.StorageVolumePaths[v].Length));
            if (doc != null) {
               canread = doc.CanRead();
               canwrite = doc.CanWrite();
               lastmodified = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime().AddMilliseconds(doc.LastModified()); // Bezug auf den 1.1.1970
               len = doc.Length();
            }
#endif

         }
         return len;
      }

      /// <summary>
      /// fkt. NICHT im secondary external storage (und bei Android 7 auch nicht im primary storage)
      /// </summary>
      /// <param name="fullpath"></param>
      /// <param name="canwrite"></param>
      /// <param name="lastmodified"></param>
      public partial void SetFileAttributes(string fullpath, bool canwrite, DateTime lastmodified) {
         if (useStd(fullpath)) {
            FileInfo fi = new(fullpath) {
               IsReadOnly = !canwrite
            };
            fi.LastWriteTime = fi.LastAccessTime = lastmodified;
         }
      }

      ///// <summary>
      ///// <para>liefert das "private" Dateiverzeichnis für den internen (Index 0) und den/die externen Speicher (Index &gt; 0)</para>
      ///// <para>Returns absolute paths to application-specific directories on all shared/external storage devices where the application can place 
      ///// persistent files it owns. These files are internal to the application, and not typically visible to the user as media.
      ///// These files will be deleted when the application is uninstalled, however there are some important differences:
      ///// </para>
      ///// <para>- Shared storage may not always be available, since removable media can be ejected by the user.</para>
      ///// <para>- There is no security enforced with these files. For example, any application holding Manifest.permission.WRITE_EXTERNAL_STORAGE can write to these files. </para>
      ///// </summary>
      ///// <param name="activity">Android-Activity</param>
      ///// <returns></returns>
      //static public partial List<string> PrivateFilesDirs(object activity) {
      //   List<string> paths = new List<string>();
      //   paths.Add(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
      //   if (activity != null &&
      //       activity is Activity) {
      //      Java.IO.File[] jpaths = (activity as Activity).ApplicationContext.GetExternalFilesDirs("");
      //      for (int i = 0; i < jpaths.Length; i++)
      //         if (jpaths[i] != null)
      //            paths.Add(jpaths[i].CanonicalPath);
      //   }
      //   return paths;
      //}

      ///// <summary>
      ///// <para>liefert das "private" Verzeichnis für temp. Dateien für den internen (Index 0) und für den/die externen Speicher (Index &gt; 0)</para>
      ///// </summary>
      ///// <param name="activity"></param>
      ///// <returns></returns>
      //static public List<string> PrivateTempDirs(object activity) {
      //   List<string> paths = new List<string>();
      //   paths.Add(System.IO.Path.GetTempPath());
      //   if (activity != null &&
      //       activity is Activity) {
      //      Java.IO.File[] jpaths = (activity as Activity).ApplicationContext.GetExternalCacheDirs();
      //      for (int i = 0; i < jpaths.Length; i++)
      //         if (jpaths[i] != null)
      //            paths.Add(jpaths[i].CanonicalPath);
      //   }
      //   return paths;
      //}

      static public partial string PublicFolder() => AndroidFunction.ExternalStorageDirectory;


      #region private Methoden

      /// <summary>
      /// Standardmethoden oder SAF-Methoden verwenden
      /// </summary>
      /// <param name="vol"></param>
      /// <returns></returns>
      bool useStd(int vol) => !ispre_r ||    // ab Android 11 immer Std.
                              vol < 1;

      bool useStd(string fullpath) => useStd(vol.GetVolumeNo4Path(fullpath));

      /// <summary>
      /// liefert die Data- und Cache-Pfade (Index 0 immer mit den internen Pfaden)
      /// <para>Nicht für jedes Volume existieren diese Pfade!</para>
      /// </summary>
      /// <param name="dataPaths"></param>
      /// <param name="cachePaths"></param>
      void getSpecPaths(out List<string> dataPaths, out List<string> cachePaths) {
         dataPaths = [];
         cachePaths = [];

         Java.IO.File[]? paths;
         Android.Content.Context? context = null;
         if (activity != null)
            context = AndroidFunction.GetActivityContext(activity);

         dataPaths.Add(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
         if (context != null) {
            paths = context.GetExternalFilesDirs("");
            if (paths != null) {
               for (int i = 0; i < paths.Length; i++)
                  if (paths[i] != null)
                     dataPaths.Add(paths[i].CanonicalPath);
            }
         }

         cachePaths.Add(System.IO.Path.GetTempPath());

         if (context != null) {
            paths = context.GetExternalCacheDirs();
            if (paths != null) {
               for (int i = 0; i < paths.Length; i++)
                  if (paths[i] != null)
                     cachePaths.Add(paths[i].CanonicalPath);
            }
         }
      }

#if USE_STORAGEACCESSFRAMEWORK
      bool exists(string fullpath, bool isdir, int vol) {
         if (useStd(vol)) {

            if (isdir)
               return Directory.Exists(fullpath);
            else
               return File.Exists(fullpath);

         } else

            return saf.ObjectExists(this.vol.StorageVolumeNames[vol],
                                    fullpath.Substring(this.vol.StorageVolumePaths[vol].Length),
                                    isdir);
      }
#else
      bool exists(string fullpath, bool isdir) =>
         isdir ? Directory.Exists(fullpath) : File.Exists(fullpath);
#endif

      List<string> objectList(string fullpath, bool isdir) {
         List<string> lst = [];
         try {
#if USE_STORAGEACCESSFRAMEWORK
            int v = vol.GetVolumeNo4Path(fullpath);
            if (exists(fullpath, true, v)) { // Verzeichnis ex.
               if (ispre_r || useStd(v)) {      // ev. fkt. das auch für >= R ???

                  if (isdir)
                     lst.AddRange(Directory.GetDirectories(fullpath));
                  else
                     lst.AddRange(Directory.GetFiles(fullpath));

               } else {

                  List<string> obj = saf.ObjectList(vol.StorageVolumeNames[v],
                                                    fullpath.Substring(vol.StorageVolumePaths[v].Length),
                                                    isdir); // Objektnamen ohne Pfad!
                  foreach (string name in obj) {
                     lst.Add(Path.Combine(fullpath, name));
                  }

               }
            }
#else
            if (isdir)
               lst.AddRange(Directory.GetDirectories(fullpath));
            else
               lst.AddRange(Directory.GetFiles(fullpath));
#endif
            return lst;
         } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine("ObjectList(" + fullpath + "," + isdir + ") Exception: " + ex.Message);
         }
         return lst;
      }

      bool copy(string fullpathsrc, string fullpathdst, int blksize = 512) {
         try {
            int v1 = vol.GetVolumeNo4Path(fullpathsrc);
            int v2 = vol.GetVolumeNo4Path(fullpathdst);
            if (v1 < 1 &&
                v2 < 1) {
               File.Copy(fullpathsrc, fullpathdst);
               return true;
            } else {

               //string filesrc = fullpathsrc.Substring(svh.StorageVolumePaths[v1].Length);
               //string pathdst = Path.GetDirectoryName(fullpathdst.Substring(svh.StorageVolumePaths[v2].Length));
               //string filetmp = Path.Combine(pathdst, Path.GetFileName(fullpathsrc));

               //if (seh.Copy(svh.StorageVolumeNames[v1],
               //             filesrc,
               //             svh.StorageVolumeNames[v2],
               //             pathdst))
               //   if (seh.Rename(svh.StorageVolumeNames[v2],
               //                  filetmp,
               //                  Path.GetFileName(fullpathdst)))
               //      return true;

               // letzter Versuch:
               using (Stream? outp = OpenFile(fullpathdst, "w")) {
                  using (Stream? inp = OpenFile(fullpathsrc, "r")) {
                     if (inp != null && outp != null) {
                        byte[] buffer = new byte[blksize * 1024];
                        int len;
                        while ((len = inp.Read(buffer, 0, buffer.Length)) > 0) {
                           outp.Write(buffer, 0, len);
                        }
                        //try {
                        //   inp.Seek(0, SeekOrigin.Begin);
                        //} catch (Exception ex) {
                        //   System.Diagnostics.Debug.WriteLine(ex.Message);     // "Specified method is not supported."
                        //}
                     }
                  }
               }
               return true;
            }
         } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine("Copy(" + fullpathsrc + ", " + fullpathdst + ") Exception: " + ex.Message);
         }
         return false;
      }

      int idx4VolumeName(string storagename) {
         for (int i = 0; i < Volumes; i++)
            if (VolumeNames[i] == storagename)
               return i;
         return -1;
      }


      #endregion

      public override string ToString() => string.Format("Volumes={0}: {1}", Volumes, string.Join(", ", VolumeNames));

#if DEBUG

      //public partial object? test1(object? o1) {
      //   object? result = null;

      //   Task.Run(async () => {
      //      Tests t = new Tests();
      //      result = await t.Start(activity, saf, vol);
      //   });

      //   return result;
      //}

#endif

   }

}