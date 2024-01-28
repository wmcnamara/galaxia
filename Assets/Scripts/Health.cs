using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public enum DamageReason
{
    None,
    Laser,
    Fall,
}

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxAndDefaultHealth = 100;

    private NetworkVariable<int> health = new NetworkVariable<int>(0);

    public int RemainingHealth
    {
        get { return health.Value; }

        private set 
        {
            health.Value = Mathf.Clamp(value, 0, maxAndDefaultHealth); 
        }
    }

    public bool IsDead
    {
        get { return health.Value <= 0; }
    }

    public void AddDamage(int amt, DamageReason reason, out bool doesThisDamageKill)
    {
        Debug.Log("Damage added to: " + name + " for reason: " + reason.ToString());

        if (IsServer)
        {
            RemainingHealth -= amt;

            doesThisDamageKill = IsDead;
        }
        else
        {
            doesThisDamageKill = health.Value - amt < 0;
        }
    }

    public void AddHealth(int amt)
    {
        Debug.Log("Health added to: " + name);

        RemainingHealth += amt;
    }

    public void ResetHealth()
    {
        if (IsServer)
        {
            health.Value = maxAndDefaultHealth;
        }
    }

    private void Start()
    {
        ResetHealth();
    }
}
