using System;
using MultiAR.Core.Services.Implementations;
using Photon.Pun;
using UnityEngine;

namespace MultiAR.Shell.Scripts.Room
{
    using Core.Models;
    using Core.Services.Interfaces;

    [RequireComponent(typeof(PhotonView))]
    public class UserIndicator : MonoBehaviourPun, IPunObservable, IUserObject
    {
        public MeshRenderer meshRenderer;

        public bool hideLocal = true;

        private bool _isHidden;
        private bool? _isLocalCoLocated;

        public UsernameTextBanner usernameTextBanner;

        public User User { get; private set; }

        private bool IsLocalClientColocated
        {
            get
            {
                if (_isLocalCoLocated == null)
                {
                    var multiUserService = FindObjectOfType<MultiUserService>();
                    if (multiUserService == null)
                    {
                        throw new Exception("MultiARSystem could not be found");
                    }

                    var activeRoom = multiUserService.GetActiveRoom();
                    if (activeRoom == null)
                    {
                        throw new Exception(
                            "Failed to get active room although - Should be available as we are already connected");
                    }

                    _isLocalCoLocated = activeRoom.Colocated;
                }

                return _isLocalCoLocated!.Value;
            }
        }

        public void SetUser(User user)
        {
            User = user;
            var colorVector = new Vector3(user.Color.r, user.Color.g, user.Color.b);
            photonView.RPC(nameof(OnUserSet), RpcTarget.AllBuffered, user.Name, colorVector, User.Colocated);
        }

        private void Update()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            if (hideLocal && !_isHidden)
            {
                ShowMesh(false);
            }
        }

        [PunRPC]
        private void OnUserSet(string username, Vector3 color, bool colocated)
        {
            gameObject.name = username;
            //meshRenderer.material.color = ColorHelper.GetColorForId(id);
            // Debug.Log("Received user indicator updated for " + username + " co-located: " + colocated);

            if (IsLocalClientColocated && colocated)
            {
                // If we are colocated and the other instance is also colocated then we do not draw
                Debug.Log("Hiding Indicator for " + username + " as we are both co-located");
                ShowMesh(false);
            }

            if (usernameTextBanner)
            {
                usernameTextBanner.SetUserInfo(username, new Color(color.x, color.y, color.z));
            }
        }

        private void ShowMesh(bool show)
        {
            meshRenderer.enabled = show;
            foreach (var childRenderer in GetComponentsInChildren<Renderer>())
            {
                childRenderer.enabled = show;
            }

            if (!show)
            {
                // Always show username text of other users regardless of setting
                foreach (var childRenderer in usernameTextBanner.GetComponentsInChildren<Renderer>())
                {
                    childRenderer.enabled = !photonView.IsMine;
                }
            }

            _isHidden = !show;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // For some weird reason Photon spams errors if this object is not of type IPunObservable
        }

    }
}
