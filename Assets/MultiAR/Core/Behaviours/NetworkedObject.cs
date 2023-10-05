using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using MultiAR.Core.Models;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace MultiAR.Core.Behaviours
{
    using Helper;
    using Photon.Realtime;
    using Services.Implementations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;

    [RequireComponent(typeof(PhotonView))]
    public class NetworkedObject : MonoBehaviourPun, IOnPhotonViewControllerChange
    {
        private BoundsControl _boundsControl;
        private ObjectManipulator _objectManipulator;
        private NearInteractionGrabbable _nearInteractionGrabbable;

        public UnityEvent<NetworkObjectInteraction> onInteractionStart = new UnityEvent<NetworkObjectInteraction>();
        public UnityEvent<NetworkObjectInteraction> onInteractionStop = new UnityEvent<NetworkObjectInteraction>();

        public readonly BehaviorSubject<NetworkObjectInteraction> TransformInteraction =
            new BehaviorSubject<NetworkObjectInteraction>(null);

        public readonly BehaviorSubject<ImmutableInteractionSet> FocusInteractions =
            new BehaviorSubject<ImmutableInteractionSet>(ImmutableInteractionSet.Empty);

        private ActiveRoomService _activeRoomService;

        private bool _isLocallyInteracting;

        private void Awake()
        {
            photonView.OwnershipTransfer = OwnershipOption.Takeover;

            _boundsControl = GetComponent<BoundsControl>();
            _objectManipulator = GetComponent<ObjectManipulator>();
            _nearInteractionGrabbable = GetComponent<NearInteractionGrabbable>();
        }

        private void OnEnable()
        {
            if (_boundsControl != null)
            {
                _boundsControl.TranslateStarted.AddListener(OnLocalInteractionStart);
                _boundsControl.TranslateStopped.AddListener(OnLocalInteractionStop);
                _boundsControl.RotateStarted.AddListener(OnLocalInteractionStart);
                _boundsControl.RotateStopped.AddListener(OnLocalInteractionStop);
                _boundsControl.ScaleStarted.AddListener(OnLocalInteractionStart);
                _boundsControl.ScaleStopped.AddListener(OnLocalInteractionStop);
            }

            if (_objectManipulator != null)
            {
                _objectManipulator.OnManipulationStarted.AddListener((_) => OnLocalInteractionStart());
                _objectManipulator.OnManipulationEnded.AddListener((_) => OnLocalInteractionStop());
            }

            _activeRoomService = FindObjectOfType<ActiveRoomService>();
            if (_activeRoomService == null)
            {
                throw new Exception("ActiveRoomService could not be found - Please add the prefab to the scene");
            }

            _activeRoomService.OnUserEnteredRoom().Subscribe(SendCurrentStateToNewUser).AddTo(this);
            _activeRoomService.OnUserLeftRoom().Subscribe(OnUserLeftRoom).AddTo(this);

            photonView.AddCallbackTarget(this);
        }


        private void OnDisable()
        {
            photonView.RemoveCallbackTarget(this);

            if (_boundsControl != null)
            {
                _boundsControl.TranslateStarted.RemoveListener(OnLocalInteractionStart);
                _boundsControl.TranslateStopped.RemoveListener(OnLocalInteractionStop);
                _boundsControl.RotateStarted.RemoveListener(OnLocalInteractionStart);
                _boundsControl.RotateStopped.RemoveListener(OnLocalInteractionStop);
                _boundsControl.ScaleStarted.RemoveListener(OnLocalInteractionStart);
                _boundsControl.ScaleStopped.RemoveListener(OnLocalInteractionStop);
            }

            if (_objectManipulator != null)
            {
                _objectManipulator.OnManipulationStarted.RemoveAllListeners();
                _objectManipulator.OnManipulationEnded.RemoveAllListeners();
            }
        }

        public void OnLocalInteractionStart()
        {
            _isLocallyInteracting = true;

            var currentTransformInteraction = TransformInteraction.Value;
            if (currentTransformInteraction != null)
            {
                if (!currentTransformInteraction.IsLocal)
                {
                    Debug.LogWarning("Trying to start interaction on object that is locked by someone else");
                    SetLocalInteractionEnabled(false);
                }

                return;
            }

            // Local interaction starts as soon as ownership was acquired
            if (photonView.IsMine)
            {
                photonView.RPC(nameof(InteractionEvent), RpcTarget.All, true);
            }
            else
            {
                photonView.RequestOwnership();
            }
        }

        public void OnLocalInteractionStop()
        {
            _isLocallyInteracting = false;

            var currentTransformInteraction = TransformInteraction.Value;
            if (currentTransformInteraction == null)
            {
                return;
            }

            if (!currentTransformInteraction.IsLocal)
            {
                Debug.LogWarning("Trying to stop interaction that was started by someone else");
                SetLocalInteractionEnabled(false);
                return;
            }

            photonView.RPC(nameof(InteractionEvent), RpcTarget.All, false);
        }

        public void Focus()
        {
            photonView.RPC(nameof(OnFocusEvent), RpcTarget.All, true);
        }

        public void Unfocus()
        {
            photonView.RPC(nameof(OnFocusEvent), RpcTarget.All, false);
        }

        private void HandleFocusEvent(bool focused, NetworkObjectInteraction focusInteraction)
        {
            var currentFocused = FocusInteractions.Value;

            FocusInteractions.OnNext(focused
                ? currentFocused.TryAdd(focusInteraction)
                : currentFocused.TryRemove(focusInteraction));
        }

        private void HandleInteractionEvent(bool start, NetworkObjectInteraction interaction)
        {
            var currentInteraction = TransformInteraction.Value;
            if (start)
            {
                if (Equals(currentInteraction, interaction))
                {
                    Debug.LogWarning("Trying to start interaction that is already running");
                    return;
                }

                if (!interaction.IsLocal)
                {
                    SetLocalInteractionEnabled(false);
                }

                TransformInteraction.OnNext(interaction);
                onInteractionStart?.Invoke(interaction);
                // Debug.Log("User " + interaction.User.Name + " started interaction");
            }
            else
            {
                if (!Equals(currentInteraction, interaction))
                {
                    Debug.LogWarning("Trying to stop interaction that is currently not running");
                    return;
                }

                TransformInteraction.OnNext(null);
                onInteractionStop?.Invoke(interaction);

                // Debug.Log("User " + interaction.User.Name + " stopped interaction");

                if (!interaction.IsLocal)
                {
                    SetLocalInteractionEnabled(true);
                }
            }
        }

        [PunRPC]
        void OnFocusEvent(bool focused, PhotonMessageInfo info)
        {
            HandleFocusEvent(focused, NetworkObjectInteraction.From(info.Sender));
        }

        [PunRPC]
        void InteractionEvent(bool start, PhotonMessageInfo info)
        {
            HandleInteractionEvent(start, NetworkObjectInteraction.From(info.Sender));
        }

        private void SetLocalInteractionEnabled(bool value)
        {
            if (_objectManipulator)
            {
                _objectManipulator.enabled = value;
            }

            if (_boundsControl)
            {
                _boundsControl.enabled = value;
            }

            if (_nearInteractionGrabbable)
            {
                _nearInteractionGrabbable.enabled = value;
            }

            if (!value)
            {
                _isLocallyInteracting = false;
            }
        }

        private void OnUserLeftRoom(User user)
        {
            // Debug.Log("Removing all interactions of user");

            var interaction = NetworkObjectInteraction.From(user);

            if (TransformInteraction.Value != null && TransformInteraction.Value.User != null && TransformInteraction.Value.User.Equals(user))
            {
                HandleInteractionEvent(false, interaction);
            }

            HandleFocusEvent(false, interaction);
        }

        private void SendCurrentStateToNewUser(User user)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            Debug.Log("Sending current state to new user: " + user.Name);

            var currentTransformInteraction = TransformInteraction.Value;
            var currentFocusInteractions = FocusInteractions.Value;

            var interactingUser = currentTransformInteraction != null ? currentTransformInteraction.User.Id : 0;
            var focusingUserIds = currentFocusInteractions.Data.Select(interaction => interaction.User.Id).ToArray();

            var newPlayer = FindPlayer(user.Id);
            if (newPlayer != null)
            {
                photonView.RPC(nameof(InitStateNewPlayer), newPlayer, interactingUser, focusingUserIds);
            }
        }

        [PunRPC]
        public void InitStateNewPlayer(int interactingUserId, int[] focusingUserIds)
        {
            var users = new Dictionary<int, User>();

            if (interactingUserId > 0)
            {
                users[interactingUserId] = null;
            }

            foreach (var user in focusingUserIds)
            {
                users[user] = null;
            }

            LookupUser(users);

            if (interactingUserId > 0 && users[interactingUserId] != null)
            {
                var interactingUser = users[interactingUserId];
                HandleInteractionEvent(true,
                    new NetworkObjectInteraction()
                    {
                        User = interactingUser, IsLocal = interactingUser.IsLocal
                    });
            }

            foreach (int focusingUserId in focusingUserIds)
            {
                var focusingUser = users[focusingUserId];
                if (focusingUser != null)
                {
                    HandleFocusEvent(true,
                        new NetworkObjectInteraction() {User = focusingUser, IsLocal = focusingUser.IsLocal});
                }
            }
        }

        private static Player FindPlayer(int actorId)
        {
            return PhotonNetwork.PlayerList.FirstOrDefault(u => u.ActorNumber == actorId);
        }

        private static void LookupUser(Dictionary<int, User> users)
        {
            var allUsers = PhotonNetwork.PlayerList;
            foreach (var actorId in users.Keys.ToList())
            {
                users[actorId] = User.FromPhotonPlayer(allUsers.FirstOrDefault(u => u.ActorNumber == actorId));
            }
        }

        public void OnControllerChange(Player newController, Player previousController)
        {
            //Debug.Log("Owner changed from " + previousController.UserId + " -> " + newController.UserId);

            var currentTransformInteraction = TransformInteraction.Value;
            if (currentTransformInteraction != null)
            {
                var previousUser = User.FromPhotonPlayer(previousController);
                HandleInteractionEvent(false, NetworkObjectInteraction.From(previousUser));
            }

            // Check if requested ownership and is still interacting
            if (newController.IsLocal && _isLocallyInteracting)
            {
                photonView.RPC(nameof(InteractionEvent), RpcTarget.All, true);
            }
        }
    }
}
