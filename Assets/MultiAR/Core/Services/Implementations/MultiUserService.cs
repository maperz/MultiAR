using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SceneSystem;
using MultiAR.Core.Models;
using MultiAR.Core.Services.Interfaces;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;

namespace MultiAR.Core.Services.Implementations
{
    using Helper;
    using Zenject;

    internal static class RoomPropertyNames
    {
        public const string Color = "color";
        public const string AnchorId = "anchorId";
        public const string CreationDate = "creationDate";
        public const string Boundaries = "boundaries";
        public const string TypeId = "typeId";

        public static readonly string[] AllProperties = { Color, AnchorId, CreationDate, Boundaries, TypeId };
    }

    [RequireComponent(typeof(AsaColocateService), typeof(ActiveRoomService), typeof(RoomDescriptionService))]
    public class MultiUserService : MonoBehaviourPunCallbacks, IMultiUserService
    {
        private const string Version = "1.0.0";

        public string lobbySceneName;
        public string disconnectedSceneName;

        private readonly MultiUserRoomSettings _defaultSettings = new MultiUserRoomSettings()
        {
            UserLimit = 8, EmptyTimeToLive = TimeSpan.FromMinutes(5), Boundaries = null, Color = null,
        };

        public event IMultiUserService.RoomsChangedHandler RoomsChanged;
        public event IMultiUserService.RoomsChangedHandler RoomsAdded;
        public event IMultiUserService.RoomsChangedHandler RoomsRemoved;

        public event Action OnInitialized;

        private readonly IUserService _userService = new UserService();
        private IColocateService<string> _colocateService;

        private bool _receivedRoomUpdate;

        private readonly Dictionary<string, MultiUserRoom> _roomListCache = new Dictionary<string, MultiUserRoom>();
        private readonly Subject<int> _onRoomsChanged = new Subject<int>();

        private TaskCompletionSource<bool> _initTaskSource;

        public bool initOnStart;
        private bool _isInitialized;

        private IMixedRealitySceneSystem _sceneSystem;
        private IDeviceTypeService _deviceTypeService;

        [Inject] private IRoomDescriptionService _roomDescriptionService;

        private ActiveMultiUserRoom _activeRoom;

        private readonly BehaviorSubject<ActiveMultiUserRoom> _activeRoomSubject =
            new BehaviorSubject<ActiveMultiUserRoom>(null);

        private void Start()
        {
            _sceneSystem = MixedRealityToolkit.Instance.GetService<IMixedRealitySceneSystem>();
            _deviceTypeService = MixedRealityToolkit.Instance.GetService<IDeviceTypeService>();
            _colocateService = GetComponent<AsaColocateService>();
            DontDestroyOnLoad(this);
        }

        private async void Update()
        {
            if (!IsInitialized() && initOnStart)
            {
                initOnStart = false;
                await InitInternal();
            }
        }

        public IObservable<MultiUserRoom[]> GetRooms()
        {
            var roomStream = _onRoomsChanged
                .Select(_ => GetCurrentRooms().ToArray());

            if (_receivedRoomUpdate)
            {
                return roomStream.StartWith(GetCurrentRooms().ToArray);
            }

            return roomStream;
        }

        public async Task Init()
        {
            if (IsInitialized() || initOnStart)
            {
                Debug.LogWarning("Already initialized");
                return;
            }

            initOnStart = true;
            _initTaskSource = new TaskCompletionSource<bool>();
            if (_colocateService != null)
            {
                await InitInternal();
            }

            await _initTaskSource.Task;
        }

        public bool IsInitialized()
        {
            return _isInitialized;
        }

        public User GetLocalUser()
        {
            var user = User.FromPhotonPlayer(PhotonNetwork.LocalPlayer);
            user.Colocated = _activeRoom?.Colocated ?? false;
            return user;
        }

        public IObservable<bool> HasActiveRoom()
        {
            return _activeRoomSubject.Select(room => room != null).DistinctUntilChanged();
        }

