using UnityEngine;
using System.Collections.Generic;
using TechMogul.Core;

namespace TechMogul.Systems
{
    public class RivalSystem : MonoBehaviour
    {
        public static RivalSystem Instance { get; private set; }
        
        private List<RivalCompanyData> _rivals = new List<RivalCompanyData>();
        
        public IReadOnlyList<RivalCompanyData> Rivals => _rivals.AsReadOnly();
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            InitializeRivals();
        }
        
        void InitializeRivals()
        {
            _rivals = new List<RivalCompanyData>
            {
                new RivalCompanyData(
                    "rival_techvision",
                    "TechVision Inc",
                    "Software",
                    25f,
                    150,
                    "Established software company focusing on enterprise solutions"
                ),
                new RivalCompanyData(
                    "rival_innovatex",
                    "InnovateX",
                    "Hardware",
                    18f,
                    200,
                    "Hardware manufacturer known for cutting-edge devices"
                ),
                new RivalCompanyData(
                    "rival_cloudserve",
                    "CloudServe Solutions",
                    "Service",
                    15f,
                    80,
                    "Cloud service provider with growing market presence"
                ),
                new RivalCompanyData(
                    "rival_datacore",
                    "DataCore Systems",
                    "Software",
                    12f,
                    95,
                    "Database and analytics specialist for large enterprises"
                ),
                new RivalCompanyData(
                    "rival_pixelforge",
                    "PixelForge Studios",
                    "Software",
                    8f,
                    45,
                    "Creative software company focused on design tools"
                )
            };
            
            EventBus.Publish(new OnRivalsInitializedEvent(_rivals.Count));
            
            Debug.Log($"Initialized {_rivals.Count} rival companies");
        }
        
        public float GetTotalRivalMarketShare()
        {
            float total = 0f;
            foreach (var rival in _rivals)
            {
                total += rival.MarketShare;
            }
            return total;
        }
        
        public RivalCompanyData GetRivalById(string rivalId)
        {
            return _rivals.Find(r => r.CompanyId == rivalId);
        }
    }
}
