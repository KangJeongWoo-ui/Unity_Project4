using UnityEngine;

public class BackGround : MonoBehaviour
{
    [Header("BackGround")]
    public float speed; // 배경 이동 속도
    public Transform target; // 배경이 이동할 다음 위치의 기준점

    Vector3 moveVec = Vector3.down;
    void Update()
    {
        transform.position += moveVec * speed * Time.deltaTime; // 배경 이동 속도만큼 아래로 이동

        if(transform.position.y < -12) // 배경의 y값 위치가 -12를 넘어가면 기준점 기준 12.2칸 위에 이동하여 무한 스크롤 구현
        {
            transform.position = target.position + Vector3.up * 12.2f;
        }
    }
}
