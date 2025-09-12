using UnityEngine;

public class Item : MonoBehaviour
{
    Rigidbody2D rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        rb.linearVelocity = Vector2.down * 1;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Border"))
        {
            Destroy(gameObject); 
        }
    }
}
