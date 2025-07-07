using Photon.Pun;
using UnityEngine;

public class VRPlayer : MonoBehaviourPun
{
    [Header("Character Reference")]
    [SerializeField] private Transform hmdFollowerSphere;

    [Header("Item")]
    [SerializeField] private float itemGrabDistance;
    [SerializeField] private ItemSpawner ammoSpanwer;

    private Transform rightHand;
    private Transform leftHand;
    private Transform hmd;
    private OVRCameraRig rig;

    private Item heldItem = null;

    private void Start()
    {
        rig = GetComponentInChildren<OVRCameraRig>();

        if (!photonView.IsMine)
        {
            if (rig != null && rig.centerEyeAnchor != null)
            {
                // �� ���� �ƴ� �÷��̾�� centerEyeAnchor ��ü�� ��Ȱ��ȭ
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

        // HMD ��ġ�� Sphere�� �ݿ�
        if (hmd != null && hmdFollowerSphere != null)
        {
            hmdFollowerSphere.position = hmd.position;
            hmdFollowerSphere.rotation = hmd.rotation;
        }

        // �޼� �ڵ�Ʈ���Ÿ� ������ ��
        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.LTouch))
        {
            TryPickupItem();
        }

        if (ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.LTouch))
        {
            TryDropItem();
        }
    }

    public void InitPlayer1()
    {
        // ���� ��Ʈ��ũ�� ����
        if (photonView.IsMine && rightHand != null)
        {
            GameObject gun = PhotonNetwork.Instantiate("Revolver", rightHand.position, rightHand.rotation);
            //GameObject gun = PhotonNetwork.Instantiate("Pistol", rightHand.position, rightHand.rotation);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, gun.GetComponent<PhotonView>().ViewID);
        }
    }

    public void InitPlayer2()
    {
        // ���� ��Ʈ��ũ�� ����
        if (photonView.IsMine && rightHand != null)
        {
            GameObject gun = PhotonNetwork.Instantiate("Pistol", rightHand.position, rightHand.rotation);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, gun.GetComponent<PhotonView>().ViewID);
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
                // �θ� ���� ��� ��ġ/ȸ���� ���󰡵��� ����
                Gun gunScript = gunView.GetComponent<Gun>();
                if (gunScript != null)
                {
                    gunScript.SetTargetHand(hand, rig);
                }
            }
        }
    }

    private void TryPickupItem()
    {
        if (heldItem != null) return; // �̹� ��� ����

        Collider[] colliders = Physics.OverlapSphere(leftHand.position, itemGrabDistance);
        foreach (var col in colliders)
        {
            Item item = col.GetComponent<Item>();
            if (item != null)
            {
                heldItem = item;               
                item.AttachToLeftHand(photonView.ViewID); // �� ViewID�� �ѱ�
                break; // ù ��° �����۸� ����
            }
        }
    }

    private void TryDropItem()
    {
        if (heldItem == null) return;

        heldItem.DetachFromHand(); // ���� �θ� ����
        heldItem = null;
    }

    public void StartAmmoSpawn()
    {
        if (ammoSpanwer != null)
        {
            ammoSpanwer.ItemSpawnStart();
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