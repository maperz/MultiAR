namespace MultiAR.Shell.Scripts.Pointer
{
    using Core.Services.Interfaces;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Photon.Pun;
    using UniRx;
    using UnityEngine;
    using Zenject;

    [RequireComponent(typeof(BaseMixedRealityLineDataProvider))]
    public class MultiARLinePointer: MonoBehaviour
    {
        [Header("MultiAR - Settings")]

        [SerializeField]
        private NetworkedPointer networkedPointerPrefab;

        [Inject]
        private IMultiUserService _multiUserService;

        [Inject]
        private IPointerService _pointerService;


        private NetworkedPointer _networkedPointerInstance;
        private BaseMixedRealityLineDataProvider _lineDataProvider;

        protected void Start()
        {
            _lineDataProvider = GetComponent<BaseMixedRealityLineDataProvider>();
            _lineDataProvider.enabled = false;

            _multiUserService.HasActiveRoom().Subscribe(OnConnectedToRoomChanged).AddTo(this);
        }

        protected void OnDestroy()
        {
            DestroyNetworkedCursor();
        }

        private void OnConnectedToRoomChanged(bool connected)
        {
            Debug.Log("On connected to room changed: " + connected);
            if (connected)
            {
                _lineDataProvider.enabled = true;
                CreateNetworkedPointer();
            }
            else
            {
                DestroyNetworkedCursor();
                _lineDataProvider.enabled = false;
            }
        }

        private void CreateNetworkedPointer()
        {
            if (_networkedPointerInstance)
            {
                Debug.LogWarning("Networked cursor already exists - Trying to create duplicate prevented");
                return;
            }

            var cachedTransform = transform;
            var go = PhotonNetwork.Instantiate(networkedPointerPrefab.name, cachedTransform.position, cachedTransform.rotation);

            _networkedPointerInstance = go.GetComponent<NetworkedPointer>();
            var user = _multiUserService.GetLocalUser();

            _pointerService.SetSharedPointerEnabled(false);

            _networkedPointerInstance.SetPointerService(_pointerService);
            _networkedPointerInstance.transform.parent = transform;
            _networkedPointerInstance.SetUser(user);
            _networkedPointerInstance.SetLineDataProvider(_lineDataProvider);
        }

        private void DestroyNetworkedCursor()
        {
            if (_networkedPointerInstance)
            {
                Destroy(_networkedPointerInstance);
            }
        }
    }
}
