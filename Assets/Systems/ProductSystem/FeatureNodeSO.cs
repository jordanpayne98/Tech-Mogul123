using UnityEngine;
using TechMogul.Core;

namespace TechMogul.Products
{
    [CreateAssetMenu(fileName = "New Feature", menuName = "TechMogul/Feature Node")]
    public class FeatureNodeSO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [Tooltip("Unique stable ID for save/load (e.g., 'feature.3d_rendering')")]
        public string id;
        public string featureName;
        [TextArea(2, 4)] public string description;

        [Header("Time Lock")]
        [Tooltip("Year this feature unlocks (enforced later via EraSystem)")]
        public int unlockYear = 1980;

        [Header("Dependencies")]
        [Tooltip("Features required from the same product")]
        public FeatureNodeSO[] requires;
        
        [Tooltip("Features required from external products (cross-product)")]
        public FeatureNodeSO[] requiresExternal;

        [Header("Replacement Tree")]
        [Tooltip("This feature replaces/upgrades these older features")]
        public FeatureNodeSO[] replaces;
        
        [Tooltip("This feature is an upgrade of")]
        public FeatureNodeSO upgradeOf;

        [Header("Exclusivity")]
        [Tooltip("Only one feature from this group can be selected (e.g., 'audio_output')")]
        public string exclusiveGroup;

        [Header("Balance Stats")]
        [Range(1, 100)] public float devCost = 10f;
        [Range(1, 30)] public int devTime = 5;
        [Range(0, 100)] public float skillRequirement = 50f;

        [Header("Impact Stats")]
        [Range(-50f, 50f)] public float stabilityImpact = 0f;
        [Range(-50f, 50f)] public float usabilityImpact = 0f;
        [Range(0f, 100f)] public float innovationImpact = 10f;
        [Range(0f, 100f)] public float featureValue = 20f;
        [Range(0f, 100f)] public float marketValue = 15f;

        public string Id => id;

        void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"{name} missing stable ID");
            }

            foreach (var dep in requires)
            {
                if (dep == this)
                {
                    Debug.LogError($"{name} cannot require itself");
                }
            }

            if (upgradeOf != null && !System.Array.Exists(replaces, f => f == upgradeOf))
            {
                Debug.LogWarning($"{name} is upgradeOf {upgradeOf.name} but doesn't list it in replaces array");
            }
        }
    }
}
