using Android.Gms.Location;

namespace AutoBluetooth
{
    public static class DetectedActivityExtension
    {
        /// <summary>
        /// Return <see cref="DetectedActivity.Type"/> as human readable string
        /// </summary>
        /// <param name="detectedActivity"></param>
        /// <returns>Human readable string</returns>
        public static string ToHumanText(this DetectedActivity detectedActivity)
        {
            switch (detectedActivity.Type)
            {
                case DetectedActivity.InVehicle:
                    return "in_vehicle";
                case DetectedActivity.OnBicycle:
                    return "on_bicycle";
                case DetectedActivity.OnFoot:
                    return "on_foot";
                case DetectedActivity.Still:
                    return "still";
                case DetectedActivity.Unknown:
                    return "unknown";
                case DetectedActivity.Tilting:
                    return "tilting";
                default:
                    return "unkown result";
            }
        }
    }
}