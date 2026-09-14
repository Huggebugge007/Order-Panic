using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishAI : MonoBehaviour
{
    [Header("Obstacle Avoidance")]
    public LayerMask wallLayer;
    // =========================================================
    // CORE COMPONENTS (cached)
    // =========================================================
    private Rigidbody2D rb;
    private stats fishStats;
    private floatinghealthbar healthbar;
    private BoxCollider2D weaponHitbox;
    public GameObject Weapon;

    // =========================================================
    // TARGETING
    // =========================================================
    public string[] targetTags = { "Player", "fish" };
    public Transform currentTarget;
    public float scanRadius = 12f;
    public float scanInterval = 0.2f;

    public bool IsDead => isDead;
    private float nextScanTime;

    // =========================================================
    // MOVEMENT
    // =========================================================
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    private Vector2 velocity;
    private Vector2 smoothedDirection;

    // =========================================================
    // COMBAT / STATS
    // =========================================================
    private float attackRange = 4f;
    private float panicRange;
    private float safeEvadeDistance;

    public float attackCooldown = 2f;
    private float nextAttackTime;

    public float health;
    private float maxHealth;

    // =========================================================
    // AI PERSONALITY
    // =========================================================
    [Range(0, 1)]
    public float aggression = 0.7f;

    public float reactionMax = 0.4f;
    public float evadeTime = 3f;
    private float idleTimeBase = 5f;

    private float idleUntilTime;
    private float evadeUntilTime;

    // =========================================================
    // STATE MACHINE (IMPORTANT FIX)
    // Only ONE state coroutine runs at a time
    // =========================================================
    private Coroutine stateRoutine;

    enum State
    {
        Idle,
        Chase,
        Evade
    }

    [SerializeField] private State currentState;

    // =========================================================
    // FLAGS
    // =========================================================
    private bool isDead;
    private bool isAttacking;

    // =========================================================
    // UNITY LIFECYCLE
    // =========================================================
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fishStats = GetComponent<stats>();
        SpriteRenderer sr = Weapon.GetComponentInChildren<SpriteRenderer>();
        weaponHitbox = Weapon.GetComponentInChildren<BoxCollider2D>();
        float weaponSize = sr.bounds.size.x;

        attackRange = weaponSize + 0.25f;
        panicRange = attackRange * 2f;
        safeEvadeDistance = attackRange * 4f;

        // Stats
        health = fishStats.health;
        maxHealth = health;

        moveSpeed = fishStats.speed;
        aggression = fishStats.aggression;
        reactionMax = fishStats.reactionMax;
        evadeTime = fishStats.evadetime;
        idleTimeBase = fishStats.idlemiddletime;

        rb.gravityScale = 0f;


    }

    private void OnEnable()
    {
        healthbar = GetComponentInChildren<floatinghealthbar>();

        StartCoroutine(TargetScanLoop());

        // Start AI loop (ONLY ONE LOOP IN ENTIRE SCRIPT)
        StartCoroutine(AILoop());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        healthbar.updatehealthbar(health, maxHealth);
    }
    void Update()
    {
        if (isDead) return;

        UpdateHealthBar();

        if (health <= 0)
        {
            Die();
        }

    }

    void FixedUpdate()
    {
        rb.linearVelocity = Vector2.Lerp(
            rb.linearVelocity,
            velocity,
            Time.fixedDeltaTime * 6f
        );
    }

    // =========================================================
    // CORE HELPERS
    // =========================================================

    void UpdateHealthBar()
    {
        if (healthbar == null) return;
        healthbar.updatehealthbar(health, maxHealth);
    }

    void Die()
    {
        isDead = true;
        velocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        this.enabled = false;
    }

    // =========================================================
    // STATE MACHINE CONTROL (IMPORTANT FIX)
    // =========================================================
    void ChangeState(State newState)
    {
        if (isDead) return;

        if (currentState == newState && stateRoutine != null)
            return;

        if (stateRoutine != null)
            StopCoroutine(stateRoutine);

        currentState = newState;

        stateRoutine = StartCoroutine(StateRoutine(newState));
    }

    IEnumerator StateRoutine(State state)
    {
        switch (state)
        {
            case State.Idle:
                yield return StartCoroutine(IdleState());
                break;

            case State.Chase:
                yield return StartCoroutine(ChaseState());
                break;

            case State.Evade:
                yield return StartCoroutine(EvadeState());
                break;
        }

        stateRoutine = null;
    }

    // =========================================================
    // AI LOOP (SINGLE BRAIN LOOP)
    // =========================================================
    IEnumerator AILoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(Random.Range(0.1f, reactionMax));

            if (currentTarget == null)
            {
                ChangeState(State.Idle);
                continue;
            }

            float dist = DistanceToTarget();

            bool panic = dist < panicRange;
            bool close = dist < attackRange;
            bool far = dist > safeEvadeDistance;

            // ATTACK FIRST: fire immediately whenever in range, regardless of current state
            if (close && Time.time >= nextAttackTime)
            {
                TryAttack();

                // Only now roll fight-or-flight, once, right after the swing
                if (Random.value > aggression)
                {
                    evadeUntilTime = Time.time + evadeTime;
                    ChangeState(State.Evade);
                }
                else
                {
                    ChangeState(State.Chase);
                }

                continue;
            }

            if (Time.time < evadeUntilTime)
            {
                ChangeState(State.Evade);
            }
            else if (panic && Random.value > aggression)
            {
                ChangeState(State.Evade);
                evadeUntilTime = Time.time + evadeTime;
            }
            else if (far && Random.value < 0.4f)
            {
                ChangeState(State.Idle);
            }
            else if (close)
            {
                ChangeState(State.Chase);
            }
            else if (!close && !panic && Random.value < 0.2f)
            {
                ChangeState(State.Idle);
            }
            else
            {
                ChangeState(State.Chase);
            }
        }
    }

    // =========================================================
    // TARGET SCANNING
    // =========================================================
    IEnumerator TargetScanLoop()
    {
        while (!isDead)
        {
            FindTarget();
            yield return new WaitForSeconds(scanInterval);
        }
    }
    // =========================================================
    // IDLE STATE
    // =========================================================
    IEnumerator IdleState()
    {
        float duration = Random.Range(idleTimeBase * 0.7f, idleTimeBase * 1.3f);
        float endTime = Time.time + duration;

        Vector2 dir = Random.insideUnitCircle.normalized;
        float nextDirChangeTime = Time.time + Random.Range(1f, 2.5f);

        while (currentState == State.Idle)
        {
            if (currentTarget != null)
            {
                float dist = Vector2.Distance(transform.position, currentTarget.position);

                // Panic check
                if (dist < panicRange)
                {
                    if (Random.value > aggression)
                    {
                        evadeUntilTime = Time.time + evadeTime;
                        ChangeState(State.Evade);
                    }
                    else
                    {
                        ChangeState(State.Chase);
                    }
                    yield break;
                }

                // Time to chase: idle window is over and target is within range
                if (Time.time >= endTime && dist <= safeEvadeDistance)
                {
                    ChangeState(State.Chase);
                    yield break;
                }

                // Gentle avoidance bias while idle
                if (dist < safeEvadeDistance)
                {
                    Vector2 away = ((Vector2)transform.position - (Vector2)currentTarget.position).normalized;
                    dir = Vector2.Lerp(dir, away, 0.05f).normalized;
                }
            }
            else if (Time.time >= endTime)
            {
                // No target and idle time elapsed — just pick a fresh wander direction
                endTime = Time.time + Random.Range(idleTimeBase * 0.7f, idleTimeBase * 1.3f);
            }

            // Periodically pick a new casual wander direction
            if (Time.time >= nextDirChangeTime)
            {
                dir = Vector2.Lerp(dir, Random.insideUnitCircle.normalized, 0.5f).normalized;
                nextDirChangeTime = Time.time + Random.Range(1f, 2.5f);
            }

            // Wall avoidance every frame, not just once
            dir = GetSteeredDirection(dir);

            MoveInDirection(dir, moveSpeed * 0.5f);

            yield return null;
        }

        velocity = Vector2.zero;
    }

    // =========================================================
    // CHASE STATE
    // =========================================================
    IEnumerator ChaseState()
    {
        float t = 0f;
        float duration = Random.Range(0.6f, 1.2f);

        while (t < duration && currentTarget != null)
        {
            Vector2 toTarget = (currentTarget.position - transform.position);
            float dist = toTarget.magnitude;

            Vector2 dir = toTarget.normalized;

            RotateTowards(dir);

            float preferred = attackRange * 0.9f;
            float buffer = 0.25f;

            float speed = 0f;

            // Too far → move closer
            if (dist > preferred + buffer)
            {
                float slowRadius = attackRange * 2f;

                float factor = Mathf.Clamp01(
                    (dist - preferred) / (slowRadius - preferred)
                );

                speed = moveSpeed * factor;
            }
            // Too close → slight backoff
            else if (dist < attackRange)
            {
                speed = 0f; // or slight orbit if you want later
            }

            Vector2 moveDir = transform.right * speed;

            velocity = Vector2.Lerp(velocity, moveDir, 0.2f);

            t += Time.deltaTime;
            yield return null;
        }

        // Small chance to switch behavior after chase burst
        if (Random.value < (1f - aggression) * 0.2f)
        {
            evadeUntilTime = Time.time + evadeTime;
            ChangeState(State.Evade);
        }
    }

    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRadius);

        float bestDist = Mathf.Infinity;
        Transform best = null;

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;

            // Skip dead fish
            var otherFish = hit.GetComponent<FishAI>();
            if (otherFish != null && otherFish.IsDead) continue;

            for (int i = 0; i < targetTags.Length; i++)
            {
                if (!hit.CompareTag(targetTags[i])) continue;

                float d = Vector2.Distance(transform.position, hit.transform.position);

                if (d < bestDist)
                {
                    bestDist = d;
                    best = hit.transform;
                }
            }
        }

        currentTarget = best;
    }
    // =========================================================
    // MOVEMENT HELPERS
    // =========================================================
    void MoveInDirection(Vector2 dir, float speed)
    {
        RotateTowards(dir);
        velocity = Vector2.Lerp(velocity, dir * speed, 0.2f);
    }

    void RotateTowards(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f) return;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;

        float newAngle = Mathf.MoveTowardsAngle(
            currentAngle,
            targetAngle,
            rotationSpeed * 120f * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }

    // =========================================================
    // OBSTACLE AVOIDANCE (shared system)
    // =========================================================
    Vector2 GetSteeredDirection(Vector2 desired)
    {
        Vector2 best = desired;
        float bestScore = -999f;

        for (int i = -3; i <= 3; i++)
        {
            float angle = i * 25f;
            Vector2 test = Quaternion.Euler(0, 0, angle) * desired;

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                test,
                2f,
                wallLayer
            );

            float score = 0f;

            if (!hit)
                score += 3f;
            else
                score -= 3f / Mathf.Max(hit.distance, 0.1f);

            score += Vector2.Dot(test, desired);

            if (score > bestScore)
            {
                bestScore = score;
                best = test;
            }
        }

        return best.normalized;
    }
    // =========================================================
    // EVADE STATE
    // =========================================================
    IEnumerator EvadeState()
    {
        float startTime = Time.time;

        smoothedDirection = transform.right;

        while (currentState == State.Evade)
        {
            if (currentTarget == null)
            {
                ChangeState(State.Idle);
                yield break;
            }

            float dist = Vector2.Distance(transform.position, currentTarget.position);
            float timeEvading = Time.time - startTime;

            float escapeLimit = Mathf.Lerp(2f, 6f, aggression);

            // Exit conditions
            // Exit conditions
            if (dist > safeEvadeDistance)
            {
                idleUntilTime = Time.time + Random.Range(1.5f, 3.5f);
                ChangeState(State.Idle);
                yield break;
            }
            else if (timeEvading > escapeLimit)
            {
                ChangeState(State.Chase);
                yield break;
            }

            // =====================================================
            // BASE ESCAPE DIRECTION
            // =====================================================
            Vector2 away = ((Vector2)transform.position - (Vector2)currentTarget.position).normalized;

            // =====================================================
            // STEERING (avoid walls + optimize escape path)
            // =====================================================
            Vector2 bestDir = away;
            float bestScore = -999f;

            for (int i = -3; i <= 3; i++)
            {
                float angle = i * 25f;
                Vector2 testDir = Quaternion.Euler(0, 0, angle) * away;

                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    testDir,
                    2f,
                    wallLayer
                );

                float score = 0f;

                if (!hit)
                    score += 3f;
                else
                    score -= 5f / Mathf.Max(hit.distance, 0.1f);

                // strongly prefer moving away from target
                score += Vector2.Dot(testDir, away) * 2f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDir = testDir;
                }
            }

            // Smooth direction changes
            smoothedDirection = Vector2.Lerp(smoothedDirection, bestDir, 0.25f).normalized;

            RotateTowards(smoothedDirection);

            // Panic speed boost
            float panicBoost = Mathf.Lerp(1f, 2.2f, 1f - Mathf.Clamp01(dist / panicRange));

            velocity = transform.right * moveSpeed * panicBoost;

            yield return null;
        }
    }
    // =========================================================
    // ATTACK STATE (called from AI loop via Chase proximity)
    // =========================================================
    IEnumerator AttackRoutine()
    {
        if (Weapon == null || weaponHitbox == null)
            yield break;

        isAttacking = true;

        nextAttackTime = Time.time + attackCooldown;

        weaponHitbox.enabled = true;

        Vector3 startPos = Weapon.transform.localPosition;
        Vector3 forwardPos = startPos + Vector3.right * 0.25f;

        float duration = 0.8f;
        float t = 0f;

        // Swing forward + return
        while (t < duration)
        {
            float half = duration * 0.5f;

            if (t < half)
            {
                Weapon.transform.localPosition =
                    Vector3.Lerp(startPos, forwardPos, t / half);
            }
            else
            {
                Weapon.transform.localPosition =
                    Vector3.Lerp(forwardPos, startPos, (t - half) / half);
            }

            t += Time.deltaTime;
            yield return null;
        }

        Weapon.transform.localPosition = startPos;
        weaponHitbox.enabled = false;

        isAttacking = false;
    }

    // =========================================================
    // OPTIONAL: external trigger (if something calls attack)
    // =========================================================
    public void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        StartCoroutine(AttackRoutine());
    }
    // =========================================================
    // SAFE DISTANCE CHECK (avoids repeated Distance calls)
    // =========================================================
    float DistanceToTarget()
    {
        if (currentTarget == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, currentTarget.position);
    }

    // =========================================================
    // HARD STOP MOVEMENT (used on death or state switch safety)
    // =========================================================
    void StopMovement()
    {
        velocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }
}
