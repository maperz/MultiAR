using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace MultiAR.Core.Helper
{
    [RequireComponent(typeof(MixedRealityToolkit))]
    public class MrtkPlatformProfileSwapper : MonoBehaviour
    {
        public bool isEnabled = true;

        [SerializeField] public MixedRealityToolkitConfigurationProfile windowsProfile;

        [SerializeField] public MixedRealityToolkitConfigurationProfile mobileProfile;

        [SerializeField] public MixedRealityToolkitConfigurationProfile webProfile;

        private void Awake()
        {
            if (!isEnabled)
            {
                return;
            }

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    MixedRealityToolkit.SetProfileBeforeInitialization(windowsProfile);
                    break;
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    MixedRealityToolkit.SetProfileBeforeInitialization(mobileProfile);
                    break;
                case RuntimePlatform.WebGLPlayer:
                    MixedRealityToolkit.SetProfileBeforeInitialization(webProfile);
                    break;
                default:
                    Debug.LogWarning($"No MRTK Platform profile found for current platform: {Application.platform}");
                    break;
            }
        }
    }
}
