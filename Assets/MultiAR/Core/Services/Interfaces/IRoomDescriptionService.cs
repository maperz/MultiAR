namespace MultiAR.Core.Services.Interfaces
{
    using Models;
    using Shell.Scripts.Lobby;

    public interface IRoomDescriptionService
    {
        RoomDescription GetRoomDescription(string roomType);

        RoomDescription GetRoomDescription(MultiUserRoom room);
    }
}
