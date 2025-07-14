using Photon.Pun;
using UnityEngine;

/* 플레이어 프리팹 스크립트 */
public class VRPlayer : MonoBehaviourPun
{
    [Header("Character Reference")]
    [SerializeField] private GameObject characterHead;      // 플레이어의 머리 모델
    [SerializeField] private GameObject characterLeftHand;  // 플레이어의 왼손 모델

    [Header("Item")]
    [SerializeField] private float itemGrabDistance;        // 아이템을 집을 수 있는 범위
    [SerializeField] private ItemSpawner ammoSpanwer;       // 리볼버 탄창 스포너

    private Transform rightHand;
    private Transform leftHand;
    private Transform hmd;
    private OVRCameraRig rig;

    private Item heldItem = null;

    private GameObject pistol;      // 피스톨
    private GameObject revolver;    // 리볼버
    private GameObject sniperRifle; // 스나이퍼 라이플

    private void Start()
    {
        rig = GetComponentInChildren<OVRCameraRig>();

        if (!photonView.IsMine)
        {
            if (rig != null && rig.centerEyeAnchor != null)
            {
                // 내 것이 아닌 플레이어의 centerEyeAnchor 비활성화
                rig.centerEyeAnchor.gameObject.SetActive(false);
            }

            return;
        }

        if (rig != null)
        {
            rightHand = rig.rightHandAnchor;
            leftHand = rig.leftHandAnchor;
            hmd = rig.centerEyeAnchor;
        }

        StartAmmoSpawn();
    }

