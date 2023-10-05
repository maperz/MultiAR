using UnityEngine;

namespace MultiAR.Shell.Scripts.Room
{
    using Core.Services.Interfaces;
    using UniRx;
    using UnityEngine.Events;
    using Zenject;

    public class RoomEvents : MonoBehaviour
    {
        public UnityEvent onRoomJoined = new UnityEvent();
        public UnityEvent onRoomLeft = new UnityEvent();

        [Inject] private readonly IMultiUserService _multiUserService;

        private void Start()
        {
            _multiUserService.HasActiveRoom().DistinctUntilChanged().Skip(1).Subscribe(ActiveRoomChanged).AddTo(this);
        }

        private void ActiveRoomChanged(bool active)
        {
            if (active)
            {
                onRoomJoined?.Invoke();
            }
            else
            {
                onRoomLeft?.Invoke();
            }
        }
    }
}
