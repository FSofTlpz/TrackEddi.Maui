using System.ComponentModel;
using static TrackEddi.SimplifyTrackPage;

namespace TrackEddi {
   public class SimplifyTrackPage_ListViewObjectItem : INotifyPropertyChanged {

      public event PropertyChangedEventHandler? PropertyChanged;

      public string Name { get; private set; }

      public SimplificationData SimplificationData { get; private set; }


      public SimplifyTrackPage_ListViewObjectItem(string name, SimplificationData sd) {
         Name = name;
         SimplificationData = sd;
      }

      /// <summary>
      /// zum Auslösen eines <see cref="PropertyChanged"/>-Events (auch "extern")
      /// </summary>
      /// <param name="propname"></param>
      public void Notify4PropChanged(string propname) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));

      public override string ToString() {
         return Name;
      }

   }

}
