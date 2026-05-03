// ============================================================
//  GuestAnimalData.cs — Woodland Burrow
//  ScriptableObject: definisce un animale ospite.
//  Crea istanze via: Assets > Create > WoodlandBurrow > GuestAnimal
// ============================================================

using UnityEngine;

namespace WoodlandBurrow
{
    [CreateAssetMenu(
        menuName = "WoodlandBurrow/GuestAnimal",
        fileName = "Animal_Talpa")]
    public class GuestAnimalData : ScriptableObject
    {
        [Header("Identità")]
        public string animalName = "Talpa Mina";
        public string description = "Scava nuove stanze più veloce.";
        public Sprite sprite;

        [Header("Zona di appartenenza")]
        public int zoneIndex = 0;

        [Header("Bonus")]
        public AnimalBonusType bonusType = AnimalBonusType.ProductionMultiplier;
        public double bonusValue = 1.25;   // es. ×1.25 produzione

        [Header("Sblocco")]
        public double unlockCostAcorns = 200;
    }

    public enum AnimalBonusType
    {
        ProductionMultiplier,   // Talpa: produzione ×N
        TapMultiplier,          // Cucciolo: tap ×N
        OfflineMultiplier,      // Gufo: guadagno offline ×N
        PeriodicBonus,          // Rana: porta risorse ogni X secondi
        EventTrigger            // Coccinella: attiva eventi bonus casuali
    }
}
