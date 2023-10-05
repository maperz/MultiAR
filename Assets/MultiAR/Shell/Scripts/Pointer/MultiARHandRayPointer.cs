namespace MultiAR.Shell.Scripts.Pointer
{
    using Microsoft.MixedReality.Toolkit.Input;
    using Core.Services.Interfaces;
    using Photon.Pun;
    using UniRx;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Zenject;

    public class MultiARHandRayPointer : ShellHandRayPointer
    {
        [Header("MultiAR - Settings")]

        [SerializeField]
        private NetworkedPointer networkedPointerPrefab;

        [Inject]
        private IMultiUserService _multiUserService;

        [Inject]
        private IPointerService _pointerService;

        private NetworkedPointer _networkedPointerInstance;

        protected override void Start()
        {
            base.Start();
            _multiUserService.HasActiveRoom().Subscribe(OnConnectedToRoomChanged).AddTo(this);
        }

        protected void OnDestroy()
        {
            DestroyNetworkedCursor();
        }

        private void OnConnectedToRoomChanged(bool connected)
        {
            // Debug.Log("On connected to room changed: " + connected);
            if (connected)
            {
                CreateNetworkedPointer();
            }
            else
            {
                DestroyNetworkedCursor();
            }
        }

        private void CreateNetworkedPointer()
        {
            if (_networkedPointerInstance)
            {
                Debug.LogWarning("Networked cursor already exists - Trying to create duplicate prevented");
                return;
            }

            var lineRenderer = LineRenderers[0];

            Assert.IsNotNull(lineRenderer);

            var cachedTransform = transform;
            var go = PhotonNetwork.Instantiate(networkedPointerPrefab.name, cachedTransform.position, cachedTransform.rotation);

            _networkedPointerInstance = go.GetComponent<NetworkedPointer>();
            var user = _multiUserService.GetLocalUser();

            _pointerService.SetSharedPointerEnabled(false);

            _networkedPointerInstance.SetPointerService(_pointerService);
            _networkedPointerInstance.transform.parent = transform;
            _networkedPointerInstance.SetUser(user);
            _networkedPointerInstance.SetLineDataProvider(lineRenderer.LineDataSource);
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
