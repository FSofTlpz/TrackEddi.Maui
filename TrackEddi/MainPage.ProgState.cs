namespace TrackEddi {
   public partial class MainPage : ContentPage {

      public class ProgState {

         public enum State {
            Unknown,

            /// <summary>
            /// Im Programm können keine Daten verändert werden. 
            /// </summary>
            Viewer,

            /// <summary>
            /// Im Programm können Marker gesetzt/verschoben werden. 
            /// </summary>
            Edit_Marker,

            /// <summary>
            /// Im Programm können Tracks gezeichnet werden. 
            /// </summary>
            Edit_TrackDraw,

            /// <summary>
            /// Im Programm können Punkte aus einem Tracks gelöscht werden. 
            /// </summary>
            Edit_TrackPointremove,

            ///// <summary>
            ///// Im Programm können Tracks getrennt werden.
            ///// </summary>
            Edit_TrackSplit,

            ///// <summary>
            ///// Im Programm können Tracks verbunden werden.
            ///// </summary>
            Edit_TrackConcat,

         };

         State _programState = State.Unknown;

         /// <summary>
         /// akt. Programm-Status
         /// </summary>
         public State ProgramState {
            get => _programState;
            set {
               if (_programState != value) {
                  map.M_Refresh(false, false, false, false);
                  _programState = value;
               }
            }
         }

         SpecialMapCtrl.SpecialMapCtrl map;


         public ProgState(SpecialMapCtrl.SpecialMapCtrl map) {
            this.map = map;
         }

      }

   }
}
