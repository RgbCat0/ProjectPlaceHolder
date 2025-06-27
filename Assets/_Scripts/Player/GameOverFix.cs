using Managers;
using UnityEngine;
 class GameOverFix : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Instance.GameOverUI(gameObject);
        Destroy(this);
    }
}
