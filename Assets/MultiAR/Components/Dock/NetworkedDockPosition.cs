using UnityEngine;

namespace MultiAR.Components.Dock
{
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Photon.Pun;
    using System.Linq;

    [RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(PhotonView))]
    public class NetworkedDockPosition : MonoBehaviourPun
    {
        [SerializeField] [Tooltip("The object that is currently docked in this position (can be null).")]
        private NetworkedDockable _dockedObject = null;

        public MeshOutline meshOutline;

        /// <summary>
        /// True if this position is occupied, false otherwise.
        /// </summary>
        public bool IsOccupied => _dockedObject != null;

        [SerializeField] private string[] acceptedKeys = null;

        public bool addNameAsKey = false;

        private Renderer _renderer;

        public bool hideWhenDocked = true;

        public bool AcceptsKey(string key)
        {
            if (acceptedKeys == null)
            {
                return true;
            }

            return acceptedKeys.Contains(key);
        }

        /// <summary>
        /// Ensure this object has a triggering collider, and ensure that
        /// this object doesn't block manipulations.
        /// </summary>
        public void Awake()
        {
            if (addNameAsKey)
            {
                acceptedKeys = acceptedKeys.Append(gameObject.name).ToArray();
            }

            // Don't raycast this object to prevent blocking collisions
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            // Ensure there's a trigger collider for this position
            // The shape can be customized, but this adds a box as default.
            var objectCollider = GetComponent<Collider>();
            if (objectCollider == null)
            {
                objectCollider = gameObject.AddComponent<BoxCollider>();
            }

            objectCollider.isTrigger = true;

            // Ensure this collider can be used as a trigger by having
            // a RigidBody attached to it.
            var rigidBody = gameObject.EnsureComponent<Rigidbody>();
            rigidBody.isKinematic = true;

            if (meshOutline == null)
            {
                meshOutline = GetComponentInChildren<MeshOutline>();
            }

            _renderer = GetComponentInChildren<Renderer>();

            EnableHighlight(false);
        }

        public void EnableHighlight(bool enable)
        {
            if (meshOutline)
            {
                meshOutline.enabled = enable;
            }
        }

        public void SetDocked(NetworkedDockable dockable)
        {
            _dockedObject = dockable;
            OnDockedChanged(_dockedObject != null);
        }

        private void OnDockedChanged(bool docked)
        {
            if (_renderer != null && hideWhenDocked)
            {
                _renderer.enabled = !docked;
            }
        }
    }
}
