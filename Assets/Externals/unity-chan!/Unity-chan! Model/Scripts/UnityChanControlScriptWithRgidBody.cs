//
// Mecanim의 애니메이션 데이터가, 원점에서 이동하지 않을 경우의 Rigidbody를 사용한 컨트롤러
// 샘플
// 2014/03/13 N.Kobyasahi
//
using UnityEngine;
using System.Collections;

namespace UnityChan
{
    // 필요한 컴포넌트 나열
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]

    public class UnityChanControlScriptWithRgidBody : MonoBehaviour
    {

        public float animSpeed = 1.5f;              // 애니메이션 재생 속도 설정
        public float lookSmoother = 3.0f;           // 카메라 모션의 부드러움 설정
        public bool useCurves = true;               // Mecanim에서 커브 조정을 사용할지 설정
                                                    // 이 스위치가 꺼져 있으면 커브는 사용되지 않음
        public float useCurvesHeight = 0.5f;        // 커브 보정의 유효 높이 (지면을 뚫고 지나가기 쉬울 때는 높게 설정)

        // 이하 캐릭터 컨트롤러용 파라미터
        // 전진 속도
        public float forwardSpeed = 7.0f;
        // 후진 속도
        public float backwardSpeed = 2.0f;
        // 회전 속도
        public float rotateSpeed = 2.0f;
        // 점프 힘
        public float jumpPower = 3.0f;
        // 캐릭터 컨트롤러 (캡슐 콜라이더) 참조
        private CapsuleCollider col;
        private Rigidbody rb;
        // 캐릭터 컨트롤러 (캡슐 콜라이더)의 이동량
        private Vector3 velocity;
        // CapsuleCollider에 설정된 콜라이더의 Height, Center 초기값을 저장하는 변수
        private float orgColHight;
        private Vector3 orgVectColCenter;
        private Animator anim;                          // 캐릭터에 부착된 애니메이터 참조
        private AnimatorStateInfo currentBaseState;         // base layer에서 사용되는 애니메이터의 현재 상태 참조

        private GameObject cameraObject;    // 메인 카메라 참조

        // 애니메이터 각 스테이트 참조
        static int idleState = Animator.StringToHash("Base Layer.Idle");
        static int locoState = Animator.StringToHash("Base Layer.Locomotion");
        static int jumpState = Animator.StringToHash("Base Layer.Jump");
        static int restState = Animator.StringToHash("Base Layer.Rest");

        // 초기화
        void Start()
        {
            // Animator 컴포넌트를 얻는다
            anim = GetComponent<Animator>();
            // CapsuleCollider 컴포넌트를 얻는다 (캡슐형 콜리전)
            col = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();
            // 메인 카메라를 얻는다
            cameraObject = GameObject.FindWithTag("MainCamera");
            // CapsuleCollider 컴포넌트의 Height, Center 초기값을 저장한다
            orgColHight = col.height;
            orgVectColCenter = col.center;
        }


        // 이하, 메인 처리. 리지드바디와 관련되므로, FixedUpdate 내에서 처리한다.
        void FixedUpdate()
        {
            float h = Input.GetAxis("Horizontal");              // 입력 장치의 수평 축을 h로 정의
            float v = Input.GetAxis("Vertical");                // 입력 장치의 수직 축을 v로 정의
            anim.SetFloat("Speed", v);                          // Animator에서 설정된 "Speed" 파라미터에 v를 전달
            anim.SetFloat("Direction", h);                      // Animator에서 설정된 "Direction" 파라미터에 h를 전달
            anim.speed = animSpeed;                             // Animator의 모션 재생 속도에 animSpeed를 설정
            currentBaseState = anim.GetCurrentAnimatorStateInfo(0); // 참조용 상태 변수에 Base Layer (0)의 현재 상태를 설정
            rb.useGravity = true; // 점프 중에 중력을 끄므로, 그 외에는 중력의 영향을 받도록 설정



            // 이하, 캐릭터의 이동 처리
            velocity = new Vector3(0, 0, v);        // 상하 키 입력에서 Z축 방향의 이동량을 얻음
                                                    // 캐릭터의 로컬 공간에서의 방향으로 변환
            velocity = transform.TransformDirection(velocity);
            // 이하의 v의 임계값은, Mecanim 측의 트랜지션과 함께 조정
            if (v > 0.1)
            {
                velocity *= forwardSpeed;       // 이동 속도를 곱함
            }
            else if (v < -0.1)
            {
                velocity *= backwardSpeed;  // 이동 속도를 곱함
            }

            if (Input.GetButtonDown("Jump"))
            {   // 스페이스 키를 입력하면

                // 애니메이션 상태가 Locomotion 중일 때만 점프 가능
                if (currentBaseState.nameHash == locoState)
                {
                    // 상태 전환 중이 아닐 때만 점프 가능
                    if (!anim.IsInTransition(0))
                    {
                        rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                        anim.SetBool("Jump", true);     // Animator에 점프로 전환하는 플래그를 보냄
                    }
                }
            }


            // 상하 키 입력으로 캐릭터를 이동시킴
            transform.localPosition += velocity * Time.fixedDeltaTime;

            // 좌우 키 입력으로 캐릭터를 Y축으로 회전시킴
            transform.Rotate(0, h * rotateSpeed, 0);


            // 이하, Animator의 각 스테이트 중에서의 처리
            // Locomotion 중
            // 현재의 베이스 레이어가 locoState일 때
            if (currentBaseState.nameHash == locoState)
            {
                // 커브로 콜라이더 조정을 할 때는, 만약을 위해 리셋
                if (useCurves)
                {
                    resetCollider();
                }
            }
            // JUMP 중의 처리
            // 현재의 베이스 레이어가 jumpState일 때
            else if (currentBaseState.nameHash == jumpState)
            {
                cameraObject.SendMessage("setCameraPositionJumpView");  // 점프 중의 카메라로 변경
                                                                        // 상태가 트랜지션 중이 아닐 경우
                if (!anim.IsInTransition(0))
                {

                    // 이하, 커브 조정을 하는 경우의 처리
                    if (useCurves)
                    {
                        // 이하 JUMP00 애니메이션에 있는 커브 JumpHeight와 GravityControl
                        // JumpHeight: JUMP00에서의 점프 높이 (0~1)
                        // GravityControl: 1⇒점프 중 (중력 무효), 0⇒중력 유효
                        float jumpHeight = anim.GetFloat("JumpHeight");
                        float gravityControl = anim.GetFloat("GravityControl");
                        if (gravityControl > 0)
                            rb.useGravity = false;  // 점프 중 중력의 영향을 끔

                        // 레이캐스트를 캐릭터의 중심에서 떨어뜨림
                        Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
                        RaycastHit hitInfo = new RaycastHit();
                        // 높이가 useCurvesHeight 이상일 때만, 콜라이더의 높이와 중심을 JUMP00 애니메이션의 커브로 조정
                        if (Physics.Raycast(ray, out hitInfo))
                        {
                            if (hitInfo.distance > useCurvesHeight)
                            {
                                col.height = orgColHight - jumpHeight;          // 조정된 콜라이더의 높이
                                float adjCenterY = orgVectColCenter.y + jumpHeight;
                                col.center = new Vector3(0, adjCenterY, 0); // 조정된 콜라이더의 중심
                            }
                            else
                            {
                                // 임계값보다 낮을 때는 초기값으로 되돌림 (만약을 위해)					
                                resetCollider();
                            }
                        }
                    }
                    // Jump bool 값을 리셋 (루프되지 않도록)				
                    anim.SetBool("Jump", false);
                }
            }
            // IDLE 중의 처리
            // 현재의 베이스 레이어가 idleState일 때
            else if (currentBaseState.nameHash == idleState)
            {
                // 커브로 콜라이더 조정을 할 때는, 만약을 위해 리셋
                if (useCurves)
                {
                    resetCollider();
                }
                // 스페이스 키를 입력하면 Rest 상태로 전환
                if (Input.GetButtonDown("Jump"))
                {
                    anim.SetBool("Rest", true);
                }
            }
            // REST 중의 처리
            // 현재의 베이스 레이어가 restState일 때
            else if (currentBaseState.nameHash == restState)
            {
                //cameraObject.SendMessage("setCameraPositionFrontView");		// 카메라를 정면으로 전환
                // 상태가 전환 중이 아닐 때, Rest bool 값을 리셋 (루프되지 않도록)
                if (!anim.IsInTransition(0))
                {
                    anim.SetBool("Rest", false);
                }
            }
        }

        void OnGUI()
        {
            GUI.Box(new Rect(Screen.width - 260, 10, 250, 150), "Interaction");
            GUI.Label(new Rect(Screen.width - 245, 30, 250, 30), "Up/Down Arrow : 전진/후진");
            GUI.Label(new Rect(Screen.width - 245, 50, 250, 30), "Left/Right Arrow : 좌회전/우회전");
            GUI.Label(new Rect(Screen.width - 245, 70, 250, 30), "Running 중 Space key : 점프");
            GUI.Label(new Rect(Screen.width - 245, 90, 250, 30), "Stopping 중 Space key : 휴식");
            GUI.Label(new Rect(Screen.width - 245, 110, 250, 30), "Left Control : 정면 카메라");
            GUI.Label(new Rect(Screen.width - 245, 130, 250, 30), "Alt : 바라보기 카메라");
        }


        // 캐릭터의 콜라이더 크기 리셋 함수
        void resetCollider()
        {
            // 컴포넌트의 Height, Center 초기값으로 되돌림
            col.height = orgColHight;
            col.center = orgVectColCenter;
        }
    }
}
