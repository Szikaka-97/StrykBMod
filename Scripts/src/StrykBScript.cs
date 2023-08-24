using System.Reflection;
using UnityEngine;
using Receiver2;
using Receiver2ModdingKit;

public class StrykBScript : ModGunScript {
    private static readonly float slide_catch_striker_amount = 0.43608f;

    public Texture2D troll;
    public Renderer rds_renderer;

    public Transform trigger_bar_component;
    public Transform firing_pin_safety;
    public Transform ejector;

    public SkinnedMeshRenderer fat_spring;
    public SkinnedMeshRenderer medium_spring;
    public SkinnedMeshRenderer skinny_spring;

    private bool cocking_striker = true;

    public override void AwakeGun() {
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

    public void LateUpdate() {
        UpdateStriker();

        if (this.slide.amount == 0 && this._firing_pin.amount > 0 && this._firing_pin.amount < 0.5) {
            this.trigger.amount = Mathf.Max(this.trigger.amount, Mathf.InverseLerp(0.5f, 0f, this._firing_pin.amount) * 0.75f);
            this.trigger.UpdateDisplay();
        }

        ApplyTransform("disconnector_animation", this.slide.amount * Mathf.Clamp01(Mathf.InverseLerp(0.433f, 1, this.trigger.amount) + 0.45f), trigger_bar_component);

        if (this.round_in_chamber != null && this.malfunction != Malfunction.DoubleFeed) {
            ApplyTransform("extractor_animation", this.slide.amount, this.slide.transform.Find("extractor"));
        }
        else {
            ApplyTransform("extractor_animation", 0, this.slide.transform.Find("extractor"));
        }

        this.fat_spring.SetBlendShapeWeight(0, 100 - Mathf.InverseLerp(0, 0.2628f, slide.amount) * 100);
        this.medium_spring.SetBlendShapeWeight(0, 100 - Mathf.InverseLerp(0.2628f, 0.6596f, slide.amount) * 100);
        this.skinny_spring.SetBlendShapeWeight(0, 100 - Mathf.InverseLerp(0.6596f, 1, slide.amount) * 100);
    }

	public override void UpdateGun() {
        bool fired_on_this_frame = false;

		if (this.trigger.amount == 1 && !this._disconnector_needs_reset && this._firing_pin.amount == 0) {
            this._firing_pin.asleep = false;

            this._disconnector_needs_reset = true;
            this.cocking_striker = false;

            fired_on_this_frame = true;
        }
        else if (this.trigger.amount <= 0.8) this._disconnector_needs_reset = false;
        
        if (this.slide.amount == 0) {
            ApplyTransform("firing_pin_safety", this.trigger.amount, firing_pin_safety);
        }
        else {
            ApplyTransform("firing_pin_safety", 0, firing_pin_safety);
        }

        float old_vel = this._firing_pin.vel;
        this._firing_pin.TimeStep(Time.deltaTime * Time.timeScale);
        float new_vel = this._firing_pin.vel;

        if (this._firing_pin.amount == 1 && (old_vel > new_vel || fired_on_this_frame)) {
            this.TryFireBullet(1);
        }

        if (this.magazine_instance_in_gun != null && this.magazine_instance_in_gun.extracting) {
            float top_round_forward_amount = Vector3.Dot(this.magazine_instance_in_gun.rounds[0].transform.position, transform.forward);
            float magazine_forward_amount = Vector3.Dot(this.magazine_instance_in_gun.round_top.position, transform.forward);
            float barrel_forward_amount = Vector3.Dot(this.transform.Find("barrel/point_chambered_round").position, transform.forward);

            ApplyTransform("ejector_animation", Mathf.InverseLerp(magazine_forward_amount, barrel_forward_amount, top_round_forward_amount), this.ejector);
        }
        else {
            ApplyTransform("ejector_animation", 0, this.ejector);
        }
	}
}
/*
Firing pin positions:
-0.05905
-0.039


*/