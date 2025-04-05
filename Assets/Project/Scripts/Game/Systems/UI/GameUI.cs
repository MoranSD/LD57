using System;
using UnityEngine;

namespace Game
{
    public class GameUI : MonoBehaviour
    {
        public ToolTip ToolTip;

        private void Awake()
        {
            G.UI = this;
        }

        public Vector3 GetMousePosition()
        {
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = 5f; //distance of the plane from the camera
            return Camera.main.ScreenToWorldPoint(screenPoint);
        }

        public void DisableUI()
        {
            ToolTip.DisableInfoShow();
        }

        public void EnableUI()
        {
            ToolTip.EnableInfoShow();
        }

        public void ShowLose()
        {
            Debug.Log("todo: lose screen");
        }
    }
}