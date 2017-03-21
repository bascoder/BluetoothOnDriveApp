using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Gms.Location;
using Android.Util;
using AutoBluetooth.Helper;

namespace AutoBluetooth
{
    [Service]
    public class BluetoothOnDrivingService : IntentService
    {
        private const int MinConfidence = 50;
        private const string Tag = nameof(BluetoothOnDrivingService);

        public BluetoothOnDrivingService() : base("BluetoothOnDrivingService")
        {

        }

        protected override void OnHandleIntent(Intent intent)
        {
            if (ActivityRecognitionResult.HasResult(intent))
            {
                HandleActivityRecognitionResult(intent);
            }
        }

        private void HandleActivityRecognitionResult(Intent intent)
        {
            var result = ActivityRecognitionResult.ExtractResult(intent);

            DetectedActivity activity = result.MostProbableActivity;
            int confidence = activity.Confidence;

            if (IsConfident(confidence))
            {
                ChangeBluetooth(activity);
            }
            else
            {
                Log.Verbose(Tag, $"Ignored detected activity ({activity.ToHumanText()}) because of low confidence ({confidence})");
            }
        }

        private static bool IsConfident(int confidence)
        {
            return confidence >= MinConfidence;
        }

        private void ChangeBluetooth(DetectedActivity activity)
        {
            if (activity.Type == DetectedActivity.InVehicle)
            {
                Log.Info(Tag, $"Detected ({activity.ToHumanText()}) with confidence ({activity.Confidence}), so enabling bluetooth");
                EnableBluetooth();
            }
            else
            {
                Log.Debug(Tag, "Not in vehicle but " + activity.ToHumanText());
                DisableBluetooth();
            }
        }

        private void EnableBluetooth()
        {
            var adapter = BluetoothAdapter.DefaultAdapter;
            if (!adapter.IsEnabled)
            {
                adapter.Enable();
                Log.Debug(Tag, "Enabled bluetooth");
            }
        }

        private void DisableBluetooth()
        {
            var adapter = BluetoothAdapter.DefaultAdapter;
            if (CanDisableBluetooth(adapter))
            {
                adapter.Disable();
                Log.Debug(Tag, "Disabled bluetooth");
            }
        }

        private static bool CanDisableBluetooth(BluetoothAdapter adapter)
        {
            return adapter.IsEnabled &&
                // only disconnect if bluetooth is not in use
                !BluetoothHelper.GuessHasActiveConnection(adapter);
        }
    }
}