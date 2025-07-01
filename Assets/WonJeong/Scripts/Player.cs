using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Gun Prefab")]
    [SerializeField] private GameObject gunPrefab;

    private void Start()
    {
        // OVRCameraRig 내부의 RightHandAnchor를 찾아서 부착
        Transform rightHand = transform.Find("OVRCameraRig/TrackingSpace/RightHandAnchor");

        if (rightHand != null && gunPrefab != null)
        {
            GameObject gunInstance = Instantiate(gunPrefab, rightHand);
            gunInstance.transform.localPosition = Vector3.zero;
            gunInstance.transform.localRotation = Quaternion.identity;

            // 필요 시 위치/회전 미세 조정
            gunInstance.transform.localPosition += new Vector3(0.05f, -0.02f, 0.1f); // 원하는 위치로 조정
            gunInstance.transform.localEulerAngles = new Vector3(0, 90, 0); // 원하는 회전으로 조정
        }
        else
        {
            Debug.LogWarning("RightHandAnchor or gunPrefab is missing.");
        }
    }
    private void OnDrawGizmos()
    {
        Transform rightHand = transform.Find("OVRCameraRig/TrackingSpace/RightHandAnchor");
        Transform leftHand = transform.Find("OVRCameraRig/TrackingSpace/LeftHandAnchor");

        if (rightHand != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(rightHand.position, 0.02f); // 파란 원 (오른손)
        }

        if (leftHand != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leftHand.position, 0.02f); // 빨간 원 (왼손)
        }
    }

}
