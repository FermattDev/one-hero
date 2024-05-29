using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : EntityController
{
    [SerializeField] protected ProjectileController projectilePrefab;
    [SerializeField] protected float detectionRange = 3;
    [SerializeField] protected float fireDelay = 1f;

    protected PlayerController playerController;
    protected AvatarController avatarTracker;
    protected bool actionStart;
    protected Task fireTask;
    protected int index = 0;
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;
    protected LevelManager levelManager;

    protected override void Start()
    {
        base.Start();
        levelManager = ManagerLocator.Get<LevelManager>();
        levelManager.OnLevelReset += ResetEnemy;
        playerController = levelManager.GetPlayerController();
        initialRotation = transform.rotation;
        initialPosition = transform.position;
        
        OnEntityDead += AvatarDead;

    }

    private void OnDestroy()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelReset -= ResetEnemy;
        }
        OnEntityDead -= AvatarDead;
    }

    public void ResetEnemy(PlayerController player)
    {
        ResetEntity();
        playerController = player;
        avatarTracker = null;
        actionStart = true;
        index = 0;
        transform.rotation = initialRotation;
        transform.position = initialPosition;
    }

    protected virtual void AvatarSetTracker(object value)
    {
        int avatarId = (int)value;

        if (avatarId < 0)
        {
            avatarTracker = null;
            return;
        }
        
        avatarTracker = levelManager.GetAvatarById(avatarId);
        
        if(!actionStart)
        {
            AddActionToRecorder("Tracker", value);
        }
    }

    protected virtual async Task AvatarFire()
    {
        if(!actionStart)
        {
            AddActionToRecorder("Fire", true);
        }
    }

    protected virtual void AvatarDead()
    {
        gameObject.SetActive(false);

        if (!actionStart)
        {
            AddActionToRecorder("Dead", true);
        }
        else
        {
            AddLastActionToRecorder("Dead", true, index);
        }
    }

    protected virtual void Update()
    {
        if (!avatarTracker.gameObject.activeSelf)
        {
            avatarTracker = null;
        }
        
        if (ActionRecorder.Count > index)
        {
            if ((ActionRecorder[index].Key) < Time.time - TimeStart)
            {
                switch (ActionRecorder[index].Value.Key)
                {
                    case "Fire":
                        fireTask = AvatarFire();
                        break;
                    case "Dead":
                        AvatarDead();
                        break;
                    case "Tracker":
                        AvatarSetTracker(ActionRecorder[index].Value.Value);
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
