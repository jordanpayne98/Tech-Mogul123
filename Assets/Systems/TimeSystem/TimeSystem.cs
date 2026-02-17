using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Systems
{
    public class TimeSystem : MonoBehaviour
    {
        [Header("Starting Date")]
        [SerializeField] private int startYear = 2024;
        [SerializeField] private int startMonth = 1;
        [SerializeField] private int startDay = 1;
        
        [Header("Tick Settings")]
        [SerializeField] private float baseTickInterval = 10f;
        
        private GameDate _currentDate;
        private TimeSpeed _currentSpeed = TimeSpeed.Normal;
        private float _tickTimer;
        
        public GameDate CurrentDate => _currentDate;
        public TimeSpeed CurrentSpeed => _currentSpeed;
        
        void Awake()
        {
            Initialize();
        }
        
        void OnEnable()
        {
            SubscribeToEvents();
        }
        
        void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        void Update()
        {
            if (_currentSpeed == TimeSpeed.Paused) return;
            
            _tickTimer += Time.deltaTime;
            float adjustedInterval = baseTickInterval / GetSpeedMultiplier();
            
            if (_tickTimer >= adjustedInterval)
            {
                _tickTimer = 0f;
                AdvanceDay();
            }
        }
        
        void Initialize()
        {
            _currentDate = new GameDate
            {
                Year = startYear,
                Month = startMonth,
                Day = startDay
            };
            
            _tickTimer = 0f;
            _currentSpeed = TimeSpeed.Paused;
            
            Debug.Log($"TimeSystem initialized: {FormatDate(_currentDate)}");
        }
        
        void SubscribeToEvents()
        {
            EventBus.Subscribe<RequestChangeSpeedEvent>(HandleChangeSpeedRequest);
            EventBus.Subscribe<RequestSetDateEvent>(HandleSetDateRequest);
            EventBus.Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<RequestChangeSpeedEvent>(HandleChangeSpeedRequest);
            EventBus.Unsubscribe<RequestSetDateEvent>(HandleSetDateRequest);
            EventBus.Unsubscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleChangeSpeedRequest(RequestChangeSpeedEvent evt)
        {
            SetSpeed(evt.Speed);
        }
        
        void HandleSetDateRequest(RequestSetDateEvent evt)
        {
            _currentDate.Year = evt.Year;
            _currentDate.Month = evt.Month;
            _currentDate.Day = evt.Day;
            
            Debug.Log($"Date set to: {FormatDate(_currentDate)}");
            
            PublishDateUpdate();
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            Initialize();
            SetSpeed(TimeSpeed.Normal);
            
            PublishDateUpdate();
        }
        
        void PublishDateUpdate()
        {
            EventBus.Publish(new OnDayTickEvent
            {
                CurrentDate = new GameDate
                {
                    Year = _currentDate.Year,
                    Month = _currentDate.Month,
                    Day = _currentDate.Day
                }
            });
        }
        
        void AdvanceDay()
        {
            _currentDate.Day++;
            
            if (_currentDate.Day > 30)
            {
                _currentDate.Day = 1;
                _currentDate.Month++;
                
                if (_currentDate.Month > 12)
                {
                    _currentDate.Month = 1;
                    _currentDate.Year++;
                }
                
                TriggerMonthTick();
            }
            
            TriggerDayTick();
        }
        
        void TriggerDayTick()
        {
            EventBus.Publish(new OnDayTickEvent
            {
                CurrentDate = new GameDate
                {
                    Year = _currentDate.Year,
                    Month = _currentDate.Month,
                    Day = _currentDate.Day
                }
            });
        }
        
        void TriggerMonthTick()
        {
            EventBus.Publish(new OnMonthTickEvent
            {
                CurrentDate = new GameDate
                {
                    Year = _currentDate.Year,
                    Month = _currentDate.Month,
                    Day = _currentDate.Day
                },
                Month = _currentDate.Month,
                Year = _currentDate.Year
            });
            
            Debug.Log($"Month advanced: {FormatDate(_currentDate)}");
        }
        
        public void SetSpeed(TimeSpeed speed)
        {
            _currentSpeed = speed;
            
            EventBus.Publish(new OnSpeedChangedEvent
            {
                NewSpeed = speed,
                Multiplier = GetSpeedMultiplier()
            });
            
            Debug.Log($"Time speed changed to: {speed} ({GetSpeedMultiplier()}x)");
        }
        
        float GetSpeedMultiplier()
        {
            return _currentSpeed switch
            {
                TimeSpeed.Paused => 0f,
                TimeSpeed.Normal => 1f,
                TimeSpeed.Fast => 2f,
                TimeSpeed.Faster => 4f,
                TimeSpeed.Fastest => 8f,
                _ => 1f
            };
        }
        
        string FormatDate(GameDate date)
        {
            string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            return $"{months[date.Month - 1]} {date.Day}, {date.Year}";
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: Advance 1 Day")]
        void DebugAdvanceDay()
        {
            AdvanceDay();
        }
        
        [ContextMenu("Debug: Advance 1 Month")]
        void DebugAdvanceMonth()
        {
            for (int i = 0; i < 30; i++)
            {
                AdvanceDay();
            }
        }
        
        [ContextMenu("Debug: Set Speed to Normal")]
        void DebugSetNormal()
        {
            SetSpeed(TimeSpeed.Normal);
        }
        
        [ContextMenu("Debug: Set Speed to Fast")]
        void DebugSetFast()
        {
            SetSpeed(TimeSpeed.Fast);
        }
        
        [ContextMenu("Debug: Pause")]
        void DebugPause()
        {
            SetSpeed(TimeSpeed.Paused);
        }
        #endif
    }
    
    [System.Serializable]
    public class GameDate
    {
        public int Year;
        public int Month;
        public int Day;
    }
    
    public enum TimeSpeed
    {
        Paused,
        Normal,
        Fast,
        Faster,
        Fastest
    }
    
    public class OnDayTickEvent
    {
        public GameDate CurrentDate;
    }
    
    public class OnMonthTickEvent
    {
        public GameDate CurrentDate;
        public int Month;
        public int Year;
    }
    
    public class OnSpeedChangedEvent
    {
        public TimeSpeed NewSpeed;
        public float Multiplier;
    }
    
    public class RequestChangeSpeedEvent
    {
        public TimeSpeed Speed;
    }
}
