namespace MultiAR.Shell.Scripts.Lobby
{
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Core.Models;
    using Core.Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using UniRx;
    using UnityEngine;
    using Zenject;

    public class RoomCardList : MonoBehaviour
    {
        public GridObjectCollection grid;
        public GameObject cardPrefab;

        [Inject] private readonly IRoomService _roomService;

        private bool _updateRequired;

        private void Start()
        {
            _roomService.GetRooms().Subscribe(rooms =>
            {
                foreach (Transform child in grid.transform)
                {
                    Destroy(child.gameObject);
                }

                foreach (MultiUserRoom room in rooms)
                {
                    CreateEntry(room);
                }
                _updateRequired = true;
            }).AddTo(this);
        }

        private void Update()
        {
            if (_updateRequired)
            {
                grid.UpdateCollection();
                _updateRequired = false;
            }
        }

        private RoomCard CreateEntry(MultiUserRoom room)
        {
            var entry = Instantiate(cardPrefab, grid.transform);
            var card = entry.GetComponent<RoomCard>();
            card.SetRoom(room);
            return card;
        }
    }
}
