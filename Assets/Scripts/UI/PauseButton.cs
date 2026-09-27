using UnityEngine;
using UnityEngine.UI;
using RogueLike.Core;

namespace RogueLike.UI
{
    [RequireComponent(typeof(Button))]
    public class PauseButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            GameManager.Instance?.TogglePause();
        }
    }
}
