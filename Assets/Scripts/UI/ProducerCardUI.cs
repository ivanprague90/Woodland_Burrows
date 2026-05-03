// ============================================================
//  ProducerCardUI.cs — Woodland Burrow
//  Attach su ogni card produttore nella barra in basso.
//  Si occupa di mostrare icona, nome, produzione, costo,
//  e di evidenziare la card quando il giocatore può permettersela.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WoodlandBurrow.UI
{
    public class ProducerCardUI : MonoBehaviour
    {
        [Header("Riferimento Producer")]
        [SerializeField] private Producer producer;

        [Header("UI Elements")]
        [SerializeField] private Image           iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI rateText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button          upgradeButton;
        [SerializeField] private GameObject      affordableGlow;  // bordo dorato
        [SerializeField] private GameObject      lockedOverlay;   // overlay lock

        // Colori
        [Header("Colori")]
        [SerializeField] private Color activeRateColor   = new Color(0.62f, 1f, 0.9f);
        [SerializeField] private Color inactiveRateColor = new Color(1f, 1f, 1f, 0.35f);

        private bool _wasAffordable = false;

        // --------------------------------------------------------

        private void Start()
        {
            upgradeButton?.onClick.AddListener(OnUpgradeClick);
            ResourceManager.Instance.OnAcornsChanged += _ => RefreshAffordable();
            Refresh();
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.OnAcornsChanged -= _ => RefreshAffordable();
        }

        // --------------------------------------------------------

        public void Refresh()
        {
            if (producer == null || producer.GetData() == null) return;
            var data  = producer.GetData();
            int level = producer.GetLevel();
            bool active = level > 0;
            bool locked = false; // TODO: collega a ZoneManager per stanze bloccate

            if (iconImage != null && data.icon != null)
                iconImage.sprite = data.icon;

            if (nameText  != null) nameText.text  = data.producerName.ToUpper();
            if (levelText != null) levelText.text  = active ? $"Lv{level}" : "Lv0";

            if (rateText != null)
            {
                rateText.color = active ? activeRateColor : inactiveRateColor;
                rateText.text  = active
                    ? $"{FormatNumber(data.ProductionAtLevel(level - 1))}/s"
                    : "—";
            }

            if (costText != null)
                costText.text = $"🌰 {FormatNumber(data.CostAtLevel(level))}";

            if (lockedOverlay != null) lockedOverlay.SetActive(locked);
            if (upgradeButton != null) upgradeButton.interactable = !locked;

            RefreshAffordable();
        }

        private void RefreshAffordable()
        {
            if (producer == null) return;
            double cost = producer.GetData().CostAtLevel(producer.GetLevel());
            bool affordable = ResourceManager.Instance.CanAfford(cost);

            if (affordableGlow != null) affordableGlow.SetActive(affordable);

            // Piccola animazione quando diventa affordable
            if (affordable && !_wasAffordable)
                StartCoroutine(PulseAnimation());

            _wasAffordable = affordable;
        }

        private void OnUpgradeClick()
        {
            producer?.TryUpgrade();
            Refresh();
        }

        // --------------------------------------------------------
        //  Pulse animation quando diventa acquistabile
        // --------------------------------------------------------

        private System.Collections.IEnumerator PulseAnimation()
        {
            var rt = GetComponent<RectTransform>();
            if (rt == null) yield break;

            Vector3 original = rt.localScale;
            rt.localScale = original * 1.08f;
            yield return new WaitForSeconds(0.12f);
            rt.localScale = original;
        }

        // --------------------------------------------------------

        private static string FormatNumber(double n)
        {
            if (n >= 1_000_000) return $"{n / 1_000_000:0.#}M";
            if (n >= 1_000)     return $"{n / 1_000:0.#}k";
            return $"{n:0}";
        }
    }
}
