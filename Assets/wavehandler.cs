using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns a set of wave sprites and continuously scrolls them left -> right.
/// When a wave passes the right edge of the camera's view, it's teleported
/// back to the left edge (Flappy-Bird-pipe style respawn).
///
/// Because positions are calculated from the camera's current view bounds
/// (not fixed world coordinates), this keeps working correctly even as the
/// player/camera moves around the world.
///
/// Setup:
/// 1. Create an empty GameObject in your scene (e.g. "WaveManager").
/// 2. Attach this script to it.
/// 3. Assign your wave sprite as a Prefab in the "Wave Prefab" field.
/// 4. Tweak Wave Count / Speed / Spacing in the Inspector to taste.
///
/// Assumes an orthographic 2D camera (Camera.main).
/// </summary>
public class wavehandler : MonoBehaviour
{
    [Header("Wave Setup")]
    [Tooltip("The wave sprite prefab to spawn and scroll.")]
    public GameObject wavePrefab;

    [Tooltip("How many wave instances to keep alive/looping at once.")]
    public int waveCount = 6;

    [Header("Movement")]
    [Tooltip("Horizontal speed in world units per second.")]
    public float moveSpeed = 2f;

    [Tooltip("Initial horizontal gap between each spawned wave.")]
    public float horizontalSpacing = 3f;

    [Tooltip("Extra distance past the camera edge before a wave respawns/appears, so the pop doesn't happen on-screen.")]
    public float edgeBuffer = 1f;

    [Header("Vertical Placement")]
    [Tooltip("If true, each wave keeps whatever Y position it's given in the prefab. If false, all waves share vertical position from this object.")]
    public bool useIndividualHeights = true;

    private readonly List<Transform> waves = new List<Transform>();
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (wavePrefab == null)
        {
            Debug.LogError("WaveScroller: no Wave Prefab assigned.");
            enabled = false;
            return;
        }

        for (int i = 0; i < waveCount; i++)
        {
            GameObject wave = Instantiate(wavePrefab, transform);
            wave.name = $"Wave_{i}";
            waves.Add(wave.transform);
        }

        PositionWavesInitially();
    }

    void PositionWavesInitially()
    {
        float leftEdge = GetCameraLeftEdge();

        for (int i = 0; i < waves.Count; i++)
        {
            float xPos = leftEdge + i * horizontalSpacing;
            float yPos = useIndividualHeights ? waves[i].position.y : transform.position.y;
            waves[i].position = new Vector3(xPos, yPos, waves[i].position.z);
        }
    }

    void Update()
    {
        if (cam == null) return;

        float rightEdge = GetCameraRightEdge();
        float leftEdge = GetCameraLeftEdge();

        foreach (Transform wave in waves)
        {
            wave.position += Vector3.right * moveSpeed * Time.deltaTime;

            if (wave.position.x > rightEdge + edgeBuffer)
            {
                float respawnX = leftEdge - edgeBuffer;
                wave.position = new Vector3(respawnX, wave.position.y, wave.position.z);
            }
        }
    }

    float GetCameraLeftEdge()
    {
        return cam.transform.position.x - GetCameraHalfWidth();
    }

    float GetCameraRightEdge()
    {
        return cam.transform.position.x + GetCameraHalfWidth();
    }

    float GetCameraHalfWidth()
    {
        return cam.orthographicSize * cam.aspect;
    }
}