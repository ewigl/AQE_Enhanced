using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using AQE_Enhanced.UserInterface;

namespace AQE_Enhanced;

[BepInPlugin("aqe-enhanced", "AQE Enhanced", "0.0.1")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static float windowFix = 1f;
    public static WindowHandler wh;
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo(" AQE Enhanced.");

        Screen.fullScreen = false;
        if (!PlayerPrefs.HasKey("aqe_w") || PlayerPrefs.GetInt("aqe_w") == 0)
        {
            PlayerPrefs.SetInt("aqe_w", 1280);
        }
        if (!PlayerPrefs.HasKey("aqe_h") || PlayerPrefs.GetInt("aqe_h") == 0)
        {
            PlayerPrefs.SetInt("aqe_h", 720);
        }
        PlayerPrefs.SetInt("onFrameLimit", 1);
        Time.fixedDeltaTime = 0.016f;
        QualitySettings.vSyncCount = 1;
    }

    void Update()
    {
        var keyF7 = new BepInEx.Configuration.KeyboardShortcut(KeyCode.F7);
        if (keyF7.IsDown())
        {
            Logger.LogInfo("F7, SetWindowState(false)");
            wh.SetWindowState(false);
        }

        var keyF8 = new BepInEx.Configuration.KeyboardShortcut(KeyCode.F8);
        if (keyF8.IsDown())
        {
            Logger.LogInfo("F8, SetWindowState(true)");
            wh.SetWindowState(true);
        }

        if (windowFix > 0f)
        {
            windowFix -= Time.deltaTime;
            if (windowFix <= 0f)
            {
                wh ??= new WindowHandler();
                wh.SetWindowState(true);
            }
        }
    }
}
