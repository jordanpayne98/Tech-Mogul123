using UnityEngine;
using TMPro;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.UI
{
    public class SpeedIndicator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private bool showMultiplier = true;
        
        void OnEnable()
        {
            EventBus.Subscribe<OnSpeedChangedEvent>(HandleSpeedChanged);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<OnSpeedChangedEvent>(HandleSpeedChanged);
        }
        
        void HandleSpeedChanged(OnSpeedChangedEvent evt)
        {
            UpdateDisplay(evt.NewSpeed, evt.Multiplier);
        }
        
        void UpdateDisplay(TimeSpeed speed, float multiplier)
        {
            if (speedText != null)
            {
                if (speed == TimeSpeed.Paused)
                {
                    speedText.text = "PAUSED";
                    speedText.color = new Color(0.91f, 0.30f, 0.24f); // Red
                }
                else
                {
                    if (showMultiplier)
                    {
                        speedText.text = $"{multiplier}x";
                    }
                    else
                    {
                        speedText.text = speed.ToString();
                    }
                    
                    speedText.color = speed switch
                    {
                        TimeSpeed.Normal => Color.white,
                        TimeSpeed.Fast => new Color(0.95f, 0.61f, 0.07f), // Orange
                        TimeSpeed.Faster => new Color(1f, 0.4f, 0.0f), // Dark Orange
                        TimeSpeed.Fastest => new Color(0.91f, 0.30f, 0.24f), // Red
                        _ => Color.white
                    };
                }
            }
        }
    }
}
