using FSofTUtils.OSInterface.Sound;

namespace FSofTUtils.OSInterface.Page {

   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class SoundPickerPage : ContentPage {

      public class CloseEventArgs {

         /// <summary>
         /// null, wenn keine Auswahl erfolgt ist
         /// </summary>
         public SoundHelper.SoundData NativeSoundData {
            get;
            protected set;
         }

         public double Volume {
            get;
            protected set;
         }


         public CloseEventArgs(SoundHelper.SoundData data, double volume) {
            NativeSoundData = data;
            Volume = volume;
         }
      }

      /// <summary>
      /// wird immer beim Schließen der Seite ausgelöst
      /// </summary>
      public event EventHandler<CloseEventArgs>? CloseEvent;



      public SoundPickerPage() {
         InitializeComponent();
         soundPicker.Volume = 1;
      }

      protected override async void OnAppearing() {
         base.OnAppearing();

         soundPicker.BusyChangedEvent += (s, e) => {
            BusyIndicator.IsRunning = e;
         };

         await soundPicker.CollectSoundData();

         if (soundPicker.SelectedIndex >= 0)
            soundPicker.ScrollTo(soundPicker.SelectedIndex, false);  // ScrollTo() fkt. vorher noch NICHT
      }

      protected override void OnDisappearing() {
         base.OnDisappearing();
         SoundDataExt? soundData = soundPicker.SelectedSound;
         if (soundData != null)
            soundData.Active = false;
      }

      public void AddAdditionalAudiofiles(IList<string> audiofiles) {
         foreach (var file in audiofiles)
            soundPicker.AppendFile(file);
      }

      /// <summary>
      /// Sound markieren und Lautstärke setzen
      /// </summary>
      /// <param name="sound"></param>
      /// <param name="volume"></param>
      public void InitSound(string? sound, double volume = 1) {
         if (!string.IsNullOrEmpty(sound)) {
            sound = sound.Trim();
            if (sound != "") {
               int idx = soundPicker.GetIndex4File(sound);
               if (idx < 0)
                  idx = soundPicker.GetIndex4Title(sound);
               if (idx >= 0)
                  soundPicker.ScrollTo(idx, true);    // fkt. hier NICHT -> OnAppearing()
            }
            soundPicker.Volume = volume;
         }
      }

      private async void soundPicker_CloseEvent(object sender, EventArgs e) {
         Control.SoundPicker sp = (Control.SoundPicker)sender;
         if (sp.Result != null)
            CloseEvent?.Invoke(this, new CloseEventArgs(sp.Result, sp.Volume));
         await Helper.GoBack();
      }

   }
}