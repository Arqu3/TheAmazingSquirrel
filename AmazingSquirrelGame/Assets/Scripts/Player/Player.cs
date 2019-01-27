using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour, ICollector
{
    #region Public variables

    [Header ("Interact range")]
    [SerializeField]
    float interactRange = 10f;

    [Header("Game variables")]
    public float time = 600f;
    public int foodRequried = 20;

    #endregion

    private List<ICollectable> heldCollectables = new List<ICollectable> ();

    private const float InteractCD = 0.5f;
    float timeSinceLastInteract = 0.0f;

    void Start()
    {
        TimeManager.Instance.ResetData();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quit");
            Application.Quit();
        }

        if (Input.GetMouseButtonDown(0) && (Time.timeSinceLevelLoad - timeSinceLastInteract > InteractCD))
        {
            var colliders = Physics.OverlapSphere (transform.position, interactRange);

            var interactables = (from c in colliders where c.GetComponent<IInteractable>() != null select c.GetComponent<IInteractable>()).ToArray();
            if (interactables.Length > 0)
            {
                var selected = interactables.Length == 1 ? interactables.First() : interactables.OrderBy(x => Vector3.Angle(Camera.main.transform.forward, x.GetGameObject().transform.position - transform.position)).First();
                selected.Interact(new InteractData { source = gameObject });
                timeSinceLastInteract = Time.timeSinceLevelLoad;
            }
        }
    }

    #region Implemented functions

    public void Add ( ICollectable collectable )
    {
        if ( !heldCollectables.Contains (collectable) )
        {
            heldCollectables.Add (collectable);
            if (collectable as CollectableFood) HUD.Current.UpdateFood(heldCollectables.Count);
        }
    }

    #endregion

    public int GetFoodCount()
    {
        return heldCollectables.Select(x => x as CollectableFood != null).Count();
    }

#if UNITY_EDITOR

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere (transform.position, interactRange);
    }

#endif
}
