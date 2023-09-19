#if !UNITY_EDITOR

using BepInEx;

namespace StrykBMod {
	[BepInPlugin("pl.szikaka.strykbmod", "Stryk B Plugin", "1.0.0")]
	[BepInDependency("pl.szikaka.receiver_2_modding_kit", "1.4.0")]
	public class CorePlugin : BaseUnityPlugin {
		private void Awake() {
			Logger.LogInfo($"Stryk B Plugin loaded!");
		}
	}
}

#endif