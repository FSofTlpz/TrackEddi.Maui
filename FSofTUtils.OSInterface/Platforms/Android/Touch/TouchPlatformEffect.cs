using Android.Views;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using static FSofTUtils.OSInterface.Touch.TouchEffect;

namespace FSofTUtils.OSInterface.Touch {

   public class TouchPlatformEffect : PlatformEffect /*RoutingEffect*/  {
      Android.Views.View? view;
      Element? formsElement;
      TouchEffect? touchEffect;
      bool capture;
      Func<double, double>? fromPixels;
      int[] twoIntArray = new int[2];

      static readonly Dictionary<Android.Views.View, TouchPlatformEffect> viewDictionary =
         new Dictionary<Android.Views.View, TouchPlatformEffect>();

      static readonly Dictionary<int, TouchPlatformEffect> idToEffectDictionary =
         new Dictionary<int, TouchPlatformEffect>();


      protected override void OnAttached() {
         // Get the Android View corresponding to the Element that the effect is attached to
         view = Control ?? Container;

         // Get access to the TouchEffect class in the .NET Standard library
         Effect? effect = Element.Effects.FirstOrDefault(e => e is TouchEffect);
         if (effect != null) {
            TouchEffect touchEffect1 = (TouchEffect)effect;
            if (touchEffect1 != null && view != null) {
               viewDictionary.Add(view, this);

               formsElement = Element;

               touchEffect = touchEffect1;

               // Save fromPixels function
               fromPixels = view.Context.FromPixels;

               // Set event handler on View
               view.Touch += onTouch;
            }
         }
      }

      protected override void OnDetached() {
         if (view != null && viewDictionary.ContainsKey(view)) {
            viewDictionary.Remove(view);
            view.Touch -= onTouch;
         }
      }

      void onTouch(object? sender, Android.Views.View.TouchEventArgs args) {
         if (sender != null && sender is Android.Views.View) {
            // Two object common to all the events
            Android.Views.View senderView = (Android.Views.View)sender;
            if (args.Event != null) {
               MotionEvent motionEvent = args.Event;

               // Get the pointer index
               int pointerIndex = motionEvent.ActionIndex;

               // Get the id that identifies a finger over the course of its progress
               int id = motionEvent.GetPointerId(pointerIndex);
               // For Android.Views.MotionEvent.ACTION_POINTER_DOWN or Android.Views.MotionEvent.ACTION_POINTER_UP
               // as returned by ActionMasked, this returns the associated pointer index.

               senderView.GetLocationOnScreen(twoIntArray);
               Point screenPointerCoords = new Point(twoIntArray[0] + motionEvent.GetX(pointerIndex),
                                                     twoIntArray[1] + motionEvent.GetY(pointerIndex));

               // Use ActionMasked here rather than Action to reduce the number of possibilities
               switch (args.Event.ActionMasked) {
                  case MotionEventActions.Down:
                  case MotionEventActions.PointerDown:
                     fireEvent(this, id, TouchActionEventArgs.TouchActionType.Pressed, screenPointerCoords, true);

                     idToEffectDictionary.Add(id, this);
                     //if (!idToEffectDictionary.ContainsKey(id))
                     //   idToEffectDictionary.Add(id, this);
                     //else
                     //   idToEffectDictionary[id] = this;
                     if (touchEffect != null)
                        capture = touchEffect.Capture;
                     break;

                  case MotionEventActions.Move:
                     // Multiple Move events are bundled, so handle them in a loop
                     for (pointerIndex = 0; pointerIndex < motionEvent.PointerCount; pointerIndex++) {
                        id = motionEvent.GetPointerId(pointerIndex);
                        screenPointerCoords = new Point(twoIntArray[0] + motionEvent.GetX(pointerIndex),
                                                        twoIntArray[1] + motionEvent.GetY(pointerIndex));

                        if (capture) {
                           senderView.GetLocationOnScreen(twoIntArray);
                           fireEvent(this, id, TouchActionEventArgs.TouchActionType.Moved, screenPointerCoords, true);
                        } else {
                           checkForBoundaryHop(id, screenPointerCoords);
                           if (idToEffectDictionary[id] != null)
                              fireEvent(idToEffectDictionary[id], id, TouchActionEventArgs.TouchActionType.Moved, screenPointerCoords, true);
                        }
                     }
                     break;

                  case MotionEventActions.Up:
                  case MotionEventActions.Pointer1Up:
                     if (capture) {
                        fireEvent(this, id, TouchActionEventArgs.TouchActionType.Released, screenPointerCoords, false);
                     } else {
                        checkForBoundaryHop(id, screenPointerCoords);
                        if (idToEffectDictionary[id] != null)
                           fireEvent(idToEffectDictionary[id], id, TouchActionEventArgs.TouchActionType.Released, screenPointerCoords, false);
                     }
                     idToEffectDictionary.Remove(id);
                     break;

                  case MotionEventActions.Cancel:
                     if (capture) {
                        fireEvent(this, id, TouchActionEventArgs.TouchActionType.Cancelled, screenPointerCoords, false);
                     } else {
                        if (idToEffectDictionary[id] != null)
                           fireEvent(idToEffectDictionary[id], id, TouchActionEventArgs.TouchActionType.Cancelled, screenPointerCoords, false);
                     }
                     idToEffectDictionary.Remove(id);
                     break;
               }

            }
         }
      }

      void checkForBoundaryHop(int id, Point pointerLocation) {
         TouchPlatformEffect? touchEffectHit = null;

         foreach (Android.Views.View view in viewDictionary.Keys) {
            try {
               // Get the view rectangle
               view.GetLocationOnScreen(twoIntArray);
            } catch { // System.ObjectDisposedException: Cannot access a disposed object.
               continue;
            }
            int x = twoIntArray[0];
            int y = twoIntArray[1];
            if (x <= pointerLocation.X && pointerLocation.X <= x + view.Width &&
                y <= pointerLocation.Y && pointerLocation.Y <= y + view.Height)
               touchEffectHit = viewDictionary[view];
         }

         if (touchEffectHit != idToEffectDictionary[id]) {
            if (idToEffectDictionary[id] != null)
               fireEvent(idToEffectDictionary[id], id, TouchActionEventArgs.TouchActionType.Exited, pointerLocation, true);
            if (touchEffectHit != null)
               fireEvent(touchEffectHit, id, TouchActionEventArgs.TouchActionType.Entered, pointerLocation, true);
            if (touchEffectHit != null)
               idToEffectDictionary[id] = touchEffectHit;
         }
      }

      void fireEvent(TouchPlatformEffect touchPlatformEffect,
                     int id,
                     TouchActionEventArgs.TouchActionType actionType,
                     Point pointerLocation,
                     bool isInContact) {
         if (touchPlatformEffect.touchEffect != null) {
            // Get the method to call for firing events
            Action<Element, TouchActionEventArgs> onTouchAction = touchPlatformEffect.touchEffect.OnTouchAction;

            // Get the location of the pointer within the view
            if (touchPlatformEffect.view != null) {
               touchPlatformEffect.view.GetLocationOnScreen(twoIntArray);
               double x = pointerLocation.X - twoIntArray[0];
               double y = pointerLocation.Y - twoIntArray[1];
               if (fromPixels != null) {
                  Point point = new Point(fromPixels(x), fromPixels(y));

                  // Call the method
                  if (touchPlatformEffect.formsElement != null)
                     onTouchAction(touchPlatformEffect.formsElement,
                                   new TouchActionEventArgs(id, actionType, point, isInContact));
               }
            }
         }
      }
   }

}
