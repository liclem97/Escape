using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    protected enum GunType { Pistol, Revolver }

    [Header("Crosshair UI")]
    [SerializeField] protected Image gunCrosshair;
    [SerializeField] protected GameObject rayVisualizer; // 크로스헤어를 갖고있는 Ray Visualizer

    [Header("Gun Stats")]
    [SerializeField] protected float gunDamage;
    [SerializeField] protected float attackDelay;
    [SerializeField] protected int ammo;
    [SerializeField] protected int maxAmmo;

    //[Header("Camera Shake")]
    //[SerializeField] protected CameraShake cameraShake;

    [Header("Sound")]
    [SerializeField] protected AudioClip gunFireSound; // 총 발사 소리
    [SerializeField] protected AudioClip gunTickSound; // 탄창 없음 소리

    private AudioSource audioSource;
    private GunType gunType;

    /* ARAVR Inputs */
    private ARAVRInput.Button fireButton = ARAVRInput.Button.IndexTrigger;
    private ARAVRInput.Controller fireController = ARAVRInput.Controller.RTouch;

    /*********************************************************************************************
    작성자: 박원정
    함수: Start
    기능: AudioSource를 초기화하고 커서를 숨김
    작성일자: 2025-06-30
    *********************************************************************************************/
    private void Start()
    {
        // AudioSource 컴포넌트 초기화
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Cursor.visible = false; // 커서 숨김
    }

    /*********************************************************************************************
    작성자: 박원정
    함수: Update
    기능: 커서를 숨기고, 발사 입력을 받아 사격 로직을 수행함
    작성일자: 2025-06-30
    *********************************************************************************************/
    private void Update()
    {
        // 커서를 항상 비활성화 한다.
        if (Cursor.visible == true)
            if (Cursor.visible)
                Cursor.visible = false;

        if (ARAVRInput.GetDown(fireButton, fireController))
        {
            ARAVRInput.PlayVibration(fireController);

            // 탄 확인
            if (gunType == GunType.Revolver && ammo <= 0)
            {
                PlayGunSound(false); // 빈 탄창 소리
                return;
            }

            Fire();

            // 탄약 차감
            if (gunType == GunType.Revolver)
            {
                ammo--;
            }

            PlayGunSound(true);
        }
    }

    /*********************************************************************************************
    작성자: 박원정
    함수: Fire
    기능: Ray를 쏘아 적중한 대상에 대해 처리를 수행
    작성일자: 2025-06-30
    *********************************************************************************************/
    private void Fire()
    {
        Ray ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
        RaycastHit hitResult;

        if (Physics.Raycast(ray, out hitResult))
        {
            // 타겟에 데미지 주기 등 로직
            Debug.Log($"Hit: {hitResult.collider.name}");

            // 시각 효과, 데미지 처리 등 필요시 추가
        }
    }

    /*********************************************************************************************
    작성자: 박원정
    함수: PlayGunSound
    기능: 발사 혹은 탄창 없음에 따라 적절한 사운드를 재생함
    매개변수: bool isFiring - true면 발사음, false면 빈 탄창음
    작성일자: 2025-06-30
    *********************************************************************************************/
    private void PlayGunSound(bool isFiring)
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = isFiring ? gunFireSound : gunTickSound;
        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay);
    }
}
