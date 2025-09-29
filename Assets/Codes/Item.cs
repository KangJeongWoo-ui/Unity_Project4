using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon}; //총알, 코인, 폭탄, 생명, 무기
    public Type type; //아이템 종류
    public int value; //아이템 값

    Rigidbody rigid;
    SphereCollider sphereCollider;
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }
    void Update()
    {
        if(transform.root.CompareTag("Player"))
        {
            return;
        }
        transform.Rotate(Vector3.up * 20 * Time.deltaTime); //아이템 회전
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }
    }
}
