// ============================================================
//  Tapper.cs — Woodland Burrow
//  Attach su un UI Button. Ogni tap aggiunge ghiande
//  in base a GameConstants.AcornsPerTap, moltiplicato
//  dai bonus attivi (Cuccia Morbida, ecc).
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WoodlandBurrow.UI;

namespace WoodlandBurrow
{
    public class Tapper : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button tapButton;
        [SerializeField] private FloatingText floatingTextPrefab;
        [SerializeField] private RectTransform floatingTextParent;

        [Header("Feel")]
        [SerializeField] private float punchScale    = 1.15f;
        [SerializeField] private float punchDuration = 0.08f;

        [Header("Particelle")]
        [SerializeField] private ParticleSystem tapParticles;

        private Vector3 _baseScale;

        // --------------------------------------------------------

        private void Awake()
        {
            _baseScale = tapButton.transform.localScale;
            tapButton.onClick.AddListener(OnTap);
        }

        // --------------------------------------------------------

        public void OnTap()
        {
            double tapMult      = ResourceManager.Instance.TapMultiplier;
            double prestigeMult = PrestigeManager.Instance?.PrestigeMultiplier ?? 1.0;
            double earned       = GameConstants.AcornsPerTap * tapMult * prestigeMult;
            ResourceManager.Instance.AddAcorns(earned);

            SpawnFloatingText(earned);
            StartCoroutine(PunchAnimation());

            tapParticles?.Play();
            AudioManager.Instance?.PlayTap();
        }

        // --------------------------------------------------------
        //  Floating text "+6 🌰"
        // --------------------------------------------------------

        private void SpawnFloatingText(double amount)
        {
            if (floatingTextPrefab == null || floatingTextParent == null) return;

            var ft = Instantiate(floatingTextPrefab, floatingTextParent);
            var startPos = new Vector2(Random.Range(-40f, 40f), Random.Range(0f, 30f));
            ft.Play($"+{FormatNumber(amount)} G", startPos);
        }

        // --------------------------------------------------------
        //  Punch scale (senza librerie esterne)
        // --------------------------------------------------------

        private System.Collections.IEnumerator PunchAnimation()
        {
            tapButton.transform.localScale = _baseScale * punchScale;
            yield return new WaitForSeconds(punchDuration);
            tapButton.transform.localScale = _baseScale;
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
