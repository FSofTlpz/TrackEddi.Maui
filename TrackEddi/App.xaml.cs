namespace TrackEddi {
   public partial class App : Application {

      public enum AppEvent {
         OnStart,
         OnSleep,
         OnResume,
      }

      static public MainPage? MyMainPage; // wird im Konstruktor von MainPage gesetzt

      static public string LogFilename = string.Empty;

      static public string ErrorFilename = string.Empty;


      public App() {
         InitializeComponent();

         //#if ANDROID
         //         MainPage = new NavigationPage(new MainPage());  // mit AppShell fkt. TabbedPage NICHT (nur als Rootpage möglich)
         //#else
         //         MainPage = new AppShell();
         //#endif

         MainPage = new AppShell();
      }

      protected override Window CreateWindow(IActivationState? activationState) {
         //var window = base.CreateWindow(activationState);

         //var window = MyMainPage != null ? MyMainPage.Window : base.CreateWindow(activationState);

         Window window = base.CreateWindow(activationState);

#if WINDOWS
         // nur zum Test unter Windows
         if (DeviceInfo.Idiom == DeviceIdiom.Desktop) {
            window.Y = 5;
            window.Height = DeviceDisplay.MainDisplayInfo.Height * .7;
            window.Width = window.Height / 16.0 * 9.0;
         }
#endif
         /*
         The Window class defines the following cross-platform lifecycle events:
                              This event is raised ...
         Created 	            ... after the native window has been created.
                              At this point the cross-platform window will have a native window handler, but the window
                              might not be visible yet.
         window.Activated+=   ... when the window has been activated, and is, or will become, the focused window.
         window.Deactivated+= ... when the window is no longer the focused window. However, the window might still be visible.
         window.Stopped+=     ... when the window is no longer visible. There's no guarantee that an app will resume 
                              from this state, because it may be terminated by the operating system.
         window.Resumed+=     ... when an app resumes after being stopped. This event won't be raised the first time 
                              your app launches, and can only be raised if the Stopped event has previously been raised.
         window.Destroying+=  ... when the native window is being destroyed and deallocated. The same cross-platform 
                              window might be used against a new native window when the app is reopened.
         */


    //     if (window.Handler?.PlatformView is Android.App.Activity oldActivity &&
    ////oldActivity != activity &&
    //!oldActivity.IsDestroyed)
    //        throw new InvalidOperationException("hi");

         return window;
      }

      /// <summary>
      /// wird beim Test-Smartie mit Android 5.1.1 NIE AUFGERUFEN !?
      /// </summary>
      protected override void OnStart() {
         base.OnStart();
         MyMainPage?.AppEvent(AppEvent.OnStart);
      }

      protected override void OnSleep() {
         base.OnSleep();
         MyMainPage?.AppEvent(AppEvent.OnSleep);
      }

      protected override void OnResume() {
         base.OnResume();
         MyMainPage?.AppEvent(AppEvent.OnResume);
      }

      protected override void OnAppLinkRequestReceived(Uri uri) {
         MainThread.BeginInvokeOnMainThread(async () => {
            // Beim Start der App mit einem Intent ex. die MainPage i.A. noch nicht. In diesem Fall wird etwas gewartet.
            await Task.Run(() => {
               int i = 0;
               while (MyMainPage == null && ++i < 20)    // max. 20*500ms = 10s
                  Thread.Sleep(500);      // abwarten bis MyMainPage ex.
            });
            if (MyMainPage != null)
               await MyMainPage.ReceiveAppLink(uri);
         });
         base.OnAppLinkRequestReceived(uri);
      }
   }
}
