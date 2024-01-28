using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Alive,
    Dead,
    Spectating
}

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Health))]
public class Player : NetworkBehaviour, IDamageable
{
    [Header("Input and Interaction")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float movementSpeed = 7;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float interactDistance = 4f;
    [SerializeField] private Color defaultCrosshairColor = Color.white;
    [SerializeField] private Color hoveringOnInteractableCrosshairColor = Color.green;

    [Header("More Config")]
    [SerializeField] private GameObject playerBody;
    [SerializeField] private Camera playerCamera;

    [Header("HUD")]
    [SerializeField] private PlayerHUD hudPrefab;

    [Header("Weapons")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private float timeBetweenFire = .3f;
    [SerializeField] private AudioClip shootSound;

    private bool blockInput;

    public PlayerState State
    {
        get
        {
            return playerState.Value;
        }

        set
        {
            playerState.Value = value;
        }
    }

    private NetworkVariable<PlayerState> playerState = new NetworkVariable<PlayerState>(PlayerState.Alive);

    private float timeToNextFire = 0.0f;
    private CharacterController characterController;
    private PlayerInputActions playerActions;
    private AudioSource playerAudioSource;
    private Health health;


    //private PlayerHUD playerHUD;

    private float xRot;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        characterController = GetComponent<CharacterController>();
        playerAudioSource = GetComponent<AudioSource>();
        health = GetComponent<Health>();

        playerState.Value = PlayerState.Alive;
        playerState.OnValueChanged += OnPlayerStateChanged;

        xRot = 0;

        //Setup input stuff
        if (IsOwner)
        {
            playerActions = new PlayerInputActions();
            playerActions.Enable();
            ConnectInputEvents();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        playerState.OnValueChanged -= OnPlayerStateChanged;
    }

    [ClientRpc]
    public void SetPositionAndRotationClientRpc(Vector3 pos, Quaternion rot)
    {
        characterController.enabled = false;
        transform.position = pos;
        transform.rotation = rot;
        characterController.enabled = true;
    }

    public void Kill()
    {
        if (IsServer)
        {
            playerState.Value = PlayerState.Dead;
        }
        else
        {
            DisplayClientSideDeathFX();
        }

        EventContainer.GamePlay.FireOnPlayerDied(OwnerClientId);
    }

    public void ApplyDamage(int amt, DamageReason damageReason, ulong instigator = 0)
    {
        bool isDead;
        health.AddDamage(amt, damageReason, out isDead);

        if (isDead)
        {
            Kill();
        }
    }

    //Should reet the player to a playable state, ready to go
    public void ResetLife()
    {
        SetRenderersActive(true);

        if (IsServer)
        {
            health.ResetHealth();
            playerState.Value = PlayerState.Alive;
        }
    }

    private void OnDisable()
    {
        if (!IsOwner)
            return;

        playerActions.Disable();
        DisconnectInputEvents();
    }

    private void ConnectInputEvents()
    {
        playerActions.PlayerMovement.Interact.performed += OnInteractPressed;
        playerActions.PlayerMovement.Fire.performed += OnFirePressed;
    }

    private void DisconnectInputEvents()
    {
        playerActions.PlayerMovement.Interact.performed -= OnInteractPressed;
        playerActions.PlayerMovement.Fire.performed -= OnFirePressed;
    }

    private void OnInteractPressed(InputAction.CallbackContext context) 
    {
        Debug.DrawLine(playerCamera.transform.position, playerCamera.transform.position + (playerCamera.transform.forward * interactDistance), Color.red, 3.0f);

        if (PerformInteractionRaycast(out RaycastHit hitData))
        {
            if(hitData.transform.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
                return;
            }

            Debug.Log("Object is not interactable: " + hitData.transform.name);
        }
    }

    private void Update()
    {
        playerCamera.enabled = IsOwner;
        playerCamera.GetComponent<AudioListener>().enabled = IsOwner;

        if (!IsOwner)
            return;

        HandleMovement();
        HandleLooking();
        UpdateState();
        HandleUI();
        TickTimers();
    }

    private void OnPlayerStateChanged(PlayerState previousState, PlayerState currentState)
    {
        //Handle state changes
        if (previousState != currentState)
        {
            //Became alive
            if (currentState == PlayerState.Alive)
            {
                blockInput = false;

                SetRenderersActive(true);
            }

            //Became dead
            if (currentState == PlayerState.Dead)
            {
                blockInput = true;

                SetRenderersActive(false);

                if (IsOwner)
                {
                    FindObjectOfType<GameplayUI>().SetLocalOnScreenMessageText("You died...", 3.0f);
                }
            }
        }
    }

    private void SetRenderersActive(bool active)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = active;
        }
    }

