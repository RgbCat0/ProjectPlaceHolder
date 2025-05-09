using _Scripts.Enemies;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{

    private AttackManager _attackManager;
    void Start()
    {
        _attackManager = FindAnyObjectByType<AttackManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (other != null)
            {
                enemy.TakeDamage(_attackManager.GetSpell().damage);
            }
        }
    }
}
