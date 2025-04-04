using UnityEngine;

namespace Game
{
    public class GameUI : MonoBehaviour
    {
        //public MapManager MapManager;

        private void Awake()
        {
            G.UI = this;
        }

        public void DisableUI()//hide all
        {
            
        }

        public void ShowLose()
        {

        }
    }
}