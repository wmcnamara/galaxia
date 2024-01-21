using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

public class LaserProjectile : NetworkBehaviour
{
    [SerializeField] private float bulletSpeed = 4.0f;
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private GameObject destroyParticle;

    private NetworkVariable<ulong> shooterClientID = new NetworkVariable<ulong>(0);
    private Vector3 laserVelocty = Vector3.zero;
    private Vector3 previousPosition = Vector3.zero;
    private GameObject previouslyHitObject = null;

    private int bouncesUntilDestroy;

    public void SetShooterID(ulong shooterID)
    {
        if (IsServer)
        {
            shooterClientID.Value = shooterID;
        }
    }

    private void Start()
    {
        if (IsServer)
        {
            laserVelocty = transform.forward;
            bouncesUntilDestroy = maxBounces;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            Vector3 currentPos = transform.position;
            PerformLaserRaycast(previousPosition, currentPos);
            previousPosition = currentPos;

            transform.Translate(laserVelocty * bulletSpeed * Time.deltaTime, Space.World);
        }
    }

    void PerformLaserRaycast(Vector3 prevPos, Vector3 currentPos)
    {
        if (Physics.Linecast(prevPos, currentPos, out RaycastHit info, hitMask))
        {
            if (info.transform.gameObject.GetInstanceID() == gameObject.GetInstanceID())
                return;

            //Check for a player hit
            if (info.transform.TryGetComponent(out Player player))
            {
                if (player.OwnerClientId != shooterClientID.Value)
                {
                    //Hit a player
                    DestroyLaser();
                }

                return;
            }

            if (previouslyHitObject != null && previouslyHitObject == info.transform.gameObject)
                return;


            //Bounce of the wall
            laserVelocty = Vector3.Reflect(laserVelocty, info.normal);
            laserVelocty.Normalize();
            transform.position = info.point;
            transform.forward = laserVelocty;
            bouncesUntilDestroy--;
            previouslyHitObject = info.transform.gameObject;

            Debug.Log("Bounce Registered. Bounces left: " + bouncesUntilDestroy);

            if (bouncesUntilDestroy <= 0)
            {
                DestroyLaser();
            }
        }
    }

    void DestroyLaser()
    {
        Instantiate(destroyParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
