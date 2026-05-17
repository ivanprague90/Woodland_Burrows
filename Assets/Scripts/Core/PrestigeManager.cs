// ============================================================
//  PrestigeManager.cs — Woodland Burrow
//  Gestisce il sistema prestige: reset del gioco con bonus
//  permanente. Dati salvati in PlayerPrefs separati dal save
//  principale, sopravvivono al reset.
//
//  Prestige disponibile quando la Zona 5 (Vetta Nevosa, index 4)
//  è sbloccata. Ogni prestige moltiplica la produzione globale
//  per 1.5^n (n = numero prestige effettuati).
// ============================================================

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WoodlandBurrow
{
    public class PrestigeManager : MonoBehaviour
    {
        public static PrestigeManager Instance { get; private set; }

        private const string PrestigeCountKey  = "WB_PrestigeCount";
        private const int    PrestigeZoneIndex = 4;    // Zona 5 = index 4
        private const double PrestigeMultBase  = 1.5;  // ×1.5 cumulativo per prestige

        public int    PrestigeCount      { get; private set; }
        public double PrestigeMultiplier { get; private set; } = 1.0;

        public event Action OnPrestigeAvailable;

        public bool CanPrestige => ZoneManager.Instance != null &&
                                   ZoneManager.Instance.IsUnlocked(PrestigeZoneIndex);

        // --------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            PrestigeCount      = PlayerPrefs.GetInt(PrestigeCountKey, 0);
            PrestigeMultiplier = PrestigeCount == 0 ? 1.0 : Math.Pow(PrestigeMultBase, PrestigeCount);
        }

        private void OnEnable()
        {
            if (ZoneManager.Instance != null)
                ZoneManager.Instance.OnZoneUnlocked += CheckPrestigeAvailable;
        }

        private void OnDisable()
        {
            if (ZoneManager.Instance != null)
                ZoneManager.Instance.OnZoneUnlocked -= CheckPrestigeAvailable;
        }

        // --------------------------------------------------------

        private void CheckPrestigeAvailable(ZoneData zone)
        {
            if (zone != null && zone.zoneIndex == PrestigeZoneIndex)
                OnPrestigeAvailable?.Invoke();
        }

        // --------------------------------------------------------
        //  Reset completo + salva nuovo contatore prestige
        // --------------------------------------------------------

        public void DoPrestige()
        {
            if (!CanPrestige)
            {
                Debug.LogWarning("[PrestigeManager] Prestige non disponibile (Zona 5 non ancora sbloccata).");
                return;
            }

            PrestigeCount++;
            PrestigeMultiplier = Math.Pow(PrestigeMultBase, PrestigeCount);

            PlayerPrefs.SetInt(PrestigeCountKey, PrestigeCount);
            PlayerPrefs.DeleteKey("WoodlandBurrow_Save");
            PlayerPrefs.Save();

            AudioManager.Instance?.PlayPrestige();

            Debug.Log($"[PrestigeManager] Prestige #{PrestigeCount} completato! " +
                      $"Moltiplicatore globale: ×{PrestigeMultiplier:0.00}");

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
