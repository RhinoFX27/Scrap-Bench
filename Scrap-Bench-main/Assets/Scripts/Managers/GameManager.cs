using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerManager playerManager;
    

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }

    }
    private void InitializeManagers()
    {
        playerManager = GetComponentInChildren<PlayerManager>();
        
        if(playerManager == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/PlayerManager");
            if(prefab == null)
            {
                Debug.Log($"PlayerManager prefab not found");
            }
            else
            {
                Instantiate(prefab, transform.position, Quaternion.identity, transform);
                playerManager = GetComponentInChildren<PlayerManager>();
            }
        }
    }
}
