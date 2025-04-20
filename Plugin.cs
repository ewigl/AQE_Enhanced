using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using AQE_Enhanced.UserInterface;
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace AQE_Enhanced;

[BepInPlugin("aqe-enhanced", "AQE Enhanced", "0.0.2")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static Plugin Instance { get; private set; }
    public static WindowHandler wh;
    private bool hasInitialized = false;
    public int TargetFullscreenWidth { get; private set; }
    public int TargetFullscreenHeight { get; private set; }
    private int lastWidth = 0;
    private int lastHeight = 0;

    public struct Resolution
    {
        public int width;
        public int height;
    }

    private void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        Logger.LogInfo("AQE Enhanced initializing.");

        // 获取最高分辨率
        Resolution highestRes = GetHighestResolution();
        TargetFullscreenWidth = highestRes.width;
        TargetFullscreenHeight = highestRes.height;
        Logger.LogInfo($"Target fullscreen resolution: {TargetFullscreenWidth}x{TargetFullscreenHeight}");

        // 获取游戏的全屏设置
        bool isFullScreen = Screen.fullScreen;
        if (PlayerPrefs.HasKey("Screenmanager Is Fullscreen mode"))
        {
            isFullScreen = PlayerPrefs.GetInt("Screenmanager Is Fullscreen mode", isFullScreen ? 1 : 0) == 1;
        }
        Logger.LogInfo($"Game fullscreen setting: Screen.fullScreen={Screen.fullScreen}, PlayerPrefs={isFullScreen}");

        // 设置 PlayerPrefs 分辨率
        if (isFullScreen)
        {
            PlayerPrefs.SetInt("Screenmanager Resolution Width", TargetFullscreenWidth);
            PlayerPrefs.SetInt("Screenmanager Resolution Height", TargetFullscreenHeight);
            PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 1);
        }
        else
        {
            int windowedWidth = (int)(TargetFullscreenWidth * 0.8f);
            int windowedHeight = (int)(TargetFullscreenHeight * 0.8f);
            PlayerPrefs.SetInt("Screenmanager Resolution Width", windowedWidth);
            PlayerPrefs.SetInt("Screenmanager Resolution Height", windowedHeight);
            PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 0);
        }
        PlayerPrefs.Save();
        Logger.LogInfo($"PlayerPrefs set: Width={PlayerPrefs.GetInt("Screenmanager Resolution Width")}, Height={PlayerPrefs.GetInt("Screenmanager Resolution Height")}, Fullscreen={PlayerPrefs.GetInt("Screenmanager Is Fullscreen mode")}");

        // vSync
        PlayerPrefs.SetInt("onFrameLimit", 1);
        Time.fixedDeltaTime = 0.016f;
        QualitySettings.vSyncCount = 1;
    }

    private void Start()
    {
        bool isFullScreen = PlayerPrefs.GetInt("Screenmanager Is Fullscreen mode", Screen.fullScreen ? 1 : 0) == 1;
        StartCoroutine(InitializeWindow(isFullScreen));
    }

    private IEnumerator InitializeWindow(bool fullScreen)
    {
        Logger.LogInfo("Waiting for window initialization...");
        IntPtr intPtr = IntPtr.Zero;
        int attempts = 0;
        while (intPtr == IntPtr.Zero && attempts < 40)
        {
            yield return new WaitForSeconds(0.5f);
            intPtr = FindWindowByCaption(IntPtr.Zero, "AQEv1.0"); // 替换为实际标题
            attempts++;
            Logger.LogInfo($"Attempt {attempts}: Window handle={intPtr}");
        }

        if (intPtr == IntPtr.Zero)
        {
            Logger.LogError("Window not found after 20 seconds! Using Unity API only.");
            try
            {
                int width = fullScreen ? TargetFullscreenWidth : (int)(TargetFullscreenWidth * 0.8f);
                int height = fullScreen ? TargetFullscreenHeight : (int)(TargetFullscreenHeight * 0.8f);
                Screen.SetResolution(width, height, fullScreen);
                Screen.fullScreen = fullScreen;
                PlayerPrefs.SetInt("Screenmanager Resolution Width", width);
                PlayerPrefs.SetInt("Screenmanager Resolution Height", height);
                PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", fullScreen ? 1 : 0);
                PlayerPrefs.Save();
                Logger.LogInfo($"Fallback: fullScreen={Screen.fullScreen}, resolution={Screen.width}x{Screen.height}");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Fallback failed: {ex.Message}");
            }
            yield break;
        }

        try
        {
            wh = new WindowHandler();
            wh.SetWindowState(fullScreen);
            Screen.fullScreen = fullScreen;
            int width = fullScreen ? TargetFullscreenWidth : (int)(TargetFullscreenWidth * 0.8f);
            int height = fullScreen ? TargetFullscreenHeight : (int)(TargetFullscreenHeight * 0.8f);
            PlayerPrefs.SetInt("Screenmanager Resolution Width", width);
            PlayerPrefs.SetInt("Screenmanager Resolution Height", height);
            PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", fullScreen ? 1 : 0);
            PlayerPrefs.Save();
            Logger.LogInfo($"After InitializeWindow: fullScreen={Screen.fullScreen}, resolution={Screen.width}x{Screen.height}, PlayerPrefs=Width:{width},Height:{height}");
            hasInitialized = true;
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Failed to initialize window: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private IEnumerator ForceWindowedResolution(int width, int height)
    {
        yield return new WaitForSeconds(0.5f); // 延迟确保游戏设置完成
        Logger.LogInfo($"Forcing windowed resolution: {width}x{height}");
        try
        {
            wh.SetWindowState(false);
            Screen.fullScreen = false;
            PlayerPrefs.SetInt("Screenmanager Resolution Width", width);
            PlayerPrefs.SetInt("Screenmanager Resolution Height", height);
            PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 0);
            PlayerPrefs.Save();
            Logger.LogInfo($"Forced windowed resolution: {Screen.width}x{Screen.height}, expected={width}x{height}");
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Force windowed resolution failed: {ex.Message}");
        }
    }

    void Update()
    {
        if (!hasInitialized) return;

        // 检测分辨率变化
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            Logger.LogInfo($"Resolution changed: {lastWidth}x{lastHeight} -> {Screen.width}x{Screen.height}");
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }

        // 检查分辨率是否正确
        if (Screen.fullScreen && (Screen.width != TargetFullscreenWidth || Screen.height != TargetFullscreenHeight))
        {
            Logger.LogWarning($"Incorrect fullscreen resolution detected: {Screen.width}x{Screen.height}. Forcing {TargetFullscreenWidth}x{TargetFullscreenHeight}");
            try
            {
                wh.SetWindowState(true);
                Screen.fullScreen = true;
                PlayerPrefs.SetInt("Screenmanager Resolution Width", TargetFullscreenWidth);
                PlayerPrefs.SetInt("Screenmanager Resolution Height", TargetFullscreenHeight);
                PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 1);
                PlayerPrefs.Save();
                Logger.LogInfo($"Forced fullscreen: resolution={Screen.width}x{Screen.height}");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Force fullscreen failed: {ex.Message}");
            }
        }
        else if (!Screen.fullScreen)
        {
            int expectedWidth = (int)(TargetFullscreenWidth * 0.8f);
            int expectedHeight = (int)(TargetFullscreenHeight * 0.8f);
            if (Screen.width != expectedWidth || Screen.height != expectedHeight)
            {
                Logger.LogWarning($"Incorrect windowed resolution detected: {Screen.width}x{Screen.height}. Forcing {expectedWidth}x{expectedHeight}");
                StartCoroutine(ForceWindowedResolution(expectedWidth, expectedHeight));
            }
        }

        var keyF7 = new BepInEx.Configuration.KeyboardShortcut(KeyCode.F7);
        if (keyF7.IsDown())
        {
            Logger.LogInfo("F7 pressed, setting windowed mode");
            try
            {
                wh.SetWindowState(false);
                Screen.fullScreen = false;
                int width = (int)(TargetFullscreenWidth * 0.8f);
                int height = (int)(TargetFullscreenHeight * 0.8f);
                PlayerPrefs.SetInt("Screenmanager Resolution Width", width);
                PlayerPrefs.SetInt("Screenmanager Resolution Height", height);
                PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 0);
                PlayerPrefs.Save();
                Logger.LogInfo($"After F7: fullScreen={Screen.fullScreen}, resolution={Screen.width}x{Screen.height}, expected={width}x{height}");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"F7 failed: {ex.Message}");
            }
        }

        var keyF8 = new BepInEx.Configuration.KeyboardShortcut(KeyCode.F8);
        if (keyF8.IsDown())
        {
            Logger.LogInfo("F8 pressed, setting full screen");
            try
            {
                wh.SetWindowState(true);
                Screen.fullScreen = true;
                PlayerPrefs.SetInt("Screenmanager Resolution Width", TargetFullscreenWidth);
                PlayerPrefs.SetInt("Screenmanager Resolution Height", TargetFullscreenHeight);
                PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 1);
                PlayerPrefs.Save();
                Logger.LogInfo($"After F8: fullScreen={Screen.fullScreen}, resolution={Screen.width}x{Screen.height}, expected={TargetFullscreenWidth}x{TargetFullscreenHeight}");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"F8 failed: {ex.Message}");
            }
        }
    }

    [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
    private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    private const int SM_CXSCREEN = 0; // 主显示器宽度
    private const int SM_CYSCREEN = 1; // 主显示器高度

    private Resolution GetHighestResolution()
    {
        Resolution result = new Resolution();

        // 优先使用 Screen.resolutions
        var resolutions = Screen.resolutions;
        int maxWidth = 0;
        int maxHeight = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            Logger.LogInfo($"Available resolution {i}: {resolutions[i].width}x{resolutions[i].height}@{resolutions[i].refreshRate}Hz");
            if (resolutions[i].width > maxWidth || (resolutions[i].width == maxWidth && resolutions[i].height > maxHeight))
            {
                maxWidth = resolutions[i].width;
                maxHeight = resolutions[i].height;
            }
        }

        // 如果 Screen.resolutions 未提供有效分辨率，使用 Windows API
        if (maxWidth == 0 || maxHeight == 0)
        {
            maxWidth = GetSystemMetrics(SM_CXSCREEN);
            maxHeight = GetSystemMetrics(SM_CYSCREEN);
            Logger.LogInfo($"Screen.resolutions empty, using GetSystemMetrics: {maxWidth}x{maxHeight}");
        }

        // 确保分辨率有效
        if (maxWidth <= 0 || maxHeight <= 0)
        {
            maxWidth = 1920; // Fallback 默认值
            maxHeight = 1080;
            Logger.LogWarning($"Invalid resolution detected, using fallback: {maxWidth}x{maxHeight}");
        }

        result.width = maxWidth;
        result.height = maxHeight;
        return result;
    }
}