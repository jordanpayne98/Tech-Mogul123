using System;

namespace TechMogul.Core.Save
{
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
}
