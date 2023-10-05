using Microsoft.MixedReality.Toolkit;
using MultiAR.Core.Services.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace MultiAR.Core.Behaviours
{
    using Models;
    using UnityEngine.Serialization;

    public class PlatformEvents : MonoBehaviour
    {
        public bool enableInEditor;

        [FormerlySerializedAs("onHololensModeDetected")]
        public UnityEvent hololensModeDetected = new UnityEvent();


        [FormerlySerializedAs("onScreenModeDetected")]
        public UnityEvent mobileModeDetected = new UnityEvent();

        private void Start()
        {
#if UNITY_EDITOR
            if (!enableInEditor)
            {
                return;
            }
#endif

            var deviceTypeService = MixedRealityToolkit.Instance.GetService<IDeviceTypeService>();
            var device = deviceTypeService.GetDeviceType();

            switch (device)
            {
                case Device.HoloLens:
                    hololensModeDetected?.Invoke();
                    break;
                case Device.Mobile:
                    mobileModeDetected?.Invoke();
                    break;
                case Device.Unknown:
                default:
                    Debug.LogWarning("Unknown device type detected");
                    break;
            }
        }
    }
}
