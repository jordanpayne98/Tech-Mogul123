using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechMogul.Traits
{
    [CreateAssetMenu(fileName = "New Arc", menuName = "TechMogul/Arc Definition")]
    public class ArcDefinitionSO : ScriptableObject
    {
        [Header("Arc Identity")]
        public string arcId;
        public string arcName;
        [TextArea(2, 3)]
        public string description;
        
        [Header("Trigger")]
        public ConditionDef startCondition;
        [Range(0.001f, 0.1f)]
        [Tooltip("Daily probability of arc starting when conditions met")]
        public float baseStartChancePerDay = 0.01f;
        
        [Header("Stages")]
        public List<ArcStage> stages = new List<ArcStage>();
        
        [Header("Resolution")]
        [Tooltip("Days of cooldown after arc completes")]
        public int cooldownDays = 30;
        [Range(0.1f, 1f)]
        [Tooltip("Multiplier to retrigger chance after cooldown")]
        public float postCooldownChanceMultiplier = 0.4f;
        [Tooltip("Days to gradually return multiplier to 1.0")]
        public int recoveryDaysToBaseline = 90;
        
        void OnValidate()
        {
            if (string.IsNullOrEmpty(arcId))
            {
                Debug.LogWarning($"Arc '{name}' missing arcId");
            }
            
            if (stages.Count == 0)
            {
                Debug.LogWarning($"Arc '{name}' has no stages");
            }
        }
    }
    
    [Serializable]
    public class ArcStage
    {
        public string stageId;
        public string stageName;
        [TextArea(2, 3)]
        public string description;
        
        [Header("Duration")]
        [Tooltip("Min days in this stage before advancing")]
        public int minDays = 5;
        [Tooltip("Max days in this stage before auto-advancing")]
        public int maxDays = 15;
        
        [Header("Stage Effects")]
        public List<ModifierDef> stageModifiers = new List<ModifierDef>();
        
        [Header("Events")]
        public List<ArcEventDef> possibleEvents = new List<ArcEventDef>();
    }
    
    [Serializable]
    public class ArcEventDef
    {
        public string eventId;
        public string eventDescription;
        [Range(0f, 1f)]
        public float weight = 0.5f;
        public bool isWarning;
        public bool canInterrupt;
        public List<string> interventionOptions = new List<string>();
    }
}
