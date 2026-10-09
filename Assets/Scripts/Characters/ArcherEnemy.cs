using System.Threading.Tasks;
using UnityEngine;

public class ArcherEnemy : EnemyController
{
    [SerializeField] private ProjectileController projectilePrefab;

    [Header("Boss")]
    [SerializeField] private int bossPelletCount = 9;
    [SerializeField] private float bossSpreadAngle = 60f;

    protected override async Task AvatarFire(object value)
    {
        await Task.Delay((int)(fireCharge * 1000));

        if (health <= 0 || (playerController.transform.position - transform.position).magnitude > detectionRange)
        {
            return;
        }

        Transform target = (Transform)value;
        var direction = (target.position - transform.position).normalized;

        if (!isBoss)
        {
            SpawnProjectile(projectilePrefab, direction);
            return;
        }

        // Boss: shotgun blast spread evenly around the aim direction.
        for (int i = 0; i < bossPelletCount; i++)
        {
            float t = bossPelletCount > 1 ? (float)i / (bossPelletCount - 1) - 0.5f : 0f;
            SpawnProjectile(projectilePrefab, Quaternion.Euler(0, 0, t * bossSpreadAngle) * direction);
        }
    }
}
