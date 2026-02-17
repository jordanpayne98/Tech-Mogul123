using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using TechMogul.Systems;
using TechMogul.Products;
using TechMogul.Contracts;

namespace TechMogul.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "savegame.json";
        [SerializeField] private bool prettyPrintJson = true;
        
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        void OnEnable()
        {
            EventBus.Subscribe<RequestSaveGameEvent>(HandleSaveRequest);
            EventBus.Subscribe<RequestLoadGameEvent>(HandleLoadRequest);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<RequestSaveGameEvent>(HandleSaveRequest);
            EventBus.Unsubscribe<RequestLoadGameEvent>(HandleLoadRequest);
        }
        
        void HandleSaveRequest(RequestSaveGameEvent evt)
        {
            SaveGame();
        }
        
        void HandleLoadRequest(RequestLoadGameEvent evt)
        {
            LoadGame();
        }
        
        public void SaveGame()
        {
            try
            {
                SaveData saveData = GatherSaveData();
                
                string json = JsonUtility.ToJson(saveData, prettyPrintJson);
                File.WriteAllText(SaveFilePath, json);
                
                Debug.Log($"Game saved successfully to: {SaveFilePath}");
                EventBus.Publish(new OnGameSavedEvent { Success = true });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
                EventBus.Publish(new OnGameSavedEvent { Success = false });
            }
        }
        
        public void LoadGame()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.LogWarning("No save file found");
                EventBus.Publish(new OnGameLoadedEvent { Success = false });
                return;
            }
            
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                
                ApplySaveData(saveData);
                
                Debug.Log($"Game loaded successfully from: {SaveFilePath}");
                EventBus.Publish(new OnGameLoadedEvent { Success = true });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                EventBus.Publish(new OnGameLoadedEvent { Success = false });
            }
        }
        
        public bool SaveFileExists()
        {
            return File.Exists(SaveFilePath);
        }
        
        SaveData GatherSaveData()
        {
            SaveData data = new SaveData
            {
                saveVersion = 1,
                saveName = "Auto Save",
                saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            if (GameManager.Instance != null)
            {
                data.gameManager = new GameManagerData
                {
                    currentCash = GameManager.Instance.CurrentCash,
                    isGameRunning = GameManager.Instance.IsGameRunning
                };
            }
            
            TimeSystem timeSystem = FindFirstObjectByType<TimeSystem>();
            if (timeSystem != null)
            {
                data.timeSystem = new TimeSystemData
                {
                    year = timeSystem.CurrentDate.Year,
                    month = timeSystem.CurrentDate.Month,
                    day = timeSystem.CurrentDate.Day,
                    currentSpeed = timeSystem.CurrentSpeed
                };
            }
            
            if (EmployeeSystem.Instance != null)
            {
                data.employeeSystem = new EmployeeSystemData
                {
                    employees = new List<SerializableEmployee>(),
                    employeeCounter = EmployeeSystem.Instance.Employees.Count
                };
                
                foreach (var employee in EmployeeSystem.Instance.Employees)
                {
                    data.employeeSystem.employees.Add(SerializableEmployee.FromEmployee(employee));
                }
            }
            
            ProductSystem productSystem = FindFirstObjectByType<ProductSystem>();
            if (productSystem != null)
            {
                data.productSystem = new ProductSystemData
                {
                    products = new List<SerializableProduct>()
                };
                
                foreach (var product in productSystem.Products)
                {
                    data.productSystem.products.Add(SerializableProduct.FromProduct(product));
                }
            }
            
            ContractSystem contractSystem = FindFirstObjectByType<ContractSystem>();
            if (contractSystem != null)
            {
                data.contractSystem = new ContractSystemData
                {
                    contracts = new List<SerializableContract>(),
                    daysSinceLastGeneration = 0
                };
                
                foreach (var contract in contractSystem.Contracts)
                {
                    data.contractSystem.contracts.Add(SerializableContract.FromContract(contract));
                }
            }
            
            ReputationSystem reputationSystem = FindFirstObjectByType<ReputationSystem>();
            if (reputationSystem != null)
            {
                data.reputationSystem = new ReputationSystemData
                {
                    currentReputation = reputationSystem.CurrentReputation
                };
            }
            
            return data;
        }
        
        void ApplySaveData(SaveData data)
        {
            EventBus.Publish(new OnBeforeLoadGameEvent());
            
            if (data.gameManager != null && GameManager.Instance != null)
            {
                EventBus.Publish(new RequestSetCashEvent { Amount = data.gameManager.currentCash });
            }
            
            if (data.timeSystem != null)
            {
                EventBus.Publish(new RequestSetDateEvent
                {
                    Year = data.timeSystem.year,
                    Month = data.timeSystem.month,
                    Day = data.timeSystem.day
                });
                
                EventBus.Publish(new RequestChangeSpeedEvent { Speed = data.timeSystem.currentSpeed });
            }
            
            if (data.employeeSystem != null)
            {
                EventBus.Publish(new RequestLoadEmployeesEvent
                {
                    Employees = data.employeeSystem.employees
                });
            }
            
            if (data.productSystem != null)
            {
                EventBus.Publish(new RequestLoadProductsEvent
                {
                    Products = data.productSystem.products
                });
            }
            
            if (data.contractSystem != null)
            {
                EventBus.Publish(new RequestLoadContractsEvent
                {
                    Contracts = data.contractSystem.contracts
                });
            }
            
            if (data.reputationSystem != null)
            {
                EventBus.Publish(new RequestSetReputationEvent
                {
                    Reputation = data.reputationSystem.currentReputation
                });
            }
            
            EventBus.Publish(new OnAfterLoadGameEvent());
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: Show Save Path")]
        void DebugShowSavePath()
        {
            Debug.Log($"Save file path: {SaveFilePath}");
            Debug.Log($"Save file exists: {SaveFileExists()}");
        }
        
        [ContextMenu("Debug: Delete Save File")]
        void DebugDeleteSaveFile()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("Save file deleted");
            }
            else
            {
                Debug.Log("No save file to delete");
            }
        }
        #endif
    }
    
    public class RequestSaveGameEvent { }
    
    public class RequestLoadGameEvent { }
    
    public class OnGameSavedEvent
    {
        public bool Success;
    }
    
    public class OnGameLoadedEvent
    {
        public bool Success;
    }
    
    public class OnBeforeLoadGameEvent { }
    
    public class OnAfterLoadGameEvent { }
    
    public class RequestSetCashEvent
    {
        public float Amount;
    }
    
    public class RequestSetDateEvent
    {
        public int Year;
        public int Month;
        public int Day;
    }
    
    public class RequestLoadEmployeesEvent
    {
        public List<SerializableEmployee> Employees;
    }
    
    public class RequestLoadProductsEvent
    {
        public List<SerializableProduct> Products;
    }
    
    public class RequestLoadContractsEvent
    {
        public List<SerializableContract> Contracts;
    }
    
    public class RequestSetReputationEvent
    {
        public float Reputation;
    }
}
