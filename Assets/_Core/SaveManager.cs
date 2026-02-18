using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Systems;
using TechMogul.Products;
using TechMogul.Contracts;
using TechMogul.Core.Save;

namespace TechMogul.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        [Header("Save Settings")]
        [SerializeField] private int maxSaveSlots = 3;
        [SerializeField] private bool prettyPrintJson = true;
        
        [Header("Definition Registry")]
        [SerializeField] private DefinitionRegistrySO definitionRegistry;
        
        private IEventBus _eventBus;
        private IDefinitionResolver _definitionResolver;
        private readonly List<IDisposable> _subs = new List<IDisposable>();
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
            
            InitializeServices();
            EnsureSaveDirectoryExists();
        }
        
        void InitializeServices()
        {
            _eventBus = ServiceLocator.Instance.Get<IEventBus>();
            
            if (ServiceLocator.Instance.TryGet<IDefinitionResolver>(out IDefinitionResolver existingResolver))
            {
                _definitionResolver = existingResolver;
                Debug.Log("Using existing IDefinitionResolver from ServiceLocator");
            }
            else
            {
                if (definitionRegistry == null)
                {
                    Debug.LogError("DefinitionRegistry is not assigned in SaveManager. Save/Load will fail.");
                    return;
                }
                
                _definitionResolver = new DefinitionResolver(definitionRegistry);
                if (ServiceLocator.Instance.TryRegister<IDefinitionResolver>(_definitionResolver))
                {
                    Debug.Log("Registered IDefinitionResolver in ServiceLocator");
                }
            }
        }
        
        void OnEnable()
        {
            if (_eventBus == null)
            {
                _eventBus = ServiceLocator.Instance.Get<IEventBus>();
            }
            
            _subs.Add(_eventBus.Subscribe<RequestSaveGameToSlotEvent>(HandleSaveToSlotRequest));
            _subs.Add(_eventBus.Subscribe<RequestLoadGameFromSlotEvent>(HandleLoadFromSlotRequest));
            _subs.Add(_eventBus.Subscribe<RequestDeleteSaveSlotEvent>(HandleDeleteSlotRequest));
            _subs.Add(_eventBus.Subscribe<RequestGetSaveSlotsEvent>(HandleGetSaveSlotsRequest));
        }
        
        void OnDisable()
        {
            for (int i = 0; i < _subs.Count; i++)
            {
                _subs[i]?.Dispose();
            }
            _subs.Clear();
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
            _eventBus.Publish(new OnSaveSlotsReceivedEvent { SaveSlots = slots });
        }
        
        public void SaveGameToSlot(int slotIndex, string saveName)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid save slot index: {slotIndex}");
                _eventBus.Publish(new OnGameSavedEvent { Success = false });
                return;
            }
            
            try
            {
                SaveData saveData = GatherSaveData();
                
                if (saveData == null)
                {
                    Debug.LogError("Failed to gather save data. Cannot save game.");
                    _eventBus.Publish(new OnGameSavedEvent { Success = false });
                    return;
                }
                
                saveData.saveName = saveName;
                saveData.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                string json = JsonUtility.ToJson(saveData, prettyPrintJson);
                string filePath = GetSaveFilePath(slotIndex);
                File.WriteAllText(filePath, json);
                
                Debug.Log($"Game saved to slot {slotIndex}: {filePath}");
                _eventBus.Publish(new OnGameSavedEvent { Success = true, SlotIndex = slotIndex });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game to slot {slotIndex}: {e.Message}");
                _eventBus.Publish(new OnGameSavedEvent { Success = false });
            }
        }
        
        public void LoadGameFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid save slot index: {slotIndex}");
                _eventBus.Publish(new OnGameLoadedEvent { Success = false });
                return;
            }
            
            string filePath = GetSaveFilePath(slotIndex);
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"No save file found in slot {slotIndex}");
                _eventBus.Publish(new OnGameLoadedEvent { Success = false });
                return;
            }
            
            try
            {
                string json = File.ReadAllText(filePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                
                SaveDataMigration.MigrateSaveData(saveData);
                
                ApplySaveData(saveData);
                
                Debug.Log($"Game loaded from slot {slotIndex}: {filePath}");
                _eventBus.Publish(new OnGameLoadedEvent { Success = true, SlotIndex = slotIndex });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game from slot {slotIndex}: {e.Message}");
                _eventBus.Publish(new OnGameLoadedEvent { Success = false });
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
                    _eventBus.Publish(new OnSaveSlotDeletedEvent { SlotIndex = slotIndex });
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
                    
                    string corruptPath = filePath + ".corrupt";
                    try
                    {
                        if (!File.Exists(corruptPath))
                        {
                            File.Move(filePath, corruptPath);
                            Debug.LogWarning($"Moved corrupt save file to: {corruptPath}");
                        }
                    }
                    catch (Exception moveEx)
                    {
                        Debug.LogError($"Failed to rename corrupt file: {moveEx.Message}");
                    }
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
            if (_definitionResolver == null)
            {
                Debug.LogError("DefinitionResolver is not initialized. Cannot save game.");
                return null;
            }
            
            SaveData data = new SaveData
            {
                saveVersion = SaveConstants.CURRENT_VERSION,
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
            
            if (ServiceLocator.Instance.TryGet<TimeSystem>(out TimeSystem timeSystem))
            {
                data.timeSystem = new TimeSystemData
                {
                    year = timeSystem.CurrentDate.Year,
                    month = timeSystem.CurrentDate.Month,
                    day = timeSystem.CurrentDate.Day,
                    dayIndex = timeSystem.DayIndex,
                    currentSpeed = timeSystem.CurrentSpeed
                };
            }
            
            TechnologySystemData techData = null;
            _eventBus.Subscribe<OnTechnologyDataSavedEvent>(evt => techData = evt.Data).Dispose();
            _eventBus.Publish(new RequestSaveTechnologyDataEvent());
            if (techData != null)
            {
                data.technologySystem = techData;
            }
            
            if (EmployeeSystem.Instance != null)
            {
                data.employeeSystem = new EmployeeSystemData
                {
                    employees = new List<SerializableEmployee>(),
                    employeeCounter = EmployeeSystem.Instance.Employees.Count,
                    pendingSeverancePayments = new List<PendingSeverancePayment>()
                };
                
                foreach (var employee in EmployeeSystem.Instance.Employees)
                {
                    SerializableEmployee serialized = SerializableEmployee.FromEmployee(employee, _definitionResolver);
                    if (serialized != null)
                    {
                        data.employeeSystem.employees.Add(serialized);
                    }
                }
                
                foreach (var (employeeName, salary) in EmployeeSystem.Instance.PendingSeverancePayments)
                {
                    data.employeeSystem.pendingSeverancePayments.Add(new PendingSeverancePayment
                    {
                        employeeName = employeeName,
                        salary = salary
                    });
                }
            }
            
            if (ServiceLocator.Instance.TryGet<ProductSystem>(out ProductSystem productSystem))
            {
                data.productSystem = new ProductSystemData
                {
                    products = new List<SerializableProduct>()
                };
                
                foreach (var product in productSystem.Products)
                {
                    SerializableProduct serialized = SerializableProduct.FromProduct(product, _definitionResolver);
                    if (serialized != null)
                    {
                        data.productSystem.products.Add(serialized);
                    }
                }
            }
            
            if (ServiceLocator.Instance.TryGet<ContractSystem>(out ContractSystem contractSystem))
            {
                data.contractSystem = new ContractSystemData
                {
                    contracts = new List<SerializableContract>(),
                    daysSinceLastGeneration = 0
                };
                
                foreach (var contract in contractSystem.Contracts)
                {
                    SerializableContract serialized = SerializableContract.FromContract(contract, _definitionResolver);
                    if (serialized != null)
                    {
                        data.contractSystem.contracts.Add(serialized);
                    }
                }
            }
            
            if (ServiceLocator.Instance.TryGet<ReputationSystem>(out ReputationSystem reputationSystem))
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
            _eventBus.Publish(new OnBeforeLoadGameEvent());
            
            if (data.gameManager != null && GameManager.Instance != null)
            {
                _eventBus.Publish(new RequestSetCashEvent { Amount = data.gameManager.currentCash });
            }
            
            if (data.timeSystem != null)
            {
                _eventBus.Publish(new RequestSetDateEvent
                {
                    Year = data.timeSystem.year,
                    Month = data.timeSystem.month,
                    Day = data.timeSystem.day,
                    DayIndex = data.timeSystem.dayIndex
                });
                
                _eventBus.Publish(new RequestChangeSpeedEvent { Speed = data.timeSystem.currentSpeed });
            }
            
            if (data.technologySystem != null)
            {
                _eventBus.Publish(new RequestLoadTechnologyDataEvent
                {
                    Data = data.technologySystem
                });
            }
            
            if (data.employeeSystem != null)
            {
                _eventBus.Publish(new RequestLoadEmployeesEvent
                {
                    Employees = data.employeeSystem.employees,
                    PendingSeverancePayments = data.employeeSystem.pendingSeverancePayments ?? new List<PendingSeverancePayment>()
                });
            }
            
            if (data.productSystem != null)
            {
                _eventBus.Publish(new RequestLoadProductsEvent
                {
                    Products = data.productSystem.products
                });
            }
            
            if (data.contractSystem != null)
            {
                _eventBus.Publish(new RequestLoadContractsEvent
                {
                    Contracts = data.contractSystem.contracts
                });
            }
            
            if (data.reputationSystem != null)
            {
                _eventBus.Publish(new RequestSetReputationEvent
                {
                    Reputation = data.reputationSystem.currentReputation
                });
            }
            
            _eventBus.Publish(new OnAfterLoadGameEvent());
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
}
