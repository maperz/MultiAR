namespace MultiAR.Shell.Scripts.Pointer
{
    using System;

    public interface IPointerService
    {
        public void SetSharedPointerEnabled(bool enabled);

        public void ToggleSharedPointerEnabled();

        public IObservable<bool> IsSharedPointerEnabled();
    }
}
