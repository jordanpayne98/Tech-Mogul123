using System;
using System.Collections.Generic;
using UnityEngine;
using TechMogul.Core.Save;

namespace TechMogul.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game State")]
        [SerializeField] private float startingCash = 50000f;
        
        private IEventBus _eventBus;
        private float _currentCash;
        private bool _isGameRunning;
        private readonly List<IDisposable> _subs = new List<IDisposable>();
        
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
            
            InitializeServices();
            InitializeSystems();
        }
        
        void InitializeServices()
        {
            if (ServiceLocator.Instance.TryGet<IEventBus>(out IEventBus existingEventBus))
            {
                _eventBus = existingEventBus;
            }
            else
            {
                _eventBus = new EventBus(ex => Debug.LogException(ex));
                ServiceLocator.Instance.Register<IEventBus>(_eventBus);
            }
            
            Debug.Log("Services initialized");
        }
        
        void OnEnable()
        {
            SubscribeToEvents();
        }
        
        void OnDisable()
        {
            for (int i = 0; i < _subs.Count; i++)
            {
                _subs[i]?.Dispose();
            }
            _subs.Clear();
        }
        
        void InitializeSystems()
        {
            _currentCash = startingCash;
            _isGameRunning = false;
            
            Debug.Log("GameManager initialized");
        }
        
        void Start()
        {
            PublishCashChanged(0, _currentCash);
        }
        
        void SubscribeToEvents()
        {
            _subs.Add(_eventBus.Subscribe<RequestAddCashEvent>(HandleAddCash));
            _subs.Add(_eventBus.Subscribe<RequestDeductCashEvent>(HandleDeductCash));
            _subs.Add(_eventBus.Subscribe<RequestSetCashEvent>(HandleSetCash));
            _subs.Add(_eventBus.Subscribe<RequestStartNewGameEvent>(HandleStartNewGame));
        }
        
        void HandleStartNewGame(RequestStartNewGameEvent evt)
        {
            StartNewGame();
        }
        
        public void StartNewGame()
        {
            float oldCash = _currentCash;
            _currentCash = startingCash;
            _isGameRunning = true;
            
            _eventBus.Publish(new OnGameStartedEvent());
            PublishCashChanged(oldCash, _currentCash);
            
            Debug.Log($"New game started with ${_currentCash:N0}");
        }
        
        void HandleSetCash(RequestSetCashEvent evt)
        {
            float oldCash = _currentCash;
            _currentCash = evt.Amount;
            
            PublishCashChanged(oldCash, _currentCash);
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
            
            PublishCashChanged(oldCash, _currentCash);
            
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
                _eventBus.Publish(new OnInsufficientCashEvent 
                { 
                    Required = evt.Amount,
                    Available = _currentCash 
                });
                return;
            }
            
            float oldCash = _currentCash;
            _currentCash -= evt.Amount;
            
            PublishCashChanged(oldCash, _currentCash);
            
            Debug.Log($"Cash deducted: ${evt.Amount:N0}. New balance: ${_currentCash:N0}");
            
            CheckBankruptcy();
        }
        
        void CheckBankruptcy()
        {
            if (_currentCash <= 0)
            {
                if (!_isGameRunning)
                {
                    _isGameRunning = true;
                    Debug.LogWarning("Game wasn't officially started, but setting _isGameRunning = true for bankruptcy check");
                }
                
                _eventBus.Publish(new OnBankruptcyEvent());
                Debug.LogWarning("Bankruptcy! Cash reached $0");
            }
        }
        
        void PublishCashChanged(float oldCash, float newCash)
        {
            _eventBus.Publish(new OnCashChangedEvent
            {
                OldCash = oldCash,
                NewCash = newCash,
                Change = newCash - oldCash
            });
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
            _eventBus.Publish(new RequestAddCashEvent { Amount = 10000 });
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
