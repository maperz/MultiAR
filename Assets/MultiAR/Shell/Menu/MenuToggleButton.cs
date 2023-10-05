using UniRx;
using UnityEngine;
using UnityEngine.Assertions;

namespace MultiAR.Shell.Menu
{
    public class MenuToggleButton : MonoBehaviour
    {
        [Tooltip("The menu index that sets the toggle to activated")]
        public int menuIndex;

        public GameObject first;
        public GameObject second;

        private MenuService _menuService;

        void Start()
        {
            SetActivated(false);

            _menuService = GetComponentInParent<MenuService>();
            Assert.IsTrue(_menuService != null);
            _menuService.GetOpenMenuIndex().Subscribe(i => SetActivated(i == menuIndex)).AddTo(this);
        }

        private void SetActivated(bool activated)
        {
            first.SetActive(!activated);
            second.SetActive(activated);
        }
    }
}
