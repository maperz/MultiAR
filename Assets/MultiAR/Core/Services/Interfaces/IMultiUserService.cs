using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MultiAR.Core.Models;
using UnityEngine;

namespace MultiAR.Core.Services.Interfaces
{
    public interface IMultiUserService
    {
        public delegate void RoomsChangedHandler(IEnumerable<MultiUserRoom> rooms);

        public event RoomsChangedHandler RoomsAdded;
        public event RoomsChangedHandler RoomsRemoved;
        public event RoomsChangedHandler RoomsChanged;

        public IEnumerable<MultiUserRoom> GetCurrentRooms();

        public IObservable<MultiUserRoom[]> GetRooms();

        public Task<MultiUserRoom> CreateRoom(string name, string typeId, Pose localOrigin);
        public Task<MultiUserRoom> CreateRoom(string name, string typeId, MultiUserRoomSettings settings);

        public void JoinRoom(MultiUserRoom room, Pose pose, bool colocated);

        public void LeaveRoom();

        public ActiveMultiUserRoom GetActiveRoom();

        public User GetLocalUser();

        public IObservable<bool> HasActiveRoom();
    }
}
