using Common;
using UnityEngine;

namespace Game
{
    public static class G
    {
        public static AudioSystem Audio;
        public static CameraHandle Camera;
        public static GameUI UI;
        public static HUD HUD;
        public static Main Main;
        public static GameState State;
    }
    public class GameState
    {

    }
    public class Main : MonoBehaviour
    {
        public Interactor Interactor;
        private void Start()
        {
            Interactor = new Interactor();
            Interactor.Init();
            CMS.Init();
            G.Main = this;
            G.State = new GameState();
        }
        private void OnDestroy()
        {
        }
        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.W))
            {

            }
#endif
        }
    }
}