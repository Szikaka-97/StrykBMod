using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Receiver2ModdingKit;

public class StrykBScript : ModGunScript {
	public static bool loaded = true;
    
    public override void AwakeGun() {
        
    }

	public override void UpdateGun() {
		if (this.trigger.amount == 1) {
            Debug.Log("Bum");
            this._disconnector_needs_reset = true;
        }
	}
}
