using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Core.Save;
using TechMogul.Systems;

namespace TechMogul.Contracts
{
    public class ContractSystem : GameSystem
    {
        [Header("Settings")]
        [SerializeField] private int maxAvailableContracts = 5;
        [SerializeField] private int contractGenerationIntervalDays = 7;
        [SerializeField] private int contractsPerGeneration = 2;
        
        [Header("Milestone 7 Configuration")]
        [SerializeField] private ContractBalanceConfigSO balanceConfig;
        [SerializeField] private ContractNamingSystem namingSystem;
        [SerializeField] private ContractWorldEffectsManager worldEffectsManager;

        [Header("Contract Templates")]
        [SerializeField] private List<TechMogul.Data.ContractTemplateSO> contractTemplates = new List<TechMogul.Data.ContractTemplateSO>();

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private List<ContractData> _contracts = new List<ContractData>();
        private int _currentDay = 0;
        private int _daysSinceLastGeneration = 0;
        
        private IRng _rng;
        private ContractSimulation _simulation;
        private ContractResolver _resolver;
        private RivalContractOfferGenerator _rivalOfferGenerator;
        
        private ReputationSystem _reputationSystem;
        private EmployeeSystem _employeeSystem;
        private RivalSystem _rivalSystem;
        private EraSystem _eraSystem;
        private TechnologySystem _technologySystem;
        private MarketSystem _marketSystem;

        public IReadOnlyList<ContractData> Contracts => _contracts;
        public ContractWorldEffectsManager WorldEffectsManager => worldEffectsManager;

        protected override void Awake()
        {
            base.Awake();
            _rng = new UnityRng();
            
            _reputationSystem = ServiceLocator.Instance.Get<ReputationSystem>();
            _employeeSystem = ServiceLocator.Instance.Get<EmployeeSystem>();
            _rivalSystem = ServiceLocator.Instance.Get<RivalSystem>();
            _eraSystem = ServiceLocator.Instance.Get<EraSystem>();
            _technologySystem = ServiceLocator.Instance.Get<TechnologySystem>();
            _marketSystem = ServiceLocator.Instance.Get<MarketSystem>();
            
            InitializeSubsystems();
            
            ServiceLocator.Instance.TryRegister<ContractSystem>(this);
        }

        void Start()
        {
            if (balanceConfig != null)
            {
                maxAvailableContracts = balanceConfig.maxAvailableOffers;
                contractGenerationIntervalDays = balanceConfig.generationIntervalDays;
                contractsPerGeneration = balanceConfig.offersPerGeneration;
            }
            
            GenerateInitialContracts();
        }
        
        void InitializeSubsystems()
        {
            _simulation = new ContractSimulation(_rng, _employeeSystem);
            _resolver = new ContractResolver(_rng, _employeeSystem, showDebugLogs);
            
            if (balanceConfig != null && namingSystem != null)
            {
                _rivalOfferGenerator = new RivalContractOfferGenerator(
                    contractTemplates,
                    balanceConfig,
                    namingSystem,
                    _rng,
                    showDebugLogs
                );
            }
            
            if (worldEffectsManager != null && balanceConfig != null)
            {
                worldEffectsManager.Initialize(balanceConfig);
            }
        }
        
        void GenerateInitialContracts()
        {
            if (_rivalOfferGenerator != null && _rivalSystem != null)
            {
                var newContracts = _rivalOfferGenerator.GenerateRivalContracts(
                    maxAvailableContracts,
                    _currentDay,
                    _rivalSystem,
                    _eraSystem,
                    _technologySystem,
                    _marketSystem);
                    
                _contracts.AddRange(newContracts);
                EventBus.Publish(new OnContractsChangedEvent());
            }
        }

        protected override void SubscribeToEvents()
        {
            Subscribe<OnDayTickEvent>(HandleDayTick);
            Subscribe<OnQuarterTickEvent>(HandleQuarterTick);
            Subscribe<RequestAcceptContractEvent>(HandleAcceptContractRequest);
            Subscribe<RequestAssignEmployeeToContractEvent>(HandleAssignEmployeeRequest);
            Subscribe<RequestUnassignEmployeeFromContractEvent>(HandleUnassignEmployeeRequest);
            Subscribe<RequestClearCompletedContractsEvent>(HandleClearCompletedContracts);
            Subscribe<RequestLoadContractsEvent>(HandleLoadContracts);
            Subscribe<OnGameStartedEvent>(HandleGameStarted);
            Subscribe<OnContractReadyToCompleteEvent>(HandleContractReadyToComplete);
            Subscribe<OnContractReadyToFailEvent>(HandleContractReadyToFail);
        }
        
