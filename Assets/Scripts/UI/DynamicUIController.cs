using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using MergeGame.Systems;
 
namespace MergeGame.UI
{
    /// <summary>
    /// UI controller for dynamic elements.
    /// Assigned to DynamicCanvas, separated from StaticCanvas to avoid
    /// triggering full Canvas rebuilds on static elements when collection updates.
    /// </summary>
     
    public class DynamicUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _energyLevelText;
        [SerializeField] private Slider _energyLevelBar;

        private EnergyManager _energyManager;

        [Inject]
        public void Construct(EnergyManager energyManager)
        {
            _energyManager = energyManager;
        }

        private void OnEnable()
        {
            if (_energyManager == null)
            {
                return;
            }
        }

        private void Start()
        {
            if (_energyManager != null)
            {
                 _energyManager.OnEnergyRegenerated += UpdateEnergy;
            }

            UpdateEnergy();
        }

        private void OnDestroy()
        {
            if (_energyManager != null)
            {
                _energyManager.OnEnergyRegenerated -= UpdateEnergy;
            }
        }

        private void UpdateEnergy()
        {
            if (_energyManager != null)
            {
                if (_energyLevelText != null)
                {
                    _energyLevelText.text = _energyManager.GetCurrentEnergy().ToString();
                }

                if (_energyLevelBar != null)
                {
                    _energyLevelBar.maxValue = _energyManager.GetMaxEnergy();
                    _energyLevelBar.value = _energyManager.GetCurrentEnergy();
                }
            }
        }
    }
}

