namespace MultiAR.Core.Helper
{
    using Models;
    using UnityEngine;
    using Services.Interfaces;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UniRx;
    using Zenject;

    public class AutoJoinRoom : MonoBehaviour
    {
        [Inject] private readonly IMultiUserService _multiUserService;

        public string roomTypeId;

        public bool createIfNotExists = true;
        public Pose pose = Pose.identity;
        public bool colocated;

        private void OnEnable()
        {
            if (!Debug.isDebugBuild)
            {
                Debug.LogWarning("Auto join script only works on debug build - Skipping auto join");
                return;
            }

#pragma warning disable CS4014
            _multiUserService.GetRooms().First().Subscribe((rooms) => JoinOrCreateRoom(rooms));
#pragma warning restore CS4014
        }

        private async Task JoinOrCreateRoom(MultiUserRoom[] activeRooms)
        {
            var existingRoom = activeRooms.Where(room => room.TypeId == roomTypeId).OrderBy(room => room.UserCount)
                .FirstOrDefault();

            if (existingRoom != null)
            {
                Debug.Log("Joining existing room with name: " + existingRoom.Name);
                _multiUserService.JoinRoom(existingRoom, pose, colocated);
            }
            else if (createIfNotExists)
            {
                Debug.Log("Creating a new room with type: " + roomTypeId);
                var room = await _multiUserService.CreateRoom($"AutoJoin_{roomTypeId.ToUpper()}", roomTypeId,
                    new MultiUserRoomSettings {LocalOrigin = pose, EmptyTimeToLive = TimeSpan.Zero});

                if (room != null)
                {
                    Debug.Log($"Successfully created room with name {room.Name}");
                }
                else
                {
                    Debug.Log($"Failed to create room with type {roomTypeId}");
                }
            }
        }
    }
}
