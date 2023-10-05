using UnityEngine;

namespace MultiAR.Shell.Scripts.Lobby
{
    using System;
    using UniRx;

    public enum ActivatedMenu
    {
        None,
        RoomList,
        CreateRoom,
        Settings,
        DebugWindow
    }

    public class LobbyMenu : MonoBehaviour
    {
        private readonly BehaviorSubject<ActivatedMenu> _activeMenu =
            new BehaviorSubject<ActivatedMenu>(ActivatedMenu.None);

        public IObservable<bool> IsRoomListActive()
        {
            return IsActive(ActivatedMenu.RoomList);
        }

        public IObservable<bool> IsCreateRoomActive()
        {
            return IsActive(ActivatedMenu.CreateRoom);
        }

        public IObservable<bool> IsSettingsActive()
        {
            return IsActive(ActivatedMenu.Settings);
        }

        public IObservable<bool> IsDebugWindowActive()
        {
            return IsActive(ActivatedMenu.DebugWindow);
        }

        public IObservable<bool> IsActive(ActivatedMenu menu)
        {
            return _activeMenu.Select(activeMenu => menu == activeMenu).DistinctUntilChanged();
        }

        public void CloseMenu()
        {
            _activeMenu.OnNext(ActivatedMenu.None);
        }

        public void ToggleActiveMenu(ActivatedMenu menu)
        {
            if (_activeMenu.Value != menu)
            {
                _activeMenu.OnNext(menu);
            }
            else
            {
                _activeMenu.OnNext(ActivatedMenu.None);
            }
        }

        public void ActivateRoomList()
        {
            ToggleActiveMenu(ActivatedMenu.RoomList);
        }

        public void ActivateCreateRoom()
        {
            ToggleActiveMenu(ActivatedMenu.CreateRoom);
        }

        public void ActivateSettings()
        {
            ToggleActiveMenu(ActivatedMenu.Settings);
        }

        public void ActivateDebugWindow()
        {
            ToggleActiveMenu(ActivatedMenu.DebugWindow);
        }
    }
}
