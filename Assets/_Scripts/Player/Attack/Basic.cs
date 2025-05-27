using System.Collections;
using Unity.Netcode;
using UnityEngine;
using _Scripts.Enemies;
using _Scripts.Player;

public class Basic : NetworkBehaviour
{

    private PlayerStats _playerStats;
    private AttackManager _attackManager;
    
    private Collider _collider;
    void Start()
    {
        StartCoroutine(DestroyAfterTime(2f));
    }
    
    public void SetCaster(GameObject player)
    {
        _playerStats = player.GetComponent<PlayerStats>();
        _attackManager = player.GetComponent<AttackManager>();
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log(gameObject.name + " was destroyed");
        NetworkObject.Despawn(this.gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (_playerStats == null || _attackManager == null)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            _collider = other;
            SendDamageRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SendDamageRpc()
    {
        Debug.Log($"Hit {_collider.gameObject.name} ");
        Enemy enemy = _collider.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.SetAttacker(_attackManager.GetCastedSpell(), _playerStats);
        }
        else
        {
            Debug.LogWarning("Enemy script not found.");
        }
        NetworkObject.Despawn(this.gameObject);
    }
}
