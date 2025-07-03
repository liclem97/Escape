using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCm : MonoBehaviour
{
        // Update is called once per frame
    void Update()
    {
        //좀비의 체력바 Canvas가 메인카메라를 바라보도록 설정
        transform.LookAt(Camera.main.transform.position);
    }
}
