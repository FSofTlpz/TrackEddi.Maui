//#define WITH_DEBUG_INFO

using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FSofTUtils.OSInterface.Control {

   [XamlCompilation(XamlCompilationOptions.Compile)]

   public partial class WheelView : ContentView {

      /*
         Standard
         Wheel.PeekAreaInsets = Left=0, Top=0, Right=0, Bottom=0, HorizontalThickness=0, VerticalThickness=0

         Wheel.Height = 150
         itemHeight = 38,5

         virt. Item hat die gleiche Höhe wie gesamtes CarouselView (hc)
         virt. Items folgen direkt aufeinander
         sichtbares Item kann kleiner sein (Höhe hi) -> es gibt eine Lücke zwischen 2 sichtbaren Items (l=hc-hi)
         um diese Lücke zu verkleinern müssen die virt. Items teilweise "übereinander" (ov) geschoben werden (l=hc-hi-ov)
         z.B.
            111,5 (übereinandergeschobener Bereich)
               bei CarouselView-Höhe hc=150 bleiben 150-111,5=38,5 je Item
               ist jedes sichtbare Item genau hi=38,5 groß, bleibt keine Lücke mehr
            60
               hi=hc-l, 150-60=90 je Item
               Lücke 90-38,5=51,5
      
               150 (1 Item am oberen Rand, 1 in der Mitte 1 am unteren Rand)
                  38,5
                  17,25       150-38,5 +x=17,25 ov=111,5-17,25=94,25
                  38,5
                  17,25
                  38,5

       */

      const int DEFAULT_FONTSIZE = 25;

      const int DEFAULT_MAXVALUE = 59;

      #region Events

      public class ValueChangedEventArgs {
         public readonly int Value;

         public ValueChangedEventArgs(int v) {
            Value = v;
         }
      }

      /// <summary>
      /// der angezeigte Wert hat sich verändert
      /// </summary>
      public EventHandler<ValueChangedEventArgs>? ValueChangedEvent;

      #endregion

      #region Binding-Vars

      #region  Binding-Var BackColor

      public static readonly BindableProperty BackColorProperty = BindableProperty.Create(
         nameof(BackColor),
         typeof(Color),
         typeof(WheelView),
         Colors.LightSalmon);

      /// <summary>
      /// Hintergrundfarbe des Controls
      /// </summary>
      public Color BackColor {
         get => (Color)GetValue(BackColorProperty);
         set => SetValue(BackColorProperty, value);
      }

      #endregion

      #region  Binding-Var ItemTextColor

      public static readonly BindableProperty ItemTextColorProperty = BindableProperty.Create(
         nameof(ItemTextColor),
         typeof(Color),
         typeof(WheelView),
         Color.FromRgb(0, 0, 0));

      /// <summary>
      /// Textfarbe der Items
      /// </summary>
      public Color ItemTextColor {
         get => (Color)GetValue(ItemTextColorProperty);
         set => SetValue(ItemTextColorProperty, value);
      }

      #endregion

      #region  Binding-Var ItemColor

      public static readonly BindableProperty ItemColorProperty = BindableProperty.Create(
         nameof(ItemColor),
         typeof(Color),
         typeof(WheelView),
         Color.FromRgb(0x30, 0xA0, 0xFF));

      /// <summary>
      /// Hintergrundfarbe der Items
      /// </summary>
      public Color ItemColor {
         get => (Color)GetValue(ItemColorProperty);
         set => SetValue(ItemColorProperty, value);
      }

      #endregion

      #region  Binding-Var ItemColorCenter

      public static readonly BindableProperty ItemColorCenterProperty = BindableProperty.Create(
         nameof(ItemColorCenter),
         typeof(Color),
         typeof(WheelView),
         Color.FromRgb(0xA0, 0xE0, 0xFF));

      /// <summary>
      /// Hintergrundfarbe der Items
      /// </summary>
      public Color ItemColorCenter {
         get => (Color)GetValue(ItemColorCenterProperty);
         set => SetValue(ItemColorCenterProperty, value);
      }

      #endregion

      #region Binding-Var ItemFontSize

      public static readonly BindableProperty ItemFontSizeProperty = BindableProperty.Create(
         nameof(ItemFontSize),
         typeof(int),
         typeof(WheelView),
         DEFAULT_FONTSIZE);

      /// <summary>
      /// Font-Größe für die Items
      /// </summary>
      public int ItemFontSize {
         get => (int)GetValue(ItemFontSizeProperty);
         set => SetValue(ItemFontSizeProperty, value);
      }

      #endregion

      #region  Binding-Var MaxValue

      public static readonly BindableProperty MaxValueProperty = BindableProperty.Create(
         nameof(MaxValue),            // the name of the bindable property
         typeof(int),           // the bindable property type
         typeof(WheelView),     // the parent object type
         DEFAULT_MAXVALUE,                    // the default value for the property
         propertyChanged: OnMaxValueChanged); // Delegat, der ausgeführt wird, wenn der Wert geändert wurde

      /// <summary>
      /// auswählbarer Maximalwert
      /// </summary>
      public int MaxValue {
         get => (int)GetValue(MaxValueProperty);
         set => SetValue(MaxValueProperty, value);
      }

      static void OnMaxValueChanged(BindableObject bindable, object oldValue, object newValue) {
         if (bindable is WheelView) {
            WheelView wv = (WheelView)bindable;
            wv.setWheelRange(wv.Wheel, 0, (int)newValue);
         }
      }

      #endregion

      #region  Binding-Var Loop

      public static readonly BindableProperty LoopProperty = BindableProperty.Create(
         nameof(Loop),
         typeof(bool),
         typeof(WheelView),
         false);

      /// <summary>
      /// <see cref="CarouselView.Loop"/>
      /// </summary>
      public bool Loop {
         get => (bool)GetValue(LoopProperty);
         //set => SetValue(LoopProperty, value);
         set {
            Debug.WriteLine("::: WheelView.Loop A " + value);
            SetValue(LoopProperty, value);
            Debug.WriteLine("::: WheelView.Loop B " + value);
         }
      }

      #endregion

      #endregion

      /// <summary>
      /// Höhe eines Item (kleiner 0, wenn noch nicht bestimmt)
      /// </summary>
      double itemHeight = -1;

      /// <summary>
      /// Datenarray mit den auswählbaren int-Werten (für ItemsSource)
      /// </summary>
      ObservableCollection<int> dat = [];

      /// <summary>
      /// max. möglicher <see cref="Idx"/>
      /// </summary>
      public int MaxIdx => dat.Count - 1;


      long _idx = 0;

      int idx {
         get => (int)Interlocked.Read(ref _idx);
         set => Interlocked.Exchange(ref _idx, value);
      }

      public int Idx {
         get => idx;
         set {
            idx = value;
#if WITH_DEBUG_INFO
            showDebug("set Value=" + value);
#endif
            while (!setWheelPos(Wheel, idx, false))
               Thread.Sleep(100);
         }
      }


      public WheelView() {
         InitializeComponent();

         setWheelRange(Wheel, 0, DEFAULT_MAXVALUE, 0);

         Wheel.PropertyChanged += Wheel_PropertyChanged;
         Wheel.Loaded += Wheel_Loaded;
      }

      private void Wheel_Loaded(object? sender, EventArgs e) {
#if WITH_DEBUG_INFO
         showDebug("Wheel_Loaded() A");
#endif
         Wheel.PositionChanged += Wheel_PositionChanged;
#if WITH_DEBUG_INFO
         showDebug("Wheel_Loaded() B");
#endif
      }

      private void Wheel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
#if WITH_DEBUG_INFO
         string txt = "";
         if (e.PropertyName == nameof(Wheel.Loop)) {
            txt = Wheel.Loop.ToString();
         } else if (e.PropertyName == nameof(Wheel.IsVisible)) {
            txt = Wheel.IsVisible.ToString();
         } else if (e.PropertyName == nameof(Wheel.IsScrollAnimated)) {
            txt = Wheel.IsScrollAnimated.ToString();
         } else if (e.PropertyName == nameof(Wheel.ItemsSource)) {
            int i = 0;
            if (Wheel.ItemsSource != null)
               foreach (var item in Wheel.ItemsSource)
                  i++;
            txt = i.ToString();
         } else if (e.PropertyName == nameof(Wheel.Position)) {
            txt = Wheel.Position.ToString();
         } else if (e.PropertyName == nameof(Wheel.CurrentItem)) {
            txt = Wheel.CurrentItem?.ToString();
         } else if (e.PropertyName == nameof(Wheel.Width)) {
            txt = Wheel.Width.ToString();
         } else if (e.PropertyName == nameof(Wheel.Height)) {
            txt = Wheel.Height.ToString();

            //doAsyncAction(WheelAction.SetPeekAreaInsets, [itemHeight]);

            Wheel.SetPeekAreaInset(itemHeight);

         } else if (e.PropertyName == nameof(Wheel.PeekAreaInsets)) {
            txt = Wheel.PeekAreaInsets.Top.ToString();
         }
         showDebug("Wheel_PropertyChanged() " + e.PropertyName + ": " + txt);
#else
         if (e.PropertyName == nameof(Wheel.Height))
            Wheel.SetPeekAreaInset(Wheel.Height - itemHeight);
#endif

      }

      private void Wheel_PositionChanged(object? sender, PositionChangedEventArgs e) {
#if WITH_DEBUG_INFO
         showDebug("wheel_PositionChanged(): CurrentPosition=" + e.CurrentPosition);
#endif
         idx = e.CurrentPosition;
         OnValueChanged();
      }

      /// <summary>
      /// löst das <see cref="ValueChangedEvent"/> mit dem akt. <see cref="Idx"/> aus
      /// </summary>
      public virtual void OnValueChanged() =>
         ValueChangedEvent?.Invoke(this, new ValueChangedEventArgs(Idx));


      enum WheelAction {
         SetData,
         SetIdx,
         SetDataIdx,
         SetPeekAreaInsets,
      }


      object objActionDoor = new object();

      void setWheelRange(CarouselViewExt wheel, int min, int max, int newpos = -1) {
         lock (objActionDoor) {
#if WITH_DEBUG_INFO
            showDebug("setWheelRange() A: min=" + min + ", max=" + max + ", newpos=" + newpos);
#endif
            int pos = wheel.Position;

            int[] tmp = new int[max + 1 - min];
            for (int i = 0; i < tmp.Length; i++)
               tmp[i] = i;
            dat = new ObservableCollection<int>(tmp);
            wheel.ItemsSource = dat;

            pos = Math.Min(pos, MaxIdx);        // nach Möglichkeit alte Pos. wieder einnehmen
            if (newpos >= 0)                    // neue Pos. vorgegeben
               pos = Math.Min(newpos, MaxIdx);
            wheel.Position = pos;
         }
#if WITH_DEBUG_INFO
         showDebug("setWheelRange() B: min=" + min + ", max=" + max + ", newpos=" + newpos);
#endif
      }

      /// <summary>
      /// stellt auf eine bestimmte Pos. ein
      /// </summary>
      /// <param name="wheel"></param>
      /// <param name="pos"></param>
      /// <param name="animation"></param>
      /// <returns>true, wenn ok</returns>
      bool setWheelPos(CarouselViewExt wheel, int pos, bool animation) {
         lock (objActionDoor) {
#if WITH_DEBUG_INFO
            showDebug("setWheelPos() A: pos=" + pos + ", animation=" + animation);
#endif
            if (!animation) {
               wheel.Position = pos;
               //while (wheel.Position != pos)
               //   Thread.Sleep(100);

               wheel.Invalidate();
            } else {
               wheel.ScrollTo(pos, -1, ScrollToPosition.Center, true);
            }

#if WITH_DEBUG_INFO
            showDebug("setWheelPos() B: pos=" + pos + ", animation=" + animation);
#endif
         }
         return wheel.Position == pos;
      }

      /// <summary>
      /// registriert die Itemhöhe des Testitems und schaltet es unsichtbar
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void testItem_Loaded(object sender, EventArgs e) {
         Size sizeRequest = testItem.Measure(1000, 1000);
         testItem.IsVisible = false;
         if (itemHeight < 0)
            itemHeight = sizeRequest.Height;
      }


#if WITH_DEBUG_INFO
      void showDebug(string txt = "") {
         Debug.WriteLine(string.Format(">>> WheelView {0} (Pos {1}): Idx={2}, MaxIdx={3} VerticalThickness={4}, {5}",
                                       Id,
                                       Wheel.Position,
                                       Idx,
                                       MaxIdx,
                                       Wheel.PeekAreaInsets.VerticalThickness,
                                       txt));
      }

#endif

   }
}
