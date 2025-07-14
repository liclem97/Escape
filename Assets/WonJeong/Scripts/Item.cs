using Photon.Pun;
using System.Collections;
using UnityEngine;

/* 플레이어가 사용하는 아이템 스크립트 */
public class Item : MonoBehaviourPun
{
    [SerializeField] protected float autoUseTimer = 1.5f; // 자동 사용 딜레이

    [Header("Sounds")]
    [SerializeField] protected AudioClip itemUseSound;

    private ItemSpawner itemSpawner;

    private bool isHeld = false;                // 아이템이 잡혀있는 상태인지 판단
    private bool isInTargetTrigger = false;     // 아이템이 트리거 안에 있는지 판단
    protected bool shouldUseItem = false;       // 아이템을 사용해야 하는지 판다

    private Transform holder = null;            // 아이템을 집은 hand의 transform
    private Transform targetPlayer;             // 아이템 타겟의 위치

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
    * 작성자: 박원정
    * 함수: RPC_AttachToLeftHand
    * 기능: 아이템을 왼손에 붙이고 동기화 하는 함수
    * 입력:
    *  playerViewID: 아이템을 붙일 플레이어의 ViewID
    ***********************************************************************************/
    [PunRPC]
    protected virtual void RPC_AttachToLeftHand(int playerViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);          // 플레이어의 뷰 ID
        var rig = playerView?.GetComponentInChildren<OVRCameraRig>();   // 플레이어의 rig
        Transform leftHand = rig?.leftHandAnchor;                       // rig에서 왼손 위치를 저장함

        if (leftHand == null)
        {
            Debug.LogWarning("[Item] LeftHandAnchor not found");
            return;
        }

        transform.SetParent(leftHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;                  // 아이템의 부모 설정

        if (TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;                                      // Kinematic On

        isHeld = true;                                                  // 잡은 상태로 전환
        holder = leftHand;                                              // 왼손 저장
        holdingPlayerViewID = playerViewID;                             // 잡은 플레이어의 viewID 저장
    }

    public void DetachFromHand()
    {
        photonView.RPC(nameof(RPC_DetachFromHand), RpcTarget.AllBuffered);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: RPC_DetachFromHand
    * 기능: 아이템을 왼손에서 분리하고 동기화하는 함수    
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

        // 아이템이 TargetTrigger에 있고 타깃 플레이어가 설정된 경우
        // 타깃 플레이어의 위치로 아이템이 날아간다
        if (isInTargetTrigger && targetPlayer != null)
        {
            photonView.RPC(nameof(RPC_FlyTo), RpcTarget.AllBuffered,
                targetPlayer.position, targetPlayer.rotation, 1f);
            shouldUseItem = true;

            // 모든 클라이언트에서 일정 시간 후 UseItem 시도
            StartCoroutine(AutoUseAfterDelay(autoUseTimer));
        }
        else if (Spawner != null)   // 이 외의 경우엔 스포너의 위치로 되돌아간다
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

    // 아이템 트리거 안에 있는 경우 목표를 설정한다
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
    * 작성자: 박원정
    * 함수: FlyTo
    * 기능: 아이템을 목표 위치로 날리고 동기화하는 함수
    * 입력:
    *   - targetPosition: 아이템 목표의 위치
    *   - targetRotation: 아이템 목표의 회전
    *   - duration: 날라가는 시간
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
    * 작성자: 박원정
    * 함수: AutoUseAfterDelay
    * 기능: 일정 시간 이후 아이템을 자동으로 사용하는 함수
    * 입력:    
    *   - delay: 아이템 자동 사용 딜레이
    ***********************************************************************************/
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