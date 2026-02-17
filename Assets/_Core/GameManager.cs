using UnityEngine;

namespace TechMogul.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game State")]
        [SerializeField] private float startingCash = 50000f;
        
        private float _currentCash;
        private bool _isGameRunning;
        
        public float CurrentCash => _currentCash;
        public bool IsGameRunning => _isGameRunning;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSystems();
        }
        
        void OnEnable()
        {
            SubscribeToEvents();
        }
        
        void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        void InitializeSystems()
        {
            _currentCash = startingCash;
            _isGameRunning = false;
            
            Debug.Log("GameManager initialized");
        }
        
        void Start()
        {
            EventBus.Publish(new OnCashChangedEvent { NewCash = _currentCash, OldCash = 0, Change = _currentCash });
        }
        
        void SubscribeToEvents()
        {
            EventBus.Subscribe<RequestAddCashEvent>(HandleAddCash);
            EventBus.Subscribe<RequestDeductCashEvent>(HandleDeductCash);
            EventBus.Subscribe<RequestSetCashEvent>(HandleSetCash);
            EventBus.Subscribe<RequestStartNewGameEvent>(HandleStartNewGame);
        }
        
        void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<RequestAddCashEvent>(HandleAddCash);
            EventBus.Unsubscribe<RequestDeductCashEvent>(HandleDeductCash);
            EventBus.Unsubscribe<RequestSetCashEvent>(HandleSetCash);
            EventBus.Unsubscribe<RequestStartNewGameEvent>(HandleStartNewGame);
        }
        
        void HandleStartNewGame(RequestStartNewGameEvent evt)
        {
            StartNewGame();
        }
        
        public void StartNewGame()
        {
            _currentCash = startingCash;
            _isGameRunning = true;
            
            EventBus.Publish(new OnGameStartedEvent());
            EventBus.Publish(new OnCashChangedEvent { NewCash = _currentCash, OldCash = 0, Change = _currentCash });
            
            Debug.Log($"New game started with ${_currentCash:N0}");
        }
        
        void HandleSetCash(RequestSetCashEvent evt)
        {
            float oldCash = _currentCash;
            _currentCash = evt.Amount;
            _isGameRunning = true;
            
            EventBus.Publish(new OnCashChangedEvent
            {
                NewCash = _currentCash,
                OldCash = oldCash,
                Change = _currentCash - oldCash
            });
        }
        
        void HandleAddCash(RequestAddCashEvent evt)
        {
            if (evt.Amount <= 0)
            {
                Debug.LogWarning($"Attempted to add invalid cash amount: {evt.Amount}");
                return;
            }
            
            float oldCash = _currentCash;
            _currentCash += evt.Amount;
            
            EventBus.Publish(new OnCashChangedEvent 
            { 
                NewCash = _currentCash,
                OldCash = oldCash,
                Change = evt.Amount
            });
            
            Debug.Log($"Cash added: ${evt.Amount:N0}. New balance: ${_currentCash:N0}");
        }
        
        void HandleDeductCash(RequestDeductCashEvent evt)
        {
            if (evt.Amount <= 0)
            {
                Debug.LogWarning($"Attempted to deduct invalid cash amount: {evt.Amount}");
                return;
            }
            
            if (_currentCash < evt.Amount)
            {
                Debug.LogWarning($"Insufficient cash. Required: ${evt.Amount:N0}, Available: ${_currentCash:N0}");
                EventBus.Publish(new OnInsufficientCashEvent 
                { 
                    Required = evt.Amount,
                    Available = _currentCash 
                });
                
                // Deduct what cash is available and go into debt (negative cash)
                float oldCash = _currentCash;
                _currentCash -= evt.Amount;
                
                EventBus.Publish(new OnCashChangedEvent 
                { 
                    NewCash = _currentCash,
                    OldCash = oldCash,
                    Change = -evt.Amount
                });
                
                Debug.Log($"Cash deducted (insufficient funds): ${evt.Amount:N0}. New balance: ${_currentCash:N0}");
                
                // Check for bankruptcy after going into debt
                CheckBankruptcy();
                return;
            }
            
            float oldCash2 = _currentCash;
            _currentCash -= evt.Amount;
            
            EventBus.Publish(new OnCashChangedEvent 
            { 
                NewCash = _currentCash,
                OldCash = oldCash2,
                Change = -evt.Amount
            });
            
            Debug.Log($"Cash deducted: ${evt.Amount:N0}. New balance: ${_currentCash:N0}");
            
            CheckBankruptcy();
        }
        
        void CheckBankruptcy()
        {
            if (_currentCash <= 0)
            {
                // Set game as running if somehow it wasn't started yet
                if (!_isGameRunning)
                {
                    _isGameRunning = true;
                    Debug.LogWarning("Game wasn't officially started, but setting _isGameRunning = true for bankruptcy check");
                }
                
                EventBus.Publish(new OnBankruptcyEvent());
                Debug.LogWarning("Bankruptcy! Cash reached $0");
            }
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: Start New Game")]
        void DebugStartNewGame()
        {
            StartNewGame();
        }
        
        [ContextMenu("Debug: Add $10,000")]
        void DebugAddCash()
        {
            EventBus.Publish(new RequestAddCashEvent { Amount = 10000 });
        }
        
        [ContextMenu("Debug: Save Game")]
        void DebugSaveGame()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
            else
            {
                Debug.LogWarning("SaveManager instance not found");
            }
        }
        
        [ContextMenu("Debug: Load Game")]
        void DebugLoadGame()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.LoadGame();
            }
            else
            {
                Debug.LogWarning("SaveManager instance not found");
            }
        }
        
        [ContextMenu("Debug: Trigger Game Over")]
        void DebugTriggerGameOver()
        {
            _currentCash = 0;
            CheckBankruptcy();
            Debug.Log("Game Over triggered manually");
        }
        #endif
    }
    
    public class OnGameStartedEvent { }
    
    public class RequestStartNewGameEvent { }
    
    public class OnCashChangedEvent
    {
        public float NewCash;
        public float OldCash;
        public float Change;
    }
    
    public class RequestAddCashEvent
    {
        public float Amount;
    }
    
    public class RequestDeductCashEvent
    {
        public float Amount;
    }
    
    public class OnInsufficientCashEvent
    {
        public float Required;
        public float Available;
    }
    
    public class OnBankruptcyEvent { }
}
