using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCm : MonoBehaviour
{
        // Update is called once per frame
    void Update()
    {
        //������ ü�¹� Canvas�� ����ī�޶� �ٶ󺸵��� ����
        transform.LookAt(Camera.main.transform.position);
    }
}
