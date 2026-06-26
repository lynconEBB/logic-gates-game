using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LogicGatesGame.Scripts
{
    /// <summary>
    /// Populates the main-menu name page with the latest player names from
    /// Supabase. Shows a spinner while loading, an error state on failure, and
    /// a refresh button to re-fetch. Selecting a name hands it to the
    /// <see cref="MainMenuManager"/>.
    /// </summary>
    public class PlayerNameListController : MonoBehaviour
    {
        [SerializeField] private MainMenuManager menuManager;
        [SerializeField] private SupabaseConfig supabaseConfig;
        [SerializeField] private Transform nameButtonContainer;
        [SerializeField] private PlayerNameButton nameButtonPrefab;
        [SerializeField] private GameObject loadingPage;
        [SerializeField] private GameObject errorState;
        [SerializeField] private Button refreshButton;
        [SerializeField] private int nameCount = 5;

        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
        private bool _isLoading;

        private void OnEnable()
        {
            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshNames);

            RefreshNames();
        }

        private void OnDisable()
        {
            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(RefreshNames);
        }

        public async void RefreshNames()
        {
            if (_isLoading)
                return;

            SetLoading(true);
            ClearButtons();

            try
            {
                List<string> names = await SupabasePlayersClient.FetchLatestNamesAsync(supabaseConfig, nameCount);
                PopulateButtons(names);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[PlayerNameListController] Failed to load player names: {exception.Message}");
                if (errorState != null)
                    errorState.SetActive(true);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void PopulateButtons(List<string> names)
        {
            if (nameButtonPrefab == null || nameButtonContainer == null)
                return;

            foreach (string playerName in names)
            {
                PlayerNameButton button = Instantiate(nameButtonPrefab, nameButtonContainer);
                _spawnedButtons.Add(button.gameObject);
                button.Setup(playerName, menuManager.OnNameSelected);
            }
        }

        private void ClearButtons()
        {
            foreach (GameObject button in _spawnedButtons)
            {
                if (button != null)
                    Destroy(button);
            }

            _spawnedButtons.Clear();
        }

        private void SetLoading(bool loading)
        {
            _isLoading = loading;

            if (loadingPage != null)
                loadingPage.SetActive(loading);

            if (errorState != null && loading)
                errorState.SetActive(false);

            if (refreshButton != null)
                refreshButton.interactable = !loading;
        }
    }
}
