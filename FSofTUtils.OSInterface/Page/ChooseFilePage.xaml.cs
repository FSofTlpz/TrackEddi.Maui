using FSofTUtils.OSInterface.Control;
using FSofTUtils.OSInterface.Storage;
using System.Text.RegularExpressions;
using static FSofTUtils.OSInterface.Control.ChooseFile;

namespace FSofTUtils.OSInterface.Page {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class ChooseFilePage : ContentPage {

      public event EventHandler<ChoosePathAndFileEventArgs>? ChooseFileReadyEvent;


      public object? AndroidActivity { get; set; }

      /// <summary>
      /// letzter ausgewählter Pfad (aus dem App-Dictionary)
      /// <para>Wird i.A. nur von den aufrufenden Pages gesetzt und gelesen.</para>
      /// </summary>
      public static string LastChoosedPath {
         get => ChooseFile.LastChoosedPath;
         set => ChooseFile.LastChoosedPath = value;
      }

      /// <summary>
      /// Startpfad für die Auswahl (und Endpfad falls <see cref="Result"/>==true)
      /// </summary>
      public string Path { get; set; } = "";

      /// <summary>
      /// Startdateiname für die Auswahl (und Enddateiname falls <see cref="Result"/>==true)
      /// </summary>
      public string Filename { get; set; } = "";

      /// <summary>
      /// Können nur existierende Dateien ausgewählt werden (dann keine Eingabemöglichkeit für Dateiname)?
      /// </summary>
      public bool OnlyExistingFile { get; set; } = true;

      /// <summary>
      /// bei true ist nur die Auswahl eines (existierenden) Verzeichnisses möglich
      /// </summary>
      public bool OnlyExistingDirectory { get; set; } = false;

      /// <summary>
      /// Ergebnis der Auswahl
      /// </summary>
      public bool Result {
         get; protected set;
      }

      /// <summary>
      /// null oder nur Dateinamen passen zu dieser RegEx werden angezeigt
      /// </summary>
      public Regex? Match4Filenames { get; set; } = null;

      /// <summary>
      /// null oder nur Verzeichnisnamen passen zu dieser RegEx werden angezeigt
      /// </summary>
      public Regex? Match4Dirnames { get; set; } = null;



      StorageHelper? storageHelper = null;

      List<string> VolumenPaths = new List<string>();

      bool hasChooseFileReadyEvent = false;


      public ChooseFilePage() {
         InitializeComponent();
      }

      protected override async void OnAppearing() {
         base.OnAppearing();

         if (AndroidActivity == null)
            AndroidActivity = DirtyGlobalVars.AndroidActivity;

         if (AndroidActivity != null)
            await init(AndroidActivity);
      }

      protected override void OnDisappearing() {
         base.OnDisappearing();

         if (!hasChooseFileReadyEvent)
            ChooseFileReadyEvent?.Invoke(this, new ChoosePathAndFileEventArgs(false, Path, Filename));
      }

      async Task init(object androidactivity) {
         try {

            Result = false;

            if (androidactivity == null)
               throw new Exception("AndroidActivity darf nicht null sein.");

            storageHelper = new StorageHelper(androidactivity);
            chooseFile.Match4Filenames = Match4Filenames;
            chooseFile.Match4Dirnames = Match4Dirnames;
            chooseFile.BusyChangedEvent += (s, e) => {
               BusyIndicator.IsRunning = e;
            };
            await chooseFile.Activate(Path, Filename, OnlyExistingFile, OnlyExistingDirectory, storageHelper);

            VolumenPaths = storageHelper.VolumePaths;

            if (VolumenPaths.Count < 1)
               throw new Exception("Kein external Storage vorhanden.");

         } catch (Exception ex) {
            await Helper.MessageBox(this, "Fehler", ex.Message);
         }
      }

      async void ctrl_ChooseFileReadyEvent(object sender, ChoosePathAndFileEventArgs e) {
         if (e.OK) {
            Path = e.Path;
            Filename = e.Filename;
            Result = e.OK;
         }
         hasChooseFileReadyEvent = true;
         await Helper.GoBack();
         ChooseFileReadyEvent?.Invoke(this, new ChoosePathAndFileEventArgs(e.OK, e.Path, e.Filename));
      }
   }
}