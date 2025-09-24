using FSofTUtils.Geography.Garmin;

namespace TrackEddi {
   public class SymbolObjectItem {

      public string Name { get; protected set; }

      public string Group { get; protected set; }

      public ImageSource Picture { get; protected set; }

      /// <summary>
      /// akt. Bilddaten für <see cref="Picture"/>
      /// </summary>
      byte[] pictdata;

      public GarminSymbol GarminSymbol { get; protected set; }

      public SymbolObjectItem(GarminSymbol symbol) {
         pictdata = WinHelper.GetImageSource4WindowsBitmap(symbol.Bitmap, out ImageSource picture);

         Picture = picture;
         Name = symbol.Name;
         Group = symbol.Group;
         GarminSymbol = symbol;
      }

      public override string ToString() {
         return Group + ": " + Name;
      }

   }
}
