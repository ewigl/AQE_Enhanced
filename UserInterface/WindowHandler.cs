using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace AQE_Enhanced.UserInterface
{
	public class WindowHandler
	{
		private readonly string WINDOW_NAME = "AQEv1.0"; // 替换为实际窗口标题

		[DllImport("USER32.DLL")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
		private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		public WindowHandler()
		{
			IntPtr intPtr = FindWindowByCaption(IntPtr.Zero, WINDOW_NAME);
			Debug.Log($"WindowHandler initialized: WINDOW_NAME={WINDOW_NAME}, Handle={intPtr}");
		}

		public void SetWindow(int _width, int _height, int _desktopWidth, int _desktopHeight, bool fullScreen = true)
		{
			IntPtr intPtr = FindWindowByCaption(IntPtr.Zero, WINDOW_NAME);
			if (intPtr == IntPtr.Zero)
			{
				Debug.LogError($"Window '{WINDOW_NAME}' not found! Cannot set window state.");
				return;
			}

			Debug.Log($"SetWindow: fullScreen={fullScreen}, width={_width}, height={_height}, desktop={_desktopWidth}x{_desktopHeight}");

			try
			{
				if (fullScreen)
				{
					SetWindowLong(intPtr, -16, 524288); // WS_POPUP (无边框全屏)
				}
				else
				{
					SetWindowLong(intPtr, -16, 13238272); // WS_OVERLAPPEDWINDOW | WS_VISIBLE (窗口化)
				}

				// 更新 UI
				try
				{
					GameObject gameObject = GameObject.Find("Name_Window");
					if (gameObject != null)
					{
						Text component = gameObject.GetComponent<Text>();
						if (component != null)
						{
							component.text = $"{_width} x {_height} {(fullScreen ? "F" : "W")}";
						}
						else
						{
							Debug.LogWarning("Name_Window found but missing Text component!");
						}
					}
					else
					{
						Debug.LogWarning("Name_Window GameObject not found!");
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogError($"UI update failed: {ex.Message}");
				}

				// 设置分辨率
				Screen.SetResolution(_width, _height, fullScreen);
				Debug.Log($"After SetResolution: fullScreen={Screen.fullScreen}, resolution={Screen.width}x{Screen.height}, requested={_width}x{_height}");

				// 设置窗口位置
				if (fullScreen)
				{
					SetWindowPos(intPtr, -2, 0, 0, _width, _height, 64); // 全屏时定位到 (0,0)
				}
				else
				{
					SetWindowPos(intPtr, -2, (_desktopWidth - _width) / 2, (_desktopHeight - _height) / 2, _width, _height, 64); // 窗口化时居中
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"SetWindow failed: {ex.Message}\n{ex.StackTrace}");
			}
		}

		public void SetWindowState(bool fullScreen)
		{
			// 全屏使用 Plugin 提供的最高分辨率
			int width = Screen.currentResolution.width;
			int height = Screen.currentResolution.height;

			// 如果 Screen.currentResolution 不正确，使用 Plugin 的 targetFullscreenWidth/Height
			if (width != Plugin.Instance.TargetFullscreenWidth || height != Plugin.Instance.TargetFullscreenHeight)
			{
				width = Plugin.Instance.TargetFullscreenWidth;
				height = Plugin.Instance.TargetFullscreenHeight;
				Debug.Log($"Screen.currentResolution ({Screen.currentResolution.width}x{Screen.currentResolution.height}) incorrect, using target: {width}x{height}");
			}

			if (!fullScreen)
			{
				// 窗口化使用 80% 分辨率
				width = (int)(width * 0.8f);
				height = (int)(height * 0.8f);
			}

			Debug.Log($"SetWindowState: fullScreen={fullScreen}, targetResolution={width}x{height}");
			SetWindow(width, height, Plugin.Instance.TargetFullscreenWidth, Plugin.Instance.TargetFullscreenHeight, fullScreen);
		}
	}
}