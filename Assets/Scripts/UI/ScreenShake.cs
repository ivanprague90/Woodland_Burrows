// ============================================================
//  ScreenShake.cs — Woodland Burrow
//  Attach su un GameObject nella scena. Shakea la camera
//  principale quando viene chiamato Shake(Intensity).
//  Tre livelli: Small (upgrade), Medium (primo acquisto),
//  Big (zona sbloccata).
// ============================================================

using System.Collections;
using UnityEngine;

namespace WoodlandBurrow.UI
{
    public class ScreenShake : MonoBehaviour
    {
        public static ScreenShake Instance { get; private set; }

        public enum Intensity { Small, Medium, Big }

        [SerializeField] private Camera targetCamera;

        private Coroutine _shakeRoutine;
        private Vector3   _camOriginalPos;

        // --------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera != null) _camOriginalPos = targetCamera.transform.localPosition;
        }

        // --------------------------------------------------------

        public void Shake(Intensity intensity)
        {
            float mag, dur;
            switch (intensity)
            {
                case Intensity.Big:    mag = 0.20f; dur = 0.45f; break;
                case Intensity.Medium: mag = 0.12f; dur = 0.28f; break;
                default:               mag = 0.06f; dur = 0.15f; break;
            }

            if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(DoShake(mag, dur));
        }

        // --------------------------------------------------------

        private IEnumerator DoShake(float magnitude, float duration)
        {
            if (targetCamera == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float strength = magnitude * (1f - elapsed / duration);
                targetCamera.transform.localPosition =
                    _camOriginalPos + (Vector3)(Random.insideUnitCircle * strength);
                yield return null;
            }
            targetCamera.transform.localPosition = _camOriginalPos;
        }
    }
}
