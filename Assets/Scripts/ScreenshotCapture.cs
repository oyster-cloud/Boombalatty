using System;
using System.IO;
using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
  void Start()
  {
    Debug.Log("ScreenshotCapture is active. Press P to save screenshot.");
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.P))
    {
      string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
      string fileName = $"Boombalatty_{Screen.width}x{Screen.height}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
      string fullPath = Path.Combine(desktopPath, fileName);

      ScreenCapture.CaptureScreenshot(fullPath);

      Debug.Log($"Screenshot saved to: {fullPath}");
    }
  }
}