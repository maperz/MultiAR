using UnityEngine;

namespace MultiAR.Components.Dock
{
    using Core.Behaviours;
    using Core.Helper;
    using Core.Models;
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.Experimental.UI;
    using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
    using Photon.Pun;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UnityEngine.Assertions;
    using UnityEngine.Events;

    [RequireComponent(typeof(PhotonView), typeof(NetworkedObject), typeof(Collider))]
    public class NetworkedDockable : MonoBehaviourPun, IPunObservable
    {
        private const float MoveLerpTime = 0.1f;
        private const float MoveLerpTimeWhenDocked = 0.05f;

        public string key = null;
        public bool useNameAsKey = false;

        public bool CanDock => _dockingState == DockingState.Undocked || _dockingState == DockingState.Undocking;

        /// <summary>
        /// True if this object can currently be undocked, false otherwise.
        /// </summary>
        public bool CanUndock => _dockingState == DockingState.Docked;

        // Constants
        private const float DistanceTolerance = 0.01f; // in meters
        private const float AngleTolerance = 20.0f; // in degrees
        private const float ScaleTolerance = 0.01f; // in percentage

        private readonly HashSet<NetworkedDockPosition> _overlappingPositions = new HashSet<NetworkedDockPosition>();
        private Vector3 _dockedPositionScale = Vector3.one;

        private bool _isDragging;

        private NetworkedObject _networkedObject;

        // Shared multi user state
        private DockingState _dockingState = DockingState.Undocked;
        private NetworkedDockPosition _dockedPosition;
        private Vector3 _originalScale = Vector3.one;

        private bool _isDirty = true;
        private bool _freshInteraction;

        private NetworkedDockPosition _hoveredPosition;
        private CompositeDisposable _subscription;

        public UnityEvent<NetworkedDockPosition> onDocked = new UnityEvent<NetworkedDockPosition>();
        public UnityEvent onUndocked = new UnityEvent();

        /// <summary>
        /// Subscribes to manipulation events.
        /// </summary>
        private void OnEnable()
        {
            if (useNameAsKey)
            {
                key = gameObject.name;
            }

            _networkedObject = GetComponent<NetworkedObject>();

            if (_networkedObject != null)
            {
                _subscription?.Dispose();

                _subscription = new CompositeDisposable();
                _subscription.Add(_networkedObject.onInteractionStart.AsObservable().Subscribe(OnInteractionStarted));
                _subscription.Add(_networkedObject.onInteractionStop.AsObservable().Subscribe(OnInteractionEnded));
                _subscription.AddTo(this);
            }

            Assert.IsNotNull(gameObject.GetComponent<Collider>(),
                "A Network dockable object must have a Collider component.");
        }

        /// <summary>
        /// Unsubscribes from manipulation events.
        /// </summary>
        private void OnDisable()
        {
            _overlappingPositions.Clear();

            if (!photonView.IsMine)
            {
                return;
            }

            if (photonView.IsMine && _dockedPosition != null)
            {
                _dockedPosition.SetDocked(null);
                _dockedPosition = null;
            }

            if (_dockingState == DockingState.Docked)
            {
                _dockingState = DockingState.Undocked;
                ChangeLocalDockState(_dockingState, null);
            }
        }

