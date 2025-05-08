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

    [SerializeField] private GameObject spellObject;
    [SerializeField] private RawImage fireballIndicator;
    [SerializeField] private Canvas boltIndicator;
    [SerializeField] private Canvas arcaneIndicator;

    private float cd;

   

    private void Start()
    {
        InitializeSpells();
        InitializeIndicators();
    }

    private void InitializeSpells()
    {
        foreach (var spell in Resources.LoadAll<Spell>("Spells"))
        {
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
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSpell(Spells.Fireball);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSpell(Spells.Bolt);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSpell(Spells.Arcane);
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
            }

            foreach (var indicator in _indicators)
            {
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

            
                Debug.Log("Casting spell: " + _selectedSpell.name);
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) && Time.time > cd)
                {
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
        castedSpell.GetComponent<Rigidbody>().AddForce(transform.up * 10, ForceMode.Impulse);
        yield return new WaitForSeconds(spell.castTime);
        Debug.Log("Instantiated");
        if(spell.arreaOfEffect == Spell.AreaOfEffect.Circle)
        {
            while (Vector3.Distance(castedSpell.transform.position, pos) > 0.01f)
            {
                castedSpell.transform.position = Vector3.Slerp(castedSpell.transform.position, pos, 5f * Time.deltaTime);
                yield return null;
            }
        }
        else if (spell.arreaOfEffect == Spell.AreaOfEffect.Line)
        {
            castedSpell.transform.position = transform.position + transform.forward * spell.range;
        }
        else if (spell.arreaOfEffect == Spell.AreaOfEffect.Cone)
        {
            castedSpell.transform.position = transform.position + transform.forward * spell.range;
        }

    }

    private void SetIndicator(Spells selectedSpell)
    {
        //_casting = true;

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
            //_casting = false;
        }
    }

    public Spell SetSpell(Spells spellName)
    {
        if (_spellDictionary.TryGetValue(spellName, out var spell))
        {
            Debug.Log($"Spell found: {spell.name}");
            _selectedSpell = spell;
            return spell;
        }

        Debug.LogWarning("Spell not found");
        return null;
    }
}