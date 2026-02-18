using UnityEngine;
using System.Collections.Generic;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.Traits
{
    public class TraitSystem : GameSystem
    {
        public static TraitSystem Instance { get; private set; }
        
        [Header("Databases")]
        [SerializeField] private TraitDatabaseSO traitDatabase;
        [SerializeField] private SynergyRuleSetSO synergyRules;
        
        private TraitEvaluator _evaluator;
        private ArcManager _arcManager;
        
        public TraitDatabaseSO Database => traitDatabase;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            if (traitDatabase != null)
            {
                traitDatabase.Initialize();
            }
            
            if (synergyRules != null)
            {
                // Initialize synergy rules if empty
                if (synergyRules.tier1Rules.Count == 0 && synergyRules.tier2Rules.Count == 0)
                {
                    synergyRules.InitializeLockedRules();
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(synergyRules);
                    #endif
                }
                _evaluator = new TraitEvaluator(synergyRules);
            }
            
            _arcManager = new ArcManager();
            
            ServiceLocator.Instance.TryRegister<TraitSystem>(this);
        }
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnDayTickEvent>(HandleDayTick);
        }
        
        void HandleDayTick(OnDayTickEvent evt)
        {
        }
        
        public Dictionary<StatType, float> EvaluateEmployeeTraits(
            string employeeId,
            string majorTraitId,
            List<string> minorTraitIds,
            EmployeeConditionContext context)
        {
            if (traitDatabase == null || _evaluator == null)
            {
                return new Dictionary<StatType, float>();
            }
            
            TraitDefinitionSO majorTrait = traitDatabase.GetTraitById(majorTraitId);
            List<TraitDefinitionSO> minorTraits = new List<TraitDefinitionSO>();
            
            for (int i = 0; i < minorTraitIds.Count; i++)
            {
                TraitDefinitionSO trait = traitDatabase.GetTraitById(minorTraitIds[i]);
                if (trait != null)
                {
                    minorTraits.Add(trait);
                }
            }
            
            Dictionary<StatType, float> baseModifiers = _evaluator.EvaluateTraits(majorTrait, minorTraits, context);
            
            if (majorTrait != null)
            {
                _arcManager.ProcessDaily(employeeId, majorTrait, context);
                List<ModifierDef> arcModifiers = _arcManager.GetActiveArcModifiers(employeeId);
                
                for (int i = 0; i < arcModifiers.Count; i++)
                {
                    if (arcModifiers[i].ConditionMet(context))
                    {
                        if (baseModifiers.ContainsKey(arcModifiers[i].stat))
                        {
                            baseModifiers[arcModifiers[i].stat] += arcModifiers[i].value;
                        }
                        else
                        {
                            baseModifiers[arcModifiers[i].stat] = arcModifiers[i].value;
                        }
                    }
                }
            }
            
            return baseModifiers;
        }
        
        public float RollProductivityVariance(Dictionary<StatType, float> modifiers)
        {
            if (_evaluator != null)
            {
                return _evaluator.RollProductivityVariance(modifiers);
            }
            return 1.0f;
        }
        
        public float GetEventWeightModifier(Dictionary<StatType, float> modifiers, EventCategory category)
        {
            if (_evaluator != null)
            {
                return _evaluator.GetEventWeightModifier(modifiers, category);
            }
            return 1.0f;
        }
        
        public TraitDefinitionSO GetTrait(string traitId)
        {
            if (traitDatabase != null)
            {
                return traitDatabase.GetTraitById(traitId);
            }
            return null;
        }
        
        public Dictionary<TraitTag, int> CountTraitTags(string majorTraitId, List<string> minorTraitIds)
        {
            Dictionary<TraitTag, int> tagCounts = new Dictionary<TraitTag, int>();
            
            TraitDefinitionSO majorTrait = GetTrait(majorTraitId);
            if (majorTrait != null && majorTrait.tags != null)
            {
                for (int i = 0; i < majorTrait.tags.Count; i++)
                {
                    if (!tagCounts.ContainsKey(majorTrait.tags[i]))
                    {
                        tagCounts[majorTrait.tags[i]] = 0;
                    }
                    tagCounts[majorTrait.tags[i]]++;
                }
            }
            
            if (minorTraitIds != null)
            {
                for (int i = 0; i < minorTraitIds.Count; i++)
                {
                    TraitDefinitionSO minorTrait = GetTrait(minorTraitIds[i]);
                    if (minorTrait != null && minorTrait.tags != null)
                    {
                        for (int j = 0; j < minorTrait.tags.Count; j++)
                        {
                            if (!tagCounts.ContainsKey(minorTrait.tags[j]))
                            {
                                tagCounts[minorTrait.tags[j]] = 0;
                            }
                            tagCounts[minorTrait.tags[j]]++;
                        }
                    }
                }
            }
            
            return tagCounts;
        }
        
        public ArcState GetArcState(string employeeId)
        {
            return _arcManager.GetArcState(employeeId);
        }
        
        public void LoadArcState(string employeeId, ArcState state)
        {
            _arcManager.LoadArcState(employeeId, state);
        }
    }
}
