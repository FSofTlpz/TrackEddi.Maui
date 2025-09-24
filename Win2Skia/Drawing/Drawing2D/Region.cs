using SkiaSharp;

namespace System.Drawing.Drawing2D {
   public class Region : SKRegion {

      public SKRegion SKRegion => this; 

      public Region(GraphicsPath path) : base(path) { }

      public GraphicsPath GetRegionData() => new GraphicsPath(GetBoundaryPath());

      public void Intersect(RectangleF rect) =>
         Intersects(new SKRectI((int)rect.Left, 
                                (int)rect.Top, 
                                (int)rect.Right, 
                                (int)rect.Bottom));

      public void Exclude(RectangleF rect) =>
         Op(new SKRectI((int)rect.Left, 
                        (int)rect.Top, 
                        (int)rect.Right, 
                        (int)rect.Bottom), 
            SKRegionOperation.Difference);

      public void Exclude(GraphicsPath path) => Op(path, SKRegionOperation.Difference);

      public void Union(GraphicsPath path) => Op(path, SKRegionOperation.Union);

      public new bool IsEmpty(Graphics canvas) => base.IsEmpty;

   }
}