namespace MultiAR.Shell.Scripts.Lobby
{
    using Core.Behaviours;
    using Core.Models;
    using Core.Services.Interfaces;
    using System.Text;
    using TMPro;
    using Zenject;

    public class RoomInfoHUD : RoomMonoBehaviour
    {
        public TextMeshPro nameText;
        public TextMeshPro statusText;
        public TextMeshPro descriptionText;

        private RoomDescription _roomDescription;

        [Inject] private IRoomDescriptionService _roomDescriptionService;

        protected override void Start()
        {
            base.Start();
            SyncRoomWithParent();
        }

        protected override void OnRoomUpdate(MultiUserRoom room)
        {
            _roomDescription = _roomDescriptionService.GetRoomDescription(Room);

            if (_roomDescription != null && descriptionText != null)
            {
                descriptionText.SetText(_roomDescription.description);
            }

            nameText.SetText(Room.Name);

            var statusBuilder = new StringBuilder();
            statusBuilder.Append(Room.UserCount > 0
                ? $"<color=#00FF00>{Room.UserCount} / {Room.UserLimit}</color>"
                : $"{Room.UserCount} / {Room.UserLimit}");

            statusBuilder.Append($" - {Room.CreationDate.ToShortDateString()}");
            statusText.SetText(statusBuilder.ToString());
        }
    }
}
