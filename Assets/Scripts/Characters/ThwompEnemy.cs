using System.Threading.Tasks;
using UnityEngine;

public class ThwompEnemy : EnemyController
{
    [Header("Detection (uses Detection Range)")]
    [SerializeField] private bool detectUp;
    [SerializeField] private bool detectDown = true;
    [SerializeField] private bool detectLeft;
    [SerializeField] private bool detectRight;
    [SerializeField] private float laneHalfWidth = 0.5f;

    [Header("Charge")]
    [SerializeField] private float chargeSpeed = 6f;
    [SerializeField] private LayerMask wallMask = ~0;
    [SerializeField] private float wallCheckRadius = 0.4f;
    [SerializeField] private float contactRadius = 0.6f;
    [SerializeField] private bool invulnerable = true;

    // Gap left between the thwomp and the wall it stops at, so it never rests inside the wall collider.
    private const float WallSkin = 0.02f;

    private Vector2 chargeDirection;
    private bool charging;
    private Vector3 startPosition;

    private void OnEnable()
    {
        startPosition = transform.position;
    }

    public override void TakeDamage(int amount)
    {
        if (!invulnerable)
        {
            base.TakeDamage(amount);
        }
    }

    protected override void ResetEnemy(PlayerController player)
    {
        base.ResetEnemy(player);
        charging = false;
        transform.position = startPosition;
    }

    // Thwomp never shoots.
    protected override Task AvatarFire(object value) => Task.CompletedTask;

    protected override void Behave()
    {
        if (!charging && !TryDetectPlayer(out chargeDirection))
        {
            return;
        }

        charging = true;
        Charge();
        TryHitPlayer();
    }

    private bool TryDetectPlayer(out Vector2 direction)
    {
        direction = Vector2.zero;
        Vector2 offset = playerController.transform.position - transform.position;

        if (detectUp && TryLane(offset, Vector2.up)) { direction = Vector2.up; return true; }
        if (detectDown && TryLane(offset, Vector2.down)) { direction = Vector2.down; return true; }
        if (detectLeft && TryLane(offset, Vector2.left)) { direction = Vector2.left; return true; }
        if (detectRight && TryLane(offset, Vector2.right)) { direction = Vector2.right; return true; }
        return false;
    }

    private bool TryLane(Vector2 offset, Vector2 dir)
    {
        float along = Vector2.Dot(offset, dir);
        float across = Mathf.Abs(Vector2.Dot(offset, new Vector2(-dir.y, dir.x)));
        if (along <= 0 || along > detectionRange || across > laneHalfWidth)
        {
            return false;
        }

        // A wall between the thwomp and the player blocks detection.
        return !TryFindWall(Physics2D.RaycastAll(transform.position, dir, along, wallMask), dir, out _);
    }

    private void Charge()
    {
        float step = chargeSpeed * Time.deltaTime;
        var hits = Physics2D.CircleCastAll(transform.position, wallCheckRadius, chargeDirection, step, wallMask);

        if (TryFindWall(hits, chargeDirection, out float distance))
        {
            transform.position += (Vector3)(chargeDirection * Mathf.Max(0f, distance - WallSkin));
            charging = false;
            return;
        }

        transform.position += (Vector3)(chargeDirection * step);
    }

    private void TryHitPlayer()
    {
        if (playerController != null &&
            (playerController.transform.position - transform.position).magnitude <= contactRadius)
        {
            playerController.TakeDamage(1);
        }
    }
}
