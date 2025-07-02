using UnityEngine;

public class Gun : MonoBehaviour
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
        if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.RTouch))
        {
            TryFire();
        }
    }

    protected void GunInitialize()
    {
        if (rayVisualizer != null)
            rayVisualizer.On();

        if (gunType != GunType.Pistol)
            currentAmmo = maxAmmo;

        // �� ����� �ҽ��� ������ ������Ʈ �߰�
        gunAudioSource = GetComponent<AudioSource>();
        if (gunAudioSource == null)
            gunAudioSource = gameObject.AddComponent<AudioSource>();
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
        ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch);
    }

    protected void SpawnBulletFX(RaycastHit hit)
    {
        int hitLayer = hit.collider.gameObject.layer;
        GameObject effect = null;

        if (hitLayer == LayerMask.NameToLayer("Zombie") || hitLayer == LayerMask.NameToLayer("Human"))
        {
            effect = livingHitEffect;
        }
        else if (hitLayer == LayerMask.NameToLayer("Environment"))
        {
            effect = environmentHitEffect;
        }

        if (effect != null)
        {
            Instantiate(effect, hit.point, Quaternion.LookRotation(hit.normal));
        }

        Debug.DrawRay(rayVisualizer.transform.position, rayVisualizer.transform.forward * 100f, Color.red, 1f);
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
        // ���ε� �ִϸ��̼� �� ȣ��� �� �ֵ��� �ڷ�ƾ ����
        currentAmmo = maxAmmo;
        gunState = GunState.Idle;
    }
}
