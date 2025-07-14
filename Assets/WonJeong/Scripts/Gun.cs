using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/* �÷��̾��� �ѱ� �Լ� */
public class Gun : MonoBehaviourPun
{
    protected enum GunType { Pistol, Revolver, SniperRifle }

    [Header("Shoot Point")]
    [SerializeField] protected Transform muzzlePoint;           // ���� ��ġ
    [SerializeField] protected RayVisualizer rayVisualizer;     // ���� �����
    [SerializeField] protected LayerMask HitRayMask;            // ��Ʈ ���� ����ũ

    [Header("Gun Stats")]
    [SerializeField] protected float gunDamage = 10f;           // �� �����
    [SerializeField] protected float gunAttackDelay = 0.5f;     // �߻� ���� �ð�
    [SerializeField] protected int maxAmmo = 6;                 // źâ
    [SerializeField] protected float attackRange = 100f;        // ���� �Ÿ�

    [Header("Gun Effect")]
    [SerializeField] protected GameObject muzzleFlashEffect;
    [SerializeField] protected GameObject environmentHitEffect;
    [SerializeField] protected GameObject livingHitEffect;
    [SerializeField] protected AudioClip gunFireSound;          // �� ����Ʈ �� ����

    [Header("UI")]
    [SerializeField] protected Text ammoText;
    [SerializeField] protected Image attackCooldownImage;       // źâ �� �߻� ���� UI

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
        //if (targetHand != null)
        //{
        //    transform.position = targetHand.position;
        //    transform.rotation = targetHand.rotation;
        //}

        if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.RTouch))
        {            
            TryFire();
        }

        UpdateCooldownImage();
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: SetTargetHand
    * ���: ���� ���� hand�� ��ġ�� �����ϴ� �Լ�
    * �Է�: 
    *   - hand: ���� �ٴ� ��ġ    
    ***********************************************************************************/
    public void SetTargetHand(Transform hand)
    {
        targetHand = hand;
        transform.SetParent(targetHand);  // �θ�� ����
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: GunInitialize
    * ���: �ѱ��� �ʱ�ȭ �Լ�
    ***********************************************************************************/
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

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: TryFire
    * ���: ���� �߻縦 �õ��ϴ� �Լ�
    ***********************************************************************************/
    protected virtual void TryFire()
    {
        if (Time.time - lastAttackTime < gunAttackDelay)
            return;

        if (gunType == GunType.Revolver && currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            return;
        }

        Fire();
        lastAttackTime = Time.time;

        // �������� ��� źâ�� �����ϰ� źâ�� �ؽ�Ʈ�� ������Ʈ��
        if (gunType == GunType.Revolver)
        {
            currentAmmo--;
            UpdateAmmoText();
        }

        // ���� ������ UI�� ������
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

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: SpawnBulletFX
    * ���: �ѱ� �ǰ� ȿ���� ����ϴ� �Լ�
    * �Է�: 
    *   - position: ����Ʈ ���� ��ġ
    *   - normal: ����Ʈ ���� ����
    *   - hitLayer: ����Ʈ ���� ���̾�
    ***********************************************************************************/
    protected void SpawnBulletFX(Vector3 position, Vector3 normal, int hitLayer)
    {
        GameObject effect = null;

        // ��Ʈ ���̾ ���� �Ǵ� ����� ��� �� ����Ʈ ���
        if (hitLayer == LayerMask.NameToLayer("Zombie") || hitLayer == LayerMask.NameToLayer("Human"))
            effect = livingHitEffect;
        // ��Ʈ ���̾ ȯ�� �Ǵ� ��ź�� ��� ���� ����Ʈ ���
        else if (hitLayer == LayerMask.NameToLayer("Environment") || hitLayer == LayerMask.NameToLayer("Bomb"))
            effect = environmentHitEffect;

        // ����Ʈ ���� �� ���� �ð� �� �ı�
        if (effect != null)
        {
            GameObject instance = Instantiate(effect, position, Quaternion.LookRotation(normal));
            var ps = instance.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(instance, 2f);
        }
    }

    protected void PlayGunFireSound()
    {
        if (gunFireSound != null && gunAudioSource != null)
            gunAudioSource.PlayOneShot(gunFireSound);
    }

    public virtual void Reload()
    {
        if (gunType != GunType.Revolver) return;
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
