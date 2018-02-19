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
        public const string BroadcastAction = "BluetoothOnDrivingService_NewDetectedActivity";
        public const string KeyActivity = "Activity";

        private const string Tag = nameof(BluetoothOnDrivingService);

        private int minConfidence;

        public BluetoothOnDrivingService() : base("BluetoothOnDrivingService")
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();

            minConfidence = Resources.GetInteger(Resource.Integer.default_min_confidence);
            UpdateMinConfidence();
        }

        protected override void OnHandleIntent(Intent intent)
        {
            UpdateMinConfidence();
            if (ActivityRecognitionResult.HasResult(intent))
            {
                HandleActivityRecognitionResult(intent);
            }
        }

        private void UpdateMinConfidence()
        {
            var pref = GetSharedPreferences(GetString(Resource.String.shared_preferences_key), FileCreationMode.Private);
            pref.GetInt(GetString(Resource.String.preference_min_confidence), minConfidence);
        }

        private void HandleActivityRecognitionResult(Intent intent)
        {
            var result = ActivityRecognitionResult.ExtractResult(intent);

            DetectedActivity activity = result.MostProbableActivity;
            int confidence = activity.Confidence;

            if (IsConfident(confidence))
            {
                HandleDetectedActivity(activity);
            }
            else
            {
                Log.Verbose(Tag, $"Ignored detected activity ({activity.ToHumanText()}) because of low confidence ({confidence})");
            }
        }

        private void HandleDetectedActivity(DetectedActivity activity)
        {
            ChangeBluetooth(activity);
            BroadcastDetectedActivity(activity);
        }

        private bool IsConfident(int confidence)
        {
            return confidence >= minConfidence;
        }

        private void BroadcastDetectedActivity(DetectedActivity activitiy)
        {
            Intent broadcastIntent = new Intent(BroadcastAction);
            broadcastIntent.PutExtra(KeyActivity, activitiy);

            SendBroadcast(broadcastIntent);
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
                DisableBluetooth();
            }
        }

        private void EnableBluetooth()
        {
            var adapter = BluetoothHelper.ObtainBluetoothAdapter(this);
            if (!adapter.IsEnabled)
            {
                adapter.Enable();
            }
        }

        private void DisableBluetooth()
        {
            var adapter = BluetoothHelper.ObtainBluetoothAdapter(this);
            if (CanDisableBluetooth(adapter))
            {
                adapter.Disable();
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