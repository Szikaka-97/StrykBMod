#if !UNITY_EDITOR

using BepInEx;

namespace StrykBMod {
    [BepInPlugin("pl.szikaka.strykbmod", "Stryk B Plugin", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Stryk B Plugin loaded!");
        }
    }
}

#endif