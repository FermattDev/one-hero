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
    private AvatarController avatarTracker;
    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionRecorder = new List<KeyValuePair<float, KeyValuePair<string, object>>>();

    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionPlayer = new List<KeyValuePair<float, KeyValuePair<string, object>>>();
    private bool actionStart = false;
    private float timeStart;
    private Task fireTask;
    private int index = 0;
    private Quaternion initialRotation;

    private void Start()
    {
        var levelManager = ManagerLocator.Get<LevelManager>();
        levelManager.OnLevelReset += ResetEnemy;
        playerController = levelManager.GetPlayerController();
        initialRotation = transform.rotation;
    }

    public void ResetEnemy(PlayerController player)
    {
        playerController = player;
        actionStart = true;
        actionPlayer.AddRange(actionRecorder);
        actionRecorder.Clear();
        timeStart = Time.time;
        index = 0;
        transform.rotation = initialRotation;
    }

    protected virtual void AvatarSetTracker(object value)
    {
        int avatarId = (int)value;

        if (avatarId < 0)
        {
            avatarTracker = null;
            return;
        }
        
        var levelManager = ManagerLocator.Get<LevelManager>();
        
        avatarTracker = levelManager.GetAvatarById(avatarId);
        
        if(!actionStart)
        {
            AddActionToRecorder("Tracker", value);
        }
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
            AddActionToRecorder("Fire", value);
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
        var isTrackingAvatar = avatarTracker != null;
        
        if (isTrackingAvatar)
        {
            Vector3 direction = avatarTracker.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
            transform.rotation = rotation;
        }
        
        if (!actionStart)
        {
            if(playerController == null)
            {
                return;
            }
            if(!isTrackingAvatar && (playerController.transform.position - transform.position).magnitude < detectionRange)
            {
                AvatarSetTracker(playerController.GetPlayerId());
            }
            if(isTrackingAvatar && (playerController.transform.position - transform.position).magnitude > detectionRange)
            {
                AvatarSetTracker(-1);
            }

            if (isTrackingAvatar)
            {
                if(fireTask == null || fireTask.IsCompleted)
                {
                    Vector3 direction = avatarTracker.transform.position - transform.position;
                    fireTask = AvatarFire(direction);
                }
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
                    case "Tracker":
                        AvatarSetTracker(actionPlayer[index].Value.Value);
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
