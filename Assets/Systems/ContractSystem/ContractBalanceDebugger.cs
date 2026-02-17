using UnityEngine;
using TechMogul.Core;
using TechMogul.Systems;
using TechMogul.Contracts;

namespace TechMogul.Tools
{
    /// <summary>
    /// Debug utility to test and verify contract balance at different reputation levels
    /// </summary>
    public class ContractBalanceDebugger : MonoBehaviour
    {
        [Header("References")]
        private ReputationSystem reputationSystem;
        
        void Awake()
        {
            reputationSystem = FindFirstObjectByType<ReputationSystem>();
        }
        
        [ContextMenu("Test: Show Balance at Current Reputation")]
        void ShowCurrentBalance()
        {
            if (reputationSystem == null)
            {
                UnityEngine.Debug.LogError("ReputationSystem not found!");
                return;
            }
            
            float currentRep = reputationSystem.CurrentReputation;
            float maxRep = reputationSystem.MaxReputation;
            float repPercent = (currentRep / maxRep) * 100f;
            
            float employeeMax = reputationSystem.GetEmployeeQualityMultiplier();
            float employeeMin = reputationSystem.GetEmployeeMinSkill();
            float employeePrimary = employeeMax * 1.3f;
            
            // Calculate contract multiplier (replicate logic from ContractData.cs)
            float contractMultiplier;
            if (repPercent < 20f)
                contractMultiplier = 2.0f - (repPercent / 20f) * 0.2f;
            else if (repPercent < 50f)
                contractMultiplier = 1.8f - ((repPercent - 20f) / 30f) * 0.15f;
            else
                contractMultiplier = 1.65f - ((repPercent - 50f) / 50f) * 0.09f;
            
            float contractScaling = (employeeMax * contractMultiplier) / 100f;
            
            UnityEngine.Debug.Log($"=== BALANCE CHECK at {currentRep:F0}/{maxRep:F0} reputation ({repPercent:F1}%) ===");
            UnityEngine.Debug.Log($"Employee Max Skill: {employeeMax:F1}");
            UnityEngine.Debug.Log($"Employee Min Skill: {employeeMin:F1}");
            UnityEngine.Debug.Log($"Primary Skill (1.3×): {employeePrimary:F1}");
            UnityEngine.Debug.Log($"Contract Multiplier: {contractMultiplier:F2}×");
            UnityEngine.Debug.Log($"Contract Scaling: {contractScaling:F2}×");
            UnityEngine.Debug.Log($"Contract Range (Easy template 100): {100 * 0.5f * contractScaling * 0.85f:F1} - {100 * 0.5f * contractScaling * 1.15f:F1}");
            UnityEngine.Debug.Log($"Estimated Team Size: {GetEstimatedTeamSize(contractScaling, employeePrimary)}");
        }
        
        [ContextMenu("Test: Simulate All Reputation Levels")]
        void SimulateAllLevels()
        {
            if (reputationSystem == null)
            {
                UnityEngine.Debug.LogError("ReputationSystem not found!");
                return;
            }
            
            float maxRep = reputationSystem.MaxReputation;
            
            UnityEngine.Debug.Log("=== CONTRACT BALANCE SIMULATION ===\n");
            
            float[] testReps = { 0f, 50f, 100f, 150f, 200f, 250f, 300f, 350f, 400f, 450f, 500f };
            
            foreach (float rep in testReps)
            {
                float actualRep = (rep / 500f) * maxRep;
                SimulateRepLevel(actualRep, maxRep);
            }
        }
        
        void SimulateRepLevel(float reputation, float maxReputation)
        {
            float repPercent = (reputation / maxReputation) * 100f;
            
            // Calculate employee skills (from ReputationSystem)
            float employeeMax = 15f + (repPercent * 0.725f);
            float employeePrimary = Mathf.Min(employeeMax * 1.3f, 100f);
            
            // Calculate contract multiplier
            float contractMultiplier;
            if (repPercent < 20f)
                contractMultiplier = 2.0f - (repPercent / 20f) * 0.2f;
            else if (repPercent < 50f)
                contractMultiplier = 1.8f - ((repPercent - 20f) / 30f) * 0.15f;
            else
                contractMultiplier = 1.65f - ((repPercent - 50f) / 50f) * 0.09f;
            
            float contractScaling = (employeeMax * contractMultiplier) / 100f;
            float contractReq = 100 * 0.5f * contractScaling; // Easy template average
            
            string teamSize = GetEstimatedTeamSize(contractScaling, employeePrimary);
            int stars = GetStarRating(repPercent);
            
            UnityEngine.Debug.Log($"Rep {reputation:F0}/{maxReputation:F0} ({repPercent:F0}%) {GetStarString(stars)} | Employee: {employeePrimary:F0} | Contract: {contractReq:F0} | Team: {teamSize}");
        }
        
