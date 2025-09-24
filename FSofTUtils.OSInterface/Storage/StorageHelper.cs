namespace FSofTUtils.OSInterface.Storage {

   /// <summary>
   /// die Klasse liefert Hilfsfunktionen für Volumes und Dateioperationen
   /// </summary>
   public partial class StorageHelper {

      /*
      Im internen Speicher und im primary external Storage können die normalen .Net-Funktionen verwendet werden.
      Für den primary external Storage muss lediglich das Recht
         Manifest.Permission.WriteExternalStorage
      gesetzt sein. Das kann über das Manifest oder zur Laufzeit erfolgen.

      Für den "secondary" external Storage, d.h. eine echte SD-Karte und/oder ein USB-Stick genügen diese Rechte nicht. Hier müssen zusätzliche
      Rechte als
         PersistentPermissions
      erteilt werden. Das geht ausschließlich zur Laufzeit mit der Bestätigung durch den Anwender.
      Außerdem steht hier eine zusätzliche Softwareschicht zwischen der App und den Dateisystemobjekten. Viele .Net-Funktionen wie z.B. das Ausflisten von
      Dateien und Verzeichnissen oder der Existenztest einer Datei oder eines Verzeichnisses funktionieren zwar, aber einige der wichtigsten nicht.
      Es gibt z.B. einen speziellen Mechanismus zum Öffnen/Erzeugen einer Datei, zum Löschen von Dateisystemobjekten usw.

      Mit dieser Hilfsklasse werden diese Funktionen so bereit gestellt, dass aus Sicht der App keine Unterscheidung des Speicherortes notwendig ist.

      */

      /// <summary>
      /// Status eines Volumes
      /// </summary>
      public enum VolumeState {
         /// <summary>
         /// Unknown storage state, such as when a path isn't backed by known storage media.
         /// </summary>
         MEDIA_UNKNOWN,
         /// <summary>
         /// Storage state if the media is not present. 
         /// </summary>
         MEDIA_REMOVED,
         /// <summary>
         /// Storage state if the media is present but not mounted. 
         /// </summary>
         MEDIA_UNMOUNTED,
         /// <summary>
         /// Storage state if the media is present and being disk-checked. 
         /// </summary>
         MEDIA_CHECKING,
         /// <summary>
         /// Storage state if the media is in the process of being ejected.
         /// </summary>
         MEDIA_EJECTING,
         /// <summary>
         /// Storage state if the media is present but is blank or is using an unsupported filesystem. 
         /// </summary>
         MEDIA_NOFS,
         /// <summary>
         /// Storage state if the media is present and mounted at its mount point with read/write access. 
         /// </summary>
         MEDIA_MOUNTED,
         /// <summary>
         /// Storage state if the media is present and mounted at its mount point with read-only access. 
         /// </summary>
         MEDIA_MOUNTED_READ_ONLY,
         /// <summary>
         /// Storage state if the media is present not mounted, and shared via USB mass storage.  
         /// </summary>
         MEDIA_SHARED,
         /// <summary>
         /// Storage state if the media was removed before it was unmounted. 
         /// </summary>
         MEDIA_BAD_REMOVAL,
         /// <summary>
         /// Storage state if the media is present but cannot be mounted. Typically this happens if the file system on the media is corrupted. 
         /// </summary>
         MEDIA_UNMOUNTABLE,
      }

      /// <summary>
      /// Daten eines Volumes
      /// </summary>
      public class VolumeData {
         /// <summary>
         /// Anzahl der vorhandenen Volumes
         /// </summary>
         public int Volumes;
         /// <summary>
         /// Index des abgefragten Volumes
         /// </summary>
         public int VolumeNo;
         /// <summary>
         /// Pfad zum Volume, z.B.: "/storage/emulated/0" und "/storage/19F4-0903"
         /// </summary>
         public string Path;
         /// <summary>
         /// Name des Volumes, z.B. "primary" oder "19F4-0903"
         /// </summary>
         public string Name;
         /// <summary>
         /// Beschreibung des Volumes
         /// </summary>
         public string Description;
         /// <summary>
         /// Gesamtspeicherplatz des Volumes
         /// </summary>
         public long TotalBytes;
         /// <summary>
         /// freier Speicherplatz des Volumes
         /// </summary>
         public long AvailableBytes;
         /// <summary>
         /// Ist das abgefragte Volume ein primäres Volume?
         /// </summary>
         public bool IsPrimary;
         /// <summary>
         /// Ist das abgefragte Volume entfernbar?
         /// </summary>
         public bool IsRemovable;
         /// <summary>
         /// Ist das abgefragte Volume nur emuliert?
         /// </summary>
         public bool IsEmulated;
         /// <summary>
         /// Status des Volumes
         /// </summary>
         public VolumeState State;

         public VolumeData() {
            Volumes = 0;
            VolumeNo = -1;
            Path = Name = Description = "";
            TotalBytes = AvailableBytes = 0;
            IsPrimary = IsRemovable = IsEmulated = false;
            State = VolumeState.MEDIA_UNKNOWN;
         }

         public VolumeData(VolumeData vd) {
            Volumes = vd.Volumes;
            VolumeNo = vd.VolumeNo;
            Path = vd.Path;
            Name = vd.Name;
            Description = vd.Description;
            TotalBytes = vd.TotalBytes;
            AvailableBytes = vd.AvailableBytes;
            IsPrimary = vd.IsPrimary;
            IsRemovable = vd.IsRemovable;
            IsEmulated = vd.IsEmulated;
            State = vd.State;
         }

         public override string ToString() {
            return string.Format("VolumeNo={0}, Name={1}, Description={2}, Path={3}", VolumeNo, Name, Description, Path);
         }
      }

      public class StorageItem : IComparable {
         public string Name { get; protected set; }
         public bool IsDirectory { get; protected set; }
         public bool IsFile { get; protected set; }
         public long Length { get; protected set; }
         public string MimeType { get; protected set; }
         public bool CanRead { get; protected set; }
         public bool CanWrite { get; protected set; }
         public DateTime LastModified { get; protected set; }
         public StorageItem() {
            Name = "";
            IsDirectory = false;
            IsFile = false;
            MimeType = "";
            CanRead = false;
            CanWrite = false;
            LastModified = DateTime.MinValue;
            Length = 0;
         }

         public int CompareTo(object? obj) {
            if (obj != null &&
                obj is StorageItem) {
               // erst nach IsDirectory sortieren (IsDirectory zuerst) ...
#pragma warning disable CS8600 // Das NULL-Literal oder ein möglicher NULL-Wert wird in einen Non-Nullable-Typ konvertiert.
               StorageItem sti = obj as StorageItem;
#pragma warning restore CS8600 // Das NULL-Literal oder ein möglicher NULL-Wert wird in einen Non-Nullable-Typ konvertiert.
               if (sti != null &&
                   IsDirectory != sti.IsDirectory)
                  return IsDirectory ? -1 : 1;
               // ... dann nach Namen sortieren
               return string.Compare(Name, sti?.Name);
            }

            throw new ArgumentException(nameof(obj));
         }
      }


      /// <summary>
      /// ext. StorageVolume-Pfade beim letzten <see cref="Volume.RefreshVolumes"/>, z.B. "/storage/emulated/0" oder "/storage/19F4-0903"
      /// </summary>
      public List<string> VolumePaths { get; protected set; } = new List<string>();

      /// <summary>
      /// ext. StorageVolume-Namen beim letzten <see cref="Volume.RefreshVolumes"/>, z.B. "primary" oder "19F4-0903"
      /// </summary>
      public List<string> VolumeNames { get; protected set; } = new List<string>();

      /// <summary>
      /// Anzahl der Volumes
      /// </summary>
      public int Volumes => VolumePaths.Count;

      /// <summary>
      /// die "privaten" Verzeichnisse der App, z.B.: "/data/user/0/APKNAME/files" oder "/storage/emulated/0/Android/data/APKNAME/files" oder "/storage/19F4-0903/Android/data/APKNAME/files"
      /// <para>Bei Index 0 steht der "interne" Android-Pfad.</para>
      /// </summary>
      public List<string> AppDataPaths { get; protected set; } = new List<string>();

      /// <summary>
      /// die "privaten" Verzeichnisse für temp. Daten der App z.B.: "/data/user/0/APKNAME/cache" oder "/storage/emulated/0/Android/data/APKNAME/cache" oder "/storage/19F4-0903/Android/data/APKNAME/cache"
      /// <para>Bei Index 0 steht der "interne" Android-Pfad.</para>
      /// </summary>
      public List<string> AppTmpPaths { get; protected set; } = new List<string>();

      static public partial string PublicFolder();

#if USE_STORAGEACCESSFRAMEWORK

      #region spez. für Android nötige Funktionen (Storage-Access-Framework)

      /// <summary>
      /// versucht die Schreib- und Leserechte für das Volume zu setzen (API level 19 / Kitkat / 4.4) (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
      /// <returns></returns>
      public partial bool SetSafAndroidPersistentPermissions(string storagename);

      /// <summary>
      /// versucht die Schreib- und Leserechte für das Volume zu setzen (API level 19 / Kitkat / 4.4) (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="volidx">Volume-Index (min. 1)</param>
      /// <returns></returns>
      public partial bool SetSafAndroidPersistentPermissions(int volidx);

      /// <summary>
      /// gibt die persistenten Schreib- und Leserechte frei (API level 19 / Kitkat / 4.4) (für primäres externes Volume nicht geeignet!)
      /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
      /// </summary>
      public partial void ReleaseSafAndroidPersistentPermissions(string storagename);

      /// <summary>
      /// gibt die persistenten Schreib- und Leserechte frei (API level 19 / Kitkat / 4.4) (für primäres externes Volume nicht geeignet!)
      /// <param name="volidx">Volume-Index (min. 1)</param>
      /// </summary>
      public partial void ReleaseSafAndroidPersistentPermissions(int volidx);

      /// <summary>
      /// führt zur Anfrage an den Nutzer, ob die Permissions erteilt werden sollen (API level 24 / Nougat / 7.0; deprecated in API level Q) (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="storagename">z.B. "19F4-0903"</param>
      /// <param name="requestid">ID für OnActivityResult() der activity</param>
      public partial void Ask4SafAndroidPersistentPermisson(string storagename, int requestid = 12345);

      /// <summary>
      /// führt zur Anfrage an den Nutzer, ob die Permissions erteilt werden sollen (API level 24 / Nougat / 7.0; deprecated in API level Q) (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="volidx">Volume-Index (min. 1)</param>
      /// <param name="requestid">ID für OnActivityResult() der activity</param>
      public partial void Ask4SafAndroidPersistentPermisson(int volidx, int requestid = 12345);

      /// <summary>
      /// führt zur Anfrage an den Nutzer, ob die Permissions erteilt werden sollen (API level 24 / Nougat / 7.0; deprecated in API level Q) (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="storagename">z.B. "19F4-0903"</param>
      /// <param name="requestid">ID für OnActivityResult() der activity</param>
      public partial Task<bool> Ask4SafAndroidPersistentPermissonAndWait(string storagename, int requestid = 12345);

      /// <summary>
      /// führt zur Anfrage an den Nutzer, ob die Permissions erteilt werden sollen (API level 24 / Nougat / 7.0; deprecated in API level Q) (für primäres externes Volume nicht geeignet!)
      /// </summary>
      /// <param name="volidx">Volume-Index (min. 1)</param>
      /// <param name="requestid">ID für OnActivityResult() der activity</param>
      public partial Task<bool> Ask4SafAndroidPersistentPermissonAndWait(int volidx, int requestid = 12345);

      #endregion

#endif

      /// <summary>
      /// liefert die akt. Daten für ein Volume (oder null)
      /// </summary>
      /// <param name="volidx">Volume-Index</param>
      /// <returns></returns>
      public partial VolumeData? GetVolumeData(int volidx);

      /// <summary>
      /// liefert die akt. Daten für ein Volume (oder null)
      /// </summary>
      /// <param name="storagename">z.B. "primary" oder "19F4-0903"</param>
      /// <returns></returns>
      public partial VolumeData GetVolumeData(string storagename);

      /// <summary>
      /// Akt. der Infos
      /// </summary>
      public partial void RefreshVolumesAndSpecPaths();

      /// <summary>
      /// Gibt es das Verzeichnis?
      /// </summary>
      /// <param name="fullpath"></param>
      /// <returns></returns>
      public partial bool DirectoryExists(string fullpath);

      /// <summary>
      /// Gibt es die Datei?
      /// </summary>
      /// <param name="fullpath"></param>
      /// <returns></returns>
      public partial bool FileExists(string fullpath);

      /// <summary>
      /// Liste der Dateien im Verzeichnis
      /// </summary>
      /// <param name="fullpath"></param>
      /// <returns></returns>
      public partial List<string> Files(string fullpath);

      /// <summary>
      /// Liste der Verzeichnisse im Verzeichnis
      /// </summary>
      /// <param name="fullpath"></param>
      /// <returns></returns>
      public partial List<string> Directories(string fullpath);

      /// <summary>
      /// liefert eine Liste <see cref="StorageItem"/>; das 1. Item gehört zu fullpath
      /// </summary>
      /// <param name="fullpath"></param>
      /// <param name="onlyfolders"></param>
      /// <returns></returns>
      public partial List<StorageItem> StorageItemList(string fullpath, bool onlyfolders);

      /// <summary>
      /// liefert einen Dateistream
      /// <para>Falls die Datei nicht ex. wird sie erzeugt. Auch das Verzeichnis wird bei Bedarf erzeugt.</para>
      /// <para>I.A. wird damit ein StreamWriter bzw. ein StreamReader erzeugt.</para>
      /// <para>ACHTUNG: Der Stream erfüllt nur primitivste Bedingungen. Er ist nicht "seekable", es wird keine Position oder Länge geliefert.
      /// Auch ein "rwt"-Stream scheint nur beschreibbar zu sein.</para>
      /// </summary>
      /// <param name="fullpath">Android- oder Volume-Pfad</param>
      /// <param name="mode"><para>"w" (write-only access and erasing whatever data is currently in the file),</para>
      ///                    <para>"wa" (write-only access to append to existing data),</para>
      ///                    <para>"rw" (read and write access), or</para>
      ///                    <para>"rwt" (read and write access that truncates any existing file)</para>
      ///                    <para>["r" (read-only)] ???,</para></param>
      /// <returns></returns>
      public partial Stream? OpenFile(string fullpath, string mode);

      /// <summary>
      /// löscht die Datei
      /// </summary>
      /// <param name="fullpath">Android- oder Volume-Pfad</param>
      /// <returns></returns>
      public partial bool DeleteFile(string fullpath);

      /// <summary>
      /// erzeugt ein Verzeichnis
      /// </summary>
      /// <param name="fullpath">Android- oder Volume-Pfad</param>
      /// <returns></returns>
      public partial bool CreateDirectory(string fullpath);

      /// <summary>
      /// löscht das Verzeichnis
      /// </summary>
      /// <param name="fullpath">Android- oder Volume-Pfad</param>
      /// <returns></returns>
      public partial bool DeleteDirectory(string fullpath);

      /// <summary>
      /// verschiebt eine Datei (einschließlich Umbenennung)
      /// </summary>
      /// <param name="fullpathsrc"></param>
      /// <param name="fullpathdst"></param>
      /// <returns></returns>
      public partial bool Move(string fullpathsrc, string fullpathdst);

      /// <summary>
      /// kopiert eine Datei in eine neue Datei
      /// </summary>
      /// <param name="fullpathsrc"></param>
      /// <param name="fullpathdst"></param>
      /// <returns></returns>
      public partial bool Copy(string fullpathsrc, string fullpathdst);

      /// <summary>
      /// liefert die Länge einer Datei, Lese- und Schreibberechtigung und den Zeitpunkt der letzten Änderung
      /// <para>Das Leserecht kann nur beim secondary external storage false ergeben.</para>
      /// </summary>
      /// <param name="fullpath"></param>
      /// <param name="pathisdir">wenn true, ist Pfad ein Verzeichnis</param>
      /// <param name="canread"></param>
      /// <param name="canwrite"></param>
      /// <param name="lastmodified"></param>
      /// <returns>Dateilänge</returns>
      public partial long GetFileAttributes(string fullpath,
                                            bool pathisdir,
                                            out bool canread,
                                            out bool canwrite,
                                            out DateTime lastmodified);

      /// <summary>
      /// setzt den ReadOnly-Status und den Zeitpunkt der letzten Änderung
      /// <para>fkt. NICHT im secondary external storage</para>
      /// </summary>
      /// <param name="fullpath"></param>
      /// <param name="canwrite"></param>
      /// <param name="lastmodified"></param>
      public partial void SetFileAttributes(string fullpath, bool canwrite, DateTime lastmodified);

#if DEBUG

      //public partial object? test1(object? o1);

#endif
   }

}
