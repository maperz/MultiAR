namespace MultiAR.Core.Services.Implementations
{
    using Interfaces;
    using Models;
    using System;
    using System.Collections.Generic;
    using UniRx;
    using Zenject;

    public class RoomService : IRoomService, IInitializable, IDisposable
    {
        private readonly Subject<MultiUserRoom> _roomChanged = new Subject<MultiUserRoom>();

        [Inject] private readonly IMultiUserService _multiUserService;

        public IObservable<MultiUserRoom[]> GetRooms()
        {
            return _multiUserService.GetRooms();
        }

        public IObservable<MultiUserRoom> GetRoomUpdates(MultiUserRoom room)
        {
            if (room == null)
            {
                throw new ArgumentNullException();
            }

            return _roomChanged.Where(update => update != null && update.Name == room.Name).StartWith(room);
        }

        private void OnRoomsChanged(IEnumerable<MultiUserRoom> rooms)
        {
            foreach (MultiUserRoom room in rooms)
            {
                _roomChanged.OnNext(room);
            }
        }

        public void Initialize()
        {
            _multiUserService.RoomsChanged += OnRoomsChanged;
        }

        public void Dispose()
        {
            _multiUserService.RoomsChanged -= OnRoomsChanged;
        }
    }
}
