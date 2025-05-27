using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class InputHandler : MonoBehaviour
{

    [Header("InputSystem")]
    [SerializeField] public InputActionAsset playerControls;

    [Header("Action map name reference")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Action name reference")]
    [SerializeField] private string movementActionName = "Move";
    //[SerializeField] private string jumpActionName = "Jump";
    //[SerializeField] private string sprintActionName = "Sprint";
    [SerializeField] private string attackActionName = "attack";
    //[SerializeField] private string aimActionName = "Aim";
    //[SerializeField] private string reloadActionName = "Reload";
    [SerializeField] private string spell1ActionName = "Spell 1";
    [SerializeField] private string spell2ActionName = "Spell 2";
    [SerializeField] private string spell3ActionName = "Spell 3";
    //[SerializeField] private string scoreboardActionName = "Scoreboard";
    //[SerializeField] private string settingsActionName = "Settings";

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction attackAction;
    private InputAction aimAction;
    private InputAction reloadAction;
    public InputAction spell1Action;
    public InputAction spell2Action;
    public InputAction spell3Action;
    private InputAction scoreboardAction;
    private InputAction settingsAction;

    public int spellIndex { get; private set; }

    public Vector2 moveInput { get; private set; }

    //public float sprintValue { get; private set; }

    //public bool jumpTriggered { get; private set; }

    public bool attackTriggered { get; set; }

    //public bool aimTriggered { get; private set; }

    //public bool reloadTriggered { get; private set; }

    public bool spell1Triggered { get; private set; }
    public bool spell2Triggered { get; private set; }
    public bool spell3Triggered { get; private set; }

    //public bool scoreboardTriggered { get; private set; }

    //public bool settingsTriggered { get; private set; }


    public static InputHandler Instance { get; private set; }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        moveAction = playerControls.FindActionMap(actionMapName).FindAction(movementActionName);
        //sprintAction = playerControls.FindActionMap(actionMapName).FindAction(sprintActionName);
        //jumpAction = playerControls.FindActionMap(actionMapName).FindAction(jumpActionName);
        attackAction = playerControls.FindActionMap(actionMapName).FindAction(attackActionName);
        //aimAction = playerControls.FindActionMap(actionMapName).FindAction(aimActionName);
        //reloadAction = playerControls.FindActionMap(actionMapName).FindAction(reloadActionName);
        spell1Action = playerControls.FindActionMap(actionMapName).FindAction(spell1ActionName);
        spell2Action = playerControls.FindActionMap(actionMapName).FindAction(spell2ActionName);
        spell3Action = playerControls.FindActionMap(actionMapName).FindAction(spell3ActionName);
        //scoreboardAction = playerControls.FindActionMap(actionMapName).FindAction(scoreboardActionName);
        //settingsAction = playerControls.FindActionMap(actionMapName).FindAction(settingsActionName);
        RegisterInputActions();
    }

    private void RegisterInputActions()
    {
        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = Vector2.zero;

        //sprintAction.performed += context => sprintValue = context.ReadValue<float>();
        //sprintAction.canceled += context => sprintValue = 0f;

        //jumpAction.performed += context => jumpTriggered = true;
        //jumpAction.canceled += context => jumpTriggered = false;

        attackAction.performed += context => attackTriggered = true;
        attackAction.canceled += context => attackTriggered = false;

        //aimAction.performed += context => aimTriggered = true;
        //aimAction.canceled += context => aimTriggered = false;

        //reloadAction.performed += context => reloadTriggered = true;
        //reloadAction.canceled += context => reloadTriggered = false;
        
        spell1Action.performed += OnSpellSwap;
        spell1Action.canceled += context => spellIndex = 0;
        
        spell2Action.performed += OnSpellSwap;
        spell2Action.canceled += context => spellIndex = 0;
        
        spell3Action.performed += OnSpellSwap;
        spell3Action.canceled += context => spellIndex = 0;

        //scoreboardAction.performed += context => scoreboardTriggered = true;
        //scoreboardAction.canceled += context => scoreboardTriggered = false;

        //settingsAction.performed += context => settingsTriggered = !settingsTriggered;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        //sprintAction.Enable();
        //jumpAction.Enable();    
        attackAction.Enable();
        //aimAction.Enable();
        //reloadAction.Enable();
        spell1Action.Enable();
        spell2Action.Enable();
        spell3Action.Enable();
        //scoreboardAction.Enable();
        //settingsAction.Enable();

    }

    private void OnDisable()
    {
        moveAction.Disable();
        //sprintAction.Disable();
        //jumpAction.Disable();
        attackAction.Disable();
        //aimAction.Disable();
        //reloadAction.Disable();
        spell1Action.Disable();
        spell2Action.Disable();
        spell3Action.Disable();
        //scoreboardAction.Disable();
        //settingsAction.Disable();
    }

    private void OnSpellSwap(InputAction.CallbackContext context)
    {
        spellIndex = (int)context.ReadValue<float>();
    }
}
