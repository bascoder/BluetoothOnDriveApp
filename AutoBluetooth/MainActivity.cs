using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Hardware;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Content;
using Android.Gms.Location;
using Android.Util;
using Android.Bluetooth;
using AutoBluetooth.Helper;
using System.Linq;
using Android.Views;

namespace AutoBluetooth
{
    [Activity(Label = "AutoBluetooth", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        // update at about 10 seconds
        private const int DetectionInterval = 10000;
        private const string Tag = nameof(MainActivity);

        private CheckBox chkSensorSupported;
        private TextView tvDetectedActivityPlaceholder;
        private Spinner spSelectCar;

        private GoogleApiClient googleClient;

        private BroadcastReceiver detectedActivityReceiver;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            Button btnStartService = FindViewById<Button>(Resource.Id.btnStartService);
            chkSensorSupported = FindViewById<CheckBox>(Resource.Id.chSensorSupported);
            tvDetectedActivityPlaceholder = FindViewById<TextView>(Resource.Id.tvDetectedActivityPlaceholder);
            spSelectCar = FindViewById<Spinner>(Resource.Id.spSelectCar);

            InitCarSelector();
            btnStartService.Click += OnStartServiceClick;
            CheckSensorSupported();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (detectedActivityReceiver != null)
            {
                UnregisterReceiver(detectedActivityReceiver);
            }
        }

        private void InitCarSelector()
        {
            var pairedDevices = BluetoothHelper.ObtainBluetoothAdapter(this).BondedDevices.ToArray();
            var adapter = new BluetoothDeviceAdapter(this, pairedDevices);

            spSelectCar.Adapter = adapter;
        }

        private void OnStartServiceClick(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                btn.Enabled = false;
            };

            SetupBroadcastReceiver();
            SetupPlayServices();
        }

        private void SetupBroadcastReceiver()
        {
            detectedActivityReceiver = new DetectedActivityBroadcastReceiver(this);

            IntentFilter filter = new IntentFilter();
            filter.AddAction(BluetoothOnDrivingService.BroadcastAction);
            RegisterReceiver(detectedActivityReceiver, filter);
        }

        private void SetupPlayServices()
        {
            if (IsPlayServicesAvailable())
            {
                SetupActivityListener();
            }
            else
            {
                NotifyInstallPlayServices();
            }
        }

        private bool IsPlayServicesAvailable()
        {
            var availability = GoogleApiAvailability.Instance;
            return availability.IsGooglePlayServicesAvailable(this) == ConnectionResult.Success;
        }

        private void SetupActivityListener()
        {
            Action<Bundle> connectedCallback = (arg0) =>
            {
                if (googleClient != null)
                {
                    Intent intent = new Intent(this, typeof(BluetoothOnDrivingService));
                    var pIntent = PendingIntent.GetService(this, 0, intent, PendingIntentFlags.UpdateCurrent);
                    ActivityRecognition.ActivityRecognitionApi.RequestActivityUpdates(googleClient, DetectionInterval, pIntent);
                }
            };

            Action<int> connectionSuspendedCallback =
                (cause) => Log.Warn(Tag, "Connection suspended " + cause);

            Action<ConnectionResult> connectionFailedCallback = (r) =>
            {
                Log.Warn(Tag, "Connection failed " + r.ErrorMessage);
                Toast.MakeText(this, Resource.String.toast_play_api_connection_fail, ToastLength.Long)
                    .Show();
            };

            googleClient = new GoogleApiClient.Builder(this)
                .AddApi(ActivityRecognition.API)
                .AddConnectionCallbacks(connectedCallback, connectionSuspendedCallback)
                .AddOnConnectionFailedListener(connectionFailedCallback)
                .Build();
            googleClient.Connect();
        }

        private void NotifyInstallPlayServices()
        {
            new AlertDialog.Builder(this)
                .SetTitle(Resource.String.no_google_play)
                .SetMessage(Resource.String.msg_install_google_play)
                .Show();
        }

        private void CheckSensorSupported()
        {
            bool isSupported = IsMotionSensorSupported();
            chkSensorSupported.Checked = isSupported;

            WarnIfNotSupported(isSupported);
        }

        private void WarnIfNotSupported(bool isSupported)
        {
            if (!isSupported)
            {
                Toast.MakeText(this, Resource.String.motion_sensor_unsupported, ToastLength.Long)
                    .Show();
            }
        }

        private bool IsMotionSensorSupported()
        {
            var sensorManager = (SensorManager)GetSystemService(SensorService);
            var sensors = sensorManager.GetSensorList(SensorType.SignificantMotion);

            return sensors.Count > 0;
        }

        [BroadcastReceiver(Enabled = true)]
        private class DetectedActivityBroadcastReceiver : BroadcastReceiver
        {
            private WeakReference<MainActivity> parent;

            public DetectedActivityBroadcastReceiver()
            {

            }

            public DetectedActivityBroadcastReceiver(MainActivity parent)
            {
                this.parent = new WeakReference<MainActivity>(parent);
            }

            public override void OnReceive(Context context, Intent intent)
            {
                var activity = intent.GetParcelableExtra(BluetoothOnDrivingService.KeyActivity) as DetectedActivity;
                if (activity != null)
                {
                    parent.TryGetTarget(out MainActivity parentActivity);
                    if (parentActivity != null)
                    {
                        parentActivity.tvDetectedActivityPlaceholder.Text = activity.ToHumanText();
                    }
                }
            }
        }

        private class BluetoothDeviceAdapter : BaseAdapter
        {
            public override int Count => array?.Length ?? 0;

            private BluetoothDevice[] array;
            private WeakReference<Context> context;

            public BluetoothDeviceAdapter(Context context, BluetoothDevice[] initialArray)
            {
                this.context = new WeakReference<Context>(context);
                this.array = initialArray;
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return array[position];
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                this.context.TryGetTarget(out Context context);

                var textView = convertView as TextView;
                if (textView == null)
                {
                    if (context == null) return convertView;
                    textView = (TextView)LayoutInflater.From(context).Inflate(Resource.Layout.SpinnerView, parent, false);
                }

                var str = array[position].Name;
                textView.SetText(new Java.Lang.String(str), TextView.BufferType.Normal);

                return textView;
            }
        }
    }
}
