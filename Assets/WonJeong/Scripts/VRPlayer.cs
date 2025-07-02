using Photon.Pun;
using UnityEngine;

public class VRPlayer : MonoBehaviourPun
{
    [Header("Gun Prefab")]
    [SerializeField] private GameObject gunPrefab;

    private Transform rightHand;
    private Transform leftHand;
    private Transform hmd;

    private void Start()
    {
        var rig = GetComponentInChildren<OVRCameraRig>();

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

        // ���� ��Ʈ��ũ�� ����
        if (photonView.IsMine && rightHand != null && gunPrefab != null)
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
                    gunScript.SetTargetHand(hand);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (rightHand == null || leftHand == null || hmd == null)
        {
            var rig = GetComponentInChildren<OVRCameraRig>();
            if (rig != null)
            {
                rightHand = rig.rightHandAnchor;
                leftHand = rig.leftHandAnchor;
                hmd = rig.centerEyeAnchor;
            }
        }

        if (rightHand != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(rightHand.position, 0.05f);
        }

        if (leftHand != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leftHand.position, 0.05f);
        }

        if (hmd != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hmd.position, 0.05f);
        }
    }
}