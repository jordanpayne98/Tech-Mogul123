using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Systems;
using TechMogul.Products;
using TechMogul.Contracts;

namespace TechMogul.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        [Header("Save Settings")]
        [SerializeField] private int maxSaveSlots = 3;
        [SerializeField] private bool prettyPrintJson = true;
        
        private string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            EnsureSaveDirectoryExists();
        }
        
        void OnEnable()
        {
            EventBus.Subscribe<RequestSaveGameToSlotEvent>(HandleSaveToSlotRequest);
            EventBus.Subscribe<RequestLoadGameFromSlotEvent>(HandleLoadFromSlotRequest);
            EventBus.Subscribe<RequestDeleteSaveSlotEvent>(HandleDeleteSlotRequest);
            EventBus.Subscribe<RequestGetSaveSlotsEvent>(HandleGetSaveSlotsRequest);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<RequestSaveGameToSlotEvent>(HandleSaveToSlotRequest);
            EventBus.Unsubscribe<RequestLoadGameFromSlotEvent>(HandleLoadFromSlotRequest);
            EventBus.Unsubscribe<RequestDeleteSaveSlotEvent>(HandleDeleteSlotRequest);
            EventBus.Unsubscribe<RequestGetSaveSlotsEvent>(HandleGetSaveSlotsRequest);
        }
        
        void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
                Debug.Log($"Created save directory: {SaveDirectory}");
            }
        }
        
        void HandleSaveToSlotRequest(RequestSaveGameToSlotEvent evt)
        {
            SaveGameToSlot(evt.SlotIndex, evt.SaveName);
        }
        
        void HandleLoadFromSlotRequest(RequestLoadGameFromSlotEvent evt)
        {
            LoadGameFromSlot(evt.SlotIndex);
        }
        
        void HandleDeleteSlotRequest(RequestDeleteSaveSlotEvent evt)
        {
            DeleteSaveSlot(evt.SlotIndex);
        }
        
        void HandleGetSaveSlotsRequest(RequestGetSaveSlotsEvent evt)
        {
            var slots = GetAllSaveSlots();
            EventBus.Publish(new OnSaveSlotsReceivedEvent { SaveSlots = slots });
        }
        
        public void SaveGameToSlot(int slotIndex, string saveName)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid save slot index: {slotIndex}");
                EventBus.Publish(new OnGameSavedEvent { Success = false });
                return;
            }
            
            try
            {
                SaveData saveData = GatherSaveData();
                saveData.saveName = saveName;
                saveData.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                string json = JsonUtility.ToJson(saveData, prettyPrintJson);
                string filePath = GetSaveFilePath(slotIndex);
                File.WriteAllText(filePath, json);
                
                Debug.Log($"Game saved to slot {slotIndex}: {filePath}");
                EventBus.Publish(new OnGameSavedEvent { Success = true, SlotIndex = slotIndex });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game to slot {slotIndex}: {e.Message}");
                EventBus.Publish(new OnGameSavedEvent { Success = false });
            }
        }
        
        public void LoadGameFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid save slot index: {slotIndex}");
                EventBus.Publish(new OnGameLoadedEvent { Success = false });
                return;
            }
            
            string filePath = GetSaveFilePath(slotIndex);
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"No save file found in slot {slotIndex}");
                EventBus.Publish(new OnGameLoadedEvent { Success = false });
                return;
            }
            
            try
            {
                string json = File.ReadAllText(filePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                
                ApplySaveData(saveData);
                
                Debug.Log($"Game loaded from slot {slotIndex}: {filePath}");
                EventBus.Publish(new OnGameLoadedEvent { Success = true, SlotIndex = slotIndex });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game from slot {slotIndex}: {e.Message}");
                EventBus.Publish(new OnGameLoadedEvent { Success = false });
            }
        }
        
        public void DeleteSaveSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid save slot index: {slotIndex}");
                return;
            }
            
            string filePath = GetSaveFilePath(slotIndex);
            
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Debug.Log($"Deleted save in slot {slotIndex}");
                    EventBus.Publish(new OnSaveSlotDeletedEvent { SlotIndex = slotIndex });
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to delete save slot {slotIndex}: {e.Message}");
                }
            }
        }
        
        public List<SaveSlotInfo> GetAllSaveSlots()
        {
            var slots = new List<SaveSlotInfo>();
            
            for (int i = 0; i < maxSaveSlots; i++)
            {
                slots.Add(GetSaveSlotInfo(i));
            }
            
            return slots;
        }
        
        public SaveSlotInfo GetSaveSlotInfo(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            
            var slotInfo = new SaveSlotInfo
            {
                SlotIndex = slotIndex,
                IsEmpty = true
            };
            
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                    
                    slotInfo.IsEmpty = false;
                    slotInfo.SaveName = saveData.saveName;
                    slotInfo.SaveTimestamp = saveData.saveTimestamp;
                    slotInfo.Cash = saveData.gameManager?.currentCash ?? 0;
                    slotInfo.Year = saveData.timeSystem?.year ?? 0;
                    slotInfo.Month = saveData.timeSystem?.month ?? 0;
                    slotInfo.Day = saveData.timeSystem?.day ?? 0;
                    slotInfo.EmployeeCount = saveData.employeeSystem?.employees?.Count ?? 0;
                    slotInfo.Reputation = saveData.reputationSystem?.currentReputation ?? 0;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to read save slot {slotIndex}: {e.Message}");
                    slotInfo.IsEmpty = true;
                }
            }
            
            return slotInfo;
        }
        
        string GetSaveFilePath(int slotIndex)
        {
            return Path.Combine(SaveDirectory, $"save_{slotIndex}.json");
        }
        
        public bool HasAnySaveFiles()
        {
            return GetAllSaveSlots().Any(s => !s.IsEmpty);
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
        [ContextMenu("Debug: Show Save Directory")]
        void DebugShowSaveDirectory()
        {
            Debug.Log($"Save directory: {SaveDirectory}");
            Debug.Log($"Number of saves: {GetAllSaveSlots().Count(s => !s.IsEmpty)}");
        }
        
        [ContextMenu("Debug: List All Saves")]
        void DebugListAllSaves()
        {
            var slots = GetAllSaveSlots();
            foreach (var slot in slots.Where(s => !s.IsEmpty))
            {
                Debug.Log($"Slot {slot.SlotIndex}: {slot.SaveName} - {slot.SaveTimestamp}");
            }
        }
        
        [ContextMenu("Debug: Delete All Saves")]
        void DebugDeleteAllSaves()
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                DeleteSaveSlot(i);
            }
            Debug.Log("All saves deleted");
        }
        #endif
    }
    
    [Serializable]
    public class SaveSlotInfo
    {
        public int SlotIndex;
        public bool IsEmpty;
        public string SaveName;
        public string SaveTimestamp;
        public float Cash;
        public int Year;
        public int Month;
        public int Day;
        public int EmployeeCount;
        public float Reputation;
    }
    
    public class RequestSaveGameToSlotEvent
    {
        public int SlotIndex;
        public string SaveName;
    }
    
    public class RequestLoadGameFromSlotEvent
    {
        public int SlotIndex;
    }
    
    public class RequestDeleteSaveSlotEvent
    {
        public int SlotIndex;
    }
    
    public class RequestGetSaveSlotsEvent { }
    
    public class OnSaveSlotsReceivedEvent
    {
        public List<SaveSlotInfo> SaveSlots;
    }
    
    public class OnSaveSlotDeletedEvent
    {
        public int SlotIndex;
    }
    
    public class OnGameSavedEvent
    {
        public bool Success;
        public int SlotIndex;
    }
    
    public class OnGameLoadedEvent
    {
        public bool Success;
        public int SlotIndex;
    }
    
    public class RequestSaveGameEvent { }
    
    public class RequestLoadGameEvent { }
    
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
