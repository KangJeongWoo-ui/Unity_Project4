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
    public GameObject[] weapons; //���� ����
    public GameObject[] Grenades; //��ź ����

    public bool[] getweapons; //���� ���� Ȯ��
    public GameObject GrenadeObject; //������ ��ź

    public float speed; //ĳ���� �ӷ�
    public float jump; //ĳ���� ������

    public int Ammo; //�Ѿ�
    public int Coin; //����
    public int Heart; //����
    public int Grenade; //��ź
    public int score;

    public int MaxAmmo; //�ִ��Ѿ� ����
    public int MaxCoin; //�ִ����� ����
    public int MaxHeart; //�ִ���� ����
    public int MaxGrenade; //�ִ���ź ����

    float h; //����
    float v; //������

    bool walkDown; //�ȱ�
    bool jumpDown; //����
    bool getItemDown; //������ �ݱ�
    bool fire1Down; //����
    bool fire2Down;
    bool reloadDown; //������

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

    Vector3 moveVec; //������ ����
    Vector3 dodgeVec; //������ ����

    GameObject nearObject;
    public Weapon equipWeapon;

    int equipWeaponIndex = -1;
    float fireDelay;

    Rigidbody rigid;
    Animator animator; //�ִϸ��̼� ����
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
    void GetInput() // Ű �Է�
    {
        h = Input.GetAxisRaw("Horizontal"); //���� �Է�
        v = Input.GetAxisRaw("Vertical"); //������ �Է�
        walkDown = Input.GetButton("Walk"); //shiftŰ�� �ȱ� �Է�
        jumpDown = Input.GetButtonDown("Jump"); //���� �Է�
        getItemDown = Input.GetButtonDown("Interation"); //������ �ݱ�
        fire1Down = Input.GetButtonDown("Fire1"); //����
        fire2Down = Input.GetButtonDown("Fire2");
        reloadDown = Input.GetButton("Reload");
        swapWeapon1 = Input.GetButtonDown("Swap1"); //���� ��ü
        swapWeapon2 = Input.GetButtonDown("Swap2");
        swapWeapon3 = Input.GetButtonDown("Swap3");
    }
    void Move() // ������
    {
        // ���� �ӵ����� ����(y) ������ �����ϰ�, ����(xz)�� ����
        float moveSpeed = speed * (walkDown ? 0.3f : 1.0f);

        Vector3 desired = moveVec * moveSpeed;        // ���ϴ� ���� �ӵ� (xz)
        Vector3 vel = rigid.linearVelocity;                 // ���� �ӵ�
        vel.x = desired.x;
        vel.z = desired.z;

        // ����/����/���ε�� ����� �� ���� ������ 0����
        if (isSwap || isReload || !isFireReady)
        {
            vel.x = 0f; vel.z = 0f;
        }

        rigid.linearVelocity = vel; // ���� ����
    }
    void Turn() // ĳ���� ȸ��
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
    void Jump() // ĳ���� ����
    {
        if (jumpDown && moveVec == Vector3.zero && !isJump && !isDodge) //����Ű�� �����ų� �������� �ƴҰ�� ����
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
    } // ĳ���� ����
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
    void Reload() // ������
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
        // �ѿ��� ������ ź�� �� ���
        int needAmmo = equipWeapon.maxAmmo - equipWeapon.curAmmo;

        // �κ��丮 Ammo�� �� ���ٸ� �׸�ŭ�� ä��
        int reAmmo = (Ammo >= needAmmo) ? needAmmo : Ammo;

        equipWeapon.curAmmo += reAmmo; // ������ ��ŭ�� ä��
        Ammo -= reAmmo;                // �κ��丮 ź�� ����

        isReload = false;
    }
    void Dodge() // ĳ���� ������
    {
        if (jumpDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            dodgeVec = moveVec;
            speed *= 2; //�����⸦ �� �� �ӵ� * 2
            animator.SetTrigger("_DoDodge");
            isDodge = true;

            StartCoroutine(DodgeInvoke()); //�ڷ�ƾ�� ����
        }
    }
    IEnumerator DodgeInvoke()
    {
        yield return new WaitForSeconds(0.4f); //�����⿡ 0.4�� ������ ��
        speed *= 0.5f; // ������� ������ �ٽ� ���� �ӵ�
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
    } // ���� ��ü
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
                shop.Enter(this); // �� EŰ ������ ���� ȣ��
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            animator.SetBool("_IsJump", false);
            isJump = false; //�ٴڿ� ������� �������� �ƴ�
        }
    }
    void FreezeRotation() //ĳ���Ͱ� ��ü�� �浹�Ͽ� ȸ���Ǵ°� ����
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
            // �� nearObject ���� other�� ���� �ޱ�
            Shop shop = other.GetComponent<Shop>();
            if (shop != null) shop.Exit();

            if (nearObject == other.gameObject) nearObject = null;
        }
    }
}
