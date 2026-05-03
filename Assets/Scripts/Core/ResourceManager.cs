// ============================================================
//  ResourceManager.cs — Woodland Burrow
//  Singleton che gestisce tutte le valute.
//  Usa eventi C# per notificare la UI senza accoppiamento.
// ============================================================

using System;
using UnityEngine;

namespace WoodlandBurrow
{
    public class ResourceManager : MonoBehaviour
    {
        // Singleton
        public static ResourceManager Instance { get; private set; }

        // --- Valute ---
        private double _acorns;        // 🌰 valuta principale
        private double _sparks;        // ✨ valuta premium
        private double _mushrooms;     // 🍄 craft currency

        // --- Produzione totale (calcolata da tutti i Producer attivi) ---
        public double TotalProductionPerSecond { get; private set; }

        // --- Eventi per la UI ---
        public event Action<double> OnAcornsChanged;
        public event Action<double> OnSparksChanged;
        public event Action<double> OnMushroomsChanged;
        public event Action<double> OnProductionChanged;

        // --- Proprietà pubbliche (sola lettura) ---
        public double Acorns    => _acorns;
        public double Sparks    => _sparks;
        public double Mushrooms => _mushrooms;

        // --------------------------------------------------------

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SaveManager.Instance?.Load();
        }

        // --------------------------------------------------------
        //  Aggiunta valute
        // --------------------------------------------------------

        public void AddAcorns(double amount)
        {
            _acorns += amount;
            OnAcornsChanged?.Invoke(_acorns);
        }

        public void AddSparks(double amount)
        {
            _sparks += amount;
            OnSparksChanged?.Invoke(_sparks);
        }

        public void AddMushrooms(double amount)
        {
            _mushrooms += amount;
            OnMushroomsChanged?.Invoke(_mushrooms);
        }

        // --------------------------------------------------------
        //  Spesa valute (ritorna false se non abbastanza)
        // --------------------------------------------------------

        public bool SpendAcorns(double amount)
        {
            if (_acorns < amount) return false;
            _acorns -= amount;
            OnAcornsChanged?.Invoke(_acorns);
            return true;
        }

        public bool SpendSparks(double amount)
        {
            if (_sparks < amount) return false;
            _sparks -= amount;
            OnSparksChanged?.Invoke(_sparks);
            return true;
        }

        public bool CanAfford(double acornCost) => _acorns >= acornCost;

        // --------------------------------------------------------
        //  Produzione idle — chiamato dai Producer ogni frame
        // --------------------------------------------------------

        public void RegisterProduction(double acornsPerSecond)
        {
            TotalProductionPerSecond += acornsPerSecond;
            OnProductionChanged?.Invoke(TotalProductionPerSecond);
        }

        public void UnregisterProduction(double acornsPerSecond)
        {
            TotalProductionPerSecond = Math.Max(0, TotalProductionPerSecond - acornsPerSecond);
            OnProductionChanged?.Invoke(TotalProductionPerSecond);
        }

        // --------------------------------------------------------
        //  Serializzazione (usata da SaveManager)
        // --------------------------------------------------------

        public void SetState(double acorns, double sparks, double mushrooms)
        {
            _acorns    = acorns;
            _sparks    = sparks;
            _mushrooms = mushrooms;
            OnAcornsChanged?.Invoke(_acorns);
            OnSparksChanged?.Invoke(_sparks);
            OnMushroomsChanged?.Invoke(_mushrooms);
        }

        public (double acorns, double sparks, double mushrooms) GetState()
            => (_acorns, _sparks, _mushrooms);
    }
}
