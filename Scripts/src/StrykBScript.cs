using System.Reflection;
using UnityEngine;
using Receiver2;
using Receiver2ModdingKit;

public class StrykBScript : ModGunScript {
    private static readonly float slide_catch_striker_amount = 0.43608f;

    public Texture2D troll;
    public Renderer rds_renderer;

    private static FieldInfo m_firing_pin;

    private bool cocking_striker = true;

    private LinearMover _firing_pin {
        get { return (LinearMover) m_firing_pin.GetValue(this); }
    }

    public override void AwakeGun() {
        m_firing_pin = typeof(GunScript).GetField("firing_pin", BindingFlags.Instance | BindingFlags.NonPublic);

        this._firing_pin.accel = 8000;
        this._firing_pin.target_amount = 1;
    }

    private void UpdateStriker() {
        if (this.slide.amount < slide_catch_striker_amount) {
            if (this.cocking_striker) {
                float pin_amount = 1 - ((slide_catch_striker_amount - this.slide.amount) / slide_catch_striker_amount);

                this._firing_pin.amount = pin_amount;
            }
        }
        else {
            this.cocking_striker = true;
        }

        this._firing_pin.UpdateDisplay();
    }

    private void LateUpdate() {
        UpdateStriker();
    }

	public override void UpdateGun() {
        bool fired_on_this_frame = false;

		if (this.trigger.amount == 1 && !this._disconnector_needs_reset && this._firing_pin.amount == 0) {
            this._firing_pin.asleep = false;

            this._disconnector_needs_reset = true;
            this.cocking_striker = false;

            fired_on_this_frame = true;
        }
        else if (this.trigger.amount == 0) this._disconnector_needs_reset = false;
        
        float old_vel = this._firing_pin.vel;
        this._firing_pin.TimeStep(Time.deltaTime * Time.timeScale);
        float new_vel = this._firing_pin.vel;

        if (this._firing_pin.amount == 1 && (old_vel > new_vel || fired_on_this_frame)) {
            this.TryFireBullet(1);

            Debug.Log("Bang");
        }
	}
}
/*
Firing pin positions:
-0.05905
-0.039


*/