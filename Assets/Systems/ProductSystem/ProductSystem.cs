using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Core.Save;
using TechMogul.Systems;

namespace TechMogul.Products
{
    public class ProductSystem : GameSystem
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private List<ProductData> _products = new List<ProductData>();
        private int _currentDay = 0;

        public IReadOnlyList<ProductData> Products => _products;

        protected override void Awake()
        {
            base.Awake();
            ServiceLocator.Instance.TryRegister<ProductSystem>(this);
        }

        protected override void SubscribeToEvents()
        {
            Subscribe<OnDayTickEvent>(HandleDayTick);
            Subscribe<OnMonthTickEvent>(HandleMonthTick);
            Subscribe<RequestStartProductEvent>(HandleStartProductRequest);
            Subscribe<RequestAssignEmployeeToProductEvent>(HandleAssignEmployeeRequest);
            Subscribe<RequestUnassignEmployeeFromProductEvent>(HandleUnassignEmployeeRequest);
            Subscribe<RequestAddFeatureToProductEvent>(HandleAddFeatureRequest);
            Subscribe<RequestSetProductQATierEvent>(HandleSetQATierRequest);
            Subscribe<RequestAdvanceProductPhaseEvent>(HandleAdvancePhaseRequest);
            Subscribe<RequestLoadProductsEvent>(HandleLoadProducts);
            Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            _products.Clear();
            _currentDay = 0;
            Debug.Log("ProductSystem reset for new game");
        }
        
        void HandleLoadProducts(RequestLoadProductsEvent evt)
        {
            _products.Clear();
            
            IDefinitionResolver resolver = ServiceLocator.Instance.Get<IDefinitionResolver>();
            if (resolver == null)
            {
                Debug.LogError("IDefinitionResolver not found. Cannot load products.");
                return;
            }
            
            foreach (var serializedProduct in evt.Products)
            {
                ProductData product = serializedProduct.ToProduct(_currentDay, resolver);
                if (product != null)
                {
                    _products.Add(product);
                }
            }
            
            Debug.Log($"Loaded {_products.Count} products");
        }

        void HandleDayTick(OnDayTickEvent evt)
        {
            _currentDay = evt.DayIndex;

            foreach (var product in _products.Where(p => p.state == ProductState.InDevelopment).ToList())
            {
                ProcessDailyWork(product);

                if (product.developmentProgress >= 100f)
                {
                    CompleteProduct(product);
                }
                else
                {
                    EventBus.Publish(new OnProductProgressUpdatedEvent
                    {
                        productId = product.productId,
                        progress = product.developmentProgress,
                        daysRemaining = CalculateDaysRemaining(product)
                    });
                }
            }

            ApplyDailyBurnout();
        }
        
        void ProcessDailyWork(ProductData product)
        {
            if (product.assignedEmployeeIds.Count == 0)
            {
                return;
            }
            
            float totalProgress = 0f;
            float totalBugs = 0f;
            float totalQuality = 0f;
            
            foreach (var employeeId in product.assignedEmployeeIds)
            {
                var employee = GetEmployeeData(employeeId);
                if (employee == null) continue;
                
                var workResult = WorkSimulation.ProcessDailyWork(employee, product);
                
                totalProgress += workResult.progressAdded;
                totalBugs += workResult.bugsAdded;
                totalQuality += workResult.qualityAdded;
                
                WorkSimulation.UpdateEmployeeStress(employee, true, 1);
                WorkSimulation.UpdateEmployeeMorale(employee);
                WorkSimulation.UpdateEmployeeBurnout(employee);
            }
            
            product.developmentProgress += totalProgress;
            product.estimatedBugs += totalBugs;
            
            RecalculateProductStats(product);
        }

        void HandleMonthTick(OnMonthTickEvent evt)
        {
            float totalRevenue = 0f;
            int activeProductCount = 0;

            foreach (var product in _products.Where(p => p.state == ProductState.Released).ToList())
            {
                float revenue = product.monthlyRevenue;

                float decayMultiplier = CalculateDecayMultiplier(product.monthsActive);
                revenue *= decayMultiplier;

                product.totalRevenue += revenue;
                product.monthsActive++;
                totalRevenue += revenue;
                activeProductCount++;

                EventBus.Publish(new OnProductRevenueEvent
                {
                    productId = product.productId,
                    revenue = revenue
                });
            }

            if (totalRevenue > 0f)
            {
                EventBus.Publish(new RequestAddCashEvent { Amount = totalRevenue });
                EventBus.Publish(new OnTotalProductRevenueEvent
                {
                    totalRevenue = totalRevenue,
                    productCount = activeProductCount
                });

                if (showDebugLogs)
                {
                    Debug.Log($"Product revenue this month: ${totalRevenue:F2} from {activeProductCount} products");
                }
            }
        }

