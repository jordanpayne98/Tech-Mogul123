using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    public class ContractWorldEffectsManager : MonoBehaviour
    {
        private List<ContractWorldEffect> _activeEffects = new List<ContractWorldEffect>();
        private ContractBalanceConfigSO _config;
        
        public IReadOnlyList<ContractWorldEffect> ActiveEffects => _activeEffects;
        
        public void Initialize(ContractBalanceConfigSO config)
        {
            _config = config;
        }
        
        public void AddEffect(ContractWorldEffect effect)
        {
            if (effect == null || _config == null)
            {
                return;
            }
            
            if (!ValidateEffect(effect))
            {
                Debug.LogWarning($"[WorldEffects] Effect validation failed for {effect.issuingRivalId}");
                return;
            }
            
            _activeEffects.Add(effect);
            Debug.Log($"[WorldEffects] Added effect: {effect.component} +{effect.magnitude:F2} for {effect.issuingRivalId} in {effect.targetCategoryId}, {effect.durationQuarters}Q");
        }
        
        public void ProcessQuarterTick()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                _activeEffects[i].quartersRemaining--;
                
                if (_activeEffects[i].IsExpired)
                {
                    Debug.Log($"[WorldEffects] Effect expired: {_activeEffects[i].component} for {_activeEffects[i].issuingRivalId}");
                    _activeEffects.RemoveAt(i);
                }
            }
        }
        
        public float GetTotalEffectForRivalCategory(string rivalId, string categoryId)
        {
            float total = 0f;
            foreach (var effect in _activeEffects)
            {
                if (effect.issuingRivalId == rivalId && effect.targetCategoryId == categoryId && effect.IsActive)
                {
                    total += effect.magnitude;
                }
            }
            return total;
        }
        
        public float GetComponentEffectForRivalCategory(string rivalId, string categoryId, WorldEffectComponent component)
        {
            float total = 0f;
            foreach (var effect in _activeEffects)
            {
                if (effect.issuingRivalId == rivalId && 
                    effect.targetCategoryId == categoryId && 
                    effect.component == component &&
                    effect.IsActive)
                {
                    total += effect.magnitude;
                }
            }
            
            if (_config != null)
            {
                total = Mathf.Min(total, _config.maxEffectPerComponentPerCompanyCategory);
            }
            
            return total;
        }
        
        public Dictionary<WorldEffectComponent, float> GetAllComponentEffectsForRivalCategory(string rivalId, string categoryId)
        {
            var effects = new Dictionary<WorldEffectComponent, float>();
            
            foreach (WorldEffectComponent component in System.Enum.GetValues(typeof(WorldEffectComponent)))
            {
                float effect = GetComponentEffectForRivalCategory(rivalId, categoryId, component);
                if (effect > 0f)
                {
                    effects[component] = effect;
                }
            }
            
            return effects;
        }
        
        bool ValidateEffect(ContractWorldEffect effect)
        {
            if (_config == null) return false;
            
            float currentTotal = GetTotalEffectForRivalCategory(effect.issuingRivalId, effect.targetCategoryId);
            if (currentTotal + effect.magnitude > _config.maxTotalEffectPerCompanyCategory)
            {
                Debug.LogWarning($"[WorldEffects] Total effect cap exceeded for {effect.issuingRivalId}");
                return false;
            }
            
            float currentComponent = GetComponentEffectForRivalCategory(effect.issuingRivalId, effect.targetCategoryId, effect.component);
            if (currentComponent + effect.magnitude > _config.maxEffectPerComponentPerCompanyCategory)
            {
                Debug.LogWarning($"[WorldEffects] Component effect cap exceeded for {effect.component}");
                return false;
            }
            
            if (effect.durationQuarters > _config.maxEffectDurationQuarters)
            {
                effect.durationQuarters = _config.maxEffectDurationQuarters;
                effect.quartersRemaining = _config.maxEffectDurationQuarters;
            }
            
            return true;
        }
        
        public void RemoveEffectsForRival(string rivalId)
        {
            _activeEffects.RemoveAll(e => e.issuingRivalId == rivalId);
        }
        
        public void ClearAllEffects()
        {
            _activeEffects.Clear();
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: Print Active Effects")]
        void DebugPrintEffects()
        {
            Debug.Log($"=== Active World Effects ({_activeEffects.Count}) ===");
            foreach (var effect in _activeEffects)
            {
                Debug.Log($"{effect.issuingRivalId} → {effect.targetCategoryId}: {effect.component} +{effect.magnitude:F2} ({effect.quartersRemaining}Q remaining)");
            }
        }
        #endif
    }
}
