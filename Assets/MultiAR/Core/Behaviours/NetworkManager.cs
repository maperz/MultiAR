using System.Collections.Generic;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiAR.Core.Behaviours
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        private const string Version = "1.0.0";

        [Tooltip("A scene that should be shown when successfully connected to a room")]
        public string roomSceneName;

        private Scene? _sceneBeforeConnected;

        [CanBeNull] public string playerName;

        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            if (PhotonNetwork.IsConnected) return;

            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = $"{SystemInfo.deviceName} ({SystemInfo.operatingSystem})" + Random.Range(0, 1000);
            }

            PhotonNetwork.NickName = playerName;

            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = Version;
            PhotonNetwork.ConnectUsingSettings();
            DontDestroyOnLoad(this);
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log($"Connected to PUN master - Local player nickname: {PhotonNetwork.LocalPlayer.NickName}");
            PhotonNetwork.JoinRandomOrCreateRoom();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (var roomInfo in roomList)
            {
                Debug.Log("Room exists: " + roomInfo.Name);
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"OnFailedToConnectToPhoton. StatusCode: {cause}, ServerAddress: {PhotonNetwork.ServerAddress}");
            OnRoomLost();
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Room joined successfully");
            OnRoomReady();
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("Room was created successfully");
            OnRoomReady();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"New player {newPlayer.ActorNumber} joined room with name {newPlayer.NickName}");
        }

        public override void OnPlayerLeftRoom(Player newPlayer)
        {
            Debug.Log($"Player {newPlayer.ActorNumber} left room with name {newPlayer.NickName}");
        }

        private void OnRoomReady()
        {
            if (roomSceneName != null)
            {
                _sceneBeforeConnected = SceneManager.GetActiveScene();
                SceneManager.LoadSceneAsync(roomSceneName);
            }
        }

        private void OnRoomLost()
        {
            if (_sceneBeforeConnected != null)
            {
                SceneManager.LoadScene(_sceneBeforeConnected?.name);
                _sceneBeforeConnected = null;
            }
        }
    }
}
