// ============================================================
//  HUDController.cs — Woodland Burrow
//  Gestisce tutta la UI principale: valute, produzione/s,
//  zona attiva, badge offline. Si abbona agli eventi di
//  ResourceManager senza polling ogni frame.
// ============================================================

using UnityEngine;
using TMPro;

namespace WoodlandBurrow.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Valute — Top Bar")]
        [SerializeField] private TextMeshProUGUI acornsText;
        [SerializeField] private TextMeshProUGUI acornsRateText;   // "+8.0/s"
        [SerializeField] private TextMeshProUGUI sparksText;

        [Header("Zona attiva")]
        [SerializeField] private TextMeshProUGUI zoneNameText;

        [Header("Badge offline")]
        [SerializeField] private GameObject      offlineBadge;
        [SerializeField] private TextMeshProUGUI offlineBadgeText;

        // --------------------------------------------------------

        private void OnEnable()
        {
            ResourceManager.Instance.OnAcornsChanged    += UpdateAcorns;
            ResourceManager.Instance.OnSparksChanged    += UpdateSparks;
            ResourceManager.Instance.OnProductionChanged += UpdateRate;
            ZoneManager.Instance.OnActiveZoneChanged    += UpdateZoneName;
        }

        private void OnDisable()
        {
            if (ResourceManager.Instance == null) return;
            ResourceManager.Instance.OnAcornsChanged    -= UpdateAcorns;
            ResourceManager.Instance.OnSparksChanged    -= UpdateSparks;
            ResourceManager.Instance.OnProductionChanged -= UpdateRate;
            ZoneManager.Instance.OnActiveZoneChanged    -= UpdateZoneName;
        }

        private void Start()
        {
            // Inizializzazione con i valori correnti
            UpdateAcorns(ResourceManager.Instance.Acorns);
            UpdateSparks(ResourceManager.Instance.Sparks);
            UpdateRate(ResourceManager.Instance.TotalProductionPerSecond);
            UpdateZoneName(ZoneManager.Instance.GetActiveZone());
        }

        // --------------------------------------------------------
        //  Callbacks
        // --------------------------------------------------------

        private void UpdateAcorns(double amount)
        {
            if (acornsText != null)
                acornsText.text = FormatNumber(amount);
        }

        private void UpdateSparks(double amount)
        {
            if (sparksText != null)
                sparksText.text = FormatNumber(amount);
        }

        private void UpdateRate(double perSecond)
        {
            if (acornsRateText != null)
                acornsRateText.text = $"+{FormatNumber(perSecond)}/s";
        }

        private void UpdateZoneName(ZoneData zone)
        {
            if (zoneNameText != null && zone != null)
                zoneNameText.text = zone.zoneName;
        }

        // --------------------------------------------------------
        //  Badge guadagno offline
        // --------------------------------------------------------

        public void ShowOfflineBadge(double acornsEarned)
        {
            if (offlineBadge == null) return;
            offlineBadge.SetActive(true);
            if (offlineBadgeText != null)
                offlineBadgeText.text = $"+{FormatNumber(acornsEarned)} 🌰 offline!";

            // Auto-dismiss dopo 5 secondi
            Invoke(nameof(HideOfflineBadge), 5f);
        }

        private void HideOfflineBadge()
        {
            if (offlineBadge != null) offlineBadge.SetActive(false);
        }

        // --------------------------------------------------------

        private static string FormatNumber(double n)
        {
            if (n >= 1_000_000_000) return $"{n / 1_000_000_000:0.##}B";
            if (n >= 1_000_000)     return $"{n / 1_000_000:0.##}M";
            if (n >= 1_000)         return $"{n / 1_000:0.#}k";
            return $"{n:0}";
        }
    }
}
