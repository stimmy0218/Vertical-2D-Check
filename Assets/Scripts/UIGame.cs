using UnityEngine;
using TMPro;
public class UIGame : MonoBehaviour
{
    public GameObject[] livesGo;
    public TMP_Text scoreText;
    
    void Start()
    {
        
    }

    public void UpdateLivesGo()
    {
        // 생명력이 3 일경우 0, 1, 2 보여준다 
        // 생명력이 2 일경우는 0, 1 보여준다 
        // 생명력이 1 일경우는 0 보여준다 
        // 생명력이 0 일경우는 안보여준다
    }

    public void UpdateScoreText()
    {
        
    }


}