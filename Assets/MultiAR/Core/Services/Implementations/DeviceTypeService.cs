using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using MultiAR.Core.Models;
using MultiAR.Core.Services.Interfaces;
using UnityEngine;

namespace MultiAR.Core.Services.Implementations
{
    [MixedRealityExtensionService((SupportedPlatforms)(-1))]
    public class DeviceTypeService : BaseExtensionService, IDeviceTypeService
    {
        public DeviceTypeService(string name, uint priority, BaseMixedRealityProfile profile) : base(name, priority,
            profile)
        {
            // Debug.Log($"Setting up Device Type Service: [Device: {GetDeviceType()}, ScreenBased: {IsDeviceScreenBased()}]");
        }

        public Device GetDeviceType()
        {
            return Application.platform switch
            {
                RuntimePlatform.WSAPlayerX64 => Device.HoloLens,
                RuntimePlatform.WSAPlayerX86 => Device.HoloLens,
                RuntimePlatform.WSAPlayerARM => Device.HoloLens,
                RuntimePlatform.WindowsPlayer => Device.HoloLens,
                RuntimePlatform.WindowsEditor => Device.HoloLens,

                RuntimePlatform.Android => Device.Mobile,
                RuntimePlatform.IPhonePlayer => Device.Mobile,
                _ => Device.Unknown
            };
        }

        public bool IsDeviceScreenBased()
        {
            if (Application.isEditor)
            {
                return true;
            }

            return Application.platform switch
            {
#if UNITY_WSA
                RuntimePlatform.WindowsPlayer => false,
#else
                RuntimePlatform.WindowsPlayer => true,
#endif
                _ => true
            };
        }
    }
}
