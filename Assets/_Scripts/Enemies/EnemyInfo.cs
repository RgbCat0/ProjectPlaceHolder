using UnityEngine;

namespace _Scripts.Enemies
{
    [CreateAssetMenu(fileName = "EnemyInfo", menuName = "Scriptable Objects/EnemyInfo", order = 1)]
    public class EnemyInfo : ScriptableObject
    {
        public string identifier; // (name)
        public GameObject modelPrefab; // (model to add to the base)
        public float health;
        public float speed;
    }
}
