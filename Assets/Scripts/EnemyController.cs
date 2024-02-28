using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private float detectionRange = 3;
    [SerializeField] protected float fireDelay = 1f;

    private PlayerController playerController;
    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionRecorder = new List<KeyValuePair<float, KeyValuePair<string, object>>>();

    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionPlayer = new List<KeyValuePair<float, KeyValuePair<string, object>>>();
    private bool actionStart = false;
    private float timeStart;
    private Task fireTask;
    private int index = 0;

    private void Start()
    {
        var levelManager = ManagerLocator.Get<LevelManager>();
        levelManager.OnLevelReset += ResetEnemy;
        playerController = levelManager.GetPlayerController();
    }

    public void ResetEnemy(PlayerController player)
    {
        playerController = player;
        actionStart = true;
        timeStart = Time.time;
        index = 0;
    }

    protected virtual async Task AvatarFire(object value)
    {
        Vector3 direction = (Vector3)value;

        var projectile = Instantiate(projectilePrefab);
        projectile.transform.position = transform.position;
        projectile.SetProjectileDirection(direction.normalized);
        projectile.SetProjectileCreator(transform);

        if(!actionStart)
        {
            AddActionToRecorder("Fire", true);
        }

        await Task.Delay((int)(fireDelay * 1000));
    }

    private void AddActionToRecorder(string key, object value)
    {
        var actionValue = new KeyValuePair<string, object>(key, value);
        var action = new KeyValuePair<float, KeyValuePair<string, object>>(Time.time - timeStart, actionValue);
        actionRecorder.Add(action);
    }

    protected virtual void AvatarDead()
    {
        Destroy(gameObject);

        if (!actionStart)
        {
            AddActionToRecorder("Dead", true);
        }
    }

    private void Update()
    {
        if (!actionStart)
        {
            if(playerController == null || (fireTask != null && !fireTask.IsCompleted))
            {
                return;
            }
            if((playerController.transform.position - transform.position).magnitude < detectionRange)
            {
                Vector3 direction = playerController.transform.position - transform.position;
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
                transform.rotation = rotation;

                fireTask = AvatarFire(direction);
            }
            return;
        }
        if (actionPlayer.Count > index)
        {
            if ((actionPlayer[index].Key) < Time.time - timeStart)
            {
                switch (actionPlayer[index].Value.Key)
                {
                    case "Fire":
                        fireTask = AvatarFire(actionPlayer[index].Value.Value);
                        break;
                    case "Dead":
                        AvatarDead();
                        break;
                }

                index++;
            }
        }
        else
        {
            actionStart = false;
        }
    }
}
