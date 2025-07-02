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
            if (rig != null)
                rig.gameObject.SetActive(false);
            return;
        }

        if (rig != null)
        {
            rightHand = rig.rightHandAnchor;
            leftHand = rig.leftHandAnchor;
            hmd = rig.centerEyeAnchor;
        }

        // 내 플레이어일 때만 총 생성
        if (photonView.IsMine && rightHand != null && gunPrefab != null)
        {
            GameObject gunInstance = Instantiate(gunPrefab, rightHand); // 부모로 붙이기
            gunInstance.transform.localPosition = Vector3.zero;
            gunInstance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("총 생성 실패: rightHand 또는 gunPrefab이 null입니다.");
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