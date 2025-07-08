using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f; //이동속도
    CharacterController cc;
    public float jumpPower = 5; // 점프 파워

     bool isJumping = false;

    public float gravity = -20f; // 중력 가속도
    float yVelocity = 0; // 수직 속도

    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        //사용자 입력을 받는다
        float h = ARAVRInput.GetAxis("Horizontal");
        float v = ARAVRInput.GetAxis("Vertical");

        //방향을 정한다
        Vector3 dir = new Vector3(h, 0, v);
        //사용자가 바라보는 방향으로 전환
        dir = Camera.main.transform.TransformDirection(dir);

        //중력을 적용한 수직 방향 추가
        yVelocity += gravity * Time.deltaTime;
     

        //if(cc.collisionFlags == CollisionFlags.Below)
        //{
            //CollisionFlag : 어디에 붙어있는 지 파악이 가능함
        //}

        if (cc.isGrounded) // 바닥에 있을 경우, y속도를 0으로..
        {
            yVelocity = 0;
            isJumping = true;
        }
        if(ARAVRInput.GetDown(ARAVRInput.Button.Two,
            ARAVRInput.Controller.RTouch) && isJumping)
        {
            yVelocity = jumpPower;
            isJumping= false;
        }

      //print("중력값:" + yVelocity); 
        dir.y = yVelocity;

     //이동을 한다.
        cc.Move(dir *  speed * Time.deltaTime); 
    }
}
