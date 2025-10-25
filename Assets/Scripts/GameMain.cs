using UnityEngine;

public class GameMain : MonoBehaviour
{
    public Player player;
    public UIGame uiGame;
    public UIGameOver uiGameOver;
    public GameObject boomPrefab;
    
    
    void Start()
    {
        GameManager.Instance.onUpdateScore = () =>
        {
            uiGame.UpdateScoreText();
        };
        player.onHit = () => 
        {
            uiGame.UpdateLivesGo(player.life);
        };
        player.onGetBoomItem = () =>
        {
            uiGame.UpdateBoomItemsGo(player.boom);
        };
        
        player.onBoom = () =>
        {
            uiGame.UpdateBoomItemsGo(player.boom);
            
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
