using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using _Scripts.Player;
using static UnityEditor.PlayerSettings;
using static UnityEngine.Rendering.DebugUI;

public class AttackManager : MonoBehaviour
{
    public enum Spells { Basic, Fireball, Bolt, Arcane }

    private readonly Dictionary<Spells, Spell> _spellDictionary = new();
    private readonly Dictionary<Spells, float> _spellCooldowns = new();
    private readonly Dictionary<Spells, GameObject> _indicators = new();
    private Spell _selectedSpell;
    private Spells _currentSpell;
    private Spell _castedSpell;

    private PlayerStats _playerStats;
    private PlayerMovement _playerMovement;

    [SerializeField] private GameObject spellObject;
    [SerializeField] private RawImage fireballIndicator;
    [SerializeField] private Canvas boltIndicator;
    [SerializeField] private Canvas arcaneIndicator;

    [SerializeField] private LayerMask groundMask;

    private bool _cd;
    private void Update()
    {
        HandleSpellSelection();
        HandleCasting();

        RaycastHit hit;
        if (InputHandler.Instance.attackTriggered)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, groundMask) && Time.time > _spellCooldowns[_currentSpell] && !_cd)
            {
                Vector3 hitPos = hit.point;
                StartCoroutine(CastSpell(_selectedSpell, hitPos));
                _castedSpell = _selectedSpell;
                _spellCooldowns[_currentSpell] = Time.time + (_selectedSpell.cooldown * _playerStats.cooldownMultiplier);
                _selectedSpell = SetSpell(Spells.Basic);
            }
        }
    }

    #region Initialization
    private void Start()
    {
        groundMask = LayerMask.GetMask("Ground");
        InitializeSpells();
        InitializeIndicators();
        _playerStats = GetComponent<PlayerStats>();
        _playerMovement = GetComponent<PlayerMovement>();
        _selectedSpell = SetSpell(Spells.Basic);
    }


    private void InitializeSpells()
    {
        foreach (var spell in Resources.LoadAll<Spell>("Spells"))
        {
            if (System.Enum.TryParse(spell.name, out Spells spellEnum))
            {
                _spellDictionary[spellEnum] = spell;
                _spellCooldowns[spellEnum] = 0f;
            }
        }
    }
    private void InitializeIndicators()
    {
        _indicators[Spells.Fireball] = fireballIndicator.gameObject;
        _indicators[Spells.Bolt] = boltIndicator.gameObject;
        _indicators[Spells.Arcane] = arcaneIndicator.gameObject;
    }
    #endregion
    

    #region Setting indicator values
    private void HandleCasting()
    {

        // Indicators
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, groundMask))
        {
            Vector3 hitPos = hit.point;
            if (_indicators.TryGetValue(Spells.Fireball, out var fireballIndicator) && fireballIndicator.activeSelf)
            {
                fireballIndicator.transform.Find("TargetIndicator").position = hitPos;
                fireballIndicator.transform.Find("TargetIndicator").GetComponent<RectTransform>().localScale = new Vector3(_selectedSpell.areaOfEffectRadius / 2, _selectedSpell.areaOfEffectRadius / 2, 0);
            }

            foreach (var indicator in _indicators)
            {
                if (_selectedSpell != null && _selectedSpell.areaOfEffect == Spell.AreaOfEffect.Line || _selectedSpell != null && _selectedSpell.areaOfEffect == Spell.AreaOfEffect.Cone)
                {
                    indicator.Value.transform.GetComponent<RectTransform>().localScale = new Vector3(_selectedSpell.areaOfEffectRadius, 3, _selectedSpell.range / 4);
                }

                else if (_selectedSpell != null && _selectedSpell.areaOfEffect == Spell.AreaOfEffect.Circle)
                {
                    indicator.Value.transform.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    indicator.Value.transform.GetComponent<RectTransform>().sizeDelta = new Vector3(_selectedSpell.range / 2, _selectedSpell.range / 2);
                }


                if (indicator.Key != Spells.Fireball && indicator.Value.activeSelf)
                {
                    Transform transform = indicator.Value.transform;
                    Vector3 dir = (hitPos - transform.position).normalized;
                    transform.rotation = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                }
            }
        }
    }
        #endregion

    #region Spell Casting
    private IEnumerator CastSpell(Spell spell, Vector3 pos)
    {
        if (_playerStats.currentMana >= spell.manaCost)
        {
            
            _playerStats.currentMana -= spell.manaCost;
            float castTime = Time.time + spell.castTime;

            GameObject castedSpell = Instantiate(spell.spellPrefab, spellObject.transform.position, Quaternion.identity);

            castedSpell.GetComponent<Rigidbody>().useGravity = false;


            while (Time.time < castTime)
            {
                _cd = true;
                _playerMovement.canMove = false;
                Quaternion targetRotation = Quaternion.LookRotation(pos - transform.position).normalized;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, spell.castTime * Time.deltaTime * 10f);
                yield return new WaitForFixedUpdate();
            }

            _playerMovement.canMove = true;
            _cd = false;

            // Basic attack
            if (spell.areaOfEffect == Spell.AreaOfEffect.None)
            {
                StartCoroutine(BasicAttack(castedSpell, spell, pos));
            }


            // Circle AOE Spells
            else if (spell.areaOfEffect == Spell.AreaOfEffect.Circle)
            {
                StartCoroutine(CircleAoeAttack(castedSpell, spell, pos));
            }

            // Line AOE Spells
            else if (spell.areaOfEffect == Spell.AreaOfEffect.Line)
            {
                StartCoroutine(LineAoeAttack(castedSpell, spell, pos));
            }

            // Cone AOE Spells
            else if (spell.areaOfEffect == Spell.AreaOfEffect.Cone)
            {
                StartCoroutine(ConeAoeAttack(castedSpell, spell, pos));
            }

            
        }

        else 
        {
            Debug.Log("Not enough mana to cast " + spell.name);
        }
    }

    private IEnumerator BasicAttack(GameObject castedSpell, Spell spell, Vector3 pos)
    {
        Vector3 origin = spellObject.transform.position;
        bool travel = true;
        castedSpell.GetComponent<Rigidbody>().useGravity = false;
        castedSpell.GetComponent<Rigidbody>().linearVelocity = (pos - transform.position).normalized * spell.travelSpeed;
        while (travel == true)
        {
            float distance = Vector3.Distance(origin, castedSpell.transform.position);
            if (distance > spell.range)
            {
                Destroy(castedSpell);
                travel = false;
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator CircleAoeAttack(GameObject castedSpell, Spell spell, Vector3 pos)
    {
        while (Vector3.Distance(castedSpell.transform.position, pos) > 0.8f)
        {
            castedSpell.transform.position = Vector3.Slerp(castedSpell.transform.position, pos, spell.travelSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(castedSpell);
        spell.hitboxPrefab.transform.localScale = new Vector3(spell.areaOfEffectRadius / 2, spell.areaOfEffectRadius / 2, spell.areaOfEffectRadius / 2);
        GameObject hitbox = Instantiate(spell.hitboxPrefab, pos, Quaternion.identity);
        hitbox.GetComponent<AttackHitbox>().SetCaster(gameObject);
        yield return new WaitForSeconds(spell.duration);
        Destroy(hitbox);
    }

    private IEnumerator LineAoeAttack(GameObject castedSpell, Spell spell, Vector3 pos)
    {
        // Implement line spell visual effects

        spell.hitboxPrefab.transform.localScale = new Vector3(spell.areaOfEffectRadius / 2, 3, (spell.range / 4) * 2);
        GameObject hitbox = Instantiate(spell.hitboxPrefab, transform.position, Quaternion.identity);
        hitbox.GetComponent<AttackHitbox>().SetCaster(gameObject);
        hitbox.transform.rotation = Quaternion.LookRotation(pos - transform.position);
        hitbox.transform.position += hitbox.transform.forward * 6;
        Destroy(castedSpell);
        yield return new WaitForSeconds(spell.duration);
        Destroy(hitbox);
        //Destroy(castedSpell);
    }

    private IEnumerator ConeAoeAttack(GameObject castedSpell, Spell spell, Vector3 pos)
    {
        //hitbox.GetComponent<AttackHitbox>().SetCaster(gameObject);
        throw new System.NotImplementedException();
    }
    #endregion


    #region Setting & Selecting

    private void HandleSpellSelection()
    {
            if (Input.GetKeyDown(KeyCode.Alpha1) && _currentSpell != Spells.Fireball) SetSpell(Spells.Fireball);
            else if (Input.GetKeyDown(KeyCode.Alpha1)) SetSpell(Spells.Basic);
            if (Input.GetKeyDown(KeyCode.Alpha2) && _currentSpell != Spells.Bolt) SetSpell(Spells.Bolt);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SetSpell(Spells.Basic);
            if (Input.GetKeyDown(KeyCode.Alpha3) && _currentSpell != Spells.Arcane) SetSpell(Spells.Arcane);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SetSpell(Spells.Basic);
    }

    private void SetIndicator(Spells selectedSpell)
    {
        foreach (var _indicator in _indicators.Values)
        {
            _indicator.SetActive(false);
        }

        if (_indicators.TryGetValue(selectedSpell, out var indicator))
        {
            indicator.SetActive(true);
        }
    }

    public Spell SetSpell(Spells spellName)
    {
  
        if (_spellDictionary.TryGetValue(spellName, out var spell) && spell != null)
        {
            if (_playerStats.currentMana >= spell.manaCost)
            {
                _selectedSpell = spell;
                _currentSpell = spellName;
                SetIndicator(spellName);
                return spell;
            }
            else
            {
                Debug.Log("Not enough mana to cast " + spell.name);
                _selectedSpell = _spellDictionary[Spells.Basic];
                _currentSpell = Spells.Basic;
                SetIndicator(Spells.Basic);
                return _selectedSpell;
            }
        }

        Debug.LogWarning("Spell not found " + spellName);
        return null;
    }

    public Spell GetCastedSpell()
    {
        return _castedSpell;
    }
    #endregion
}