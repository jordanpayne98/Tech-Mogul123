using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SaveLoadDialogController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxSaveSlots = 3;
        
        private UIDocument uiDocument;
        private VisualElement root;
        
        private VisualElement saveDialog;
        private VisualElement loadDialog;
        
        private ScrollView saveSlotsList;
        private ScrollView loadSlotsList;
        
        private Button closeSaveBtn;
        private Button closeLoadBtn;
        
        private TextField saveNameInput;
        
        private List<SaveSlotInfo> currentSlots = new List<SaveSlotInfo>();
        private bool isInSaveMode = false;
        
        void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        void OnEnable()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("SaveLoadDialogController: UIDocument or root visual element is null");
                return;
            }
            
            root = uiDocument.rootVisualElement;
            CacheReferences();
            BindButtons();
            SubscribeToEvents();
            
            HideDialogs();
        }
        
        void OnDisable()
        {
            UnbindButtons();
            UnsubscribeFromEvents();
        }
        
        void CacheReferences()
        {
            saveDialog = root.Q<VisualElement>("save-dialog");
            loadDialog = root.Q<VisualElement>("load-dialog");
            
            saveSlotsList = root.Q<ScrollView>("save-slots-list");
            loadSlotsList = root.Q<ScrollView>("load-slots-list");
            
            closeSaveBtn = root.Q<Button>("close-save-btn");
            closeLoadBtn = root.Q<Button>("close-load-btn");
            
            saveNameInput = root.Q<TextField>("save-name-input");
            
            if (saveDialog == null) Debug.LogWarning("save-dialog not found");
            if (loadDialog == null) Debug.LogWarning("load-dialog not found");
        }
        
        void BindButtons()
        {
            if (closeSaveBtn != null) closeSaveBtn.clicked += HideDialogs;
            if (closeLoadBtn != null) closeLoadBtn.clicked += HideDialogs;
        }
        
        void UnbindButtons()
        {
            if (closeSaveBtn != null) closeSaveBtn.clicked -= HideDialogs;
            if (closeLoadBtn != null) closeLoadBtn.clicked -= HideDialogs;
        }
        
        void SubscribeToEvents()
        {
            EventBus.Subscribe<RequestOpenSaveDialogEvent>(HandleOpenSaveDialog);
            EventBus.Subscribe<RequestOpenLoadDialogEvent>(HandleOpenLoadDialog);
            EventBus.Subscribe<OnSaveSlotsReceivedEvent>(HandleSaveSlotsReceived);
            EventBus.Subscribe<OnGameSavedEvent>(HandleGameSaved);
            EventBus.Subscribe<OnGameLoadedEvent>(HandleGameLoaded);
        }
        
        void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<RequestOpenSaveDialogEvent>(HandleOpenSaveDialog);
            EventBus.Unsubscribe<RequestOpenLoadDialogEvent>(HandleOpenLoadDialog);
            EventBus.Unsubscribe<OnSaveSlotsReceivedEvent>(HandleSaveSlotsReceived);
            EventBus.Unsubscribe<OnGameSavedEvent>(HandleGameSaved);
            EventBus.Unsubscribe<OnGameLoadedEvent>(HandleGameLoaded);
        }
        
        void HandleOpenSaveDialog(RequestOpenSaveDialogEvent evt)
        {
            isInSaveMode = true;
            EventBus.Publish(new RequestGetSaveSlotsEvent());
        }
        
        void HandleOpenLoadDialog(RequestOpenLoadDialogEvent evt)
        {
            isInSaveMode = false;
            EventBus.Publish(new RequestGetSaveSlotsEvent());
        }
        
        void HandleSaveSlotsReceived(OnSaveSlotsReceivedEvent evt)
        {
            currentSlots = evt.SaveSlots;
            
            if (isInSaveMode)
            {
                ShowSaveDialog();
            }
            else
            {
                ShowLoadDialog();
            }
        }
        
        void HandleGameSaved(OnGameSavedEvent evt)
        {
            if (evt.Success)
            {
                HideDialogs();
            }
        }
        
        void HandleGameLoaded(OnGameLoadedEvent evt)
        {
            if (evt.Success)
            {
                HideDialogs();
            }
        }
        
        void ShowSaveDialog()
        {
            if (saveDialog == null || saveSlotsList == null) return;
            
            PopulateSaveSlots();
            
            if (saveNameInput != null)
            {
                saveNameInput.value = GenerateDefaultSaveName();
            }
            
            saveDialog.style.display = DisplayStyle.Flex;
            loadDialog.style.display = DisplayStyle.None;
        }
        
        void ShowLoadDialog()
        {
            if (loadDialog == null || loadSlotsList == null) return;
            
            PopulateLoadSlots();
            
            saveDialog.style.display = DisplayStyle.None;
            loadDialog.style.display = DisplayStyle.Flex;
        }
        
        void HideDialogs()
        {
            if (saveDialog != null) saveDialog.style.display = DisplayStyle.None;
            if (loadDialog != null) loadDialog.style.display = DisplayStyle.None;
        }
        
        void PopulateSaveSlots()
        {
            saveSlotsList.Clear();
            
            for (int i = 0; i < maxSaveSlots; i++)
            {
                var slotInfo = currentSlots.ElementAtOrDefault(i);
                if (slotInfo == null) slotInfo = new SaveSlotInfo { SlotIndex = i, IsEmpty = true };
                
                var slotCard = CreateSaveSlotCard(slotInfo);
                saveSlotsList.Add(slotCard);
            }
        }
        
        void PopulateLoadSlots()
        {
            loadSlotsList.Clear();
            
            var usedSlots = currentSlots.Where(s => !s.IsEmpty).ToList();
            
            if (usedSlots.Count == 0)
            {
                var emptyLabel = new Label("No save files found");
                emptyLabel.AddToClassList("empty-state-message");
                loadSlotsList.Add(emptyLabel);
                return;
            }
            
            foreach (var slotInfo in usedSlots)
            {
                var slotCard = CreateLoadSlotCard(slotInfo);
                loadSlotsList.Add(slotCard);
            }
        }
        
        VisualElement CreateSaveSlotCard(SaveSlotInfo slotInfo)
        {
            var card = new VisualElement();
            card.AddToClassList("save-slot-card");
            
            var header = new VisualElement();
            header.AddToClassList("slot-header");
            
            var slotLabel = new Label($"Slot {slotInfo.SlotIndex + 1}");
            slotLabel.AddToClassList("slot-number");
            
            header.Add(slotLabel);
            card.Add(header);
            
            if (!slotInfo.IsEmpty)
            {
                var infoContainer = new VisualElement();
                infoContainer.AddToClassList("slot-info");
                
                var nameLabel = new Label(slotInfo.SaveName);
                nameLabel.AddToClassList("slot-name");
                
                var dateLabel = new Label(slotInfo.SaveTimestamp);
                dateLabel.AddToClassList("slot-date");
                
                var statsLabel = new Label($"${slotInfo.Cash:N0} | {slotInfo.Year}/{slotInfo.Month}/{slotInfo.Day} | {slotInfo.EmployeeCount} employees");
                statsLabel.AddToClassList("slot-stats");
                
                infoContainer.Add(nameLabel);
                infoContainer.Add(dateLabel);
                infoContainer.Add(statsLabel);
                
                card.Add(infoContainer);
            }
            else
            {
                var emptyLabel = new Label("Empty Slot");
                emptyLabel.AddToClassList("slot-empty");
                card.Add(emptyLabel);
            }
            
            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("slot-buttons");
            
            var saveButton = new Button(() => OnSaveToSlot(slotInfo.SlotIndex));
            saveButton.text = slotInfo.IsEmpty ? "Save Here" : "Overwrite";
            saveButton.AddToClassList("slot-button");
            saveButton.AddToClassList("slot-button-primary");
            
            buttonContainer.Add(saveButton);
            
            if (!slotInfo.IsEmpty)
            {
                var deleteButton = new Button(() => OnDeleteSlot(slotInfo.SlotIndex));
                deleteButton.text = "Delete";
                deleteButton.AddToClassList("slot-button");
                deleteButton.AddToClassList("slot-button-danger");
                buttonContainer.Add(deleteButton);
            }
            
            card.Add(buttonContainer);
            
            return card;
        }
        
        VisualElement CreateLoadSlotCard(SaveSlotInfo slotInfo)
        {
            var card = new VisualElement();
            card.AddToClassList("save-slot-card");
            
            var header = new VisualElement();
            header.AddToClassList("slot-header");
            
            var slotLabel = new Label($"Slot {slotInfo.SlotIndex + 1}");
            slotLabel.AddToClassList("slot-number");
            
            header.Add(slotLabel);
            card.Add(header);
            
            var infoContainer = new VisualElement();
            infoContainer.AddToClassList("slot-info");
            
            var nameLabel = new Label(slotInfo.SaveName);
            nameLabel.AddToClassList("slot-name");
            
            var dateLabel = new Label(slotInfo.SaveTimestamp);
            dateLabel.AddToClassList("slot-date");
            
            var statsLabel = new Label($"${slotInfo.Cash:N0} | {slotInfo.Year}/{slotInfo.Month}/{slotInfo.Day} | {slotInfo.EmployeeCount} employees | Rep: {slotInfo.Reputation:F0}");
            statsLabel.AddToClassList("slot-stats");
            
            infoContainer.Add(nameLabel);
            infoContainer.Add(dateLabel);
            infoContainer.Add(statsLabel);
            
            card.Add(infoContainer);
            
            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("slot-buttons");
            
            var loadButton = new Button(() => OnLoadFromSlot(slotInfo.SlotIndex));
            loadButton.text = "Load Game";
            loadButton.AddToClassList("slot-button");
            loadButton.AddToClassList("slot-button-primary");
            
            var deleteButton = new Button(() => OnDeleteSlot(slotInfo.SlotIndex));
            deleteButton.text = "Delete";
            deleteButton.AddToClassList("slot-button");
            deleteButton.AddToClassList("slot-button-danger");
            
            buttonContainer.Add(loadButton);
            buttonContainer.Add(deleteButton);
            
            card.Add(buttonContainer);
            
            return card;
        }
        
        void OnSaveToSlot(int slotIndex)
        {
            string saveName = saveNameInput != null ? saveNameInput.value : "Save Game";
            if (string.IsNullOrWhiteSpace(saveName))
            {
                saveName = GenerateDefaultSaveName();
            }
            
            EventBus.Publish(new RequestSaveGameToSlotEvent
            {
                SlotIndex = slotIndex,
                SaveName = saveName
            });
            
            Debug.Log($"Saving to slot {slotIndex} with name: {saveName}");
        }
        
        void OnLoadFromSlot(int slotIndex)
        {
            EventBus.Publish(new RequestLoadGameFromSlotEvent
            {
                SlotIndex = slotIndex
            });
            
            Debug.Log($"Loading from slot {slotIndex}");
        }
        
        void OnDeleteSlot(int slotIndex)
        {
            EventBus.Publish(new RequestDeleteSaveSlotEvent
            {
                SlotIndex = slotIndex
            });
            
            EventBus.Publish(new RequestGetSaveSlotsEvent());
        }
        
        string GenerateDefaultSaveName()
        {
            var timeSystem = FindFirstObjectByType<TimeSystem>();
            if (timeSystem != null)
            {
                var date = timeSystem.CurrentDate;
                return $"Save {date.Year}/{date.Month}/{date.Day}";
            }
            
            return "Save Game";
        }
    }
}
