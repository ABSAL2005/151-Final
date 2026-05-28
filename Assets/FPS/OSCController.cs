using extOSC;
using Unity.FPS.AI;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;

public class OSCController : MonoBehaviour
{
    public OSCTransmitter transmitter;
    private PlayerCharacterController playerController;
    private PlayerWeaponsManager weaponsManager;
    private PlayerInputHandler inputHandler;
    private float gunshotCooldown = 0f;
    private float footstepCooldown = 0f;
    private float wallCooldown = 0f;
    private EnemyController[] enemies;
    private DetectionModule[] detectionModules;
    private Health playerHealth;
    private bool bossMusicSent = false;

    void Start()
    {
        playerController = FindObjectOfType<PlayerCharacterController>();
        weaponsManager = FindObjectOfType<PlayerWeaponsManager>();
        inputHandler = FindObjectOfType<PlayerInputHandler>();

        EventManager.AddListener<PickupEvent>(OnPickup);
        EventManager.AddListener<EnemyKillEvent>(OnEnemyKill);
        EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);

        enemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in enemies)
        {
            enemy.onAttack += () => Send("/enemyshot", 1);
        }
        
        detectionModules = FindObjectsOfType<DetectionModule>();
        foreach (var dm in detectionModules)
        {
            dm.onDetectedTarget += () => Send("/enemydetect", 1);
            dm.onLostTarget += () => Send("/enemydetect", 0);
        }

        playerHealth = playerController.GetComponent<Health>();
        playerHealth.OnDamaged += OnPlayerDamaged;
        playerController.OnWallHit += OnWallHit;

        Invoke(nameof(SendMusic), 1f);  // wait 1s for Pd to be ready
    }

    void SendMusic()
    {
        Send("/music", 1);
    }

    void OnDestroy()
    {
        EventManager.RemoveListener<PickupEvent>(OnPickup);
        EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKill);
        EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        playerHealth.OnDamaged -= OnPlayerDamaged;
        playerController.OnWallHit -= OnWallHit;
    }

    void Update()
    {
        if (playerController.HasJumpedThisFrame)
        {
            Send("/jump", 1);
        }

        if (playerController.IsGrounded &&
            playerController.CharacterVelocity.magnitude > 0.1f)
        {
            if (footstepCooldown <= 0f)
            {
                Send("/footstep", 1);
                footstepCooldown = inputHandler.GetSprintInputHeld() ? 0.2f : 0.4f;
            }
        }
        footstepCooldown -= Time.deltaTime;

        WeaponController activeWeapon = weaponsManager.GetActiveWeapon();
        if (inputHandler.GetFireInputHeld())
        {
            if (activeWeapon != null && activeWeapon.CurrentAmmoRatio > 0f
                && !activeWeapon.IsCooling && gunshotCooldown <= 0f)
            {
                Send("/gunshot", 1);
                gunshotCooldown = 0.1f;
            }
        }
        gunshotCooldown -= Time.deltaTime;
        wallCooldown -= Time.deltaTime;

        if (activeWeapon != null && activeWeapon.IsCooling)
        {
            Send("/cooling", 1);
        }
    }

    void OnPickup(PickupEvent evt)
    {
        if (evt.Pickup.GetComponent<HealthPickup>() == null)
            Send("/pickup", 1);
    }
    void OnEnemyKill(EnemyKillEvent evt) => Send("/enemydestroy", 1);
    void OnPlayerDeath(PlayerDeathEvent evt) => Send("/playerdeath", 1);
    void OnPlayerDamaged(float damage, GameObject source) => Send("/damage", 1);
    void OnWallHit()
    {
        if (wallCooldown <= 0f)
        {
            Send("/wall", 1);
            wallCooldown = 0.3f;
        }
    }
    void OnApplicationQuit()
    {
        Send("/music", 0);
        Send("/bossmusic", 0);
    }

    public void SendBossMusic()
    {
        if (!bossMusicSent)
        {
            Send("/bossmusic", 1);
            bossMusicSent = true;
        }
    }

    private void Send(string address, int value)
    {
        OSCMessage msg = new OSCMessage(address);
        msg.AddValue(OSCValue.Int(value));
        transmitter.Send(msg);
    }
}