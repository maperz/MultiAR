namespace MultiAR.Core.Services.Implementations
{
    using Interfaces;
    using Models;
    using Shell.Scripts.Lobby;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class RoomDescriptionService : MonoBehaviour, IRoomDescriptionService
    {
        public RoomDescription[] descriptions = Array.Empty<RoomDescription>();

        private IList<RoomDescription> _loadedDescriptions;

        public void OnEnable()
        {
            _loadedDescriptions = descriptions.Where(IsValidRoomDescription).ToList();

#if UNITY_EDITOR
            CheckIfAllRoomDescriptionsAreIncluded();
#endif
        }

        public RoomDescription GetRoomDescription(string typeId)
        {
            var room = _loadedDescriptions.FirstOrDefault(room => room.typeId == typeId);
            if (room == null)
            {
                Debug.LogError(
                    $"No room found for Type '{typeId}'. Are you sure you registered the {nameof(RoomDescription)} on the service?");
            }

            return room;
        }

        public RoomDescription GetRoomDescription(MultiUserRoom room)
        {
            return GetRoomDescription(room.TypeId);
        }

        private static bool IsValidRoomDescription(RoomDescription description)
        {
            if (string.IsNullOrWhiteSpace(description.typeId) || description.typeId.Length < 3)
            {
                Debug.LogWarning(
                    $"Room '{description.typeId}' has an invalid field: {nameof(RoomDescription.typeId)}.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(description.title) || description.title.Length < 3)
            {
                Debug.LogWarning($"Room '{description.typeId}' has an invalid title.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(description.sceneName))
            {
                Debug.LogWarning($"Room '{description.typeId}' has an invalid scene.");
                return false;
            }

            return true;
        }


#if UNITY_EDITOR

        private void CheckIfAllRoomDescriptionsAreIncluded()
        {
            var allResourceDescriptions = AssetDatabase.FindAssets($"t: {nameof(RoomDescription)}").ToList()
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<RoomDescription>);

            var noResourceDescriptions = allResourceDescriptions
                .Where(description => !_loadedDescriptions.Contains(description))
                .ToList();

            foreach (var description in noResourceDescriptions)
            {
                Debug.LogWarning(
                    $"{nameof(RoomDescription)} not valid! Please add {nameof(RoomDescription)} with name {description.name} to this service.");
            }

            if (!noResourceDescriptions.Any())
            {
                Debug.Log($"All {nameof(RoomDescription)}s are correctly registered.");
            }
        }
#endif
    }
}
