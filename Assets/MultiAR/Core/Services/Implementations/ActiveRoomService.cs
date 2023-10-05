using System;
using System.Collections.Generic;
using System.Linq;
using MultiAR.Core.Models;
using MultiAR.Core.Services.Interfaces;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MultiAR.Core.Services.Implementations
{
    using UniRx;
    using User = Models.User;

    public class ActiveRoomService : MonoBehaviourPunCallbacks, IActiveRoomService
    {
        public event IActiveRoomService.UserListChanged OnUserListChanged;
        private IMultiUserService _multiUserService;

        private readonly Subject<User> _userJoinedSubject = new Subject<User>();
        private readonly Subject<User> _userLeftSubject = new Subject<User>();

        public IObservable<User> OnUserEnteredRoom()
        {
            return _userJoinedSubject.AsObservable();
        }

        public IObservable<User> OnUserLeftRoom()
        {
            return _userLeftSubject.AsObservable();
        }

        private void Start()
        {
            _multiUserService = FindObjectOfType<MultiUserService>();
            if (_multiUserService == null)
            {
                throw new Exception("MultiARSystem could not be found - Please add the prefab to the scene");
            }
        }

        public IEnumerable<User> GetUsers()
        {
            return GetInternalPlayerList();
        }

        public ActiveMultiUserRoom GetActiveRoom()
        {
            return _multiUserService.GetActiveRoom();
        }

        private IEnumerable<User> GetInternalPlayerList()
        {
            var currentRoom = PhotonNetwork.CurrentRoom;
            if (currentRoom == null)
            {
                Debug.LogWarning("Trying to get player list while not in a room");
                return new List<User>();
            }

            return currentRoom.Players.Values.Select(User.FromPhotonPlayer);
        }

        private void UpdateUserList()
        {
            OnUserListChanged?.Invoke(GetInternalPlayerList());
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdateUserList();
            _userJoinedSubject.OnNext(User.FromPhotonPlayer(newPlayer));
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            UpdateUserList();
            _userLeftSubject.OnNext(User.FromPhotonPlayer(otherPlayer));
        }
    }
}
