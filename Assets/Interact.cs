using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;

    private playerscript playerscript;

    public GameObject eanimation;
    public Vector3 animationOffset = new Vector3(0, 1f, 0);

    private GameObject currentAnimation;
    private Transform currentInteractable;

    private void Start()
    {
        playerscript = GetComponent<playerscript>();
    }

    private void Update()
    {
        UpdateInteractionAnimation();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void UpdateInteractionAnimation()
    {
        IInteractable closestInteractable = FindClosestInteractable();

        if (closestInteractable == null)
        {
            if (currentAnimation != null)
            {
                Destroy(currentAnimation);
                currentAnimation = null;
                currentInteractable = null;
            }

            return;
        }

        Component interactableComponent = closestInteractable as Component;

        if (interactableComponent == null)
            return;

        Transform interactableTransform = interactableComponent.transform;

        // If we're already displaying it on this object, do nothing.
        if (currentInteractable == interactableTransform)
            return;

        // Closest object changed.
        if (currentAnimation != null)
        {
            Destroy(currentAnimation);
        }

        currentInteractable = interactableTransform;

        currentAnimation = Instantiate(
            eanimation,
            interactableTransform.position + animationOffset,
            Quaternion.identity,
            interactableTransform
        );
    }

    private void TryInteract()
    {
        IInteractable closestInteractable = FindClosestInteractable();

        if (!playerscript.isgrappled)
        {
            closestInteractable?.Interact();
        }
    }

    private IInteractable FindClosestInteractable()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
            transform.position,
            interactionRadius,
            interactableLayer
        );

        IInteractable closestInteractable = null;
        float closestSqrDistance = Mathf.Infinity;

        foreach (Collider2D nearbyCollider in nearbyColliders)
        {
            IInteractable interactable =
                nearbyCollider.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            Component interactableComponent = interactable as Component;

            if (interactableComponent == null)
                continue;

            Vector2 direction =
                (Vector2)interactableComponent.transform.position
                - (Vector2)transform.position;

            float sqrDistance = direction.sqrMagnitude;

            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closestInteractable = interactable;
            }
        }

        return closestInteractable;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}