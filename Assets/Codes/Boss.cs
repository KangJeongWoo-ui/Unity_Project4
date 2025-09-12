using System.Collections;
using System.Xml.Serialization;
using UnityEngine;

public class Boss : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer sr;
    public enum BossType { Boss0, Boss1, Boss2 }
    public BossType bossType;

    [Header("Boss")]
    public float hp; // 보스 체력
    public float speed; // 보스 이동 속도
    public GameObject[] bossBullet; // 보스 총알
    void Start()
    {
        rb.linearVelocity = Vector2.down * speed;
        StartCoroutine(MoveStopCoroutine()); // 보스가 생성후 내려오다가 3초후 정지
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        bossPattern();
        Dead();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet")) // 캐릭터 총알에 맞으면 캐릭터 총알은 삭제되고 적의 색을 빨간색으로 바꿔 피격 당한것을 표현
        {
            PlayerBullet playerbullet = collision.GetComponent<PlayerBullet>();
            hp -= playerbullet.damage;
            sr.color = Color.red;
            Damage();
            Destroy(collision.gameObject);
        }
    }
    void bossPattern() // 보스 패턴
    {
        switch(bossType)
        {
            case BossType.Boss0:
                break;
            case BossType.Boss1:
                break;
            case BossType.Boss2:
                break;
        }
    }
    void Damage() // 데미지를 받을때 빨간색으로 변한 적을 0.5초 뒤에 흰색으로 바꿔 피격당한 효과를 구현하는 코루틴
    {
        StartCoroutine(DamageCoroutine());
    }
    IEnumerator DamageCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }
    void Dead() // 보스 죽음
    {
        if(hp <= 0)
        {
            Destroy(gameObject);
        }
    }
    IEnumerator MoveStopCoroutine()
    {
        yield return new WaitForSeconds(3f);
        rb.linearVelocity = Vector2.zero;
    }
}
