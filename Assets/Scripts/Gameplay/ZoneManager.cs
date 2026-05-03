// ============================================================
//  ZoneManager.cs — Woodland Burrow
//  Gestisce tutte le zone del bosco: sblocco, attivazione,
//  e notifica alla UI quando una zona diventa disponibile.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace WoodlandBurrow
{
    public class ZoneManager : MonoBehaviour
    {
        public static ZoneManager Instance { get; private set; }

        [Header("Tutte le zone (ordinate per indice)")]
        public ZoneData[] allZones;

        // Stato runtime
        private HashSet<int> _unlockedZones = new HashSet<int>();
        private int          _activeZoneIndex = 0;

        // Eventi
        public event Action<ZoneData> OnZoneUnlocked;
        public event Action<ZoneData> OnActiveZoneChanged;

        // --------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // La zona 0 (Tana) è sempre sbloccata
            _unlockedZones.Add(0);
        }

        // --------------------------------------------------------
        //  Ogni tot secondi verifica se nuove zone si sbloccano
        //  (puoi chiamarlo anche dall'UI con un bottone esplicito)
        // --------------------------------------------------------

        public void CheckZoneUnlocks()
        {
            foreach (var zone in allZones)
            {
                if (_unlockedZones.Contains(zone.zoneIndex)) continue;
                if (!PreviousZoneUnlocked(zone.zoneIndex))   continue;
                if (!ResourceManager.Instance.CanAfford(zone.UnlockCost)) continue;

                // Tutto soddisfatto: sblocca la zona
                if (ResourceManager.Instance.SpendAcorns(zone.UnlockCost))
                {
                    _unlockedZones.Add(zone.zoneIndex);
                    OnZoneUnlocked?.Invoke(zone);
                    Debug.Log($"[ZoneManager] Zona sbloccata: {zone.zoneName}");
                }
            }
        }

        public void SetActiveZone(int index)
        {
            if (!_unlockedZones.Contains(index))
            {
                Debug.LogWarning($"[ZoneManager] Zona {index} non ancora sbloccata.");
                return;
            }
            _activeZoneIndex = index;
            OnActiveZoneChanged?.Invoke(allZones[index]);
        }

        public ZoneData GetActiveZone()  => allZones[_activeZoneIndex];
        public bool IsUnlocked(int idx)  => _unlockedZones.Contains(idx);

        // --------------------------------------------------------
        //  Condizione: la zona precedente deve essere sbloccata
        // --------------------------------------------------------

        private bool PreviousZoneUnlocked(int zoneIndex)
            => zoneIndex == 0 || _unlockedZones.Contains(zoneIndex - 1);

        // --------------------------------------------------------
        //  Serializzazione
        // --------------------------------------------------------

        public int[]  GetUnlockedZones()              => new List<int>(_unlockedZones).ToArray();
        public void   SetUnlockedZones(int[] zones)   { _unlockedZones = new HashSet<int>(zones); }
        public int    GetActiveZoneIndex()             => _activeZoneIndex;
        public void   SetActiveZoneIndex(int index)   => _activeZoneIndex = index;
    }
}
