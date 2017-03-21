using Android.Bluetooth;

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
    }
}