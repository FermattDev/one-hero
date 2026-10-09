using System.Threading.Tasks;
using UnityEngine;

public class SpinnerEnemy : EnemyController
{
    private const int DirectionCount = 8;

    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private float spinSpeed = 90f;

    [Header("Boss")]
    [SerializeField] private int bossDirectionCount = 16;
    [SerializeField] private float bossSpinSpeed = 240f;

    protected override void Behave()
    {
        transform.Rotate(0, 0, (isBoss ? bossSpinSpeed : spinSpeed) * Time.deltaTime);

        if ((playerController.transform.position - transform.position).magnitude > detectionRange)
        {
            return;
        }

        if (fireTask is { IsCompleted: false })
        {
            return;
        }

        fireTask = AvatarFire(playerController.transform);
    }

    protected override async Task AvatarFire(object value)
    {
        await Task.Delay((int)(fireCharge * 1000));

        if (health <= 0 || (playerController.transform.position - transform.position).magnitude > detectionRange)
        {
            return;
        }

        int count = isBoss ? bossDirectionCount : DirectionCount;
        for (int i = 0; i < count; i++)
        {
            Vector3 direction = Quaternion.Euler(0, 0, i * (360f / count)) * transform.up;
            SpawnProjectile(projectilePrefab, direction);
        }

        if (!isBoss)
        {
            return;
        }

        // Boss fires a second ring, offset by half a step, shortly after the first.
        await Task.Delay(300);
        if (health <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 direction = Quaternion.Euler(0, 0, (i + 0.5f) * (360f / count)) * transform.up;
            SpawnProjectile(projectilePrefab, direction);
        }
    }
}
