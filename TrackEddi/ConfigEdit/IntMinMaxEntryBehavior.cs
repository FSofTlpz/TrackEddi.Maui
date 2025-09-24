namespace TrackEddi.ConfigEdit {
   public class IntMinMaxEntryBehavior : Behavior<Entry> {

      public int MaximumValue { get; set; } = int.MaxValue;

      public int MinimumValue { get; set; } = int.MinValue;

      public int StandardValue { get; set; } = int.MinValue;


      protected override void OnAttachedTo(Entry bindable) {
         base.OnAttachedTo(bindable);
         bindable.TextChanged += Entry_TextChanged;
         bindable.Unfocused += Entry_Unfocused;
      }

      protected override void OnDetachingFrom(Entry bindable) {
         base.OnDetachingFrom(bindable);
      }

      protected virtual void Entry_TextChanged(object? sender, TextChangedEventArgs e) {
         if (!string.IsNullOrEmpty(e.NewTextValue)) {
            int value;
            Entry? entry = (Entry?)sender;
            if (entry != null) {
               if (!int.TryParse(e.NewTextValue, out value) ||
                value.ToString() != e.NewTextValue) {
                  entry.Text = e.OldTextValue;
               } else {
                  if (value < MinimumValue)
                     entry.Text = MinimumValue.ToString();
                  else if (MaximumValue < value)
                     entry.Text = MaximumValue.ToString();
               }
            }
         }
      }

      private void Entry_Unfocused(object? sender, FocusEventArgs e) {
         if (!e.IsFocused) {
            Entry? entry = (Entry?)sender;
            if (entry != null &&
                entry.Text.Trim() == "")
               entry.Text = StandardValue.ToString();
         }
      }



   }
}
