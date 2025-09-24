﻿namespace TrackEddi {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class ColorChoosingPage : ContentPage {

      /// <summary>
      /// Dialog mit Auswahl beendet
      /// </summary>
      public event EventHandler<EventArgs>? EndWithOk;


      public static readonly BindableProperty ActualColorProperty = BindableProperty.Create(
        nameof(ActualColor),
        typeof(Color),
        typeof(ColorChoosingPage),
        Colors.Green,
        propertyChanged: colorchanged);

      public Color ActualColor {
         get => (Color)GetValue(ActualColorProperty);
         set {
            if (ActualColor != value)
               SetValue(ActualColorProperty, value);
         }
      }

      private static void colorchanged(BindableObject bindable, object oldValue, object newValue) {
         var control = bindable as ColorChoosingPage;
         if (control != null) {
            control.ActualColor = (Color)newValue;
         }
      }

      public bool Ok { get; protected set; } = false;


      public ColorChoosingPage() {
         InitializeComponent();
      }

      protected override void OnAppearing() {
         base.OnAppearing();
         ChooseColor_ColorChanged(null, EventArgs.Empty);
      }

      private void ChooseColor_ColorChanged(object? sender, EventArgs e) {
      }

      async private void Button_Clicked(object sender, EventArgs e) {
         Ok = true;
         EndWithOk?.Invoke(this, EventArgs.Empty);
         await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
      }

      async private void GarminButton_Clicked(object sender, EventArgs e) {
         ActualColor = ((Button)sender).BackgroundColor;
         EndWithOk?.Invoke(this, EventArgs.Empty);
         await FSofTUtils.OSInterface.Helper.GoBack();     // diese Seite sofort schließen
      }
   }
}