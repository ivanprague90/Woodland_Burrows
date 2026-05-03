// ============================================================
//  NavBarController.cs — Woodland Burrow
//  Gestisce la navbar in basso (Tana / Mappa / Ospiti / Premi)
//  e attiva/disattiva i pannelli corrispondenti.
// ============================================================

using UnityEngine;
using UnityEngine.UI;

namespace WoodlandBurrow.UI
{
    public class NavBarController : MonoBehaviour
    {
        [Header("Pannelli da mostrare/nascondere")]
        [SerializeField] private GameObject panelTana;
        [SerializeField] private GameObject panelMappa;
        [SerializeField] private GameObject panelOspiti;
        [SerializeField] private GameObject panelPremi;

        [Header("Bottoni navbar")]
        [SerializeField] private Button btnTana;
        [SerializeField] private Button btnMappa;
        [SerializeField] private Button btnOspiti;
        [SerializeField] private Button btnPremi;

        [Header("Colori tab")]
        [SerializeField] private Color activeTabColor   = new Color(1f, 1f, 1f, 0.9f);
        [SerializeField] private Color inactiveTabColor = new Color(1f, 1f, 1f, 0.45f);

        private int _currentTab = 0;

        // --------------------------------------------------------

        private void Start()
        {
            btnTana?.onClick.AddListener(()   => SwitchTab(0));
            btnMappa?.onClick.AddListener(()  => SwitchTab(1));
            btnOspiti?.onClick.AddListener(() => SwitchTab(2));
            btnPremi?.onClick.AddListener(()  => SwitchTab(3));

            SwitchTab(0);
        }

        // --------------------------------------------------------

        public void SwitchTab(int index)
        {
            _currentTab = index;

            panelTana?.SetActive(index   == 0);
            panelMappa?.SetActive(index  == 1);
            panelOspiti?.SetActive(index == 2);
            panelPremi?.SetActive(index  == 3);

            RefreshTabColors();
        }

        private void RefreshTabColors()
        {
            SetTabColor(btnTana,   _currentTab == 0);
            SetTabColor(btnMappa,  _currentTab == 1);
            SetTabColor(btnOspiti, _currentTab == 2);
            SetTabColor(btnPremi,  _currentTab == 3);
        }

        private void SetTabColor(Button btn, bool active)
        {
            if (btn == null) return;
            var texts = btn.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach (var t in texts)
                t.color = active ? activeTabColor : inactiveTabColor;
        }
    }
}
