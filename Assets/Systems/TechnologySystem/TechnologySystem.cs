using System;
using System.Collections.Generic;
using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Systems
{
    public class TechnologySystem : GameSystem
    {
        [Header("Technology Definitions")]
        [SerializeField] private TechnologySO[] technologies;
        
        [Header("Drift Configuration")]
        [SerializeField] private float driftMaxAdvanceYears = 6f;
        [SerializeField] private float driftMaxDelayYears = -5f;
        [SerializeField] private float driftMaxAnnualDelta = 0.5f;
        [SerializeField] private float driftDecayMultiplier = 0.94f;
        [SerializeField] private bool forwardOnlyDrift = true;
        
        private Dictionary<string, float> _adoptionRates = new Dictionary<string, float>();
        private Dictionary<string, TechAdoptionPhase> _phases = new Dictionary<string, TechAdoptionPhase>();
        private float _globalDriftOffset = 0f;
        private int _currentYear;
        
        public IReadOnlyDictionary<string, float> AdoptionRates => _adoptionRates;
        public IReadOnlyDictionary<string, TechAdoptionPhase> Phases => _phases;
        public float GlobalDriftOffset => _globalDriftOffset;
        
        protected override void Awake()
        {
            base.Awake();
            ServiceLocator.Instance.TryRegister<TechnologySystem>(this);
        }
        
        void Start()
        {
            TimeSystem timeSystem = ServiceLocator.Instance.Get<TimeSystem>();
            if (timeSystem != null)
            {
                _currentYear = timeSystem.CurrentDate.Year;
                UpdateAllTechnologies();
                Debug.Log($"[TechnologySystem] Initialized with {technologies?.Length ?? 0} technologies for year {_currentYear}");
            }
        }
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnYearChangedEvent>(HandleYearChanged);
            Subscribe<OnGameStartedEvent>(HandleGameStarted);
            Subscribe<RequestSaveTechnologyDataEvent>(HandleSaveRequest);
            Subscribe<RequestLoadTechnologyDataEvent>(HandleLoadRequest);
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            TimeSystem timeSystem = ServiceLocator.Instance.Get<TimeSystem>();
            if (timeSystem != null)
            {
                _currentYear = timeSystem.CurrentDate.Year;
                _globalDriftOffset = 0f;
                UpdateAllTechnologies();
            }
        }
        
        void HandleSaveRequest(RequestSaveTechnologyDataEvent evt)
        {
            EventBus.Publish(new OnTechnologyDataSavedEvent
            {
                Data = new TechMogul.Core.Save.TechnologySystemData
                {
                    globalDriftOffset = _globalDriftOffset
                }
            });
        }
        
        void HandleLoadRequest(RequestLoadTechnologyDataEvent evt)
        {
            if (evt.Data != null)
            {
                _globalDriftOffset = evt.Data.globalDriftOffset;
                Debug.Log($"[TechnologySystem] Loaded drift offset: {_globalDriftOffset:F2} years");
            }
        }
        
        void HandleYearChanged(OnYearChangedEvent evt)
        {
            _currentYear = evt.NewYear;
            ApplyDriftDecay();
            UpdateAllTechnologies();
        }
        
        void UpdateAllTechnologies()
        {
            if (technologies == null || technologies.Length == 0)
            {
                return;
            }
            
            foreach (TechnologySO tech in technologies)
            {
                if (tech == null)
                {
                    continue;
                }
                
                UpdateTechnology(tech);
            }
            
            EventBus.Publish(new OnTechnologiesUpdatedEvent
            {
                Year = _currentYear,
                AdoptionRates = new Dictionary<string, float>(_adoptionRates),
                Phases = new Dictionary<string, TechAdoptionPhase>(_phases)
            });
        }
        
        void UpdateTechnology(TechnologySO tech)
        {
            if (!ArePrerequisitesMet(tech))
            {
                _adoptionRates[tech.id] = 0f;
                _phases[tech.id] = TechAdoptionPhase.Locked;
                return;
            }
            
            if (_currentYear < tech.researchYear)
            {
                _adoptionRates[tech.id] = 0f;
                _phases[tech.id] = TechAdoptionPhase.Locked;
                return;
            }
            
            float adoptionRate = CalculateAdoptionRate(tech);
            _adoptionRates[tech.id] = adoptionRate;
            _phases[tech.id] = GetPhase(adoptionRate);
        }
        
        float CalculateAdoptionRate(TechnologySO tech)
        {
            float effectiveInflectionYear = tech.inflectionYear;
            
            if (_currentYear < tech.inflectionYear && !forwardOnlyDrift)
            {
                effectiveInflectionYear += _globalDriftOffset;
                effectiveInflectionYear = Mathf.Clamp(effectiveInflectionYear, 
                    tech.inflectionYear + driftMaxDelayYears, 
                    tech.inflectionYear + driftMaxAdvanceYears);
            }
            else if (_currentYear < tech.inflectionYear && forwardOnlyDrift && _globalDriftOffset > 0)
            {
                effectiveInflectionYear += _globalDriftOffset;
                effectiveInflectionYear = Mathf.Clamp(effectiveInflectionYear, 
                    tech.inflectionYear, 
                    tech.inflectionYear + driftMaxAdvanceYears);
            }
            
            float yearDelta = _currentYear - effectiveInflectionYear;
            float exponent = -tech.adoptionSpeedConstant * yearDelta;
            float adoptionRate = 1f / (1f + Mathf.Exp(exponent));
            
            return Mathf.Clamp01(adoptionRate);
        }
        
        bool ArePrerequisitesMet(TechnologySO tech)
        {
            if (tech.prerequisites == null || tech.prerequisites.Length == 0)
            {
                return true;
            }
            
            foreach (TechnologySO prereq in tech.prerequisites)
            {
                if (prereq == null)
                {
                    continue;
                }
                
                if (!_adoptionRates.ContainsKey(prereq.id) || _adoptionRates[prereq.id] < 0.15f)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        TechAdoptionPhase GetPhase(float adoptionRate)
        {
            if (adoptionRate >= 0.90f) return TechAdoptionPhase.Mandatory;
            if (adoptionRate >= 0.75f) return TechAdoptionPhase.Mainstream;
            if (adoptionRate >= 0.40f) return TechAdoptionPhase.Growth;
            if (adoptionRate >= 0.15f) return TechAdoptionPhase.EarlyAdoption;
            return TechAdoptionPhase.Research;
        }
        
        void ApplyDriftDecay()
        {
            _globalDriftOffset *= driftDecayMultiplier;
            
            if (Mathf.Abs(_globalDriftOffset) < 0.01f)
            {
                _globalDriftOffset = 0f;
            }
        }
        
        public void AddDrift(float years)
        {
            float clampedDelta = Mathf.Clamp(years, -driftMaxAnnualDelta, driftMaxAnnualDelta);
            _globalDriftOffset += clampedDelta;
            
            _globalDriftOffset = Mathf.Clamp(_globalDriftOffset, driftMaxDelayYears, driftMaxAdvanceYears);
            
            Debug.Log($"[TechnologySystem] Drift adjusted by {clampedDelta:F2} years. Total drift: {_globalDriftOffset:F2}");
        }
        
        public float GetAdoptionRate(string techId)
        {
            return _adoptionRates.ContainsKey(techId) ? _adoptionRates[techId] : 0f;
        }
        
        public TechAdoptionPhase GetPhase(string techId)
        {
            return _phases.ContainsKey(techId) ? _phases[techId] : TechAdoptionPhase.Locked;
        }
        
        public TechnologySO GetTechnology(string techId)
        {
            if (technologies == null)
            {
                return null;
            }
            
            foreach (TechnologySO tech in technologies)
            {
                if (tech != null && tech.id == techId)
                {
                    return tech;
                }
            }
            
            return null;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: Print All Technologies")]
        void DebugPrintTechnologies()
        {
            Debug.Log($"=== Technologies ({_currentYear}) ===");
            foreach (TechnologySO tech in technologies)
            {
                if (tech == null) continue;
                
                float adoption = GetAdoptionRate(tech.id);
                TechAdoptionPhase phase = GetPhase(tech.id);
                Debug.Log($"{tech.techName}: {adoption:P1} ({phase})");
            }
        }
        
        [ContextMenu("Debug: Add +1 Year Drift")]
        void DebugAddDrift()
        {
            AddDrift(1f);
        }
        #endif
    }
    
    public class OnTechnologiesUpdatedEvent
    {
        public int Year;
        public Dictionary<string, float> AdoptionRates;
        public Dictionary<string, TechAdoptionPhase> Phases;
    }
    
    public class RequestSaveTechnologyDataEvent { }
    
    public class OnTechnologyDataSavedEvent
    {
        public TechMogul.Core.Save.TechnologySystemData Data;
    }
    
    public class RequestLoadTechnologyDataEvent
    {
        public TechMogul.Core.Save.TechnologySystemData Data;
    }
}
