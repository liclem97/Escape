using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/* 플레이어의 총기 함수 */
public class Gun : MonoBehaviourPun
{
    protected enum GunType { Pistol, Revolver, SniperRifle }

    [Header("Shoot Point")]
    [SerializeField] protected Transform muzzlePoint;           // 머즐 위치
    [SerializeField] protected RayVisualizer rayVisualizer;     // 레이 비쥬얼
    [SerializeField] protected LayerMask HitRayMask;            // 히트 레이 마스크

    [Header("Gun Stats")]
    [SerializeField] protected float gunDamage = 10f;           // 총 대미지
    [SerializeField] protected float gunAttackDelay = 0.5f;     // 발사 지연 시간
    [SerializeField] protected int maxAmmo = 6;                 // 탄창
    [SerializeField] protected float attackRange = 100f;        // 공격 거리

    [Header("Gun Effect")]
    [SerializeField] protected GameObject muzzleFlashEffect;
    [SerializeField] protected GameObject environmentHitEffect;
    [SerializeField] protected GameObject livingHitEffect;
    [SerializeField] protected AudioClip gunFireSound;          // 각 이펙트 및 사운드

    [Header("UI")]
    [SerializeField] protected Text ammoText;
    [SerializeField] protected Image attackCooldownImage;       // 탄창 및 발사 지연 UI

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
    * 작성자: 박원정
    * 함수: SetTargetHand
    * 기능: 총이 붙을 hand의 위치를 저장하는 함수
    * 입력: 
    *   - hand: 총이 붙는 위치    
    ***********************************************************************************/
    public void SetTargetHand(Transform hand)
    {
        targetHand = hand;
        transform.SetParent(targetHand);  // 부모로 설정
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: GunInitialize
    * 기능: 총기의 초기화 함수
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
    * 작성자: 박원정
    * 함수: TryFire
    * 기능: 총의 발사를 시도하는 함수
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

        // 리볼버인 경우 탄창이 감소하고 탄창의 텍스트를 업데이트함
        if (gunType == GunType.Revolver)
        {
            currentAmmo--;
            UpdateAmmoText();
        }

        // 공격 딜레이 UI를 갱신함
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
    * 작성자: 박원정
    * 함수: SpawnBulletFX
    * 기능: 총기 피격 효과를 출력하는 함수
    * 입력: 
    *   - position: 이펙트 스폰 위치
    *   - normal: 이펙트 스폰 방향
    *   - hitLayer: 이펙트 구분 레이어
    ***********************************************************************************/
    protected void SpawnBulletFX(Vector3 position, Vector3 normal, int hitLayer)
    {
        GameObject effect = null;

        // 히트 레이어가 좀비 또는 사람일 경우 피 이펙트 출력
        if (hitLayer == LayerMask.NameToLayer("Zombie") || hitLayer == LayerMask.NameToLayer("Human"))
            effect = livingHitEffect;
        // 히트 레이어가 환경 또는 폭탄일 경우 스톤 이펙트 출력
        else if (hitLayer == LayerMask.NameToLayer("Environment") || hitLayer == LayerMask.NameToLayer("Bomb"))
            effect = environmentHitEffect;

        // 이펙트 스폰 및 일정 시간 후 파괴
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
