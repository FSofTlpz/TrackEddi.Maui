using System.ComponentModel;

namespace TrackEddi {
   internal class GPXSearchPage_ListViewObjectItem : INotifyPropertyChanged {

      public event PropertyChangedEventHandler? PropertyChanged;


      bool _isMarked = false;

      public bool IsMarked {
         get => _isMarked;
         set {
            if (_isMarked != value) {
               _isMarked = value;
               Notify4PropChanged(nameof(IsMarked));
            }
         }
      }

      string _fullfilename = string.Empty;


      public string Filename => Path.GetFileName(_fullfilename);

      public string FullFilename => _fullfilename;

      public string Filepath => _fullfilename.Substring(0, _fullfilename.Length - Filename.Length);


      public GPXSearchPage_ListViewObjectItem(string fullfilename) {
         _fullfilename = fullfilename;
         IsMarked = false;
      }


      /// <summary>
      /// zum Auslösen eines <see cref="PropertyChanged"/>-Events (auch "extern" möglich!)
      /// </summary>
      /// <param name="propname"></param>
      public void Notify4PropChanged(string propname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));

   }
}
