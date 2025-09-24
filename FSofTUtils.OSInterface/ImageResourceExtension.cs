using System.Reflection;

namespace FSofTUtils.OSInterface {
   /// <summary>
   /// DO NOT FORGET THAT YOU NEED TO MAKE THE IMAGE FILES "Embedded Resource Files"
   /// <para>https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/images?tabs=windows</para>
   /// </summary>
   [ContentProperty(nameof(Source))]
   [AcceptEmptyServiceProvider]           // wenn OHNE "serviceProvider.GetService(typeof(T))", sonst [RequireService(typeof(T))]
   public class ImageResourceExtension : IMarkupExtension {
      public string? Source { get; set; }

      public object? ProvideValue(IServiceProvider serviceProvider) {
         if (Source == null) 
            return null;

         // Do your translation lookup here, using whatever method you require
         var imageSource = ImageSource.FromResource(Source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);

         return imageSource;
      }
   }

}
