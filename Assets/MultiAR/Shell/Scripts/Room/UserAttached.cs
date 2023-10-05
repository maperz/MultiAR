namespace MultiAR.Shell.Scripts.Room
{
    using Core.Behaviours;
    using Core.Models;
    using Core.Services.Interfaces;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Photon.Pun;
    using UnityEngine;

    [RequireComponent(typeof(PhotonRelativeToOriginView))]
    public class UserAttached : MonoBehaviourPun, IPunObservable, IUserObject
    {
        public bool hideOnLocal = true;

        public User User { get; private set; }
        public bool attachOnStart = true;

        private void Start()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (attachOnStart)
            {
                var cameraTransform = CameraCache.Main.transform;
                transform.SetParent(cameraTransform);
            }

            if (hideOnLocal)
            {
                ShowMesh(false);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // For some weird reason Photon spams errors if this object is not of type IPunObservable
        }

        private void ShowMesh(bool show)
        {
            var rendererComponent = GetComponent<Renderer>();
            if (rendererComponent)
            {
                rendererComponent.enabled = show;
            }
            foreach (var childRenderer in GetComponentsInChildren<Renderer>())
            {
                childRenderer.enabled = show;
            }
        }
    }
}
