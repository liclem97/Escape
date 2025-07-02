using Photon.Pun;
using UnityEngine;

public class Gun : MonoBehaviourPun
{
    protected enum GunType { Pistol, Revolver, SniperRifle }
    protected enum GunState { Idle, Fire, Reloading }

    [Header("Ray")]
    [SerializeField] protected RayVisualizer rayVisualizer;

    [Header("Gun Stats")]
    [SerializeField] protected float gunDamage = 10f;
    [SerializeField] protected float gunAttackDelay = 0.5f;
    [SerializeField] protected int maxAmmo = 6;
    [SerializeField] protected float attackRange = 100f;

    [Header("Gun Effect")]
    [SerializeField] protected GameObject environmentHitEffect;
    [SerializeField] protected GameObject livingHitEffect;
    [SerializeField] protected AudioClip gunFireSound;

    private AudioSource gunAudioSource;
    private Transform targetHand;

    protected GunType gunType = GunType.Pistol;
    protected GunState gunState = GunState.Idle;

    protected float lastAttackTime;
    protected int currentAmmo;

    protected virtual void Start()
    {
        GunInitialize();
    }

    protected virtual void Update()
    {
        if (!photonView.IsMine) return;
        if (targetHand != null)
        {
            transform.position = targetHand.position;
            transform.rotation = targetHand.rotation;
        }

        if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.RTouch))
        {            
            TryFire();
        }

    }

    protected void GunInitialize()
    {
        if (photonView.IsMine && rayVisualizer != null)
            rayVisualizer.On();

        if (gunType != GunType.Pistol)
            currentAmmo = maxAmmo;

        // 총 오디오 소스가 없으면 컴포넌트 추가
        gunAudioSource = GetComponent<AudioSource>();
        if (gunAudioSource == null)
            gunAudioSource = gameObject.AddComponent<AudioSource>();
    }

    public void SetTargetHand(Transform hand)
    {
        targetHand = hand;
    }

    protected virtual void TryFire()
    {
        if (Time.time - lastAttackTime < gunAttackDelay || gunState != GunState.Idle)
            return;

        if (gunType != GunType.Pistol && currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            return;
        }

        Fire();
        lastAttackTime = Time.time;

        if (gunType != GunType.Pistol)
            currentAmmo--;
    }

    protected virtual void Fire()
    {
       
    }

    protected void SpawnBulletFX(Vector3 position, Vector3 normal, int hitLayer)
    {
        GameObject effect = null;

        if (hitLayer == LayerMask.NameToLayer("Zombie") || hitLayer == LayerMask.NameToLayer("Human"))
            effect = livingHitEffect;
        else if (hitLayer == LayerMask.NameToLayer("Environment"))
            effect = environmentHitEffect;

        if (effect != null)
        {
            GameObject instance = Instantiate(effect, position, Quaternion.LookRotation(normal));
            var ps = instance.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(instance, 2f);
        }

        Debug.DrawRay(position, normal * -0.3f, Color.red, 1f);
    }

    protected void PlayGunFireSound()
    {
        if (gunFireSound != null && gunAudioSource != null)
            gunAudioSource.PlayOneShot(gunFireSound);
    }

    public virtual void Reload()
    {
        if (gunType == GunType.Pistol) return;

        gunState = GunState.Reloading;
        // 리로드 애니메이션 후 호출될 수 있도록 코루틴 가능
        currentAmmo = maxAmmo;
        gunState = GunState.Idle;
    }
}