        /// <summary>
        /// Updates the transform and state of this object every frame, depending on
        /// manipulations and docking state.
        /// </summary>
        public void Update()
        {
            if (!photonView.IsMine || !PhotonNetwork.InRoom)
            {
                return;
            }

            var oldHoveredPosition = _hoveredPosition;
            _hoveredPosition = GetClosestValidPositionThatOverlaps();

            if (_freshInteraction)
            {
                _freshInteraction = false;
                if (_isDragging && CanUndock)
                {
                    Undock();
                }
                else if (!_isDragging && CanDock && _hoveredPosition != null)
                {
                    Dock(_hoveredPosition);
                }
            }

            if (oldHoveredPosition != _hoveredPosition)
            {
                if (oldHoveredPosition != null)
                {
                    oldHoveredPosition.EnableHighlight(false);
                }

                if (_hoveredPosition != null)
                {
                    _hoveredPosition.EnableHighlight(true);
                }
            }

            var cachedTransform = transform;
            if (_dockingState == DockingState.Docked || _dockingState == DockingState.Docking)
            {
                Assert.IsNotNull(_dockedPosition, "When a dockable is docked, its dockedPosition must be valid.");
                var lerpTime = _dockingState == DockingState.Docked ? MoveLerpTimeWhenDocked : MoveLerpTime;

                if (!_isDragging)
                {
                    // Don't override dragging
                    transform.position = Solver.SmoothTo(cachedTransform.position, _dockedPosition.transform.position,
                        Time.deltaTime, lerpTime);
                    transform.rotation = Solver.SmoothTo(cachedTransform.rotation, _dockedPosition.transform.rotation,
                        Time.deltaTime, lerpTime);
                }

                transform.localScale =
                    Solver.SmoothTo(transform.localScale, _dockedPositionScale, Time.deltaTime, lerpTime);

                if (VectorExtensions.CloseEnough(_dockedPosition.transform.position, transform.position,
                        DistanceTolerance) &&
                    QuaternionExtensions.AlignedEnough(_dockedPosition.transform.rotation, transform.rotation,
                        AngleTolerance) &&
                    AboutTheSameSize(_dockedPositionScale.x, transform.localScale.x))
                {
                    if (_dockingState == DockingState.Docking)
                    {
                        // Finished docking
                        _dockingState = DockingState.Docked;
                        _isDirty = true;

                        // Snap to position
                        var docketTransform = _dockedPosition.transform;
                        transform.SetPositionAndRotation(docketTransform.position, docketTransform.rotation);
                        transform.localScale = _dockedPositionScale;
                        ChangeLocalDockState(_dockingState, _dockedPosition);
                    }
                }
            }
            else if (_dockedPosition == null && _dockingState == DockingState.Undocking)
            {
                transform.localScale =
                    Solver.SmoothTo(transform.localScale, _originalScale, Time.deltaTime, MoveLerpTime);

                if (AboutTheSameSize(_originalScale.x, transform.localScale.x))
                {
                    // Finished undocking
                    _dockingState = DockingState.Undocked;
                    ChangeLocalDockState(_dockingState, _dockedPosition);
                    _isDirty = true;

                    // Snap to size
                    transform.localScale = _originalScale;
                }
            }
        }


        /// <summary>
        /// Docks this object in a given <see cref="DockPosition"/>.
        /// </summary>
        /// <param name="position">The <see cref="DockPosition"/> where we'd like to dock this object.</param>
        public void Dock(NetworkedDockPosition position)
        {
            if (!photonView.IsMine)
            {
                Debug.LogError("Trying to dock an object that is not owned");
                return;
            }

            if (!CanDock)
            {
                Debug.LogError($"Trying to dock an object that was not undocked. State = {_dockingState}");
                return;
            }

            if (!position.AcceptsKey(this.key))
            {
                Debug.LogError($"Trying to dock an object on an incompatible dock position. Key = {this.key}");
                return;
            }

            // Debug.Log($"Docking object {gameObject.name} on position {position.gameObject.name}");

            _dockedPosition = position;
            _dockedPosition.SetDocked(this);

            /*float scaleToFit = gameObject.GetComponent<Collider>().bounds
                .GetScaleToFitInside(_dockedPosition.GetComponent<Collider>().bounds);
            _dockedPositionScale = transform.localScale * scaleToFit;*/

            _dockedPositionScale = _dockedPosition.transform.localScale;

            if (_dockingState == DockingState.Undocked)
            {
                // Only register the original scale when first docking
                _originalScale = transform.localScale;
            }

            _dockingState = DockingState.Docking;
            ChangeLocalDockState(_dockingState, position);
            _isDirty = true;
        }

        /// <summary>
        /// Undocks this <see cref="Dockable"/> from the current <see cref="DockPosition"/> where it is docked.
        /// </summary>
        public void Undock()
        {
            if (!photonView.IsMine)
            {
                Debug.LogError("Trying to undock an object that is not owned");
                return;
            }

            if (!CanUndock)
            {
                Debug.LogError($"Trying to undock an object that was not docked. State = {_dockingState}");
                return;
            }

            // Debug.Log($"Undocking object {gameObject.name} from position {_dockedPosition.gameObject.name}");

            _dockedPosition.SetDocked(null);

            _dockedPosition = null;
            _dockedPositionScale = Vector3.one;
            _dockingState = DockingState.Undocking;
            ChangeLocalDockState(_dockingState, null);

            _isDirty = true;
        }

        #region Collision events

