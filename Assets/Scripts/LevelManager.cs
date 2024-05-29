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
            Debug.LogError("More than one LevelManager tried to awake.");
            Destroy(gameObject);
            return;
        }

        playerInstance = Instantiate(playerPrefab);
        playerInstance.AvatarId = 0;
        playerInstance.transform.position = playerStartPosition.position;
        playerInstance.PlayerDead += OnPlayerDeadHandler;
    }

    private void OnDestroy()
    {
        if(playerInstance != null)
        {
            playerInstance.PlayerDead -= OnPlayerDeadHandler;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && playerInstance.gameObject.activeSelf == false)
        {
            foreach (var ghost in ghostInstances)
            {
                if (ghost != null)
                {
                    Destroy(ghost.gameObject);
                }
            }

            ghostInstances.Clear();

            playerInstance.transform.position = playerStartPosition.position;
            playerInstance.transform.rotation = playerStartPosition.rotation;
            playerInstance.gameObject.SetActive(true);

            //TODO: Remove ghost instantiare and reutilize already created ghosts.
            foreach (var action in ghostActions)
            {
                var ghostInstance = Instantiate(ghostPrefab);
                ghostInstance.transform.position = playerInstance.transform.position;
                ghostInstance.SetActionPlayer(action);
                ghostInstance.AvatarId = ghostInstances.Count;
                ghostInstances.Add(ghostInstance);
            }

            playerInstance.AvatarId = ghostInstances.Count;
            OnLevelReset?.Invoke(playerInstance);
        }
    }

    private void OnPlayerDeadHandler()
    {
        var actions = new List<KeyValuePair<float, KeyValuePair<string, object>>>();
        actions.AddRange(playerInstance.GetActionRecorder());
        ghostActions.Add(actions);
        playerInstance.ClearActionRecorder();
    }

    public PlayerController GetPlayerController()
    {
        return playerInstance;
    }

    public AvatarController GetAvatarById(int id)
    {
        if (playerInstance.AvatarId == id)
        {
            return playerInstance;
        }
        
        foreach (var ghostInstance in ghostInstances)
        {
            if (id == ghostInstance.AvatarId)
            {
                return ghostInstance;
            }
        }

        Debug.LogError($"There is no avatar of ID {id}");
        return null;
    }
}
