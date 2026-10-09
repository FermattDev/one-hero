using System.Threading.Tasks;
using UnityEngine;

public class LaserTurretEnemy : EnemyController
{
    private enum State { Idle, Charging, Firing, Cooldown }

    [SerializeField] private float chargeTime = 1.5f;
    [SerializeField] private float beamDuration = 0.5f;
    [SerializeField] private float cooldown = 2f;
    [SerializeField] private float beamLength = 10f;
    [SerializeField] private float beamWidth = 0.3f;
    [SerializeField] private LayerMask wallMask = ~0;

    [Header("Boss")]
    [SerializeField] private float bossSweepSpeed = 60f;
    [SerializeField] private float bossBeamDurationMultiplier = 3f;
    [SerializeField] private float bossChargeTimeMultiplier = 0.6f;

    private static readonly Color TelegraphColor = new Color(1f, 0.2f, 0.2f, 0.35f);
    private static readonly Color BeamColor = new Color(1f, 0.1f, 0.1f, 0.9f);
    private const float TelegraphWidth = 0.05f;

    private State state = State.Idle;
    private float stateTimer;
    private Vector2 aimDirection;
    private float currentLength;
    private SpriteRenderer beam;

    private void OnEnable()
    {
        if (beam == null)
        {
            // Child of the turret; sprite pivot is the left edge so scale.x is the beam length.
            beam = new GameObject("Beam").AddComponent<SpriteRenderer>();
            beam.transform.SetParent(transform, false);
            beam.sprite = GetWhiteSprite();
            beam.sortingOrder = 50;
        }

        beam.enabled = false;
    }

    protected override void ResetEnemy(PlayerController player)
    {
        base.ResetEnemy(player);
        SetState(State.Idle);
    }

    protected override void AvatarDead()
    {
        SetState(State.Idle);
        base.AvatarDead();
    }

    // Turret fires a beam instead of projectiles.
    protected override Task AvatarFire(object value) => Task.CompletedTask;

    protected override void Behave()
    {
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case State.Idle:
                if (PlayerVisible())
                {
                    Aim();
                    SetState(State.Charging);
                }
                break;

            case State.Charging:
                UpdateBeam(TelegraphWidth, TelegraphColor);
                if (stateTimer >= chargeTime * (isBoss ? bossChargeTimeMultiplier : 1f))
                {
                    SetState(State.Firing);
                }
                break;

            case State.Firing:
                if (isBoss)
                {
                    // Boss beam sweeps across the arena while firing.
                    aimDirection = Quaternion.Euler(0, 0, bossSweepSpeed * Time.deltaTime) * aimDirection;
                    transform.rotation = Quaternion.LookRotation(Vector3.forward, aimDirection);
                }

                UpdateBeam(beamWidth * SizeFactor, BeamColor);
                TryHitPlayer();
                if (stateTimer >= beamDuration * (isBoss ? bossBeamDurationMultiplier : 1f))
                {
                    SetState(State.Cooldown);
                }
                break;

            case State.Cooldown:
                if (stateTimer >= cooldown)
                {
                    SetState(State.Idle);
                }
                break;
        }
    }

    private void SetState(State newState)
    {
        state = newState;
        stateTimer = 0f;

        if (beam != null)
        {
            beam.enabled = newState == State.Charging || newState == State.Firing;
        }
    }

    private bool PlayerVisible()
    {
        Vector2 toPlayer = playerController.transform.position - transform.position;
        float distance = toPlayer.magnitude;
        if (distance > detectionRange)
        {
            return false;
        }

        Vector2 direction = toPlayer / distance;
        return !TryFindWall(Physics2D.RaycastAll(transform.position, direction, distance, wallMask), direction, out _);
    }

    // The aim is locked when charging starts, so the player can dodge.
    private void Aim()
    {
        Vector2 toPlayer = playerController.transform.position - transform.position;
        aimDirection = toPlayer.normalized;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, toPlayer);
    }

    private void UpdateBeam(float width, Color color)
    {
        currentLength = beamLength * SizeFactor;
        if (TryFindWall(Physics2D.RaycastAll(transform.position, aimDirection, beamLength * SizeFactor, wallMask), aimDirection, out float wallDistance))
        {
            currentLength = wallDistance;
        }

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        beam.transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, angle));
        beam.transform.localScale = new Vector3(currentLength, width, 1) / SizeFactor; // parent is scaled for bosses
        beam.color = color;
    }

    private void TryHitPlayer()
    {
        Vector2 toPlayer = playerController.transform.position - transform.position;
        float along = Vector2.Dot(toPlayer, aimDirection);
        float across = Mathf.Abs(Vector2.Dot(toPlayer, new Vector2(-aimDirection.y, aimDirection.x)));

        if (along >= 0 && along <= currentLength && across <= beamWidth * SizeFactor / 2f)
        {
            playerController.TakeDamage(1);
        }
    }
}
