using UnityEngine;

public class GameMain : MonoBehaviour
{
    public Player player;
    public UIGameOver uiGameOver;
    void Start()
    {
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