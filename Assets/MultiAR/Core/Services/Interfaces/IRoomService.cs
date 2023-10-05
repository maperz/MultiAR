using System;

namespace MultiAR.Core.Services.Interfaces
{
    using Models;

    public interface IRoomService
    {
        public IObservable<MultiUserRoom[]> GetRooms();

        public IObservable<MultiUserRoom> GetRoomUpdates(MultiUserRoom room);
    }
}
