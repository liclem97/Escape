using Photon.Pun;
using UnityEngine;

public class VRPlayer : MonoBehaviourPun
{
    [Header("Character Reference")]
    [SerializeField] private GameObject characterHead;
    [SerializeField] private GameObject characterLeftHand;

    [Header("Item")]
    [SerializeField] private float itemGrabDistance;
    [SerializeField] private ItemSpawner ammoSpanwer;

    private Transform rightHand;
    private Transform leftHand;
    private Transform hmd;
    private OVRCameraRig rig;

    private Item heldItem = null;

    private GameObject pistol;
    private GameObject revolver;
    private GameObject sniperRifle;

    private void Start()
    {
        rig = GetComponentInChildren<OVRCameraRig>();

        if (!photonView.IsMine)
        {
            if (rig != null && rig.centerEyeAnchor != null)
            {
                // 내 것이 아닌 플레이어는 centerEyeAnchor 전체를 비활성화
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

        //StartAmmoSpawn();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (GameManager.Instance.IsGameOver)
        {
            photonView.RPC(nameof(HideAllMeshes), RpcTarget.AllBuffered);            
            return;
        }

        // HMD 위치를 Sphere에 반영
        if (hmd != null && characterHead != null)
        {
            characterHead.transform.position = hmd.position;
            characterHead.transform.rotation = hmd.rotation;
        }

        if (leftHand != null && characterLeftHand != null)
        {
            characterLeftHand.transform.position = leftHand.position;
            
            Quaternion offsetRotation = Quaternion.Euler(0f, 0f, 0f);
            characterLeftHand.transform.rotation = leftHand.rotation * offsetRotation;
        }

        // 왼손 핸드트리거를 눌렀을 때
        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.LTouch))
        {
            TryPickupItem();
        }

        if (ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.LTouch))
        {
            TryDropItem();
        }

        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.RTouch))
        {
            SwapWeapon();
        }
    }

    [PunRPC]
    private void SwapWeapon()
    {
        // 스나이퍼 라이플이 활성화 중인 경우 (2P)
        if (sniperRifle != null && sniperRifle.activeSelf) return;

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

    public void InitPlayer1()
    {
        // 총은 네트워크로 스폰
        if (photonView.IsMine && rightHand != null)
        {
            revolver = PhotonNetwork.Instantiate("Revolver", rightHand.position, rightHand.rotation);
            pistol = PhotonNetwork.Instantiate("Pistol", rightHand.position, rightHand.rotation);
            //GameObject gun = PhotonNetwork.Instantiate("SniperRifle", rightHand.position, rightHand.rotation);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, revolver.GetComponent<PhotonView>().ViewID);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, pistol.GetComponent<PhotonView>().ViewID);

            revolver.SetActive(false);
            //StartAmmoSpawn(); // 메인 빌드시 주석처리
        }
    }

    public void InitPlayer2()
    {
        // 총은 네트워크로 스폰
        if (photonView.IsMine && rightHand != null)
        {
            sniperRifle = PhotonNetwork.Instantiate("SniperRifle", rightHand.position, rightHand.rotation);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, sniperRifle.GetComponent<PhotonView>().ViewID);
        }
    }

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
                    gunScript.SetTargetHandAndRig(hand, rig);
                }
            }
        }
    }

    private void TryPickupItem()
    {
        if (heldItem != null) return; // 이미 잡고 있음

        Collider[] colliders = Physics.OverlapSphere(leftHand.position, itemGrabDistance);
        foreach (var col in colliders)
        {
            Item item = col.GetComponent<Item>();
            if (item != null)
            {
                heldItem = item;
                item.AttachToLeftHand(photonView.ViewID); // 내 ViewID만 넘김

                if (characterLeftHand != null)
                {
                    characterLeftHand.SetActive(false);
                }

                break; // 첫 번째 아이템만 집음
            }
        }
    }

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