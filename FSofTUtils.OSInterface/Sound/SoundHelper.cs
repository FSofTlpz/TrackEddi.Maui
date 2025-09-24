namespace FSofTUtils.OSInterface.Sound {
   public partial class SoundHelper {

      public static partial Task<List<SoundData>> GetNativeSoundData(bool intern,
                                                                     bool isalarm,
                                                                     bool isnotification,
                                                                     bool isringtone,
                                                                     bool ismusic);

      public static partial void PlayExclusiveNativeSound(SoundData soundData, float volume = 1, bool looping = true);

      /// <summary>
      /// Ein Pfad, der mit '//' anfängt, muss mit einer Authority, z.B. 'media' beginnen. Jeder andere Pfad wird als normaler Dateipfad interpretiert.
      /// </summary>
      /// <param name="path"></param>
      /// <param name="volume"></param>
      /// <param name="looping"></param>
      public static partial void PlayExclusiveNativeSound(string path, float volume = 1, bool looping = true);

      public static partial void StopExclusiveNativeSound();

   }
}
