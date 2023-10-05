namespace MultiAR.Shell.Menu
{
    using Microsoft.MixedReality.Toolkit;
    using Core.Services.Interfaces;
    using System.Linq;
    using UniRx;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class DeviceIndependentMenu : MonoBehaviour
    {
        [SerializeField] private GameObject screenBasedOutlet;

        [SerializeField] private GameObject hololensOutlet;

        [SerializeField] private GameObject menu;

        private bool _isScreenBased;
        private MenuService _menuService;

        private void Start()
        {
            _menuService = GetComponentInParent<MenuService>();
            Assert.IsTrue(_menuService != null);

            var deviceTypeService = MixedRealityToolkit.Instance.GetService<IDeviceTypeService>();
            _isScreenBased = deviceTypeService.IsDeviceScreenBased();

            AddMenuTo(hololensOutlet);

            _menuService.GetOpenMenu().Where(openMenu => openMenu == this).Subscribe(_ => SetMenuActive(true))
                .AddTo(this);
            _menuService.GetOpenMenu().Where(openMenu => openMenu == null).Subscribe(_ => SetMenuActive(false))
                .AddTo(this);
        }

        private void SetMenuActive(bool active)
        {
            if (_isScreenBased)
            {
                ClearOutlet(screenBasedOutlet);
                if (active)
                {
                    AddMenuTo(screenBasedOutlet);
                }
            }
        }

        private void AddMenuTo(GameObject outlet)
        {
            var menuObject = Instantiate(menu, outlet.transform);
            menuObject.transform.localPosition = Vector3.zero;
        }

        private void ClearOutlet(GameObject outlet)
        {
            var children = (from Transform child in outlet.transform select child.gameObject).ToList();
            children.ForEach(Destroy);
        }
    }
}
