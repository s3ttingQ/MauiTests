using Android.App;
using Android.App.Job;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using Android;
using Android.Content;
using Android.Content.Res;
using RGPopup.Maui.Extensions;
using RGPopup.Maui.Services;
using AndroidX.Activity;
using static Microsoft.Maui.LifecycleEvents.AndroidLifecycle;

namespace MauiTestField
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Instance = this;

            RGPopup.Maui.Droid.Popup.Init(this);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((s, e) =>
            {
            });

            TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>((s, e) =>
            {
#if DEBUG
                throw e.Exception;
#else
                LogService.Instance.LogFatal("Unobserved Task exception: {0}", e.Exception.ToString());
#endif
            });

            AndroidEnvironment.UnhandledExceptionRaiser += new EventHandler<RaiseThrowableEventArgs>((s, e) =>
            {
            });

            Platform.Init(this, savedInstanceState);

            if (Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.R)
            {
                if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) != Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(Platform.CurrentActivity, [Manifest.Permission.BluetoothConnect], 102);
                }
                ActivityCompat.RequestPermissions(Platform.CurrentActivity, [Manifest.Permission.BluetoothScan], 101);
            }

            if (Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.R && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Bluetooth) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(Platform.CurrentActivity, [Manifest.Permission.Bluetooth], 102);
            }
        }
    }


}
