using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviourPun
{
    protected enum GunType { Pistol, Revolver, SniperRifle }

    [Header("Shoot Point")]
    [SerializeField] protected Transform muzzlePoint;
    [SerializeField] protected RayVisualizer rayVisualizer;
    [SerializeField] protected LayerMask HitRayMask;

    [Header("Gun Stats")]
    [SerializeField] protected float gunDamage = 10f;
    [SerializeField] protected float gunAttackDelay = 0.5f;
    [SerializeField] protected int maxAmmo = 6;
    [SerializeField] protected float attackRange = 100f;

    [Header("Gun Effect")]
    [SerializeField] protected GameObject muzzleFlashEffect;
    [SerializeField] protected GameObject environmentHitEffect;
    [SerializeField] protected GameObject livingHitEffect;
    [SerializeField] protected AudioClip gunFireSound;   

    [Header("UI")]
    [SerializeField] protected Text ammoText;
    [SerializeField] protected Image attackCooldownImage;

    [Header("Camera Zoom")]
    [SerializeField] protected float zoomFOV = 30f;
    [SerializeField] protected float normalFOV = 60f;
    [SerializeField] protected float zoomSpeed = 10f;
    [SerializeField] protected float zoomOffset = 0.2f; // 앞쪽으로 당기는 거리

    protected Vector3 originalHeadLocalPos;
    protected bool headOffsetApplied = false;

    protected OVRCameraRig playerRig;
    protected Camera eyeCamera;
    protected bool isZooming = false;

    private AudioSource gunAudioSource;
    private Transform targetHand;

    protected GunType gunType = GunType.Pistol;
    protected float lastAttackTime;
    protected int currentAmmo;
    protected ParticleSystem muzzleEffect;

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

        UpdateCooldownImage();
    }

    public void SetTargetHandAndRig(Transform hand, OVRCameraRig rig)
    {
        targetHand = hand;
        playerRig = rig;

        if (playerRig != null)
        {
            eyeCamera = playerRig.centerEyeAnchor.GetComponent<Camera>();
            if (eyeCamera != null)
            {
                originalHeadLocalPos = playerRig.centerEyeAnchor.localPosition;
            }
        }
    }

    protected void GunInitialize()
    {
        if (!photonView.IsMine) return;

        InitializeRayVisualizer();
        InitializeAmmo();
        InitializeAudio();
        CacheMuzzleEffect();
        UpdateAmmoText();
    }

    private void InitializeRayVisualizer()
    {
        if (rayVisualizer != null)
            rayVisualizer.On();
    }

    private void InitializeAmmo()
    {
        if (gunType != GunType.Pistol)
            currentAmmo = maxAmmo;
    }

    private void InitializeAudio()
    {
        gunAudioSource = GetComponent<AudioSource>();
        if (gunAudioSource == null)
            gunAudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void CacheMuzzleEffect()
    {
        if (muzzleFlashEffect != null)
            muzzleEffect = muzzleFlashEffect.GetComponent<ParticleSystem>();
    }

    protected virtual void TryFire()
    {
        if (Time.time - lastAttackTime < gunAttackDelay)
            return;

        if (gunType != GunType.Pistol && currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            return;
        }

        Fire();
        lastAttackTime = Time.time;

        if (gunType == GunType.Revolver)
        {
            currentAmmo--;
            UpdateAmmoText();
        }

        if (attackCooldownImage != null)
        {
            attackCooldownImage.fillAmount = 0f;
        }
    }

    protected virtual void Fire()
    {
        if (muzzleEffect != null)
        {
            muzzleEffect.Play();
        }
    }

    protected void SpawnBulletFX(Vector3 position, Vector3 normal, int hitLayer)
    {
        GameObject effect = null;

        if (hitLayer == LayerMask.NameToLayer("Zombie") || hitLayer == LayerMask.NameToLayer("Human"))
            effect = livingHitEffect;
        else if (hitLayer == LayerMask.NameToLayer("Environment") || hitLayer == LayerMask.NameToLayer("Bomb"))
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
    }

    protected void UpdateAmmoText()
    {
        if (!ammoText) return;

        ammoText.text = $"{currentAmmo}/{maxAmmo}";
    }

    protected void UpdateCooldownImage()
    {
        if (attackCooldownImage == null || attackCooldownImage.fillAmount == 1f) return;

        float elapsed = Time.time - lastAttackTime;
        attackCooldownImage.fillAmount = Mathf.Clamp01(elapsed / gunAttackDelay);
    }
}
