using ArtificeToolkit.Attributes;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellObject", menuName = "Scriptable Objects/SpellObject")]
public class Spell : ScriptableObject
{
    public enum SpellType
    {
        None,
        Fire,
        Water,
        Ice,
        Lightning,
        Earth,
    }

    public enum AreaOfEffect
    {
        None,
        Circle,
        Line,
        Cone,
    }
    public SpellType spellType;
    [EnableIf(nameof(spellType), SpellType.Fire, SpellType.Water, SpellType.Ice, SpellType.Lightning, SpellType.Earth)]
    public NetworkObject hitboxPrefab;
    public NetworkObject spellPrefab;
    public float effectDuration;
    public int effectDamage;
    public AreaOfEffect areaOfEffect;
    public float areaOfEffectRadius;
    public bool isPiercing;
    public int damage;
    public int manaCost;
    public float range;
    public float castTime;
    public float cooldown;
    [Tooltip("How long the hitbox will stay active")]
    public float duration;
    public float travelSpeed;
}
