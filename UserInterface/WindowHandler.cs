using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;


namespace AQE_Enhanced.UserInterface
{

	public class WindowHandler
	{
		private readonly string WINDOW_NAME;

		[DllImport("USER32.DLL")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
		private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

		[DllImport("user32.dll")]
		public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

		public WindowHandler()
		{
			WINDOW_NAME = "AQEv1.0";
		}

		public void SetWindow(int _width, int _height, int _desktopWidth, int _desktopHeight, bool fullScreen = true)
		{
			IntPtr intPtr = FindWindowByCaption(IntPtr.Zero, WINDOW_NAME);
			if (fullScreen)
			{
				SetWindowLong(intPtr, -16, 524288);
			}
			else
			{
				SetWindowLong(intPtr, -16, 13238272);
			}
			PlayerPrefs.SetInt("aqe_w", _width);
			PlayerPrefs.SetInt("aqe_h", _height);
			GameObject gameObject = GameObject.Find("Name_Window");
			if (gameObject != null)
			{
				Text component = gameObject.GetComponent<Text>();
				if (component != null)
				{
					component.text = string.Format("{0} x {1} {2}", _width, _height, fullScreen ? "F" : "W");
				}
			}
			else
			{
				Debug.LogError("Name_Window NOT found!");
			}
			Screen.SetResolution(_width, _height, false);
			SetWindowPos(intPtr, -2, (int)((_desktopWidth - _width) / 2f), (int)((_desktopHeight - _height) / 2f), _width, _height, 64);
		}

		public void SetWindowState(bool fullScreen)
		{
			float num = Screen.currentResolution.height * Mathf.Min(Screen.currentResolution.width / (float)Screen.currentResolution.height, 1.77777779f);
			float num2 = Screen.currentResolution.height;
			if (!fullScreen)
			{
				num *= 0.8f;
				num2 *= 0.8f;
			}
			SetWindow((int)num, (int)num2, Screen.currentResolution.width, Screen.currentResolution.height, fullScreen);
		}

	}
}
