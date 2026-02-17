using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float unsavedWarningThreshold = 300f;
        
        private UIDocument uiDocument;
        private VisualElement root;
        
        private VisualElement pauseMenu;
        private VisualElement confirmDialog;
        
        private Button resumeBtn;
        private Button newGameBtn;
        private Button saveGameBtn;
        private Button loadGameBtn;
        
        private Button confirmSaveBtn;
        private Button confirmDontSaveBtn;
        private Button confirmCancelBtn;
        private Label confirmMessage;
        
        private bool isPaused = false;
        private float lastSaveTime = 0f;
        private string pendingAction = "";
        
        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        void OnEnable()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("PauseMenuController: UIDocument or root visual element is null");
                return;
            }
            
            root = uiDocument.rootVisualElement;
            CacheReferences();
            BindButtons();
            SubscribeToEvents();
            
            HidePauseMenu();
        }
        
        void OnDisable()
        {
            UnbindButtons();
            UnsubscribeFromEvents();
        }
        
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePauseMenu();
            }
        }
        
        void CacheReferences()
        {
            pauseMenu = root.Q<VisualElement>("pause-menu");
            confirmDialog = root.Q<VisualElement>("confirm-dialog");
            
            if (pauseMenu != null)
            {
                resumeBtn = pauseMenu.Q<Button>("resume-btn");
                newGameBtn = pauseMenu.Q<Button>("new-game-btn");
                saveGameBtn = pauseMenu.Q<Button>("save-game-btn");
                loadGameBtn = pauseMenu.Q<Button>("load-game-btn");
            }
            
            if (confirmDialog != null)
            {
                confirmSaveBtn = confirmDialog.Q<Button>("confirm-save-btn");
                confirmDontSaveBtn = confirmDialog.Q<Button>("confirm-dont-save-btn");
                confirmCancelBtn = confirmDialog.Q<Button>("confirm-cancel-btn");
                confirmMessage = confirmDialog.Q<Label>("confirm-message");
            }
            
            LogMissingElements();
        }
        
        void LogMissingElements()
        {
            if (pauseMenu == null) Debug.LogWarning("pause-menu VisualElement not found");
            if (confirmDialog == null) Debug.LogWarning("confirm-dialog VisualElement not found");
        }
        
        void BindButtons()
        {
            if (resumeBtn != null) resumeBtn.clicked += OnResumeClicked;
            if (newGameBtn != null) newGameBtn.clicked += OnNewGameClicked;
            if (saveGameBtn != null) saveGameBtn.clicked += OnSaveGameClicked;
            if (loadGameBtn != null) loadGameBtn.clicked += OnLoadGameClicked;
            
            if (confirmSaveBtn != null) confirmSaveBtn.clicked += OnConfirmSaveClicked;
            if (confirmDontSaveBtn != null) confirmDontSaveBtn.clicked += OnConfirmDontSaveClicked;
            if (confirmCancelBtn != null) confirmCancelBtn.clicked += OnConfirmCancelClicked;
        }
        
        void UnbindButtons()
        {
            if (resumeBtn != null) resumeBtn.clicked -= OnResumeClicked;
            if (newGameBtn != null) newGameBtn.clicked -= OnNewGameClicked;
            if (saveGameBtn != null) saveGameBtn.clicked -= OnSaveGameClicked;
            if (loadGameBtn != null) loadGameBtn.clicked -= OnLoadGameClicked;
            
            if (confirmSaveBtn != null) confirmSaveBtn.clicked -= OnConfirmSaveClicked;
            if (confirmDontSaveBtn != null) confirmDontSaveBtn.clicked -= OnConfirmDontSaveClicked;
            if (confirmCancelBtn != null) confirmCancelBtn.clicked -= OnConfirmCancelClicked;
        }
        
        void SubscribeToEvents()
        {
            EventBus.Subscribe<OnGameSavedEvent>(HandleGameSaved);
            EventBus.Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<OnGameSavedEvent>(HandleGameSaved);
            EventBus.Unsubscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleGameSaved(OnGameSavedEvent evt)
        {
            if (evt.Success)
            {
                lastSaveTime = Time.time;
                Debug.Log("Game saved - timestamp updated");
            }
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            lastSaveTime = Time.time;
            HidePauseMenu();
        }
        
        void TogglePauseMenu()
        {
            if (isPaused)
            {
                HidePauseMenu();
            }
            else
            {
                ShowPauseMenu();
            }
        }
        
        void ShowPauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.style.display = DisplayStyle.Flex;
                isPaused = true;
                
                EventBus.Publish(new RequestChangeSpeedEvent { Speed = TimeSpeed.Paused });
                
                Debug.Log("Pause menu shown");
            }
        }
        
        void HidePauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.style.display = DisplayStyle.None;
            }
            
            if (confirmDialog != null)
            {
                confirmDialog.style.display = DisplayStyle.None;
            }
            
            isPaused = false;
            pendingAction = "";
        }
        
        void OnResumeClicked()
        {
            HidePauseMenu();
            EventBus.Publish(new RequestChangeSpeedEvent { Speed = TimeSpeed.Normal });
            Debug.Log("Resumed game");
        }
        
        void OnNewGameClicked()
        {
            if (HasUnsavedChanges())
            {
                ShowConfirmDialog("Start a new game? You have unsaved progress.", "new-game");
            }
            else
            {
                ConfirmNewGame();
            }
        }
        
        void OnSaveGameClicked()
        {
            EventBus.Publish(new RequestOpenSaveDialogEvent());
            Debug.Log("Opening save dialog");
        }
        
        void OnLoadGameClicked()
        {
            if (!SaveManager.Instance.HasAnySaveFiles())
            {
                Debug.LogWarning("No save files found");
                return;
            }
            
            if (HasUnsavedChanges())
            {
                ShowConfirmDialog("Load saved game? You have unsaved progress.", "load-game");
            }
            else
            {
                ConfirmLoadGame();
            }
        }
        
        bool HasUnsavedChanges()
        {
            if (!GameManager.Instance.IsGameRunning)
            {
                return false;
            }
            
            float timeSinceLastSave = Time.time - lastSaveTime;
            return timeSinceLastSave > unsavedWarningThreshold;
        }
        
        void ShowConfirmDialog(string message, string action)
        {
            if (confirmDialog == null || pauseMenu == null)
            {
                Debug.LogWarning("Confirm dialog not available, proceeding with action");
                ExecuteAction(action);
                return;
            }
            
            pendingAction = action;
            
            if (confirmMessage != null)
            {
                confirmMessage.text = message;
            }
            
            pauseMenu.style.display = DisplayStyle.None;
            confirmDialog.style.display = DisplayStyle.Flex;
        }
        
        void OnConfirmSaveClicked()
        {
            EventBus.Publish(new RequestOpenSaveDialogEvent());
            
            HidePauseMenu();
            
            this.enabled = false;
            Invoke(nameof(ExecutePendingActionDelayed), 0.5f);
        }
        
        void ExecutePendingActionDelayed()
        {
            this.enabled = true;
            ExecuteAction(pendingAction);
        }
        
        void OnConfirmDontSaveClicked()
        {
            ExecuteAction(pendingAction);
        }
        
        void OnConfirmCancelClicked()
        {
            if (confirmDialog != null)
            {
                confirmDialog.style.display = DisplayStyle.None;
            }
            
            if (pauseMenu != null)
            {
                pauseMenu.style.display = DisplayStyle.Flex;
            }
            
            pendingAction = "";
        }
        
        void ExecuteAction(string action)
        {
            switch (action)
            {
                case "new-game":
                    ConfirmNewGame();
                    break;
                case "load-game":
                    ConfirmLoadGame();
                    break;
            }
            
            pendingAction = "";
        }
        
        void ConfirmNewGame()
        {
            EventBus.Publish(new RequestStartNewGameEvent());
            HidePauseMenu();
            Debug.Log("New game started");
        }
        
        void ConfirmLoadGame()
        {
            EventBus.Publish(new RequestOpenLoadDialogEvent());
            HidePauseMenu();
            Debug.Log("Opening load dialog");
        }
    }
    
    public class RequestOpenSaveDialogEvent { }
    
    public class RequestOpenLoadDialogEvent { }
}
