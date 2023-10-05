using MultiAR.Core.Models;
using MultiAR.Core.Services.Implementations;
using MultiAR.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace MultiAR.Shell.Scripts.Lobby
{
    public class CoLocatedAnchorList : MonoBehaviour
    {
        public GameObject entryPrefab;

        private IMultiUserService _multiUserService;
        private IColocateService<string> _colocateService;

        private readonly Dictionary<string, RoomLobbyEntry> _entries = new Dictionary<string, RoomLobbyEntry>();

        private void Start()
        {
            _multiUserService = FindObjectOfType<MultiUserService>();
            if (_multiUserService == null)
            {
                throw new Exception("MultiARSystem could not be found - Please add the prefab to the scene");
            }

            _colocateService = FindObjectOfType<AsaColocateService>();
            if (_colocateService == null)
            {
                throw new Exception("AsaColocateService could not be found - Please add the prefab to the scene");
            }

            if (!_colocateService.IsSupported())
            {
                Debug.Log("Disabling AnchorList as co-location is not supported on this device");
                return;
            }

            _colocateService.OnLocated().Subscribe(OnRoomAnchorLocationUpdate).AddTo(this);

            _colocateService.AddInitializedListener(OnColocateServiceReady);
        }

        private void OnColocateServiceReady()
        {
            Debug.Log("On colocate service is ready - AnchorList listeners installed");
            _multiUserService.RoomsChanged += OnRoomsUpdated;
            _multiUserService.RoomsAdded += OnRoomsUpdated;
            _multiUserService.RoomsRemoved += OnRoomsRemoved;

            OnRoomsUpdated(_multiUserService.GetCurrentRooms());

            _colocateService.RemoveInitializedListener(OnColocateServiceReady);
        }

        private void OnDestroy()
        {
            _multiUserService.RoomsChanged -= OnRoomsUpdated;
            _multiUserService.RoomsAdded -= OnRoomsUpdated;
            _multiUserService.RoomsRemoved -= OnRoomsRemoved;

            lock (_entries)
            {
                // Debug.Log("Removing anchors as we are cleaning up");
                var rooms = _entries.Values.Select(e => e.Room);
                OnRoomsRemoved(rooms);
            }
        }

        private void OnRoomsUpdated(IEnumerable<MultiUserRoom> rooms)
        {
            // Debug.Log("Rooms updated for AnchorList");
            lock (_entries)
            {
                foreach (var room in rooms)
                {
                    // Debug.Log("Received update request for room: " + room.Name);
                    try
                    {
                        if (room.AnchorId == null)
                        {
                            // Ignore rooms without AnchorId
                            // Debug.Log("Skipping room as it is not anchored: " + room.Name);
                            continue;
                        }

                        if (_entries.TryGetValue(room.AnchorId, out var entry))
                        {
                            // Debug.Log("Updating room: " + room.Name);
                            entry.SetRoom(room);
                        }
                        else
                        {
                            OnRoomWithAnchorAdded(room);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to update room " + room.Name + " Error: " + e.Message);
                    }
                }
            }
        }

        private void OnRoomsRemoved(IEnumerable<MultiUserRoom> rooms)
        {
            lock (_entries)
            {
                foreach (var room in rooms)
                {
                    if (room.AnchorId == null)
                    {
                        // Ignore rooms without AnchorId
                        continue;
                    }

                    OnRoomWithAnchorRemoved(room);
                }
            }
        }

        private void OnRoomAnchorLocationUpdate(LocationEvent<string> locationEvent)
        {
            lock (_entries)
            {
                OnRoomAnchorLocationUpdateLocked(locationEvent.Identifier, locationEvent.Pose);
            }
        }

        private void OnRoomWithAnchorAdded(MultiUserRoom room)
        {
            Debug.Log("[+] Anchored Room added: " + room.Name);
            try
            {
                var entryObject = Instantiate(entryPrefab, transform);
                entryObject.SetActive(false);
                var lobbyEntry = entryObject.GetComponent<RoomLobbyEntry>();
                lobbyEntry.SetRoom(room);
                _entries[room.AnchorId] = lobbyEntry;
                _colocateService.StartLocating(room.AnchorId);

                var cachedLocation = _colocateService.GetCachedLocation(room.AnchorId);
                if (cachedLocation != null)
                {
                    OnRoomAnchorLocationUpdateLocked(room.AnchorId, cachedLocation.Value);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to locate anchored room: " + room.Name + ", " + e.Message);
            }
        }

        private void OnRoomWithAnchorRemoved(MultiUserRoom room)
        {
            if (!_entries.TryGetValue(room.AnchorId, out var entry))
            {
                Debug.Log("Trying to remove room that is not added: " + room.Name);
                return;
            }

            Debug.Log("[-] Anchored Room removed: " + room.Name);
            _entries.Remove(room.AnchorId);
            _colocateService.StopLocating(room.AnchorId);
            Destroy(entry.gameObject);
        }

        private void OnRoomAnchorLocationUpdateLocked(string anchorId, Pose pose)
        {
            // Get stored entry and its anchor object
            if (!_entries.TryGetValue(anchorId, out var entry))
            {
                Debug.Log("Received an update for an room that is not in the entries: " + anchorId);
                return;
            }

            // Update position of anchor object
            OnUpdateAnchorLocation(entry.Room, entry.gameObject, pose);
        }

        private void OnUpdateAnchorLocation(MultiUserRoom room, GameObject anchor, Pose pose)
        {
            Debug.Log("Anchored Room: " + room.Name + " positioned at " + pose.position);
            anchor.SetActive(true);
            anchor.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }
}
