using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class playertavern : MonoBehaviour
{
    public bool insideTavern;

    [Header("References")]
    [SerializeField] private playerscript playerscript;
    [SerializeField] private handler handler;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject image;

    [Header("Rooms")]
    public int unlockedrooms;
    [SerializeField] private GameObject roomnohole, roomhole;
    GameObject roomClone = null;
    [SerializeField] private Transform roomparent;
    [SerializeField] private float roomspacing = 0.96f;

    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private Vector2 ladderCheckSize = new Vector2(0.8f, 1.5f);

    [SerializeField] private GameObject wanimation;
    [SerializeField] private Vector3 wAnimationOffset = new Vector3(0, 1f, 0);
    [SerializeField] private Vector3 wAnimationScale = Vector3.one;

    private GameObject currentWAnimation;

    private Rigidbody2D rb;
    private int previousamountofrooms;

private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
}

    private void FixedUpdate()
    {
        bool onLadder = Physics2D.OverlapBox(
            transform.position,
            ladderCheckSize,
            0f,
            ladderLayer
        );

        if (onLadder && Input.GetKey(KeyCode.W))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, climbSpeed);
        }
    }

    private int currentrooms;
    private float roompos;
    private bool ready;

    private void Start()
    {
        previousamountofrooms = 0;
        roompos = 0f;
        currentrooms = 1;
        unlockedrooms = 2;
        ready = true;

        if (playerscript == null)
        {
            playerscript = GetComponent<playerscript>();
        }
    }

    private void Update()
    {
        AddUnlockedRooms();
        ladderClimb();
        if(currentrooms != previousamountofrooms)
        {
            StartCoroutine(FindSeatsNextFrame());

        }
        previousamountofrooms = currentrooms;
    }
    private IEnumerator FindSeatsNextFrame()
    {
        yield return null;
        handler.findseats();
    }

    private void ladderClimb()
    {
        bool onLadder = Physics2D.OverlapBox(
            transform.position,
            ladderCheckSize,
            0f,
            ladderLayer
        );

        // Show W prompt
        if (onLadder)
        {
            if (currentWAnimation == null)
            {
                currentWAnimation = Instantiate(
                    wanimation,
                    transform.position + wAnimationOffset,
                    Quaternion.identity
                );

                currentWAnimation.transform.localScale = wAnimationScale;
            }

            // Follow player without rotating/flipping/scaling with player
            currentWAnimation.transform.position =
                transform.position + wAnimationOffset;

            currentWAnimation.transform.rotation = Quaternion.identity;
            currentWAnimation.transform.localScale = wAnimationScale;
        }
        else
        {
            if (currentWAnimation != null)
            {
                Destroy(currentWAnimation);
                currentWAnimation = null;
            }
        }


        if (onLadder && Input.GetKey(KeyCode.W))
        {
            rb.linearVelocity =
                new Vector2(rb.linearVelocity.x / 2, climbSpeed);
        }
    }


    private void AddUnlockedRooms()
    {
        while (unlockedrooms > currentrooms)
        {
            if (roomClone != null)
            {
                Destroy(roomClone);
                roomClone = Instantiate(
                roomhole,
                new Vector2(0f, roompos+roomspacing),
                Quaternion.identity,
                roomparent
            );
                roomClone.transform.localPosition += Vector3.up * 0.12f;
            }
            roomClone = Instantiate(
                roomnohole,
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