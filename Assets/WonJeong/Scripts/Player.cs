using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Gun Prefab")]
    [SerializeField] private GameObject gunPrefab;

    private void Start()
    {
        // OVRCameraRig ������ RightHandAnchor�� ã�Ƽ� ����
        Transform rightHand = transform.Find("OVRCameraRig/TrackingSpace/RightHandAnchor");

        if (rightHand != null && gunPrefab != null)
        {
            GameObject gunInstance = Instantiate(gunPrefab, rightHand);
            gunInstance.transform.localPosition = Vector3.zero;
            gunInstance.transform.localRotation = Quaternion.identity;

            // �ʿ� �� ��ġ/ȸ�� �̼� ����
            gunInstance.transform.localPosition += new Vector3(0.05f, -0.02f, 0.1f); // ���ϴ� ��ġ�� ����
            gunInstance.transform.localEulerAngles = new Vector3(0, 90, 0); // ���ϴ� ȸ������ ����
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
            Gizmos.DrawSphere(rightHand.position, 0.02f); // �Ķ� �� (������)
        }

        if (leftHand != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leftHand.position, 0.02f); // ���� �� (�޼�)
        }
    }

}
