using UnityEngine;
using TechMogul.Core;
using TechMogul.Systems;
using TechMogul.Contracts;
using TechMogul.Products;

namespace TechMogul.Systems
{
    public class MonthlyReportSystem : GameSystem
    {
        private MonthlyReport currentMonthReport;
        private float monthStartCash;
        
        protected override void Awake()
        {
            base.Awake();
            currentMonthReport = new MonthlyReport();
        }
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnMonthTickEvent>(HandleMonthTick);
            Subscribe<OnDayTickEvent>(HandleDayTick);
            Subscribe<OnContractCompletedEvent>(HandleContractCompleted);
            Subscribe<OnProductReleasedEvent>(HandleProductReleased);
            Subscribe<OnCashChangedEvent>(HandleCashChanged);
            Subscribe<OnGameStartedEvent>(HandleGameStarted);
        }
        
        void HandleGameStarted(OnGameStartedEvent evt)
        {
            ResetReport();
            monthStartCash = GameManager.Instance.CurrentCash;
        }
        
        void HandleDayTick(OnDayTickEvent evt)
        {
            currentMonthReport.DaysInMonth++;
        }
        
        void HandleMonthTick(OnMonthTickEvent evt)
        {
            float endCash = GameManager.Instance.CurrentCash;
            float profit = endCash - monthStartCash;
            currentMonthReport.Profit = profit;
            
            CalculateAverageMorale();
            
            EventBus.Publish(new OnMonthlyReportEvent
            {
                Month = evt.Month,
                Year = evt.Year,
                Report = currentMonthReport.Clone()
            });
            
            ResetReport();
            monthStartCash = endCash;
        }
        
        void HandleContractCompleted(OnContractCompletedEvent evt)
        {
            if (evt.success)
            {
                currentMonthReport.ContractsCompleted++;
            }
            else
            {
                currentMonthReport.ContractsFailed++;
            }
        }
        
        void HandleProductReleased(OnProductReleasedEvent evt)
        {
            currentMonthReport.ProductsReleased++;
        }
        
        void HandleCashChanged(OnCashChangedEvent evt)
        {
            if (evt.Change > 0)
            {
                currentMonthReport.MoneyEarned += evt.Change;
            }
            else if (evt.Change < 0)
            {
                currentMonthReport.MoneySpent += Mathf.Abs(evt.Change);
            }
        }
        
        void CalculateAverageMorale()
        {
            var employeeSystem = FindFirstObjectByType<EmployeeSystem>();
            if (employeeSystem != null)
            {
                var employees = employeeSystem.GetAllEmployees();
                if (employees.Count > 0)
                {
                    float totalMorale = 0;
                    foreach (var emp in employees)
                    {
                        totalMorale += emp.morale;
                    }
                    currentMonthReport.AverageMorale = totalMorale / employees.Count;
                }
                else
                {
                    currentMonthReport.AverageMorale = 0;
                }
            }
        }
        
        void ResetReport()
        {
            currentMonthReport = new MonthlyReport
            {
                DaysInMonth = 0,
                ContractsCompleted = 0,
                ContractsFailed = 0,
                ProductsReleased = 0,
                MoneyEarned = 0,
                MoneySpent = 0,
                Profit = 0,
                AverageMorale = 0
            };
        }
    }
    
    public class MonthlyReport
    {
        public int DaysInMonth;
        public int ContractsCompleted;
        public int ContractsFailed;
        public int ProductsReleased;
        public float MoneyEarned;
        public float MoneySpent;
        public float Profit;
        public float AverageMorale;
        
        public MonthlyReport Clone()
        {
            return new MonthlyReport
            {
                DaysInMonth = this.DaysInMonth,
                ContractsCompleted = this.ContractsCompleted,
                ContractsFailed = this.ContractsFailed,
                ProductsReleased = this.ProductsReleased,
                MoneyEarned = this.MoneyEarned,
                MoneySpent = this.MoneySpent,
                Profit = this.Profit,
                AverageMorale = this.AverageMorale
            };
        }
    }
    
    public class OnMonthlyReportEvent
    {
        public int Month;
        public int Year;
        public MonthlyReport Report;
    }
}
