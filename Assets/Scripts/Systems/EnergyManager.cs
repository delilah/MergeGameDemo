using System;
using UnityEngine;
using Zenject;
using MergeGame.Data;
using System.Collections;
using System.Globalization;



namespace MergeGame.Systems
{
    public class EnergyManager : MonoBehaviour
    {
        public event Action OnEnergyChanged;
        public event Action OnEnergyRestored; // fires specifically when crossing 0 -> 1
        private bool _isInitialized = false;

        public enum EnergyCost
        {
            Base = 1,
            Medium = 2,
            High = 3
        }

        private const string PREF_ENERGY = "Energy";
        private const string PREF_LAST_REGEN_TIME = "LastEnergyRegenerationTime";
        private const string PREF_NEXT_REGEN_TIME = "NextEnergyRegenerationTime";

        private GameConfig _config;

        private int _maxEnergy;
        private int _currentEnergy;
        private Coroutine _regenerationCoroutine;

        private DateTime _lastEnergyRegenerationTime;
        private DateTime _nextEnergyRegenerationTime;

        [Inject]
        public void Construct(GameConfig gameConfig)
        {
            _config = gameConfig;
        }

        private void Awake()
        {
            _maxEnergy = _config.maxEnergy;
    
            bool hasSavedEnergy = PlayerPrefs.HasKey(PREF_ENERGY);
            bool hasSavedTime = PlayerPrefs.HasKey(PREF_LAST_REGEN_TIME);

                
            if (hasSavedEnergy && hasSavedTime)
            {
                Load();
            }
            else
            {
                SetEnergy(_maxEnergy);
                _lastEnergyRegenerationTime = DateTime.UtcNow;
                _nextEnergyRegenerationTime = _lastEnergyRegenerationTime.AddSeconds(_config.regenEnergyTime);

                Save(); 
            }

            CheckAndRegenerateIfNeeded();
            _isInitialized = true;
        }

        // If app is in background, doesn't go through awake so OnApplicationFocus is used
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                CheckAndRegenerateIfNeeded();
            }
        }

        // If app goes in background gets paused: save
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
            else
            {
                CheckAndRegenerateIfNeeded();
            }
        }

        // On app quit: save
        private void OnApplicationQuit()
        {
            Save();
        }

        /// <summary>
        /// Checks how much energy has regenerated based on timestamps and restores it.
        /// Preserves the partial cycle so the next unit continues from where it left off.
        /// </summary>
        private void CheckAndRegenerateIfNeeded()
        {
            if (_currentEnergy >= _maxEnergy)
            {
                return;
            }

            TimeSpan timePassed = DateTime.UtcNow - _lastEnergyRegenerationTime;

            int energyToRegenerate = (int)(timePassed.TotalSeconds / _config.regenEnergyTime);
            energyToRegenerate = Mathf.Min(energyToRegenerate, _maxEnergy - _currentEnergy);

            if (energyToRegenerate > 0)
            {
                _currentEnergy += energyToRegenerate;

                // Preserve the partial cycle so the next unit continues from where it left off
                double remainingSeconds = timePassed.TotalSeconds % _config.regenEnergyTime;
                _lastEnergyRegenerationTime = DateTime.UtcNow.AddSeconds(-remainingSeconds);
                _nextEnergyRegenerationTime = _lastEnergyRegenerationTime.AddSeconds(_config.regenEnergyTime);

                OnEnergyChanged?.Invoke();
                Save();
            }

            TryStartRegenerationCoroutine();
        }

        public int GetCurrentEnergy()
        {
            return _currentEnergy;
        }

        public int GetMaxEnergy()
        {
            return _maxEnergy;
        }

        private void SetEnergy(int newValue)
        {
            var _previousEnergy = _currentEnergy;
            _currentEnergy = newValue;

            OnEnergyChanged?.Invoke();

            if (_isInitialized && _previousEnergy == 0 && _currentEnergy == 1)
            {
                OnEnergyRestored?.Invoke();
            }
        }

        /// <summary>
        /// Saves data to PlayerPrefs.
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetInt(PREF_ENERGY, _currentEnergy);
            PlayerPrefs.SetString(PREF_LAST_REGEN_TIME, _lastEnergyRegenerationTime.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(PREF_NEXT_REGEN_TIME, _nextEnergyRegenerationTime.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Loads data from PlayerPrefs.
        /// </summary>
        private void Load()
        {
            SetEnergy(PlayerPrefs.GetInt(PREF_ENERGY, _maxEnergy));
            
            DateTime currentTime = DateTime.UtcNow;
            DateTime lastRegenTime, nextRegenTime;
            
            // Parse last regeneration time with validation
            string lastRegenString = PlayerPrefs.GetString(PREF_LAST_REGEN_TIME, "");
            if (string.IsNullOrEmpty(lastRegenString) || 
                !DateTime.TryParse(lastRegenString, CultureInfo.InvariantCulture, DateTimeStyles.None, out lastRegenTime) ||
                lastRegenTime > currentTime)
            {
                // Reset to current time if invalid or in the future
                lastRegenTime = currentTime;
            }
            
            // Parse next regeneration time with validation
            string nextRegenString = PlayerPrefs.GetString(PREF_NEXT_REGEN_TIME, "");
            if (string.IsNullOrEmpty(nextRegenString) || 
                !DateTime.TryParse(nextRegenString, CultureInfo.InvariantCulture, DateTimeStyles.None, out nextRegenTime) ||
                nextRegenTime <= lastRegenTime || 
                nextRegenTime > currentTime.AddHours(24)) // Reasonable future limit (24 hours)
            {
                // Calculate valid next regeneration time
                nextRegenTime = lastRegenTime.AddSeconds(_config.regenEnergyTime);
            }
            
            _lastEnergyRegenerationTime = lastRegenTime;
            _nextEnergyRegenerationTime = nextRegenTime;
        }

        /// <summary>
        /// Attempts to spend energy based on the specified cost.
        /// Resets the regen timer on success so the next unit takes a full cycle.
        /// </summary>
        /// <param name="cost">The energy cost to spend.</param>
        /// <returns>True if energy was spent, false if there was not enough energy.</returns>
        public bool TrySpendEnergy(EnergyCost cost = EnergyCost.Base)
        {
            int amount = (int)cost;

            if (_currentEnergy < amount)
            {
                return false;
            }

            SetEnergy(_currentEnergy - amount);

            TryStartRegenerationCoroutine();
            Save();

            return true;
        }

        /// <summary>
        /// Starts the regeneration coroutine only if not already running and energy is below max.
        /// Centralised to prevent double start race conditions.
        /// </summary>
        private void TryStartRegenerationCoroutine()
        {
            if (_currentEnergy < _maxEnergy && _regenerationCoroutine == null)
            {
                _regenerationCoroutine = StartCoroutine(RegenerationCoroutine());
            }
        }

        /// <summary>
        /// Regenerates 1 energy unit every regenEnergyTime seconds.
        /// </summary>
        private IEnumerator RegenerationCoroutine()
        {
            while (_currentEnergy < _maxEnergy)
            {
                yield return new WaitForSeconds(_config.regenEnergyTime);
                
                if (_currentEnergy < _maxEnergy)
                {
                    SetEnergy(_currentEnergy + 1);

                    _lastEnergyRegenerationTime = DateTime.UtcNow;
                    _nextEnergyRegenerationTime = _lastEnergyRegenerationTime.AddSeconds(_config.regenEnergyTime);

                    Save();
                }
            }
            
            _regenerationCoroutine = null;
        }
    }
}