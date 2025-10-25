using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 1;
    private Vector3 dir;

    public void Init(Vector3 dir)
    {
        this.dir = dir;
        //DrawArrow.ForDebug2D(this.transform.position, dir, 1000, Color.green);
    }

    void Update()
    {
        transform.Translate(dir.normalized * speed * Time.deltaTime);    
    }
}
