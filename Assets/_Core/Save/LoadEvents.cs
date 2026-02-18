using System.Collections.Generic;

namespace TechMogul.Core.Save
{
    public class RequestSetCashEvent
    {
        public float Amount;
    }
    
    public class RequestSetDateEvent
    {
        public int Year;
        public int Month;
        public int Day;
        public int DayIndex = -1;
    }
    
    public class RequestLoadEmployeesEvent
    {
        public List<SerializableEmployee> Employees;
        public List<PendingSeverancePayment> PendingSeverancePayments;
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
