namespace FSofTUtils.OSInterface {
   public class Helper {

      /// <summary>
      /// Wenn das Element in ein ScrollView eingebettet ist, wird das ScrollView so verschoben, dass das Ende des Elementes sichtbar ist.
      /// </summary>
      /// <param name="el"></param>
      async public static void SrollToEnd(Element el) {
         Element parent = el.Parent;
         while (parent != null && !(parent is ScrollView)) {
            parent = parent.Parent;
         }
         if (parent != null &&
             parent is ScrollView)
            await ((ScrollView)parent).ScrollToAsync(el, ScrollToPosition.End, false);
      }


      /// <summary>
      /// wartet eine Anwort ab (nur bei Auswahl von 'accept' wird true geliefert)
      /// </summary>
      /// <param name="title"></param>
      /// <param name="msg"></param>
      /// <param name="accept"></param>
      /// <param name="cancel"></param>
      /// <returns></returns>
      async public static Task<bool> MessageBox(Microsoft.Maui.Controls.Page page,
                                                string title,
                                                string msg,
                                                string accept,
                                                string cancel) => await page.DisplayAlert(title, msg, accept, cancel);

      /// <summary>
      /// wartet eine Bestätigung ab
      /// </summary>
      /// <param name="title"></param>
      /// <param name="msg"></param>
      /// <param name="cancel"></param>
      /// <returns></returns>
      async public static Task MessageBox(Microsoft.Maui.Controls.Page page,
                                          string title,
                                          string msg,
                                          string cancel = "weiter") => await page.DisplayAlert(title, msg, cancel);

      /// <summary>
      /// wartet eine Auswahl ab
      /// </summary>
      /// <param name="page"></param>
      /// <param name="title">Titel</param>
      /// <param name="buttons">Schaltflächen</param>
      /// <param name="cancel">Schaltfläche „Cancel“ (kann null sein)</param>
      /// <param name="destruction">button that represents destructive behavior (can be null)</param>
      /// <returns>liefert den Buttonindex oder -1 bei <paramref name="destruction"/> bzw. -2 bei <paramref name="cancel"/></returns>
      async public static Task<int> MessageBox(Microsoft.Maui.Controls.Page page,
                                               string title,
                                               string[] buttons,
                                               string? cancel,
                                               string? destruction = null) {
         string result = await page.DisplayActionSheet(title, cancel, destruction, buttons);
         for (int i = 0; i < buttons.Length; i++)
            if (buttons[i] == result)
               return i;
         if (destruction != null && destruction == result)
            return -1;
         return -2;
      }

      /// <summary>
      /// führt eine Action verzögert im UI-Thread aus
      /// </summary>
      /// <param name="ms"></param>
      /// <param name="action"></param>
      /// <returns></returns>
      public static async Task DelayedUIAction(int ms, Action action) =>
         await Task.Run(() => {
            Thread.Sleep(ms);
            MainThread.BeginInvokeOnMainThread(() => action());
         });

      /// <summary>
      /// zur vorhergehenden Seite zurück gehen
      /// </summary>
      /// <returns></returns>
      public static async Task<Microsoft.Maui.Controls.Page> GoBack() => await Shell.Current.Navigation.PopAsync();

      public static async Task GoTo(Microsoft.Maui.Controls.Page page) => await Shell.Current.Navigation.PushAsync(page);


      public static void PlaySound(string fullpath, float volume = 1F, bool looping = false) =>
         FSofTUtils.OSInterface.Sound.SoundHelper.PlayExclusiveNativeSound(fullpath, volume, looping);

      public static void ShowToastText(string txt) {
         if (MainThread.IsMainThread)
            Android.Widget.Toast.MakeText(Android.App.Application.Context, txt, Android.Widget.ToastLength.Long)?.Show();
         else
            MainThread.BeginInvokeOnMainThread(() =>
               Android.Widget.Toast.MakeText(Android.App.Application.Context, txt, Android.Widget.ToastLength.Long)?.Show()
            );
      }


      // await DisplayPromptAsync
      // bool answer = await DisplayAlert

   }
}
