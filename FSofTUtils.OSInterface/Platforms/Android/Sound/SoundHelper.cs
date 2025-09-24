using Android.Content;
using Android.Media;
using Android.Provider;

namespace FSofTUtils.OSInterface.Sound {

   public partial class SoundHelper {

      /// <summary>
      /// liefert eine Liste mit Sounds
      /// </summary>
      /// <param name="intern">mit internen Sounds</param>
      /// <param name="isalarm">mit Alarmsounds</param>
      /// <param name="isnotification">mit Benachrichtigungssounds</param>
      /// <param name="isringtone">mit Telefonklingelsounds</param>
      /// <param name="ismusic">mit Musiksounds</param>
      /// <returns></returns>
      public static partial async Task<List<SoundData>> GetNativeSoundData(bool intern,
                                                                           bool isalarm,
                                                                           bool isnotification,
                                                                           bool isringtone,
                                                                           bool ismusic) {
         List<SoundData> lst = new List<SoundData>();

         await Task.Run(() => {
            ContentResolver? contentResolver = AndroidFunction.GetContentResolver;
            if (contentResolver == null)
               return;

            string where = "";
            if (isalarm)
               where = (where.Length == 0 ? "" : " or ") + MediaStore.Audio.Media.InterfaceConsts.IsAlarm + ">0";
            if (isnotification)
               where = (where.Length == 0 ? "" : " or ") + MediaStore.Audio.Media.InterfaceConsts.IsNotification + ">0";
            if (isringtone)
               where = (where.Length == 0 ? "" : " or ") + MediaStore.Audio.Media.InterfaceConsts.IsRingtone + ">0";
            if (ismusic)
               where = (where.Length == 0 ? "" : " or ") + MediaStore.Audio.Media.InterfaceConsts.IsMusic + ">0";

            Android.Net.Uri? contenturi = intern ?
                                                MediaStore.Audio.Media.InternalContentUri :
                                                MediaStore.Audio.Media.ExternalContentUri;
            if (contenturi != null) {
               Android.Database.ICursor? cursor = contentResolver?.Query(contenturi,
                                                                         [
                                                                            MediaStore.Audio.Media.InterfaceConsts.Id,
                                                                            MediaStore.Audio.Media.InterfaceConsts.Title,
                                                                            MediaStore.Audio.Media.InterfaceConsts.Data,
                                                                         ],
                                                                         where,
                                                                         null,
                                                                         null);
               if (cursor != null && cursor.Count > 0) {
                  cursor.MoveToFirst();
                  do {
                     string? id = "";
                     string? name = "";
                     string? data = "";
                     for (int i = 0; i < cursor.ColumnCount; i++) {
                        string? colname = cursor.GetColumnName(i);
                        if (colname != null) {
                           if (colname == MediaStore.Audio.Media.InterfaceConsts.Id)
                              id = cursor.GetString(i);
                           else if (colname == MediaStore.Audio.Media.InterfaceConsts.Title)
                              name = cursor.GetString(i);
                           else if (colname == MediaStore.Audio.Media.InterfaceConsts.Data)
                              data = cursor.GetString(i);
                        }
                     }
                     if (id != null &&
                         name != null &&
                         data != null)
                        lst.Add(new SoundData(intern, id, name, data));
                  }
                  while (!cursor.IsAfterLast && cursor.MoveToNext());
                  cursor.Close();
               }
            }
         });

         return lst;
      }

      static Ringtone? exclusiveRingtone = null;

      /// <summary>
      /// spielt den Sound ab
      /// </summary>
      /// <param name="soundData"></param>
      /// <param name="volume">0..1</param>
      /// <param name="looping"></param>
      public static partial void PlayExclusiveNativeSound(SoundData soundData,
                                                          float volume,
                                                          bool looping) {
         /*    nativeSoundData.Intern == true                                 -> MediaStore.Audio.Media.InternalContentUri
          *    nativeSoundData.Intern == false && nativeSoundData.ID != ""    -> MediaStore.Audio.Media.ExternalContentUri.ToString() + "/" + nativeSoundData.ID
          *    nativeSoundData.Intern == false && nativeSoundData.ID == ""    -> nativeSoundData.Data
          */
         Android.Net.Uri? contentUri = soundData.Intern ? MediaStore.Audio.Media.InternalContentUri : MediaStore.Audio.Media.ExternalContentUri;
         Android.Net.Uri? uri = soundData.ID == "" ?
                                             AndroidFunction.UriParse(soundData.Data) :       // ev. "file:" + ... ?
                                             AndroidFunction.UriParse(contentUri != null ?
                                                                        contentUri.ToString() + "/" + soundData.ID :
                                                                        "");
         if (uri != null)
            playExclusiveNativeSound(uri, volume, looping);
      }

      /// <summary>
      /// Ein Pfad, der mit '//' anfängt, muss mit einer Authority, z.B. 'media' beginnen. Jeder andere Pfad wird als normaler Dateipfad interpretiert.
      /// </summary>
      /// <param name="path"></param>
      /// <param name="volume"></param>
      /// <param name="looping"></param>
      public static partial void PlayExclusiveNativeSound(string path,
                                                          float volume,
                                                          bool looping) {
         if (path.StartsWith("//"))
            playExclusiveNativeSound(AndroidFunction.UriParse("content:" + path), volume, looping);
         else
            playExclusiveNativeSound(AndroidFunction.UriParse("file:" + path), volume, looping);
      }

      static void playExclusiveNativeSound(Android.Net.Uri? uri, float volume = 1.0F, bool looping = true) {
         StopExclusiveNativeSound();
         if (uri != null) {
            Android.Content.Context? context = AndroidFunction.ApplicationContext;
            if (context != null) {
               Ringtone? rt = RingtoneManager.GetRingtone(context, uri);
               if (rt != null) {
                  Android.Media.AudioAttributes.Builder b = new AudioAttributes.Builder();
                  b.SetUsage(AudioUsageKind.Alarm);
                  b.SetContentType(AudioContentType.Music);
                  rt.AudioAttributes = b.Build();
                  if (System.OperatingSystem.IsAndroidVersionAtLeast(28)) {
                     rt.Looping = looping;
                     rt.Volume = Math.Max(0F, Math.Min(volume, 1F));
                  }
                  rt.Play();
                  exclusiveRingtone = rt;
               }
            }
         }
      }

      /// <summary>
      /// stopt den Sound
      /// </summary>
      public static partial void StopExclusiveNativeSound() {
         if (exclusiveRingtone != null && exclusiveRingtone.IsPlaying)
            exclusiveRingtone.Stop();
         if (exclusiveRingtone != null)
            exclusiveRingtone.Dispose();
         exclusiveRingtone = null;
      }

   }
}