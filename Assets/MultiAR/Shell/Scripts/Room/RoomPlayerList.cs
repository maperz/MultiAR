using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiAR.Core.Models;
using MultiAR.Core.Services.Implementations;
using MultiAR.Core.Services.Interfaces;
using TMPro;
using UnityEngine;

namespace MultiAR.Shell.Scripts.Room
{
    public class RoomPlayerList : MonoBehaviour
    {
        public TextMeshPro roomNameText;
        public TextMeshPro userList;

        private IActiveRoomService _activeRoomService;

        private void Start()
        {
            _activeRoomService = FindObjectOfType<ActiveRoomService>();
            if (_activeRoomService == null)
            {
                throw new Exception("ActiveRoomService could not be found - Please add the prefab to the scene");
            }

            _activeRoomService.OnUserListChanged += SetUsers;

            SetRoom(_activeRoomService.GetActiveRoom());
            SetUsers(_activeRoomService.GetUsers());
        }

        private void OnDestroy()
        {
            if (_activeRoomService != null)
            {
                _activeRoomService.OnUserListChanged -= SetUsers;
            }
        }

        private void SetRoom(MultiUserRoom room)
        {
            roomNameText.SetText(room.Name);
        }

        private void SetUsers(IEnumerable<MultiAR.Core.Models.User> users)
        {
            var sortedUsers = users.OrderBy(u => u.Id);
            var textBuilder = new StringBuilder();
            foreach (var user in sortedUsers)
            {
                var color = ColorUtility.ToHtmlStringRGB(user.Color);
                textBuilder.AppendLine($"#{user.Id} - <color=#{color}>{user.Name}</color>");
            }

            userList.SetText(textBuilder.ToString() ?? "");
        }
    }
}
