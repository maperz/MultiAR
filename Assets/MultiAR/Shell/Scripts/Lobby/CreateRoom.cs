using System;
using MultiAR.Core.Helper;
using MultiAR.Core.Services.Implementations;
using MultiAR.Core.Services.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace MultiAR.Shell.Scripts.Lobby
{
    public class CreateRoom : MonoBehaviour
    {
        private IMultiUserService _multiUserService;

        private RoomDescription _roomDescription;

        [SerializeField] [Tooltip("This event is triggered when the anchor creation is started ")]
        private UnityEvent onStartedCreating = new UnityEvent();

        [SerializeField] [Tooltip("This event is triggered when the anchor creation is finished ")]
        private UnityEvent onFinishedCreating = new UnityEvent();

        private void Start()
        {
            _multiUserService = FindObjectOfType<MultiUserService>();
            if (_multiUserService == null)
            {
                throw new Exception("MultiARSystem could not be found - Please add the prefab to the scene");
            }
        }

        public void SetRoomDescription(RoomDescription roomDescription)
        {
            _roomDescription = roomDescription;
        }

        public async void Create()
        {
            try
            {
                // For now we generate a room name
                var roomName = $"{_roomDescription.title} #{Random.Range(0, 10000)}";
                var cachedTransform = transform;

                Instantiate(_roomDescription.model, cachedTransform);

                OnAnchorPlacingStarted();

                var room = await _multiUserService
                    .CreateRoom(roomName, _roomDescription.typeId,
                        new Pose(cachedTransform.position, cachedTransform.rotation))
                    .WithTimeout(TimeSpan.FromSeconds(60));

                if (room == null)
                {
                    OnAnchorPlacingFailed();
                }

                OnAnchorPlacingFinished();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in DoAnchorPlacement: {e}");
                OnAnchorPlacingFailed();
                throw;
            }
        }

        private void OnAnchorPlacingStarted()
        {
            //Debug.Log("On anchor placing started");
            onStartedCreating?.Invoke();
        }

        private void OnAnchorPlacingFailed()
        {
            Debug.LogError("On anchor placing failed");
            onFinishedCreating?.Invoke();
        }

        private void OnAnchorPlacingFinished()
        {
            //Debug.Log("On anchor placed finished");
            onFinishedCreating?.Invoke();
        }
    }
}
