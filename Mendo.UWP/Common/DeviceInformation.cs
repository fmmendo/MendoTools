using System;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Xaml;

namespace Mendo.UWP.Common
{
    public enum DeviceFamily
    {
        Unknown = 1,
        Desktop = 2,
        Mobile = 4,
        Team = 8,
        IoT = 16,
        Xbox = 32,
        HoloLens = 64
    }

    public sealed class DeviceInformation : SingletonViewModelBase<DeviceInformation>
    {
        public bool IsPhone { get; }
        public bool IsXbox { get; }
        public bool HasPhoneHardwareButtons { get; }

        /// <summary>
        /// Returns the current scale factor for the display
        /// </summary>
        public static double ScaleFactor => (double)DisplayInformation.GetForCurrentView().ResolutionScale / 100;

        public bool SupportsSDK10586 { get; }
        public bool SupportsSDK14393 { get; }

        public string SystemFamily { get; }
        public string SystemVersion { get; }
        public string SystemArchitecture { get; }
        public string ApplicationName { get; }
        public string ApplicationVersion { get; }
        public string DeviceManufacturer { get; }
        public string DeviceModel { get; }
        public Guid DeviceId { get;  }

        public ProcessorArchitecture ProcessorArchitecture => Package.Current.Id.Architecture;

        public ResolutionScale ResolutionScale { get; private set; } = ResolutionScale.Scale100Percent;

        public double AppMemoryUsage => (double)MemoryManager.AppMemoryUsage / 1024d / 1024d;

        public DeviceInformation()
        {
            IsPhone = ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1);
            HasPhoneHardwareButtons = ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");

            IsXbox = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.ToLower().Contains("windows.xbox");
            
            // get the system family name
            AnalyticsVersionInfo ai = AnalyticsInfo.VersionInfo;
            SystemFamily = ai.DeviceFamily;

            // get the system version number
            string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(sv);
            ulong v1 = (v & 0xFFFF000000000000L) >> 48;
            ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (v & 0x000000000000FFFFL);
            SystemVersion = $"{v1}.{v2}.{v3}.{v4}";

            // get the package architecure
            Package package = Package.Current;
            SystemArchitecture = package.Id.Architecture.ToString();

            // get the user friendly app name
            ApplicationName = package.DisplayName;

            // get the app version
            PackageVersion pv = package.Id.Version;
            ApplicationVersion = $"{pv.Major}.{pv.Minor}.{pv.Build}.{pv.Revision}";

            // get the device manufacturer and model name
            EasClientDeviceInformation eas = new EasClientDeviceInformation();
            DeviceManufacturer = eas.SystemManufacturer;
            DeviceModel = eas.SystemProductName;
            DeviceId = eas.Id;
        }

        private Size? CachedSize = null;
        /// <summary>
        /// Returns the current window resolution in a thread-safe way
        /// </summary>
        /// <returns></returns>
        public Size GetActualWindowResolution()
        {
            if (Window.Current != null && !Window.Current.Bounds.IsEmpty)
            {
                CachedSize = new Size(ConvertToActualResolution(Window.Current.Bounds.Width), ConvertToActualResolution(Window.Current.Bounds.Height));
                return CachedSize.Value;
            }
            
            return CachedSize.HasValue ? CachedSize.Value : Size.Empty;
        }

        /// <summary>
        /// Converts Device-Independent resolution to the actual screen resolution active on the device
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public static double ConvertToActualResolution(double dimension)
        {
            ResolutionScale scale = DisplayInformation.GetForCurrentView().ResolutionScale;

            if (scale == ResolutionScale.Invalid)
                return dimension;

            return Math.Round((((double)scale) / 100) * dimension);
        }

        public static double ConvertToScaledResolution(double dimension)
        {
            if (Instance.ResolutionScale == ResolutionScale.Invalid)
                return dimension;

            return Math.Round((dimension / 4) * ((double)Instance.ResolutionScale) / 100);
        }

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
