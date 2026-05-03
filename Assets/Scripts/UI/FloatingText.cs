// ============================================================
//  FloatingText.cs — Woodland Burrow
//  Attach sul prefab del floating text.
//  Si anima verso l'alto e svanisce in ~1 secondo.
//  Usa solo Coroutine Unity, nessuna libreria esterna.
// ============================================================

using System.Collections;
using UnityEngine;
using TMPro;

namespace WoodlandBurrow.UI
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float riseDistance = 80f;
        [SerializeField] private float duration     = 1.1f;

        // --------------------------------------------------------

        public void Play(string text, Vector2 startPos)
        {
            if (label != null) label.text = text;

            var rt = GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = startPos;

            StartCoroutine(Animate(rt));
        }

        private IEnumerator Animate(RectTransform rt)
        {
            float elapsed = 0f;
            Vector2 start = rt != null ? rt.anchoredPosition : Vector2.zero;

            Color startColor = label != null ? label.color : Color.white;
            Vector3 startScale = transform.localScale;

            // Pop in
            transform.localScale = startScale * 0.6f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Rise
                if (rt != null)
                    rt.anchoredPosition = start + Vector2.up * (riseDistance * t);

                // Fade out (inizia dopo metà durata)
                if (label != null)
                {
                    float alpha = t < 0.5f ? 1f : 1f - ((t - 0.5f) / 0.5f);
                    label.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                }

                // Pop scale
                float scaleT = Mathf.Clamp01(elapsed / 0.12f);
                transform.localScale = Vector3.Lerp(startScale * 0.6f, startScale, scaleT);

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
