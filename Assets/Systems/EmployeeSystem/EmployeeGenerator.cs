using UnityEngine;
using TechMogul.Data;
using TechMogul.Contracts;
using TechMogul.Traits;
using System.Collections.Generic;

namespace TechMogul.Systems
{
    public class EmployeeGenerator
    {
        private readonly IRng _rng;
        private TraitGenerator _traitGenerator;
        
        public EmployeeGenerator(IRng rng)
        {
            _rng = rng;
        }
        
        public EmployeeGenerator() : this(new UnityRng())
        {
        }
        
        public void SetTraitDatabase(TraitDatabaseSO database)
        {
            if (database != null)
            {
                _traitGenerator = new TraitGenerator(database);
            }
        }
        
        public GeneratedEmployeeData GenerateEmployee(RoleSO role, float minSkillCap, float maxSkillCap)
        {
            if (role == null)
            {
                Debug.LogError("EmployeeGenerator: RoleSO is null");
                return null;
            }
            
            float primarySkillValue = _rng.Range(minSkillCap, maxSkillCap);
            float secondarySkillMultiplier = _rng.Range(0.4f, 0.7f);
            float tertiarySkillMultiplier = _rng.Range(0.3f, 0.6f);
            
            float devSkill = 0f;
            float designSkill = 0f;
            float marketingSkill = 0f;
            
            switch (role.roleName)
            {
                case "Developer":
                    devSkill = primarySkillValue * 1.3f;
                    designSkill = primarySkillValue * secondarySkillMultiplier;
                    marketingSkill = primarySkillValue * tertiarySkillMultiplier;
                    break;
                    
                case "Designer":
                    devSkill = primarySkillValue * tertiarySkillMultiplier;
                    designSkill = primarySkillValue * 1.3f;
                    marketingSkill = primarySkillValue * secondarySkillMultiplier;
                    break;
                    
                case "Marketer":
                    devSkill = primarySkillValue * tertiarySkillMultiplier;
                    designSkill = primarySkillValue * secondarySkillMultiplier;
                    marketingSkill = primarySkillValue * 1.3f;
                    break;
                    
                default:
                    devSkill = primarySkillValue;
                    designSkill = primarySkillValue * secondarySkillMultiplier;
                    marketingSkill = primarySkillValue * tertiarySkillMultiplier;
                    break;
            }
            
            float boostedMax = maxSkillCap * 1.3f;
            
            devSkill = Mathf.Round(Mathf.Clamp(devSkill, 1f, boostedMax));
            designSkill = Mathf.Round(Mathf.Clamp(designSkill, 1f, maxSkillCap));
            marketingSkill = Mathf.Round(Mathf.Clamp(marketingSkill, 1f, maxSkillCap));
            
            devSkill = Mathf.Max(devSkill, 1f);
            designSkill = Mathf.Max(designSkill, 1f);
            marketingSkill = Mathf.Max(marketingSkill, 1f);
            
            float morale = _rng.Range(70f, 90f);
            float burnout = _rng.Range(0f, 20f);
            float stress = _rng.Range(0f, 15f);
            float fatigue = _rng.Range(0f, 10f);
            
            float avgSkill = (devSkill + designSkill + marketingSkill) / 3f;
            float productivity = 50f + (avgSkill / 2f);
            float qualityContribution = avgSkill;
            
            float monthlySalary = CalculateSalaryFromSkills(devSkill, designSkill, marketingSkill);
            
            EmployeeTraits traits = GenerateTraits();
            
            return new GeneratedEmployeeData
            {
                devSkill = devSkill,
                designSkill = designSkill,
                marketingSkill = marketingSkill,
                morale = morale,
                burnout = burnout,
                stress = stress,
                fatigue = fatigue,
                productivity = productivity,
                qualityContribution = qualityContribution,
                monthlySalary = monthlySalary,
                majorTraitId = traits.majorTrait != null ? traits.majorTrait.id : "",
                minorTraitIds = GetTraitIds(traits.minorTraits)
            };
        }
        
        EmployeeTraits GenerateTraits()
        {
            if (_traitGenerator != null)
            {
                return _traitGenerator.GenerateTraits();
            }
            return new EmployeeTraits { minorTraits = new List<TraitDefinitionSO>() };
        }
        
        List<string> GetTraitIds(List<TraitDefinitionSO> traits)
        {
            List<string> ids = new List<string>();
            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i] != null)
                {
                    ids.Add(traits[i].id);
                }
            }
            return ids;
        }
        
        float CalculateSalaryFromSkills(float devSkill, float designSkill, float marketingSkill)
        {
            float averageSkill = (devSkill + designSkill + marketingSkill) / 3f;
            
            float baseSalaryPerSkill = 50f;
            
            float adjustedSkill = averageSkill;
            if (averageSkill > 60f)
            {
                float excess = averageSkill - 60f;
                adjustedSkill = 60f + (excess * 0.65f);
            }
            
            float calculatedSalary = adjustedSkill * baseSalaryPerSkill;
            
            float primarySkill = Mathf.Max(devSkill, Mathf.Max(designSkill, marketingSkill));
            float specializationBonus = (primarySkill / 100f) * calculatedSalary * 0.12f;
            calculatedSalary += specializationBonus;
            
            float variance = calculatedSalary * 0.08f;
            float finalSalary = calculatedSalary + _rng.Range(-variance, variance);
            
            finalSalary = Mathf.Max(finalSalary, 500f);
            
            return Mathf.Round(finalSalary / 50f) * 50f;
        }
    }
    
    public class GeneratedEmployeeData
    {
        public float devSkill;
        public float designSkill;
        public float marketingSkill;
        public float morale;
        public float burnout;
        public float stress;
        public float fatigue;
        public float productivity;
        public float qualityContribution;
        public float monthlySalary;
        public string majorTraitId;
        public List<string> minorTraitIds;
    }
}
