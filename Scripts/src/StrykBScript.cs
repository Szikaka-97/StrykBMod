using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using Receiver2;
using Receiver2ModdingKit;

namespace StrykBMod {
	public class StrykBScript : ModGunScript {
		private static readonly float slide_catch_striker_amount = 0.43608f;

		public Transform trigger_bar_component;
		public Transform firing_pin_safety;
		public Transform ejector;

		public SkinnedMeshRenderer fat_spring;
		public SkinnedMeshRenderer medium_spring;
		public SkinnedMeshRenderer skinny_spring;

		public InventoryItem rds_prefab;

		private bool cocking_striker = true;

		private StrykBSightScript rds_instance;

		private bool installing_rds = false;

		private bool rds_installed {
			get {
				return rds_instance != null;
			}
		}

		private List<ActiveItem> rds_items = new List<ActiveItem>();

		private bool SpawnHandler(ref ActiveItem item) {

			if (Random.value <= 0.25 && LocalAimHandler.player_instance != null && LocalAimHandler.player_instance.TryGetGun(out var gun) && !(gun as StrykBScript).rds_installed) {
				item.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

				item.SetItemSpawn(rds_prefab.InternalName);

				(gun as StrykBScript).rds_items.Add(item);

				return true;
			}
			return false;
		}

		public override void InitializeGun() {
			ModdingKitEvents.RegisterItemSpawnHandler(this.InternalName, SpawnHandler);

			ReceiverCoreScript.Instance().AddItemsToSpawnList(rds_prefab);
		}

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

		private IEnumerator AnimateSightPlacement(InventoryItem red_dot) {
			installing_rds = true;

			while (red_dot.transform.localPosition != new Vector3(0, 0.01f) || red_dot.transform.localRotation != Quaternion.identity) {
				red_dot.transform.localPosition = Vector3.MoveTowards(red_dot.transform.localPosition, new Vector3(0, 0.01f), 0.01f);
				red_dot.transform.localRotation = Quaternion.RotateTowards(red_dot.transform.localRotation, Quaternion.identity, 5);
				yield return null;
			}

			while (red_dot.transform.localPosition != Vector3.zero) {
				red_dot.transform.localPosition = Vector3.MoveTowards(red_dot.transform.localPosition, Vector3.zero, 0.003f);
				yield return null;
			}

			installing_rds = false;
		}

		private void AttachRedDotSight(StrykBSightScript red_dot, bool animate) {
			foreach (ActiveItem active_item in this.rds_items) {
				if (active_item.loaded) {
					active_item.item.GetComponent<InventoryItem>().Move(null);

					DestroyImmediate(active_item.item);

					active_item.item_type = Type.Magazine;

					active_item.loaded = false;

					active_item.rotation = Quaternion.Euler(0, 0, 90);
					active_item.position += Vector3.up * 0.01f;
				}
				else {
					active_item.item_type = Type.Magazine;
				}
			}

			AudioManager.PlayOneShotAttached("event:/strykb_attach_sight", this.slide.transform.Find("rds_position").gameObject);

			LocalAimHandler.player_instance.MoveInventoryItem(red_dot, this.GetComponent<InventorySlot>());

			red_dot.transform.parent = this.slide.transform.Find("rds_position");

			if (animate) {
				StartCoroutine(AnimateSightPlacement(red_dot));
			}
			else {
				red_dot.transform.localPosition = Vector3.zero;
				red_dot.transform.localRotation = Quaternion.identity;
			}

			red_dot.rds_renderer.gameObject.SetActive(true);

			this.transform.Find("pose_aim_down_sights").localPosition = this.transform.Find("pose_aim_down_sights_rds").localPosition;

			this.rds_instance = red_dot;
		}

		public void LateUpdate() {
			if (installing_rds) {
				this.slide.amount = 0;
			}

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

			if (
				LocalAimHandler.player_instance != null
				&&
				!rds_installed
				&&
				this.slot != null
				&&
				this.slot.type == InventorySlot.Type.RightHand
				&&
				LocalAimHandler.player_instance.TryGetItemInHandWithState(LocalAimHandler.Hand.State.HoldingGenericItem, out InventoryItem item)
				&&
				item is StrykBSightScript
				&&
				this.slide.amount == 0
				&&
				player_input.GetButtonDown(RewiredConsts.Action.Hammer)
			) {
				AttachRedDotSight((StrykBSightScript) item, true);
			}
		}

		public override void SetPersistentData(JSONObject data) {
			base.SetPersistentData(data);

			if (data.HasKey("rds_installed") && data["rds_installed"].AsBool) {
				this.AttachRedDotSight(Instantiate(this.rds_prefab.GetComponent<StrykBSightScript>()), false);
			}
		}

		public override JSONObject GetPersistentData() {
			JSONObject json = new JSONObject();

			json["rds_installed"] = this.rds_installed;

			return json;
		}
	}
}