    private void UpdateState()
    {
        switch (playerState.Value)
        {
            case PlayerState.Alive:
                break;

            case PlayerState.Dead:
                break;

            case PlayerState.Spectating:
                break;
        }
    }

    private void HandleUI()
    {
        Color crosshairColor = IsLookingAtInteractable() ? hoveringOnInteractableCrosshairColor : defaultCrosshairColor;
        crosshairColor.a = 1f;
    }

    private void TickTimers()
    {
        timeToNextFire -= Time.deltaTime;
    }

    private bool PerformInteractionRaycast(out RaycastHit hit)
    {
        return Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactDistance, interactLayer);
    }

    private bool IsLookingAtInteractable()
    {
        if (PerformInteractionRaycast(out RaycastHit hitData))
        {
            if (hitData.transform.TryGetComponent(out IInteractable interactable))
            {
                return interactable.CanBeInteractedWith();
            }
        }

        return false;
    } 

    private void OnFirePressed(InputAction.CallbackContext context)
    {
        if (blockInput)
            return;

        if (timeToNextFire <= 0.0f)
        {
            ShootLaser();

            timeToNextFire = timeBetweenFire;
        }
    }

    private void ShootLaser()
    {
        Debug.Log("Firing laser");

        //If we are on the server, just send 

        if (IsServer)
        {
            CreateLaserClientRpc(playerCamera.transform.position, playerCamera.transform.forward, OwnerClientId);

            if (!IsHost)
            {
                Debug.LogWarning("Shooting lasers is host dependent. You need to change this code to use a dedicated server.");
            }
        }
        else
        {
            CreateLaserObject(playerCamera.transform.position, playerCamera.transform.forward, OwnerClientId);

            CreateLaserServerRpc(playerCamera.transform.position, playerCamera.transform.forward);
        }

        playerAudioSource.PlayOneShot(shootSound);
    }

    [ServerRpc]
    private void CreateLaserServerRpc(Vector3 spawnPoint, Vector3 forwardDirection, ServerRpcParams serverRpcParams = default)
    {
        CreateLaserObject(spawnPoint, forwardDirection, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void CreateLaserClientRpc(Vector3 spawnPoint, Vector3 forwardDirection, ulong shooterClientID)
    {
        CreateLaserObject(spawnPoint, forwardDirection, shooterClientID);
    }

    private LaserProjectile CreateLaserObject(Vector3 spawnPoint, Vector3 forwardDirection, ulong shooterID)
    {
        GameObject laserObject = Instantiate(laserPrefab, spawnPoint, Quaternion.identity);

        laserObject.transform.position = spawnPoint;
        laserObject.transform.forward = forwardDirection;

        LaserProjectile laser = laserObject.GetComponent<LaserProjectile>();
        laser.SetShooterID(shooterID);

        return laser;
    }

    private void HandleMovement()
    {
        if (blockInput)
            return;

        Vector2 movementInput = playerActions.PlayerMovement.Movement.ReadValue<Vector2>();

        Vector3 movement = new Vector3(movementInput.x, 0.0f, movementInput.y) * movementSpeed * Time.deltaTime;

        movement = transform.TransformVector(movement);

        characterController.Move(movement);
    }

    private void HandleLooking()
    {
        if (blockInput)
            return;

        Vector2 lookInput = playerActions.PlayerMovement.Look.ReadValue<Vector2>();

        Vector2 look = lookInput * sensitivity * 0.1f;
        xRot -= look.y;
        xRot = Mathf.Clamp(xRot, -90.0f, 90.0f);

        transform.Rotate(Vector3.up * look.x);

        playerCamera.transform.localRotation = Quaternion.Euler(xRot, 0.0f, 0.0f);
    }

    private void DisplayClientSideDeathFX()
    {

    }
}
