using Android.App;

namespace FSofTUtils.OSInterface.Platforms.Android {
   public class SimpleDialogs {

      public static AlertDialog? GetBaseDialog(Activity? activity, string title, string txt) {
         if (activity != null) {
            AlertDialog? dlg = new AlertDialog.Builder(activity).Create();
            if (dlg != null) {
               dlg.SetTitle(title);
               dlg.SetMessage(txt);
               dlg.SetCancelable(false);
            }
            return dlg;
         }
         return null;
      }

      public static async Task ShowFinishDialog(Activity activity, string title, string txt) {
         await Task.Run(() => {
            ManualResetEvent mre = new ManualResetEvent(false);
            activity.RunOnUiThread(() => {
               AlertDialog? dlg = GetBaseDialog(activity, title, txt);
               if (dlg != null) {
                  dlg.SetButton("Bye bye ...", (c, ev) => {
                     activity.Finish();
                     mre.Set();
                  });
                  dlg.Show();
               }
            });
            mre.WaitOne();
         });
      }

      public static async Task<bool> ShowYesNoDialog(Activity activity, string title, string txt) {
         bool ok = false;
         await Task.Run(() => {
            ManualResetEvent mre = new ManualResetEvent(false);
            activity.RunOnUiThread(() => {
               AlertDialog? dlg = GetBaseDialog(activity, title, txt);
               if (dlg != null) {
                  dlg.SetButton("Ja", (c, ev) => {
                     ok = true;
                     mre.Set();
                  });
                  dlg.SetButton2("Nein", (c, ev) => {
                     ok = false;
                     mre.Set();
                  });
                  dlg.Show();
               }
            });
            mre.WaitOne();
         });
         return ok;
      }

      public static async Task ShowInfoDialog(Activity activity, string title, string txt) {
         await Task.Run(() => {
            ManualResetEvent mre = new ManualResetEvent(false);
            activity.RunOnUiThread(() => {
               AlertDialog? dlg = GetBaseDialog(activity, title, txt);
               if (dlg != null) {
                  dlg.SetButton("Weiter", (c, ev) => {
                     mre.Set();
                  });
                  dlg.Show();
               }
            });
            mre.WaitOne();
         });
      }

   }
}
