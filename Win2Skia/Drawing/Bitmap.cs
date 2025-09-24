using System.IO;
using SkiaSharp;
using SkiaWrapper;

namespace System.Drawing {
   public class Bitmap : SKBitmap {

      static SKBitmap fromFile(string filename) {
         SKBitmap bm;
         using (FileStream stream = new FileStream(filename, FileMode.Open)) {
            return bm = SKBitmap.Decode(stream);
         }
      }

      public Bitmap(int width, int height) : 
         base(width, 
              height,
              //SKColorType.Bgra8888,    // Bgra8888: Represents a 32-bit color with the format BGRA.
              SKColorType.Rgba8888,    // Rgba8888: Represents a 32-bit color with the format RGBA.
              SKAlphaType.Premul) {    // Premul: All pixels have their alpha premultiplied in their color components. This is the natural format for the rendering target pixels.
      }

      public Bitmap(Bitmap bm) : base(bm.Width, bm.Height) {
         bm.CopyTo(this);
      }

      public Bitmap(SKBitmap bm) : base(bm.Width, bm.Height) {
         bm.CopyTo(this);
      }

      /// <summary>
      /// Bitmap aus einem Stream erzeugen
      /// </summary>
      /// <param name="stream"></param>
      public Bitmap(Stream stream) : this(Decode(stream)) { }

      public static Bitmap FromStream(MemoryStream ms) => new Bitmap(ms);



      public Bitmap(string filename) : this(fromFile(filename)) { }

      /// <summary>
      /// Bitmap in einem Stream mit dem angeg. Format speichern
      /// </summary>
      /// <param name="memoryStream"></param>
      /// <param name="format"></param>
      public void Save(MemoryStream memoryStream, Imaging.ImageFormat format) {
         SKEncodedImageFormat sKEncodedImageFormat = SKEncodedImageFormat.Png;

         switch (format) {
            case Imaging.ImageFormat.Png:
               sKEncodedImageFormat = SKEncodedImageFormat.Png;
               break;

            case Imaging.ImageFormat.Bmp:
               sKEncodedImageFormat = SKEncodedImageFormat.Bmp;
               break;

         }
         Encode(memoryStream, sKEncodedImageFormat, 100);
      }

      public void Save(string filename, Imaging.ImageFormat format = Imaging.ImageFormat.Png) {
         using (MemoryStream mem = new MemoryStream()) {
            Save(mem, format);
            using (FileStream stream = new FileStream(filename, FileMode.Create)) {
               stream.Write(mem.ToArray(), 0, (int)mem.Length);
            }
         }
      }

      /// <summary>
      /// Legt die Farbe des angegebenen Pixels in dieser Bitmap fest.
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="color"></param>
      public void SetPixel(int x, int y, Color color) => SetPixel(x, y, Helper.ConvertColor(color));

      public new Color GetPixel(int x, int y) => Helper.ConvertColor(base.GetPixel(x, y));

      /// <summary>
      /// Erstellt eine Kopie des Bitmaps.
      /// </summary>
      /// <returns></returns>
      public Bitmap Clone() => new Bitmap(this);

   }
}
