using System.Collections.Generic;

namespace TechMogul.Core.Save
{
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
    
    public class OnBeforeLoadGameEvent { }
    
    public class OnAfterLoadGameEvent { }
}
