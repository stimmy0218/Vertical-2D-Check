using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 1;
    private Vector3 dir;

    public void Init(Vector3 dir)
    {
        this.dir = dir;
    }

    void Update()
    {
        transform.Translate(dir.normalized * speed * Time.deltaTime);
    }
}