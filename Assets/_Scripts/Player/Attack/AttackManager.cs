using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackManager : MonoBehaviour
{
    public enum Spells
    {
        Fireball,
        Bolt,
        Arcane
    }
    private Dictionary<Spells, Spell> _spellDictionary = new();
    private int _selectedSpellIndex;
    private GameObject _selectedSpellObject;


    [SerializeField] private RawImage fireballIndicator;
    [SerializeField] private Canvas boltIndicator;
    [SerializeField] private Canvas arcaneIndicator;
    private Vector3 position;
    private RaycastHit hit;
    private Ray ray;


    private void Start()
    {
        InitializeSpells();
    }

    private void InitializeSpells()
    {
        Spell[] loadedSpells = Resources.LoadAll<Spell>("Spells");
        foreach (var spell in loadedSpells)
        {
            if (System.Enum.TryParse(spell.name, out Spells spellEnum))
            {
                _spellDictionary[spellEnum] = spell;
            }
        }

        _selectedSpellIndex = 0;
        _selectedSpellObject = null;
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetSpell(Spells.Fireball);
            SetIndicator(_selectedSpellIndex);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetSpell(Spells.Bolt);
            SetIndicator(_selectedSpellIndex);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetSpell(Spells.Arcane);
            SetIndicator(_selectedSpellIndex);
        }
    }


    private void SetIndicator(int selectedSpell)
    {
        switch(selectedSpell)
        {
            case 1:
                fireballIndicator.gameObject.SetActive(true);
                boltIndicator.gameObject.SetActive(false);
                arcaneIndicator.gameObject.SetActive(false);
                break;
            case 2:
                boltIndicator.gameObject.SetActive(true);
                fireballIndicator.gameObject.SetActive(false);
                arcaneIndicator.gameObject.SetActive(false);
                break;
            case 3:
                arcaneIndicator.gameObject.SetActive(true);
                fireballIndicator.gameObject.SetActive(false);
                boltIndicator.gameObject.SetActive(false);
                break;
            default:
                Debug.LogWarning("No spell Selected");
                break;
        }
    }

    public Spell SetSpell(Spells spellName)
    {
        if (_spellDictionary.TryGetValue(spellName, out Spell spell))
        {
            Debug.Log("Spell found: " + spell.name);
            _selectedSpellIndex = (int)spellName;
            return spell;
        }
        else
        {
            Debug.LogWarning("Spell not found");
            return null;
        }
    }
}

