using UnityEngine;

namespace LogicGatesGame.Scripts
{
    /// <summary>
    /// Drives the two main-menu pages. Switching pages just toggles which
    /// RectTransform is active. Page 1 is the player-name selection; page 2 is
    /// the existing difficulty selection that starts the game.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private RectTransform namePage;
        [SerializeField] private RectTransform difficultyPage;

        private void Start()
        {
            ShowNamePage();
        }

        public void ShowNamePage()
        {
            SetPageActive(namePage, true);
            SetPageActive(difficultyPage, false);
        }

        public void ShowDifficultyPage()
        {
            SetPageActive(namePage, false);
            SetPageActive(difficultyPage, true);
        }

        public void OnNameSelected(string playerName)
        {
            PlayerSession.SelectedPlayerName = playerName;
            ShowDifficultyPage();
        }

        private static void SetPageActive(RectTransform page, bool active)
        {
            if (page != null)
                page.gameObject.SetActive(active);
        }
    }
}
