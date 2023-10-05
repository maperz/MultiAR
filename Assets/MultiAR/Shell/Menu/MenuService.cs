using UnityEngine;

namespace MultiAR.Shell.Menu
{
    using System;
    using UniRx;

    public class MenuService : MonoBehaviour
    {
        [SerializeField] private DeviceIndependentMenu[] menus;

        private readonly BehaviorSubject<int?> _openMenuIndex = new BehaviorSubject<int?>(null);

        public void OnEnable()
        {
            HideCurrentMenu();
        }

        public void ShowMenu(int index)
        {
            if (menus.Length <= index)
            {
                throw new ArgumentOutOfRangeException();
            }

            _openMenuIndex.OnNext(index);
        }

        public void HideCurrentMenu()
        {
            _openMenuIndex.OnNext(null);
        }

        public IObservable<int?> GetOpenMenuIndex()
        {
            return _openMenuIndex.DistinctUntilChanged();
        }

        public IObservable<DeviceIndependentMenu> GetOpenMenu()
        {
            return _openMenuIndex.Select(i => i != null ? menus[i.Value] : null);
        }
    }
}
