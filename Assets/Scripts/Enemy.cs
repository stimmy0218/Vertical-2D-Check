using System;
using UnityEngine;
using System.Collections;
public class Enemy : MonoBehaviour
{
    public enum EnemyType
    {
        A,
        B,
        C
    }

    public EnemyType enemyType;

    public float speed = 1f;
    public int health = 1;
    public Sprite[] sprites;
    public SpriteRenderer spriteRenderer;
    public GameObject enemyBulletPrefab;

    public Action onDie;
    private Coroutine coroutine;
    private float delta = 0;
    private float span = 1;

    private Player player;
    public void Init(Player player)
    {
        this.player = player;
    }

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        if (transform.position.y < -5.9f)
        {
            Destroy(this.gameObject);
        }

        Fire();
    }

    private void Fire()
    {
        
        if(enemyType != EnemyType.C)
            return;
            
        delta += Time.deltaTime;
        if (delta >= span)
        {
            //총알 생성한다
            CreateEnemyBullet();
            delta = 0;
        }
    }

    private void CreateEnemyBullet()
    {
        GameObject go = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        EnemyBullet enemyBullet = go.GetComponent<EnemyBullet>();
        Vector3 dir = player.transform.position - this.transform.position;
        
        enemyBullet.Init(dir);     
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        spriteRenderer.sprite = sprites[1];
        
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(ReturnSprite());
        
        if (health <= 0)
        {
            onDie();
            Destroy(this.gameObject);
        }
    }

    IEnumerator ReturnSprite()
    {
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.sprite = sprites[0];
    }
}
