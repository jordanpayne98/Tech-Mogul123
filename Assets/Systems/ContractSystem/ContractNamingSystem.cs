using System.Collections.Generic;
using UnityEngine;

namespace TechMogul.Contracts
{
    [CreateAssetMenu(fileName = "ContractNamingSystem", menuName = "TechMogul/Contract/Naming System")]
    public class ContractNamingSystem : ScriptableObject
    {
        [System.Serializable]
        public class EraVocabulary
        {
            public string eraId;
            public List<string> moduleDevelopmentTerms;
            public List<string> complianceTerms;
            public List<string> ecosystemTerms;
            public List<string> marketingTerms;
            public List<string> optimizationTerms;
        }
        
        [System.Serializable]
        public class TechTag
        {
            public string tagName;
            public List<string> descriptors;
        }
        
        public List<EraVocabulary> eraVocabularies;
        public List<TechTag> techTags;
        
        public string GenerateContractName(ContractType type, string eraId, string categoryName, int seed)
        {
            Random.InitState(seed);
            
            EraVocabulary vocab = GetVocabularyForEra(eraId);
            if (vocab == null)
            {
                return GetFallbackName(type, categoryName);
            }
            
            List<string> terms = GetTermsForType(type, vocab);
            if (terms == null || terms.Count == 0)
            {
                return GetFallbackName(type, categoryName);
            }
            
            string baseTerm = terms[Random.Range(0, terms.Count)];
            
            TechTag tag = techTags.Count > 0 ? techTags[Random.Range(0, techTags.Count)] : null;
            string descriptor = "";
            if (tag != null && tag.descriptors.Count > 0)
            {
                descriptor = tag.descriptors[Random.Range(0, tag.descriptors.Count)] + " ";
            }
            
            return $"{descriptor}{baseTerm} - {categoryName}";
        }
        
        EraVocabulary GetVocabularyForEra(string eraId)
        {
            if (eraVocabularies == null) return null;
            
            foreach (var vocab in eraVocabularies)
            {
                if (vocab.eraId == eraId)
                {
                    return vocab;
                }
            }
            
            return eraVocabularies.Count > 0 ? eraVocabularies[eraVocabularies.Count - 1] : null;
        }
        
        List<string> GetTermsForType(ContractType type, EraVocabulary vocab)
        {
            return type switch
            {
                ContractType.ModuleDevelopment => vocab.moduleDevelopmentTerms,
                ContractType.ComplianceStandards => vocab.complianceTerms,
                ContractType.EcosystemIntegration => vocab.ecosystemTerms,
                ContractType.MarketingCampaign => vocab.marketingTerms,
                ContractType.OptimizationCost => vocab.optimizationTerms,
                _ => null
            };
        }
        
        string GetFallbackName(ContractType type, string categoryName)
        {
            string baseName = type switch
            {
                ContractType.ModuleDevelopment => "Module Development",
                ContractType.ComplianceStandards => "Compliance Work",
                ContractType.EcosystemIntegration => "Integration Project",
                ContractType.MarketingCampaign => "Marketing Campaign",
                ContractType.OptimizationCost => "Optimization Project",
                _ => "Contract"
            };
            
            return $"{baseName} - {categoryName}";
        }
    }
}
