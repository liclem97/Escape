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

    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    private int index1 = 0;
    private int index2 = 0;

    private bool isMoving = false;
    private int currentIndex = 0;

    public GameObject Player1
    {
        get => player1;
        set => player1 = value;
    }

    public GameObject Player2
    {
        get => player2;
        set => player2 = value;
    }

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        // 1P, 2P가 모두 있어야 동작함
        if (player1 == null && player2 == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 모든 클라이언트에게 MovePlayers RPC 호출
            photonView.RPC(nameof(RPC_MovePlayers), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_MovePlayers()
    {
        if (player1 == null || player2 == null) return;

        StartCoroutine(MoveToPosition(player1, movePointsPlayer1[index1]));
        StartCoroutine(MoveToPosition(player2, movePointsPlayer2[index2]));

        index1 = (index1 + 1) % movePointsPlayer1.Length;
        index2 = (index2 + 1) % movePointsPlayer2.Length;
    }

    private IEnumerator MoveToPosition(GameObject player, MoveStruct targetStruct)
    {
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
    }
}