using System;
using System.Collections.Generic;
using TechMogul.Systems;
using TechMogul.Products;
using TechMogul.Contracts;
using UnityEngine;

namespace TechMogul.Core
{
    [Serializable]
    public class SaveData
    {
        public int saveVersion = 1;
        public string saveName;
        public string saveTimestamp;
        
        public GameManagerData gameManager;
        public TimeSystemData timeSystem;
        public EmployeeSystemData employeeSystem;
        public ProductSystemData productSystem;
        public ContractSystemData contractSystem;
        public ReputationSystemData reputationSystem;
    }
    
    [Serializable]
    public class GameManagerData
    {
        public float currentCash;
        public bool isGameRunning;
    }
    
    [Serializable]
    public class TimeSystemData
    {
        public int year;
        public int month;
        public int day;
        public TimeSpeed currentSpeed;
    }
    
    [Serializable]
    public class EmployeeSystemData
    {
        public List<SerializableEmployee> employees;
        public int employeeCounter;
    }
    
    [Serializable]
    public class SerializableEmployee
    {
        public string employeeId;
        public string employeeName;
        public string roleAssetPath;
        
        public float devSkill;
        public float designSkill;
        public float marketingSkill;
        
        public List<SkillSnapshot> skillHistory;
        
        public float morale;
        public float burnout;
        
        public float monthlySalary;
        public string currentAssignment;
        public bool isAvailable;
        public bool isFired;
        public bool needsFinalPayment;
        
        public int daysSinceHired;
        public int totalProjectsCompleted;
        public int totalContractsCompleted;
        
        public static SerializableEmployee FromEmployee(Employee employee)
        {
            string rolePath = "";
            #if UNITY_EDITOR
            if (employee.role != null)
            {
                rolePath = UnityEditor.AssetDatabase.GetAssetPath(employee.role);
            }
            #endif
            
            return new SerializableEmployee
            {
                employeeId = employee.employeeId,
                employeeName = employee.employeeName,
                roleAssetPath = rolePath,
                
                devSkill = employee.devSkill,
                designSkill = employee.designSkill,
                marketingSkill = employee.marketingSkill,
                
                skillHistory = new List<SkillSnapshot>(employee.skillHistory),
                
                morale = employee.morale,
                burnout = employee.burnout,
                
                monthlySalary = employee.monthlySalary,
                currentAssignment = employee.currentAssignment,
                isAvailable = employee.isAvailable,
                isFired = employee.isFired,
                needsFinalPayment = employee.needsFinalPayment,
                
                daysSinceHired = employee.daysSinceHired,
                totalProjectsCompleted = employee.totalProjectsCompleted,
                totalContractsCompleted = employee.totalContractsCompleted
            };
        }
        
        public Employee ToEmployee()
        {
            TechMogul.Data.RoleSO role = null;
            
            #if UNITY_EDITOR
            if (!string.IsNullOrEmpty(roleAssetPath))
            {
                role = UnityEditor.AssetDatabase.LoadAssetAtPath<TechMogul.Data.RoleSO>(roleAssetPath);
            }
            #else
            if (!string.IsNullOrEmpty(roleAssetPath))
            {
                role = Resources.Load<TechMogul.Data.RoleSO>(roleAssetPath);
            }
            #endif
            
            var employee = new Employee(role, employeeName)
            {
                employeeId = this.employeeId,
                
                devSkill = this.devSkill,
                designSkill = this.designSkill,
                marketingSkill = this.marketingSkill,
                
                skillHistory = new List<SkillSnapshot>(this.skillHistory ?? new List<SkillSnapshot>()),
                
                morale = this.morale,
                burnout = this.burnout,
                
                monthlySalary = this.monthlySalary,
                currentAssignment = this.currentAssignment,
                isAvailable = this.isAvailable,
                isFired = this.isFired,
                needsFinalPayment = this.needsFinalPayment,
                
                daysSinceHired = this.daysSinceHired,
                totalProjectsCompleted = this.totalProjectsCompleted,
                totalContractsCompleted = this.totalContractsCompleted
            };
            
            return employee;
        }
    }
    