        void HandleStartProductRequest(RequestStartProductEvent evt)
        {
            if (string.IsNullOrEmpty(evt.productName))
            {
                Debug.LogWarning("Cannot start product with empty name");
                return;
            }

            if (evt.category == null)
            {
                Debug.LogWarning("Cannot start product without category");
                return;
            }

            string productId = System.Guid.NewGuid().ToString();
            var product = new ProductData(productId, evt.productName, evt.category, _currentDay);

            if (evt.assignedEmployeeIds != null)
            {
                foreach (var employeeId in evt.assignedEmployeeIds)
                {
                    product.assignedEmployeeIds.Add(employeeId);
                    EventBus.Publish(new RequestAssignEmployeeEvent
                    {
                        EmployeeId = employeeId,
                        Assignment = EmployeeAssignment.Product(product.productId, evt.productName)
                    });
                }
            }

            _products.Add(product);

            EventBus.Publish(new OnProductStartedEvent
            {
                productId = product.productId,
                name = product.name,
                category = product.category,
                assignedEmployeeIds = new List<string>(product.assignedEmployeeIds)
            });

            if (showDebugLogs)
            {
                Debug.Log($"Started product '{evt.productName}' with {product.assignedEmployeeIds.Count} employees");
            }
        }

        void HandleAssignEmployeeRequest(RequestAssignEmployeeToProductEvent evt)
        {
            var product = _products.FirstOrDefault(p => p.productId == evt.productId);
            if (product == null)
            {
                Debug.LogWarning($"Product {evt.productId} not found");
                return;
            }

            if (product.assignedEmployeeIds.Contains(evt.employeeId))
            {
                Debug.LogWarning($"Employee {evt.employeeId} already assigned to product");
                return;
            }

            product.assignedEmployeeIds.Add(evt.employeeId);

            EventBus.Publish(new RequestAssignEmployeeEvent
            {
                EmployeeId = evt.employeeId,
                Assignment = EmployeeAssignment.Product(product.productId, product.name)
            });

            if (showDebugLogs)
            {
                Debug.Log($"Assigned employee to product '{product.name}'");
            }
        }

        void HandleUnassignEmployeeRequest(RequestUnassignEmployeeFromProductEvent evt)
        {
            var product = _products.FirstOrDefault(p => p.productId == evt.productId);
            if (product == null)
            {
                Debug.LogWarning($"Product {evt.productId} not found");
                return;
            }

            if (!product.assignedEmployeeIds.Remove(evt.employeeId))
            {
                Debug.LogWarning($"Employee {evt.employeeId} not assigned to product");
                return;
            }

            EventBus.Publish(new RequestUnassignEmployeeEvent
            {
                EmployeeId = evt.employeeId
            });

            if (showDebugLogs)
            {
                Debug.Log($"Unassigned employee from product '{product.name}'");
            }
        }

        void HandleAddFeatureRequest(RequestAddFeatureToProductEvent evt)
        {
            var product = _products.FirstOrDefault(p => p.productId == evt.productId);
            if (product == null)
            {
                Debug.LogWarning($"Product {evt.productId} not found");
                return;
            }

            if (product.state != ProductState.InDevelopment)
            {
                Debug.LogWarning($"Cannot add features to released product");
                return;
            }

            var selectedFeatures = ResolveFeatureList(product);
            
            if (!FeatureGraphValidator.ValidateFeatureSelection(selectedFeatures, evt.feature, out string errorMessage))
            {
                Debug.LogWarning($"Cannot add feature '{evt.feature.featureName}': {errorMessage}");
                EventBus.Publish(new OnFeatureAddFailedEvent
                {
                    productId = product.productId,
                    featureId = evt.feature.id,
                    errorMessage = errorMessage
                });
                return;
            }

            selectedFeatures = FeatureGraphValidator.RemoveReplacedFeatures(selectedFeatures, evt.feature);
            
            if (!product.selectedFeatureIds.Contains(evt.feature.id))
            {
                product.selectedFeatureIds.Add(evt.feature.id);
            }

            RecalculateProductStats(product);

            EventBus.Publish(new OnFeatureAddedToProductEvent
            {
                productId = product.productId,
                featureId = evt.feature.id,
                featureName = evt.feature.featureName
            });

            if (showDebugLogs)
            {
                Debug.Log($"Added feature '{evt.feature.featureName}' to product '{product.name}'");
            }
        }