        private async Task InitInternal()
        {
            Debug.Log("MultiUserService Initialization started");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.NickName = await _userService.GetUserName();

            UpdatePlayerProperties(new Hashtable { { "deviceType", _deviceTypeService.GetDeviceType() } });

            if (PhotonNetwork.IsConnected) return;

            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Version;
            PhotonNetwork.ConnectUsingSettings();
        }

        private void UpdatePlayerProperties(Hashtable newProperties)
        {
            PhotonNetwork.SetPlayerCustomProperties(newProperties);
            //Debug.Log("Set Player properties: " + (result ? "Successful" : "Not successful"));
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log($"Connected to Photon - Username: {PhotonNetwork.LocalPlayer.NickName}");
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }

        public override void OnJoinedLobby()
        {
            //Debug.Log("Joined Lobby");

            if (!_isInitialized)
            {
                //Debug.Log("Finished MultiUserService initialization");
                _isInitialized = true;
                _initTaskSource?.SetResult(true);

                if (_colocateService.IsSupported())
                {
                    _colocateService.AddInitializedListener(() => OnInitialized?.Invoke());
                    _colocateService.Init();
                }
                else
                {
                    OnInitialized?.Invoke();
                }
            }

            _ = EnterLobbyScene();
        }

        public override async void OnDisconnected(DisconnectCause cause)
        {
            if (cause != DisconnectCause.DisconnectByClientLogic)
            {
                Debug.Log(
                    $"OnFailedToConnectToPhoton. StatusCode: {cause}, ServerAddress: {PhotonNetwork.ServerAddress}");
                await _sceneSystem.LoadContent(disconnectedSceneName, LoadSceneMode.Single);
                return;
            }

            Debug.Log("On disconnect called with cause: " + cause);
            if (_activeRoom != null)
            {
                await EnterLobbyScene();
            }
        }

        public override void OnJoinedRoom()
        {
            var colocated = _activeRoom.Colocated;
            Debug.Log("Room joined successfully. Colocated = " + colocated);
            UpdatePlayerProperties(new Hashtable { { "colocated", colocated } });

            var roomDescription = _roomDescriptionService.GetRoomDescription(_activeRoom);
            EnterRoomScene(roomDescription.sceneName);
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("Room was created successfully");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            var removedRooms = new List<MultiUserRoom>();
            var addedRooms = new List<MultiUserRoom>();
            var updatedRooms = new List<MultiUserRoom>();

            foreach (var roomInfo in roomList)
            {
                if (roomInfo.RemovedFromList && _roomListCache.TryGetValue(roomInfo.Name, out var room))
                {
                    _roomListCache.Remove(roomInfo.Name);
                    removedRooms.Add(room);
                }
                else
                {
                    room = ConvertFromPhotonRoomInfo(roomInfo);
                    var added = !_roomListCache.ContainsKey(roomInfo.Name);
                    _roomListCache[roomInfo.Name] = room;
                    if (added)
                    {
                        addedRooms.Add(room);
                    }
                    else
                    {
                        updatedRooms.Add(room);
                    }
                }
            }

            RoomsChanged?.Invoke(updatedRooms);
            RoomsAdded?.Invoke(addedRooms);
            RoomsRemoved?.Invoke(removedRooms);

            _onRoomsChanged.OnNext(roomList.Count);
            _receivedRoomUpdate = true;
        }

        public IEnumerable<MultiUserRoom> GetCurrentRooms()
        {
            return _roomListCache.Values;
        }

        public Task<MultiUserRoom> CreateRoom(string roomName, string typeId, Pose localOrigin)
        {
            return CreateRoom(roomName, typeId, new MultiUserRoomSettings() { LocalOrigin = localOrigin });
        }

        public async Task<MultiUserRoom> CreateRoom(string roomName, string typeId, MultiUserRoomSettings settings)
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                Debug.LogError("Cannot create room when not connected to Photon");
                throw new Exception("Service not ready yet");
            }

