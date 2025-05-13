using _Scripts.Enemies;
using _Scripts.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class AttackHitbox : MonoBehaviour
{
    private PlayerStats _playerStats;
    private AttackManager _attackManager;

    public void SetCaster(GameObject player)
    {
        _playerStats = player.GetComponent<PlayerStats>();
        _attackManager = player.GetComponent<AttackManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(_playerStats == null || _attackManager == null)
        {
            return;
        }
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Hit {other.name}");
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(_attackManager.GetCastedSpell().damage * _playerStats.damageMultiplier);
            }
            else
            {
                Debug.LogWarning("Enemy script not found.");
            }
        }
    }
}
