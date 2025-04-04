using UnityEngine;

namespace Game
{
    public class HUD : MonoBehaviour
    {
        private void Awake()
        {
            G.HUD = this;
        }

        public void DisableHud()
        {

        }
    }
}