using Fusion;
using System.Collections;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnFireChanged))]
    public bool isFiring { get; set; }

    public ParticleSystem fireParticleSystem;
    public Transform aimPoint;
    public LayerMask collisionLayers;

    float lastTimeFired = 0f;

    HPHandler hpHandler;

    private void Awake()
    {
        hpHandler = GetComponent<HPHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    public override void FixedUpdateNetwork()
    {
        if (hpHandler.isDead)
            return;

        if(GetInput(out NetworkInputData networkInputData))
        {
            if(networkInputData.isFireButtonPressed)
                Fire(networkInputData.aimForwardVector);
        }        
    }

    void Fire(Vector3 aimForwardVector)
    {
        if (Time.time - lastTimeFired < 0.15f)
            return;

        StartCoroutine(FireEffectCO());

        Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitInfo, collisionLayers, HitOptions.IncludePhysX);

        float hitDistance = 100;
        bool isHitOtherPlayer = false;

        if(hitInfo.Distance > 0)
            hitDistance = hitInfo.Distance;

        if(hitInfo.Hitbox != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit hitbox {hitInfo.Hitbox.transform.root.name}");

            if (Object.HasStateAuthority)
                hitInfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage();

            isHitOtherPlayer = true;
        }
        else if(hitInfo.Collider != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitInfo.Collider.transform.name}");
        }


        if(isHitOtherPlayer)
            Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.red, 1);
        else 
            Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.green, 1);

        lastTimeFired = Time.time;
    }

    IEnumerator FireEffectCO()
    {
        isFiring = true;

        fireParticleSystem.Play();

        yield return new WaitForSeconds(0.09f);

        isFiring = false;
    }

    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        //Debug.Log($"{Time.time} OnFireChanged value {changed.Behaviour.isFiring}");

        bool isFiringCurrent = changed.Behaviour.isFiring;

        changed.LoadOld();

        bool isFiringOld = changed.Behaviour.isFiring;

        if (isFiringCurrent && !isFiringOld)
            changed.Behaviour.OnFireRemote();
    }

    void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
            fireParticleSystem.Play();
    }
}
