using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class FishAI : MonoBehaviour
{
    [Header("Obstacle Avoidance")]
    public LayerMask wallLayer;
    public float wallLookAhead = 0.7f;
    [Range(8, 32)] public int wallDirectionChecks = 24;
    [Range(0.25f, 1f)] public float wallBodyRadiusScale = 0.75f;
    // =========================================================
    // CORE COMPONENTS (cached)
    // =========================================================
    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private stats fishStats;
    private floatinghealthbar healthbar;
    private BoxCollider2D weaponHitbox;
    public GameObject Weapon;
    public bool poisoned = false;
    Coroutine regenCoroutine;

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
    private float previoushealth;
    private bool frozen;
    private Coroutine freezeRoutine;
    public bool IsFrozen => frozen;

    // =========================================================
    // STATE MACHINE (IMPORTANT FIX)
    // Only ONE state coroutine runs at a time
    // =========================================================
    private Coroutine stateRoutine;

    [Header("Rapid Fire")]
    public float rapidFireDuration = 2f;
    public float rapidFireSpinSpeed = 120;
    public float rapidFireReloadMultiplier = 0.5f;

    private bool rapidFiring;

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

    private SpriteRenderer [] sprites;

    private float timetonextterretorial;
    private float regentimer = 0;


    [Header("Camouflage")]
    private float camoRevealDistance = 0.75f;
    private float camoFadeSpeed = 0.5f;      // alpha per second when hiding
    [Range(0f, 1f)] private float camoMinAlpha = 0f;
    private float camoAlpha = 1f;
    private float camoRevealUntil;

    private CanvasGroup healthbarGroup;

    bool ShouldCamouflage()
    {
        if (Time.time < camoRevealUntil) return false;
        if (currentState == State.Chase) return false;
        if (isAttacking || rapidFiring) return false;
        if (currentTarget != null && DistanceToTarget() < camoRevealDistance) return false;
        return true;
    }

    void LateUpdate()
    {
        if (isDead) return;

        if(!fishStats.Camoflage)return;
        
        float target = ShouldCamouflage() ? camoMinAlpha : 1f;
        float speed = target < camoAlpha ? camoFadeSpeed : camoFadeSpeed * 4f;
        camoAlpha = Mathf.MoveTowards(camoAlpha, target, speed * Time.deltaTime);

        foreach (SpriteRenderer s in sprites)
        {
            Color c = s.color;
            c.a = camoAlpha;
            s.color = c;
        }
        if (healthbarGroup != null)
        {

            healthbarGroup.alpha = Mathf.InverseLerp(camoMinAlpha, 1f, camoAlpha);
        }
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        fishStats = GetComponent<stats>();
        SpriteRenderer sr = Weapon.GetComponentInChildren<SpriteRenderer>();
        weaponHitbox = Weapon.GetComponentInChildren<BoxCollider2D>();
        float weaponSize = sr.bounds.size.x;

        if (fishStats.shooter)
        {
            attackRange = fishStats.shootdistance;
            panicRange = attackRange / 2f;
            safeEvadeDistance = attackRange;
            Destroy(Weapon.GetComponent<damage>());
        }
        else
        {
            attackRange = weaponSize + 0.25f;
            panicRange = attackRange * 2f;
            safeEvadeDistance = attackRange * 4f;
        }

        if(fishStats.reloadtime <= 0)
        {
            attackCooldown = 2f;
        }

        // Stats
        health = fishStats.health;
        maxHealth = health;

        moveSpeed = fishStats.speed;
        aggression = fishStats.aggression;
        reactionMax = fishStats.reactionMax;
        evadeTime = fishStats.evadetime;
        idleTimeBase = fishStats.idlemiddletime;

        rb.gravityScale = 0f;

        sprites = GetComponentsInChildren<SpriteRenderer>();
        previoushealth = health;
    }

    private void OnEnable()
    {
        healthbar = GetComponentInChildren<floatinghealthbar>();

        if (healthbar != null)
        {
            healthbarGroup = healthbar.GetComponent<CanvasGroup>();
            if (healthbarGroup == null)
                healthbarGroup = healthbar.gameObject.AddComponent<CanvasGroup>();
        }

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
    this.enabled = false;

    foreach (SpriteRenderer sprite in sprites)
    {
        sprite.color = Color.red;
    }

    if (regenCoroutine != null)
    {
        StopCoroutine(regenCoroutine);
        regenCoroutine = null;
    }

    return;
}

// Detect health changes
if (health < previoushealth && !poisoned)
{
            if (!frozen)
            {
                StartCoroutine(flashred(Color.white));
            }
            else
            {
                StartCoroutine(flashred(new Color(0/ 255f, 210/255f, 1f)));
            }
   

    regentimer = 0f;

    if (regenCoroutine != null)
    {
        StopCoroutine(regenCoroutine);
        regenCoroutine = null;
    }
}

// Wait 5 seconds after damage before regenerating
if (!poisoned && health < maxHealth && regentimer >= 5f)
{
    if (regenCoroutine == null)
    {
        regenCoroutine = StartCoroutine(regen());
    }
}

regentimer += Time.deltaTime;
previoushealth = health;
        previoushealth = health;

        if (health <= 0)
        {

            Die();

        }

        if(Vector2.Distance(transform.position,currentTarget.position) < fishStats.personalspace && fishStats.terretorial && timetonextterretorial <= 0)
        {
            shootmany();
            timetonextterretorial = fishStats.terretorialattacktime;
        }
        timetonextterretorial-=Time.deltaTime;

    }
    
            


