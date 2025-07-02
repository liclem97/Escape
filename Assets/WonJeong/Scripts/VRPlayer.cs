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

        // 총은 네트워크로 스폰
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
                // 부모 설정 대신 위치/회전만 따라가도록 설정
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