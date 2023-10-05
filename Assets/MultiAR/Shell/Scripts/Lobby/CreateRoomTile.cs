using UnityEngine;

namespace MultiAR.Shell.Scripts.Lobby
{
    using Placer;
    using TMPro;
    using Zenject;

    public class CreateRoomTile : MonoBehaviour
    {
        [SerializeField] private RoomDescription room;

        private LobbyMenu _lobbyMenu;

        [SerializeField] private TextMeshPro titleText;
        [SerializeField] private TextMeshPro descriptionText;
        [SerializeField] private GameObject modelSlot;

        [Inject] private IPlacementService _placementService;

        private void Start()
        {
            _lobbyMenu = GetComponentInParent<LobbyMenu>();

            titleText.SetText(room.title);
            descriptionText.SetText(room.description);
            if (room.model)
            {
                Instantiate(room.model, modelSlot.transform);
            }
        }

        public void OnClick()
        {
            var placementObject = Instantiate(room.placerPrefab);

            var createRoomScript =
                placementObject.GetComponent<CreateRoom>() ?? placementObject.AddComponent<CreateRoom>();
            createRoomScript.SetRoomDescription(room);

            var placedListener = placementObject.GetComponent<PlacedListener>();
            if (placedListener == null)
            {
                placedListener = placementObject.AddComponent<PlacedListener>();
                placedListener.onPlaced.AddListener(() =>
                {
                    placedListener.onPlaced.RemoveAllListeners();
                    createRoomScript.Create();
                });
            }

            _placementService.StartPlacing(placementObject);

            if (_lobbyMenu)
            {
                _lobbyMenu.CloseMenu();
            }
        }
    }
}
