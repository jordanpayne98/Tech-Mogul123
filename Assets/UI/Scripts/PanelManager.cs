using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TechMogul.UI
{
    public class PanelManager : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject dashboardPanel;
        [SerializeField] private GameObject employeePanel;
        [SerializeField] private GameObject productPanel;
        [SerializeField] private GameObject contractPanel;
        [SerializeField] private GameObject marketPanel;
        
        [Header("Navigation Buttons")]
        [SerializeField] private Button employeeButton;
        [SerializeField] private Button productButton;
        [SerializeField] private Button contractButton;
        [SerializeField] private Button marketButton;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color normalButtonColor = Color.white;
        [SerializeField] private Color activeButtonColor = new Color(0.2f, 0.6f, 1f);
        
        private GameObject _currentActivePanel;
        private Dictionary<GameObject, Button> _panelButtonMap;
        
        void Start()
        {
            InitializePanelMapping();
            SetupNavigationButtons();
            
            // Dashboard is always visible, start with no additional panel
            _currentActivePanel = null;
            UpdatePanelVisibility();
        }
        
        void InitializePanelMapping()
        {
            _panelButtonMap = new Dictionary<GameObject, Button>();
            
            if (employeePanel != null && employeeButton != null)
                _panelButtonMap[employeePanel] = employeeButton;
            
            if (productPanel != null && productButton != null)
                _panelButtonMap[productPanel] = productButton;
            
            if (contractPanel != null && contractButton != null)
                _panelButtonMap[contractPanel] = contractButton;
            
            if (marketPanel != null && marketButton != null)
                _panelButtonMap[marketPanel] = marketButton;
        }
        
        void SetupNavigationButtons()
        {
            if (employeeButton != null)
                employeeButton.onClick.AddListener(() => ShowPanel(employeePanel));
            
            if (productButton != null)
                productButton.onClick.AddListener(() => ShowPanel(productPanel));
            
            if (contractButton != null)
                contractButton.onClick.AddListener(() => ShowPanel(contractPanel));
            
            if (marketButton != null)
                marketButton.onClick.AddListener(() => ShowPanel(marketPanel));
        }
        
        public void ShowPanel(GameObject panel)
        {
            if (panel == null) return;
            
            // Toggle off if clicking the same panel
            if (_currentActivePanel == panel)
            {
                _currentActivePanel = null;
            }
            else
            {
                _currentActivePanel = panel;
            }
            
            UpdatePanelVisibility();
            UpdateButtonHighlights();
        }
        
        void UpdatePanelVisibility()
        {
            // Dashboard always visible
            if (dashboardPanel != null)
                dashboardPanel.SetActive(true);
            
            // Show/hide other panels
            SetPanelActive(employeePanel, _currentActivePanel == employeePanel);
            SetPanelActive(productPanel, _currentActivePanel == productPanel);
            SetPanelActive(contractPanel, _currentActivePanel == contractPanel);
            SetPanelActive(marketPanel, _currentActivePanel == marketPanel);
        }
        
        void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }
        
        void UpdateButtonHighlights()
        {
            foreach (var kvp in _panelButtonMap)
            {
                GameObject panel = kvp.Key;
                Button button = kvp.Value;
                
                if (button != null)
                {
                    bool isActive = (_currentActivePanel == panel);
                    var colors = button.colors;
                    colors.normalColor = isActive ? activeButtonColor : normalButtonColor;
                    button.colors = colors;
                }
            }
        }
        
        public void HideAllPanels()
        {
            _currentActivePanel = null;
            UpdatePanelVisibility();
            UpdateButtonHighlights();
        }
    }
}
