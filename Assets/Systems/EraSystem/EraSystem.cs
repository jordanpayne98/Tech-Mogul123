using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Systems
{
    public class EraSystem : GameSystem
    {
        [Header("Era Definitions")]
        [SerializeField] private EraSO[] eras;
        
        private EraSO _currentEra;
        private int _currentYear;
        
        public EraSO CurrentEra => _currentEra;
        public int CurrentYear => _currentYear;
        
        protected override void Awake()
        {
            base.Awake();
            ServiceLocator.Instance.TryRegister<EraSystem>(this);
        }
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnDayTickEvent>(HandleDayTick);
            Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            TimeSystem timeSystem = ServiceLocator.Instance.Get<TimeSystem>();
            if (timeSystem != null)
            {
                _currentYear = timeSystem.CurrentDate.Year;
                UpdateEra(_currentYear);
            }
        }
        
        void HandleDayTick(OnDayTickEvent evt)
        {
            if (evt.CurrentDate.Year != _currentYear)
            {
                int previousYear = _currentYear;
                _currentYear = evt.CurrentDate.Year;
                
                EventBus.Publish(new OnYearChangedEvent
                {
                    NewYear = _currentYear,
                    PreviousYear = previousYear
                });
                
                UpdateEra(_currentYear);
            }
        }
        
        void UpdateEra(int year)
        {
            if (eras == null || eras.Length == 0)
            {
                Debug.LogWarning("[EraSystem] No eras configured");
                return;
            }
            
            EraSO newEra = FindEraForYear(year);
            
            if (newEra != _currentEra)
            {
                EraSO previousEra = _currentEra;
                _currentEra = newEra;
                
                EventBus.Publish(new OnEraChangedEvent
                {
                    NewEra = newEra,
                    PreviousEra = previousEra,
                    Year = year
                });
                
                if (newEra != null)
                {
                    Debug.Log($"[EraSystem] Era changed to: {newEra.eraName} ({newEra.startYear}-{newEra.endYear})");
                }
            }
        }
        
        EraSO FindEraForYear(int year)
        {
            for (int i = 0; i < eras.Length; i++)
            {
                if (eras[i] != null && eras[i].IsYearInEra(year))
                {
                    return eras[i];
                }
            }
            
            Debug.LogWarning($"[EraSystem] No era found for year {year}");
            return null;
        }
        
        public float GetCurrentMarketSizeMultiplier()
        {
            return _currentEra != null ? _currentEra.baseMarketSizeMultiplier : 1f;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: Print Current Era")]
        void DebugPrintCurrentEra()
        {
            if (_currentEra != null)
            {
                Debug.Log($"Current Era: {_currentEra.eraName} ({_currentEra.startYear}-{_currentEra.endYear}), Market Multiplier: {_currentEra.baseMarketSizeMultiplier}x");
            }
            else
            {
                Debug.Log("No current era set");
            }
        }
        #endif
    }
    
    public class OnYearChangedEvent
    {
        public int NewYear;
        public int PreviousYear;
    }
    
    public class OnEraChangedEvent
    {
        public EraSO NewEra;
        public EraSO PreviousEra;
        public int Year;
    }
}
