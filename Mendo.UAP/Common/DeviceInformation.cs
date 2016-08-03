using System;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.System.Profile;

namespace Mendo.UAP.Common
{
    public enum DeviceFamily
    {
        Unknown,
        Desktop,
        Mobile,
        Team,
        IoT,
        Xbox,
        HoloLens
    }

    public sealed class DeviceInformation
    {
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

        public DeviceFamily CurrentDeviceFamily
        {
            get
            {
                switch (AnalyticsInfo.VersionInfo.DeviceFamily)
                {
                    case "Windows.Desktop":
                        return DeviceFamily.Desktop;
                    case "Windows.Mobile":
                        return DeviceFamily.Mobile;
                    case "Windows.Team":
                        return DeviceFamily.Team;
                    case "Windows.IoT":
                        return DeviceFamily.IoT;
                    case "Windows.Xbox":
                        return DeviceFamily.Xbox;
                    case "Windows.HoloLens":
                        return DeviceFamily.HoloLens;
                    default:
                        return DeviceFamily.Unknown;
                }
            }
        }
    }
}
