using System;
using UnityEngine;
using Zenject;
using MergeGame.Data;
using System.Collections;

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
        private Coroutine _regenerationCoroutine;

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

        public int GetCurrentEnergy()
        {
            return _currentEnergy;
        }

        public int GetMaxEnergy()
        {
            return _maxEnergy;
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

            if (_currentEnergy < _maxEnergy  && _regenerationCoroutine == null)
            {
                 _regenerationCoroutine = StartCoroutine(RegenerationCoroutine());
            }

            return true;
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
                    OnEnergyRegenerated?.Invoke();
                }
            }
            
            _regenerationCoroutine = null;
        }
    }
}