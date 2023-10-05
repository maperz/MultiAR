namespace MultiAR.Components.Button
{
    using Microsoft.MixedReality.Toolkit.UI;
    using Photon.Pun;
    using UnityEngine;
    using UnityEngine.Events;

    public class NetworkedButton : MonoBehaviourPun
    {
        public UnityEvent onPressed = new UnityEvent();

        [SerializeField] private Interactable interactable;

        private void Start()
        {
            if (interactable)
            {
                interactable.OnClick.AddListener(OnPressedLocally);
            }
        }

        private void OnDestroy()
        {
            if (interactable)
            {
                interactable.OnClick.RemoveListener(OnPressedLocally);
            }
        }

        public void OnPressedLocally()
        {
            photonView.RPC(nameof(OnPressedViaNetwork), RpcTarget.All);
        }

        [PunRPC]
        void OnPressedViaNetwork()
        {
            onPressed?.Invoke();
        }
    }
}
