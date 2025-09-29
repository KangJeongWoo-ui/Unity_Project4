using Mono.Cecil.Cil;
using System.Collections;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    public Camera followCamera;
    public GameManager manager;

    [Header("Character Stats")]
    public GameObject[] weapons; //무기 종류
    public GameObject[] Grenades; //폭탄 종류

    public bool[] getweapons; //무기 습득 확인
    public GameObject GrenadeObject; //던지는 폭탄

    public float speed; //캐릭터 속력
    public float jump; //캐릭터 점프력

    public int Ammo; //총알
    public int Coin; //코인
    public int Heart; //생명
    public int Grenade; //폭탄
    public int score;

    public int MaxAmmo; //최대총알 갯수
    public int MaxCoin; //최대코인 갯수
    public int MaxHeart; //최대생명 갯수
    public int MaxGrenade; //최대폭탄 갯수

    float h; //수평값
    float v; //수직값

    bool walkDown; //걷기
    bool jumpDown; //점프
    bool getItemDown; //아이템 줍기
    bool fire1Down; //공격
    bool fire2Down;
    bool reloadDown; //재장전

    bool swapWeapon1;
    bool swapWeapon2;
    bool swapWeapon3;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady = true;
    bool isDamage;
    bool isDead;

    Vector3 moveVec; //움직임 변수
    Vector3 dodgeVec; //구르기 변수

    GameObject nearObject;
    public Weapon equipWeapon;

    int equipWeaponIndex = -1;
    float fireDelay;

    Rigidbody rigid;
    Animator animator; //애니메이션 변수
    MeshRenderer[] meshRenderer;
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        meshRenderer = GetComponentsInChildren<MeshRenderer>();

        Debug.Log(PlayerPrefs.GetInt("MaxScore"));
        //PlayerPrefs.SetInt("MaxScore", 112300);
    }
    void Update()
    {
        GetInput();
        Turn();
        Jump();
        Dodge();
        Swap();
        Attack();
        ThrowGrenade();
        Reload();
        Interation();

        moveVec = new Vector3(h, 0, v).normalized;

        if (isDodge)
        {
            moveVec = dodgeVec;
        }

        if (isSwap || isReload || !isFireReady)
        {
            moveVec = Vector3.zero;
        }

        animator.SetBool("_IsRun", moveVec != Vector3.zero);
        animator.SetBool("_IsWalk", walkDown);
    }
    void GetInput() // 키 입력
    {
        h = Input.GetAxisRaw("Horizontal"); //수평값 입력
        v = Input.GetAxisRaw("Vertical"); //수직값 입력
        walkDown = Input.GetButton("Walk"); //shift키로 걷기 입력
        jumpDown = Input.GetButtonDown("Jump"); //점프 입력
        getItemDown = Input.GetButtonDown("Interation"); //아이템 줍기
        fire1Down = Input.GetButtonDown("Fire1"); //공격
        fire2Down = Input.GetButtonDown("Fire2");
        reloadDown = Input.GetButton("Reload");
        swapWeapon1 = Input.GetButtonDown("Swap1"); //무기 교체
        swapWeapon2 = Input.GetButtonDown("Swap2");
        swapWeapon3 = Input.GetButtonDown("Swap3");
    }
    void Move() // 움직임
    {
        // 현재 속도에서 수직(y) 성분은 유지하고, 수평(xz)만 갱신
        float moveSpeed = speed * (walkDown ? 0.3f : 1.0f);

        Vector3 desired = moveVec * moveSpeed;        // 원하는 수평 속도 (xz)
        Vector3 vel = rigid.linearVelocity;                 // 현재 속도
        vel.x = desired.x;
        vel.z = desired.z;

        // 공격/스왑/리로드로 멈춰야 할 때는 수평을 0으로
        if (isSwap || isReload || !isFireReady)
        {
            vel.x = 0f; vel.z = 0f;
        }

        rigid.linearVelocity = vel; // 실제 적용
    }
    void Turn() // 캐릭터 회전
    {
        transform.LookAt(transform.position + moveVec);

        if (fire1Down)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }
    void Jump() // 캐릭터 점프
    {
        if (jumpDown && moveVec == Vector3.zero && !isJump && !isDodge) //점프키를 누르거나 점프중이 아닐경우 점프
        {
            rigid.AddForce(Vector3.up * jump, ForceMode.Impulse);
            animator.SetBool("_IsJump", true);
            animator.SetTrigger("_DoJump");
            isJump = true;
        }
    }
    void Attack()
    {
        if (equipWeapon == null)
        {
            return;
        }
        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fire1Down && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            animator.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "_DoSwing" : "_DoShot");
            fireDelay = 0;
        }
    } // 캐릭터 공격
    void ThrowGrenade()
    {
        if(Grenade == 0)
        {
            return;
        }
        if(fire2Down && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(GrenadeObject, transform.position, transform.rotation);
                Rigidbody rigidbodyGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidbodyGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidbodyGrenade.AddForce(Vector3.back * 10, ForceMode.Impulse);

                Grenade--;
                Grenades[Grenade].SetActive(false);
            }
        }
    }
    void Reload() // 재장전
    {
        if(equipWeapon == null)
        {
            return;
        }
        if(equipWeapon.type == Weapon.Type.Melee)
        {
            return;
        }
        if(Ammo == 0)
        {
            return;
        }
        if(reloadDown && !isJump && !isSwap && isFireReady)
        {
            animator.SetTrigger("_DoReload");
            isReload = true;

            StartCoroutine(ReloadInvoke());
        }
    }
    IEnumerator ReloadInvoke()
    {
        yield return new WaitForSeconds(2.0f);
        // 총에서 부족한 탄약 수 계산
        int needAmmo = equipWeapon.maxAmmo - equipWeapon.curAmmo;

        // 인벤토리 Ammo가 더 적다면 그만큼만 채움
        int reAmmo = (Ammo >= needAmmo) ? needAmmo : Ammo;

        equipWeapon.curAmmo += reAmmo; // 부족한 만큼만 채움
        Ammo -= reAmmo;                // 인벤토리 탄약 차감

        isReload = false;
    }
    void Dodge() // 캐릭터 구르기
    {
        if (jumpDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            dodgeVec = moveVec;
            speed *= 2; //구르기를 할 시 속도 * 2
            animator.SetTrigger("_DoDodge");
            isDodge = true;

            StartCoroutine(DodgeInvoke()); //코루틴을 실행
        }
    }
    IEnumerator DodgeInvoke()
    {
        yield return new WaitForSeconds(0.4f); //구르기에 0.4초 간격을 줌
        speed *= 0.5f; // 구르기까 끝나면 다시 원래 속도
        isDodge = false;
    }
    void Swap()
    {
        if (swapWeapon1 && (!getweapons[0] || equipWeaponIndex == 0))
            return;
        if (swapWeapon2 && (!getweapons[1] || equipWeaponIndex == 1))
            return;
        if (swapWeapon3 && (!getweapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (swapWeapon1) weaponIndex = 0;
        if (swapWeapon2) weaponIndex = 1;
        if (swapWeapon3) weaponIndex = 2;

        if ((swapWeapon1 || swapWeapon2 || swapWeapon3) && !isJump && !isDodge)
        {
            if(equipWeapon != null)
            {
                equipWeapon.gameObject.SetActive(false);
            }
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            animator.SetTrigger("_DoSwap");

            isSwap = true;

            StartCoroutine(SwapInvoke());
        }
    } // 무기 교체
    IEnumerator SwapInvoke()
    {
        yield return new WaitForSeconds(0.4f);
        isSwap = false;
    }
    void Interation()
    {
        if (!getItemDown || nearObject == null || isJump || isDodge)
            return;

        if (nearObject.CompareTag("Weapon"))
        {
            Item item = nearObject.GetComponent<Item>();
            int weaponIndex = item.value;
            getweapons[weaponIndex] = true;
            Destroy(nearObject);
        }
        else if (nearObject.CompareTag("Shop"))
        {
            Shop shop = nearObject.GetComponent<Shop>();
            if (shop != null)
                shop.Enter(this); // ← E키 눌렀을 때만 호출
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            animator.SetBool("_IsJump", false);
            isJump = false; //바닥에 닿을경우 점프중이 아님
        }
    }
    void FreezeRotation() //캐릭터가 물체와 충돌하여 회전되는거 방지
    {
        rigid.angularVelocity = Vector3.zero;
    }
    void FixedUpdate()
    {
        Move();
        FreezeRotation();
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();

            switch(item.type)
            {
                case Item.Type.Ammo:
                    Ammo += item.value;
                    if(Ammo > MaxAmmo)
                        Ammo = MaxAmmo;
                    break;
                case Item.Type.Coin:
                    Coin += item.value;
                    if (Coin > MaxCoin)
                        Coin = MaxCoin;
                    break;
                case Item.Type.Heart:
                    Heart += item.value;
                    if (Heart > MaxHeart)
                        Heart = MaxHeart;
                    break;
                case Item.Type.Grenade:
                    Grenades[Grenade].SetActive(true);
                    Grenade += item.value;
                    if (Grenade > MaxGrenade)
                    {
                        Grenade = MaxGrenade;
                    }
                    break;
            }
            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                Heart -= enemyBullet.damage;

                bool isBossAttack = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAttack));
            }
            if (other.GetComponent<Rigidbody>() != null)
            {
                Destroy(other.gameObject);
            }
        }
    }
    IEnumerator OnDamage(bool isBossAttack)
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshRenderer)
        {
            mesh.material.color = Color.red;
        }
        if(isBossAttack)
        {
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
        }
        if (Heart <= 0 && !isDead)
            OnDie();

        yield return new WaitForSeconds(1f);
        isDamage = false;
        foreach (MeshRenderer mesh in meshRenderer)
        {
            mesh.material.color = Color.white;
        }
        if(isBossAttack)
        {
            rigid.linearVelocity = Vector3.zero;
        }
        

    }
    void OnDie()
    {
        animator.SetTrigger("_DoDie");
        isDead = true;
        manager.GameOver();
    }
    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon" || other.tag == "Shop")
        {
            nearObject = other.gameObject;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            if (nearObject == other.gameObject) nearObject = null;
        }
        else if (other.CompareTag("Shop"))
        {
            // ★ nearObject 말고 other로 직접 받기
            Shop shop = other.GetComponent<Shop>();
            if (shop != null) shop.Exit();

            if (nearObject == other.gameObject) nearObject = null;
        }
    }
}
