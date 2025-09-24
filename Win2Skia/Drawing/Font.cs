using SkiaSharp;

namespace System.Drawing {

   public class Font : IDisposable {

      public SKTypeface? SKTypeface { get; protected set; }

      public string FontFamilyname => SKTypeface != null ? SKTypeface.FamilyName : string.Empty;




      /// <summary>
      /// Die Geviertgröße dieser Font in Punkt.
      /// </summary>
      public int SizeInPoints => (int)Math.Round(SizeInPointsF);

      /// <summary>
      /// Die Geviertgröße dieser Font in Punkt.
      /// </summary>
      public float SizeInPointsF { get; protected set; }

      public float Height => GetHeight();

      /// <summary>
      /// Eine FontStyle-Enumeration, die Informationen zum Schriftschnitt für diese Font enthält.
      /// </summary>
      public FontStyle Style => (Bold ? FontStyle.Bold : FontStyle.Regular) |
                                (Italic ? FontStyle.Italic : FontStyle.Regular);

      public bool Bold {
         get => SKTypeface != null && SKTypeface.IsBold;
         set {
            if (SKTypeface != null)
               setTypeface(SKTypeface.FamilyName, value, SKTypeface.IsItalic);
         }
      }

      public bool Italic {
         get => SKTypeface != null && SKTypeface.IsItalic;
         set {
            if (SKTypeface != null)
               setTypeface(SKTypeface.FamilyName, SKTypeface.IsBold, value);
         }
      }

      public float Size => SizeInPointsF;



      /// <summary>
      /// 
      /// </summary>
      /// <param name="fontfamily">Eine Zeichenfolgendarstellung der FontFamily für die neue Font.</param>
      /// <param name="emSize">Die Geviertgröße der neuen Schriftart in Punkt.</param>
      public Font(string fontfamily, float emSize) {
         setTypeface(fontfamily, false, false);
         SizeInPointsF = emSize;
      }

      public Font(string fontfamily, float emSize, FontStyle style, GraphicsUnit unit = GraphicsUnit.Pixel) {
         setTypeface(fontfamily, (style | FontStyle.Bold) != 0, (style | FontStyle.Italic) != 0);
         SizeInPointsF = emSize;
      }

      public Font(FontFamily fontfamily, float emSize, FontStyle style, GraphicsUnit unit = GraphicsUnit.Pixel) :
         this(fontfamily.Name, emSize, style, unit) { }

      public Font(SKTypeface typeface, float emSize) {
         SKTypeface = typeface;
         SizeInPointsF = emSize;
      }


      /// <summary>
      /// Gibt den Zeilenabstand dieser Schriftart in Pixel zurück.
      /// </summary>
      /// <returns></returns>
      public float GetHeight() {
         return new SKFont() {
            Typeface = SKTypeface,
            Size = SizeInPointsF,
         }.Spacing;

         //SKPaint sKPaint = new SKPaint() {
         //   Typeface = SKTypeface,
         //   TextSize = SizeInPointsF,
         //};
         //return sKPaint.FontSpacing; // recommend line spacing
      }

      void setTypeface(string fontfamily, bool bold = false, bool italic = false) {
         if (SKTypeface != null) {
            SKTypeface.Dispose();
            SKTypeface = null;
         }
         SKTypeface = getTypeface(fontfamily, bold, italic);
      }

      SKTypeface getTypeface(string fontfamily, bool bold = false, bool italic = false) {
         return SKTypeface.FromFamilyName(
                              fontfamily,
                              bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,    // Invisible, Thin, ... Normal ... Black
                              SKFontStyleWidth.Normal,                                     // UltraCondensed, ... Normal ... ExtraExpanded
                              italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

      }

      #region Implementierung der IDisposable-Schnittstelle

      /// <summary>
      /// true, wenn schon ein Dispose() erfolgte
      /// </summary>
      private bool _isdisposed = false;

      /// <summary>
      /// kann expliziet für das Objekt aufgerufen werden um interne Ressourcen frei zu geben
      /// </summary>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// überschreibt die Standard-Methode
      /// <para></para>
      /// </summary>
      /// <param name="notfromfinalizer">falls, wenn intern vom Finalizer aufgerufen</param>
      protected virtual void Dispose(bool notfromfinalizer) {
         if (!this._isdisposed) {            // bisher noch kein Dispose erfolgt
            if (notfromfinalizer) {          // nur dann alle managed Ressourcen freigeben
               SKTypeface?.Dispose();
               SKTypeface = null;
            }
            // jetzt immer alle unmanaged Ressourcen freigeben (z.B. Win32)

            _isdisposed = true;        // Kennung setzen, dass Dispose erfolgt ist
         }
      }

      #endregion

   }
}