using UnityEngine;
using TMPro;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.UI
{
    public class DateDisplay : UIController
    {
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private bool showYear = true;
        
        private string[] _monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                         "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            var timeSystem = FindFirstObjectByType<TimeSystem>();
            if (timeSystem != null && timeSystem.CurrentDate != null)
            {
                UpdateDisplay(timeSystem.CurrentDate);
            }
        }
        
        protected override void SubscribeToEvents()
        {
            Subscribe<OnDayTickEvent>(HandleDayTick);
        }
        
        void HandleDayTick(OnDayTickEvent evt)
        {
            UpdateDisplay(evt.CurrentDate);
        }
        
        void UpdateDisplay(GameDate date)
        {
            if (dateText != null && date != null)
            {
                string monthName = date.Month >= 1 && date.Month <= 12 
                    ? _monthNames[date.Month - 1] 
                    : "???";
                
                if (showYear)
                {
                    dateText.text = $"{monthName} {date.Day}, {date.Year}";
                }
                else
                {
                    dateText.text = $"{monthName} {date.Day}";
                }
            }
        }
    }
}
