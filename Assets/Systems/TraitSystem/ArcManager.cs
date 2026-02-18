using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechMogul.Traits
{
    public class ArcManager
    {
        Dictionary<string, ActiveArc> _activeArcs = new Dictionary<string, ActiveArc>();
        Dictionary<string, ArcHistory> _arcHistories = new Dictionary<string, ArcHistory>();
        
        public void ProcessDaily(string employeeId, TraitDefinitionSO majorTrait, EmployeeConditionContext context)
        {
            if (majorTrait == null || !majorTrait.arcCapable || majorTrait.arcDefinition == null)
            {
                return;
            }
            
            if (_activeArcs.ContainsKey(employeeId))
            {
                UpdateActiveArc(employeeId, context);
            }
            else
            {
                TryStartArc(employeeId, majorTrait, context);
            }
            
            UpdateArcHistory(employeeId);
        }
        
        void TryStartArc(string employeeId, TraitDefinitionSO majorTrait, EmployeeConditionContext context)
        {
            ArcDefinitionSO arcDef = majorTrait.arcDefinition;
            
            if (arcDef.startCondition != null && !arcDef.startCondition.Evaluate(context))
            {
                return;
            }
            
            float baseChance = arcDef.baseStartChancePerDay;
            
            if (_arcHistories.TryGetValue(employeeId, out ArcHistory history))
            {
                if (history.cooldownDaysRemaining > 0)
                {
                    return;
                }
                
                baseChance *= history.retriggerMultiplier;
            }
            
            if (UnityEngine.Random.value < baseChance)
            {
                StartArc(employeeId, arcDef);
            }
        }
        
        void StartArc(string employeeId, ArcDefinitionSO arcDef)
        {
            if (arcDef.stages.Count == 0)
            {
                return;
            }
            
            ActiveArc arc = new ActiveArc
            {
                arcDefinition = arcDef,
                currentStageIndex = 0,
                daysInCurrentStage = 0
            };
            
            _activeArcs[employeeId] = arc;
        }
        
        void UpdateActiveArc(string employeeId, EmployeeConditionContext context)
        {
            if (!_activeArcs.TryGetValue(employeeId, out ActiveArc arc))
            {
                return;
            }
            
            arc.daysInCurrentStage++;
            
            ArcStage currentStage = arc.arcDefinition.stages[arc.currentStageIndex];
            
            if (arc.daysInCurrentStage >= currentStage.maxDays)
            {
                AdvanceOrCompleteArc(employeeId, arc);
            }
        }
        
        void AdvanceOrCompleteArc(string employeeId, ActiveArc arc)
        {
            int nextStageIndex = arc.currentStageIndex + 1;
            
            if (nextStageIndex >= arc.arcDefinition.stages.Count)
            {
                CompleteArc(employeeId, arc);
            }
            else
            {
                arc.currentStageIndex = nextStageIndex;
                arc.daysInCurrentStage = 0;
            }
        }
        
        void CompleteArc(string employeeId, ActiveArc arc)
        {
            _activeArcs.Remove(employeeId);
            
            ArcHistory history = new ArcHistory
            {
                cooldownDaysRemaining = arc.arcDefinition.cooldownDays,
                retriggerMultiplier = arc.arcDefinition.postCooldownChanceMultiplier,
                recoveryDaysToBaseline = arc.arcDefinition.recoveryDaysToBaseline
            };
            
            _arcHistories[employeeId] = history;
        }
        
        void UpdateArcHistory(string employeeId)
        {
            if (!_arcHistories.TryGetValue(employeeId, out ArcHistory history))
            {
                return;
            }
            
            if (history.cooldownDaysRemaining > 0)
            {
                history.cooldownDaysRemaining--;
            }
            else
            {
                float recoveryRate = (1.0f - history.retriggerMultiplier) / history.recoveryDaysToBaseline;
                history.retriggerMultiplier = Mathf.Min(1.0f, history.retriggerMultiplier + recoveryRate);
            }
        }
        
        public List<ModifierDef> GetActiveArcModifiers(string employeeId)
        {
            if (!_activeArcs.TryGetValue(employeeId, out ActiveArc arc))
            {
                return new List<ModifierDef>();
            }
            
            if (arc.currentStageIndex >= arc.arcDefinition.stages.Count)
            {
                return new List<ModifierDef>();
            }
            
            ArcStage stage = arc.arcDefinition.stages[arc.currentStageIndex];
            return stage.stageModifiers;
        }
        
        public ActiveArc GetActiveArc(string employeeId)
        {
            if (_activeArcs.TryGetValue(employeeId, out ActiveArc arc))
            {
                return arc;
            }
            return null;
        }
        
        public ArcState GetArcState(string employeeId)
        {
            ActiveArc activeArc = GetActiveArc(employeeId);
            _arcHistories.TryGetValue(employeeId, out ArcHistory history);
            
            return new ArcState
            {
                activeArc = activeArc,
                history = history
            };
        }
        
        public void LoadArcState(string employeeId, ArcState state)
        {
            if (state.activeArc != null)
            {
                _activeArcs[employeeId] = state.activeArc;
            }
            
            if (state.history != null)
            {
                _arcHistories[employeeId] = state.history;
            }
        }
    }
    
    [Serializable]
    public class ActiveArc
    {
        public ArcDefinitionSO arcDefinition;
        public int currentStageIndex;
        public int daysInCurrentStage;
    }
    
    [Serializable]
    public class ArcHistory
    {
        public int cooldownDaysRemaining;
        public float retriggerMultiplier;
        public int recoveryDaysToBaseline;
    }
    
    [Serializable]
    public struct ArcState
    {
        public ActiveArc activeArc;
        public ArcHistory history;
    }
}
