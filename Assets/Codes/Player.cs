using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;

    public GameObject playerBullet; // 캐릭터 총알
    public GameObject[] boomIcon; // 폭탄 아이콘
    public GameObject boomEffect; // 폭발 효과

    [Header("Player")]
    public int hp; // 캐릭터 체력
    public float speed; // 캐릭터 이동 속도
    public int powerLevel; // 캐릭터가 Power 아이템을 먹었을떄 올라가는 총알 레벨
    public int boomCount; // 폭탄 갯수
    public float fireRate; // 캐릭터 공격 속도
    float nextFire = 0f;
    int boomiconindex = 0;

    bool isDead; // 캐릭터가 죽었는지 확인
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        if(!isDead && Input.GetButton("Fire1") && Time.time > nextFire) // 죽었을때 공격 x, 마우스 왼쪽 클릭을 하면 공격
        {
            Fire();
            nextFire = Time.time + fireRate; // 다음 공격 시전을 현재 시간에서 공격 시간을 더해 딜레이를 줌
        }

        Boom();
        Dead();
    }
    void FixedUpdate()
    {
        Move();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) // 적에 닿을 경우 데미지를 받음
        {
            Damage();
        }
        else if(collision.CompareTag("Power")) // 파워 아이템을 먹으면 총알 레벨이 1 증가하고 아이템 삭제
        {
            powerLevel += 1;
            if(powerLevel >= 2)
            {
                powerLevel = 2;
            }
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Boom")) // 폭탄 아이템을 먹으면 현재 폭탄 갯수가 1 증가하고 아이템 삭제
        {
            boomCount += 1;
            if (boomCount >= 3)
            {
                boomCount = 3;
            }
            Destroy(collision.gameObject);

            if(boomiconindex < boomIcon.Length) // 폭탄 아이템을 먹으면 화면 아래에 폭탄 아이콘을 보이게 하여 현재 남은 폭탄 갯수 확인
            {
                boomIcon[boomiconindex].SetActive(true);
                boomiconindex++;
            }
        }
    }
    void Move() // 캐릭터 움직임
    {
        float x = Input.GetAxisRaw("Horizontal"); // 좌 우 움직임
        float y = Input.GetAxisRaw("Vertical"); // 상 하 움직임

        Vector2 moveVec = new Vector2(x, y).normalized; // normalized로 대각선 방향이 루트값이 되어 빨라지는거 방지

        rb.MovePosition(rb.position + moveVec * speed * Time.fixedDeltaTime);
    }
    void Fire() // 캐릭터 공격
    {
        switch(powerLevel)
        {
            case 0: // 총알 레벨이 0단계에서는 총알을 가운데에서 하나만 발사
                GameObject bullet = Instantiate(playerBullet, transform.position, transform.rotation);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                rb.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;
            case 1: // 총알 레벨이 1단계에서는 총알을 두개씩 발사
                GameObject bulletR = Instantiate(playerBullet, 
                    transform.position + new Vector3(0.15f, 0, 0), 
                    transform.rotation);
                Rigidbody2D rbR = bulletR.GetComponent<Rigidbody2D>();
                rbR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

                GameObject bulletL = Instantiate(playerBullet, 
                    transform.position + new Vector3(-0.15f, 0, 0), 
                    transform.rotation);
                Rigidbody2D rbL = bulletL.GetComponent<Rigidbody2D>();
                rbL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;
            case 2: // 총알 레벨이 2단계에서는 총알을 삼각형으로 발사
                GameObject bulletRR = Instantiate(playerBullet,
                    transform.position + new Vector3(0.15f, 0, 0),
                    transform.rotation);
                Rigidbody2D rbRR = bulletRR.GetComponent<Rigidbody2D>();
                rbRR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

                GameObject bulletMM = Instantiate(playerBullet,
                    transform.position + new Vector3(0, 0.25f, 0),
                    transform.rotation);
                Rigidbody2D rbMM = bulletMM.GetComponent<Rigidbody2D>();
                rbMM.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

                GameObject bulletLL = Instantiate(playerBullet,
                    transform.position + new Vector3(-0.15f, 0, 0),
                    transform.rotation);
                Rigidbody2D rbLL = bulletLL.GetComponent<Rigidbody2D>();
                rbLL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;
        }
    }
    void Boom() // 폭탄 발사
    {
        if(boomCount!=0 && Input.GetKeyDown("z")) // 현재 폭탄이 0개가 아닐떄 z 키를 누르면 0,0,0 좌표에서 폭발 효과를 생성하고 2초 뒤에 삭제하는 폭발 코루틴
        {
            boomCount--;
            if (boomiconindex > 0)
            {
                boomiconindex--;
                boomIcon[boomiconindex].SetActive(false);
            }
            StartCoroutine(BoomCoroutine());
        }
    }
    IEnumerator BoomCoroutine()
    {
        GameObject boomeffect = Instantiate(boomEffect, new Vector3(0, 0, 0), transform.rotation);
        yield return new WaitForSeconds(2f);
        Destroy(boomeffect);
    }
    void Damage() // 캐릭터가 데미지를 받음
    {
        hp -= 10; // 피격을 받으면 현재 채력 -10
        StartCoroutine(DamageCoroutine()); // 빨간색으로 바꾼후 0.5초 뒤에 다시 원래 색으로 바꾸는 코루틴
    }
    IEnumerator DamageCoroutine()
    {
        sr.color = Color.red; // 캐릭터의 색깔을 빨간색으로 바꿈
        yield return new WaitForSeconds(0.5f);
        sr.color = Color.white;
    }
    void Dead() // 캐릭터 죽음
    {
        if (hp <= 0) // 캐릭터 체력이 0이하 일때 죽음을 확인하는 isDead를 true로 바꿈
        {
            isDead = true;
            anim.SetBool("_IsDead", true);
            StartCoroutine(DeadCoroutine()); // 죽음 애니매이션을 재생한 후에 캐릭터를 삭제 시키는 코루틴
        }
    }
    IEnumerator DeadCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
