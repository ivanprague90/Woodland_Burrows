// ============================================================
//  ZoneData.cs — Woodland Burrow
//  ScriptableObject: definisce una zona del bosco.
//  Crea istanze via: Assets > Create > WoodlandBurrow > Zone
// ============================================================

using UnityEngine;

namespace WoodlandBurrow
{
    [CreateAssetMenu(
        menuName = "WoodlandBurrow/Zone",
        fileName = "Zone_00_Burrow")]
    public class ZoneData : ScriptableObject
    {
        [Header("Identità")]
        public string zoneName    = "La Tana di Spike";
        public string description = "Il punto di partenza di Spike nel Bosco Antico.";
        public Sprite background;
        public Sprite icon;
        public Color  accentColor = Color.green;

        [Header("Indice")]
        [Tooltip("0 = Tana (start), 1 = Prato, 2 = Ruscello, …")]
        public int zoneIndex = 0;

        [Header("Sblocco")]
        [Tooltip("Calcolato automaticamente da GameConstants se = 0")]
        public double overrideUnlockCost = 0;

        [Header("Producer disponibili in questa zona")]
        public ProducerData[] producers;

        [Header("Animali ospiti")]
        public GuestAnimalData[] guestAnimals;

        [Header("Meccanica speciale (Phase 3+)")]
        public ZoneMechanic specialMechanic = ZoneMechanic.None;

        // --------------------------------------------------------

        /// <summary>Costo reale di sblocco (usa override se > 0)</summary>
        public double UnlockCost => overrideUnlockCost > 0
            ? overrideUnlockCost
            : GameConstants.ZoneUnlockCost(zoneIndex);

        public bool IsStartZone => zoneIndex == 0;
    }

    public enum ZoneMechanic
    {
        None,
        Seasons,       // Prato: bonus cambiano con le stagioni
        FlowProducer,  // Ruscello: produzione a cascata
        Weather,       // Foresta: meteo casuale modifica risorse
        Prestige       // Vetta: reset con moltiplicatori permanenti
    }
}
