using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Receiver2ModdingKit;

public class StrykBScript : ModGunScript {
	public static bool loaded = true;
    
    public Transform trigger_transform;

    public override void AwakeGun() {
        this.trigger.transform = this.trigger_transform;
    }

	public override void UpdateGun() {
		
	}
}
