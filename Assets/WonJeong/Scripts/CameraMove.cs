using Photon.Pun;
using System.Collections;
using UnityEngine;

/*  플레이어1과 플레이어2를 다음 지점으로 이동시키는 스크립트    */
public class CameraMove : MonoBehaviourPun
{
    public static CameraMove Instance { get; private set; }
    public enum PlayerType { Player1, Player2 }

    [SerializeField] private GameObject vehicle;    // 플레이어2가 타고다니는 헬리콥터

    public GameObject Vehicle
    {
        get => vehicle;
        set => vehicle = value;
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

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: MoveToPosition
    * 기능: 플레이어를 다음 포인트로 서서히 이동시킴
    ***********************************************************************************/
    public IEnumerator MoveToPosition(GameObject player, Transform movePoint, float moveTime)
    {
        // 이동 전 모든 RayVisualizer 비활성화
        RayVisualizer[] rayVisualizers = player.GetComponentsInChildren<RayVisualizer>();
        foreach (var ray in rayVisualizers)
        {
            ray.Off();
        }
        // 시작 지점
        Vector3 startPos = player.transform.position;
        Quaternion startRot = player.transform.rotation;

        // 도착 지점
        Vector3 targetPos = movePoint.position;
        Quaternion targetRot = movePoint.rotation;

        float elapsed = 0f;
        float duration = moveTime;

        // lerp로 서서히 이동
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

        //이동이 끝난 후 RayVisualizer 다시 활성화
        foreach (var ray in rayVisualizers)
        {
            ray.On();
        }
    }
}