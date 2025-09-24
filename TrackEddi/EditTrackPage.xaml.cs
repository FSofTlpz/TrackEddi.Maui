using SpecialMapCtrl;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class EditTrackPage : ContentPage {

      /// <summary>
      /// Dialog mit Auswahl beendet
      /// </summary>
      public event EventHandler<EventArgs>? EndWithOk;

      public Track Track { get; protected set; }

      public Color TrackColor {
         get => WinHelper.ConvertColor(Track.LineColor);
         set {
            System.Drawing.Color col = WinHelper.ConvertColor(value);
            if (col != Track.LineColor) {
               Track.LineColor = col;
               OnPropertyChanged(nameof(TrackColor));
            }
         }
      }

      public string Name {
         get => Track.GpxTrack.Name;
         set => Track.GpxTrack.Name = value;
      }

      public string Description {
         get => Track.GpxTrack.Description;
         set => Track.GpxTrack.Description = value;
      }

      public string Comment {
         get => Track.GpxTrack.Comment;
         set => Track.GpxTrack.Comment = value;
      }

      public string Source {
         get => Track.GpxTrack.Source;
         set => Track.GpxTrack.Source = value;
      }

      public string Length {
         get {
            double len = Track.LengthTS();
            return len < 1000 ?
                           len.ToString() + " m" :
                           (len / 1000).ToString("f1") + " km";
         }
      }

      public ImageSource ElevationProfile {
         get {
            System.Drawing.Bitmap bm = Common.TrackHeightProfile.BuildImage4Track(1200, 900, Track, new List<int>());
            pictdata = WinHelper.GetImageSource4WindowsBitmap(bm, out ImageSource ims);
            return ims;
         }
      }

      byte[]? pictdata;

      public string StatisticalInfo => Track.GetSimpleStatsText().Trim();



      public EditTrackPage(Track track) {
         InitializeComponent();

         Track = track;
         BindingContext = this;
      }

      async private void Button_Clicked(object sender, EventArgs e) {
         EndWithOk?.Invoke(this, EventArgs.Empty);
         await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
      }

      async private void TapGestureRecognizerColor_Tapped(object sender, EventArgs e) {
         try {
            ColorChoosingPage page = new ColorChoosingPage() {
               ActualColor = WinHelper.ConvertColor(Track.LineColor),
            };
            page.EndWithOk += (object? sender2, EventArgs e2) => {
               TrackColor = page.ActualColor;
            };
            await FSofTUtils.OSInterface.Helper.GoTo(page);
         } catch (Exception ex) {
            await FSofTUtils.OSInterface.Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

   }
}