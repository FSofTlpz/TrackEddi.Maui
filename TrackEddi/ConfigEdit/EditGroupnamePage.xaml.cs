namespace TrackEddi.ConfigEdit {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class EditGroupnamePage : ContentPage {

      public string Groupname = "";

      public bool Ok { get; protected set; } = false;


      public EditGroupnamePage(string groupname) {
         InitializeComponent();

         entryGroupname.Text = Groupname = groupname;
      }

      private async void btnSave_Clicked(object sender, EventArgs e) {
         Groupname = entryGroupname.Text.Trim();
         Ok = Groupname.Length > 0;
         await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
      }

      private void entryGroupname_TextChanged(object sender, TextChangedEventArgs e) {
         Entry entry = (Entry)sender;
         btnSave.IsEnabled = entry.Text.Trim().Length > 0;
      }
   }
}