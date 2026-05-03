// ============================================================
//  GameConstants.cs — Woodland Burrow
//  Tutti i valori di bilanciamento centralizzati.
//  Modifica qui per ribilanciare il gioco senza toccare
//  la logica degli altri script.
// ============================================================

namespace WoodlandBurrow
{
    public static class GameConstants
    {
        // --- TAP ---
        public const double AcornsPerTap = 6.0;

        // --- PRODUZIONE BASE ---
        public const double BaseProductionPerSecond = 8.0;

        // --- UPGRADE ---
        public const double UpgradeMultiplier   = 1.8;   // produzione ×1.8 per livello
        public const double UpgradeBaseCost     = 50.0;  // costo upgrade livello 1
        public const double UpgradeCostExponent = 1.6;   // crescita esponenziale del costo

        // --- ZONE ---
        public const double Zone2UnlockCost   = 500.0;
        public const double ZoneCostMultiplier = 4.0;   // ogni zona costa ×4 la precedente

        // --- OFFLINE EARNINGS ---
        public const float MaxOfflineHours = 8f;        // cap guadagno offline

        // --- PRESTIGE (Phase 4) ---
        public const double PrestigeRequiredAcorns = 1_000_000.0;

        // Helper: costo upgrade al livello n (0-indexed)
        public static double UpgradeCostAtLevel(int level)
            => UpgradeBaseCost * System.Math.Pow(UpgradeCostExponent, level);

        // Helper: produzione al livello n (0-indexed)
        public static double ProductionAtLevel(double baseProduction, int level)
            => baseProduction * System.Math.Pow(UpgradeMultiplier, level);

        // Helper: costo sblocco zona (0 = Tana, 1 = Prato, ...)
        public static double ZoneUnlockCost(int zoneIndex)
        {
            if (zoneIndex <= 0) return 0;
            return Zone2UnlockCost * System.Math.Pow(ZoneCostMultiplier, zoneIndex - 1);
        }
    }
}
