using System;

namespace TechMogul.Systems
{
    public enum AssignmentType
    {
        Idle,
        Contract,
        Product
    }

    [Serializable]
    public class EmployeeAssignment
    {
        public AssignmentType assignmentType;
        public string assignmentId;
        public string displayName;

        public static EmployeeAssignment Idle()
        {
            return new EmployeeAssignment
            {
                assignmentType = AssignmentType.Idle,
                assignmentId = string.Empty,
                displayName = "Idle"
            };
        }

        public static EmployeeAssignment Contract(string contractId, string clientName)
        {
            return new EmployeeAssignment
            {
                assignmentType = AssignmentType.Contract,
                assignmentId = contractId,
                displayName = $"Contract: {clientName}"
            };
        }

        public static EmployeeAssignment Product(string productId, string productName)
        {
            return new EmployeeAssignment
            {
                assignmentType = AssignmentType.Product,
                assignmentId = productId,
                displayName = $"Product: {productName}"
            };
        }

        public bool IsIdle => assignmentType == AssignmentType.Idle;
        public bool IsOnContract => assignmentType == AssignmentType.Contract;
        public bool IsOnProduct => assignmentType == AssignmentType.Product;
        public bool IsAssigned => !IsIdle;
    }
}
