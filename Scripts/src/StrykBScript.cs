using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Receiver2ModdingKit;
using ImGuiNET;

public class StrykBScript : ModGunScript {
	public static bool loaded = true;
    
    public override void AwakeGun() {
        
    }

	public override void UpdateGun() {
        ImGui.SetNextWindowSize(new Vector2(360f, 500f), ImGuiCond.Always);

		if (this.trigger.amount == 1) {
            Debug.Log("Bum");
            this._disconnector_needs_reset = true;
        }
	}
}
/*
Firing pin positions:
-0.05905
-0.039


*/