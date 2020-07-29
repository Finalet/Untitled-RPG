using System;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    [AwesomeTechnologiesScriptOrder(200)]
    public class ScreenshotUtility : MonoBehaviour
    {
        public void TakeScreenshot()
        {
            ScreenCapture.CaptureScreenshot("Screenshot_" + Guid.NewGuid() + ".png", 1);
        }

        void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                Debug.Log("screenshot");
                TakeScreenshot();
            }
        }
    }
}