        void HandleSetQATierRequest(RequestSetProductQATierEvent evt)
        {
            var product = _products.FirstOrDefault(p => p.productId == evt.productId);
            if (product == null)
            {
                Debug.LogWarning($"Product {evt.productId} not found");
                return;
            }

            if (product.state != ProductState.InDevelopment)
            {
                Debug.LogWarning($"Cannot change QA tier of released product");
                return;
            }

            product.selectedQATierId = evt.qaTier?.id;
            RecalculateProductStats(product);

            EventBus.Publish(new OnProductQATierChangedEvent
            {
                productId = product.productId,
                qaTierId = evt.qaTier?.id
            });

            if (showDebugLogs)
            {
                string tierName = evt.qaTier != null ? evt.qaTier.tierName : "None";
                Debug.Log($"Set QA tier '{tierName}' for product '{product.name}'");
            }
        }

        void HandleAdvancePhaseRequest(RequestAdvanceProductPhaseEvent evt)
        {
            var product = _products.FirstOrDefault(p => p.productId == evt.productId);
            if (product == null)
            {
                Debug.LogWarning($"Product {evt.productId} not found");
                return;
            }

            if (product.state != ProductState.InDevelopment)
            {
                Debug.LogWarning($"Cannot advance phase of released product");
                return;
            }

            ProjectPhase nextPhase = product.currentPhase switch
            {
                ProjectPhase.Implementation => ProjectPhase.BugFix,
                ProjectPhase.BugFix => ProjectPhase.Polish,
                ProjectPhase.Polish => ProjectPhase.Polish,
                _ => ProjectPhase.Implementation
            };

            if (nextPhase == product.currentPhase)
            {
                Debug.LogWarning($"Product '{product.name}' is already in final phase");
                return;
            }

            product.currentPhase = nextPhase;
            product.developmentProgress = 0f;

            EventBus.Publish(new OnProductPhaseAdvancedEvent
            {
                productId = product.productId,
                newPhase = nextPhase
            });

            if (showDebugLogs)
            {
                Debug.Log($"Advanced product '{product.name}' to {nextPhase} phase");
            }
        }

        float CalculateDailyProgress(ProductData product)
        {
            if (product.assignedEmployeeIds.Count == 0)
            {
                return 0f;
            }

            float baseRate = 100f / product.category.baseDevelopmentDays;
            float teamProductivity = CalculateTeamProductivity(product);
            float productivityMultiplier = teamProductivity / 50f;

            float phaseMultiplier = product.currentPhase switch
            {
                ProjectPhase.Implementation => 1.0f,
                ProjectPhase.BugFix => 0.8f,
                ProjectPhase.Polish => 0.6f,
                _ => 1.0f
            };

            var features = ResolveFeatureList(product);
            var qaTier = ResolveQATier(product);
            int totalDevTime = FeatureGraphValidator.CalculateTotalDevTime(features, qaTier);
            float timeAdjustment = totalDevTime > 0 ? (product.category.baseDevelopmentDays / (float)totalDevTime) : 1.0f;

            return baseRate * productivityMultiplier * phaseMultiplier * timeAdjustment;
        }

        float CalculateTeamProductivity(ProductData product)
        {
            if (product.assignedEmployeeIds.Count == 0) return 0f;
            
            var employees = new List<Employee>();
            foreach (var employeeId in product.assignedEmployeeIds)
            {
                var emp = GetEmployeeData(employeeId);
                if (emp != null) employees.Add(emp);
            }
            
            if (employees.Count == 0) return 0f;
            
            // Sort by productivity - best employee first
            employees = employees.OrderByDescending(e => 
                CalculateEmployeeProductivity(e, product.category)).ToList();
            
            float totalProductivity = 0f;
            
            for (int i = 0; i < employees.Count; i++)
            {
                var employee = employees[i];
                float baseProductivity = CalculateEmployeeProductivity(employee, product.category);
                
                // Apply morale and burnout multipliers
                float moraleMultiplier = employee.morale / 100f; // 0% morale = 0x, 100% = 1x
                float burnoutPenalty = employee.burnout / 100f; // 0% burnout = no penalty, 100% = -100%
                float effectiveMultiplier = moraleMultiplier * (1f - burnoutPenalty);
                effectiveMultiplier = Mathf.Max(effectiveMultiplier, 0.1f); // Minimum 10% productivity
                
                float adjustedProductivity = baseProductivity * effectiveMultiplier;
                
                if (i == 0)
                {
                    // First employee (best) = 1.0x base speed
                    totalProductivity = adjustedProductivity;
                }
                else
                {
                    // Additional employees add 0.1x to 0.6x based on skill match
                    // Higher skill match to product = higher bonus (0.6x)
                    // Lower skill match = lower bonus (0.1x)
                    float skillMatchRatio = adjustedProductivity / 100f; // 0 to 1.0 (assuming max skill ~100)
                    skillMatchRatio = Mathf.Clamp(skillMatchRatio, 0f, 1f);
                    
                    // Map 0-1 range to 0.1-0.6 range
                    float additionalBonus = 0.1f + (skillMatchRatio * 0.5f);
                    
                    totalProductivity += adjustedProductivity * additionalBonus;
                }
            }
            
            return totalProductivity;
        }

