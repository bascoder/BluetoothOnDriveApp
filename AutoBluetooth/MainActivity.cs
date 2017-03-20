using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Hardware;

namespace AutoBluetooth
{
    [Activity(Label = "AutoBluetooth", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private CheckBox chkSensorSupported;
        private TextView tvDetectedActivityPlaceholder;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            Button btnStartService = FindViewById<Button>(Resource.Id.btnStartService);
            chkSensorSupported = FindViewById<CheckBox>(Resource.Id.chSensorSupported);
            tvDetectedActivityPlaceholder = FindViewById<TextView>(Resource.Id.tvDetectedActivityPlaceholder);

            btnStartService.Click += OnStartServiceClick;
            CheckSensorSupported();
        }

        private void OnStartServiceClick(object sender, EventArgs e)
        {

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
    }
}
