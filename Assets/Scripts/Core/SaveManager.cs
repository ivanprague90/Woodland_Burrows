// ============================================================
//  SaveManager.cs — Woodland Burrow
//  Salva e carica lo stato del gioco in JSON (PlayerPrefs).
//  Calcola anche il guadagno offline al ritorno del giocatore.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using WoodlandBurrow.UI;

namespace WoodlandBurrow
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SaveKey = "WoodlandBurrow_Save";

        [Header("Riferimenti")]
        [SerializeField] private Producer[] allProducers;  // trascina nell'Inspector

        // --------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationPause(bool pause) { if (pause) Save(); }
        private void OnApplicationQuit()            { Save(); }

        // --------------------------------------------------------
        //  SALVATAGGIO
        // --------------------------------------------------------

        public void Save()
        {
            var (acorns, sparks, mushrooms) = ResourceManager.Instance.GetState();

            var data = new SaveData
            {
                acorns         = acorns,
                sparks         = sparks,
                mushrooms      = mushrooms,
                lastSaveTime   = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                unlockedZones  = ZoneManager.Instance.GetUnlockedZones(),
                activeZone     = ZoneManager.Instance.GetActiveZoneIndex(),
                producerLevels = new int[allProducers.Length]
            };

            for (int i = 0; i < allProducers.Length; i++)
                data.producerLevels[i] = allProducers[i].GetLevel();

            string json = JsonUtility.ToJson(data, prettyPrint: false);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();

            Debug.Log($"[SaveManager] Salvato — Ghiande: {acorns:0}");
        }

        // --------------------------------------------------------
        //  CARICAMENTO + OFFLINE EARNINGS
        // --------------------------------------------------------

        public void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;

            string json = PlayerPrefs.GetString(SaveKey);
            var data    = JsonUtility.FromJson<SaveData>(json);

            // Risorse
            ResourceManager.Instance.SetState(data.acorns, data.sparks, data.mushrooms);

            // Zone
            ZoneManager.Instance.SetUnlockedZones(data.unlockedZones);
            ZoneManager.Instance.SetActiveZoneIndex(data.activeZone);

            // Producer
            for (int i = 0; i < allProducers.Length && i < data.producerLevels.Length; i++)
                allProducers[i].SetLevel(data.producerLevels[i]);

            // --- Guadagno offline ---
            long now       = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long secondsAway = now - data.lastSaveTime;

            if (secondsAway > 60)
            {
                float cappedSeconds = Mathf.Min(secondsAway,
                    GameConstants.MaxOfflineHours * 3600f);

                double offlineEarnings =
                    ResourceManager.Instance.TotalProductionPerSecond * cappedSeconds;

                if (offlineEarnings > 0)
                {
                    ResourceManager.Instance.AddAcorns(offlineEarnings);
                    NotifyOfflineEarnings(offlineEarnings, (long)cappedSeconds);
                }
            }
        }

        // --------------------------------------------------------
        //  Popup guadagno offline (collega la tua UI qui)
        // --------------------------------------------------------

        private void NotifyOfflineEarnings(double amount, long seconds)
        {
            string time   = FormatDuration(seconds);
            string earned = FormatNumber(amount);
            Debug.Log($"[SaveManager] Offline earnings: +{earned} 🌰 in {time}");
            HUDController.Instance?.ShowOfflineBadge(amount);
        }

        // --------------------------------------------------------
        //  Helpers
        // --------------------------------------------------------

        private static string FormatDuration(long seconds)
        {
            if (seconds < 3600) return $"{seconds / 60}m";
            return $"{seconds / 3600}h {(seconds % 3600) / 60}m";
        }

        private static string FormatNumber(double n)
        {
            if (n >= 1_000_000) return $"{n / 1_000_000:0.#}M";
            if (n >= 1_000)     return $"{n / 1_000:0.#}k";
            return $"{n:0}";
        }

        // --------------------------------------------------------
        //  Struttura dati di salvataggio
        // --------------------------------------------------------

        [Serializable]
        private class SaveData
        {
            public double acorns;
            public double sparks;
            public double mushrooms;
            public long   lastSaveTime;
            public int[]  unlockedZones;
            public int    activeZone;
            public int[]  producerLevels;
        }
    }
}
