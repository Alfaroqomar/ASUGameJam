using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] public GameObject player;
    private Vector3 playerStartPosition;

    private void Awake()
    {
        // Ensure there's only ever one of this object
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        playerStartPosition = player.transform.position;
    }

    public void PlayerDeath() {
        player.transform.position = playerStartPosition;
    }
}
