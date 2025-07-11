using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public static VRPlayerHealth player1Health; // �÷��̾�1�� ü�� ����
    public static float sharedHealthPercent = 1f;
    public static bool isFadingOut = false;
    public static bool isFadingIn = false;

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    [Header("Stage Start Point")]
    [SerializeField] private Transform player1Stage1Point;
    [SerializeField] private Transform player2Stage1Point;

    [Header("Managers")]
    [SerializeField] private CameraMove cameraMoveManager;

    [Header("Players (Runtime Assigned)")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    [Header("Item Spawner")]
    [SerializeField] private ItemSpawner healPackItemSpawner;

    [Header("Move Points")]
    [SerializeField] private Transform gameoverMovePoint;
    [SerializeField] private Transform gameclearMovePoint;

    [Header("Game information UI")]
    [SerializeField] private GameObject beforeStartUI;
    [SerializeField] private Text beforeStartUI_Text;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameClearUI;

    [Header("Sound")]
    [SerializeField] private AudioClip gamePlayBGM;
    [SerializeField] private AudioClip bossBGM;

    public GameObject GetPlayer1() => player1;
    public GameObject GetPlayer2() => player2;    

    private bool isGameOver = false;
    private bool isGameClear = false;

    public bool IsGameOver
    {
        get => isGameOver;
    }

    public bool IsGameClear
    {
        get => isGameClear;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // PhotonView ������Ʈ�� �ʼ�
        if (photonView == null)
        {
            Debug.LogError("[GameManager] PhotonView ������Ʈ�� �����ϴ�.");
        }
    }

    private void Start()
    {
        Debug.Log($"VRPlayer Start() - IsMine: {photonView.IsMine}");
        if (gameOverUI) gameOverUI.SetActive(false);
        if (gameClearUI) gameClearUI.SetActive(false);
    }

    private void Update()
    {
        if (!isGameOver && player1Health != null && sharedHealthPercent <= 0f)
        {
            isGameOver = true;

            photonView.RPC(nameof(RPC_SetIsFadeOut), RpcTarget.AllBuffered, true);

            OnGameOver();
        }
    }

    private void OnGameStart()
    {
        if (player1 == null || player2 == null) return;

        // ���� ���� �� ó�� ����Ʈ�� �ڵ� �̵�
        if (player1Stage1Point != null && player2Stage1Point != null)
        {
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(player1, player1Stage1Point, 2f));
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(CameraMove.Instance.Vehicle, player2Stage1Point, 2f));
        }

        // ������ ���� ���� �� ���� ��ǥ �÷��̾� ����
        ItemTargetTrigger trigger = FindFirstObjectByType<ItemTargetTrigger>();
        if (trigger != null && healPackItemSpawner)
        {
            trigger.FlyTarget = player1.transform;
            healPackItemSpawner.ItemSpawnStart();
        }

        // �÷��̾�1 ������ źâ ���� ����
        VRPlayer player = player1.GetComponent<VRPlayer>();
        if (player != null)
        {
            player.StartAmmoSpawn();
        }

        if (BGMPlayer.Instance != null && gamePlayBGM != null)
        {
            BGMPlayer.Instance.PlayBGM(gamePlayBGM);
        }
    }

    [PunRPC]
    void RPC_SetIsFadeOut(bool shoudFadeOut)
    {
        isFadingIn = !shoudFadeOut;
        isFadingOut = shoudFadeOut;
    }

    [PunRPC]
    private void RPC_SetActiveGameClearUI()
    {
        if (gameClearUI) gameClearUI.SetActive(true);
    }

    [PunRPC]
    private void RPC_SetActiveGameOverUI()
    {
        if (gameOverUI) gameOverUI.SetActive(true);
    }

    private void OnGameOver()
    {
        Debug.Log("[GameManager] ���� ����!");

        // ���ӿ��� �������� �̵� ����
        photonView.RPC(nameof(RPC_DelayAndMovePlayers), RpcTarget.AllBuffered, 2.5f, gameoverMovePoint.position, gameoverMovePoint.rotation);
        //StartCoroutine(DelayAndMovePlayers(2.5f, gameoverMovePoint));
        DisableAllZombieSpawners();
        DisableAllEnemyBases();

        Debug.Log("gameover rpc ȣ��");
        photonView.RPC(nameof(RPC_SetActiveGameOverUI), RpcTarget.AllBuffered);
    }

    public void OnGameClear()
    {
        Debug.Log("[GameManager] ���� Ŭ����!");

        photonView.RPC(nameof(RPC_SetIsFadeOut), RpcTarget.AllBuffered, true);
        photonView.RPC(nameof(RPC_DelayAndMovePlayers), RpcTarget.AllBuffered, 3f, gameclearMovePoint.position, gameclearMovePoint.rotation);
        // Ŭ���� �������� �̵� ����
        DisableAllZombieSpawners();
        KillAllZombies();

        Debug.Log("gameclear rpc ȣ��");
        photonView.RPC(nameof(RPC_SetActiveGameClearUI), RpcTarget.AllBuffered);
    }

    private void DisableAllEnemyBases()
    {
        var enemyBases = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var enemy in enemyBases)
        {
            enemy.gameObject.SetActive(false);
        }
    }

    private void KillAllZombies()
    {
        // ���� �ִ� ��� EnemyBase ������Ʈ�� ���� ���� ã��
        EnemyBase[] zombies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

        int myViewID = photonView.ViewID; // GameManager�� PhotonView ID

        foreach (EnemyBase zombie in zombies)
        {
            // ���� ����� ����
            if (zombie == null || zombie.gameObject == null) continue;

            // ������ ū �������� ��
            zombie.TakeDamage(9999f, myViewID);
        }

        Debug.Log("[GameManager] ��� ���� ������");
    }

    private void DisableAllZombieSpawners()
    {
        ZombieSpawner[] zSpawners = FindObjectsByType<ZombieSpawner>(FindObjectsSortMode.None);
        foreach (ZombieSpawner spawner in zSpawners)
        {
            spawner.enabled = false;
        }
    }

    [PunRPC]
    private void RPC_DelayAndMovePlayers(float delay, Vector3 movePos, Quaternion moveRot)
    {
        StartCoroutine(DelayAndMovePlayers(delay, movePos, moveRot));
    }

    private IEnumerator DelayAndMovePlayers(float delay, Vector3 movePos, Quaternion moveRot)
    {
        if (movePos == null) yield break;
        yield return new WaitForSeconds(delay);

        float moveDuration = 0.5f;

        if (player1 != null)
        {
            yield return StartCoroutine(MoveToPosition(player1.transform, movePos, moveRot, moveDuration));
        }

        if (player2 != null)
        {
            yield return StartCoroutine(MoveToPosition(player2.transform, movePos, moveRot, moveDuration));
        }

        photonView.RPC(nameof(RPC_SetIsFadeOut), RpcTarget.AllBuffered, false);
        //TriggerFadeInAllPlayers();
    }

    private IEnumerator MoveToPosition(Transform target, Vector3 endPos, Quaternion endRot, float duration)
    {
        Vector3 startPos = target.position;
        Quaternion startRot = target.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.position = Vector3.Lerp(startPos, endPos, t);
            target.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        target.position = endPos;
        target.rotation = endRot;

        photonView.RPC(nameof(RPC_SetIsFadeOut), RpcTarget.AllBuffered, false);
        //TriggerFadeInAllPlayers();
    }

    //private void TriggerFadeInAllPlayers()
    //{
    //    if (player1 != null && player2 != null)
    //    {

    //        isFadingIn = true;
    //    }
    //}

    public void OnPlayerJoinedRoom()
    {
        string prefabName = "VRPlayer";

        Transform spawn = PhotonNetwork.CurrentRoom.PlayerCount == 1
            ? player1SpawnPoint
            : player2SpawnPoint;

        if (spawn == null)
        {
            Debug.LogError("[GM] Spawn point is null! Cannot instantiate player.");
            return;
        }

        // �÷��̾� ����
        GameObject player = PhotonNetwork.Instantiate(prefabName, spawn.position, spawn.rotation);

        // �г������� �̸� ���� (���ÿ����� �����)   
        player.name = PhotonNetwork.NickName;

        // ��� Ŭ���̾�Ʈ�� �÷��̾� ���Ҵ� ��� ����
        photonView.RPC(nameof(RPC_AssignPlayers), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_AssignPlayers()
    {
        StartCoroutine(AssignPlayersAfterDelay());
    }

    private IEnumerator AssignPlayersAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        AssignPlayers();
    }

    private void AssignPlayers()
    {
        foreach (var view in FindObjectsByType<PhotonView>(FindObjectsSortMode.None))
        {
            if (view.CompareTag("Player"))
            {
                int actorNumber = view.Owner.ActorNumber;

                if (actorNumber == 1 && player1 == null)
                {
                    player1 = view.gameObject;

                    // InitPlayer1 ȣ��
                    VRPlayer vrPlayer = player1.GetComponent<VRPlayer>();
                    vrPlayer?.InitPlayer1();
                }
                else if (actorNumber == 2 && player2 == null)
                {
                    player2 = view.gameObject;
                    if (CameraMove.Instance != null)
                    {
                        //cameraMoveManager.Player2 = player2;

                        if (CameraMove.Instance.Vehicle != null)
                        {
                            Transform attachPoint = CameraMove.Instance.Vehicle.transform.Find("AttachPoint");
                            if (attachPoint != null)
                            {
                                player2.transform.SetParent(attachPoint);
                                player2.transform.localPosition = Vector3.zero;
                                player2.transform.localRotation = Quaternion.identity;
                            }
                        }
                    }
                    // InitPlayer2 ȣ��
                    VRPlayer vrPlayer = player2.GetComponent<VRPlayer>();
                    vrPlayer?.InitPlayer2();
                }
            }
        }

        string p1Name = player1 != null ? player1.name : "null";
        string p2Name = player2 != null ? player2.name : "null";
        Debug.Log($"[GameManager] Assigned Player1: {p1Name}, Player2: {p2Name}");

        if (player1 != null)
        {
            player1Health = player1.GetComponent<VRPlayerHealth>();
            if (player1Health != null)
            {
                player1Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, false); // Player1 ���� ����
            }
        }

        if (player2 != null)
        {
            VRPlayerHealth p2Health = player2.GetComponent<VRPlayerHealth>();
            if (p2Health != null)
            {
                p2Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, true); // Player2 ���� ����
            }
        }

        if (player1 != null && player2 != null)
        {
            //Debug.Log("[GM] �� �÷��̾ �Ҵ��. ���� ����");
            //OnGameStart();

            photonView.RPC(nameof(RPC_ShowBeforeStartText), RpcTarget.All, "Start Game Soon...");

            // ���� ���� ���� �� ���� ����
            StartCoroutine(DelayedGameStart(2.5f));
        }
    }

    [PunRPC]
    private void RPC_ShowBeforeStartText(string message)
    {
        if (beforeStartUI_Text != null && beforeStartUI != null)
        {
            beforeStartUI_Text.text = message;
            beforeStartUI.SetActive(true);
        }
    }

    private IEnumerator DelayedGameStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        // UI ����
        beforeStartUI.SetActive(false);

        OnGameStart();
    }

    [PunRPC]
    public void RPC_SetSharedHealthPercent(float value)
    {
        sharedHealthPercent = value;
        Debug.Log($"[GameManager] sharedHealthPercent ���ŵ�: {value}");
    }

    public void PlayBossBGM()
    {
        if (BGMPlayer.Instance != null && bossBGM != null)
        {
            BGMPlayer.Instance.PlayBGM(bossBGM);
        }
    }
}