            try
            {
                var roomDescription = _roomDescriptionService.GetRoomDescription(typeId);

                var room = new MultiUserRoom()
                {
                    Name = roomName,
                    CreationDate = DateTime.UtcNow,
                    TypeId = typeId,
                    Boundaries = settings.Boundaries ?? _defaultSettings.Boundaries,
                    UserLimit = settings.UserLimit ?? _defaultSettings.UserLimit,
                    Color = settings.Color ?? ColorHelper.RandomFlatColor()
                };

                var timeToLive = settings.EmptyTimeToLive ?? _defaultSettings.EmptyTimeToLive;

                if (_colocateService.IsSupported())
                {
                    var anchorId = await _colocateService.CreateAnchor(settings.LocalOrigin, TimeSpan.FromDays(7));
                    if (anchorId == null)
                    {
                        Debug.LogWarning("Failed to create local origin anchor");
                    }
                    else
                    {
                        room.AnchorId = anchorId;
                    }
                }


                var emptyTtl = Math.Min((int)(timeToLive?.TotalMilliseconds ?? TimeSpan.FromHours(1).TotalMilliseconds),
                    300000);

                var roomOptions = new RoomOptions
                {
                    EmptyRoomTtl = roomDescription.destroyRoomWhenEmpty ? 0 : emptyTtl,
                    MaxPlayers = room.UserLimit ?? 8,
                    CustomRoomProperties = MultiUserRoom.SerializeRoomProperties(room),
                    CustomRoomPropertiesForLobby = RoomPropertyNames.AllProperties,
                    IsOpen = true,
                    IsVisible = true,
                };

                var isColocated = !string.IsNullOrEmpty(room.AnchorId);
                if (PhotonNetwork.CreateRoom(room.Name, roomOptions))
                {
                    //Debug.Log("Created room with name " + room.Name + " and AnchorId: " + room.AnchorId);
                    _activeRoom = new ActiveMultiUserRoom(room, settings.LocalOrigin, isColocated);
                    return room;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create room: {e}");
            }

            return null;
        }

        public void JoinRoom(MultiUserRoom room, Pose pose, bool colocated)
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                throw new Exception("Service not ready yet");
            }

            _activeRoom = new ActiveMultiUserRoom(room, pose, colocated);
            PhotonNetwork.JoinRoom(room.Name);
        }


        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError("Failed to join room: " + message);
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public ActiveMultiUserRoom GetActiveRoom()
        {
            return _activeRoom;
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError("Failed to create room: " + message);
        }

        private static MultiUserRoom ConvertFromPhotonRoomInfo(RoomInfo roomInfo)
        {
            var room = new MultiUserRoom()
            {
                Name = roomInfo.Name, UserCount = roomInfo.PlayerCount, UserLimit = roomInfo.MaxPlayers,
            };

            MultiUserRoom.DeserializeRoomProperties(roomInfo.CustomProperties, room);
            return room;
        }


        public override void OnLeftRoom()
        {
            // Don't do anything here as we are executing stuff in OnJoinedLobby
        }

        private async void EnterRoomScene(string sceneName)
        {
            using (var _ = new PhotonMessageQueueBreak())
            {
                await _sceneSystem.LoadContent(sceneName, LoadSceneMode.Single);
            }

            CoreServices.SpatialAwarenessSystem.SuspendObservers();

            PushActiveRoomChanged();
        }

        private async Task EnterLobbyScene()
        {
            using (var _ = new PhotonMessageQueueBreak())
            {
                await _sceneSystem.LoadContent(lobbySceneName, LoadSceneMode.Single);
            }

            CoreServices.SpatialAwarenessSystem.ResumeObservers();

            _activeRoom = null;
            PushActiveRoomChanged();
        }

        private void PushActiveRoomChanged()
        {
            _activeRoomSubject.OnNext(_activeRoom);
        }
    }
}
