using System.Collections;
using UnityEngine;

[System.Serializable]
public struct MoveStruct
{
    public Transform movePoint;
    public float moveTime;
}

public class CameraMove : MonoBehaviour
{
    [SerializeField] private MoveStruct[] movePoints;
    [SerializeField] private GameObject player;

    private bool isMoving = false;
    private int currentIndex = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isMoving)
        {
            StartCoroutine(MoveToPosition(movePoints[currentIndex]));

            // 다음 인덱스로 순환 (0 → 1 → 2 → 0 → ...)
            currentIndex = (currentIndex + 1) % movePoints.Length;
        }
    }

    private IEnumerator MoveToPosition(MoveStruct targetStruct)
    {
        isMoving = true;

        Vector3 startPos = player.transform.position;
        Quaternion startRot = player.transform.rotation;

        Vector3 targetPos = targetStruct.movePoint.position;
        Quaternion targetRot = targetStruct.movePoint.rotation;

        float elapsed = 0f;
        float duration = targetStruct.moveTime;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            player.transform.position = Vector3.Lerp(startPos, targetPos, t);
            player.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        player.transform.position = targetPos;
        player.transform.rotation = targetRot;

        isMoving = false;
    }
}