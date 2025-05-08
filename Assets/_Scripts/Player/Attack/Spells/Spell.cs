using UnityEngine;

[CreateAssetMenu(fileName = "SpellObject", menuName = "Scriptable Objects/SpellObject")]
public class Spell : ScriptableObject
{
    public enum SpellType
    {
        Fire,
        Ice,
        Lightning,
        Earth,
        Wind
    }

    public enum AreaOfEffect
    {
        None,
        Circle,
        Line,
        Cone,
    }

    public GameObject hitboxPrefab;
    public GameObject spellPrefab;
    public SpellType spellType;
    public AreaOfEffect arreaOfEffect;
    public bool isPiercing;
    public int damage;
    public int manaCost;
    public float range;
    public float castTime;
    public float cooldown;
    public float duration;

}
