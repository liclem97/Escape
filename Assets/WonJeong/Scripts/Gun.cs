using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    protected enum GunType { Pistol, Revolver }

    [Header("Crosshair UI")]
    [SerializeField] protected Image gunCrosshair;
    [SerializeField] protected GameObject rayVisualizer; // ũ�ν��� �����ִ� Ray Visualizer

    [Header("Gun Stats")]
    [SerializeField] protected float gunDamage;
    [SerializeField] protected float attackDelay;
    [SerializeField] protected int ammo;
    [SerializeField] protected int maxAmmo;

    //[Header("Camera Shake")]
    //[SerializeField] protected CameraShake cameraShake;

    [Header("Sound")]
    [SerializeField] protected AudioClip gunFireSound; // �� �߻� �Ҹ�
    [SerializeField] protected AudioClip gunTickSound; // źâ ���� �Ҹ�

    private AudioSource audioSource;
    private GunType gunType;

    /* ARAVR Inputs */
    private ARAVRInput.Button fireButton = ARAVRInput.Button.IndexTrigger;
    private ARAVRInput.Controller fireController = ARAVRInput.Controller.RTouch;

    /*********************************************************************************************
    �ۼ���: �ڿ���
    �Լ�: Start
    ���: AudioSource�� �ʱ�ȭ�ϰ� Ŀ���� ����
    �ۼ�����: 2025-06-30
    *********************************************************************************************/
    private void Start()
    {
        // AudioSource ������Ʈ �ʱ�ȭ
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Cursor.visible = false; // Ŀ�� ����
    }

    /*********************************************************************************************
    �ۼ���: �ڿ���
    �Լ�: Update
    ���: Ŀ���� �����, �߻� �Է��� �޾� ��� ������ ������
    �ۼ�����: 2025-06-30
    *********************************************************************************************/
    private void Update()
    {
        // Ŀ���� �׻� ��Ȱ��ȭ �Ѵ�.
        if (Cursor.visible == true)
            if (Cursor.visible)
                Cursor.visible = false;

        if (ARAVRInput.GetDown(fireButton, fireController))
        {
            ARAVRInput.PlayVibration(fireController);

            // ź Ȯ��
            if (gunType == GunType.Revolver && ammo <= 0)
            {
                PlayGunSound(false); // �� źâ �Ҹ�
                return;
            }

            Fire();

            // ź�� ����
            if (gunType == GunType.Revolver)
            {
                ammo--;
            }

            PlayGunSound(true);
        }
    }

    /*********************************************************************************************
    �ۼ���: �ڿ���
    �Լ�: Fire
    ���: Ray�� ��� ������ ��� ���� ó���� ����
    �ۼ�����: 2025-06-30
    *********************************************************************************************/
    private void Fire()
    {
        Ray ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
        RaycastHit hitResult;

        if (Physics.Raycast(ray, out hitResult))
        {
            // Ÿ�ٿ� ������ �ֱ� �� ����
            Debug.Log($"Hit: {hitResult.collider.name}");

            // �ð� ȿ��, ������ ó�� �� �ʿ�� �߰�
        }
    }

    /*********************************************************************************************
    �ۼ���: �ڿ���
    �Լ�: PlayGunSound
    ���: �߻� Ȥ�� źâ ������ ���� ������ ���带 �����
    �Ű�����: bool isFiring - true�� �߻���, false�� �� źâ��
    �ۼ�����: 2025-06-30
    *********************************************************************************************/
    private void PlayGunSound(bool isFiring)
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = isFiring ? gunFireSound : gunTickSound;
        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay);
    }
}