        void HandleQuarterTick(OnQuarterTickEvent evt)
        {
            if (worldEffectsManager != null)
            {
                worldEffectsManager.ProcessQuarterTick();
            }
            
            if (_rivalOfferGenerator != null)
            {
                _rivalOfferGenerator.ProcessQuarterTickCooldowns();
            }
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            _contracts.Clear();
            _currentDay = 0;
            _daysSinceLastGeneration = 0;
            
            if (worldEffectsManager != null)
            {
                worldEffectsManager.ClearAllEffects();
            }
            
            GenerateInitialContracts();
            EventBus.Publish(new OnContractsChangedEvent());
            
            if (showDebugLogs)
            {
                Debug.Log("[ContractSystem] Reset for new game");
            }
        }
        
        void HandleLoadContracts(RequestLoadContractsEvent evt)
        {
            _contracts.Clear();
            
            IDefinitionResolver resolver = ServiceLocator.Instance.Get<IDefinitionResolver>();
            if (resolver == null)
            {
                Debug.LogError("IDefinitionResolver not found. Cannot load contracts.");
                return;
            }
            
            foreach (var serializedContract in evt.Contracts)
            {
                ContractData contract = serializedContract.ToContract(_currentDay, resolver);
                if (contract != null)
                {
                    _contracts.Add(contract);
                }
            }
            
            EventBus.Publish(new OnContractsChangedEvent());
            Debug.Log($"Loaded {_contracts.Count} contracts");
        }

        void HandleDayTick(OnDayTickEvent evt)
        {
            _currentDay = evt.DayIndex;
            _daysSinceLastGeneration++;

            _simulation.TickAvailableContracts(_contracts);
            _simulation.TickActiveContracts(_contracts, _currentDay, EventBus);
            _simulation.ApplyDailyBurnout(_contracts, EventBus);

            int expiryDays = balanceConfig != null ? balanceConfig.offerExpiryDays : 14;
            foreach (var contract in _contracts.Where(c => c.state == ContractState.Available && c.daysAvailable >= expiryDays).ToList())
            {
                ExpireContract(contract);
            }

            if (_daysSinceLastGeneration >= contractGenerationIntervalDays)
            {
                GenerateNewRivalContracts();
                _daysSinceLastGeneration = 0;
                EventBus.Publish(new OnContractsChangedEvent());
            }
        }
        
        void GenerateNewRivalContracts()
        {
            if (_rivalOfferGenerator == null) return;
            
            int availableSlots = maxAvailableContracts - _contracts.Count(c => c.state == ContractState.Available);
            if (availableSlots <= 0) return;
            
            int toGenerate = Mathf.Min(contractsPerGeneration, availableSlots);
            
            var newContracts = _rivalOfferGenerator.GenerateRivalContracts(
                toGenerate,
                _currentDay,
                _rivalSystem,
                _eraSystem,
                _technologySystem,
                _marketSystem);
                
            _contracts.AddRange(newContracts);
            
            if (showDebugLogs && newContracts.Count > 0)
            {
                Debug.Log($"[ContractSystem] Generated {newContracts.Count} new rival contracts");
            }
        }
        
        void HandleContractReadyToComplete(OnContractReadyToCompleteEvent evt)
        {
            var contract = _contracts.FirstOrDefault(c => c.contractId == evt.ContractId);
            if (contract == null) return;
            
            _resolver.CompleteContract(contract, true, _currentDay, EventBus);
            
            ApplyContractWorldEffect(contract);
            
            PayIssuerForContract(contract);
            
            EventBus.Publish(new OnContractsChangedEvent());
        }
        
        void ApplyContractWorldEffect(ContractData contract)
        {
            if (contract.worldEffect != null && worldEffectsManager != null)
            {
                worldEffectsManager.AddEffect(contract.worldEffect);
                
                if (showDebugLogs)
                {
                    Debug.Log($"[ContractSystem] Applied world effect: {contract.worldEffect.component} +{contract.worldEffect.magnitude:F2} for {contract.issuingRivalId}");
                }
            }
        }
        
        void PayIssuerForContract(ContractData contract)
        {
            if (_rivalSystem == null || string.IsNullOrEmpty(contract.issuingRivalId))
            {
                return;
            }
            
            var issuer = _rivalSystem.AllCompanies.FirstOrDefault(c => c.CompanyId == contract.issuingRivalId);
            if (issuer != null)
            {
                float cost = contract.totalPayout;
                issuer.Cash -= cost;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[ContractSystem] {issuer.Name} paid ${cost:N0} for contract completion");
                }
            }
        }
        
        void HandleContractReadyToFail(OnContractReadyToFailEvent evt)
        {
            var contract = _contracts.FirstOrDefault(c => c.contractId == evt.ContractId);
            if (contract == null) return;
            
            FailContract(contract);
            EventBus.Publish(new OnContractsChangedEvent());
        }