void FixedUpdate()
{
    if (frozen)
    {
        velocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        return;
    }

    rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, velocity, Time.fixedDeltaTime * 6f);
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
        velocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;


        foreach(SpriteRenderer sprite in sprites)
        {
            sprite.color = Color.red;
        }
        StopAllCoroutines();
        isDead = true;
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
    private float nextEngageRollTime;

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

            // Idle and Evade end themselves. Only Chase is re-evaluated here.
            if ((currentState == State.Idle || currentState == State.Evade) && stateRoutine != null)
                continue;

            float dist = DistanceToTarget();
            bool close = dist < attackRange;

            if (close && Time.time >= nextAttackTime)
            {
                TryAttack();

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

            // Aggression decides whether to keep chasing, rolled every few
            // seconds instead of every tick.
            if (Time.time >= nextEngageRollTime)
            {
                nextEngageRollTime = Time.time + Random.Range(2f, 4f);
                if (Random.value > aggression)
                {
                    ChangeState(State.Idle);
                    continue;
                }
            }

            ChangeState(State.Chase);
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

        float nextPanicRoll = 0f;
        while (currentState == State.Idle)
        {
            if (currentTarget != null)
            {
                float dist = Vector2.Distance(transform.position, currentTarget.position);

                // A nearby target should make the fish engage.
                // Aggression is only rolled after an attack, not repeatedly while approaching.
                // Target too close: fight or flee, rolled at most once per second
            if (dist < panicRange && Time.time >= nextPanicRoll)
            {
                nextPanicRoll = Time.time + 1f;
                if (Random.value < aggression)
                {
                    ChangeState(State.Chase);
                }
                else
                {
                    evadeUntilTime = Time.time + evadeTime;
                    ChangeState(State.Evade);
                }
                yield break;
            }

            // Idle window over: only engage if feeling aggressive, else keep hiding
            if (Time.time >= endTime)
            {
                if (dist <= safeEvadeDistance && Random.value < aggression)
                {
                    ChangeState(State.Chase);
                    yield break;
                }
                endTime = Time.time + Random.Range(idleTimeBase * 0.7f, idleTimeBase * 1.3f);
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

            Vector2 dir = GetSteeredDirection(toTarget.normalized);

            RotateTowards(dir);

            float preferred = attackRange * 0.7f;
            float buffer = 0.1f;

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

    }
    IEnumerator waitbeforespin()
    {
        yield return new WaitForSeconds(2);
        StartRapidFire();
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
        if (rapidFiring || frozen) return;

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
        if (desired.sqrMagnitude < 0.001f)
            return transform.right;

        desired.Normalize();

        // Faster movement needs more distance to react to a wall.
        float speedMultiplier = Mathf.Clamp(
            rb.linearVelocity.magnitude / Mathf.Max(moveSpeed, 0.01f),
            1f,
            2.2f
        );

        float lookAhead = wallLookAhead * speedMultiplier;
        float bodyRadius = GetWallCheckRadius();

        Vector2 best = desired;
        float bestScore = float.NegativeInfinity;

        int checks = Mathf.Max(8, wallDirectionChecks);
        float angleStep = 360f / checks;

        // Check the full 360 degrees. This is especially important in the
        // square arena because a fish in a corner may need to move sideways
        // or briefly less directly away from its target to escape the corner.
        for (int i = 0; i < checks; i++)
        {
            float angle = i * angleStep;
            Vector2 test = Quaternion.Euler(0f, 0f, angle) * desired;
            test.Normalize();

            RaycastHit2D hit = Physics2D.CircleCast(
                transform.position,
                bodyRadius,
                test,
                lookAhead,
                wallLayer
            );

            // 1 = completely clear for the entire look-ahead distance.
            // 0 = wall is immediately in front of the fish.
            float clearance = hit.collider == null
                ? 1f
                : Mathf.Clamp01(hit.distance / lookAhead);

            // Prefer the original wanted direction when several paths are safe.
            float desiredAlignment = (Vector2.Dot(test, desired) + 1f) * 0.5f;

            // Slightly prefer directions that do not require an unnecessarily
            // large turn. This stops the fish from flicking between directions.
            float forwardAlignment = (Vector2.Dot(test, transform.right) + 1f) * 0.5f;

            float score =
                clearance * 6f +
                desiredAlignment * 2.5f +
                forwardAlignment * 0.25f;

            // A very close wall should overpower the desire to keep travelling
            // toward the target/escape direction.
            if (hit.collider != null && hit.distance < lookAhead * 0.35f)
            {
                score -= (1f - clearance) * 8f;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = test;
            }
        }

        return best.normalized;
    }

    float GetWallCheckRadius()
    {
        if (bodyCollider == null)
            return 0.2f;

        Bounds bounds = bodyCollider.bounds;

        // Use the smaller half of the collider as the cast radius so the cast
        // represents the fish's body without becoming excessively large for
        // long fish.
        float radius = Mathf.Min(bounds.extents.x, bounds.extents.y);
        radius *= wallBodyRadiusScale;

        return Mathf.Max(radius, 0.05f);
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

            // More aggressive fish retreat for less time when they do choose to evade.
            float escapeLimit = Mathf.Lerp(evadeTime * 1.5f, evadeTime * 0.6f, aggression);

            // Exit conditions
            // Exit conditions
           bool minTimePassed = timeEvading >= evadeTime * 0.5f;

            if (minTimePassed && dist > safeEvadeDistance)
            {
                evadeUntilTime = 0f;
                ChangeState(State.Idle);
                yield break;
            }
            else if (timeEvading > escapeLimit)
            {
                evadeUntilTime = 0f;
                ChangeState(Random.value < aggression ? State.Chase : State.Idle);
                yield break;
            }

            // =====================================================
            // BASE ESCAPE DIRECTION
            // =====================================================
            Vector2 away = ((Vector2)transform.position - (Vector2)currentTarget.position).normalized;

            // =====================================================
            // STEERING (shared full-circle wall avoidance)
            // =====================================================
            Vector2 bestDir = GetSteeredDirection(away);

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

        if(fishStats.reloadtime <= 0)
        {
            nextAttackTime = Time.time + 2;
        }
        else
        {
            nextAttackTime = Time.time + fishStats.reloadtime;
        }


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
        if (rapidFiring|| frozen) return;
        if (Time.time < nextAttackTime) return;

        if (fishStats.shooter)
        {
            shoot();
        }
        else
        {
            StartCoroutine(AttackRoutine());
        }
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
        IEnumerator flashred(Color originalcolor)
    {
        foreach(SpriteRenderer sprite in sprites)
        {
            sprite.color = Color.red;
        }


        yield return new WaitForSeconds(0.3f);

        foreach(SpriteRenderer sprite in sprites)
        {
            sprite.color = originalcolor;
        }
    }

        void shoot()
        {
            nextAttackTime = Time.time + fishStats.reloadtime;
            if (Weapon != null)
            {
            if (!fishStats.burst)
            {
                 float step = 360f / fishStats.shotsatatime;

                for(int i = 0; i < fishStats.shotsatatime; i++)
                {
                    float angle = transform.eulerAngles.z + step * i;

                    GameObject projectile = Instantiate(Weapon,transform.position,Quaternion.Euler(0f, 0f, angle));

                    BoxCollider2D weaponhitbox = projectile.GetComponent<BoxCollider2D>();
                    projectilescript projectilescript = projectile.AddComponent<projectilescript>();
                    projectilescript.Initialize(gameObject);
                    projectilescript.damage = fishStats.damage;
                    projectilescript.shotspeed = fishStats.shotspeed;
                    
                    weaponhitbox.enabled = true;
                }
            }

            else
            {
                StartCoroutine(burstshots(fishStats.shotsatatime, fishStats.bursttime));
            }

            }
        }
        void shootmany()
        {
            nextAttackTime = Time.time + fishStats.reloadtime;
            if (Weapon != null)
            {
                int shots = fishStats.shotsatatime + 3;
                float step = 360f / shots;

                for(int i = 0; i < shots; i++)
                {
                    float angle = transform.eulerAngles.z + step * i;

                    GameObject projectile = Instantiate(Weapon,transform.position,Quaternion.Euler(0f, 0f, angle));

                    BoxCollider2D weaponhitbox = projectile.GetComponent<BoxCollider2D>();
                    projectilescript projectilescript = projectile.AddComponent<projectilescript>();
                    projectilescript.Initialize(gameObject);
                    projectilescript.damage = fishStats.damage;
                    projectilescript.shotspeed = fishStats.shotspeed;
                    
                    weaponhitbox.enabled = true;
                }

            }
        }
        public void StartRapidFire()
        {
            if (isDead || !fishStats.shooter || rapidFiring)
                return;

            StartCoroutine(RapidFireRoutine());
        }

    IEnumerator RapidFireRoutine()
    {
        rapidFiring = true;

        float endTime = Time.time + rapidFireDuration;

        float reload = fishStats.reloadtime > 0
            ? fishStats.reloadtime * 0.2f
            : 1f;

        while (Time.time < endTime && !isDead)
        {
            // Spin the fish
            transform.Rotate(
                0f,
                0f,
                rapidFireSpinSpeed * Time.deltaTime
            );

            // Fire if the reload is finished
            if (Time.time >= nextAttackTime)
            {
                shoot();
                nextAttackTime = Time.time + reload;
            }

            yield return null;
        }

        rapidFiring = false;
    }
    public IEnumerator poision(float poisondamage)
    {
        poisoned = true;

        for(int i = 0;i < 5;i++)
        {
            foreach(SpriteRenderer sprite in sprites)
            {
                sprite.color = new Color(6/255,96/255,31/255);
            }
            yield return new WaitForSeconds(1f);
            health -= poisondamage / 5;
            
            foreach(SpriteRenderer sprite in sprites)
            {
                sprite.color = Color.red;
            }
            yield return new WaitForSeconds(0.3f);
        }

        foreach(SpriteRenderer sprite in sprites)
        {
            sprite.color = Color.white;
        }
        poisoned = false;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isActiveAndEnabled)
            return;

        if(collision.gameObject.tag == "fish")
        {
            if (collision.gameObject.GetComponent<stats>().poisnous)
            {
                if (!poisoned)
                {
                    StartCoroutine(poision(collision.gameObject.GetComponent<stats>().damage));
                }
            }
        }
    }
        IEnumerator regen()
{
    while (health < maxHealth && !poisoned)
    {
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.color =new Color(34/255, 255/255, 0);
        }

        if (health < maxHealth && !poisoned)
        {
            health = Mathf.Min(
                health + maxHealth * 0.01f,
                maxHealth
            );
        }


        yield return new WaitForSeconds(0.3f);



        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.color = Color.white;
        }
        yield return new WaitForSeconds(0.7f);
    }

    regenCoroutine = null;
}
    public IEnumerator heal(float amount)
    {
            foreach(SpriteRenderer sprite in sprites)
            {
                sprite.color =new Color (84/255, 255/255, 0);
            }
            health += amount;

            yield return new WaitForSeconds(0.3f);

            foreach(SpriteRenderer sprite in sprites)
            {
                sprite.color = Color.white;
            }

    }

    IEnumerator burstshots(int shots, float delay)
    {
        for(int i = 0; i < shots; i++)
        {
                    GameObject projectile = Instantiate(Weapon,transform.position,transform.rotation);

                    BoxCollider2D weaponhitbox = projectile.GetComponent<BoxCollider2D>();
                    projectilescript projectilescript = projectile.AddComponent<projectilescript>();
                    projectilescript.damage = fishStats.damage;
                    projectilescript.shotspeed = fishStats.shotspeed;
                    projectilescript.Initialize(gameObject);
                    weaponhitbox.enabled = true;
                    yield return new WaitForSeconds(delay);
        }
        yield return null;
    }

        public void Freeze(float duration)
{
    if (isDead) return;

    if (freezeRoutine != null)
        StopCoroutine(freezeRoutine);

    freezeRoutine = StartCoroutine(FreezeRoutine(duration));
}

IEnumerator FreezeRoutine(float duration)
{
    frozen = true;
    velocity = Vector2.zero;

    foreach (SpriteRenderer sprite in sprites)
        sprite.color = new (0/ 255f, 210/255f, 1f);

    yield return new WaitForSeconds(duration);

    frozen = false;

    foreach (SpriteRenderer sprite in sprites)
        sprite.color = Color.white;

    freezeRoutine = null;
}
    
}
