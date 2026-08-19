using System;
using System.Collections;
using UnityEngine;

public class playertavern : MonoBehaviour
{
    public bool insideTavern;

    [Header("References")]
    [SerializeField] private playerscript playerscript;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject image;

    [Header("Rooms")]
    public int unlockedrooms;
    [SerializeField] private GameObject room;
    [SerializeField] private Transform roomparent;
    [SerializeField] private float roomspacing = 1.04f;

    private int currentrooms;
    private float roompos;
    private bool ready;

    private void Start()
    {
        roompos = 0f;
        currentrooms = 1;
        unlockedrooms = 1;
        ready = true;

        if (playerscript == null)
        {
            playerscript = GetComponent<playerscript>();
        }
    }

    private void Update()
    {
        AddUnlockedRooms();
    }

    private void AddUnlockedRooms()
    {
        while (unlockedrooms > currentrooms)
        {
            GameObject roomClone = Instantiate(
                room,
                new Vector2(0f, roompos),
                Quaternion.identity,
                roomparent
            );

            roomClone.transform.localPosition += Vector3.up * 0.12f;

            currentrooms++;
            roompos -= roomspacing;
        }
    }
    public void addroom(int amount)
    {
        unlockedrooms = amount;
    }

    private void UpdateCameraSize()
    {
        cam.orthographicSize = insideTavern ? 1.4f : 2f;
    }

    public void UseDoor(Vector2 destination,bool changesTavernState,bool destinationIsInsideTavern)
    {
        if (!ready)
            return;

        if (changesTavernState)
        {
            insideTavern = destinationIsInsideTavern;
        }

        StartCoroutine(Teleport(destination));
    }

    private IEnumerator Teleport(Vector2 location)
    {
        ready = false;
        playerscript.enabled = false;

        yield return image.GetComponent<fadescript>().FadeIn(2);

        transform.position = location;

        cam.transform.position = new Vector3(location.x, location.y, cam.transform.position.z);
        UpdateCameraSize();

        transform.rotation = Quaternion.identity;

        StartCoroutine(image.GetComponent<fadescript>().FadeOut(2));

        playerscript.enabled = true;
        ready = true;
    }
}