using System;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;

namespace Mendo.UAP.Common
{
    public sealed class DeviceInformation// : Singleton<DeviceInformation>
    {
        public static bool IsPhone => ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1);
        public static bool HasPhoneHardwareButtons => ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");

        /// <summary>
        /// Returns the current scale factor for the display
        /// </summary>
        public static double ScaleFactor => (double)DisplayInformation.GetForCurrentView().ResolutionScale / 100;
        
        /// <summary>
        /// Returns the app's current memory usage in MB
        /// </summary>
        public double AppMemoryUsage => (double)Windows.System.MemoryManager.AppMemoryUsage / 1024d / 1024d;

        /// <summary>
        /// Converts Device-Independent resolution to the actual screen resolution active on the device
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public static double ConvertToActualResolution(double dimension) => Math.Round(ScaleFactor * dimension);

        public static double ConvertToScaledResolution(double dimension) => Math.Round((dimension / 4) * ScaleFactor);
    }
}
