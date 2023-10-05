namespace MultiAR.Shell.Scripts.Lobby
{
    using Core.Behaviours;
    using Core.Models;
    using Core.Services.Implementations;
    using Core.Services.Interfaces;
    using System;
    using System.Linq;
    using UnityEngine;
    using Zenject;

    /// <summary>
    /// This object sets a bunch of room information into different objects.
    /// The main purpose is to simplify setting multiple locations when a room update happens.
    /// </summary>
    public class RoomLobbyEntry : RoomMonoBehaviour
    {
        private const string ModelName = "Model";
        private const string ModelCenterName = "Center";

        public MeshRenderer marker;
        public GameObject modelSlot;

        [Inject] private IMultiUserService _multiUserService;
        [Inject] private IRoomDescriptionService _roomDescriptionService;

        [SerializeField] private bool colocated;

        private RoomDescription _roomDescription;

        // TODO: Remove this again
        [Header("Debug")]
        public bool enableDebugging = false;
        public string debugMockTypeID = null;

        protected override void Start()
        {
            base.Start();
            if (enableDebugging && colocated && !string.IsNullOrWhiteSpace(debugMockTypeID))
            {
                // TODO: Remove this again
                var devRoom = new MultiUserRoom()
                {
                    AnchorId = "",
                    Name = "Mock-Dev-Demo",
                    Color = new Color(255, 0, 0),
                    CreationDate = DateTime.Now,
                    TypeId = debugMockTypeID,
                    UserCount = 0,
                    UserLimit = 12,
                };

                SetRoom(devRoom);
            }
        }

        protected override void OnRoomUpdate(MultiUserRoom room)
        {
            enabled = room != null;
            if (room == null)
            {
                return;
            }

            _roomDescriptionService = _roomDescriptionService ?? FindObjectOfType<RoomDescriptionService>();
            _roomDescription = _roomDescriptionService?.GetRoomDescription(room);

            gameObject.name = room.Name;
            if (room.Color != null)
            {
                marker.material.color = room.Color.Value;
            }

            LoadRoomModel();
        }

        public void OnJoinRoom()
        {
            // Debug.Log("On lobby entry clicked: " + _room.Name);
            var cachedTransform = transform;
            _multiUserService.JoinRoom(Room, new Pose(cachedTransform.position, cachedTransform.rotation), colocated);
        }

        private void LoadRoomModel()
        {
            if (_roomDescription != null && _roomDescription.placerPrefab != null && modelSlot != null)
            {
                var currentModel = GetCurrentModel();
                if (currentModel != null)
                {
                    Destroy(currentModel);
                    currentModel.name += "(Old)";
                }


                var model = Instantiate(_roomDescription.placerPrefab, modelSlot.transform.position,
                    modelSlot.transform.rotation, modelSlot.transform);
                model.name = ModelName;
                RecalculateCenter();
            }
        }

        private GameObject GetCurrentModel()
        {
            return (from Transform child in modelSlot.transform
                where child.gameObject.name == ModelName
                select child.gameObject).FirstOrDefault();
        }

        private GameObject GetCenterMarker()
        {
            return (from Transform child in modelSlot.transform
                where child.gameObject.name == ModelCenterName
                select child.gameObject).FirstOrDefault();
        }

        private void RecalculateCenter()
        {
            var target = GetCurrentModel();
            var centerMarker = GetCenterMarker();

            if (target && centerMarker)
            {
                var bounds = target.GetComponent<Collider>().bounds;
                centerMarker.transform.position = bounds.center;
            }
        }
    }
}