    [Serializable]
    public class ProductSystemData
    {
        public List<SerializableProduct> products;
    }
    
    [Serializable]
    public class SerializableProduct
    {
        public string productId;
        public string name;
        public string categoryAssetPath;
        
        public ProductState state;
        public float developmentProgress;
        public float targetQuality;
        public float actualQuality;
        
        public List<string> assignedEmployeeIds;
        
        public float monthlyRevenue;
        public float totalRevenue;
        public int monthsActive;
        
        public int startDay;
        public int releaseDay;
        
        public static SerializableProduct FromProduct(ProductData product)
        {
            string categoryPath = "";
            #if UNITY_EDITOR
            if (product.category != null)
            {
                categoryPath = UnityEditor.AssetDatabase.GetAssetPath(product.category);
            }
            #endif
            
            return new SerializableProduct
            {
                productId = product.productId,
                name = product.name,
                categoryAssetPath = categoryPath,
                
                state = product.state,
                developmentProgress = product.developmentProgress,
                targetQuality = product.targetQuality,
                actualQuality = product.actualQuality,
                
                assignedEmployeeIds = new List<string>(product.assignedEmployeeIds),
                
                monthlyRevenue = product.monthlyRevenue,
                totalRevenue = product.totalRevenue,
                monthsActive = product.monthsActive,
                
                startDay = product.startDay,
                releaseDay = product.releaseDay
            };
        }
        
        public ProductData ToProduct(int currentDay)
        {
            TechMogul.Data.ProductCategorySO category = null;
            
            #if UNITY_EDITOR
            if (!string.IsNullOrEmpty(categoryAssetPath))
            {
                category = UnityEditor.AssetDatabase.LoadAssetAtPath<TechMogul.Data.ProductCategorySO>(categoryAssetPath);
            }
            #else
            if (!string.IsNullOrEmpty(categoryAssetPath))
            {
                category = Resources.Load<TechMogul.Data.ProductCategorySO>(categoryAssetPath);
            }
            #endif
            
            var product = new ProductData(productId, name, category, currentDay)
            {
                state = this.state,
                developmentProgress = this.developmentProgress,
                targetQuality = this.targetQuality,
                actualQuality = this.actualQuality,
                
                assignedEmployeeIds = new List<string>(this.assignedEmployeeIds ?? new List<string>()),
                
                monthlyRevenue = this.monthlyRevenue,
                totalRevenue = this.totalRevenue,
                monthsActive = this.monthsActive,
                
                startDay = this.startDay,
                releaseDay = this.releaseDay
            };
            
            return product;
        }
    }
    
    [Serializable]
    public class ContractSystemData
    {
        public List<SerializableContract> contracts;
        public int daysSinceLastGeneration;
    }
    
    [Serializable]
    public class SerializableContract
    {
        public string contractId;
        public string clientName;
        public string templateAssetPath;
        public int difficulty;
        
        public int state;
        public float progress;
        public int daysRemaining;
        public int totalDays;
        public int daysAvailable;
        
        public List<string> assignedEmployeeIds;
        public List<SerializableGoalDefinition> selectedGoals;
        public List<bool> goalCompletionStatus;
        public List<float> goalPenalties;
        public List<float> goalTargetValues;
        
        public float basePayout;
        public float qualityBonus;
        public float totalPayout;
        
        public int startDay;
        public int completionDay;
        
        public float requiredDevSkill;
        public float requiredDesignSkill;
        public float requiredMarketingSkill;
        
