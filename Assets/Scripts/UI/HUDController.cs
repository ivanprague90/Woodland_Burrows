// ============================================================
//  HUDController.cs — Woodland Burrow
//  Gestisce tutta la UI principale: valute, produzione/s,
//  zona attiva, badge offline. Si abbona agli eventi di
//  ResourceManager senza polling ogni frame.
// ============================================================

using System.Collections;
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

        [Header("Prestige")]
        [SerializeField] private GameObject prestigeButton;

        public static HUDController Instance { get; private set; }

        private Coroutine _badgeRoutine;

        // --------------------------------------------------------

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnAcornsChanged    += UpdateAcorns;
                ResourceManager.Instance.OnSparksChanged    += UpdateSparks;
                ResourceManager.Instance.OnProductionChanged += UpdateRate;
            }
            if (ZoneManager.Instance != null)
                ZoneManager.Instance.OnActiveZoneChanged += UpdateZoneName;
            if (PrestigeManager.Instance != null)
                PrestigeManager.Instance.OnPrestigeAvailable += ShowPrestigeButton;
        }

        private void OnDisable()
        {
            if (ResourceManager.Instance == null) return;
            ResourceManager.Instance.OnAcornsChanged    -= UpdateAcorns;
            ResourceManager.Instance.OnSparksChanged    -= UpdateSparks;
            ResourceManager.Instance.OnProductionChanged -= UpdateRate;
            if (ZoneManager.Instance != null)
                ZoneManager.Instance.OnActiveZoneChanged -= UpdateZoneName;
            if (PrestigeManager.Instance != null)
                PrestigeManager.Instance.OnPrestigeAvailable -= ShowPrestigeButton;
        }

        private void Start()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnAcornsChanged    -= UpdateAcorns;
                ResourceManager.Instance.OnSparksChanged    -= UpdateSparks;
                ResourceManager.Instance.OnProductionChanged -= UpdateRate;
                ResourceManager.Instance.OnAcornsChanged    += UpdateAcorns;
                ResourceManager.Instance.OnSparksChanged    += UpdateSparks;
                ResourceManager.Instance.OnProductionChanged += UpdateRate;

                UpdateAcorns(ResourceManager.Instance.Acorns);
                UpdateSparks(ResourceManager.Instance.Sparks);
                UpdateRate(ResourceManager.Instance.TotalProductionPerSecond);
            }
            if (ZoneManager.Instance != null)
            {
                ZoneManager.Instance.OnActiveZoneChanged -= UpdateZoneName;
                ZoneManager.Instance.OnActiveZoneChanged += UpdateZoneName;
                UpdateZoneName(ZoneManager.Instance.GetActiveZone());
            }
            if (PrestigeManager.Instance != null)
            {
                PrestigeManager.Instance.OnPrestigeAvailable -= ShowPrestigeButton;
                PrestigeManager.Instance.OnPrestigeAvailable += ShowPrestigeButton;
            }

            if (offlineBadge != null) offlineBadge.SetActive(false);
            if (prestigeButton != null) prestigeButton.SetActive(PrestigeManager.Instance?.CanPrestige ?? false);
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
        //  Badge guadagno offline — animato (pop-in / pop-out)
        // --------------------------------------------------------

        public void ShowOfflineBadge(double acornsEarned)
        {
            if (offlineBadge == null) return;
            if (offlineBadgeText != null)
                offlineBadgeText.text = $"+{FormatNumber(acornsEarned)} G offline!";

            if (_badgeRoutine != null) StopCoroutine(_badgeRoutine);
            _badgeRoutine = StartCoroutine(BadgeAnimation());

            AudioManager.Instance?.PlayOfflineBadge();
        }

        private IEnumerator BadgeAnimation()
        {
            offlineBadge.SetActive(true);
            var rt = offlineBadge.GetComponent<RectTransform>();

            // Pop-in: scale 0 → 1.1 → 1.0
            yield return ScaleTo(rt, Vector3.zero,         Vector3.one * 1.1f, 0.20f);
            yield return ScaleTo(rt, Vector3.one * 1.1f,  Vector3.one,        0.10f);

            yield return new WaitForSeconds(4.5f);

            // Pop-out: scale 1.0 → 0
            yield return ScaleTo(rt, Vector3.one, Vector3.zero, 0.18f);

            offlineBadge.SetActive(false);
        }

        private static IEnumerator ScaleTo(RectTransform rt, Vector3 from, Vector3 to, float dur)
        {
            if (rt == null) { yield return new WaitForSeconds(dur); yield break; }
            float e = 0f;
            while (e < dur)
            {
                e += Time.deltaTime;
                rt.localScale = Vector3.Lerp(from, to, e / dur);
                yield return null;
            }
            rt.localScale = to;
        }

        // --------------------------------------------------------
        //  Prestige button
        // --------------------------------------------------------

        private void ShowPrestigeButton()
        {
            if (prestigeButton != null) prestigeButton.SetActive(true);
        }

        public void OnPrestigeButtonClick()
        {
            PrestigeManager.Instance?.DoPrestige();
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
