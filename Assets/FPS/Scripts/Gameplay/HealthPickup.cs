using extOSC;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class HealthPickup : Pickup
    {
        [Header("Parameters")] [Tooltip("Amount of health to heal on pickup")]
        public float HealAmount;

        protected override void OnPicked(PlayerCharacterController player)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth && playerHealth.CanPickup())
            {
                playerHealth.Heal(HealAmount);
                PlayPickupFeedback();
                OSCTransmitter transmitter = FindObjectOfType<OSCTransmitter>();
                if (transmitter != null)
                {
                    OSCMessage msg = new OSCMessage("/health");
                    msg.AddValue(OSCValue.Int(1));
                    transmitter.Send(msg);
                }
                Destroy(gameObject);
            }
        }
    }
}