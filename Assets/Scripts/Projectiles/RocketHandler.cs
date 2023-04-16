using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RocketHandler : NetworkBehaviour
{
    [Header("Prefabs")]
    public GameObject explosionParticleSystemPrefab;

    [Header("Collision detection")]
    public Transform checkForImpactPoint;
    public LayerMask collisionLayers;

    TickTimer maxLiveDurationTickTimer = TickTimer.None;

    int rocketSpeed = 20;

    List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

    PlayerRef firedByPlayerRef;
    string firedByPlayerName;
    NetworkObject firedByNetworkObject;

    NetworkObject networkObject;

    public void Fire(PlayerRef fireByPlayerRef, NetworkObject firedByNetworkObject, string firedByPlayerName)
    {
        this.firedByPlayerRef = fireByPlayerRef;
        this.firedByPlayerName = firedByPlayerName;
        this.firedByNetworkObject = firedByNetworkObject;

        networkObject = GetComponent<NetworkObject>();

        maxLiveDurationTickTimer = TickTimer.CreateFromSeconds(Runner, 10);
    }

    public override void FixedUpdateNetwork()
    {
        transform.position += transform.forward * Runner.DeltaTime * rocketSpeed;

        if(Object.HasStateAuthority)
        {
            if(maxLiveDurationTickTimer.Expired(Runner))
            {
                Runner.Despawn(networkObject);
                return;
            }

            int hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, 0.5f, firedByPlayerRef, hits, collisionLayers, HitOptions.IncludePhysX);

            bool isValidHit = false;

            if(hitCount > 0)
                isValidHit = true;

            for(int i = 0;i < hitCount; i++)
            {
                if (hits[i].Hitbox != null)
                {
                    if (hits[i].Hitbox.Root.GetBehaviour<NetworkObject>() == firedByNetworkObject)
                        isValidHit = false;

                    
                }
            }



            if (isValidHit)
            {
                hitCount = Runner.LagCompensation.OverlapSphere(checkForImpactPoint.position, 4, firedByPlayerRef, hits, collisionLayers, HitOptions.None);

                for(int i=0;i<hitCount;i++)
                {
                    HPHandler hPHandler = hits[i].Hitbox.transform.root.GetComponent<HPHandler>();

                    if (hPHandler != null)
                        hPHandler.OnTakeDamage(firedByPlayerName, 100);
                }

                Runner.Despawn(networkObject);
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Instantiate(explosionParticleSystemPrefab, checkForImpactPoint.position, Quaternion.identity);
    }

}
