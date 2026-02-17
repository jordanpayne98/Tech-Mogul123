using UnityEngine;
using TechMogul.Core;
using TechMogul.Contracts;

namespace TechMogul.Systems
{
    public class ReputationSystem : MonoBehaviour
    {
        [Header("Reputation Settings")]
        [SerializeField] private float startingReputation = 0f;
        [SerializeField] private float maxReputation = 100f;
        
        [Header("Contract Completion Rewards")]
        [Tooltip("Reputation gained for completing an Easy contract")]
        [SerializeField] private float easyContractReputation = 3f;
        [Tooltip("Reputation gained for completing a Medium contract")]
        [SerializeField] private float mediumContractReputation = 6f;
        [Tooltip("Reputation gained for completing a Hard contract")]
        [SerializeField] private float hardContractReputation = 10f;
        
        [Header("Goal Failure Penalties")]
        [Tooltip("Percentage of reputation gain lost per failed goal")]
        [Range(0f, 1f)]
        [SerializeField] private float goalFailurePenaltyPercent = 0.2f; // 20% per failed goal
        
        [Header("Contract Failure Penalties")]
        [Tooltip("Flat reputation loss for missing deadline")]
        [SerializeField] private float missedDeadlinePenalty = 5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private float currentReputation;
        
        public float CurrentReputation => currentReputation;
        public float MaxReputation => maxReputation;
        public int StarRating => GetStarRating();
        
        void Awake()
        {
            currentReputation = startingReputation;
        }
        
        void OnEnable()
        {
            EventBus.Subscribe<OnContractCompletedEvent>(HandleContractCompleted);
            EventBus.Subscribe<OnContractFailedEvent>(HandleContractFailed);
            EventBus.Subscribe<RequestSetReputationEvent>(HandleSetReputation);
            EventBus.Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<OnContractCompletedEvent>(HandleContractCompleted);
            EventBus.Unsubscribe<OnContractFailedEvent>(HandleContractFailed);
            EventBus.Unsubscribe<RequestSetReputationEvent>(HandleSetReputation);
            EventBus.Unsubscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            currentReputation = startingReputation;
            Debug.Log("ReputationSystem reset for new game");
            
            PublishReputationUpdate(0);
        }
        
        void HandleSetReputation(RequestSetReputationEvent evt)
        {
            float oldReputation = currentReputation;
            currentReputation = Mathf.Clamp(evt.Reputation, 0, maxReputation);
            Debug.Log($"Reputation set to: {currentReputation:F1}/{maxReputation} ({StarRating}★)");
            
            PublishReputationUpdate(currentReputation - oldReputation);
        }
        
        void PublishReputationUpdate(float change)
        {
            EventBus.Publish(new OnReputationChangedEvent
            {
                newReputation = currentReputation,
                change = change,
                starRating = StarRating
            });
        }
        
        void HandleContractCompleted(OnContractCompletedEvent evt)
        {
            // Get contract data
            var contractSystem = FindObjectOfType<ContractSystem>();
            if (contractSystem == null) return;
            
            var contract = contractSystem.GetContractById(evt.contractId);
            if (contract == null) return;
            
            // Calculate base reputation gain
            float baseRepGain = GetReputationForDifficulty(contract.difficulty);
            
            // Calculate penalty for failed goals
            int goalsFailed = 0;
            if (contract.goalCompletionStatus != null)
            {
                foreach (var completed in contract.goalCompletionStatus)
                {
                    if (!completed) goalsFailed++;
                }
            }
            
            // Reduce reputation gain by percentage for each failed goal
            float penaltyMultiplier = 1f - (goalsFailed * goalFailurePenaltyPercent);
            penaltyMultiplier = Mathf.Max(penaltyMultiplier, 0.1f); // Minimum 10% of base gain
            
            float actualRepGain = baseRepGain * penaltyMultiplier;
            
            AddReputation(actualRepGain);
            
            if (showDebugLogs)
            {
                string goalText = goalsFailed > 0 ? $" ({goalsFailed} goals failed, -{(1f - penaltyMultiplier) * 100f:F0}% penalty)" : "";
                Debug.Log($"Reputation: +{actualRepGain:F1} (base: {baseRepGain:F1}){goalText} | Total: {currentReputation:F1}/{maxReputation} ({StarRating}★)");
            }
        }
        
        void HandleContractFailed(OnContractFailedEvent evt)
        {
            RemoveReputation(missedDeadlinePenalty);
            
            if (showDebugLogs)
            {
                Debug.Log($"Reputation: -{missedDeadlinePenalty:F1} (missed deadline) | Total: {currentReputation:F1}/{maxReputation} ({StarRating}★)");
            }
        }
        
        float GetReputationForDifficulty(TechMogul.Data.ContractDifficulty difficulty)
        {
            switch (difficulty)
            {
                case TechMogul.Data.ContractDifficulty.Easy:
                    return easyContractReputation;
                case TechMogul.Data.ContractDifficulty.Medium:
                    return mediumContractReputation;
                case TechMogul.Data.ContractDifficulty.Hard:
                    return hardContractReputation;
                default:
                    return mediumContractReputation;
            }
        }
        
        public void AddReputation(float amount)
        {
            float oldRep = currentReputation;
            currentReputation = Mathf.Min(currentReputation + amount, maxReputation);
            
            EventBus.Publish(new OnReputationChangedEvent
            {
                newReputation = currentReputation,
                change = currentReputation - oldRep,
                starRating = StarRating
            });
        }
        
        public void RemoveReputation(float amount)
        {
            float oldRep = currentReputation;
            currentReputation = Mathf.Max(currentReputation - amount, 0f);
            
            EventBus.Publish(new OnReputationChangedEvent
            {
                newReputation = currentReputation,
                change = currentReputation - oldRep,
                starRating = StarRating
            });
        }
        
        public int GetStarRating()
        {
            // Scale stars based on percentage of max reputation
            float percentage = (currentReputation / maxReputation) * 100f;
            
            if (percentage < 10f) return 0;
            if (percentage < 25f) return 1;
            if (percentage < 45f) return 2;
            if (percentage < 70f) return 3;
            if (percentage < 90f) return 4;
            return 5;
        }
        
        public float GetEmployeeQualityMultiplier()
        {
            // Reputation directly determines employee skill range
            // This returns the MAX skill an employee can have at this reputation level
            
            // Convert to percentage of max reputation
            float reputationPercent = (currentReputation / maxReputation) * 100f;
            
            // Map reputation percent to max skill level (slower progression)
            // With 1.3× primary boost:
            // 0% = 15 max (×1.3 = 20 primary)
            // 50% = 51 max (×1.3 = 66 primary)
            // 80% = 73 max (×1.3 = 95 primary) ← 95 skill threshold
            // 90% = 80 max (×1.3 = 104→100 primary) ← 100 skill threshold
            // 100% = 88 max (×1.3 = 114→100 primary)
            
            float maxSkill = 15f + (reputationPercent * 0.725f);
            return maxSkill;
        }
        
        public float GetEmployeeMinSkill()
        {
            // Minimum skill also scales with reputation (but more slowly)
            float reputationPercent = (currentReputation / maxReputation) * 100f;
            float minSkill = reputationPercent * 0.15f; // 0% = 0, 100% = 15
            return minSkill;
        }
    }
}
