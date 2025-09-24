//#define WITH_DEBUG_INFO

using System.Diagnostics;

namespace FSofTUtils.OSInterface.Control {

   [XamlCompilation(XamlCompilationOptions.Compile)]

   public partial class TimeWheelView : ContentView {

      const int DEFAULT_CONTROLHEIGHT = 150;
      const int DEFAULT_CONTROLWIDTH = 300;
      const int DEFAULT_FONTSIZE = 25;

      #region Events

      public class TimeChangedEventArgs {
         public readonly TimeSpan NewTimeSpan;

         public TimeChangedEventArgs(TimeSpan ts) {
            NewTimeSpan = ts;
         }
      }

      /// <summary>
      /// die angezeigte Zeit hat sich verändert
      /// </summary>
      public event EventHandler<TimeChangedEventArgs>? TimeChangedEvent;


      public class TimeSettableStatusChangedEventArgs {
         public readonly bool IsSettable;

         public TimeSettableStatusChangedEventArgs(bool issettable) {
            IsSettable = issettable;
         }
      }

      /// <summary>
      /// Kann die Zeit gesetzt werden?
      /// </summary>
      public event EventHandler<TimeSettableStatusChangedEventArgs>? TimeSettableStatusChangedEvent;

      #endregion

      #region Binding-Vars

      #region  Binding-Var BackColor

      public static readonly BindableProperty BackColorProperty = BindableProperty.Create(
         nameof(BackColor),
         typeof(Color),
         typeof(TimeWheelView),
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
         typeof(TimeWheelView),
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
         typeof(TimeWheelView),
         Color.FromRgb(0xA0, 0xE0, 0xFF));

      /// <summary>
      /// Hintergrundfarbe der Items
      /// </summary>
      public Color ItemColorCenter {
         get => (Color)GetValue(ItemColorCenterProperty);
         set => SetValue(ItemColorCenterProperty, value);
      }

      #endregion

      #region Binding-Var ControlWidth

      public static readonly BindableProperty ControlWidthProperty = BindableProperty.Create(
         nameof(ControlWidth),
         typeof(int),
         typeof(TimeWheelView),
         DEFAULT_CONTROLWIDTH);

      /// <summary>
      /// Gesamtbreite des Controls
      /// </summary>
      public int ControlWidth {
         get => (int)GetValue(ControlWidthProperty);
         set => SetValue(ControlWidthProperty, value);
      }

      #endregion

      #region Binding-Var ControlHeight

      public static readonly BindableProperty ControlHeightProperty = BindableProperty.Create(
         nameof(ControlHeight),
         typeof(int),
         typeof(TimeWheelView),
         DEFAULT_CONTROLHEIGHT,
         propertyChanged: OnControlHeightChanged);

      /// <summary>
      /// Höhe des Controls
      /// </summary>
      public int ControlHeight {
         get => (int)GetValue(ControlHeightProperty);
         set => SetValue(ControlHeightProperty, value);
      }

      static void OnControlHeightChanged(BindableObject bindable, object oldValue, object newValue) {
         if (bindable is TimeWheelView) {
            //TimeWheelView twv = bindable as TimeWheelView;
            //twv.changePeekAreaInsets((int)newValue - (int)oldValue);
         }
      }

      #endregion

      #region Binding-Var ItemFontSize

      public static readonly BindableProperty ItemFontSizeProperty = BindableProperty.Create(
         nameof(ItemFontSize),
         typeof(int),
         typeof(TimeWheelView),
         DEFAULT_FONTSIZE,
         propertyChanged: OnItemFontSizeChanged);

      /// <summary>
      /// Font-Größe für die Items
      /// </summary>
      public int ItemFontSize {
         get => (int)GetValue(ItemFontSizeProperty);
         set => SetValue(ItemFontSizeProperty, value);
      }

      static void OnItemFontSizeChanged(BindableObject bindable, object oldValue, object newValue) {
         if (bindable is TimeWheelView) {
            TimeWheelView twv = (TimeWheelView)bindable;
            twv.WheelViewHour.ItemFontSize = (int)newValue;
            //twv.WheelViewMinute.ItemFontSize = (int)newValue;
            //twv.WheelViewSecond.ItemFontSize = (int)newValue;
         }
      }

      #endregion

      #region  Binding-Var WheelLoop

      public static readonly BindableProperty WheelLoopProperty = BindableProperty.Create(
         nameof(WheelLoop),
         typeof(bool),
         typeof(TimeWheelView),
         false);

      /// <summary>
      /// <see cref="WheelView.Loop"/>
      /// </summary>
      public bool WheelLoop {
         get => (bool)GetValue(WheelLoopProperty);
         //set => SetValue(LoopProperty, value);
         set {
            Debug.WriteLine("::: TimeWheelView.Loop A " + value);
            SetValue(WheelLoopProperty, value);
            Debug.WriteLine("::: TimeWheelView.Loop B " + value);
         }
      }

      #endregion

      #endregion

      /// <summary>
      /// akt. Zeit
      /// </summary>
      public TimeSpan TimeSpan {
         get {
            return new TimeSpan(WheelViewHour.Idx,
                                WheelViewMinute.Idx,
                                WheelViewSecond.Idx);
         }
         set {
            WheelViewHour.Idx = value.Hours;
            WheelViewMinute.Idx = value.Minutes;
            WheelViewSecond.Idx = value.Seconds;
         }
      }


      public TimeWheelView() {
         InitializeComponent();
         
         WheelViewHour.ValueChangedEvent += WheelView_ValueChangedEvent;
         WheelViewMinute.ValueChangedEvent += WheelView_ValueChangedEvent;
         WheelViewSecond.ValueChangedEvent += WheelView_ValueChangedEvent;
      }

      private void WheelView_ValueChangedEvent(object? sender, WheelView.ValueChangedEventArgs args) =>
         OnTimeChanged();

      public virtual void OnTimeChanged() {
         TimeChangedEvent?.Invoke(this, new TimeChangedEventArgs(TimeSpan));
      }

   }
}
