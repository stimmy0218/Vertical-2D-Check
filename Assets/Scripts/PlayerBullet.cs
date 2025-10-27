using System;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float speed = 1f;
    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (transform.position.y > 5.4f)
        {
            //Destroy(gameObject);
            ObjectPoolManager.Instance.ReleasePlayerBullet0Go(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            enemy.TakeDamage(1);
            
            //Destroy(gameObject);
        }
    }
}
