using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject playerBulletPrefab;
    public Transform firePoint;

    private const int MAX_BOOM = 3;
    private const int MAX_POWER = 3;

    private Animator anim;
    public float speed = 1f;
    public int life = 3;
    public int boom;
    private int power;
    private float delta = 0;
    private float span = 0.1f;
    private bool isInvincibility = false;

    public Action onResetPosition;
    public Action onGameOver;
    public Action onBoom;
    public Action onGetBoomItem;
    public Action onHit;

    public bool isBoom = false;

    private void Start()
    {
        this.anim = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
        Fire();
        Boom();
        Reload();
    }

    private void Boom()
    {
        if (!Input.GetButton("Fire2"))
            return;

        if (isBoom)
            return;

        if (boom <= 0)
        {
            Debug.Log("폭탄이 없습니다.");
            return;
        }

        isBoom = true;
        this.boom--;

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

        if (delta < span)
            return;
        
        GameObject go = Instantiate(playerBulletPrefab, firePoint.position, transform.rotation);

        delta = 0;
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal"); //-1, 0, 1
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
                life = 0;
                Debug.Log("==== GameOver ====");
                onGameOver();
            }
            else
            {
                Invoke("ResetPosition", 1f);
            }

            this.gameObject.SetActive(false);

            if (other.gameObject.CompareTag("EnemyBullet"))
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
                    Debug.Log("폭탄을 획득했다!");
                    boom++;
                    if (boom >= MAX_BOOM)
                    {
                        boom = MAX_BOOM;
                        GameManager.Instance.score += 500;
                    }

                    onGetBoomItem();

                    break;

                case Item.ItemType.Coin:
                    Debug.Log("동전을 획득했다!");
                    GameManager.Instance.score += 1000;
                    break;

                case Item.ItemType.Power:
                    Debug.Log("파워를 획득했다!");
                    power++;
                    if (power >= MAX_POWER)
                    {
                        power = MAX_POWER;
                        GameManager.Instance.score += 500;
                    }

                    break;
            }

            Destroy(item.gameObject);
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