        void OnTriggerEnter(Collider otherCollider)
        {
            var dockPosition = otherCollider.gameObject.GetComponent<NetworkedDockPosition>();
            if (dockPosition != null && !_overlappingPositions.Contains(dockPosition))
            {
                _overlappingPositions.Add(dockPosition);
                //Debug.Log($"{gameObject.name} collided with {dockPosition.name}");
            }
        }

        void OnTriggerExit(Collider otherCollider)
        {
            var dockPosition = otherCollider.gameObject.GetComponent<NetworkedDockPosition>();
            if (_overlappingPositions.Contains(dockPosition))
            {
                _overlappingPositions.Remove(dockPosition);
            }
        }

        #endregion

        #region Manipulation events

        private void OnInteractionStarted(NetworkObjectInteraction interaction)
        {
            if (!interaction.IsLocal)
            {
                return;
            }

            _freshInteraction = true;
            _isDragging = true;
        }

        private void OnInteractionEnded(NetworkObjectInteraction interaction)
        {
            if (!interaction.IsLocal)
            {
                return;
            }

            _freshInteraction = true;
            _isDragging = false;
        }

        #endregion

        private NetworkedDockPosition GetClosestValidPositionThatOverlaps()
        {
            if (!_overlappingPositions.Any())
            {
                return null;
            }

            var bounds = gameObject.GetComponent<Collider>().bounds;
            var minDistance = float.MaxValue;
            NetworkedDockPosition closestPosition = null;
            foreach (var position in _overlappingPositions)
            {
                if (position.IsOccupied || !position.AcceptsKey(key))
                {
                    continue;
                }

                var distance = (position.gameObject.GetComponent<Collider>().bounds.center - bounds.center)
                    .sqrMagnitude;
                if (closestPosition == null || distance < minDistance)
                {
                    closestPosition = position;
                    minDistance = distance;
                }
            }

            return closestPosition;
        }

        #region Helpers

        private static bool AboutTheSameSize(float scale1, float scale2)
        {
            Assert.AreNotEqual(0.0f, scale2, "Cannot compare scales with an object that has scale zero.");
            return Mathf.Abs(scale1 / scale2 - 1.0f) < ScaleTolerance;
        }

        #endregion

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            stream.Serialize(ref _isDirty);
            if (!_isDirty)
            {
                return;
            }

            if (stream.IsWriting)
            {
                SendDockedPosition(stream, _dockedPosition);
                stream.SendNext(_originalScale);
                stream.SendNext(_dockedPositionScale);
                stream.SendNext((int)_dockingState);
            }
            else
            {
                var oldDockingState = _dockingState;
                var oldDockedPosition = _dockedPosition;

                _dockedPosition = ReceiveDockedPosition(stream);
                _originalScale = (Vector3)stream.ReceiveNext();
                _dockedPositionScale = (Vector3)stream.ReceiveNext();
                _dockingState = (DockingState)((int)stream.ReceiveNext());

                if (oldDockingState != _dockingState)
                {
                    if ((_dockingState == DockingState.Undocking || _dockingState == DockingState.Undocked) &&
                        oldDockedPosition != null)
                    {
                        oldDockedPosition.SetDocked(null);

                        if (_dockingState == DockingState.Undocked)
                        {
                            ChangeLocalDockState(_dockingState, null);
                        }
                    }
                    else if (_dockingState == DockingState.Docking || _dockingState == DockingState.Docked)
                    {
                        if (oldDockedPosition)
                        {
                            oldDockedPosition.SetDocked(null);
                        }

                        _dockedPosition.SetDocked(this);
                        ChangeLocalDockState(_dockingState, _dockedPosition);
                    }
                }
            }

            _isDirty = false;
        }

        private void ChangeLocalDockState(DockingState state, NetworkedDockPosition position)
        {
            // Debug.Log("Docked status: " + state);
            if (state == DockingState.Docked)
            {
                onDocked.Invoke(position);
            }
            else if (state == DockingState.Undocked)
            {
                onUndocked.Invoke();
            }
        }

        private static void SendDockedPosition(PhotonStream stream, NetworkedDockPosition position)
        {
            stream.SendNext(position != null ? position.photonView.ViewID : 0);
        }

        private static NetworkedDockPosition ReceiveDockedPosition(PhotonStream stream)
        {
            return PhotonHelper.FindNetworkedComponent<NetworkedDockPosition>((int)stream.ReceiveNext());
        }
    }
}
