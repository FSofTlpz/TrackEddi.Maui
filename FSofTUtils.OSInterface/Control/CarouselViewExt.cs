namespace FSofTUtils.OSInterface.Control {
   public class CarouselViewExt : CarouselView {

      public CarouselViewExt() : base() { }

      public void Invalidate() => InvalidateMeasure();

      public void SetPeekAreaInset(double insetTop) {
         PeekAreaInsets = new Thickness(0, insetTop, 0, 0);
         InvalidateMeasure();
      }

   }
}
