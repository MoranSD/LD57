using DG.Tweening;
using UnityEngine;

namespace Game
{
    public class CameraHandle : MonoBehaviour
    {
        public Camera MainCamera;

        private void Awake()
        {
            G.Camera = this;
            MainCamera = Camera.main;
        }

        public void Shake(float i, float t)
        {
            Camera.main.DOShakePosition(t, i, 10, 45f);
        }

        public void UIHit()
        {
            Shake(0.025f, 0.4f);
        }
    }
}