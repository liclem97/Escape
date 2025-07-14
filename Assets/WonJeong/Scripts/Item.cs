using Photon.Pun;
using System.Collections;
using UnityEngine;

/* �÷��̾ ����ϴ� ������ ��ũ��Ʈ */
public class Item : MonoBehaviourPun
{
    [SerializeField] protected float autoUseTimer = 1.5f; // �ڵ� ��� ������

    [Header("Sounds")]
    [SerializeField] protected AudioClip itemUseSound;

    private ItemSpawner itemSpawner;

    private bool isHeld = false;                // �������� �����ִ� �������� �Ǵ�
    private bool isInTargetTrigger = false;     // �������� Ʈ���� �ȿ� �ִ��� �Ǵ�
    protected bool shouldUseItem = false;       // �������� ����ؾ� �ϴ��� �Ǵ�

    private Transform holder = null;            // �������� ���� hand�� transform
    private Transform targetPlayer;             // ������ Ÿ���� ��ġ

    protected AudioSource audioSource;
    protected int holdingPlayerViewID = -1;

    public bool IsHeld => isHeld;

    public ItemSpawner Spawner
    {
        get => itemSpawner;
        set => itemSpawner = value;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void AttachToLeftHand(int playerViewID)
    {
        photonView.RPC(nameof(RPC_AttachToLeftHand), RpcTarget.AllBuffered, playerViewID);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: RPC_AttachToLeftHand
    * ���: �������� �޼տ� ���̰� ����ȭ �ϴ� �Լ�
    * �Է�:
    *  playerViewID: �������� ���� �÷��̾��� ViewID
    ***********************************************************************************/
    [PunRPC]
    protected virtual void RPC_AttachToLeftHand(int playerViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);          // �÷��̾��� �� ID
        var rig = playerView?.GetComponentInChildren<OVRCameraRig>();   // �÷��̾��� rig
        Transform leftHand = rig?.leftHandAnchor;                       // rig���� �޼� ��ġ�� ������

        if (leftHand == null)
        {
            Debug.LogWarning("[Item] LeftHandAnchor not found");
            return;
        }

        transform.SetParent(leftHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;                  // �������� �θ� ����

        if (TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;                                      // Kinematic On

        isHeld = true;                                                  // ���� ���·� ��ȯ
        holder = leftHand;                                              // �޼� ����
        holdingPlayerViewID = playerViewID;                             // ���� �÷��̾��� viewID ����
    }

    public void DetachFromHand()
    {
        photonView.RPC(nameof(RPC_DetachFromHand), RpcTarget.AllBuffered);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: RPC_DetachFromHand
    * ���: �������� �޼տ��� �и��ϰ� ����ȭ�ϴ� �Լ�    
    ***********************************************************************************/
    [PunRPC]
    protected virtual void RPC_DetachFromHand()
    {
        isHeld = false;                                                 
        transform.SetParent(null);
        holder = null;

        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        // �������� TargetTrigger�� �ְ� Ÿ�� �÷��̾ ������ ���
        // Ÿ�� �÷��̾��� ��ġ�� �������� ���ư���
        if (isInTargetTrigger && targetPlayer != null)
        {
            photonView.RPC(nameof(RPC_FlyTo), RpcTarget.AllBuffered,
                targetPlayer.position, targetPlayer.rotation, 1f);
            shouldUseItem = true;

            // ��� Ŭ���̾�Ʈ���� ���� �ð� �� UseItem �õ�
            StartCoroutine(AutoUseAfterDelay(autoUseTimer));
        }
        else if (Spawner != null)   // �� ���� ��쿣 �������� ��ġ�� �ǵ��ư���
        {
            photonView.RPC(nameof(RPC_FlyTo), RpcTarget.AllBuffered,
                Spawner.transform.position, Spawner.transform.rotation, 0.5f);

            transform.SetParent(Spawner.transform);
        }
        else
        {
            Debug.LogWarning("[Item] No Spawner assigned, cannot return.");
        }
    }

    // ������ Ʈ���� �ȿ� �ִ� ��� ��ǥ�� �����Ѵ�
    public void SetTargetPosition(Transform target)
    {
        targetPlayer = target;
        isInTargetTrigger = true;
    }

    public void ClearTargetPosition()
    {
        targetPlayer = null;
        isInTargetTrigger = false;
    }

    [PunRPC]
    protected virtual void RPC_FlyTo(Vector3 pos, Quaternion rot, float duration)
    {
        StartCoroutine(FlyTo(pos, rot, duration));
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: FlyTo
    * ���: �������� ��ǥ ��ġ�� ������ ����ȭ�ϴ� �Լ�
    * �Է�:
    *   - targetPosition: ������ ��ǥ�� ��ġ
    *   - targetRotation: ������ ��ǥ�� ȸ��
    *   - duration: ���󰡴� �ð�
    ***********************************************************************************/
    protected IEnumerator FlyTo(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        float time = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: AutoUseAfterDelay
    * ���: ���� �ð� ���� �������� �ڵ����� ����ϴ� �Լ�
    * �Է�:    
    *   - delay: ������ �ڵ� ��� ������
    ***********************************************************************************/
    private IEnumerator AutoUseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // ���� ���� �÷��̾ �ƴϰ�, ������ shouldUseItem ���¶�� ���
        if (!isHeld && shouldUseItem)
        {
            photonView.RPC(nameof(UseItem), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    protected virtual void UseItem()
    {
        transform.localScale = Vector3.zero;
        audioSource.PlayOneShot(itemUseSound);
        Destroy(gameObject, 3f);
    }
}