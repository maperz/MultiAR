using UnityEngine;

namespace MultiAR.Core.Behaviours
{
    using Microsoft.MixedReality.Toolkit.Input;
    using Microsoft.MixedReality.Toolkit.Rendering;
    using Models;
    using Photon.Pun;
    using Services.Interfaces;
    using UnityEngine.Assertions;

    public class UserFocusHighlighter : MonoBehaviourPun, IMixedRealityFocusHandler, IUserObject
    {
        [SerializeField]
        private MaterialInstance materialInstance;

        private Color _baseMaterialColor;

        private User _focusingUser = null;

        public User User => _focusingUser;

        public void Start()
        {
            Assert.IsNotNull(materialInstance);
            _baseMaterialColor = materialInstance.Material.color;
        }

        public void OnFocusEnter(FocusEventData eventData)
        {
            photonView.RPC(nameof(SetFocus), RpcTarget.AllBuffered, true);
        }

        public void OnFocusExit(FocusEventData eventData)
        {
            photonView.RPC(nameof(SetFocus), RpcTarget.AllBuffered, false);
        }

        [PunRPC]
        private void SetFocus(bool focused, PhotonMessageInfo info)
        {
            var user = User.FromPhotonPlayer(info.Sender);
            if (focused)
            {
                _focusingUser = user;
            }
            else if (_focusingUser == user)
            {
                _focusingUser = null;
            }
        }

        private void EnableFocusedState()
        {
            materialInstance.Material.color = _focusingUser.Color;
        }

        private void DisableFocusedState()
        {
            materialInstance.Material.color = _baseMaterialColor;
        }

    }
}
