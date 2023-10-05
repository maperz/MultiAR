using System.Collections.Generic;
using MultiAR.Core.Models;

namespace MultiAR.Core.Services.Interfaces
{
    using System;
    using User = Models.User;

    public interface IActiveRoomService
    {
        public delegate void UserListChanged(IEnumerable<User> roomsUsers);

        public event UserListChanged OnUserListChanged;

        public IEnumerable<User> GetUsers();

        public ActiveMultiUserRoom GetActiveRoom();

        public IObservable<User> OnUserEnteredRoom();
        public IObservable<User> OnUserLeftRoom();
    }
}
