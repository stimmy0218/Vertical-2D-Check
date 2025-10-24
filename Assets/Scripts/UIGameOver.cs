using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameOver : MonoBehaviour
{
    public Button retryButton;

    private void Start()
    {
        retryButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("GameScene");
        });
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }
}