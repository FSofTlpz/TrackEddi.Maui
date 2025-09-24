using Android.App;
using Android.Content;
using Android.OS;

namespace TrackEddi {

   /* Der spezielle BroadcastReceiver wird beim Ein- und Ausschalten des Bildschirmss benachrichtigt.
    * 
    * Beim Ausschalten wird ein WakeLock angefordert, damit die App (Service) nicht deaktiviert wird.
    * Beim Einschalten wird der WakeLock wieder freigegeben.
    */

   //[BroadcastReceiver(Name = nameof(ScreenOnOffBroadcastReceiver), Label = nameof(ScreenOnOffBroadcastReceiver), Exported = true)]

   [BroadcastReceiver(Label = nameof(ScreenOnOffBroadcastReceiver), Exported = true)]
   [IntentFilter([Intent.ActionScreenOff, Intent.ActionScreenOn], Priority = (int)IntentFilterPriority.HighPriority)]
   public class ScreenOnOffBroadcastReceiver : BroadcastReceiver {
      private readonly Microsoft.Extensions.Logging.ILogger<ScreenOnOffBroadcastReceiver>? _logger;

      private PowerManager.WakeLock? _wakeLock;


      public ScreenOnOffBroadcastReceiver() {
         Microsoft.Maui.Controls.Application? app = Microsoft.Maui.Controls.Application.Current;
         if (app != null) {
            IMauiContext? ctx = app.Handler.MauiContext;
            if (ctx != null) {
               //_logger = Microsoft.Maui.Controls.Application.Current.Handler.MauiContext.Services.GetService<Microsoft.Extensions.Logging.ILogger<ScreenOffBroadcastReceiver>>();
               _logger = ctx.Services.GetService<Microsoft.Extensions.Logging.ILogger<ScreenOnOffBroadcastReceiver>>();
            }
         }
      }

      public override void OnReceive(Context? context, Intent? intent) {
         if (intent != null) {
            if (intent.Action == Intent.ActionScreenOff) {
               acquireWakeLock();
            } else if (intent.Action == Intent.ActionScreenOn) {
               relaseWakeLock();
            }
         }
      }

      public IntentFilter GetFilter4Receiver() {
         var filter = new IntentFilter();
         filter.AddAction(Intent.ActionScreenOff);
         filter.AddAction(Intent.ActionScreenOn);
         return filter;
      }

      void relaseWakeLock() {
         _wakeLock?.Release();
         _wakeLock?.Dispose();
         _wakeLock = null;
      }

      void acquireWakeLock() {
         relaseWakeLock();

         WakeLockFlags wakeFlags = WakeLockFlags.Partial;
         PowerManager? pm = (PowerManager?)Android.App.Application.Context.GetSystemService(Context.PowerService);
         _wakeLock = pm?.NewWakeLock(wakeFlags, typeof(ScreenOnOffBroadcastReceiver).FullName);
         _wakeLock?.Acquire();
      }

      protected override void Dispose(bool disposing) {
         base.Dispose(disposing);
         relaseWakeLock();
      }
   }

}
