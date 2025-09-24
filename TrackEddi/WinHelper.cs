namespace TrackEddi {
   internal static class WinHelper {

      /// <summary>
      /// erzeugt zum Bitmap eine <see cref="ImageSource"/> und liefert die dazu weiterhin (!)
      /// benötigten Bilddaten
      /// </summary>
      /// <param name="bm"></param>
      /// <param name="ims"></param>
      /// <returns></returns>
      public static byte[] GetImageSource4WindowsBitmap(System.Drawing.Bitmap bm, out ImageSource ims) {
         MemoryStream mem = new MemoryStream();
         bm.Save(mem, System.Drawing.Imaging.ImageFormat.Png);
         mem.Position = 0;
         byte[] pictdata = mem.ToArray();
         mem.Dispose();

         ims = ImageSource.FromStream(() => {
            return new MemoryStream(pictdata);  // MS: "The delegate provided to must return a new stream on every invocation."
         });

         return pictdata;
      }

      public static Microsoft.Maui.Graphics.Color ConvertColor(System.Drawing.Color col) => new Color(col.R, col.G, col.B, col.A);

      public static System.Drawing.Color ConvertColor(Microsoft.Maui.Graphics.Color col)
         => System.Drawing.Color.FromArgb((byte)Math.Round(col.Alpha * 255),
                                          (byte)Math.Round(col.Red * 255),
                                          (byte)Math.Round(col.Green * 255),
                                          (byte)Math.Round(col.Blue * 255));


   }
}
