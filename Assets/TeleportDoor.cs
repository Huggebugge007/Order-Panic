using System.Collections;
using UnityEngine;

public class TeleportDoor : MonoBehaviour, IInteractable
{
    [Header("Teleport")]
    [SerializeField] private Transform destination;


    [Header("Optional state change")]
    [SerializeField] private bool changesTavernState;
    [SerializeField] private bool destinationIsInsideTavern;

    public Transform player;
    playertavern playertavernscript;

    private void Awake()
    {
        if(playertavernscript == null)
            playertavernscript = player.GetComponent<playertavern>();
    }

    public void Interact()
    {
        playertavernscript.UseDoor(
        destination.position,
        changesTavernState,
        destinationIsInsideTavern
);
    }

}