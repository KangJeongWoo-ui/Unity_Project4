using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("PlayerBullet")]
    public int damage; // 총알 데미지
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Border"))
        {
            Destroy(gameObject);
        }
    }
}
