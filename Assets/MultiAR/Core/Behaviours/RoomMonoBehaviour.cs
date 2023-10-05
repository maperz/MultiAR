namespace MultiAR.Core.Behaviours
{
    using Models;
    using ModestTree;
    using Services.Interfaces;
    using System;
    using UniRx;
    using UnityEngine;
    using Zenject;

    public class RoomMonoBehaviour : MonoBehaviour
    {
        private readonly BehaviorSubject<MultiUserRoom> _roomState = new BehaviorSubject<MultiUserRoom>(null);

        [Inject] protected IRoomService RoomService;

        public MultiUserRoom Room
        {
            get
            {
                return _roomState.Value;
            }
        }

        public void SetRoom(MultiUserRoom room)
        {
            if (room != Room && room != null)
            {
                _roomState.OnNext(room);
                OnRoomSet(room);
            }
        }

        protected virtual void Start()
        {
            if (RoomService == null)
            {
                Debug.LogWarning(
                    $"Room updates are disabled for {nameof(RoomMonoBehaviour)} as RoomService was not injected.");
                return;
            }

            GetRoomUpdates().Subscribe(OnRoomUpdate).AddTo(this);
        }

        public IDisposable SyncRoomWithParent()
        {
            return transform.parent.GetComponentInParent<RoomMonoBehaviour>().GetRoom().Subscribe(SetRoom).AddTo(this);
        }

        public IObservable<MultiUserRoom> GetRoom()
        {
            return _roomState.AsObservable().Where(room => room != null);
        }

        public IObservable<MultiUserRoom> GetRoomUpdates()
        {
            Assert.IsNotNull(RoomService);
            return GetRoom().Select(room => RoomService.GetRoomUpdates(room)).Switch();
        }

        protected virtual void OnRoomSet(MultiUserRoom room)
        {
        }

        protected virtual void OnRoomUpdate(MultiUserRoom room)
        {
        }
    }
}
