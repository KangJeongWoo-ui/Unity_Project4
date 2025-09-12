using System.Collections;
using UnityEditor;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;

    public GameObject power; // 적 공격력
    public GameObject boom; // 캐릭터 폭탄의 데미지를 불러오기 위한 오브젝트

    [Header("Enemy")]
    public int hp; // 적 체력
    public int speed; // 적 이동 속도

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        rb.linearVelocity = Vector2.down * speed;
    }
    void Update()
    {
        Dead();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Border")) // 벽에 닿으면 적을 삭제하여 지나간 것처럼 구현
        {
            Destroy(gameObject);
        }
        else if(collision.CompareTag("PlayerBullet")) // 캐릭터 총알에 맞으면 캐릭터 총알은 삭제되고 적의 색을 빨간색으로 바꿔 피격 당한것을 표현
        {
            PlayerBullet playerbullet = collision.GetComponent<PlayerBullet>();
            hp -= playerbullet.damage;
            Damage();
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("BoomEffect")) // 캐릭터 폭발 효과에 맞으면 데미지를 입음
        {
            Boom boom = collision.GetComponent<Boom>();
            hp -= boom.damage;
            Damage();
        }
    }
    void Damage() // 데미지를 받을때 빨간색으로 변한 적을 0.5초 뒤에 흰색으로 바꿔 피격당한 효과를 구현하는 코루틴
    {
        StartCoroutine(DamageCoroutine());
    }
    IEnumerator DamageCoroutine()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }
    void Dead() // 적의 체력이 0이하면 죽는 애니매이션 재생 후 0.5초 뒤 삭제하는 코루틴
    {
        if (hp <= 0)
        {
            StartCoroutine(DeadCoroutine());
        }
    }
    IEnumerator DeadCoroutine()
    {
        anim.SetBool("_IsDead",true);
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
        DropItem();
    }
    void DropItem() // 죽었을때 아이템 생성
    {
        int randomItemDrop = Random.Range(0, 10); // 난수 10을 설정하여 50%확률로 아이템이 나오지않고 각각 25% 확률로 파워 아이템과 폭탄 아이템 생성
        switch(randomItemDrop)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
                Debug.Log("No ItemDrop");
                break;
            case 7:
            case 8:
                GameObject dropPower = Instantiate(power,transform.position,transform.rotation);
                break;
            case 9:
            case 10:
                GameObject dropBoom = Instantiate(boom, transform.position, transform.rotation);
                break;
        }
    }
}
