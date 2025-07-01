using Photon.Pun;
using UnityEngine;

public class VRPlayer : MonoBehaviourPun
{
    [Header("Gun Prefab")]
    [SerializeField] private GameObject gunPrefab;

    private Transform rightHand;
    private Transform leftHand;

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
        }

        if (rightHand != null && gunPrefab != null)
        {
            GameObject gunInstance = Instantiate(gunPrefab, rightHand);
            gunInstance.transform.localPosition = Vector3.zero;
            gunInstance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("RightHandAnchor or gunPrefab is missing.");
        }
    }

    private void OnDrawGizmos()
    {
        if (rightHand == null || leftHand == null)
        {
            var rig = GetComponentInChildren<OVRCameraRig>();
            if (rig != null)
            {
                rightHand = rig.rightHandAnchor;
                leftHand = rig.leftHandAnchor;
            }
        }

        if (rightHand != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(rightHand.position, 0.02f);
        }

        if (leftHand != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leftHand.position, 0.02f);
        }
    }
}