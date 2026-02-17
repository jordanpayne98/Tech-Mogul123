using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using TechMogul.Core;

namespace TechMogul.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string gameSceneName = "SampleScene";

        private UIDocument _uiDocument;
        private VisualElement _root;
        
        private Button _newGameButton;
        private Button _loadGameButton;
        private Button _settingsButton;
        private Button _quitButton;
        
        void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }
        
        void OnEnable()
        {
            _root = _uiDocument.rootVisualElement;
            
            _newGameButton = _root.Q<Button>("new-game-button");
            _loadGameButton = _root.Q<Button>("load-game-button");
            _settingsButton = _root.Q<Button>("settings-button");
            _quitButton = _root.Q<Button>("quit-button");
            
            _newGameButton.RegisterCallback<ClickEvent>(OnNewGameClicked);
            _loadGameButton.RegisterCallback<ClickEvent>(OnLoadGameClicked);
            _settingsButton.RegisterCallback<ClickEvent>(OnSettingsClicked);
            _quitButton.RegisterCallback<ClickEvent>(OnQuitClicked);
        }
        
        void OnDisable()
        {
            if (_newGameButton != null)
                _newGameButton.UnregisterCallback<ClickEvent>(OnNewGameClicked);
            if (_loadGameButton != null)
                _loadGameButton.UnregisterCallback<ClickEvent>(OnLoadGameClicked);
            if (_settingsButton != null)
                _settingsButton.UnregisterCallback<ClickEvent>(OnSettingsClicked);
            if (_quitButton != null)
                _quitButton.UnregisterCallback<ClickEvent>(OnQuitClicked);
        }
        
        void OnNewGameClicked(ClickEvent evt)
        {
            Debug.Log("Starting new game");
            EventBus.Publish(new RequestStartNewGameEvent());
            SceneManager.LoadScene(gameSceneName);
        }
        
        void OnLoadGameClicked(ClickEvent evt)
        {
            Debug.Log("Load game button clicked");
            EventBus.Publish(new RequestShowSaveLoadDialogEvent { IsLoading = true });
        }
        
        void OnSettingsClicked(ClickEvent evt)
        {
            Debug.Log("Opening settings...");
            Application.OpenURL("https://youtu.be/dQw4w9WgXcQ");
        }
        
        void OnQuitClicked(ClickEvent evt)
        {
            Debug.Log("Quitting game");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
    
    public class RequestShowSaveLoadDialogEvent
    {
        public bool IsLoading;
    }
}
