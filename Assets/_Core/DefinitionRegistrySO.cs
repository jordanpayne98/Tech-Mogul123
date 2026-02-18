using System.Collections.Generic;
using UnityEngine;
using TechMogul.Products;
using TechMogul.Systems;

namespace TechMogul.Core
{
    [CreateAssetMenu(fileName = "DefinitionRegistry", menuName = "TechMogul/Definition Registry")]
    public class DefinitionRegistrySO : ScriptableObject
    {
        [Header("Definitions")]
        public List<TechMogul.Data.RoleSO> roles = new List<TechMogul.Data.RoleSO>();
        public List<TechMogul.Data.ProductCategorySO> productCategories = new List<TechMogul.Data.ProductCategorySO>();
        public List<TechMogul.Data.ContractTemplateSO> contractTemplates = new List<TechMogul.Data.ContractTemplateSO>();
        public List<FeatureNodeSO> features = new List<FeatureNodeSO>();
        public List<QATierSO> qaTiers = new List<QATierSO>();
        public List<EraSO> eras = new List<EraSO>();
        public List<TechnologySO> technologies = new List<TechnologySO>();
        public List<MarketCategorySO> marketCategories = new List<MarketCategorySO>();
    }
}
