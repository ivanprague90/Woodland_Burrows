// ============================================================
//  ProducerData.cs — Woodland Burrow
//  ScriptableObject: definisce una stanza della tana.
//  Crea istanze via: Assets > Create > WoodlandBurrow > Producer
//  Ogni stanza è un file .asset separato — aggiungi stanze
//  senza toccare il codice.
// ============================================================

using UnityEngine;

namespace WoodlandBurrow
{
    [CreateAssetMenu(
        menuName = "WoodlandBurrow/Producer",
        fileName = "Producer_Dispensa")]
    public class ProducerData : ScriptableObject
    {
        [Header("Identità")]
        public string producerName = "Dispensa";
        public string description  = "Raccoglie ghiande automaticamente.";
        public Sprite icon;

        [Header("Zona")]
        [Tooltip("0 = Tana, 1 = Prato, 2 = Ruscello, …")]
        public int zoneIndex = 0;

        [Header("Economia")]
        [Tooltip("Produzione base al livello 1 (ghiande/sec)")]
        public double baseProductionPerSecond = 8.0;   // valore da GameConstants

        [Tooltip("Costo per comprare il primo livello")]
        public double baseCost = 50.0;

        [Tooltip("Moltiplicatore produzione per ogni livello")]
        public double productionMultiplier = 1.8;      // GameConstants.UpgradeMultiplier

        [Tooltip("Esponente crescita costo")]
        public double costExponent = 1.6;              // GameConstants.UpgradeCostExponent

        [Header("Speciale")]
        [Tooltip("Questa stanza genera valuta secondaria invece di ghiande?")]
        public bool producesSecondaryCurrency = false;
        public SecondaryCurrency secondaryCurrency = SecondaryCurrency.None;

        // --------------------------------------------------------
        //  Helpers calcolati (usati da Producer.cs)
        // --------------------------------------------------------

        public double ProductionAtLevel(int level)
            => baseProductionPerSecond * System.Math.Pow(productionMultiplier, level);

        public double CostAtLevel(int level)
            => baseCost * System.Math.Pow(costExponent, level);
    }

    public enum SecondaryCurrency { None, Mushrooms, Sparks }
}
