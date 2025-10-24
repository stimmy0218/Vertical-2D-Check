using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    private const int MAX_BOOM = 3;
    private const int MAX_POWER = 3;

    public GameObject boomPrefab;
    public GameObject playerBulletPrefab;
    public Transform firePoint;

    private Animator anim;
    public float speed = 1f;
    public int life = 3;
    private int boom;
    private int power;
    private float delta = 0;
    private float span = 0.1f;
    private bool isInvincibility = false;
    bool isBoomTime = false;
    Coroutine boomCoroutine;
    
    public Action onResetPosition;
    public Action onGameOver;
    public Action onBoom;
    public bool isBoom = false;

    private void Start()
    {
        this.anim = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
        Fire();
        Reload();
    }

    void Boom()
    {
        if (!Input.GetButton("Fire2"))
            return;
        
        if (isBoom)
            return;
        isBoom = true;
        onBoom();
    }

    private void Reload()
    {
        delta += Time.deltaTime;
    }

    private void Fire()
    {
        if (!Input.GetButton("Fire1"))
            return;

        if(delta < span)
            return;
        
        Debug.Log("총알 발사!");
        GameObject go = Instantiate(playerBulletPrefab, firePoint.position, transform.rotation);

        delta = 0;
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");   //-1, 0, 1
        float v = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(h, v, 0);

        this.anim.SetInteger("Dir", (int)h);
        
        transform.Translate(dir.normalized * speed * Time.deltaTime);

        float clampX = Mathf.Clamp(transform.position.x, -2.3f, 2.3f);
        float clampY = Mathf.Clamp(transform.position.y, -4.4f, 4.4f);
        this.transform.position = new Vector3(clampX, clampY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isInvincibility || GameManager.Instance.isGameOver)
            return;

        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("EnemyBullet"))
        {
            this.life -= 1;
            
            Debug.Log($"===> life : {this.life}");

            if (this.life < 0)
            {
                Debug.Log("==== GameOver ====");
                onGameOver();
            }
            else
            {
                Invoke("ResetPosition", 1f);
            }

            this.gameObject.SetActive(false);
            
            if(other.gameObject.CompareTag("EnemyBullet"))
            {
                Destroy(other.gameObject);
            }
        }
        
        else if (other.gameObject.CompareTag("Item"))
        {
            Item item = other.gameObject.GetComponent<Item>();
            switch (item.itemType)
            {
                case Item.ItemType.Boom:
                    Debug.Log("Boom");
                    boom++;
                    if (boom >= MAX_BOOM)
                    {
                        //GameManager.Instance.score += 500;
                    }
                    break;
                
                case Item.ItemType.Coin:
                    Debug.Log("Coin");
                    //GameManager.Instance.score += 100;
                    break;
                
                case Item.ItemType.Power:
                    Debug.Log("Power");
                    power++;
                    if (power >= MAX_POWER)
                    {
                        //GameManager.Instance.score += 500;
                    }
                    break;;
            }
            Destroy(other.gameObject);
        }
    }
    
    private void ResetPosition()
    {
        this.transform.position = new Vector3(0, -3.78f, 0);
        this.gameObject.SetActive(true);

        onResetPosition();
    }

    private float deltaInvincibility = 0;
    
    public IEnumerator Invincibility()
    {
        isInvincibility = true;
        Debug.Log("무적 상태 시작");
        
        while (true)
        {
            deltaInvincibility += Time.deltaTime;
            
            //Debug.Log(deltaInvincibility);
            
            gameObject.SetActive(!gameObject.activeSelf);
            
            if (deltaInvincibility >= 0.5f)
            {
                deltaInvincibility = 0;
                break;
            }
            
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("무적 상태 종료");
        isInvincibility = false;
        gameObject.SetActive(true);
    }
}