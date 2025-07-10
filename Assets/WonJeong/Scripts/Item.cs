using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Item : MonoBehaviourPun
{
    [SerializeField] protected float autoUseTimer = 1.5f; // 자동 사용 딜레이

    [Header("Sounds")]
    [SerializeField] protected AudioClip itemUseSound;

    private ItemSpawner itemSpawner;

    private bool isHeld = false;
    private bool isInTargetTrigger = false;
    protected bool shouldUseItem = false;

    private Transform holder = null;
    private Transform targetPlayer;

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

    [PunRPC]
    protected virtual void RPC_AttachToLeftHand(int playerViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        var rig = playerView?.GetComponentInChildren<OVRCameraRig>();
        Transform leftHand = rig?.leftHandAnchor;

        if (leftHand == null)
        {
            Debug.LogWarning("[Item] LeftHandAnchor not found");
            return;
        }

        transform.SetParent(leftHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;

        isHeld = true;
        holder = leftHand;
        holdingPlayerViewID = playerViewID;
    }

    public void DetachFromHand()
    {
        photonView.RPC(nameof(RPC_DetachFromHand), RpcTarget.AllBuffered);
    }

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

        if (isInTargetTrigger && targetPlayer != null)
        {
            photonView.RPC(nameof(RPC_FlyTo), RpcTarget.AllBuffered,
                targetPlayer.position, targetPlayer.rotation, 1f);
            shouldUseItem = true;

            // 모든 클라이언트에서 일정 시간 후 UseItem 시도
            StartCoroutine(AutoUseAfterDelay(autoUseTimer));
        }
        else if (Spawner != null)
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

    private IEnumerator AutoUseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 내가 잡은 플레이어가 아니고, 여전히 shouldUseItem 상태라면 사용
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