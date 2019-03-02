using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraMovement : MonoBehaviour
{
    #region Public/Serialized fields

    public enum Mode
    {
        Static,
        ThirdPerson,
        FirstPerson
    }

    [Header("Mode variables")]
    [SerializeField]
    Mode mode = Mode.Static;

    [Header("Thirdperson variables")]
    [SerializeField]
    Vector3 offset = Vector3.zero;

    [Header("Static variables")]
    [SerializeField]
    float staticOffsetDistance = 10f;

    [Header("General input variables")]
    [SerializeField]
    Transform target;
    [SerializeField]
    Vector2 sensitivity = new Vector2(2f, 2f);

    #endregion

    #region Private variables

    Quaternion startRotation;
    Quaternion followStartRotation;
    Vector3 defaultOffset = Vector3.zero;
    Vector3 currentOffset = Vector3.zero;
    Transform followTransform;

    Vector3 staticOffset = Vector3.zero;

    Vector2 storedRotation;

    int locks = 0;

    Vector2 RotationInput
    {
        get
        {
            return locks > 0 ? Vector2.zero : new Vector2(-Input.GetAxis("Mouse Y") * sensitivity.y, Input.GetAxis("Mouse X") * sensitivity.x) * (Time.timeScale > 0f ? Time.unscaledDeltaTime : 0f);
        }
    }

    #endregion

    private void Awake()
    {
        if (transform.parent) transform.SetParent(null, true);

        startRotation = transform.rotation;
        defaultOffset = currentOffset = offset;

        followTransform = new GameObject("Camera Follow Target").transform;
        followTransform.SetParent(target);
        followTransform.localPosition = Vector3.zero;
        followTransform.rotation = Quaternion.LookRotation(followTransform.forward, Vector3.up);
        followStartRotation = followTransform.rotation;
    }

    private void OnEnable()
    {
        SetMode(mode);
    }

    public Mode GetMode()
    {
        return mode;
    }

    public Transform GetRotationTransform()
    {
        return followTransform;
    }

    public void SetMode(Mode newMode)
    {
        switch (mode = newMode)
        {
            case Mode.Static:

                transform.SetParent(target, true);
                transform.rotation = startRotation;
                transform.localPosition = Vector3.zero;
                transform.position -= transform.forward * staticOffsetDistance;
                staticOffset = transform.position - target.position;

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                //UpdateRenderer(true);

                break;
            case Mode.ThirdPerson:

                defaultOffset = currentOffset = offset;

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                //UpdateRenderer(true);

                break;
            case Mode.FirstPerson:

                defaultOffset = currentOffset = Vector3.zero;

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                //UpdateRenderer(false);

                break;
            default:
                break;
        }
        transform.SetParent(null, true);
    }

    private void FixedUpdate()
    {
        if (mode == Mode.Static)
        {
            transform.position = target.position + staticOffset;
        }
        else
        {
            Vector2 rotation = RotationInput;

            float maxAngle = 70f;

            if (storedRotation.x < maxAngle && rotation.x > 0f) storedRotation.x += rotation.x;
            else if (storedRotation.x > -maxAngle && rotation.x < 0f) storedRotation.x += rotation.x;
            storedRotation.y += rotation.y;

            followTransform.rotation = Quaternion.Euler(storedRotation);

            switch (mode)
            {
                case Mode.Static:
                    break;
                case Mode.ThirdPerson:
                    RaycastHit hit;
                    Vector3 dir = (followTransform.position + followTransform.rotation * defaultOffset) - followTransform.position;
                    if (Physics.Raycast(followTransform.position, dir, out hit, dir.magnitude,
                        Physics.AllLayers 
                        & ~LayerMask.GetMask("Player"),
                        QueryTriggerInteraction.Ignore))
                        currentOffset = followTransform.InverseTransformPoint(hit.point);
                    else currentOffset = defaultOffset;
                    break;
                case Mode.FirstPerson:
                    break;
                default:
                    break;
            }

            transform.position = Vector3.Lerp(transform.position, followTransform.position + followTransform.rotation * currentOffset, 30f * Time.fixedDeltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(followTransform.forward, Vector3.up), 15f * Time.fixedDeltaTime);

            //Used to center camera view even when shaking
            UpdateLookRotation();
        }
    }

    void UpdateLookRotation()
    {
        float dist = 10f;
        //Camera.main.transform.LookAt(followTransform.position + followTransform.forward * dist);
        //Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * dist, Color.red);

        Vector3 dir = ( followTransform.position + followTransform.forward * dist ) - Camera.main.transform.position;
        Camera.main.transform.rotation = Quaternion.RotateTowards(Camera.main.transform.rotation, Quaternion.LookRotation(dir), 15f * Time.fixedDeltaTime);

    }

    public void ResetRotation()
    {
        followTransform.rotation = followStartRotation;
        storedRotation = Vector2.zero;
    }
}
