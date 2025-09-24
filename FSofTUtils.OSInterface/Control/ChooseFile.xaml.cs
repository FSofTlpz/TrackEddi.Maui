using FSofTUtils.OSInterface.Storage;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FSofTUtils.OSInterface.Control {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class ChooseFile : ContentView {

      const string LASTCHOOSEPATH = "ChooseFile.lastChoosePath";

      #region Binding-Variablen

      #region  Binding-Var ImageFolder

      public static readonly BindableProperty ImageFolderProperty = BindableProperty.Create(
         nameof(ImageFolder),
         typeof(ImageSource),
         typeof(ChooseFile),
         imgFolder);

      /// <summary>
      /// Bild für ein Verzeichnis-Item
      /// </summary>
      public ImageSource ImageFolder {
         get => (ImageSource)GetValue(ImageFolderProperty);
         set => SetValue(ImageFolderProperty, value);
      }

      #endregion

      #region  Binding-Var ImageFile

      public static readonly BindableProperty ImageFileProperty = BindableProperty.Create(
         nameof(ImageFile),
         typeof(ImageSource),
         typeof(ChooseFile),
         imgFile);

      /// <summary>
      /// Bild für ein Datei-Item
      /// </summary>
      public ImageSource ImageFile {
         get => (ImageSource)GetValue(ImageFileProperty);
         set => SetValue(ImageFileProperty, value);
      }

      #endregion

      #region  Binding-Var FontSize

      public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
         nameof(FontSize),
         typeof(double),
         typeof(ChooseFile),
         new FontSizeConverter().ConvertFromInvariantString("Medium"));

      /// <summary>
      /// Textgröße
      /// </summary>
      public double FontSize {
         get => (double)GetValue(FontSizeProperty);
         set => SetValue(FontSizeProperty, value);
      }

      #endregion

      #region  Binding-Var ColorMainText

      public static readonly BindableProperty ColorMainTextProperty = BindableProperty.Create(
         nameof(ColorMainText),
         typeof(Color),
         typeof(ChooseFile),
         Colors.Black);

      /// <summary>
      /// Textfarbe des Haupttextes
      /// </summary>
      public Color ColorMainText {
         get => (Color)GetValue(ColorMainTextProperty);
         set => SetValue(ColorMainTextProperty, value);
      }

      #endregion

      #region  Binding-Var BackcolorMainText

      public static readonly BindableProperty BackcolorMainTextProperty = BindableProperty.Create(
         nameof(BackcolorMainText),
         typeof(Color),
         typeof(ChooseFile),
         Colors.LightBlue);

      /// <summary>
      /// Hintergrundfarbe des Haupttextes
      /// </summary>
      public Color BackcolorMainText {
         get => (Color)GetValue(BackcolorMainTextProperty);
         set => SetValue(BackcolorMainTextProperty, value);
      }

      #endregion

      #region  Binding-Var InfoFontSize

      public static readonly BindableProperty InfoFontSizeProperty = BindableProperty.Create(
         nameof(InfoFontSize),
         typeof(double),
         typeof(ChooseFile),
         new FontSizeConverter().ConvertFromInvariantString("Small"));

      /// <summary>
      /// Textgröße
      /// </summary>
      public double InfoFontSize {
         get => (double)GetValue(InfoFontSizeProperty);
         set => SetValue(InfoFontSizeProperty, value);
      }

      #endregion

      #region  Binding-Var ColorInfoText

      public static readonly BindableProperty ColorInfoTextProperty = BindableProperty.Create(
         nameof(ColorInfoText),
         typeof(Color),
         typeof(ChooseFile),
         Colors.DarkGray);

      /// <summary>
      /// Textfarbe des Haupttextes
      /// </summary>
      public Color ColorInfoText {
         get => (Color)GetValue(ColorInfoTextProperty);
         set => SetValue(ColorInfoTextProperty, value);
      }

      #endregion

      #region  Binding-Var BackcolorInfoText

      public static readonly BindableProperty BackcolorInfoTextProperty = BindableProperty.Create(
         nameof(BackcolorInfoText),
         typeof(Color),
         typeof(ChooseFile),
         Colors.White);

      /// <summary>
      /// Hintergrundfarbe des Haupttextes
      /// </summary>
      public Color BackcolorInfoText {
         get => (Color)GetValue(BackcolorInfoTextProperty);
         set => SetValue(BackcolorInfoTextProperty, value);
      }

      #endregion

      #region  Binding-Var ListViewItemColor

      public static readonly BindableProperty ListViewItemColorProperty = BindableProperty.Create(
         nameof(ListViewItemColor),
         typeof(Color),
         typeof(ChooseFile),
         Colors.AliceBlue);

      /// <summary>
      /// Hintergrundfarbe der Items
      /// </summary>
      public Color ListViewItemColor {
         get => (Color)GetValue(ListViewItemColorProperty);
         set => SetValue(ListViewItemColorProperty, value);
      }

      #endregion

      #region  Binding-Var SelectedListViewItemColor

      public static readonly BindableProperty SelectedListViewItemColorProperty = BindableProperty.Create(
         nameof(SelectedListViewItemColor),
         typeof(Color),
         typeof(ChooseFile),
         Colors.LightSteelBlue);

      /// <summary>
      /// Hintergrundfarbe des ausgewählten Items
      /// </summary>
      public Color SelectedListViewItemColor {
         get => (Color)GetValue(SelectedListViewItemColorProperty);
         set => SetValue(SelectedListViewItemColorProperty, value);
      }

      #endregion

      #region  Binding-Var ListViewSeparatorColor

      public static readonly BindableProperty ListViewSeparatorColorProperty = BindableProperty.Create(
         nameof(ListViewSeparatorColor),
         typeof(Color),
         typeof(ChooseFile),
         Colors.Black);

      /// <summary>
      /// Farbe der Item-Trennung
      /// </summary>
      public Color ListViewSeparatorColor {
         get => (Color)GetValue(ListViewSeparatorColorProperty);
         set => SetValue(ListViewSeparatorColorProperty, value);
      }

      #endregion

      #region  Binding-Var PathName

      public static readonly BindableProperty PathNameProperty = BindableProperty.Create(
         nameof(PathName),
         typeof(string),
         typeof(ChooseFile),
         "");

      /// <summary>
      /// angezeigter Pfad
      /// </summary>
      public string PathName {
         get => (string)GetValue(PathNameProperty);
         set => SetValue(PathNameProperty, value);
      }

      #endregion

      #region  Binding-Var PathInfo

      public static readonly BindableProperty PathInfoProperty = BindableProperty.Create(
         nameof(PathInfo),
         typeof(string),
         typeof(ChooseFile),
         "");

      /// <summary>
      /// Zusatzinfo zum angezeigten Pfad
      /// </summary>
      public string PathInfo {
         get => (string)GetValue(PathInfoProperty);
         set => SetValue(PathInfoProperty, value);
      }

      #endregion

      #endregion

      #region Events

      public class ChoosePathAndFileEventArgs : EventArgs {
         /// <summary>
         /// bei true ist eine Auswahl ohne Abbruch erfolgt
         /// </summary>
         public bool OK { get; private set; }
         /// <summary>
         /// Pfad
         /// </summary>
         public string Path { get; private set; }
         /// <summary>
         /// Dateiname
         /// </summary>
         public string Filename { get; private set; }

         public ChoosePathAndFileEventArgs(string path, string filename) {
            Path = path;
            Filename = filename;
         }

         public ChoosePathAndFileEventArgs(bool ok, string path, string filename) : this(path, filename) {
            OK = ok;
         }

      }

      /// <summary>
      /// Auswahl ist beendet
      /// </summary>
      public event EventHandler<ChoosePathAndFileEventArgs>? ChooseFileReadyEvent;

      public event EventHandler<bool>? BusyChangedEvent;

      #endregion

      static readonly ImageSource imgFolder = ImageSource.FromResource("FSofTUtils.OSInterface.Resource.Folder.png", typeof(ChooseFile).GetTypeInfo().Assembly);
      static readonly ImageSource imgFile = ImageSource.FromResource("FSofTUtils.OSInterface.Resource.File.png", typeof(ChooseFile).GetTypeInfo().Assembly);

      /// <summary>
      /// letzter ausgewählter Pfad (aus dem App-Dictionary)
      /// <para>Wird i.A. nur von den aufrufenden Pages gesetzt und gelesen.</para>
      /// </summary>
      public static string LastChoosedPath {
         get {
            //return Application.Current.Properties.ContainsKey(LASTCHOOSEPATH) ? Application.Current.Properties[LASTCHOOSEPATH] as string : "";
            return Preferences.Default.Get(LASTCHOOSEPATH, "");
         }
         set {
            //Application.Current.Properties[LASTCHOOSEPATH] = value;
            Preferences.Default.Set(LASTCHOOSEPATH, value);
         }
      }

      /// <summary>
      /// Dateiauswahl nur für bestehende Datei (true) oder beliebige Datei (false)
      /// </summary>
      public bool OnlyExistingFiles { get; private set; } = true;

      /// <summary>
      /// Dateiauswahl nur für bestehende Verzeichnisse
      /// </summary>
      public bool OnlyExistingDirectory { get; private set; } = false;

      /// <summary>
      /// bei true zusätzliche Infos für Volumes/Verzeichnisse (zusätzliche Zeit nötig!)
      /// </summary>
      public bool WithExtDirInfo { get; set; } = false;

      /// <summary>
      /// aktuell angezeigter Pfad
      /// </summary>
      public string ActualPath {
         get => dl != null ? dl.ActualPath : "";
      }

#if USE_STORAGEACCESSFRAMEWORK
      /// <summary>
      /// Fragt nach den Berechtigungen, falls für ein Volume nicht vorhanden.
      /// </summary>
      public bool Ask4Permissions { get; set; } = true;
#endif

      /// <summary>
      /// null oder nur Dateinamen passen zu dieser RegEx
      /// </summary>
      public Regex? Match4Filenames { get; set; } = null;

      /// <summary>
      /// null oder nur Verzeichnisnamen passen zu dieser RegEx
      /// </summary>
      public Regex? Match4Dirnames { get; set; } = null;


      DirectoryLister? dl = null;

      StorageHelper storageHelper;

      /// <summary>
      /// Zeichen, die im Dateinamen NICHT enthalten sein dürfen
      /// </summary>
      readonly char[] invalidChars4Filename;

      /// <summary>
      /// zuletzt selektierte <see cref="ViewCell"/>
      /// </summary>
      ViewCell? lastSelectedCell = null;



#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
      public ChooseFile() {
         InitializeComponent();

         BindingContext = this;

         ImageFile = imgFile;
         ImageFolder = imgFolder;

         // liefert nur '\0' und '/'   ????
         //List<char> tmpchars = new List<char>(Path.GetInvalidFileNameChars());
         //foreach (char item in Path.GetInvalidPathChars()) {
         //   if (!tmpchars.Contains(item))
         //      tmpchars.Add(item);
         //}
         //invalidChars4Filename = tmpchars.ToArray();
         invalidChars4Filename = ['\0', '?', ':', '"', '*', '|', '/', '\\', '<', '>'];
      }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

      /// <summary>
      /// liest die Daten für das Verzeichnis ein und stellt sie dar
      /// </summary>
      /// <param name="fullfilename"></param>
      /// <param name="onlyexistingfiles"></param>
      /// <param name="onlyexistingfolders"></param>
      /// <param name="storageHelper"></param>
      public async void Activate(
                           string fullfilename = "",
                           bool onlyexistingfiles = true,
                           bool onlyexistingfolders = true,
                           StorageHelper? storageHelper = null) {
         string? initpath = string.Empty;
         string filename = string.Empty;
         if (fullfilename != null)
            if (fullfilename.Contains(Path.DirectorySeparatorChar.ToString())) {
               initpath = Path.GetDirectoryName(fullfilename);
               filename = Path.GetFileName(fullfilename);
            }
         await Activate(initpath ?? string.Empty,
                   filename,
                   onlyexistingfiles,
                   onlyexistingfolders,
                   storageHelper);
      }

      /// <summary>
      /// liest die Daten für das Verzeichnis ein und stellt sie dar
      /// </summary>
      /// <param name="initpath"></param>
      /// <param name="filename"></param>
      /// <param name="onlyexistingfiles"></param>
      /// <param name="onlyexistingfolders"></param>
      /// <param name="storageHelper"></param>
      public async Task Activate(
                           string initpath = "",
                           string filename = "",
                           bool onlyexistingfiles = true,
                           bool onlyexistingfolders = true,
                           StorageHelper? storageHelper = null) {
         if (this.storageHelper == null && storageHelper == null)
            throw new ArgumentException("The parameter storageHelper must not be null");
         if (storageHelper != null)
            this.storageHelper = storageHelper;

         string path = !string.IsNullOrEmpty(initpath) ? initpath : "";

         entryDestinationStack.IsVisible = !onlyexistingfiles && !onlyexistingfolders;
         entryDestinationfile.Text = !string.IsNullOrEmpty(filename) ? filename.Trim() : "";
         buttonFolderOK.IsVisible = onlyexistingfolders;

         OnlyExistingFiles = onlyexistingfiles;
         OnlyExistingDirectory = onlyexistingfolders;

         await showContent(OnlyExistingDirectory, listViewDir, null, path);


         // Anzeige eines selektierten Items mit der Wunschfarbe fkt. hier nicht. Man müßte ein "Tapped" simulieren, aber dazu benötigt man die korrekte ViewCell.
         // Verbindung zwischen DirItem und ViewCell ex. nicht.

         //if (!string.IsNullOrEmpty(filename) &&
         //    dl != null) {
         //   for (int i = 0; i < dl.ActualContent.Count; i++) {
         //      if (dl.ActualContent[i].Name == filename) {
         //
         //         break;
         //      }
         //   }
         //}
      }

      /*
      List<DirectoryLister.Item> lst;
      if (lvdir.ItemsSource != null) {
         lst = lvdir.ItemsSource as List<DirectoryLister.Item>;
         lst.Clear();
         lvdir.ItemsSource = null;
      }

      lst = dl.Content(initpath);
      if (lst != null) {
         foreach (DirectoryLister.Item item in lst) {
            System.Diagnostics.Debug.WriteLine(string.Format("IsRoot={0}, IsDirectory={1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}",
                                                             item.IsRoot,
                                                             item.IsDirectory,
                                                             item.FullName,
                                                             item.Name,
                                                             item.InfoText,
                                                             item.Files,
                                                             item.Subdirs,
                                                             item.Filesize,
                                                             item.Timestamp,
                                                             item.Totalsize,
                                                             item.Availablesize,
                                                             item.RootIsemulated,
                                                             item.RootIsRemovable
                                                            ));
         }
      }

      lvdir.ItemsSource = lst;
      */

      /// <summary>
      /// erzeugt den <see cref="DirectoryLister"/> falls er noch nicht ex.
      /// </summary>
      /// <returns></returns>
      DirectoryLister createDirectoryLister() {
         if (dl == null) {
            dl = new DirectoryLister(storageHelper);
            dl.StorageHelper.RefreshVolumesAndSpecPaths();     // Roots einlesen
            for (int i = 0; i < dl.StorageHelper.Volumes; i++) {
#if USE_STORAGEACCESSFRAMEWORK
               bool needperms = false;
               if (Ask4Permissions &&
                   i >= 0)
                  needperms = !dl.StorageHelper.SetSafAndroidPersistentPermissions(i);
               if (i > 0 &&      // nicht für primary Volume (für Androis 10)
                   needperms)
                  ask4AndroidSafPersistentPermissonAndWait(i);
#endif
               StorageHelper.VolumeData? vd = dl.StorageHelper.GetVolumeData(i);
               if (vd != null)
                  dl.AddRoot(vd);
            }

         }
         return dl;
      }

#if USE_STORAGEACCESSFRAMEWORK
      void getAndroidSafPersistentPermisson(int idx) {
         bool needperms = false;
         if (Ask4Permissions &&
             idx >= 0 &&
             dl != null)
            needperms = !dl.StorageHelper.SetSafAndroidPersistentPermissions(idx);
         if (needperms)
            ask4AndroidSafPersistentPermissonAndWait(idx);
      }

      async void ask4AndroidSafPersistentPermissonAndWait(int volidx) {
         if (dl != null)
            await dl.StorageHelper.Ask4SafAndroidPersistentPermissonAndWait(volidx);
      }
#endif

      /// <summary>
      /// Verzeichnisinhalt asynchron holen
      /// </summary>
      /// <param name="dl"></param>
      /// <param name="initpath"></param>
      /// <param name="onlyfolders"></param>
      /// <param name="match4Filenames"></param>
      /// <param name="match4Dirnames"></param>
      /// <returns></returns>
      static async Task<List<DirItem>?> getDirContentAsync(
                        DirectoryLister dl,
                        string? initpath,
                        bool onlyfolders,
                        bool withExtDirInfo,
                        Regex? match4Filenames,
                        Regex? match4Dirnames) {
         List<DirItem>? newlst = null;
         await Task.Run(() => {
            newlst = dl.Content(initpath, onlyfolders, withExtDirInfo, match4Filenames, match4Dirnames);
            if (newlst.Count > 0 &&
                newlst[0].Name == "..")
               newlst.RemoveAt(0);
         });
         return newlst;
      }

      /// <summary>
      /// stellt den Inhalt des Pfades im <see cref="ListView"/> dar und erzeugt bei Bedarf den <see cref="DirectoryLister"/>
      /// </summary>
      /// <param name="onlyfolders"></param>
      /// <param name="lvdir"></param>
      /// <param name="dl"></param>
      /// <param name="initpath"></param>
      async Task showContent(bool onlyfolders,
                             ListView lvdir,
                             DirectoryLister? dl = null,
                             string? initpath = null) {
         BusyChangedEvent?.Invoke(this, true);

         try {

            dl = createDirectoryLister();
            lastSelectedCell = null;

            List<DirItem>? newlst = await getDirContentAsync(dl, initpath, onlyfolders, WithExtDirInfo, Match4Filenames, Match4Dirnames);

            if (newlst != null) {
               lvdir.ItemsSource = new List<DirItem>(newlst); // Kopie der Liste erzeugen!
               if (lvdir.ItemsSource == null)
                  throw new Exception("Der Verzeichnisinhalt kann nicht ermittelt werden.");

               if (labelPath != null) {
                  srollToEnd(labelPath);
                  LastChoosedPath = dl.ActualPath;
                  PathName = dl.ActualPath.Length == 0 ?
                                             "Volumes" :
                                             dl.ActualPath;

                  int dirs = 0, files = 0;
                  foreach (var item in lvdir.ItemsSource) {
                     if (item is DirItem di) {
                        if (di.IsDirectory)
                           dirs++;
                        else
                           files++;
                     }
                  }
                  PathInfo = string.Format("{0} Verzeichnis{1}, {2} Datei{3}", dirs, dirs != 1 ? "se" : "", files, files != 1 ? "en" : "");
               }
            }

         } catch (Exception ex) {
            ContentPage? page = getParentPage();
            if (page != null)
               await MessageBox(page, "Exception", ex.Message);
         } finally {
            BusyChangedEvent?.Invoke(this, false);
         }

      }

      /// <summary>
      /// liefert die übergeordnete <see cref="Page"/> oder null
      /// </summary>
      /// <returns></returns>
      ContentPage? getParentPage() {
         Element element = this;

         while (true) {
            if (element.Parent == null)
               break;
            if (element.Parent is ContentPage)
               return element.Parent as ContentPage;
            element = element.Parent;
         }

         return null;
      }

      /// <summary>
      /// setzt die Farbe der akt. selektierten <see cref="ViewCell"/>
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void viewCell_Tapped(object sender, System.EventArgs e) {
         if (lastSelectedCell != null)
            lastSelectedCell.View.BackgroundColor = Colors.Transparent;
         ViewCell viewCell = (ViewCell)sender;
         if (viewCell.View != null) {
            viewCell.View.BackgroundColor = SelectedListViewItemColor;
            lastSelectedCell = viewCell;
         }
      }

      /// <summary>
      /// Reaktion auf eine Auswahl in der Liste
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      async void listViewDir_ItemTapped(object sender, ItemTappedEventArgs e) {
         ListView lvdir = (ListView)sender;
         DirItem item = (DirItem)e.Item;
         if (item.IsDirectory) {    // dann Verzeichniswechsel

            await setDirectory(item.FullName);

         } else {                   // Dateiname wurde ausgewählt

            if (!OnlyExistingFiles) {  // dann erst mit dem OK-Button das ChooseFileReadyEvent
               entryDestinationfile.Text = item.Name;
            } else {                // nur Auswahl einer in der Liste ex. Datei möglich
               ChooseFileReadyEvent?.Invoke(this, new ChoosePathAndFileEventArgs(true,
                                                                                 dl != null ? dl.ActualPath : "?",
                                                                                 item.Name));
            }

         }
      }

      //async private void ListViewDir_ItemTapped2(object sender, EventArgs e) {
      //   await Helper.MessageBox(this, "Hi", "ListViewDir_ItemTapped2");
      //}

      //async private void ListViewDir_Swiped(object sender, SwipedEventArgs e) {
      //   await Helper.MessageBox(this, "Hi", "ListViewDir_Swiped");
      //}

      void buttonOK_Clicked(object sender, EventArgs e) => endFilenameEdit();

      void buttonFolderOK_Clicked(object sender, EventArgs e) =>
         ChooseFileReadyEvent?.Invoke(this, new ChoosePathAndFileEventArgs(true, dl != null ? dl.ActualPath : "?", ""));

      void entryDestinationfile_Completed(object sender, EventArgs e) => endFilenameEdit();

      void entryDestinationfile_TextChanged(object sender, TextChangedEventArgs e) =>
         buttonOK.IsEnabled = filenameIsValid(entryDestinationfile.Text);

      /// <summary>
      /// nur ein einfacher Test auf einen gültigen Dateinamen
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      bool filenameIsValid(string filename) {
         filename = filename.Trim();

         if (filename.Length == 0)
            return false;

         if (filename.IndexOfAny(invalidChars4Filename) >= 0)
            return false;

         return true;
      }

      void endFilenameEdit() {
         string purefilename = entryDestinationfile.Text.Trim();
         if (filenameIsValid(purefilename))
            ChooseFileReadyEvent?.Invoke(this, new ChoosePathAndFileEventArgs(true, dl != null ? dl.ActualPath : "?", purefilename));
      }

      /// <summary>
      /// Wenn das Element in ein ScrollView eingebettet ist, wird das ScrollView so verschoben, dass das Ende des Elementes sichtbar ist.
      /// </summary>
      /// <param name="el"></param>
      async static void srollToEnd(Element el) {
         Element parent = el.Parent;
         while (parent != null && parent is not ScrollView) {
            parent = parent.Parent;
         }
         if (parent != null &&
             parent is ScrollView view)
            await view.ScrollToAsync(el, ScrollToPosition.End, false);
      }

      /// <summary>
      /// wartet eine Bestätigung ab
      /// </summary>
      /// <param name="title"></param>
      /// <param name="msg"></param>
      /// <param name="cancel"></param>
      /// <returns></returns>
      async static Task MessageBox(ContentPage page, string title, string msg, string cancel = "weiter") =>
         await page.DisplayAlert(title, msg, cancel);

      void listViewDir_ItemSelected(object sender, SelectedItemChangedEventArgs e) {

      }

      async void TapGestureRecognizer_Up_Tapped(object sender, EventArgs e) => await setDirectory("..");

      async Task setDirectory(string path) {
         if (dl != null && !dl.Move(path))  // Wechsel nicht möglich (interner Fehler?)
            await showContent(OnlyExistingDirectory, listViewDir);
         else
            await showContent(OnlyExistingDirectory, listViewDir, dl);
      }
   }
}