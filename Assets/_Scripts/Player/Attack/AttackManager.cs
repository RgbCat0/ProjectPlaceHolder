using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AttackManager : MonoBehaviour
{
    public enum Spells { Basic, Fireball, Bolt, Arcane }

    private readonly Dictionary<Spells, Spell> _spellDictionary = new();
    private readonly Dictionary<Spells, GameObject> _indicators = new();
    private Spell _selectedSpell;
    private Spells _currentSpell;

    [SerializeField] private GameObject spellObject;
    [SerializeField] private RawImage fireballIndicator;
    [SerializeField] private Canvas boltIndicator;
    [SerializeField] private Canvas arcaneIndicator;

    private float cd;

   

    private void Start()
    {
        InitializeSpells();
        InitializeIndicators();
        _selectedSpell = SetSpell(Spells.Basic);
    }

    private void InitializeSpells()
    {
        foreach (var spell in Resources.LoadAll<Spell>("Spells"))
        {
            Debug.Log(spell);
            if (System.Enum.TryParse(spell.name, out Spells spellEnum))
                _spellDictionary[spellEnum] = spell;
        }
    }
    private void InitializeIndicators()
    {
        _indicators[Spells.Fireball] = fireballIndicator.gameObject;
        _indicators[Spells.Bolt] = boltIndicator.gameObject;
        _indicators[Spells.Arcane] = arcaneIndicator.gameObject;
    }

    private void Update()
    {
        HandleSpellSelection();
        HandleCasting();
    }
    
    private void HandleSpellSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && _currentSpell != Spells.Fireball) SelectSpell(Spells.Fireball);
        else if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSpell(Spells.Basic);
        if (Input.GetKeyDown(KeyCode.Alpha2) && _currentSpell != Spells.Bolt) SelectSpell(Spells.Bolt);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSpell(Spells.Basic);
        if (Input.GetKeyDown(KeyCode.Alpha3) && _currentSpell != Spells.Arcane) SelectSpell(Spells.Arcane);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSpell(Spells.Basic);
    }

    private void SelectSpell(Spells selectedSpell)
    {
        SetSpell(selectedSpell);
        SetIndicator(selectedSpell);
    }

    private void HandleCasting()
    {

        // Indicators
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            Vector3 hitPos = hit.point;
            if (_indicators.TryGetValue(Spells.Fireball, out var fireballIndicator) && fireballIndicator.activeSelf)
            {
                fireballIndicator.transform.Find("TargetIndicator").position = hitPos;
                fireballIndicator.transform.Find("TargetIndicator").GetComponent<RectTransform>().localScale = new Vector3(_selectedSpell.areaOfEffectRadius / 2, _selectedSpell.areaOfEffectRadius / 2, 0);
            }

            foreach (var indicator in _indicators)
            {
                if(_selectedSpell != null && _selectedSpell.areaOfEffect == Spell.AreaOfEffect.Line || _selectedSpell != null && _selectedSpell.areaOfEffect == Spell.AreaOfEffect.Cone)
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


        // Casting
        if(InputHandler.Instance.attackTriggered)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) && Time.time > cd)
            {
                Debug.Log("Casting spell: " + _selectedSpell.name);
                Vector3 hitPos = hit.point;
                StartCoroutine(CastSpell(_selectedSpell, hitPos));
            }
        }

    }

    private IEnumerator CastSpell(Spell spell, Vector3 pos)
    {
        //if(playerMana >= spell.manaCost)
        //{

        //}
        cd = Time.time + spell.cooldown;

        GameObject castedSpell = Instantiate(spell.spellPrefab, spellObject.transform.position, Quaternion.identity);

        castedSpell.GetComponent<Rigidbody>().useGravity = false;
        yield return new WaitForSeconds(spell.castTime);
        castedSpell.GetComponent<Rigidbody>().useGravity = true;
        Debug.Log("Instantiated");

        if (spell.areaOfEffect == Spell.AreaOfEffect.None)
        {
            Vector3 origin = spellObject.transform.position;
            bool travel = true;
            castedSpell.GetComponent<Rigidbody>().useGravity = false;
            castedSpell.GetComponent<Rigidbody>().linearVelocity = (pos - transform.position).normalized * spell.travelSpeed;
            while (travel == true)
            {
                float distance = Vector3.Distance(origin, castedSpell.transform.position);
                if(distance > spell.range)
                {
                    Destroy(castedSpell);
                    travel = false;
                    yield break;
                }
                yield return null;
            }
        }


        else if(spell.areaOfEffect == Spell.AreaOfEffect.Circle)
        {
            while (Vector3.Distance(castedSpell.transform.position, pos) > 0.8f)
            {
                castedSpell.transform.position = Vector3.Slerp(castedSpell.transform.position, pos, spell.travelSpeed * Time.deltaTime);
                Debug.Log("Moving");
                yield return null;
            }
            Destroy(castedSpell);
            spell.hitboxPrefab.transform.localScale = new Vector3(spell.areaOfEffectRadius / 2, spell.areaOfEffectRadius / 2, spell.areaOfEffectRadius / 2);
            GameObject hitbox = Instantiate(spell.hitboxPrefab, pos, Quaternion.identity);
            yield return new WaitForSeconds(spell.duration);
            Destroy(hitbox);
        }


        else if (spell.areaOfEffect == Spell.AreaOfEffect.Line)
        {
            castedSpell.transform.position = transform.position + transform.forward * spell.range;
        }


        else if (spell.areaOfEffect == Spell.AreaOfEffect.Cone)
        {
            castedSpell.transform.position = transform.position + transform.forward * spell.range;
        }


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

        if (selectedSpell == Spells.Basic)
        {
            Debug.Log("Basic Attack Selected");
        }
    }

    public Spell SetSpell(Spells spellName)
    {
  
        if (_spellDictionary.TryGetValue(spellName, out var spell))
        {
            Debug.Log($"Spell found: {spell.name}");
            _selectedSpell = spell;
            _currentSpell = spellName;
            return spell;
        }

        Debug.LogWarning("Spell not found " + spellName);
        return null;
    }

    public Spell GetSpell()
    {
        return _selectedSpell;
    }
}