    private void Update()
    {   
        // 로컬이 아닌 플레이어의 업데이트를 막음
        if (!photonView.IsMine) return;
        if (GameManager.Instance.IsGameOver)
        {
            photonView.RPC(nameof(HideAllMeshes), RpcTarget.AllBuffered);            
            return;
        }

        // 캐릭터 머리 모델의 위치를 hmd에 맞춤
        if (hmd != null && characterHead != null)
        {
            characterHead.transform.position = hmd.position;
            characterHead.transform.rotation = hmd.rotation;
        }

        // 캐릭터 왼손 모델의 위치를 leftHand에 맞춤
        if (leftHand != null && characterLeftHand != null)
        {
            characterLeftHand.transform.position = leftHand.position;
            
            Quaternion offsetRotation = Quaternion.Euler(0f, 0f, 0f);
            characterLeftHand.transform.rotation = leftHand.rotation * offsetRotation;
        }

        // 왼손 핸드트리거를 눌렀을 경우 아이템을 집으려 함
        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.LTouch))
        {
            TryPickupItem();
        }

        // 왼손 핸드트리거를 놓은 경우 아이템을 드롭
        if (ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.LTouch))
        {
            TryDropItem();
        }

        // 오른손 1번 버튼을 누른 경우 총기 스왑 시도
        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.RTouch))
        {
            SwapWeapon();
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: SwapWeapon
    * 기능: 플레이어1의 총기를 스왑하고 모든 클라이언트에서 동기화함
    ***********************************************************************************/
    [PunRPC]
    private void SwapWeapon()
    {
        // 스나이퍼 라이플이 활성화 중인 경우 (2P)
        if (sniperRifle != null && sniperRifle.activeSelf) return;
        
        // 총기 스왑
        if (pistol != null && revolver != null)
        {
            if (pistol.activeSelf && !revolver.activeSelf)
            {
                pistol.SetActive(false);
                revolver.SetActive(true);
            }
            else if (revolver.activeSelf && !pistol.activeSelf)
            {
                revolver.SetActive(false);
                pistol.SetActive(true);
            }
        }
    }

    // 플레이어1의 초기화 함수
    public void InitPlayer1()
    {
        // 총은 네트워크로 스폰
        // 피스톨, 리볼버를 스폰하고 오른손에 붙인다.
        if (photonView.IsMine && rightHand != null)
        {
            revolver = PhotonNetwork.Instantiate("Revolver", rightHand.position, rightHand.rotation);
            pistol = PhotonNetwork.Instantiate("Pistol", rightHand.position, rightHand.rotation);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, revolver.GetComponent<PhotonView>().ViewID);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, pistol.GetComponent<PhotonView>().ViewID);

            revolver.SetActive(false); // 리볼버는 처음 시작 시 비활성화
        }
    }

    // 플레이어2의 초기화 함수
    public void InitPlayer2()
    {
        // 총은 네트워크로 스폰
        if (photonView.IsMine && rightHand != null)
        {
            sniperRifle = PhotonNetwork.Instantiate("SniperRifle", rightHand.position, rightHand.rotation);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, sniperRifle.GetComponent<PhotonView>().ViewID);
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: AttachGunToHand
    * 기능: 스폰한 총기를 오른손에 붙이는 함수
    * 입력:
    *   - gunViewID: 스폰한 총의 ViewId
    ***********************************************************************************/
    [PunRPC]
    private void AttachGunToHand(int gunViewID)
    {
        PhotonView gunView = PhotonView.Find(gunViewID);
        if (gunView != null)
        {
            Transform hand = GetComponentInChildren<OVRCameraRig>()?.rightHandAnchor;
            if (hand != null)
            {
                // 부모 설정 대신 위치/회전만 따라가도록 설정
                Gun gunScript = gunView.GetComponent<Gun>();
                if (gunScript != null)
                {
                    gunScript.SetTargetHand(hand);
                }
            }
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: TryPickupItem
    * 기능: 아이템을 집고 왼손에 붙이는 함수   
    ***********************************************************************************/
    private void TryPickupItem()
    {
        if (heldItem != null) return; // 이미 잡고 있음

        // 일정 범위의 콜라이더를 탐색
        Collider[] colliders = Physics.OverlapSphere(leftHand.position, itemGrabDistance);
        foreach (var col in colliders)
        {
            Item item = col.GetComponent<Item>();
            if (item != null)
            {
                heldItem = item;    // 집은 아이템 저장
                item.AttachToLeftHand(photonView.ViewID); // 내 ViewID만 넘김

                if (characterLeftHand != null)
                {
                    characterLeftHand.SetActive(false); // 왼손 모델 잠시 비활성화
                }

                break; // 첫 번째 아이템만 집음
            }
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: TryDropItem
    * 기능: 아이템의 드롭을 시도하는 함수
    ***********************************************************************************/
    private void TryDropItem()
    {
        if (heldItem == null) return;

        heldItem.DetachFromHand(); // 직접 부모 해제
        heldItem = null;

        if (characterLeftHand != null)
        {
            characterLeftHand.SetActive(true);
        }
    }

    public void StartAmmoSpawn()
    {
        if (ammoSpanwer != null)
        {
            ammoSpanwer.ItemSpawnStart();
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: HideAllMeshes
    * 기능: 게임 종료 전 서로의 메쉬를 숨기는 함수
    ***********************************************************************************/
    [PunRPC]
    private void HideAllMeshes()
    {
        if (characterLeftHand) characterLeftHand.SetActive(false);
        if (characterHead) characterHead.SetActive(false);
        if (pistol) pistol.SetActive(false);
        if (revolver) revolver.SetActive(false);
        if (sniperRifle) sniperRifle.SetActive(false);
        if (ammoSpanwer)
        {
            ammoSpanwer.StopAllCoroutines();
            if (ammoSpanwer.CurrentItem)
            {
                ammoSpanwer.CurrentItem.SetActive(false);
            }            
        }
        if (TryGetComponent<Collider>(out Collider collider))
        {
            collider.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        //    if (rightHand == null || leftHand == null || hmd == null)
        //    {
        //        var rig = GetComponentInChildren<OVRCameraRig>();
        //        if (rig != null)
        //        {
        //            rightHand = rig.rightHandAnchor;
        //            leftHand = rig.leftHandAnchor;
        //            hmd = rig.centerEyeAnchor;
        //        }
        //    }

        //    if (rightHand != null)
        //    {
        //        Gizmos.color = Color.blue;
        //        Gizmos.DrawSphere(rightHand.position, 0.05f);
        //    }

        //    if (leftHand != null)
        //    {
        //        Gizmos.color = Color.red;
        //        Gizmos.DrawSphere(leftHand.position, itemGrabDistance);
        //    }

        //    if (hmd != null)
        //    {
        //        Gizmos.color = Color.yellow;
        //        Gizmos.DrawSphere(hmd.position, 0.05f);
        //    }
    }
}