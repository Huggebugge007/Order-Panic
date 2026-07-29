using System.Collections;
using UnityEngine;

public class TeleportDoor : MonoBehaviour
{
    [Header("Teleport")]
    [SerializeField] private Transform destination;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;

    [Header("Optional state change")]
    [SerializeField] private bool changesTavernState;
    [SerializeField] private bool destinationIsInsideTavern;

    public Transform player;
    public float useabledistance;
    playertavern playertavernscript;

    private void Awake()
    {
        if(playertavernscript == null)
            playertavernscript = player.GetComponent<playertavern>();
    }
    private void Update()
    {
        if(Vector2.Distance(transform.position, player.transform.position) < useabledistance)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                playertavernscript.UseDoor(
                    destination.position,
                    changesTavernState,
                    destinationIsInsideTavern
                );
            }
        }

    }
}