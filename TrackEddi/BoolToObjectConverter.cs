using System.Globalization;

namespace TrackEddi {
   public class BoolToObjectConverter<T> : IValueConverter {
      public T? TrueObject { get; set; }
      public T? FalseObject { get; set; }

      public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
         value != null && (bool)value ? TrueObject : FalseObject;


      public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
         value != null && ((T)value).Equals(TrueObject);
   }
}
