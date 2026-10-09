using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SentryEnemy : EnemyController
{
    [SerializeField] private ProjectileController projectilePrefab;

    [Header("Patrol (offsets from the start position, walked in order and back)")]
    [SerializeField] private List<Vector2> patrolOffsets = new List<Vector2> { new Vector2(3, 0) };
    [SerializeField] private float patrolSpeed = 1.5f;

    [Header("Vision")]
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask wallMask = ~0;

    private readonly List<Vector3> patrolPoints = new List<Vector3>();
    private int patrolIndex;
    private int patrolStep = 1;
    private bool alerted;
    private Vector3 startPosition;

    private void OnEnable()
    {
        startPosition = transform.position;

        patrolPoints.Clear();
        patrolPoints.Add(startPosition);
        foreach (var offset in patrolOffsets)
        {
            patrolPoints.Add(startPosition + (Vector3)offset);
        }
    }

    protected override void ResetEnemy(PlayerController player)
    {
        base.ResetEnemy(player);
        alerted = false;
        patrolIndex = 0;
        patrolStep = 1;
        transform.position = startPosition;
    }

    protected override void Behave()
    {
        alerted = CanSeePlayer();

        if (alerted)
        {
            Vector3 toPlayer = playerController.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, toPlayer);

            if (fireTask is not { IsCompleted: false })
            {
                fireTask = AvatarFire(playerController.transform);
            }

            return;
        }

        Patrol();
    }

    private bool CanSeePlayer()
    {
        Vector2 toPlayer = playerController.transform.position - transform.position;
        float distance = toPlayer.magnitude;
        if (distance > detectionRange)
        {
            return false;
        }

        // Only the vision cone spots the player; once alerted it keeps tracking in all directions.
        if (!alerted && Vector2.Angle(transform.up, toPlayer) > viewAngle / 2f)
        {
            return false;
        }

        Vector2 direction = toPlayer / distance;
        return !TryFindWall(Physics2D.RaycastAll(transform.position, direction, distance, wallMask), direction, out _);
    }

    private void Patrol()
    {
        if (patrolPoints.Count < 2)
        {
            return;
        }

        Vector3 target = patrolPoints[patrolIndex];
        if ((target - transform.position).sqrMagnitude < 0.0001f)
        {
            if (patrolIndex + patrolStep < 0 || patrolIndex + patrolStep >= patrolPoints.Count)
            {
                patrolStep = -patrolStep;
            }

            patrolIndex += patrolStep;
            target = patrolPoints[patrolIndex];
        }

        Vector3 delta = target - transform.position;
        if (delta.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, delta);
        }

        transform.position = Vector3.MoveTowards(transform.position, target, patrolSpeed * Time.deltaTime);
    }

    protected override async Task AvatarFire(object value)
    {
        await Task.Delay((int)(fireCharge * 1000));

        if (health <= 0 || !alerted)
        {
            return;
        }

        Transform target = (Transform)value;
        var direction = (target.position - transform.position).normalized;

        var projectile = Instantiate(projectilePrefab);
        projectile.transform.position = transform.position;
        projectile.SetProjectileDirection(direction);
    }
}
