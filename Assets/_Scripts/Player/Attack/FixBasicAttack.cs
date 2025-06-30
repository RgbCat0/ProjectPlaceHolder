using System;
using UnityEngine;

public class FixBasicAttack : MonoBehaviour
{
    private void Update()
    {
        transform.position = new Vector3(transform.position.x, 1, transform.position.z);
    }
}