        float CalculateEmployeeProductivity(Employee employee, TechMogul.Data.ProductCategorySO category)
        {
            float weightedSkill =
                (employee.GetEffectiveSkill(SkillType.Development) * category.devSkillWeight) +
                (employee.GetEffectiveSkill(SkillType.Design) * category.designSkillWeight) +
                (employee.GetEffectiveSkill(SkillType.Marketing) * category.marketingSkillWeight);

            return weightedSkill;
        }

        float CalculateDaysRemaining(ProductData product)
        {
            if (product.developmentProgress >= 100f) return 0f;

            float dailyProgress = CalculateDailyProgress(product);
            if (dailyProgress <= 0f) return 999f;

            float remainingProgress = 100f - product.developmentProgress;
            return remainingProgress / dailyProgress;
        }

        void CompleteProduct(ProductData product)
        {
            product.state = ProductState.Released;
            product.releaseDay = _currentDay;
            product.developmentProgress = 100f;
            
            RecalculateProductStats(product);
            product.actualQuality = product.qualityScore;
            product.monthlyRevenue = CalculateMonthlyRevenue(product);

            ApplyProductCompletionEffects(product);

            foreach (var employeeId in product.assignedEmployeeIds.ToList())
            {
                EventBus.Publish(new RequestUnassignEmployeeEvent
                {
                    EmployeeId = employeeId
                });
            }

            EventBus.Publish(new OnProductReleasedEvent
            {
                productId = product.productId,
                name = product.name,
                quality = product.actualQuality,
                estimatedRevenue = product.monthlyRevenue
            });

            if (showDebugLogs)
            {
                Debug.Log($"Released product '{product.name}' | Quality: {product.actualQuality:F1} | Stability: {product.stabilityScore:F1} | Innovation: {product.innovationScore:F1} | Revenue: ${product.monthlyRevenue:F0}/month");
            }
        }

        void RecalculateProductStats(ProductData product)
        {
            var features = ResolveFeatureList(product);
            var qaTier = ResolveQATier(product);

            product.stabilityScore = FeatureGraphValidator.CalculateStabilityScore(features, qaTier);
            product.usabilityScore = FeatureGraphValidator.CalculateUsabilityScore(features, qaTier);
            product.innovationScore = FeatureGraphValidator.CalculateInnovationScore(features, qaTier);

            float employeeQuality = CalculateFinalQuality(product);
            float featureValue = FeatureGraphValidator.CalculateFeatureValue(features);
            float marketValue = FeatureGraphValidator.CalculateMarketValue(features);

            product.qualityScore = (employeeQuality * 0.5f) + (featureValue * 0.3f) + (marketValue * 0.2f);
            product.qualityScore = Mathf.Clamp(product.qualityScore, 0f, 100f);

            float bugRate = (100f - product.stabilityScore) / 100f;
            product.bugCount = Mathf.RoundToInt(features.Count * bugRate * 5f);

            product.marketingSpend = 0f;
            product.reputationContribution = product.qualityScore / 100f;
            product.priceCompetitiveness = 0.5f;
            product.standardAlignmentBonus = 0f;
            product.ecosystemBonus = 0f;
        }

        List<FeatureNodeSO> ResolveFeatureList(ProductData product)
        {
            var features = new List<FeatureNodeSO>();
            var resolver = ServiceLocator.Instance.Get<IDefinitionResolver>();
            if (resolver == null) return features;

            foreach (var featureId in product.selectedFeatureIds)
            {
                var feature = resolver.Resolve<FeatureNodeSO>(featureId);
                if (feature != null)
                {
                    features.Add(feature);
                }
            }

            return features;
        }

