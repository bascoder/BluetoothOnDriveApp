using Android.Annotation;
using Android.Bluetooth;
using Android.Content;
using Android.OS;

namespace AutoBluetooth.Helper
{
    static class BluetoothHelper
    {
        /// <summary>
        /// Guess if device has an active bluetooth connection. 
        /// It will return true if there was a connection in the near past.
        /// </summary>
        /// <param name="bluetoothAdapter"></param>
        /// <returns>True if there is, or has been in the near past, an active bluetooth connection</returns>
        public static bool GuessHasActiveConnection(BluetoothAdapter bluetoothAdapter)
        {
            var pairedDevices = bluetoothAdapter.BondedDevices;
            foreach (var device in pairedDevices)
            {
                if (device.BondState != Bond.None)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns bluetooth adapter for devices API level
        /// </summary>
        /// <param name="context">Application context</param>
        /// <returns>BluetoothAdapter</returns>
        public static BluetoothAdapter ObtainBluetoothAdapter(Context context)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr2)
            {
                return ObtainBluetoothAdapterService(context);
            }
            return ObtainBluetoothAdapterLegacy();
        }

        [TargetApi(Value = (int)BuildVersionCodes.JellyBeanMr2)]
        private static BluetoothAdapter ObtainBluetoothAdapterService(Context context)
        {
            var bluetoothManager = (BluetoothManager)context.GetSystemService(Context.BluetoothService);
            return bluetoothManager.Adapter;
        }

        [TargetApi(Value = (int)BuildVersionCodes.Eclair)]
        private static BluetoothAdapter ObtainBluetoothAdapterLegacy()
        {
            return BluetoothAdapter.DefaultAdapter;
        }
    }
}