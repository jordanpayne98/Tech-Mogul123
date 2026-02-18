using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TechMogul.Products
{
    public static class FeatureGraphValidator
    {
        public static bool ValidateFeatureSelection(
            List<FeatureNodeSO> selectedFeatures,
            FeatureNodeSO featureToAdd,
            out string errorMessage)
        {
            errorMessage = null;

            if (featureToAdd == null)
            {
                errorMessage = "Feature is null";
                return false;
            }

            if (selectedFeatures == null)
            {
                selectedFeatures = new List<FeatureNodeSO>();
            }

            if (!ValidateDependencies(selectedFeatures, featureToAdd, out errorMessage))
            {
                return false;
            }

            if (!ValidateExclusivity(selectedFeatures, featureToAdd, out errorMessage))
            {
                return false;
            }

            if (!ValidateReplacement(selectedFeatures, featureToAdd, out errorMessage))
            {
                return false;
            }

            return true;
        }

        static bool ValidateDependencies(List<FeatureNodeSO> selectedFeatures, FeatureNodeSO feature, out string errorMessage)
        {
            errorMessage = null;

            if (feature.requires != null && feature.requires.Length > 0)
            {
                foreach (var required in feature.requires)
                {
                    if (required == null) continue;
                    
                    if (!selectedFeatures.Contains(required))
                    {
                        errorMessage = $"Missing required feature: {required.featureName}";
                        return false;
                    }
                }
            }

            return true;
        }

        static bool ValidateExclusivity(List<FeatureNodeSO> selectedFeatures, FeatureNodeSO feature, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrEmpty(feature.exclusiveGroup))
            {
                return true;
            }

            var conflicting = selectedFeatures.FirstOrDefault(f => 
                !string.IsNullOrEmpty(f.exclusiveGroup) && 
                f.exclusiveGroup == feature.exclusiveGroup);

            if (conflicting != null)
            {
                errorMessage = $"Conflicts with {conflicting.featureName} (exclusive group: {feature.exclusiveGroup})";
                return false;
            }

            return true;
        }

        static bool ValidateReplacement(List<FeatureNodeSO> selectedFeatures, FeatureNodeSO feature, out string errorMessage)
        {
            errorMessage = null;

            if (feature.replaces != null && feature.replaces.Length > 0)
            {
                var needsReplacement = feature.replaces.FirstOrDefault(r => selectedFeatures.Contains(r));
                if (needsReplacement == null && feature.upgradeOf != null)
                {
                    errorMessage = $"Cannot add {feature.featureName} without having {feature.upgradeOf.featureName} first";
                    return false;
                }
            }

            return true;
        }

        public static List<FeatureNodeSO> RemoveReplacedFeatures(List<FeatureNodeSO> selectedFeatures, FeatureNodeSO newFeature)
        {
            if (newFeature.replaces == null || newFeature.replaces.Length == 0)
            {
                return selectedFeatures;
            }

            var result = new List<FeatureNodeSO>(selectedFeatures);
            foreach (var replaced in newFeature.replaces)
            {
                result.Remove(replaced);
            }

            return result;
        }

        public static float CalculateTotalDevCost(List<FeatureNodeSO> features, QATierSO qaTier)
        {
            float baseCost = 0f;
            
            if (features != null)
            {
                baseCost = features.Sum(f => f.devCost);
            }

            if (qaTier != null)
            {
                baseCost += qaTier.additionalDevCost;
            }

            return baseCost;
        }

        public static int CalculateTotalDevTime(List<FeatureNodeSO> features, QATierSO qaTier)
        {
            int baseTime = 0;
            
            if (features != null)
            {
                baseTime = features.Sum(f => f.devTime);
            }

            if (qaTier != null)
            {
                baseTime = Mathf.RoundToInt(baseTime * qaTier.devTimeMultiplier);
            }

            return Mathf.Max(baseTime, 1);
        }

        public static float CalculateStabilityScore(List<FeatureNodeSO> features, QATierSO qaTier)
        {
            float stability = 50f;

            if (features != null)
            {
                stability += features.Sum(f => f.stabilityImpact);
            }

            if (qaTier != null)
            {
                stability += qaTier.stabilityBonus;
            }

            return Mathf.Clamp(stability, 0f, 100f);
        }

        public static float CalculateUsabilityScore(List<FeatureNodeSO> features, QATierSO qaTier)
        {
            float usability = 50f;

            if (features != null)
            {
                usability += features.Sum(f => f.usabilityImpact);
            }

            if (qaTier != null)
            {
                usability += qaTier.usabilityBonus;
            }

            return Mathf.Clamp(usability, 0f, 100f);
        }

        public static float CalculateInnovationScore(List<FeatureNodeSO> features, QATierSO qaTier)
        {
            float innovation = 0f;

            if (features != null)
            {
                innovation = features.Sum(f => f.innovationImpact);
            }

            if (qaTier != null)
            {
                innovation -= qaTier.innovationPenalty;
            }

            return Mathf.Clamp(innovation, 0f, 100f);
        }

        public static float CalculateFeatureValue(List<FeatureNodeSO> features)
        {
            if (features == null || features.Count == 0) return 0f;
            return features.Sum(f => f.featureValue);
        }

        public static float CalculateMarketValue(List<FeatureNodeSO> features)
        {
            if (features == null || features.Count == 0) return 0f;
            return features.Sum(f => f.marketValue);
        }
    }
}
