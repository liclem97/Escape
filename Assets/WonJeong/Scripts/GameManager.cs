using Photon.Pun;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public static VRPlayerHealth player1Health; // 플레이어1의 체력 참조
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

        // PhotonView 컴포넌트가 필수
        if (photonView == null)
        {
            Debug.LogError("[GameManager] PhotonView 컴포넌트가 없습니다.");
        }
    }

    private void Start()
    {
        Debug.Log($"VRPlayer Start() - IsMine: {photonView.IsMine}");
    }

    private void Update()
    {
        if (!isGameOver && player1Health != null && sharedHealthPercent <= 0f)
        {
            isGameOver = true;
            isFadingOut = true;
            OnGameOver();
        }
    }

    private void OnGameStart()
    {
        if (player1 == null || player2 == null) return;

        // 게임 시작 후 처음 포인트로 자동 이동
        if (player1Stage1Point != null && player2Stage1Point != null)
        {
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(player1, player1Stage1Point, 2f));
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(CameraMove.Instance.Vehicle, player2Stage1Point, 2f));
        }        

        // 아이템 스폰 시작 및 힐팩 목표 플레이어 지정
        ItemTargetTrigger trigger = FindFirstObjectByType<ItemTargetTrigger>();        
        if (trigger != null && healPackItemSpawner)
        {
            trigger.FlyTarget = player1.transform;
            healPackItemSpawner.ItemSpawnStart();           
        }

        // 플레이어1 리볼버 탄창 스폰 시작
        VRPlayer player = player1.GetComponent<VRPlayer>();
        if (player != null)
        {
            player.StartAmmoSpawn();
        }
    }

    private void OnGameOver()
    {
        Debug.Log("[GameManager] 게임 오버!");

        // 게임오버 지점으로 이동 시작
        StartCoroutine(DelayAndMovePlayers(2.5f, gameoverMovePoint));
        DisableAllZombieSpawners();
        DisableAllEnemyBases();
    }

    private void OnGameClear()
    {
        Debug.Log("[GameManager] 게임 클리어!");

        // 클리어 지점으로 이동 시작
        DisableAllZombieSpawners();
        KillAllZombies();
        StartCoroutine(DelayAndMovePlayers(3f, gameclearMovePoint));        
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
        // 씬에 있는 모든 EnemyBase 컴포넌트를 가진 좀비를 찾음
        EnemyBase[] zombies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);        

        int myViewID = photonView.ViewID; // GameManager의 PhotonView ID

        foreach (EnemyBase zombie in zombies)
        {
            // 죽은 좀비는 무시
            if (zombie == null || zombie.gameObject == null) continue;

            // 강제로 큰 데미지를 줌
            zombie.TakeDamage(9999f, myViewID);
        }

        Debug.Log("[GameManager] 모든 좀비를 제거함");
    }

    private void DisableAllZombieSpawners()
    {
        ZombieSpawner[] zSpawners = FindObjectsByType<ZombieSpawner>(FindObjectsSortMode.None);
        foreach (ZombieSpawner spawner in zSpawners)
        {
            spawner.enabled = false;
        }
    }

    private IEnumerator DelayAndMovePlayers(float delay, Transform movePoint)
    {
        if (movePoint == null) yield break;
        yield return new WaitForSeconds(delay);

        float moveDuration = 1.5f;

        if (player1 != null)
        {
            yield return StartCoroutine(MoveToPosition(player1.transform, movePoint.position, movePoint.rotation, moveDuration));
        }

        if (player2 != null)
        {
            yield return StartCoroutine(MoveToPosition(player2.transform, movePoint.position, movePoint.rotation, moveDuration));
        }

        TriggerFadeInAllPlayers();
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

        TriggerFadeInAllPlayers();
    }

    private void TriggerFadeInAllPlayers()
    {
        if (player1 != null && player2 != null)
        {
            isFadingIn = true;            
        }
    }


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

        // 플레이어 생성
        GameObject player = PhotonNetwork.Instantiate(prefabName, spawn.position, spawn.rotation);

        // 닉네임으로 이름 설정 (로컬에서만 적용됨)   
        player.name = PhotonNetwork.NickName;

        // 모든 클라이언트에 플레이어 재할당 명령 전달
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

                    // InitPlayer1 호출
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
                    // InitPlayer2 호출
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
                player1Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, false); // Player1 무적 해제
            }
        }

        if (player2 != null)
        {
            VRPlayerHealth p2Health = player2.GetComponent<VRPlayerHealth>();
            if (p2Health != null)
            {
                p2Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, true); // Player2 무적 설정
            }
        }

        if (player1 != null && player2 != null)
        {
            Debug.Log("[GM] 두 플레이어가 할당됨. 게임 시작");
            OnGameStart();
        }
    }

    [PunRPC]
    public void RPC_SetSharedHealthPercent(float value)
    {
        sharedHealthPercent = value;
        Debug.Log($"[GameManager] sharedHealthPercent 갱신됨: {value}");
    }
}