namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class ShowGarminInfo4LonLat : ContentPage {

      private List<ShowGarminInfo4LonLat_ListViewObjectItem> _listOfResultsLists;

      public List<ShowGarminInfo4LonLat_ListViewObjectItem> ListOfResultsLists {
         get => _listOfResultsLists;
         set {
            _listOfResultsLists = value;
            base.OnPropertyChanged();
         }
      }


      public ShowGarminInfo4LonLat(IList<GarminImageCreator.SearchObject> infos, string pretext) {
         InitializeComponent();

         _listOfResultsLists = new List<ShowGarminInfo4LonLat_ListViewObjectItem>();
         foreach (GarminImageCreator.SearchObject so in infos)
            ListOfResultsLists.Add(new ShowGarminInfo4LonLat_ListViewObjectItem(so));
         ListViewResults.ItemsSource = ListOfResultsLists;

         LabelPreText.Text = pretext.Trim();
      }
   }
}