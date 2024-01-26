using UnityEngine;
using Unity.Netcode;

public struct LaserPayload : INetworkSerializable
{
    public Vector3 position;
    public Vector3 velocity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref velocity);
    }
}

[RequireComponent(typeof(AudioSource))]
public class LaserProjectile : NetworkBehaviour
{
    [SerializeField] private float bulletSpeed = 4.0f;
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private GameObject destroyParticle;
    [SerializeField] private AudioClip onBounceSound;

    private ulong shooterClientID = 0;
    private Vector3 laserVelocty = Vector3.zero;
    private Vector3 previousPosition = Vector3.zero;
    private GameObject previouslyHitObject = null;
    private AudioSource audioSource = null;

    private int bouncesUntilDestroy;

    public void SetShooterID(ulong shooterID)
    {
        shooterClientID = shooterID;
    }

    private void Start()
    {
        previousPosition = transform.position;
        laserVelocty = transform.forward;
        bouncesUntilDestroy = maxBounces;
         
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        Vector3 currentPos = transform.position;
        PerformLaserRaycast(previousPosition, currentPos);
        previousPosition = currentPos;

        transform.Translate(laserVelocty * bulletSpeed * Time.deltaTime, Space.World);
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
                //Hit a player
                DestroyLaser();

                if (NetworkManager.IsServer)
                {
                    player.Damage(shooterClientID, 100);
                }

                return;
            }

            if (previouslyHitObject != null && previouslyHitObject == info.transform.gameObject)
                return;


            //Bounce off the wall
            laserVelocty = Vector3.Reflect(laserVelocty, info.normal);
            laserVelocty.Normalize();

            transform.position = info.point;
            transform.forward = laserVelocty;
            bouncesUntilDestroy--;
            previouslyHitObject = info.transform.gameObject;

            audioSource.PlayOneShot(onBounceSound);

            Debug.Log("Bounce Registered. Bounces left: " + bouncesUntilDestroy);

            if (bouncesUntilDestroy <= 0)
            {
                DestroyLaser();
            }
        }
    }

    private void DestroyLaser()
    {
        Instantiate(destroyParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    [ClientRpc]
    private void UpdateLaserDataClientRpc(LaserPayload laserPayload)
    {
        transform.position = laserPayload.position;
        laserVelocty = laserPayload.velocity;

        transform.forward = laserVelocty;
    }
}
