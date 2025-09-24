namespace TrackEddi {
   public class ShowGarminInfo4LonLat_ListViewObjectItem {

      public string Name { get; protected set; }

      public string TypeName { get; protected set; }

      public bool NameIsSet => !string.IsNullOrEmpty(Name);

      public ImageSource Picture { get; protected set; }

      /// <summary>
      /// akt. Bilddaten für <see cref="Picture"/>
      /// </summary>
      readonly byte[] pictdata = [];


      public ShowGarminInfo4LonLat_ListViewObjectItem(GarminImageCreator.SearchObject info) {
         ImageSource picture;
         if (info.Bitmap != null)
            pictdata = WinHelper.GetImageSource4WindowsBitmap(info.Bitmap, out picture);
         else
            picture = ImageSource.FromResource("Resources/Images/icon.png");
         Picture = picture;
         Name = info.Name;
         TypeName = info.TypeName;
      }


   }
}
