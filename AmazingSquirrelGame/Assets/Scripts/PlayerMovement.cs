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

    [Header("Camera")]
    [SerializeField]
    CameraMovement cameraMovement;

    public Vector3 GroundNormal
    {
        get
        {
            Vector3 normal = Vector3.up;
            float dist = col.height / 2f * 1.025f;
            Vector3 position = transform.TransformPoint(col.center);
            Debug.DrawRay(position, -transform.up * dist);
            RaycastHit hit;

            Vector3 offset = Vector3.zero;
            float offsetDist = col.radius;

            bool hitGround = false;

            for (int i = 0; i < 5; ++i)
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
                    normal = hit.normal;
                    break;
                }
            }

            grounded = hitGround;

            return normal;
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

                    i = ProjectVectorRotated(i, rotationTransform.rotation);


                    break;
                case CameraMovement.Mode.FirstPerson:

                    i = ProjectVectorRotated(i, rotationTransform.rotation);

                    break;
                default:
                    break;
            }

            return isActive ? Vector3.ClampMagnitude(i, 1f) * speed : Vector3.zero;
        }
    }

    public bool Slowed
    {
        get;
        set;
    }

    public bool Sprinting
    {
        get;
        set;
    }

    #endregion

    #region Components/Services

    Rigidbody body;
    Animator animator;
    CapsuleCollider col;

    #endregion

    #region Private variables

    bool grounded = false;

    public bool SprintInputActive
    {
        get
        {
            return UnityEngine.Input.GetKey (KeyCode.LeftShift);
        }
    }

    Transform rotationTransform;

    int locks = 0;
    Vector3 lockForward = Vector3.zero;

    Vector3 startPosition;

    bool isActive = true;

    IEnumerator addingForce = null;

    #endregion

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<CapsuleCollider>();

        startPosition = transform.position;
    }

    private void Start()
    {
        Slowed = false;
        Sprinting = false;

        rotationTransform = cameraMovement.GetRotationTransform();
    }

    private void FixedUpdate()
    {
        Vector3 input = InputVector;

        UpdateRotation(Vector3.ProjectOnPlane(input, Vector3.up));

        if (Slowed) input *= 0.1f;
        else if (Sprinting) input *= sprintMultiplier;
        if (locks <= 0) body.AddForce(input);
        if (!grounded) body.AddForce(Vector3.down * 75f);

        animator.SetFloat("Speed", body.velocity.magnitude / 12f);

        AddDrag();
        ProjectVelocityIfGrounded();
    }

    //private void Update()
    //{
    //    if (animator != null && !body.isKinematic)
    //    {
    //        if (Input.GetKeyDown(KeyCode.LeftControl) && isActive) animator.SetBool("Is Crouching", !animator.GetBool("Is Crouching"));

    //        animator.SetFloat("movement forward", Mathf.Clamp(Vector3.Dot(Vector3.ProjectOnPlane(cameraMovement.transform.right, Vector3.up).normalized, body.velocity), -1, 1));
    //        animator.SetFloat("movement right", Mathf.Clamp(Vector3.Dot(Vector3.ProjectOnPlane(cameraMovement.transform.forward, Vector3.up).normalized, body.velocity), -1, 1));
    //        animator.SetFloat("VelocityMagnitude", Mathf.Max(body.velocity.magnitude * 0.5f, 1));
    //    }
    //}

    void AddDrag()
    {
        float estimated = 100f;
        float multi = Mathf.Clamp01(body.velocity.sqrMagnitude / (estimated * estimated));
        float drag = 1f - Mathf.Pow(multi, dragExponent);
        //Debug.Log (drag);
        Vector3 vel = body.velocity;
        vel.x *= drag;
        vel.z *= drag;
        body.velocity = vel;

        //Debug.Log (body.velocity.magnitude);
        if (body.velocity.magnitude < 0.1f) body.velocity = Vector3.zero;
    }

    void ProjectVelocityIfGrounded()
    {
        if (grounded) body.velocity = Vector3.ProjectOnPlane(body.velocity, GroundNormal).normalized * body.velocity.magnitude;
    }

    Vector3 ProjectVectorRotated(Vector3 vector, Quaternion rotation)
    {
        return Vector3.ProjectOnPlane(rotation * vector, GroundNormal).normalized * vector.magnitude;
    }

    void UpdateRotation(Vector3 forward)
    {
        if (forward.sqrMagnitude < 0.1f) return;

        var a = body.rotation;
        var b = Quaternion.LookRotation(locks > 0 ? lockForward : forward, Vector3.up);
        var ang = Quaternion.Angle(a, b) / 180;
        body.MoveRotation(Quaternion.RotateTowards(a, b, Mathf.Lerp(1f, 20f, ang * ang) * turnspeed * Time.fixedDeltaTime));
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
        //cameraMovement.Lock(false);
        ++locks;
        lockForward = forward;
    }

    public void Unlock()
    {
        locks = Mathf.Clamp(locks - 1, 0, locks + 1);
        //cameraMovement.Unlock(false);
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

            if (!value) cameraMovement.SetOverrideLookatTarget(animator.GetComponentInChildren<Rigidbody>(true).transform);
        }
    }
}
