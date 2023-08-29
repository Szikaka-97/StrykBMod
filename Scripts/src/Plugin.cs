#if !UNITY_EDITOR

using BepInEx;

namespace StrykBMod {
	[BepInPlugin("pl.szikaka.strykbmod", "Stryk B Plugin", "0.1.0")]
	[BepInDependency("pl.szikaka.receiver_2_modding_kit")]
	public class CorePlugin : BaseUnityPlugin {
		private void Awake() {
			Logger.LogInfo($"Stryk B Plugin loaded!");
		}
	}
}

#endif