        public static SerializableContract FromContract(ContractData contract)
        {
            string templatePath = "";
            #if UNITY_EDITOR
            if (contract.template != null)
            {
                templatePath = UnityEditor.AssetDatabase.GetAssetPath(contract.template);
            }
            #endif
            
            var serializable = new SerializableContract
            {
                contractId = contract.contractId,
                clientName = contract.clientName,
                templateAssetPath = templatePath,
                difficulty = (int)contract.difficulty,
                
                state = (int)contract.state,
                progress = contract.progress,
                daysRemaining = contract.daysRemaining,
                totalDays = contract.totalDays,
                daysAvailable = contract.daysAvailable,
                
                assignedEmployeeIds = new List<string>(contract.assignedEmployeeIds),
                selectedGoals = new List<SerializableGoalDefinition>(),
                goalCompletionStatus = new List<bool>(contract.goalCompletionStatus),
                goalPenalties = new List<float>(contract.goalPenalties),
                goalTargetValues = new List<float>(contract.goalTargetValues),
                
                basePayout = contract.basePayout,
                qualityBonus = contract.qualityBonus,
                totalPayout = contract.totalPayout,
                
                startDay = contract.startDay,
                completionDay = contract.completionDay,
                
                requiredDevSkill = contract.requiredDevSkill,
                requiredDesignSkill = contract.requiredDesignSkill,
                requiredMarketingSkill = contract.requiredMarketingSkill
            };
            
            foreach (var goal in contract.selectedGoals)
            {
                serializable.selectedGoals.Add(SerializableGoalDefinition.FromGoalDefinition(goal));
            }
            
            return serializable;
        }
        
        public ContractData ToContract(int currentDay)
        {
            TechMogul.Data.ContractTemplateSO template = null;
            
            #if UNITY_EDITOR
            if (!string.IsNullOrEmpty(templateAssetPath))
            {
                template = UnityEditor.AssetDatabase.LoadAssetAtPath<TechMogul.Data.ContractTemplateSO>(templateAssetPath);
            }
            #else
            if (!string.IsNullOrEmpty(templateAssetPath))
            {
                template = Resources.Load<TechMogul.Data.ContractTemplateSO>(templateAssetPath);
            }
            #endif
            
            var contract = new ContractData(contractId, clientName, template, currentDay)
            {
                difficulty = (TechMogul.Data.ContractDifficulty)this.difficulty,
                
                state = (ContractState)this.state,
                progress = this.progress,
                daysRemaining = this.daysRemaining,
                totalDays = this.totalDays,
                daysAvailable = this.daysAvailable,
                
                assignedEmployeeIds = new List<string>(this.assignedEmployeeIds ?? new List<string>()),
                selectedGoals = new List<TechMogul.Data.GoalDefinition>(),
                goalCompletionStatus = new List<bool>(this.goalCompletionStatus ?? new List<bool>()),
                goalPenalties = new List<float>(this.goalPenalties ?? new List<float>()),
                goalTargetValues = new List<float>(this.goalTargetValues ?? new List<float>()),
                
                basePayout = this.basePayout,
                qualityBonus = this.qualityBonus,
                totalPayout = this.totalPayout,
                
                startDay = this.startDay,
                completionDay = this.completionDay,
                
                requiredDevSkill = this.requiredDevSkill,
                requiredDesignSkill = this.requiredDesignSkill,
                requiredMarketingSkill = this.requiredMarketingSkill
            };
            
            if (selectedGoals != null)
            {
                foreach (var goal in selectedGoals)
                {
                    contract.selectedGoals.Add(goal.ToGoalDefinition());
                }
            }
            
            return contract;
        }
    }
    
    [Serializable]
    public class SerializableGoalDefinition
    {
        public string description;
        public int type;
        public float targetValue;
        public float penaltyPercentMin;
        public float penaltyPercentMax;
        
        public static SerializableGoalDefinition FromGoalDefinition(TechMogul.Data.GoalDefinition goal)
        {
            return new SerializableGoalDefinition
            {
                description = goal.description,
                type = (int)goal.type,
                targetValue = goal.targetValue,
                penaltyPercentMin = goal.penaltyPercentMin,
                penaltyPercentMax = goal.penaltyPercentMax
            };
        }
        
        public TechMogul.Data.GoalDefinition ToGoalDefinition()
        {
            return new TechMogul.Data.GoalDefinition
            {
                description = this.description,
                type = (TechMogul.Data.GoalType)this.type,
                targetValue = this.targetValue,
                penaltyPercentMin = this.penaltyPercentMin,
                penaltyPercentMax = this.penaltyPercentMax
            };
        }
    }
    
    [Serializable]
    public class ReputationSystemData
    {
        public float currentReputation;
    }
}
