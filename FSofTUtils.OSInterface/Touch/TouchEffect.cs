namespace FSofTUtils.OSInterface.Touch {

   public class TouchEffect : RoutingEffect {

      public class TouchActionEventArgs : EventArgs {

         public enum TouchActionType {
            Entered,
            Pressed,
            Moved,
            Released,
            Exited,
            Cancelled
         }

         public TouchActionEventArgs(long id, TouchActionType type, Point location, bool isInContact) {
            Id = id;
            Type = type;
            Location = location;
            IsInContact = isInContact;
         }

         public long Id { private set; get; }

         public TouchActionType Type { private set; get; }

         public Point Location { private set; get; }

         public bool IsInContact { private set; get; }
      }

      public event EventHandler<TouchActionEventArgs>? TouchAction;


      //public TouchEffect() :
      //   base("XamarinDocs.TouchEffect") { }

      public bool Capture { set; get; }

      public void OnTouchAction(Element element, TouchActionEventArgs args) => TouchAction?.Invoke(element, args);
   }
}
