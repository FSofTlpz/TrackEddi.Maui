using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrackEddi {
   public class WorkbenchContentPage_TabPageItem : INotifyPropertyChanged {

      string _Id = string.Empty;

      public string Id {
         get => _Id;
         set {
            if (_Id != value) {
               _Id = value;
               OnPropertyChanged(nameof(Id));
            }
         }
      }

      string _HeaderName = string.Empty;

      public string HeaderName {
         get => _HeaderName;
         set {
            if (_HeaderName != value) {
               _HeaderName = value;
               OnPropertyChanged(nameof(HeaderName));
            }
         }
      }

      /// <summary>
      /// wird ausgelöst wenn <see cref="Id"/> oder <see cref="HeaderName"/> geändert werden
      /// </summary>
      public event PropertyChangedEventHandler? PropertyChanged;

      protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

      public override string ToString() {
         return "[" + HeaderName + "] ID=" + Id;
      }
   }

}
