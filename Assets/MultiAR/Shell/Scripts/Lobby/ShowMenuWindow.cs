using UnityEngine;

namespace MultiAR.Shell.Scripts.Lobby
{
    using Microsoft.MixedReality.Toolkit.UI;
    using UniRx;

    [RequireComponent(typeof(LobbyMenu))]
    public class ShowMenuWindow : MonoBehaviour
    {
        public ActivatedMenu Menu;

        public Interactable Button;
        public GameObject Target;

        private void Start()
        {
            GetComponent<LobbyMenu>().IsActive(Menu).Subscribe(OnActivatedChanged).AddTo(this);
        }

        private void OnActivatedChanged(bool activated)
        {
            Button.IsToggled = activated;

            if (Target != null)
            {
                Target.SetActive(activated);
            }
        }
    }
}
