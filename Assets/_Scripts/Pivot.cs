using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pivot : MonoBehaviour
{
    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            Quaternion rotation = Quaternion.LookRotation(transform.position - hit.point);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, rotation.eulerAngles.y, transform.eulerAngles.z);
        }
    }
}