        void HandleAcceptContractRequest(RequestAcceptContractEvent evt)
        {
            var contract = _contracts.FirstOrDefault(c => c.contractId == evt.contractId);
            if (contract == null)
            {
                Debug.LogWarning($"Contract {evt.contractId} not found");
                return;
            }

            if (contract.state != ContractState.Available)
            {
                Debug.LogWarning($"Contract {evt.contractId} is not available");
                return;
            }

            contract.state = ContractState.Active;
            contract.startDay = _currentDay;
            contract.daysAvailable = 0;

            if (evt.assignedEmployeeIds != null)
            {
                foreach (var employeeId in evt.assignedEmployeeIds)
                {
                    contract.assignedEmployeeIds.Add(employeeId);
                    EventBus.Publish(new RequestAssignEmployeeEvent
                    {
                        EmployeeId = employeeId,
                        Assignment = EmployeeAssignment.Contract(contract.contractId, contract.clientName)
                    });
                }
            }

            EventBus.Publish(new OnContractAcceptedEvent
            {
                contractId = contract.contractId,
                clientName = contract.clientName,
                deadline = contract.daysRemaining
            });
            
            EventBus.Publish(new OnContractsChangedEvent());

            if (showDebugLogs)
            {
                Debug.Log($"Accepted contract from '{contract.clientName}' with {contract.assignedEmployeeIds.Count} employees");
            }
        }

        void HandleAssignEmployeeRequest(RequestAssignEmployeeToContractEvent evt)
        {
            var contract = _contracts.FirstOrDefault(c => c.contractId == evt.contractId);
            if (contract == null)
            {
                Debug.LogWarning($"Contract {evt.contractId} not found");
                return;
            }

            if (contract.assignedEmployeeIds.Contains(evt.employeeId))
            {
                Debug.LogWarning($"Employee {evt.employeeId} already assigned to contract");
                return;
            }

            contract.assignedEmployeeIds.Add(evt.employeeId);

            EventBus.Publish(new RequestAssignEmployeeEvent
            {
                EmployeeId = evt.employeeId,
                Assignment = EmployeeAssignment.Contract(contract.contractId, contract.clientName)
            });

            if (showDebugLogs)
            {
                Debug.Log($"Assigned employee to contract from '{contract.clientName}'");
            }
        }

        void HandleUnassignEmployeeRequest(RequestUnassignEmployeeFromContractEvent evt)
        {
            var contract = _contracts.FirstOrDefault(c => c.contractId == evt.contractId);
            if (contract == null)
            {
                Debug.LogWarning($"Contract {evt.contractId} not found");
                return;
            }

            if (!contract.assignedEmployeeIds.Remove(evt.employeeId))
            {
                Debug.LogWarning($"Employee {evt.employeeId} not assigned to contract");
                return;
            }

            EventBus.Publish(new RequestUnassignEmployeeEvent
            {
                EmployeeId = evt.employeeId
            });

            if (showDebugLogs)
            {
                Debug.Log($"Unassigned employee from contract '{contract.clientName}'");
            }
        }

        void FailContract(ContractData contract)
        {
            _resolver.CompleteContract(contract, false, _currentDay, EventBus);

            EventBus.Publish(new OnContractFailedEvent
            {
                contractId = contract.contractId,
                reason = "Deadline missed"
            });
        }

        void ExpireContract(ContractData contract)
        {
            if (showDebugLogs)
            {
                Debug.Log($"Contract '{contract.clientName}' expired after 7 days");
            }

            _contracts.Remove(contract);

            EventBus.Publish(new OnContractExpiredEvent
            {
                contractId = contract.contractId,
                clientName = contract.clientName
            });
            
            EventBus.Publish(new OnContractsChangedEvent());
        }

        void HandleClearCompletedContracts(RequestClearCompletedContractsEvent evt)
        {
            int removedCount = _contracts.RemoveAll(c => c.state == ContractState.Completed || c.state == ContractState.Failed);
            
            if (showDebugLogs && removedCount > 0)
            {
                Debug.Log($"Cleared {removedCount} completed/failed contract(s)");
            }

            if (removedCount > 0)
            {
                EventBus.Publish(new OnContractsChangedEvent());
            }
        }

        public ContractData GetContract(string contractId)
        {
            return _contracts.FirstOrDefault(c => c.contractId == contractId);
        }
        
        public ContractData GetContractById(string contractId)
        {
            return GetContract(contractId);
        }

        public List<ContractData> GetAvailableContracts()
        {
            return _contracts.Where(c => c.state == ContractState.Available).ToList();
        }

        public List<ContractData> GetActiveContracts()
        {
            return _contracts.Where(c => c.state == ContractState.Active).ToList();
        }

        public List<ContractData> GetCompletedContracts()
        {
            return _contracts.Where(c => c.state == ContractState.Completed).ToList();
        }
    }
}
