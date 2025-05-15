using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.Managers;
using _Scripts.Player;

public class AttackManager : NetworkBehaviour
{
    public enum Spells
    {
        Basic,
        Fireball,
        Bolt,
        Arcane,
    }

    private readonly Dictionary<Spells, Spell> _spellDictionary = new();
    private readonly Dictionary<Spells, float> _spellCooldowns = new();
    private readonly Dictionary<Spells, GameObject> _indicators = new();
    private Spell _selectedSpell; // Stores the currently selected spell SO
    private Spells _currentSpell; // Stores the currently selected spell enum
    private Spell _castedSpell; // Stores the spell which is currently being casted
    private NetworkObject castedSpell; // Stores the casted spell object

    private PlayerStats _playerStats;
    private PlayerMovement _playerMovement;
    private PlayerAnimator _playerAnimator;

    [SerializeField]
    private GameObject[] spellObject;

    [SerializeField]
    private RawImage fireballIndicator;

    [SerializeField]
    private Canvas boltIndicator;

    [SerializeField]
    private Canvas arcaneIndicator;

    [SerializeField]
    private LayerMask groundMask;

    private ulong _playerId;

    private Camera _camera;
    public bool cd = false;
    private void Update()
    {
        if (!IsOwner) return;
        HandleSpellSelection();
        HandleCasting();

        RaycastHit hit;
        if (InputHandler.Instance.attackTriggered)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, groundMask)
                && Time.time > _spellCooldowns[_currentSpell] && !cd)
            {
                Vector3 hitPos = hit.point;
                _playerAnimator.ChangeAnimation(_currentSpell.ToString());
                _castedSpell = _selectedSpell;
                _spellCooldowns[_currentSpell] =
                    Time.time + (_selectedSpell.cooldown * _playerStats.cooldownMultiplier);

                CastSpellRpc(hitPos, (int)_currentSpell);

                // _selectedSpell = SetSpell(Spells.Basic);
            }
        }
    }
    
    #region Spell Casting
    [Rpc(SendTo.Server)]
    private void CastSpellRpc(Vector3 pos, int spellIndex)
    {
        if (!_spellDictionary.TryGetValue((Spells)spellIndex, out var spell))
        {
            Debug.LogError($"Server: Could not find spell for index {spellIndex}");
            return;
        }

        if (_playerStats.currentMana >= spell.manaCost)
        {
            _playerStats.currentMana -= spell.manaCost;
            _castedSpell = spell; // update server-side reference
            GameObject objectPos;

            if (_castedSpell.areaOfEffect == Spell.AreaOfEffect.Cone ||
                _castedSpell.areaOfEffect == Spell.AreaOfEffect.Line ||
                _castedSpell.areaOfEffect == Spell.AreaOfEffect.None)
            {
                objectPos = spellObject[0];
            }
            else {objectPos = spellObject[1];}

            if (_castedSpell.areaOfEffect == (Spell.AreaOfEffect.None) ||
                _castedSpell.areaOfEffect == (Spell.AreaOfEffect.Circle))
            {
                castedSpell = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(spell.spellPrefab, _playerId,
                    position: objectPos.transform.position, rotation: Quaternion.identity);
            }
            
            if(castedSpell != null)
                castedSpell.GetComponent<Rigidbody>().useGravity = false;

            StartCoroutine(CastSpell(spell, pos));
        }
        else
        {
            Debug.Log("Not enough mana to cast " + _castedSpell.name);
            Debug.LogWarning("_playerStats or _castedSpell is null in CastSpellRpc");
            if (_playerStats == null)
                Debug.LogError("_playerStats is null");
            if (_castedSpell == null)
                Debug.LogError("_castedSpell is null");
        }
    }

    private IEnumerator CastSpell(Spell spell, Vector3 pos)
    {
        float castTime = Time.time + spell.castTime;
        
        while (Time.time < castTime)
        {
            cd = true;
            _playerMovement.canMove = false;
            Quaternion targetRotation = Quaternion.LookRotation(pos - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, spell.castTime * Time.deltaTime * 10f);
            yield return new WaitForFixedUpdate();
        }

        _playerMovement.canMove = true;
        _playerAnimator.ChangeAnimation("Idle");


        // Basic attack
        if (spell.areaOfEffect == Spell.AreaOfEffect.None)
        {
            SpawnBasicAttackRpc(pos);
        }
        // Circle AOE Spells
        else if (spell.areaOfEffect == Spell.AreaOfEffect.Circle)
        {
            SpawnCircleAoeAttackRpc(pos);
        }
        // Line AOE Spells
        else if (spell.areaOfEffect == Spell.AreaOfEffect.Line)
        {
            SpawnLineAoeAttackRpc(pos);
        }
        // Cone AOE Spells
        else if (spell.areaOfEffect == Spell.AreaOfEffect.Cone)
        {
            SpawnConeAoeAttackRpc(pos);
        }
        WaitRpc();
        cd = false;
    }

    [Rpc(SendTo.Server)]
    private void SpawnBasicAttackRpc(Vector3 pos)
    {
        StartCoroutine(BasicAttack(pos));
    }

    private IEnumerator BasicAttack(Vector3 pos)
    {
        bool travel = true;
        castedSpell.GetComponent<Rigidbody>().useGravity = false;
        castedSpell.GetComponent<Rigidbody>().linearVelocity =
            (pos - transform.position).normalized * _castedSpell.travelSpeed;
        while (travel == true)
        {
            float distance = Vector3.Distance(spellObject[0].transform.position, castedSpell.transform.position);
            if (distance > _castedSpell.range)
            {
                Destroy(castedSpell);
                travel = false;
                yield break;
            }
            yield return null;
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnCircleAoeAttackRpc(Vector3 pos)
    {
        if (_castedSpell == null)
        {
            Debug.LogWarning("_castedSpell is null in SpawnCircleAoeAttackRpc");
            return;
        }
        if (_castedSpell.hitboxPrefab == null)
        {
            Debug.LogWarning("_castedSpell.hitboxPrefab is null in SpawnCircleAoeAttackRpc");
            return;
        }
        _castedSpell.hitboxPrefab.transform.localScale = new Vector3(
            _castedSpell.areaOfEffectRadius / 2,
            _castedSpell.areaOfEffectRadius / 2,
            _castedSpell.areaOfEffectRadius / 2);
        NetworkObject hitbox = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
            _castedSpell.hitboxPrefab,
            _playerId,
            position: pos,
            rotation: Quaternion.identity);
        hitbox.GetComponent<AttackHitbox>().SetCaster(gameObject);

        StartCoroutine(CircleAoeAttack(hitbox, pos));
    }

    private IEnumerator CircleAoeAttack(NetworkObject hitbox, Vector3 pos)
    {
        while (Vector3.Distance(castedSpell.transform.position, pos) > 0.8f)
        {
            castedSpell.transform.position = Vector3.Slerp(
                castedSpell.transform.position,
                pos,
                _castedSpell.travelSpeed * Time.deltaTime);
            yield return null;
        }

        castedSpell.Despawn(true);
        yield return new WaitForSeconds(_castedSpell.duration);
        hitbox.Despawn(true);
    }

    [Rpc(SendTo.Server)]
    private void SpawnLineAoeAttackRpc(Vector3 pos)
    {
        _castedSpell.hitboxPrefab.transform.localScale = new Vector3(
            _castedSpell.areaOfEffectRadius / 2,
            3,
            (_castedSpell.range / 4) * 2);
        
        Quaternion hitboxRotation = Quaternion.LookRotation(pos - transform.position);
        float offset = ((((_castedSpell.range / 4) * 2) / 10) * 6);
        Vector3 hitboxPosition = transform.position + hitboxRotation * Vector3.forward * offset;

        NetworkObject hitbox = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
            _castedSpell.hitboxPrefab,
            _playerId,
            position: hitboxPosition,
            rotation: hitboxRotation);
        
        hitbox.GetComponent<AttackHitbox>().SetCaster(gameObject);
        hitbox.transform.rotation = Quaternion.LookRotation(pos - transform.position);

        StartCoroutine(LineAoeAttack(hitbox, pos));
    }

    private IEnumerator LineAoeAttack(NetworkObject hitbox, Vector3 pos)
    {
        // Implement line spell visual effects

        //castedSpell.Despawn(true);
        yield return new WaitForSeconds(_castedSpell.duration);
        hitbox.Despawn(true);
        //Destroy(castedSpell);
    }

    [Rpc(SendTo.Server)]
    private void SpawnConeAoeAttackRpc(Vector3 pos)
    {
        _castedSpell.hitboxPrefab.transform.localScale = new Vector3(
            (_castedSpell.areaOfEffectRadius / 5) * 11,
            (_castedSpell.range / 15) * 8.5f,
            3);
        Quaternion hitboxRotation = Quaternion.LookRotation(pos - transform.position);
        hitboxRotation = Quaternion.Euler(
            hitboxRotation.eulerAngles.x - 90, 
            hitboxRotation.eulerAngles.y + 180, 
            hitboxRotation.eulerAngles.z);
        NetworkObject hitbox = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
            _castedSpell.hitboxPrefab,
            _playerId,
            position: transform.position,
            rotation: hitboxRotation);
        hitbox.GetComponent<AttackHitbox>().SetCaster(gameObject);

        StartCoroutine(ConeAoeAttack(hitbox, _castedSpell, pos));
    }

    private IEnumerator ConeAoeAttack(NetworkObject hitbox, Spell spell, Vector3 pos)
    {
        //castedSpell.Despawn(true);
        yield return new WaitForSeconds(_castedSpell.duration);
        hitbox.Despawn(true);
    }
    

    [Rpc(SendTo.Everyone)]
    private void WaitRpc()
    {
        _selectedSpell = SetSpell(Spells.Basic);
    }
    #endregion

    #region Initialization
    private void Start()
    {
        _playerId = NetworkManager.Singleton.LocalClientId;
        groundMask = LayerMask.GetMask("Ground");
        InitializeSpells();
        InitializeIndicators();
        _playerStats = GetComponent<PlayerStats>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerAnimator = GetComponent<PlayerAnimator>();

        _selectedSpell = SetSpell(Spells.Basic);
        _camera = Camera.main;
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
                fireballIndicator.transform.Find("TargetIndicator").GetComponent<RectTransform>().localScale = new Vector3(
                    _selectedSpell.areaOfEffectRadius / 2,
                    _selectedSpell.areaOfEffectRadius / 2,
                    0
                );
            }

            foreach (var indicator in _indicators)
            {
                if (_selectedSpell != null && _selectedSpell.areaOfEffect == Spell.AreaOfEffect.Line)
                {
                    indicator.Value.transform.GetComponent<RectTransform>().localScale =
                        new Vector3(_selectedSpell.areaOfEffectRadius, 3, _selectedSpell.range / 4);
                }
                else if (
                    _selectedSpell != null
                    && _selectedSpell.areaOfEffect == Spell.AreaOfEffect.Cone
                )
                {
                    indicator.Value.transform.GetComponent<RectTransform>().localScale =
                        new Vector3(_selectedSpell.areaOfEffectRadius, 3, _selectedSpell.range / 3);
                }
                else if (_selectedSpell != null && _selectedSpell.areaOfEffect == Spell.AreaOfEffect.Circle)
                {
                    indicator.Value.transform.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    indicator.Value.transform.GetComponent<RectTransform>().sizeDelta = new Vector3(
                        _selectedSpell.range / 2,
                        _selectedSpell.range / 2);
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


    #region Setting & Selecting

    private void HandleSpellSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && _currentSpell != Spells.Fireball)
            SetSpell(Spells.Fireball);
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            SetSpell(Spells.Basic);
        if (Input.GetKeyDown(KeyCode.Alpha2) && _currentSpell != Spells.Bolt)
            SetSpell(Spells.Bolt);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SetSpell(Spells.Basic);
        if (Input.GetKeyDown(KeyCode.Alpha3) && _currentSpell != Spells.Arcane)
            SetSpell(Spells.Arcane);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SetSpell(Spells.Basic);
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
                UIManager.Instance.UpdateSelectedSpell((int)_currentSpell);
                return spell;
            }
            else
            {
                Debug.Log("Not enough mana to cast " + spell.name);
                _selectedSpell = _spellDictionary[Spells.Basic];
                _currentSpell = Spells.Basic;
                UIManager.Instance.UpdateSelectedSpell((int)_currentSpell);
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

    public bool CheckCastedSpell(Spells spell)
    {
        if (_castedSpell == _spellDictionary[spell])
        {
            return true;
        }
        return false;
    }
    #endregion
}
