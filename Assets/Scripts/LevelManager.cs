using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour, Manager
{
    [SerializeField] private PlayerController playerPrefab;
    [SerializeField] private GhostController ghostPrefab;
    [SerializeField] private Transform playerStartPosition;

    private List<List<KeyValuePair<float, KeyValuePair<string, object>>>> ghostActions = new List<List<KeyValuePair<float, KeyValuePair<string, object>>>>();

    private PlayerController playerInstance;
    private List<GhostController> ghostInstances = new List<GhostController>();

    public Action<PlayerController> OnLevelReset;

    private void Awake()
    {
        if(!ManagerLocator.Set<LevelManager>(this))
        {
            Destroy(gameObject);
            return;
        }

        playerInstance = Instantiate(playerPrefab);
        playerInstance.SetPlayerId(0);
        playerInstance.transform.position = playerStartPosition.position;
        playerInstance.OnPlayerDead += OnPlayerDeadHandler;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && playerInstance == null)
        {
            foreach (var ghost in ghostInstances)
            {
                if (ghost != null)
                {
                    Destroy(ghost.gameObject);
                }
            }

            ghostInstances.Clear();

            playerInstance = Instantiate(playerPrefab, playerStartPosition);
            playerInstance.OnPlayerDead += OnPlayerDeadHandler;

            foreach (var action in ghostActions)
            {
                var ghostInstance = Instantiate(ghostPrefab);
                ghostInstance.transform.position = playerInstance.transform.position;
                ghostInstance.SetActionPlayer(action);
                ghostInstances.Add(ghostInstance);
            }

            playerInstance.SetPlayerId(ghostInstances.Count);
            OnLevelReset?.Invoke(playerInstance);
        }
    }

    private void OnPlayerDeadHandler()
    {
        ghostActions.Add(playerInstance.GetActionRecorder());
        playerInstance = null;
    }

    public PlayerController GetPlayerController()
    {
        return playerInstance;
    }

    public AvatarController GetAvatarById(int id)
    {
        if (ghostInstances.Count >= id)
        {
            return playerInstance;
        }

        return ghostInstances[id];
    }
}
