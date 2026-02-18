using System.Collections.Generic;

namespace TechMogul.Contracts
{
    public class RequestAcceptContractEvent
    {
        public string contractId;
        public List<string> assignedEmployeeIds;
    }

    public class OnContractAcceptedEvent
    {
        public string contractId;
        public string clientName;
        public int deadline;
    }

    public class RequestAssignEmployeeToContractEvent
    {
        public string contractId;
        public string employeeId;
    }

    public class RequestUnassignEmployeeFromContractEvent
    {
        public string contractId;
        public string employeeId;
    }

    public class OnContractProgressUpdatedEvent
    {
        public string contractId;
        public float progress;
        public int daysRemaining;
    }

    public class OnContractCompletedEvent
    {
        public string contractId;
        public string clientName;
        public float quality;
        public float payout;
        public bool success;
    }

    public class OnContractFailedEvent
    {
        public string contractId;
        public string reason;
    }

    public class OnContractExpiredEvent
    {
        public string contractId;
        public string clientName;
    }

    public class RequestClearCompletedContractsEvent
    {
    }

    public class OnContractsChangedEvent
    {
    }
}
