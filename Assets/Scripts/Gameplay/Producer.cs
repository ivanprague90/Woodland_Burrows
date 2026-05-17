// ============================================================
//  Producer.cs — Woodland Burrow
//  Logica pura di una stanza: acquisto, upgrade, produzione.
//  La UI è gestita da ProducerCardUI — questo script non
//  registra listener sul bottone per evitare doppio trigger.
// ============================================================

using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace WoodlandBurrow
{
    public class Producer : MonoBehaviour
    {
        [Header("Dati")]
        public ProducerData data;

        // Campi UI opzionali — usati solo se il Producer è standalone
        // (senza ProducerCardUI sullo stesso GameObject).
        [Header("UI standalone (lascia vuoti se usi ProducerCardUI)")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI productionText;
        [SerializeField] private TextMeshProUGUI upgradeCostText;
        [SerializeField] private Image iconImage;

        // Stato runtime
        private int    _level            = 0;
        private bool   _isActive         = false;
        private double _currentProduction = 0;  // acorns/s OPPURE valore moltiplicatore tap
        private double _accumulatedTime   = 0;
        private double _tapContrib        = 1.0; // contributo attuale al TapMultiplier globale

        // --------------------------------------------------------

        private void Start()
        {
            if (data == null) { Debug.LogError($"[Producer] Nessun ProducerData su {name}!"); return; }
            if (iconImage != null) iconImage.sprite = data.icon;
            // NESSUN listener sul bottone: ProducerCardUI gestisce il click.
            RefreshUI();
        }

        private void Update()
        {
            if (!_isActive || _currentProduction <= 0) return;
            if (data.producerType == ProducerType.TapMultiplier) return;

            _accumulatedTime += Time.deltaTime;
            if (_accumulatedTime < 1f) return;

            int ticks = (int)_accumulatedTime;
            _accumulatedTime -= ticks;
            double earned = _currentProduction * ticks;

            if (data.producesSecondaryCurrency)
            {
                switch (data.secondaryCurrency)
                {
                    case SecondaryCurrency.Mushrooms:
                        ResourceManager.Instance.AddMushrooms(earned); break;
                    case SecondaryCurrency.Sparks:
                        ResourceManager.Instance.AddSparks(earned);    break;
                }
            }
            else
            {
                ResourceManager.Instance.AddAcorns(earned);
            }

            RefreshUpgradeButton();
        }

        // --------------------------------------------------------
        //  Acquisto / Upgrade
        // --------------------------------------------------------

        public void TryUpgrade()
        {
            double cost = data.CostAtLevel(_level);
            if (!ResourceManager.Instance.SpendAcorns(cost)) return;

            bool firstPurchase = _level == 0;
            _level++;
            _isActive = true;

            double prestigeMult = PrestigeManager.Instance?.PrestigeMultiplier ?? 1.0;

            if (data.producerType == ProducerType.TapMultiplier)
            {
                double newContrib = data.ProductionAtLevel(_level - 1) * prestigeMult;
                ResourceManager.Instance.UpdateTapContribution(_tapContrib, newContrib);
                _tapContrib        = newContrib;
                _currentProduction = newContrib; // usato solo per la UI
            }
            else
            {
                if (_isActive && _level > 1)
                    ResourceManager.Instance.UnregisterProduction(_currentProduction);

                _currentProduction = data.ProductionAtLevel(_level - 1) * prestigeMult;

                if (!data.producesSecondaryCurrency)
                    ResourceManager.Instance.RegisterProduction(_currentProduction);
            }

            AudioManager.Instance?.PlayUpgrade();
            WoodlandBurrow.UI.ScreenShake.Instance?.Shake(firstPurchase
                ? WoodlandBurrow.UI.ScreenShake.Intensity.Medium
                : WoodlandBurrow.UI.ScreenShake.Intensity.Small);

            RefreshUI();
        }

        // --------------------------------------------------------
        //  UI interna (standalone)
        // --------------------------------------------------------

        private void RefreshUI()
        {
            if (levelText != null)
                levelText.text = _level == 0 ? "ACQUISTA" : $"Liv. {_level}";

            if (productionText != null)
            {
                if (!_isActive)
                {
                    productionText.text = "—";
                }
                else if (data.producerType == ProducerType.TapMultiplier)
                {
                    productionText.text = $"×{FormatNumber(_currentProduction)} tap";
                }
                else if (data.producesSecondaryCurrency)
                {
                    string sym = data.secondaryCurrency == SecondaryCurrency.Mushrooms ? "🍄" : "✨";
                    productionText.text = $"{FormatNumber(_currentProduction)}{sym}/s";
                }
                else
                {
                    productionText.text = $"{FormatNumber(_currentProduction)}/s";
                }
            }

            if (upgradeCostText != null)
                upgradeCostText.text = $"{FormatNumber(data.CostAtLevel(_level))} G";

            RefreshUpgradeButton();
        }

        private void RefreshUpgradeButton()
        {
            // Producer non ha più riferimento al bottone — gestito da ProducerCardUI.
        }

        // --------------------------------------------------------
        //  Serializzazione (SaveManager)
        // --------------------------------------------------------

        public ProducerData GetData()  => data;
        public int          GetLevel() => _level;

        public void SetLevel(int level)
        {
            double prestigeMult = PrestigeManager.Instance?.PrestigeMultiplier ?? 1.0;

            if (data.producerType == ProducerType.TapMultiplier)
            {
                double newContrib = level > 0 ? data.ProductionAtLevel(level - 1) * prestigeMult : 1.0;
                ResourceManager.Instance.UpdateTapContribution(_tapContrib, newContrib);
                _tapContrib        = newContrib;
                _currentProduction = newContrib;
            }
            else
            {
                if (_isActive && !data.producesSecondaryCurrency)
                    ResourceManager.Instance.UnregisterProduction(_currentProduction);

                _currentProduction = level > 0 ? data.ProductionAtLevel(level - 1) * prestigeMult : 0;

                if (level > 0 && !data.producesSecondaryCurrency)
                    ResourceManager.Instance.RegisterProduction(_currentProduction);
            }

            _level    = level;
            _isActive = level > 0;
            RefreshUI();
        }

        // --------------------------------------------------------

        private static string FormatNumber(double n)
        {
            if (n >= 1_000_000) return $"{n / 1_000_000:0.#}M";
            if (n >= 1_000)     return $"{n / 1_000:0.#}k";
            return $"{n:0.#}";
        }
    }
}
