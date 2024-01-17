using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private float detectionRange = 3;

    private PlayerController playerController;
    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionRecorder = new List<KeyValuePair<float, KeyValuePair<string, object>>>();

    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionPlayer = new List<KeyValuePair<float, KeyValuePair<string, object>>>();
    private bool actionStart = false;
    private float timeStart;
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

    protected virtual void AvatarFire(object value)
    {
        Vector3 direction = (Vector3)value;

        transform.LookAt(transform.position + direction.normalized);

        var projectile = Instantiate(projectilePrefab);
        projectile.transform.position = transform.position;
        projectile.SetProjectileDirection(direction);
        projectile.SetProjectileCreator(transform);

        if(!actionStart)
        {
            AddActionToRecorder("Fire", true);
        }
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
            if(playerController == null)
            {
                return;
            }
            if((playerController.transform.position - transform.position).magnitude < detectionRange)
            {
                AvatarFire(playerController.transform.position);
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
                        AvatarFire(actionPlayer[index].Value.Value);
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
