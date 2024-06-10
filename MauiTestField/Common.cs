using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiTestField
{
    public static class Common
    {
        public static async Task RunAsyncOnMainThreadAsync(Func<Task> action)
        {
            if (MainThread.IsMainThread)
            {
                await action.Invoke().ConfigureAwait(false);
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(async () => { await action.Invoke().ConfigureAwait(false); })
                    .ConfigureAwait(false);
            }
        }

        internal static async Task<bool> IsLocationServiceEnabled()
        {
#if ANDROID
            var lm = Platform.CurrentActivity.GetSystemService(Android.Content.Context.LocationService) as Android.Locations.LocationManager;

            if (lm.IsProviderEnabled(Android.Locations.LocationManager.GpsProvider) == false)
            {
                if (await App.Current.MainPage.DisplayAlert("Location off", $"Location is turned off.{Environment.NewLine}{Environment.NewLine}It is needed for bluetooth discovery.", "Enable", "Cancel"))
                {
                    Platform.CurrentActivity.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings));

                    return false;
                }
                else
                {
                    return false;
                }
            }
#endif

            return true;
        }
    }
}
