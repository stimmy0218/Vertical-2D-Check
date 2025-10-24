using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public bool isGameOver = false;
    private float delta;
    private float span = 2;
    public int score = 0;
    public System.Action<Enemy> onCreateEnemy;

    private void Awake()
    {
        GameManager.Instance = this;
    }

    void Update()
    {
        delta += Time.deltaTime;

        if (delta >= span)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, 3)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject go = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            Enemy enemy = go.GetComponent<Enemy>();
            onCreateEnemy(enemy);
            span = Random.Range(1.5f, 2.5f);
            delta = 0; 
        }
    }
}