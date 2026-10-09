using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SummonerEnemy : EnemyController
{
    [SerializeField] private ChaserEnemy minionPrefab;
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private int maxMinions = 3;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float keepDistance = 4f;

    private readonly List<ChaserEnemy> minions = new List<ChaserEnemy>();
    private float spawnTimer;
    private Vector3 startPosition;

    private void OnEnable()
    {
        startPosition = transform.position;
    }

    protected override void ResetEnemy(PlayerController player)
    {
        base.ResetEnemy(player);

        foreach (var minion in minions)
        {
            if (minion != null)
            {
                Destroy(minion.gameObject);
            }
        }

        minions.Clear();
        spawnTimer = 0f;
        transform.position = startPosition;
    }

    // Summoner never shoots.
    protected override Task AvatarFire(object value) => Task.CompletedTask;

    protected override void Behave()
    {
        Vector3 toPlayer = playerController.transform.position - transform.position;
        if (toPlayer.magnitude > detectionRange)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(Vector3.forward, toPlayer);

        // Back away from the player to keep its distance.
        if (toPlayer.magnitude < keepDistance)
        {
            transform.position -= toPlayer.normalized * moveSpeed * Time.deltaTime;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer < spawnInterval)
        {
            return;
        }

        minions.RemoveAll(m => m == null || !m.IsAlive);
        if (minions.Count >= maxMinions)
        {
            return;
        }

        spawnTimer = 0f;
        var minion = Instantiate(minionPrefab, transform.position, Quaternion.identity);
        minion.Aggro();
        minions.Add(minion);
    }
}
