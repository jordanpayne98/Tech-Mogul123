using UnityEngine;
using UnityEngine.UIElements;
using TechMogul.Core;
using TechMogul.Systems;
using TechMogul.Products;
using TechMogul.Contracts;

namespace TechMogul.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class GameOverController : UIController
    {
        private UIDocument uiDocument;
        private VisualElement root;
        private VisualElement gameOverMenu;
        
        private Label daysSurvivedLabel;
        private Label productsReleasedLabel;
        private Label contractsCompletedLabel;
        private Label employeesHiredLabel;
        
        private Button restartBtn;
        private Button mainMenuBtn;
        
        private int daysSurvived;
        private int productsReleased;
        private int contractsCompleted;
        private int employeesHired;
        
        protected override void Awake()
        {
            base.Awake();
            uiDocument = GetComponent<UIDocument>();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("GameOverController: UIDocument or root visual element is null");
                return;
            }
            
            root = uiDocument.rootVisualElement;
            CacheReferences();
            BindButtons();
            
            HideGameOverMenu();
            
            Debug.Log("GameOverController initialized");
        }
        
        protected override void OnDisable()
        {
            UnbindButtons();
            base.OnDisable();
        }
        
        void CacheReferences()
        {
            gameOverMenu = root.Q<VisualElement>("game-over-menu");
            
            if (gameOverMenu != null)
            {
                daysSurvivedLabel = gameOverMenu.Q<Label>("days-survived");
                productsReleasedLabel = gameOverMenu.Q<Label>("products-released");
                contractsCompletedLabel = gameOverMenu.Q<Label>("contracts-completed");
                employeesHiredLabel = gameOverMenu.Q<Label>("employees-hired");
                
                restartBtn = gameOverMenu.Q<Button>("restart-btn");
                mainMenuBtn = gameOverMenu.Q<Button>("main-menu-btn");
            }
            
            if (gameOverMenu == null)
            {
                Debug.LogError("game-over-menu VisualElement not found");
            }
        }
        
        void BindButtons()
        {
            if (restartBtn != null) 
            {
                restartBtn.clicked += OnRestartClicked;
            }
            
            if (mainMenuBtn != null) 
            {
                mainMenuBtn.clicked += OnMainMenuClicked;
            }
        }
        
        void UnbindButtons()
        {
            if (restartBtn != null) 
            {
                restartBtn.clicked -= OnRestartClicked;
            }
            
            if (mainMenuBtn != null) 
            {
                mainMenuBtn.clicked -= OnMainMenuClicked;
            }
        }
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnBankruptcyEvent>(HandleBankruptcy);
            Subscribe<OnDayTickEvent>(HandleDayTick);
            Subscribe<OnProductReleasedEvent>(HandleProductReleased);
            Subscribe<OnContractCompletedEvent>(HandleContractCompleted);
            Subscribe<OnEmployeeHiredEvent>(HandleEmployeeHired);
            Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleBankruptcy(OnBankruptcyEvent evt)
        {
            Debug.Log("GameOverController received OnBankruptcyEvent");
            ShowGameOverMenu();
        }
        
        void HandleDayTick(OnDayTickEvent evt)
        {
            daysSurvived++;
        }
        
        void HandleProductReleased(OnProductReleasedEvent evt)
        {
            productsReleased++;
        }
        
        void HandleContractCompleted(OnContractCompletedEvent evt)
        {
            contractsCompleted++;
        }
        
        void HandleEmployeeHired(OnEmployeeHiredEvent evt)
        {
            employeesHired++;
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            ResetStats();
            HideGameOverMenu();
        }
        
        void ShowGameOverMenu()
        {
            Debug.Log($"ShowGameOverMenu called. gameOverMenu is null: {gameOverMenu == null}");
            
            if (gameOverMenu == null)
            {
                Debug.LogError("Cannot show game over menu - gameOverMenu is null!");
                return;
            }
            
            UpdateStatsDisplay();
            
            gameOverMenu.style.display = DisplayStyle.Flex;
            
            EventBus.Publish(new RequestChangeSpeedEvent { Speed = TimeSpeed.Paused });
            
            Debug.Log("Game Over menu shown - display set to Flex");
        }
        
        void HideGameOverMenu()
        {
            if (gameOverMenu != null)
            {
                gameOverMenu.style.display = DisplayStyle.None;
            }
        }
        
        void UpdateStatsDisplay()
        {
            if (daysSurvivedLabel != null)
            {
                daysSurvivedLabel.text = daysSurvived.ToString();
            }
            
            if (productsReleasedLabel != null)
            {
                productsReleasedLabel.text = productsReleased.ToString();
            }
            
            if (contractsCompletedLabel != null)
            {
                contractsCompletedLabel.text = contractsCompleted.ToString();
            }
            
            if (employeesHiredLabel != null)
            {
                employeesHiredLabel.text = employeesHired.ToString();
            }
        }
        
        void ResetStats()
        {
            daysSurvived = 0;
            productsReleased = 0;
            contractsCompleted = 0;
            employeesHired = 0;
        }
        
        void OnRestartClicked()
        {
            EventBus.Publish(new RequestStartNewGameEvent());
            HideGameOverMenu();
            
            Debug.Log("Game restarted from Game Over menu");
        }
        
        void OnMainMenuClicked()
        {
            HideGameOverMenu();
            EventBus.Publish(new RequestChangeSpeedEvent { Speed = TimeSpeed.Paused });
            
            Debug.Log("Main menu requested - implement scene loading if needed");
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: Show Game Over Menu")]
        void DebugShowMenu()
        {
            ShowGameOverMenu();
        }
        
        [ContextMenu("Debug: Hide Game Over Menu")]
        void DebugHideMenu()
        {
            HideGameOverMenu();
        }
        #endif
    }
}
