using System;
using System.Collections.Generic;
using TechMogul.Systems;
using TechMogul.Products;
using TechMogul.Contracts;

namespace TechMogul.Core.Save
{
    [Serializable]
    public class SaveData
    {
        public int saveVersion = SaveConstants.CURRENT_VERSION;
        public string saveName;
        public string saveTimestamp;
        
        public GameManagerData gameManager;
        public TimeSystemData timeSystem;
        public TechnologySystemData technologySystem;
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
        public int dayIndex;
        public TimeSpeed currentSpeed;
    }
    
    [Serializable]
    public class TechnologySystemData
    {
        public float globalDriftOffset;
    }
    
    [Serializable]
    public class EmployeeSystemData
    {
        public List<SerializableEmployee> employees;
        public int employeeCounter;
        public List<PendingSeverancePayment> pendingSeverancePayments;
    }
    
    [Serializable]
    public class PendingSeverancePayment
    {
        public string employeeName;
        public float salary;
    }
    
    [Serializable]
    public class ProductSystemData
    {
        public List<SerializableProduct> products;
    }
    
    [Serializable]
    public class ContractSystemData
    {
        public List<SerializableContract> contracts;
        public int daysSinceLastGeneration;
    }
    
    [Serializable]
    public class ReputationSystemData
    {
        public float currentReputation;
    }
    
    public static class SaveDataMigration
    {
        public static void MigrateSaveData(SaveData data)
        {
            if (data.saveVersion < SaveConstants.CURRENT_VERSION)
            {
                // Future migration steps will go here
                // Example:
                // if (data.saveVersion < 2) { MigrateV1ToV2(data); }
                // if (data.saveVersion < 3) { MigrateV2ToV3(data); }
                
                data.saveVersion = SaveConstants.CURRENT_VERSION;
            }
        }
    }
}
