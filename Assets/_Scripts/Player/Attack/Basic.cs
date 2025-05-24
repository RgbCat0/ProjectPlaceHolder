using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Basic : NetworkBehaviour
{

    void Start()
    {

    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        NetworkObject.Despawn();
    }
    
}
