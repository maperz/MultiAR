namespace MultiAR.Core.Behaviours
{
    using UnityEngine;
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.Input;
    using Microsoft.MixedReality.Toolkit.UI;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Models;
    using Photon.Pun;
    using UniRx;
    using UnityEngine.Assertions;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(RigidBodyOriginRelativeView))]
    public class PhysicsDrivenInteractableObject : MonoBehaviour
    {
        [SerializeField]
        private Material outlineMaterial;

        private NetworkedObject _networkedObject;
        private MeshOutline _meshOutline;
        private FocusHandler _focusHandler;
        private ObjectManipulator _manipulator;
        private InteractionOutline _interactionOutline;
        private PhotonView _photonView;
        private Rigidbody _rigidbody;

        private void Start()
        {
            Assert.IsNotNull(GetComponent<Collider>(), "Interactable Object " + gameObject.name +  " requires some kind of collider");

            gameObject.SetActive(false);

            _photonView = GetComponent<PhotonView>();
            _photonView.OwnershipTransfer = OwnershipOption.Request;

            _rigidbody = GetComponent<Rigidbody>();

            _networkedObject = gameObject.EnsureComponent<NetworkedObject>();
            _meshOutline = gameObject.EnsureComponent<MeshOutline>();
            _focusHandler = gameObject.EnsureComponent<FocusHandler>();
            _manipulator = gameObject.EnsureComponent<ObjectManipulator>();
            _interactionOutline = gameObject.EnsureComponent<InteractionOutline>();

            gameObject.EnsureComponent<NearInteractionGrabbable>();

            _meshOutline.OutlineMaterial = outlineMaterial;

            _interactionOutline.outline = _meshOutline;
            _interactionOutline.networkedObject = _networkedObject;

            _manipulator.OnManipulationStarted.AddListener(_ => _networkedObject.OnLocalInteractionStart());
            _manipulator.OnManipulationEnded.AddListener(_ => _networkedObject.OnLocalInteractionStop());

            _focusHandler.OnFocusEnterEvent.AddListener(() => OnFocusChanged(true));
            _focusHandler.OnFocusExitEvent.AddListener(() => OnFocusChanged(false));

            _networkedObject.TransformInteraction.Subscribe(OnTransformInteraction).AddTo(this);

            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            _manipulator.OnManipulationStarted.RemoveAllListeners();
            _manipulator.OnManipulationEnded.RemoveAllListeners();

            _focusHandler.OnFocusEnterEvent.RemoveAllListeners();
            _focusHandler.OnFocusExitEvent.RemoveAllListeners();
        }

        private void OnFocusChanged(bool focusing)
        {
            if (focusing)
            {
                _networkedObject.Focus();
            }
            else
            {
                _networkedObject.Unfocus();
            }
        }

        private void OnTransformInteraction(NetworkObjectInteraction interaction)
        {
            if (interaction != null)
            {
                // _rigidbody.isKinematic  = !interaction.IsLocal;
            }
        }
    }
}
