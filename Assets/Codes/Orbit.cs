using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target;
    public float orbitSpeed;
    Vector3 offset;
    void Start()
    {
        offset = transform.position - target.position;
    }
    void Update()
    {
        offset = Quaternion.AngleAxis(orbitSpeed * Time.deltaTime, Vector3.up) * offset;

        transform.position = target.position + offset;
    }
}
