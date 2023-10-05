namespace MultiAR_Examples.Buttons.Scripts
{
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Photon.Pun;
    using UnityEngine;

    public class CreateAttachedGameObject: MonoBehaviour
    {

        public GameObject prefab;
        public bool preventSleepWhenInstanceCreated = true;

        private GameObject _instance;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.P) || Input.touchCount >= 3)
            {
                ToggleObject();
            }
        }

        private void ToggleObject()
        {
            if (_instance == null)
            {
                CreateObject();
            }
            else
            {
                DestroyObject();
            }
        }

        private void CreateObject()
        {
            if (_instance)
            {
                Debug.LogWarning("Not creating instance as one already exists");
                return;
            }

            Debug.Log("Creating attached object");
            var cameraTransform = CameraCache.Main.transform;
            _instance = PhotonNetwork.Instantiate(prefab.name, cameraTransform.position,
                cameraTransform.rotation);
            _instance.transform.SetParent(cameraTransform);

            if (preventSleepWhenInstanceCreated)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
        }

        private void DestroyObject()
        {
            if (!_instance)
            {
                Debug.LogWarning("Not destroying instance as it does not exist");
                return;
            }

            Debug.Log("Destroying attached object");
            PhotonNetwork.Destroy(_instance);
            _instance = null;

            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
    }
}
