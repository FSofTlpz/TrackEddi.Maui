using SkiaSharp;
using SkiaWrapper;

namespace System.Drawing {
   public class SolidBrush : Brush {

      public Color Color => Helper.ConvertColor(SKPaintSolid != null ? SKPaintSolid.Color : SKColor.Empty);

      public SolidBrush(Color col) :
         base(col) { }

   }
}