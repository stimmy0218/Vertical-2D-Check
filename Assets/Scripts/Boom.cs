using System;
using System.Collections;
using UnityEngine;

public class Boom : MonoBehaviour
{
    public Action onFinishBoom;

    IEnumerator Start()
    {
        Enemy[] enemies = GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            enemy.TakeDamage(1000);
        }

        EnemyBullet[] enemyBullets = GameObject.FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None);
        for (int i = 0; enemyBullets.Length > i; i++)
        {
            EnemyBullet enemyBullet = enemyBullets[i];
            Destroy(enemyBullet.gameObject);
        }
        
        
        yield return new WaitForSeconds(0.25f);
        
        // if (onFinishBoom != null)
        // {
        //     onFinishBoom();
        // }
        onFinishBoom?.Invoke();
    }
}
