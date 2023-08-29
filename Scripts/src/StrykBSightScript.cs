using UnityEngine;
using SimpleJSON;
using Receiver2;

namespace StrykBMod {
    public class StrykBSightScript : InventoryItem {
        public MeshRenderer rds_renderer;

        protected override void OnCollisionEnter(Collision collision) {
            if (!this.physics_collided) {
                base.OnCollisionEnter(collision);
                this.physics_collided = true;
                AudioManager.PlayOneShotAttached(AudioManager.Instance().sound_event_gun_fall, this.gameObject);
            }
        }

        public override JSONObject GetPersistentData() { return new JSONObject(); }

        public override void SetPersistentData(JSONObject persistent_data) { }
    }
}