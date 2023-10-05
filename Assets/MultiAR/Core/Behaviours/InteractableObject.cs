namespace MultiAR.Core.Behaviours
{
    using UnityEngine;
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.Input;
    using Microsoft.MixedReality.Toolkit.UI;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Photon.Pun;
    using UniRx;
    using UnityEngine.Assertions;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhotonRelativeToOriginView))]
    public class InteractableObject : MonoBehaviourPun
    {
        public Material outlineMaterial;

        private NetworkedObject _networkedObject;

        public MeshOutline meshOutline;

        private InteractionOutline _interactionOutline;

        private FocusHandler _focusHandler;
        private ObjectManipulator _manipulator;

        private void Start()
        {
            Assert.IsNotNull(GetComponent<Collider>(),
                "Interactable Object " + gameObject.name + " requires some kind of collider");

            gameObject.SetActive(false);

            _networkedObject = gameObject.EnsureComponent<NetworkedObject>();

            if (!meshOutline)
            {
                meshOutline = gameObject.EnsureComponent<MeshOutline>();
            }

            if (meshOutline.OutlineMaterial == null && outlineMaterial)
            {
                meshOutline.OutlineMaterial = outlineMaterial;
            }

            _focusHandler = gameObject.EnsureComponent<FocusHandler>();
            _manipulator = gameObject.EnsureComponent<ObjectManipulator>();

            gameObject.EnsureComponent<NearInteractionGrabbable>();

            _interactionOutline = gameObject.EnsureComponent<InteractionOutline>();
            _interactionOutline.outline = meshOutline;
            _interactionOutline.networkedObject = _networkedObject;

            if (!meshOutline.OutlineMaterial)
            {
                Debug.LogWarning("Interaction outline is disabled as no outline material was set");
                _interactionOutline.enabled = false;
            }

            _manipulator.OnManipulationStarted.AsObservable().Subscribe((_) => _networkedObject.OnLocalInteractionStart()).AddTo(this);
            _manipulator.OnManipulationEnded.AsObservable().Subscribe((_) => _networkedObject.OnLocalInteractionStop()).AddTo(this);

            _focusHandler.OnFocusEnterEvent.AsObservable().Subscribe((_) => OnFocusChanged(true)).AddTo(this);
            _focusHandler.OnFocusExitEvent.AsObservable().Subscribe((_) => OnFocusChanged(false)).AddTo(this);

            gameObject.SetActive(true);
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
    }
}
