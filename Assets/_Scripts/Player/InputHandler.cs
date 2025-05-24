using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class InputHandler : MonoBehaviour
{

    [Header("InputSystem")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action map name reference")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Action name reference")]
    [SerializeField] private string movementActionName = "Move";
    //[SerializeField] private string jumpActionName = "Jump";
    //[SerializeField] private string sprintActionName = "Sprint";
    [SerializeField] private string attackActionName = "attack";
    //[SerializeField] private string aimActionName = "Aim";
    //[SerializeField] private string reloadActionName = "Reload";
    [SerializeField] private string switchSpellActionName = "Spells";
    //[SerializeField] private string scoreboardActionName = "Scoreboard";
    //[SerializeField] private string settingsActionName = "Settings";

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction attackAction;
    private InputAction aimAction;
    private InputAction reloadAction;
    public InputAction switchSpellAction;
    private InputAction scoreboardAction;
    private InputAction settingsAction;

    public int spellIndex { get; private set; }

    public Vector2 moveInput { get; private set; }

    //public float sprintValue { get; private set; }

    //public bool jumpTriggered { get; private set; }

    public bool attackTriggered { get; set; }

    //public bool aimTriggered { get; private set; }

    //public bool reloadTriggered { get; private set; }

    public float spellValue { get; private set; }

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
        switchSpellAction = playerControls.FindActionMap(actionMapName).FindAction(switchSpellActionName);
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

        switchSpellAction.performed += OnSpellSwap;
        switchSpellAction.canceled += context => spellIndex = 0;

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
        switchSpellAction.Enable();
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
        switchSpellAction.Disable();
        //scoreboardAction.Disable();
        //settingsAction.Disable();
    }

    private void OnSpellSwap(InputAction.CallbackContext context)
    {
        spellIndex = (int)context.ReadValue<float>();
    }
}
