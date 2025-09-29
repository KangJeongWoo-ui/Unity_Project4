using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range }; //�ٰŸ�, ���Ÿ�
    public Type type;
    public int damage; //������
    public float rate; //���� �ӵ�
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPos; //�Ѿ� ���� ��ġ
    public GameObject bullet; //�Ѿ�
    public Transform bulletCasePos;
    public GameObject bulletCase;
    public void Use() //���� ���
    {
        if(type == Type.Melee) //���Ⱑ ���������� ����
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if(type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }
    IEnumerator Swing() //���� �ֵθ���
    {
        yield return new WaitForSeconds(0.4f); //0.4�� ���
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }
    IEnumerator Shot()
    {
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.linearVelocity = bulletPos.forward * 50;

        yield return null;

        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
}
