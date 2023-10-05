namespace MultiAR.Shell.Scripts.Pointer
{
    using Microsoft.MixedReality.Toolkit.UI;
    using UniRx;
    using UnityEngine;
    using Zenject;

    [RequireComponent(typeof(Interactable))]
    public class SharedPointerController : MonoBehaviour
    {
        private Interactable _interactable;

        [Inject] private IPointerService _pointerService;

        public void Start()
        {
            _interactable = GetComponent<Interactable>();
            _interactable.OnClick.AddListener(ToggleSharedPointer);
            _pointerService.IsSharedPointerEnabled().Subscribe(OnSharedPointerStateChange).AddTo(this);
        }

        private void OnDestroy()
        {
            _interactable.OnClick.RemoveListener(ToggleSharedPointer);
        }

        private void ToggleSharedPointer()
        {
            _pointerService.SetSharedPointerEnabled(_interactable.IsToggled);
        }

        private void OnSharedPointerStateChange(bool enable)
        {
            _interactable.IsToggled = enable;
        }
    }
}
