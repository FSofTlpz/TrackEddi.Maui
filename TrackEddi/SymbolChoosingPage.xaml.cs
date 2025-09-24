using FSofTUtils.Geography.Garmin;
using System.Diagnostics;

namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class SymbolChoosingPage : ContentPage {

      /// <summary>
      /// Dialog mit Auswahl beendet
      /// </summary>
      public event EventHandler<EventArgs>? EndWithOk;


      //public Command<SymbolObjectItem> ChooseSymbol { get; private set; }

      private List<SymbolGroupList> _listOfGroupLists = new List<SymbolGroupList>();

      public List<SymbolGroupList> ListOfGroupLists {
         get => _listOfGroupLists;
         set {
            _listOfGroupLists = value;
            base.OnPropertyChanged();
         }
      }

      public GarminSymbol? ActualGarminSymbol { get; protected set; }

      string oldsymbolname = "Flag, Green";            // <--> passend zum VisualMarker für editierbare Marker

      readonly IList<GarminSymbol> garminmarkersymbols;


      public SymbolChoosingPage(IList<GarminSymbol> garminmarkersymbols, string oldsymbolname) {
         InitializeComponent();

         BindingContext = this;

         this.garminmarkersymbols = garminmarkersymbols;

         if (!string.IsNullOrEmpty(oldsymbolname))
            this.oldsymbolname = oldsymbolname;

         //ChooseSymbol = new Command<SymbolObjectItem>(onChoosing);

         initList(this.garminmarkersymbols);
         ListViewSymbols.ItemsSource = ListOfGroupLists;
      }

      T getItem<T>(object sender) {
         if (sender is ImageButton)
            return (T)((ImageButton)sender).CommandParameter;

         if (sender is Button)
            return (T)((Button)sender).CommandParameter;

         //if (sender is TapGestureRecognizer) {
         //   TapGestureRecognizer g = (TapGestureRecognizer)sender;
         //   return g.CommandParameter != null ?
         //               (WorkbenchContentPage_ListViewObjectItem)g.CommandParameter :
         //               new WorkbenchContentPage_ListViewObjectItem(new Track("FEHLER"));
         //}

         throw new Exception(nameof(WorkbenchContentPage) + "." +
                             nameof(getItem) +
                             "(): Falscher Parametertyp: " +
                             sender.GetType().Name);
      }

      T getItem<T>(TappedEventArgs e) {
         if (e.Parameter != null)
            if (e.Parameter is T)
               return (T)e.Parameter;

         throw new Exception(nameof(WorkbenchContentPage) + "." +
                             nameof(getItem) +
                             "(): Falscher Parametertyp: " +
                             e.Parameter?.GetType().Name);
      }

      private void ChooseSymbolTapped(object sender, TappedEventArgs e) =>
         onChoosing(getItem<SymbolObjectItem>(e));


      //bool firstOnAppearing = true;

      //protected override void OnAppearing() {
      //   base.OnAppearing();

      //   if (firstOnAppearing) {
      //      firstOnAppearing = false;

      //   }
      //}


      void initList(IList<GarminSymbol> garminmarkersymbols) {
         BusyIndicator.IsRunning = true;

         ListOfGroupLists.Clear();

         SymbolGroupList? grouplst = null;
         string lastgroupname = string.Empty;
         SymbolObjectItem? oldsymbol = null;

         foreach (var item in garminmarkersymbols) {
            if (lastgroupname != item.Group) {
               lastgroupname = item.Group;
               grouplst = new SymbolGroupList() {
                  Groupname = lastgroupname,
               };
               ListOfGroupLists.Add(grouplst);
            }
            if (grouplst != null) {
               grouplst.Add(new SymbolObjectItem(item));

               if (!string.IsNullOrEmpty(oldsymbolname) &&
                   oldsymbolname == item.Name) {
                  oldsymbol = grouplst[grouplst.Count - 1];
                  ActualGarminSymbol = oldsymbol.GarminSymbol;
               }
            }
         }

         if (oldsymbol != null) {
            ListViewSymbols.SelectedItem = oldsymbol;
            ListViewSymbols.ScrollTo(oldsymbol, ScrollToPosition.Center, false);
         }

         BusyIndicator.IsRunning = false;
      }

      async void onChoosing(SymbolObjectItem td) {
         ActualGarminSymbol = td.GarminSymbol;
         EndWithOk?.Invoke(this, EventArgs.Empty);
         await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
      }

   }
}