using UnityEngine;
using UnityEngine.UI;
using TechMogul.Core;
using TechMogul.Systems;

namespace TechMogul.UI
{
    public class TimeControls : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button normalButton;
        [SerializeField] private Button fastButton;
        [SerializeField] private Button fasterButton;
        [SerializeField] private Button fastestButton;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color activeColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private float activeScale = 1.1f;
        
        private TimeSpeed _currentSpeed;
        
        void Start()
        {
            SetupButtons();
        }
        
        void OnEnable()
        {
            EventBus.Subscribe<OnSpeedChangedEvent>(HandleSpeedChanged);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<OnSpeedChangedEvent>(HandleSpeedChanged);
        }
        
        void SetupButtons()
        {
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => RequestSpeedChange(TimeSpeed.Paused));
            
            if (normalButton != null)
                normalButton.onClick.AddListener(() => RequestSpeedChange(TimeSpeed.Normal));
            
            if (fastButton != null)
                fastButton.onClick.AddListener(() => RequestSpeedChange(TimeSpeed.Fast));
            
            if (fasterButton != null)
                fasterButton.onClick.AddListener(() => RequestSpeedChange(TimeSpeed.Faster));
            
            if (fastestButton != null)
                fastestButton.onClick.AddListener(() => RequestSpeedChange(TimeSpeed.Fastest));
        }
        
        void RequestSpeedChange(TimeSpeed speed)
        {
            EventBus.Publish(new RequestChangeSpeedEvent { Speed = speed });
        }
        
        void HandleSpeedChanged(OnSpeedChangedEvent evt)
        {
            _currentSpeed = evt.NewSpeed;
            HighlightActiveButton(evt.NewSpeed);
        }
        
        void HighlightActiveButton(TimeSpeed activeSpeed)
        {
            ResetAllButtons();
            
            Button activeButton = activeSpeed switch
            {
                TimeSpeed.Paused => pauseButton,
                TimeSpeed.Normal => normalButton,
                TimeSpeed.Fast => fastButton,
                TimeSpeed.Faster => fasterButton,
                TimeSpeed.Fastest => fastestButton,
                _ => null
            };
            
            if (activeButton != null)
            {
                var colors = activeButton.colors;
                colors.normalColor = activeColor;
                colors.highlightedColor = activeColor;
                activeButton.colors = colors;
                
                activeButton.transform.localScale = Vector3.one * activeScale;
            }
        }
        
        void ResetAllButtons()
        {
            ResetButton(pauseButton);
            ResetButton(normalButton);
            ResetButton(fastButton);
            ResetButton(fasterButton);
            ResetButton(fastestButton);
        }
        
        void ResetButton(Button button)
        {
            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = normalColor;
                colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.3f);
                button.colors = colors;
                
                button.transform.localScale = Vector3.one;
            }
        }
    }
}
