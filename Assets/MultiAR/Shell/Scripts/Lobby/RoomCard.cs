using MultiAR.Core.Models;
using MultiAR.Core.Services.Interfaces;
using System.Text;
using TMPro;
using UnityEngine;
using Zenject;

namespace MultiAR.Shell.Scripts.Lobby
{
    using Core.Behaviours;
    using Core.Helper.Extensions;
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.Rendering;
    using Placer;

    public class RoomCard : RoomMonoBehaviour, IMaterialInstanceOwner
    {
        public TextMeshPro title;
        public TextMeshPro info;

        [Inject] private IPlacementService _placementService;
        [Inject] private IRoomDescriptionService _descriptionService;

        public GameObject roomPlacementPrefab;

        [Tooltip("The renderer that shall be updated")] [SerializeField]
        private Renderer targetRenderer;

        public GameObject roomModelSlot;


        protected override void OnRoomUpdate(MultiUserRoom room)
        {
            title.SetText(room.Name);
            info.SetText(GetInfoText(room));

            var description = _descriptionService.GetRoomDescription(room);
            if (description && description.model)
            {
                roomModelSlot.transform.DestroyAllChildren();
                Instantiate(description.model, roomModelSlot.transform);
            }

            if (targetRenderer != null && Room.Color != null)
            {
                Material material = targetRenderer.EnsureComponent<MaterialInstance>().AcquireMaterial(this);
                material.color = Room.Color.Value;
            }
        }

        private string GetInfoText(MultiUserRoom room)
        {
            var infoTextBuilder = new StringBuilder();
            infoTextBuilder.Append(room.UserCount > 0
                ? $"<color=#00FF00>{room.UserCount} / {room.UserLimit}</color>"
                : $"{room.UserCount} / {room.UserLimit}");

            infoTextBuilder.Append($" - {room.CreationDate.ToShortDateString()}");
            return infoTextBuilder.ToString();
        }

        public void OnClick()
        {
            var placementObject = Instantiate(roomPlacementPrefab);
            placementObject.GetComponent<RoomLobbyEntry>().SetRoom(Room);
            _placementService.StartPlacing(placementObject);

            GetComponentInParent<LobbyMenu>()?.CloseMenu();
        }

        public void OnMaterialChanged(MaterialInstance materialInstance)
        {
        }
    }
}
