using Photon.Pun;
using System.Collections;
using UnityEngine;

[System.Serializable]
public struct MoveStruct
{
    public Transform movePoint;
    public float moveTime;
}

public class CameraMove : MonoBehaviourPun
{
    public static CameraMove Instance { get; private set; }
    public enum PlayerType { Player1, Player2 }

    [SerializeField] private MoveStruct[] movePointsPlayer1;
    [SerializeField] private MoveStruct[] movePointsPlayer2;

    [SerializeField] private GameObject vehicle;

    public GameObject Vehicle
    {
        get => vehicle;
        set => vehicle = value;
    }

    private void Awake()
    {
        // ΩÃ±€≈Ê √ ±‚»≠
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator MoveToPosition(GameObject player, Transform movePoint, float moveTime)
    {
        Vector3 startPos = player.transform.position;
        Quaternion startRot = player.transform.rotation;

        Vector3 targetPos = movePoint.position;
        Quaternion targetRot = movePoint.rotation;

        float elapsed = 0f;
        float duration = moveTime;

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
    }
}