// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can fast/responsive melee combat with combo chains in Unity still feel weighty?
// Date: 2026-08-10

using UnityEngine;

/// <summary>
/// Prototype-only player controller: movement, a 3-hit buffered combo chain,
/// and a dodge roll with i-frames. Uses legacy Input Manager for speed —
/// production will use the new Input System (see docs/engine-reference/unity/deprecated-apis.md).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerCombatController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 720f; // degrees/sec, snappy turning while attacking

    [Header("Dodge")]
    public float dodgeDistance = 4f;
    public float dodgeDuration = 0.25f;
    public float dodgeIFrameWindow = 0.18f; // seconds of invulnerability within the dodge
    public float dodgeCooldown = 0.4f;

    [Header("Combo")]
    public ComboStep[] comboSteps = new ComboStep[]
    {
        new ComboStep { activeStart = 0.05f, activeEnd = 0.12f, totalDuration = 0.22f, bufferWindow = 0.15f, knockback = 3f },
        new ComboStep { activeStart = 0.04f, activeEnd = 0.11f, totalDuration = 0.20f, bufferWindow = 0.15f, knockback = 4f },
        new ComboStep { activeStart = 0.06f, activeEnd = 0.16f, totalDuration = 0.32f, bufferWindow = 0.0f,  knockback = 8f }, // finisher, no buffer after
    };

    [Header("Refs")]
    public AttackHitbox attackHitbox;
    public Transform cameraTransform;

    CharacterController _controller;
    Vector3 _velocity;

    // Combo state
    int _comboIndex = -1;
    float _comboStepTimer;
    bool _attackQueued;      // true if the player pressed attack during this step's buffer window
    bool _inBufferWindowNow;
    bool _isAttacking;

    // Dodge state
    bool _isDodging;
    bool _isInvulnerable;
    float _dodgeTimer;
    float _dodgeCooldownTimer;
    Vector3 _dodgeDirection;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleDodgeTimers();

        if (!_isDodging)
        {
            HandleMovement();
        }
        else
        {
            HandleDodgeMotion();
        }

        HandleComboTimers();
        HandleAttackInput();

        if (!_isAttacking && !_isDodging && Input.GetButtonDown("Jump") && _dodgeCooldownTimer <= 0f)
        {
            StartDodge();
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Camera-relative movement — flatten camera forward/right onto the XZ plane.
        Vector3 camForward = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 camRight = cameraTransform != null ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized : Vector3.right;
        Vector3 moveDir = (camForward * input.z + camRight * input.x);

        // Movement is slowed but not fully locked while attacking — keeps combat feeling responsive
        // rather than "rooted", which is the fast/weighty tension this prototype is testing.
        float speedMultiplier = _isAttacking ? 0.25f : 1f;
        _velocity.x = moveDir.x * moveSpeed * speedMultiplier;
        _velocity.z = moveDir.z * moveSpeed * speedMultiplier;

        if (moveDir.sqrMagnitude > 0.01f && !_isAttacking)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        ApplyGravityAndMove();
    }

    void ApplyGravityAndMove()
    {
        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
        else
            _velocity.y += Physics.gravity.y * Time.deltaTime;

        _controller.Move(_velocity * Time.deltaTime);
    }

    void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!_isAttacking && !_isDodging)
            {
                StartCombo();
            }
            else if (_isAttacking && _inBufferWindowNow)
            {
                // Only presses that land inside the current step's buffer window queue a chain.
                // A press outside that window is dropped — this is the "buffered but not lenient" feel we're testing.
                _attackQueued = true;
            }
        }
    }

    void StartCombo()
    {
        _comboIndex = 0;
        BeginComboStep();
    }

    void BeginComboStep()
    {
        _isAttacking = true;
        _comboStepTimer = 0f;
        _attackQueued = false;
        if (attackHitbox != null)
            attackHitbox.ArmForStep(_comboIndex, comboSteps[_comboIndex].knockback);
    }

    void HandleComboTimers()
    {
        if (!_isAttacking) return;

        _comboStepTimer += Time.deltaTime;
        ComboStep step = comboSteps[_comboIndex];

        bool inActiveWindow = _comboStepTimer >= step.activeStart && _comboStepTimer <= step.activeEnd;
        if (attackHitbox != null)
            attackHitbox.SetActive(inActiveWindow);

        // Buffer acceptance window: last (bufferWindow) seconds of the step, if there IS a buffer window at all.
        _inBufferWindowNow = step.bufferWindow > 0f && _comboStepTimer >= (step.totalDuration - step.bufferWindow);

        if (_comboStepTimer >= step.totalDuration)
        {
            bool canChain = _comboIndex < comboSteps.Length - 1 && step.bufferWindow > 0f;
            if (canChain && _attackQueued)
            {
                _comboIndex++;
                BeginComboStep();
            }
            else
            {
                EndCombo();
            }
        }
    }

    void EndCombo()
    {
        _isAttacking = false;
        _comboIndex = -1;
        _attackQueued = false;
        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    void StartDodge()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        _dodgeDirection = input.sqrMagnitude > 0.01f
            ? (transform.right * input.x + transform.forward * input.z).normalized
            : transform.forward;

        _isDodging = true;
        _isInvulnerable = true;
        _dodgeTimer = 0f;
        _dodgeCooldownTimer = dodgeCooldown;

        // Dodging cancels an in-progress attack — responsiveness over combo commitment,
        // matches the "fast/responsive" side of the hypothesis.
        EndCombo();
    }

    void HandleDodgeMotion()
    {
        _dodgeTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_dodgeTimer / dodgeDuration);
        // Ease-out so the dodge feels snappy at the start and settles at the end.
        float speed = (dodgeDistance / dodgeDuration) * (1f - t);
        _controller.Move(_dodgeDirection * speed * Time.deltaTime);

        if (_dodgeTimer >= dodgeIFrameWindow)
            _isInvulnerable = false;

        if (_dodgeTimer >= dodgeDuration)
            _isDodging = false;
    }

    void HandleDodgeTimers()
    {
        if (_dodgeCooldownTimer > 0f)
            _dodgeCooldownTimer -= Time.deltaTime;
    }

    public bool IsInvulnerable => _isInvulnerable;

    [System.Serializable]
    public struct ComboStep
    {
        public float activeStart;   // seconds into the step when the hitbox turns on
        public float activeEnd;     // seconds into the step when the hitbox turns off
        public float totalDuration; // total length of this combo step
        public float bufferWindow;  // seconds at the end of the step during which a queued input chains; 0 = finisher, no chain
        public float knockback;     // force applied to whatever this step hits
    }
}
