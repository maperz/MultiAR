using TMPro;
using UnityEngine;

namespace MultiAR.Shell.Scripts.Room
{
    public class UsernameTextBanner : MonoBehaviour
    {
        public TextMeshPro textUi;
        public MeshRenderer backplate;

        private Material _backplateMaterial;

        private void OnDestroy()
        {
            if (_backplateMaterial)
            {
                Destroy(_backplateMaterial);
            }
        }

        public void SetUserInfo(string username, Color color)
        {
            if (_backplateMaterial)
            {
                Destroy(_backplateMaterial);
            }

            _backplateMaterial = new Material(backplate.material);
            backplate.material = _backplateMaterial;

            textUi.SetText(username);
            _backplateMaterial.color = color;
        }
    }
}
