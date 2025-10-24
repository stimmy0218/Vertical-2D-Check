using UnityEngine;

public class GameMain : MonoBehaviour
{
    public Player player;
    public UIGameOver uiGameOver;
    public UIGame uiGame;
    public GameObject boomPrefab;
    
    void Start()
    {

        player.onBoom = () =>
        {
            GameObject boomGo = Instantiate(boomPrefab);
            Boom boom = boomGo.GetComponent<Boom>();
            boom.onFinishBoom = () =>
            {
                Destroy(boomGo);
                player.isBoom = false;
            };
        };
        
        player.onResetPosition = () =>
        {
            Debug.Log($"<color=yellow>{GameManager.Instance.isGameOver}</color>");
            if (GameManager.Instance.isGameOver == false)
            {
                StartCoroutine(this.player.Invincibility());
            }
        };
        player.onGameOver = () =>
        {
            GameManager.Instance.isGameOver = true;
            uiGameOver.Show();
        };
        
        GameManager.Instance.onCreateEnemy = (enemy) =>
        {
            enemy.Init(player);
        };
    }
}