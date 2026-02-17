using UnityEngine;
using TMPro;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.UI
{
    public class DateDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private bool showYear = true;
        
        private string[] _monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                         "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        
        void OnEnable()
        {
            EventBus.Subscribe<OnDayTickEvent>(HandleDayTick);
            
            // Try to get initial date from TimeSystem
            var timeSystem = FindObjectOfType<TimeSystem>();
            if (timeSystem != null && timeSystem.CurrentDate != null)
            {
                UpdateDisplay(timeSystem.CurrentDate);
            }
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<OnDayTickEvent>(HandleDayTick);
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
