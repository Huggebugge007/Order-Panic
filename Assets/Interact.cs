using UnityEngine;

public class Interact : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;
    private playerscript playerscript;

    private void Update()
    {
        playerscript = GetComponent<playerscript>();
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
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

            // The interface should be implemented by a MonoBehaviour.
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


        if(!playerscript.isgrappled)
            closestInteractable?.Interact();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}