        QATierSO ResolveQATier(ProductData product)
        {
            if (string.IsNullOrEmpty(product.selectedQATierId)) return null;
            
            var resolver = ServiceLocator.Instance.Get<IDefinitionResolver>();
            if (resolver == null) return null;

            return resolver.Resolve<QATierSO>(product.selectedQATierId);
        }

        float CalculateFinalQuality(ProductData product)
        {
            if (product.assignedEmployeeIds.Count == 0) return 0f;

            float qualitySum = 0f;

            foreach (var employeeId in product.assignedEmployeeIds)
            {
                var employee = GetEmployeeData(employeeId);
                if (employee != null)
                {
                    float employeeQuality =
                        (employee.devSkill * product.category.devSkillWeight) +
                        (employee.designSkill * product.category.designSkillWeight) +
                        (employee.marketingSkill * product.category.marketingSkillWeight);
                    qualitySum += employeeQuality;
                }
            }

            float averageQuality = qualitySum / product.assignedEmployeeIds.Count;
            float weightedQuality = averageQuality * product.category.qualityImportance;

            return Mathf.Clamp(weightedQuality, 0f, 100f);
        }

        float CalculateMonthlyRevenue(ProductData product)
        {
            float baseRevenue = Random.Range(product.category.baseRevenueMin, product.category.baseRevenueMax);

            float qualityMultiplier = 0.5f + (product.actualQuality / 100f * 0.5f);

            float marketDemandMultiplier = 1.0f;
            MarketSystem marketSystem = ServiceLocator.Instance.Get<MarketSystem>();
            if (marketSystem != null && !string.IsNullOrEmpty(product.category.id))
            {
                marketDemandMultiplier = marketSystem.GetCategoryDemandMultiplier(product.category.id);
            }

            ReputationSystem reputationSystem = ServiceLocator.Instance.Get<ReputationSystem>();
            float reputationMultiplier = 1.0f;
            if (reputationSystem != null)
            {
                float reputation = reputationSystem.CurrentReputation;
                float maxReputation = reputationSystem.MaxReputation;
                reputationMultiplier = 0.8f + (reputation / maxReputation * 0.4f);
            }

            float revenue = baseRevenue * qualityMultiplier * marketDemandMultiplier * reputationMultiplier;

            return Mathf.Round(revenue / 100f) * 100f;
        }

        float CalculateDecayMultiplier(int monthsActive)
        {
            if (monthsActive <= 6) return 1.0f;

            float decayRate = 0.02f;
            float decay = (monthsActive - 6) * decayRate;

            return Mathf.Max(1.0f - decay, 0.3f);
        }

        void ApplyProductCompletionEffects(ProductData product)
        {
            float baseXP = product.category.baseDevelopmentDays / 10f;
            float qualityBonus = product.actualQuality / 100f * baseXP * 0.5f;
            float totalXP = baseXP + qualityBonus;

            float burnoutAmount = product.category.baseDevelopmentDays / 30f * 10f;

            foreach (var employeeId in product.assignedEmployeeIds)
            {
                EventBus.Publish(new RequestAddSkillXPEvent
                {
                    EmployeeId = employeeId,
                    DevXP = totalXP * product.category.devSkillWeight,
                    DesignXP = totalXP * product.category.designSkillWeight,
                    MarketingXP = totalXP * product.category.marketingSkillWeight
                });

                EventBus.Publish(new RequestAddBurnoutEvent
                {
                    EmployeeId = employeeId,
                    Amount = burnoutAmount
                });

                var employee = GetEmployeeData(employeeId);
                if (employee != null)
                {
                    employee.totalProjectsCompleted++;
                }
            }
        }

        void ApplyDailyBurnout()
        {
            foreach (var product in _products.Where(p => p.state == ProductState.InDevelopment))
            {
                foreach (var employeeId in product.assignedEmployeeIds)
                {
                    EventBus.Publish(new RequestAddBurnoutEvent
                    {
                        EmployeeId = employeeId,
                        Amount = 1f
                    });
                }
            }
        }

        Employee GetEmployeeData(string employeeId)
        {
            var employeeSystem = FindFirstObjectByType<EmployeeSystem>();
            if (employeeSystem == null) return null;

            return employeeSystem.Employees.FirstOrDefault(e => e.employeeId == employeeId);
        }

        public ProductData GetProduct(string productId)
        {
            return _products.FirstOrDefault(p => p.productId == productId);
        }

        public List<ProductData> GetProductsInDevelopment()
        {
            return _products.Where(p => p.state == ProductState.InDevelopment).ToList();
        }

        public List<ProductData> GetReleasedProducts()
        {
            return _products.Where(p => p.state == ProductState.Released).ToList();
        }
    }
}
