using extOSC;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

public class OSCController : MonoBehaviour
{
    public OSCTransmitter transmitter;
    private PlayerCharacterController playerController;
    private PlayerWeaponsManager weaponsManager;
    private PlayerInputHandler inputHandler;

    void Start()
    {
        playerController = FindObjectOfType<PlayerCharacterController>();
        weaponsManager = FindObjectOfType<PlayerWeaponsManager>();
        inputHandler = FindObjectOfType<PlayerInputHandler>();
    }

    void Update()
    {
        if (playerController.HasJumpedThisFrame)
        {
            Send("/jump", 1);
        }

        WeaponController activeWeapon = weaponsManager.GetActiveWeapon();

        if (inputHandler.GetFireInputHeld())
        {
            if (activeWeapon != null && activeWeapon.CurrentAmmoRatio > 0f && !activeWeapon.IsCooling)
            {
                Send("/shoot", 1);
            }
        }

        if (activeWeapon != null && activeWeapon.IsCooling)
        {
            Send("/cooling", 1);
        }
    }

    private void Send(string address, int value)
    {
        OSCMessage msg = new OSCMessage(address);
        msg.AddValue(OSCValue.Int(value));
        transmitter.Send(msg);
    }
}  