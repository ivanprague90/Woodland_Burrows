// ============================================================
//  Producer.cs — Woodland Burrow
//  Attach su ogni GameObject che rappresenta una stanza.
//  Legge i dati da ProducerData (ScriptableObject) e
//  accumula risorse nel tempo.
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

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI productionText;
        [SerializeField] private TextMeshProUGUI upgradeCostText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Image iconImage;

        // Stato runtime
        private int    _level            = 0;   // 0 = non ancora acquistato
        private bool   _isActive         = false;
        private double _currentProduction = 0;
        private double _accumulatedTime   = 0;

        // --------------------------------------------------------

        private void Start()
        {
            if (data == null) { Debug.LogError($"[Producer] Nessun ProducerData su {name}!"); return; }

            if (iconImage  != null) iconImage.sprite = data.icon;
            upgradeButton?.onClick.AddListener(TryUpgrade);

            RefreshUI();

            // Notifica ResourceManager della produzione corrente (dopo il load)
            if (_isActive)
                ResourceManager.Instance.RegisterProduction(_currentProduction);
        }

        private void Update()
        {
            if (!_isActive || _currentProduction <= 0) return;

            _accumulatedTime += Time.deltaTime;

            // Accredita ogni secondo (più semplice da capire e debuggare)
            if (_accumulatedTime >= 1f)
            {
                int ticks = (int)_accumulatedTime;
                _accumulatedTime -= ticks;
                ResourceManager.Instance.AddAcorns(_currentProduction * ticks);
            }

            // Aggiorna il bottone (può diventare acquistabile durante il gioco)
            RefreshUpgradeButton();
        }

        // --------------------------------------------------------
        //  Acquisto / Upgrade
        // --------------------------------------------------------

        public void TryUpgrade()
        {
            double cost = data.CostAtLevel(_level);
            if (!ResourceManager.Instance.SpendAcorns(cost)) return;

            // Rimuovi la vecchia produzione dal totale globale
            if (_isActive)
                ResourceManager.Instance.UnregisterProduction(_currentProduction);

            _level++;
            _isActive = true;
            _currentProduction = data.ProductionAtLevel(_level - 1);

            // Registra la nuova produzione
            ResourceManager.Instance.RegisterProduction(_currentProduction);

            RefreshUI();
        }

        // --------------------------------------------------------
        //  UI
        // --------------------------------------------------------

        private void RefreshUI()
        {
            if (levelText != null)
                levelText.text = _level == 0 ? "ACQUISTA" : $"Liv. {_level}";

            if (productionText != null)
                productionText.text = _isActive
                    ? $"{FormatNumber(_currentProduction)}/s"
                    : "—";

            if (upgradeCostText != null)
                upgradeCostText.text = $"{FormatNumber(data.CostAtLevel(_level))} 🌰";

            RefreshUpgradeButton();
        }

        private void RefreshUpgradeButton()
        {
            if (upgradeButton == null) return;
            upgradeButton.interactable =
                ResourceManager.Instance.CanAfford(data.CostAtLevel(_level));
        }

        // --------------------------------------------------------
        //  Serializzazione (usata da SaveManager)
        // --------------------------------------------------------

        public ProducerData GetData() => data;
        public int  GetLevel()   => _level;
        public void SetLevel(int level)
        {
            _level    = level;
            _isActive = level > 0;
            _currentProduction = _isActive ? data.ProductionAtLevel(_level - 1) : 0;
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
