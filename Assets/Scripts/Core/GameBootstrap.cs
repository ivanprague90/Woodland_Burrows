using UnityEngine;
namespace WoodlandBurrow
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Start()
        {
            SaveManager.Instance.Load();
        }
    }
}
