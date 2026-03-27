using System;
using UnityEngine;
using Zenject;
using MergeGame.Data;

namespace MergeGame.Systems
{
    public class EnergyManager : MonoBehaviour
    {
        public event Action OnEnergyRegenerated;

        public enum EnergyCost
        {
            Base = 1,
            Medium = 2,
            High = 3
        }

        private GameConfig _config;

        private int _maxEnergy;
        private int _currentEnergy;
        private float _regenTimer = 0f;

        [Inject]
        public void Construct(GameConfig gameConfig)
        {
            _config = gameConfig;
        }

        private void Awake()
        {
            _maxEnergy = _config.maxEnergy;
            _currentEnergy = _maxEnergy;
        }

        private void Update()
        {
            if (_currentEnergy < _maxEnergy)
            {
                RegenerateEnergy();
            }
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
        /// Regenerates 1 energy unit every regenEnergyTime seconds.
        /// </summary>
        private void RegenerateEnergy()
        {
            _regenTimer += Time.deltaTime;

            if (_regenTimer >= _config.regenEnergyTime)
            {
                _currentEnergy++;
                OnEnergyRegenerated?.Invoke();
                _regenTimer = 0f;
            }
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

            if (_currentEnergy <= 0 || _currentEnergy < amount)
            {
                return false;
            }

            _currentEnergy -= amount;
            OnEnergyRegenerated?.Invoke();
            _regenTimer = 0f;
            return true;
        }
    }
}