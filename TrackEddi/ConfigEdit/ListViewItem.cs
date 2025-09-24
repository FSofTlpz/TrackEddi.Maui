using System.ComponentModel;

namespace TrackEddi.ConfigEdit {

   public class ListViewItem : INotifyPropertyChanged {

      public event PropertyChangedEventHandler? PropertyChanged;


      bool _isSelected = false;

      public bool IsSelected {
         get => _isSelected;
         set {
            if (_isSelected != value) {
               _isSelected = value;
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
         }
      }

      string _text = string.Empty;

      public string Text { 
         get=> _text;
         set {
            if (_text != value) {
               _text = value;
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
         }
      }


      public ListViewItem(string txt, bool seleted = false) {
         Text = txt;
         IsSelected = seleted;
      }

      public override string ToString() => Text + " (" + IsSelected + ")";

   }

}
