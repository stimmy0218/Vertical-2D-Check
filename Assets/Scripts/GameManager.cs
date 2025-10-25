using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public GameObject[] itemPrefabs;
    
    public bool isGameOver = false;
    
    private float delta;
    private float span = 2;
    public int score = 0;

    public System.Action<Enemy> onCreateEnemy;
    public Action onUpdateScore;

    private void Awake()
    {
        GameManager.Instance = this;
    }

    void Update()
    {
        delta += Time.deltaTime;

        if (delta >= span)
        { 
            Debug.Log("적을 생성한다");
            
            //A, B, C       0, 1, 2
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, 3)];
            //0, 1, 2, 3, 4
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)]; 
            
            GameObject go = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            Enemy enemy = go.GetComponent<Enemy>();

            enemy.onDie = () =>
            {
                this.score += 10;
                onUpdateScore();

                GameObject itemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
                GameObject itemGo = Instantiate(itemPrefab);
                itemGo.transform.position = enemy.transform.position;

            };
            onCreateEnemy(enemy);
            
            span = Random.Range(1.5f, 2.5f);
            
            delta = 0;  //대무통을 비운다 
        }
    }
}
