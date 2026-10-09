using System.Threading.Tasks;
using UnityEngine;

public class ChaserEnemy : EnemyController
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float contactRadius = 0.6f;

    private bool chasing;
    private Vector3 startPosition;

    private void OnEnable()
    {
        startPosition = transform.position;
    }

    protected override void ResetEnemy(PlayerController player)
    {
        base.ResetEnemy(player);
        chasing = false;
        transform.position = startPosition;
    }

    // Used by summoners so minions chase from the moment they spawn.
    public void Aggro()
    {
        chasing = true;
    }

    // Chaser never shoots.
    protected override Task AvatarFire(object value) => Task.CompletedTask;

    protected override void Behave()
    {
        Vector3 toPlayer = playerController.transform.position - transform.position;

        // Once triggered it never gives up: it only stops when it dies or kills the player.
        if (!chasing)
        {
            if (toPlayer.magnitude > detectionRange)
            {
                return;
            }

            chasing = true;
        }

        if (toPlayer.magnitude <= contactRadius)
        {
            playerController.TakeDamage(1);
            return;
        }

        transform.rotation = Quaternion.LookRotation(Vector3.forward, toPlayer);
        transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
    }
}
