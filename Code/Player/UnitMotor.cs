using EasyCharacterMovement;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GrabCoin.GameWorld.Player
{
    public class UnitMotor : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Header("Movement")]
        [Space(15f)]

        [Tooltip("Change in rotation per second (Deg / s).")]
        public float rotationRate = 540.0f;

        [Tooltip("The character's maximum speed.")]
        public float maxSpeed = 5.0f;

        [Tooltip("Max Acceleration (rate of change of velocity).")]
        public float maxAcceleration = 20.0f;

        [Tooltip("Setting that affects movement control. Higher values allow faster changes in direction.")]
        public float groundFriction = 8.0f;


        [Header("Dash")]
        [Space(15f)]

        [Tooltip("Dash force")]

        public bool isDashActivity = false;

        public float dashForce = 15f;
        
        public float dashDelay = 5f;

        [Header("Levitation")]
        [Space(15f)]

        public bool isLevitateActivity = false;

        public float levitationTime = 5f;

        public Vector3 levitationGravity = Vector3.down * 3f;

        [Header("Jump")]
        [Space(15f)]

        [Tooltip("Max jumps count.")]
        public int maxJumps = 1;

        [Tooltip("Initial velocity (instantaneous vertical velocity) when jumping.")]
        public float jumpImpulse = 6.5f;

        [Tooltip("Friction to apply when falling.")]
        public float airFriction = 0.1f;

        [Range(0.0f, 1.0f)]
        [Tooltip("When falling, amount of horizontal movement control available to the character.\n" +
                 "0 = no control, 1 = full control at max acceleration.")]
        public float airControl = 0.3f;

        [Tooltip("The character's gravity.")]
        public Vector3 gravity = Vector3.down * 9.81f;

        [Header("Aiming")]
        [Space(15f)]

        [Tooltip("The character's aiming speed multiplier.")]
        public float aimingSpeedMultiplier = 1f;

        [Header("Running")]
        [Space(15f)]

        [Tooltip("The character's run speed multiplier.")]
        public float runSpeedMultiplier = 1.75f;


        [Header("Crouching")]
        [Space(15f)]

        [Tooltip("The character's crouch speed multiplier.")]
        public float crouchSpeedMultiplier = 1.75f;

        [Tooltip("Character's height when standing.")]
        public float standingHeight = 2.0f;

        [Tooltip("Character's height when crouching.")]
        public float crouchingHeight = 1.25f;

        [Tooltip("Follow target transform.")]
        public Transform relativeFollowTransform;

        #endregion

        #region FIELDS

        private Coroutine _lateFixedUpdateCoroutine;

        private ThirdPersonCameraController _cmThirdPersonFollow;

        public UnityAction onJump;

        private bool _canDash = true;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Cached CharacterMovement component.
        /// </summary>

        public CharacterMovement characterMovement { get; private set; }

        /// <summary>
        /// Desired movement direction vector in world-space.
        /// </summary>

        public Vector3 movementDirection { get; set; }
        public Vector3 rotateDirection { get; set; }

        /// <summary>
        /// Jump input.
        /// </summary>

        public bool jump { get; private set; } = false;
        public bool holdJump { get; private set; }
        private int jumpsCount { get; set; } = 0;
        private float _dashTimer { get; set; }
        public bool _isLevitating { get; set; } = false;
        //public float _levitationTimer { get; private set; }
        public bool IsDash => isDashActivity && _canDash;

        /// <summary>
        /// Crouch input command.
        /// </summary>

        public bool crouch { get; set; }
        public bool availibleForNewJump { get; set; }
        public bool mining { get; set; }
        public bool die { get; set; }

        /// <summary>
        /// Is the character crouching?
        /// </summary>

        public bool isCrouching { get; protected set; }
        public bool isRunning { get; protected set; }
        public bool isAiming { get; protected set; }

        private bool isGrounded = false;

        #endregion

        #region EVENT HANDLERS

        /// <summary>
        /// FoundGround event handler.
        /// </summary>

        private void Start()
        {
            jumpsCount = maxJumps;
        }

        private void FindGround()
        {
            if (characterMovement.isGrounded && Vector3.Angle(-gravity.normalized, characterMovement.groundNormal) <= characterMovement.slopeLimit)
            {
                _isLevitating = false;
                //_levitationTimer = 0;
                isGrounded = true;
            }
        }

        #endregion

        #region METHODS

        public void SetHoldJump(bool holdJump)
        {
            if (!holdJump)
            {
                this.jump = false;
            }

            if (!holdJump && _isLevitating)
            {
                _isLevitating = false;
                //_levitationTimer = levitationTime;
            }

            if (isLevitateActivity && /*_levitationTimer < levitationTime &&*/ !_isLevitating && !IsGrounded() && characterMovement.velocity.y <= 0 && holdJump)
            {
                _isLevitating = true;
                characterMovement.velocity = characterMovement.velocity.onlyXZ() + levitationGravity * Time.deltaTime;
            }

            this.holdJump = holdJump;
        }

        public void SetJumpDown()
        {
            availibleForNewJump = true;
            jump = true;
        }

        public void SetCrouch(bool crouch)
        {
            this.crouch = crouch; 
        }

        public void SetMining(bool mining)
        {
            this.mining = mining; 
        }

        public void SetDie(bool die)
        {
            this.die = die; 
        }
        
        public void SetAiming(bool aiming)
        {
            isAiming = aiming;
        }
        
        public void SetRun(bool run)
        {
            isRunning = run && !isCrouching; 
        }
        
        public void SetLookDirection(Vector3 direction)
        {
            rotateDirection = direction;
        }

        public void Dash(Vector3 direction)
        {
            if (!isDashActivity)
                return;

            if (_dashTimer < dashDelay || !IsGrounded())
                return;

            _canDash = false;
            _dashTimer = 0;

            characterMovement.PauseGroundConstraint();
            characterMovement.LaunchCharacter(direction.normalized * dashForce, true);
        }

        public void SetMoveDirection(Vector3 dir)
        {
            movementDirection = dir;
        }

        public void SetInput(Vector2 input)
        {
            // Read Input values

            float horizontal = input.x;
            float vertical = input.y;

            // Create a Movement direction vector (in world space)

            movementDirection = Vector3.zero;

            movementDirection += Vector3.forward * vertical;
            movementDirection += Vector3.right * horizontal;

            // Make Sure it won't move faster diagonally

            movementDirection = Vector3.ClampMagnitude(movementDirection, 1.0f);

            // Make movementDirection relative to camera's view direction

            if(relativeFollowTransform)
                movementDirection = movementDirection.relativeTo(relativeFollowTransform);
        }

        /// <summary>
        /// Update character's rotation.
        /// </summary>

        private void UpdateRotation()
        {
            // Rotate towards character's movement direction

            characterMovement.RotateTowards(rotateDirection, rotationRate * Time.deltaTime);
        }

        /// <summary>
        /// Move the character when on walkable ground.
        /// </summary>

        private void GroundedMovement(Vector3 desiredVelocity)
        {
            characterMovement.velocity = Vector3.Lerp(characterMovement.velocity, desiredVelocity,
                1f - Mathf.Exp(-groundFriction * Time.deltaTime));
        }

        /// <summary>
        /// Move the character when falling or on not-walkable ground.
        /// </summary>

        private void NotGroundedMovement(Vector3 desiredVelocity)
        {
            // Current character's velocity

            Vector3 velocity = characterMovement.velocity;

            // If moving into non-walkable ground, limit its contribution.
            // Allow movement parallel, but not into it because that may push us up.

            if (characterMovement.isOnGround && Vector3.Dot(desiredVelocity, characterMovement.groundNormal) < 0.0f)
            {
                Vector3 groundNormal = characterMovement.groundNormal;
                Vector3 groundNormal2D = groundNormal.onlyXZ().normalized;

                desiredVelocity = desiredVelocity.projectedOnPlane(groundNormal2D);
            }

            // If moving...

            if (desiredVelocity != Vector3.zero)
            {
                // Accelerate horizontal velocity towards desired velocity

                Vector3 horizontalVelocity = Vector3.MoveTowards(velocity.onlyXZ(), desiredVelocity,
                    maxAcceleration * airControl * Time.deltaTime);

                // Update velocity preserving gravity effects (vertical velocity)

                velocity = horizontalVelocity + velocity.onlyY();
            }

            // Apply gravity

            velocity += (_isLevitating /*&& _levitationTimer < levitationTime*/) ? levitationGravity * Time.deltaTime : gravity * Time.deltaTime;

            // Apply Air friction (Drag)

            velocity -= velocity * airFriction * Time.deltaTime;

            // Update character's velocity

            characterMovement.velocity = velocity;
        }

        /// <summary>
        /// Handle character's Crouch / UnCrouch.
        /// </summary>

        private void Crouching()
        {
            // Process crouch input command

            if (crouch)
            {
                // If already crouching, return

                if (isCrouching)
                    return;

                // Set capsule crouching height

                characterMovement.SetHeight(crouchingHeight);

                // Update Crouching state

                isCrouching = true;
            }
            else
            {
                // If not crouching, return

                if (!isCrouching)
                    return;

                // Check if character can safely stand up

                if (!characterMovement.CheckHeight(standingHeight))
                {
                    // Character can safely stand up, set capsule standing height

                    characterMovement.SetHeight(standingHeight);

                    // Update crouching state

                    isCrouching = false;
                }
            }
        }

        /// <summary>
        /// Handle jumping state.
        /// </summary>

        private void Jumping()
        {
            if (jump && IsGrounded() || (!IsGrounded() && holdJump && availibleForNewJump && jumpsCount < maxJumps))
            {
                DoJump();
            }
            //if (_isLevitating)
            //{
            //    _levitationTimer += Time.deltaTime;
            //}
            jump = false;
        }

        public virtual void DoJump()
        {
            jump = false;
            isGrounded = false;
            if (availibleForNewJump)
            {
                availibleForNewJump = false;
                jumpsCount++;
            }
            // Pause ground constraint so character can jump off ground

            characterMovement.PauseGroundConstraint();

            // perform the jump

            Vector3 jumpVelocity = Vector3.up * jumpImpulse;

            characterMovement.LaunchCharacter(jumpVelocity, true);
            onJump?.Invoke();

        }

        public bool IsGrounded()
        {
            return characterMovement.isGrounded && isGrounded;
        }

        /// <summary>
        /// Perform character movement.
        /// </summary>

        private void Move()
        {
            float targetSpeed = maxSpeed;

            if (isAiming)
            {
                targetSpeed *= aimingSpeedMultiplier;
            }
            else if (isCrouching && IsGrounded())
            {
                targetSpeed *= crouchSpeedMultiplier;
            }
            else if (isRunning && IsGrounded())
            {
                targetSpeed *= runSpeedMultiplier;
            }

            Vector3 desiredVelocity = movementDirection * targetSpeed;

            // Update character’s velocity based on its grounding status

            if (!jump) 
            {
                if (IsGrounded())
                    GroundedMovement(desiredVelocity);
                else
                    NotGroundedMovement(desiredVelocity);
            }

            // Handle jumping state

            Jumping();

            // Handle crouching state

            Crouching();

            // Perform movement using character's current velocity

            characterMovement.Move();
        }

        /// <summary>
        /// Post-Physics update, used to sync our character with physics.
        /// </summary>

        private void OnLateFixedUpdate()
        {
            FindGround();
            Move();
        }

        private void Update()
        {
            _dashTimer += Time.deltaTime;
            if (!_canDash && _dashTimer >= dashDelay)
                _canDash = true;
            UpdateRotation();
        }

        /// <summary>
        /// Update camera rotation and follow distance.
        /// </summary>

        #endregion

        #region MONOBEHAVIOR

        private void Awake()
        {
            // Cache CharacterMovement component

            characterMovement = GetComponent<CharacterMovement>();
        }

        private void OnEnable()
        {
            // Start LateFixedUpdate coroutine

            if (_lateFixedUpdateCoroutine != null)
                StopCoroutine(_lateFixedUpdateCoroutine);

            _lateFixedUpdateCoroutine = StartCoroutine(LateFixedUpdate());

            // Subscribe to CharacterMovement events
        }

        private void OnDisable()
        {
            // Ends LateFixedUpdate coroutine

            if (_lateFixedUpdateCoroutine != null)
                StopCoroutine(_lateFixedUpdateCoroutine);

            // Un-Subscribe from CharacterMovement events
        }

        private IEnumerator LateFixedUpdate()
        {
            WaitForFixedUpdate waitTime = new WaitForFixedUpdate();

            while (true)
            {
                yield return waitTime;

                OnLateFixedUpdate();
            }
        }

        #endregion
    }
}
