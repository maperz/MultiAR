namespace MultiAR.Shell.Scripts.Pointer
{
    using System;
    using UniRx;

    public class PointerService : IPointerService
    {
        private readonly BehaviorSubject<bool> _networkedPointerEnabled = new BehaviorSubject<bool>(false);

        public void SetSharedPointerEnabled(bool enabled)
        {
            _networkedPointerEnabled.OnNext(enabled);
        }

        public void ToggleSharedPointerEnabled()
        {
            _networkedPointerEnabled.OnNext(!_networkedPointerEnabled.Value);
        }

        public IObservable<bool> IsSharedPointerEnabled()
        {
            return _networkedPointerEnabled.DistinctUntilChanged();
        }
    }
}
