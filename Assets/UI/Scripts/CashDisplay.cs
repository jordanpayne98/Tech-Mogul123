using UnityEngine;
using TMPro;
using TechMogul.Core;

namespace TechMogul.UI
{
    public class CashDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cashText;
        [SerializeField] private bool useAnimation = true;
        [SerializeField] private float animationDuration = 0.5f;
        
        private float _currentDisplayCash;
        private float _targetCash;
        private float _animationTimer;
        
        void OnEnable()
        {
            EventBus.Subscribe<OnCashChangedEvent>(HandleCashChanged);
            
            // Request initial cash value
            if (GameManager.Instance != null)
            {
                _currentDisplayCash = GameManager.Instance.CurrentCash;
                _targetCash = _currentDisplayCash;
                UpdateDisplay();
            }
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe<OnCashChangedEvent>(HandleCashChanged);
        }
        
        void Update()
        {
            if (useAnimation && _animationTimer > 0)
            {
                _animationTimer -= Time.deltaTime;
                float t = 1f - (_animationTimer / animationDuration);
                _currentDisplayCash = Mathf.Lerp(_currentDisplayCash, _targetCash, t);
                UpdateDisplay();
            }
        }
        
        void HandleCashChanged(OnCashChangedEvent evt)
        {
            _targetCash = evt.NewCash;
            
            if (useAnimation)
            {
                _animationTimer = animationDuration;
            }
            else
            {
                _currentDisplayCash = _targetCash;
                UpdateDisplay();
            }
        }
        
        void UpdateDisplay()
        {
            if (cashText != null)
            {
                cashText.text = $"${_currentDisplayCash:N0}";
                
                // Color based on cash level (optional visual feedback)
                if (_currentDisplayCash < 10000)
                {
                    cashText.color = new Color(0.91f, 0.30f, 0.24f); // Red/Danger
                }
                else if (_currentDisplayCash < 25000)
                {
                    cashText.color = new Color(0.95f, 0.61f, 0.07f); // Orange/Warning
                }
                else
                {
                    cashText.color = new Color(0.18f, 0.80f, 0.44f); // Green/Success
                }
            }
        }
    }
}
