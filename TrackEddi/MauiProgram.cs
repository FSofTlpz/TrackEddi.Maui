using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;           // for ConfigureLifecycleEvents()
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Runtime.CompilerServices;

namespace TrackEddi {
   public static class MauiProgram {

      public static MauiApp CreateMauiApp() {
         var builder = MauiApp.CreateBuilder();
         builder
            .UseMauiApp<App>()

            .ConfigureFonts(fonts => {
               fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
               fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })

            /* for using SKIA */
            .UseSkiaSharp()

            /* for using my TouchEffect */
            .ConfigureEffects(effects => {
               effects.Add<FSofTUtils.OSInterface.Touch.TouchEffect, FSofTUtils.OSInterface.Touch.TouchPlatformEffect>();
            }
            )

            .ConfigureLifecycleEvents(lifecycle => {
#if ANDROID
               lifecycle.AddAndroid(android => {
                  android.OnCreate((activity, bundle) => HandleAndroidAppIntent(activity.Intent));
               });
               lifecycle.AddAndroid(android => {
                  android.OnNewIntent((activity, intent) => HandleAndroidAppIntent(intent));
               });
#endif
            })

            ;

#if DEBUG
         builder.Logging.AddDebug();
#endif

         return builder.Build();
      }

      static void HandleAndroidAppIntent(Android.Content.Intent? intent) {
         if (intent != null) {
            var action = intent?.Action;
            string? uritxt = intent?.Data?.ToString();

            if (action == Android.Content.Intent.ActionView &&
                uritxt is not null)
               Task.Run(() => {
                  if (Uri.TryCreate(uritxt, UriKind.RelativeOrAbsolute, out var uri)) 
                     App.Current?.SendOnAppLinkRequestReceived(uri);
               });
         }
      }

   }
}
