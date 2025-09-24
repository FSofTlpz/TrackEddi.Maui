using System.ComponentModel;

namespace FSofTUtils.OSInterface.Sound {
   public class SoundDataExt : SoundHelper.SoundData, INotifyPropertyChanged {

      public event PropertyChangedEventHandler? PropertyChanged;


      static float _volume = 1F;

      static public float Volume {
         get => _volume;
         set {
            if (_volume != value)
               _volume = value;
         }
      }

      bool _active;

      public bool Active {
         get => _active;
         set {
            if (_active != value) {
               _active = value;
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
               playSound(_active);
            }
         }
      }


      public SoundDataExt(SoundHelper.SoundData sd) : base(sd) {
         Active = false;
      }

      protected void playSound(bool start) {
         if (start)
            SoundHelper.PlayExclusiveNativeSound(this, Volume, true);
         else
            SoundHelper.StopExclusiveNativeSound();
      }

   }
}
