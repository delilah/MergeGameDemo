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
                _currentEnergy = _maxEnergy;
                _lastEnergyRegenerationTime = DateTime.Now;
                _nextEnergyRegenerationTime = _lastEnergyRegenerationTime.AddSeconds(_config.regenEnergyTime);

                Save(); 
            }

            CheckAndRegenerateIfNeeded();
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

            TimeSpan timePassed = DateTime.Now - _lastEnergyRegenerationTime;

            int energyToRegenerate = (int)(timePassed.TotalSeconds / _config.regenEnergyTime);
            energyToRegenerate = Mathf.Min(energyToRegenerate, _maxEnergy - _currentEnergy);

            if (energyToRegenerate > 0)
            {
                _currentEnergy += energyToRegenerate;

                // Preserve the partial cycle so the next unit continues from where it left off
                double remainingSeconds = timePassed.TotalSeconds % _config.regenEnergyTime;
                _lastEnergyRegenerationTime = DateTime.Now.AddSeconds(-remainingSeconds);
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
        public void Load()
        {
            _currentEnergy = PlayerPrefs.GetInt(PREF_ENERGY, _maxEnergy);
            
            _lastEnergyRegenerationTime = DateTime.Parse(
                PlayerPrefs.GetString(PREF_LAST_REGEN_TIME, DateTime.Now.ToString(CultureInfo.InvariantCulture)),
                CultureInfo.InvariantCulture
            );
            
            _nextEnergyRegenerationTime = DateTime.Parse(
                PlayerPrefs.GetString(PREF_NEXT_REGEN_TIME, DateTime.Now.ToString(CultureInfo.InvariantCulture)),
                CultureInfo.InvariantCulture
            );
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

            _currentEnergy -= amount;
            OnEnergyChanged?.Invoke();

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
                    _currentEnergy++;
                    OnEnergyChanged?.Invoke();

                    _lastEnergyRegenerationTime = DateTime.Now;
                    _nextEnergyRegenerationTime = _lastEnergyRegenerationTime.AddSeconds(_config.regenEnergyTime);

                    Save();
                }
            }
            
            _regenerationCoroutine = null;
        }
    }
}