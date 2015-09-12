using Windows.Foundation.Metadata;

namespace Mendo.UAP.Common
{
    public sealed class DeviceInformation : Singleton<DeviceInformation>
    {
        public bool IsPhone { get; }
        public bool HasPhoneHardwareButtons { get; }

        public DeviceInformation()
        {
            IsPhone = ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1);
            HasPhoneHardwareButtons = ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");
        }
    }
}