        string GetEstimatedTeamSize(float contractScaling, float employeePrimary)
        {
            float avgContractReq = 100 * 0.5f * contractScaling; // Easy template
            float employeesNeeded = avgContractReq / employeePrimary;
            
            if (employeesNeeded <= 1.2f) return "1";
            if (employeesNeeded <= 2.2f) return "2";
            if (employeesNeeded <= 3.2f) return "2-3";
            if (employeesNeeded <= 4.2f) return "3-4";
            return "4-5";
        }
        
        int GetStarRating(float repPercent)
        {
            if (repPercent < 10f) return 0;
            if (repPercent < 25f) return 1;
            if (repPercent < 45f) return 2;
            if (repPercent < 70f) return 3;
            if (repPercent < 90f) return 4;
            return 5;
        }
        
        string GetStarString(int stars)
        {
            return stars switch
            {
                0 => "☆☆☆☆☆",
                1 => "★☆☆☆☆",
                2 => "★★☆☆☆",
                3 => "★★★☆☆",
                4 => "★★★★☆",
                5 => "★★★★★",
                _ => "☆☆☆☆☆"
            };
        }
        
        [ContextMenu("Test: Compare Training vs Hiring Cost")]
        void CompareTrainingVsHiring()
        {
            UnityEngine.Debug.Log("=== TRAINING VS HIRING COMPARISON ===\n");
            
            // Training scenario
            UnityEngine.Debug.Log("TRAINING THROUGH CONTRACTS:");
            UnityEngine.Debug.Log("- QuickWebsite (3 XP): ~6 contracts × 15 days = 90 days for +10 skill");
            UnityEngine.Debug.Log("- MobileApp (6 XP): ~4 contracts × 30 days = 120 days for +10 skill");
            UnityEngine.Debug.Log("- Cost: Only salary (~$3K-5K/month)");
            UnityEngine.Debug.Log("- Pros: Cheap, builds loyalty, gradual");
            UnityEngine.Debug.Log("- Cons: VERY slow\n");
            
            // Hiring scenario
            UnityEngine.Debug.Log("HIRING BETTER EMPLOYEE:");
            UnityEngine.Debug.Log("- Instant skill gain: +10-20 immediately");
            UnityEngine.Debug.Log("- Upfront cost: $6K-10K (signing bonus + firing penalty)");
            UnityEngine.Debug.Log("- Ongoing cost: +20-40% higher salary");
            UnityEngine.Debug.Log("- Pros: INSTANT upgrade, exact skills needed");
            UnityEngine.Debug.Log("- Cons: Expensive, risky (RNG)\n");
            
            UnityEngine.Debug.Log("RECOMMENDATION:");
            UnityEngine.Debug.Log("- Early game (0-100 rep): Train (can't afford hiring)");
            UnityEngine.Debug.Log("- Mid game (100-300 rep): Mix both strategies");
            UnityEngine.Debug.Log("- Late game (300-500 rep): Hire (higher income allows it)");
        }
        
        [ContextMenu("Debug: Set Reputation to 0")]
        void SetReputation0()
        {
            EventBus.Publish(new RequestSetReputationEvent { Reputation = 0 });
            UnityEngine.Debug.Log("Reputation set to 0");
            ShowCurrentBalance();
        }
        
        [ContextMenu("Debug: Set Reputation to 100")]
        void SetReputation100()
        {
            EventBus.Publish(new RequestSetReputationEvent { Reputation = 100 });
            UnityEngine.Debug.Log("Reputation set to 100");
            ShowCurrentBalance();
        }
        
        [ContextMenu("Debug: Set Reputation to 250")]
        void SetReputation250()
        {
            EventBus.Publish(new RequestSetReputationEvent { Reputation = 250 });
            UnityEngine.Debug.Log("Reputation set to 250");
            ShowCurrentBalance();
        }
        
        [ContextMenu("Debug: Set Reputation to 400")]
        void SetReputation400()
        {
            EventBus.Publish(new RequestSetReputationEvent { Reputation = 400 });
            UnityEngine.Debug.Log("Reputation set to 400");
            ShowCurrentBalance();
        }
        
        [ContextMenu("Debug: Set Reputation to 500")]
        void SetReputation500()
        {
            EventBus.Publish(new RequestSetReputationEvent { Reputation = 500 });
            UnityEngine.Debug.Log("Reputation set to 500");
            ShowCurrentBalance();
        }
    }
}
