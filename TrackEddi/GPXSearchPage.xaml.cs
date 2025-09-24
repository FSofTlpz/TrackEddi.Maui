using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TrackEddi.Common;

namespace TrackEddi {
   public partial class GPXSearchPage : ContentPage {

      MainPage mainPage;
      GpxWorkbench gpxWorkbench;
      List<string> gpxFilenames;
      readonly ObservableCollection<GPXSearchPage_ListViewObjectItem> gpxfilelst;


      public GPXSearchPage(MainPage mainpage,
                           GpxWorkbench gpxworkbench,
                           List<string> gpxfilenames) {
         InitializeComponent();
         mainPage = mainpage;
         gpxWorkbench = gpxworkbench;
         gpxFilenames = gpxfilenames;
         gpxfilelst = new ObservableCollection<GPXSearchPage_ListViewObjectItem>();
      }

      protected override void OnAppearing() {
         base.OnAppearing();
         foreach (string filename in gpxFilenames)
            gpxfilelst.Add(new GPXSearchPage_ListViewObjectItem(filename));
         ListViewFiles.ItemsSource = gpxfilelst;
      }

      async Task<bool> load(string filename) => await mainPage.Loadfile2gpxworkbench(gpxWorkbench, filename, true, false);

      private async void Button_Clicked(object sender, EventArgs e) {
         int count = 0;
         for (int i = gpxfilelst.Count - 1; i >= 0; i--) {
            if (gpxfilelst[i].IsMarked) {
               try {
                  if (await load(gpxfilelst[i].FullFilename)) {
                     gpxfilelst.RemoveAt(i);
                     gpxFilenames.RemoveAt(i);
                     count++;
                  }
               } catch (Exception ex) {
                  throw new Exception(ex.Message);
               }
            }
         }
         if (count > 0)
            await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
      }
   }
}