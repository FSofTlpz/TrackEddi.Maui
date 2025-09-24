using System.ComponentModel;

namespace FSofTUtils.OSInterface.Control {

   /// <summary>
   /// einfaches Control zum Festlegen einer Farbe (4 Slider)
   /// </summary>
   [DesignTimeVisible(true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class ChooseColor : ContentView {

      /// <summary>
      /// die Farbe wurde geändert
      /// </summary>
      public event EventHandler<EventArgs>? ColorChanged;

 
      #region  Binding-Var BorderSize

      public static readonly BindableProperty BorderSizeProperty = BindableProperty.Create(
          nameof(BorderSize),
          typeof(double),
          typeof(ChooseColor),
          5.0);

      public double BorderSize {
         get => (double)GetValue(BorderSizeProperty);
         set => SetValue(BorderSizeProperty, value);
      }

      #endregion

      #region  Binding-Var SliderMargin

      public static readonly BindableProperty SliderMarginProperty = BindableProperty.Create(
         nameof(SliderMargin),
         typeof(Thickness),
         typeof(ChooseColor),
         new Thickness(0, 0, 0, 0));

      public Thickness SliderMargin {
         get => (Thickness)GetValue(SliderMarginProperty);
         set => SetValue(SliderMarginProperty, value);
      }

      #endregion

      #region  Binding-Vars ColorComponentR, ColorComponentG, ColorComponentB, ColorComponentA

      public static readonly BindableProperty ColorComponentRProperty = BindableProperty.Create(
          nameof(ColorComponentR),
          typeof(float),
          typeof(ChooseColor),
          0F,
          propertyChanged: onChangeColorComponent);

      public float ColorComponentR {
         get => (float)GetValue(ColorComponentRProperty);
         set {
            if (ColorComponentR != value)
               SetValue(ColorComponentRProperty, value);
         }
      }

      public static readonly BindableProperty ColorComponentGProperty = BindableProperty.Create(
          nameof(ColorComponentG),
          typeof(float),
          typeof(ChooseColor),
          0F,
          propertyChanged: onChangeColorComponent);

      public float ColorComponentG {
         get => (float)GetValue(ColorComponentGProperty);
         set {
            if (ColorComponentG != value)
               SetValue(ColorComponentGProperty, value);
         }
      }

      public static readonly BindableProperty ColorComponentBProperty = BindableProperty.Create(
          nameof(ColorComponentB),
          typeof(float),
          typeof(ChooseColor),
          0F,
          propertyChanged: onChangeColorComponent);

      public float ColorComponentB {
         get => (float)GetValue(ColorComponentBProperty);
         set {
            if (ColorComponentB != value)
               SetValue(ColorComponentBProperty, value);
         }
      }

      public static readonly BindableProperty ColorComponentAProperty = BindableProperty.Create(
          nameof(ColorComponentA),
          typeof(float),
          typeof(ChooseColor),
          1F,
          propertyChanged: onChangeColorComponent);

      public float ColorComponentA {
         get => (float)GetValue(ColorComponentAProperty);
         set {
            if (ColorComponentA != value)
               SetValue(ColorComponentAProperty, value);
         }
      }

      static void onChangeColorComponent(BindableObject bindable, object oldValue, object newValue) {
         var control = bindable as ChooseColor;
         if (control != null &&
             (float)oldValue != (float)newValue)
            control.colorComponentChanged();
      }

      #endregion

      #region  Binding-Var Color

      public static readonly BindableProperty ColorProperty = BindableProperty.Create(
           nameof(Color),
           typeof(Color),
           typeof(ChooseColor),
           Colors.Red,
           propertyChanged: onChangeColor);

      bool setColorComponentIntern = false;

      public Color Color {
         get => (Color)GetValue(ColorProperty);
         set {
            setColorComponentIntern = true;
            ColorComponentR = value.Red;
            ColorComponentG = value.Green;
            ColorComponentB = value.Blue;
            ColorComponentA = value.Alpha;
            setColorComponentIntern = false;

            BackgroundColor = Color;

            if (Color != value) {
               SetValue(ColorProperty, value);
               OnColorChanged(new EventArgs());
            }
         }
      }

      private static void onChangeColor(BindableObject bindable, object oldValue, object newValue) {
         var control = bindable as ChooseColor;
         if (control != null &&
             (Color)oldValue != (Color)newValue)
            control.Color = (Color)newValue;
      }

      #endregion


      public ChooseColor() {
         InitializeComponent();
      }

      /// <summary>
      /// eine einzelne Farbkomponente wurde verändert
      /// <para>(wird nur berücksichtigt, wenn es von einem Slider kam)</para>
      /// </summary>
      void colorComponentChanged() {
         if (!setColorComponentIntern)
            BackgroundColor = Color = new Color(ColorComponentR, ColorComponentG, ColorComponentB, ColorComponentA);
      }

      protected virtual void OnColorChanged(EventArgs e) {
         ColorChanged?.Invoke(this, e);
      }

   }
}