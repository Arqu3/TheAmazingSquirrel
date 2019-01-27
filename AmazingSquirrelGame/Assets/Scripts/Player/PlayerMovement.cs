using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour, IMovement
{
    #region Public/Serialized fields

    [Header("Movement variables")]
    [SerializeField]
    float speed = 100f;
    [SerializeField]
    float dragExponent = 0.5f;
    [SerializeField]
    float sprintMultiplier = 1.3f;

    [Header("Rotation variables")]
    [SerializeField]
    float turnspeed = 30f;

    [Header("Jumping variables")]
    [SerializeField]
    float jumpStrength = 200f;

    [Header("Flight")]
    [SerializeField]
    float inputHoldTime = 0.5f;

    [Header("Camera")]
    [SerializeField]
    CameraMovement cameraMovement;

    [Header("Particle systems")]
    [SerializeField]
    ParticleSystem runPS;

    [SerializeField]
    ParticleSystem startRunPS;

    [SerializeField]
    ParticleSystem landPS;

    [Header("Mesh to move transform")]
    [SerializeField]
    Transform meshTransform;

    private Vector3 groundNormal = Vector3.up;
    private const float JumpCD = 0.1f;
    private float timeSinceLastJump = 0f;
    private float timeSincePressedSpace = 0f;

    ParticleSystem.EmissionModule runPSEmission;

    bool flying = false;

    public Vector3 GroundNormal
    {
        get
        {
            float dist = col.height / 2f * 1.1f * transform.lossyScale.y;
            Vector3 position = transform.TransformPoint(col.center);
            Debug.DrawRay(position, -transform.up * dist);
            RaycastHit hit;

            Vector3 offset = Vector3.zero;
            float offsetDist = col.radius * transform.lossyScale.x;

            bool hitGround = false;

            for (int i = 0; i < 1; ++i)
            {
                switch (i)
                {
                    case 0:
                        break;
                    case 1:
                        offset = transform.right * offsetDist;
                        break;
                    case 2:
                        offset *= -1f;
                        break;
                    case 3:
                        offset = transform.forward * offsetDist;
                        break;
                    case 4:
                        offset *= -1f;
                        break;
                    default:
                        break;
                }

                Debug.DrawRay(position + offset, -transform.up * dist);
                if (Physics.Raycast(position + offset, -transform.up * dist, out hit, dist, Physics.AllLayers & ~LayerMask.GetMask("Weapon"), QueryTriggerInteraction.Ignore))
                {
                    hitGround = true;
                    groundNormal = hit.normal;
                    break;
                }
            }

            grounded = hitGround;
            if ( !grounded ) groundNormal = Vector3.up;

            return groundNormal;
        }
    }

    public Vector3 InputVector
    {
        get
        {
            Vector3 i = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

            switch (cameraMovement.GetMode())
            {
                case CameraMovement.Mode.Static:

                    i = ProjectVectorRotated(i, Camera.main.transform.rotation);

                    break;
                case CameraMovement.Mode.ThirdPerson:

                    i = transform.rotation * i;

                    break;
                case CameraMovement.Mode.FirstPerson:

                    i = ProjectVectorRotated(i, cameraRotationTransform.rotation);

                    break;
                default:
                    break;
            }

            return isActive ? Vector3.ClampMagnitude(i, 1f) * speed : Vector3.zero;
        }
    }

    #endregion

    #region Components/Services

    Rigidbody body;
    Animator animator;
    CapsuleCollider col;

    #endregion

    #region Private variables

    bool grounded = false;

    Transform cameraRotationTransform;

    int locks = 0;
    Vector3 lockForward = Vector3.zero;

    Vector3 startPosition;

    bool isActive = true;

    IEnumerator addingForce = null;

    Vector3 previousPosition = Vector3.zero;
    bool previousGrounded = true;

    #endregion

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<CapsuleCollider>();

        runPS?.transform.SetParent(null);
        runPSEmission = runPS.emission;

        startPosition = transform.position;

        meshTransform.SetParent(null);
    }

    private void Start()
    {
        cameraRotationTransform = cameraMovement.GetRotationTransform();
        previousPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
            timeSincePressedSpace = Time.timeSinceLevelLoad;
        }

        if (!grounded && Input.GetKey(KeyCode.Space) && Time.timeSinceLevelLoad - timeSincePressedSpace > inputHoldTime)
        {
            flying = true;
        }

        if (flying)
        {
            if (grounded) flying = false;
            if (Input.GetKeyUp(KeyCode.Space)) flying = false;
        }

        if (previousGrounded != grounded && grounded)
        {
            var ps = Instantiate(landPS);
            ps.transform.position = transform.position;
            Destroy(ps.gameObject, 2f);
        }

        runPSEmission.enabled = grounded && (transform.position - previousPosition).magnitude > 0f && Input.GetKey(KeyCode.LeftShift);
        runPS.transform.position = transform.position;

        meshTransform.position = transform.position;
        meshTransform.rotation = Quaternion.RotateTowards(meshTransform.rotation, transform.rotation, 300f * Time.deltaTime);

        previousGrounded = grounded;
    }

    private void FixedUpdate()
    {
        Vector3 input = InputVector;
        groundNormal = GroundNormal;

        RaycastHit hit;
        float dist = 1f;
        Vector3 direction = input.sqrMagnitude > 0f ? input : transform.position - previousPosition;
        if (Physics.Raycast(transform.position, direction * dist * transform.lossyScale.x, out hit, dist, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.gameObject != gameObject)
                groundNormal = hit.normal;
        }

        UpdateRotation (Vector3.ProjectOnPlane (input.sqrMagnitude > 0f ? Camera.main.transform.forward : transform.forward, groundNormal).normalized);

        if (Input.GetKey(KeyCode.LeftShift) && grounded) input *= sprintMultiplier;
        if (!grounded) body.AddForce(flying ? Vector3.down * 3f * (body.velocity.y > 1f ? body.velocity.y : 1f)  : Vector3.down * 75f);
        else body.AddForce (-transform.up * 20f);

        body.MovePosition (body.position + (input * Time.fixedDeltaTime * 0.05f));

        animator?.SetFloat("Speed", (transform.position - previousPosition).magnitude);
        animator?.SetFloat("SprintMulti", Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f);
        animator?.SetBool("Grounded", grounded);
        animator?.SetBool("Flying", flying);

        AddDrag();
        ProjectVelocityIfGrounded();

        previousPosition = transform.position;
    }

    void Jump()
    {
        if (grounded && Time.timeSinceLevelLoad - timeSinceLastJump > JumpCD)
        {
            body.AddForce(transform.up * jumpStrength);
            timeSinceLastJump = Time.timeSinceLevelLoad;
        }
    }

    void AddDrag()
    {
        if (grounded)
        {
            float estimated = 100f;
            float multi = Mathf.Clamp01(body.velocity.sqrMagnitude / (estimated * estimated));
            float drag = 1f - Mathf.Pow(multi, dragExponent);
            //Debug.Log (drag);
            body.velocity *= drag;

            //Debug.Log (body.velocity.magnitude);
            if (body.velocity.magnitude < 0.1f) body.velocity = Vector3.zero;
        }
    }

    void ProjectVelocityIfGrounded()
    {
        if (grounded) body.velocity = Vector3.ProjectOnPlane(body.velocity, groundNormal).normalized * body.velocity.magnitude;
    }

    Vector3 ProjectVectorRotated(Vector3 vector, Quaternion rotation)
    {
        return Vector3.ProjectOnPlane(rotation * vector, groundNormal).normalized * vector.magnitude;
    }

    void UpdateRotation(Vector3 forward)
    {
        if (forward.sqrMagnitude < 0.1f) return;

        //var a = body.rotation;
        var b = Quaternion.LookRotation(locks > 0 ? lockForward : forward, groundNormal);
        //var ang = Quaternion.Angle(a, b) / 180;
        body.MoveRotation(b);
        //transform.localRotation = Quaternion.RotateTowards(a, b, Mathf.Clamp(0.3f, 1f, ang) * turnspeed * Time.fixedDeltaTime); //Quaternion.SlerpUnclamped (transform.localRotation, Quaternion.LookRotation (locks > 0 ? lockForward : forward, Vector3.up), turnspeed * Time.fixedDeltaTime);
    }

    public Coroutine AddForceDirectional(float strength, float duration, Vector3 dir)
    {
        return StartCoroutine(addingForce = _AddForceDirectional(strength, duration, dir));
    }

    public void StopDirectional()
    {
        if (addingForce != null) StopCoroutine(addingForce);
        addingForce = null;
    }

    private IEnumerator _AddForceDirectional(float strength, float duration, Vector3 dir)
    {
        float timer = 0.0f;

        while(timer < duration)
        {
            timer += Time.fixedDeltaTime;

            body.AddForce(dir * speed * strength);

            yield return new WaitForFixedUpdate();
        }

        addingForce = null;
    }

    public void Lock(Vector3 forward)
    {
        ++locks;
        lockForward = forward;
    }

    public void Unlock()
    {
        locks = Mathf.Clamp(locks - 1, 0, locks + 1);
    }

    public void ResetPositionAndRotation()
    {
        body.MovePosition(startPosition);
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        cameraMovement.ResetRotation();
    }

    public void SetActive(bool state)
    {
        if (state) Debug.Log(state);
        isActive = state;
        cameraMovement.gameObject.SetActive(isActive);
    }

    public bool Enabled
    {
        get
        {
            return enabled;
        }
        set
        {
            enabled = value;
        }
    }
}
