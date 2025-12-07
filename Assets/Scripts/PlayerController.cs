using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject playerCam;

    [Header("Movement Controls")]
    public float runAcceleration = 20f;
    public float runSpeed = 4f;
    public float drag = .2f;

    [Header("Camera controls")]
    public float lookSenseH = .1f;
    public float lookSenseV = .1f;
    public float lookLimitV = 89f;

    [Header("Gravity controls")]
    [SerializeField] private Vector3 _gravityDir = Vector3.down;
    public float gravityMagnitude = 9.81f;
    public float gravityMultiplier = 1f;

    [Header("Holo Preview")]
    public GameObject holo;
    public float holoDistance = 2f;
    private bool isSelecting = false;
    private Vector3 selectedDir = Vector3.zero;

    [Header("Grounding")]
    public float groundCheckDistance = 1.1f;

    private CharacterController _cc;
    private PlayerLocomotionInput _input;
    private PlayerAnimationHandler _anim;

    private Vector3 _horizontalVelocity;
    private Vector3 _verticalVelocity;

    private float _yaw;
    private float _pitch;

    private Vector3 baseForward = Vector3.forward;
    private float freeFallTime;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _input = GetComponent<PlayerLocomotionInput>();
        _anim = GetComponent<PlayerAnimationHandler>();

        if (_gravityDir == Vector3.zero) _gravityDir = Vector3.down;
        _gravityDir = _gravityDir.normalized;

        holo.SetActive(false);
    }

    private void Update()
    {
        if (UIManager.Instance.IsGameOver()) return;

        HandleGravitySelectionInput();
        HandleMovement();
        HandleGravity();
        HandleAnimation();
        CheckFreeFall();
    }

    private void LateUpdate()
    {
        if (UIManager.Instance.IsGameOver()) return;

        HandleLook();
        ApplyFinalMovement();
    }

    private void CheckFreeFall()
    {
        bool grounded = Physics.Raycast(transform.position, _gravityDir, groundCheckDistance);

        if (!grounded)
        {
            freeFallTime += Time.deltaTime;
            if (freeFallTime > 3f)
                UIManager.Instance.GameOver();
        }
        else
        {
            freeFallTime = 0;
        }
    }

    private void HandleGravitySelectionInput()
    {
        bool pressed = false;

        if (Input.GetKeyDown(KeyCode.I)) { selectedDir = Vector3.forward; pressed = true; }
        if (Input.GetKeyDown(KeyCode.K)) { selectedDir = Vector3.back; pressed = true; }
        if (Input.GetKeyDown(KeyCode.J)) { selectedDir = Vector3.left; pressed = true; }
        if (Input.GetKeyDown(KeyCode.L)) { selectedDir = Vector3.right; pressed = true; }
        if (Input.GetKeyDown(KeyCode.U)) { selectedDir = Vector3.up; pressed = true; }
        if (Input.GetKeyDown(KeyCode.O)) { selectedDir = Vector3.down; pressed = true; }

        if (pressed)
        {
            isSelecting = true;
            ShowHologramPreview(selectedDir);
        }

        if (isSelecting && Input.GetKeyDown(KeyCode.Return))
        {
            ApplyGravityDirection(selectedDir);
            holo.SetActive(false);
            isSelecting = false;
        }
    }

    private void ShowHologramPreview(Vector3 dir)
    {
        holo.SetActive(true);

        Vector3 gravityUp = -_gravityDir;
        Vector3 previewOffset = Vector3.ProjectOnPlane(dir, gravityUp).normalized;
        if (previewOffset == Vector3.zero) previewOffset = dir.normalized;

        holo.transform.position = transform.position + previewOffset * holoDistance;
        holo.transform.rotation = Quaternion.LookRotation(previewOffset, gravityUp);
    }

    private void ApplyGravityDirection(Vector3 newDir)
    {
        if (newDir == Vector3.zero) return;

        _gravityDir = newDir.normalized;

        Vector3 gravityUp = -_gravityDir;

        baseForward = Vector3.ProjectOnPlane(transform.forward, gravityUp).normalized;

        _yaw = 0;
        _pitch = 0;

        transform.rotation = Quaternion.LookRotation(baseForward, gravityUp);

        playerCam.transform.rotation =
            Quaternion.FromToRotation(Vector3.up, gravityUp) *
            Quaternion.LookRotation(baseForward, gravityUp);
    }

    private void HandleMovement()
    {
        Vector3 gravityUp = -_gravityDir;

        Vector3 camForward = Vector3.ProjectOnPlane(playerCam.transform.forward, gravityUp).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(playerCam.transform.right, gravityUp).normalized;

        Vector2 move = _input.movement;
        Vector3 movementDir = (camForward * move.y + camRight * move.x).normalized;

        Vector3 movementDelta = movementDir * runAcceleration * Time.deltaTime;

        Vector3 currentHorizontal = Vector3.ProjectOnPlane(_cc.velocity, _gravityDir);
        _horizontalVelocity = currentHorizontal + movementDelta;

        if (_horizontalVelocity.sqrMagnitude > 0.0001f)
        {
            Vector3 dragForce = _horizontalVelocity.normalized * drag * Time.deltaTime;
            _horizontalVelocity =
                (_horizontalVelocity.magnitude > drag * Time.deltaTime)
                ? _horizontalVelocity - dragForce
                : Vector3.zero;
        }

        _horizontalVelocity = Vector3.ClampMagnitude(_horizontalVelocity, runSpeed);
    }

    private void HandleGravity()
    {
        bool grounded = Physics.Raycast(transform.position, _gravityDir, groundCheckDistance);

        if (grounded)
        {
            _anim?.SetFalling(false);
            _verticalVelocity = _gravityDir * 2f;
        }
        else
        {
            _anim?.SetFalling(true);
            _verticalVelocity += _gravityDir * gravityMagnitude * gravityMultiplier * Time.deltaTime;
        }
    }

    private void ApplyFinalMovement()
    {
        _cc.Move((_horizontalVelocity + _verticalVelocity) * Time.deltaTime);
    }

    private void HandleAnimation()
    {
        float speed = Vector3.ProjectOnPlane(_cc.velocity, _gravityDir).magnitude;
        _anim?.SetSpeed(speed);
    }

    private void HandleLook()
    {
        _yaw += lookSenseH * _input.look.x;
        _pitch -= lookSenseV * _input.look.y;
        _pitch = Mathf.Clamp(_pitch, -lookLimitV, lookLimitV);

        Vector3 gravityUp = -_gravityDir;

        Quaternion yawRot = Quaternion.AngleAxis(_yaw, gravityUp);
        Vector3 forward = yawRot * baseForward;

        // CHANGE: player rotation always aligns to gravity
        transform.rotation = Quaternion.LookRotation(forward, gravityUp);

        Quaternion worldToGravityUp = Quaternion.FromToRotation(Vector3.up, gravityUp);

        playerCam.transform.rotation =
            worldToGravityUp *
            Quaternion.Euler(_pitch, 0, 0) *
            Quaternion.LookRotation(forward, gravityUp);
    }
}
