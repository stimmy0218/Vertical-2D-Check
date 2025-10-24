using UnityEngine;
using TMPro;
public class UIGame : MonoBehaviour
{
    public GameObject[] livesGo;
    public GameObject[] boomsGo;
    public TMP_Text scoreText;
    
    void Start()
    {
        
    }

    public void UpdateLivesGo(int lives)
    {
        //모두 안보여준다 
        foreach(GameObject livesGo in livesGo)
            livesGo.SetActive(false);
        
        //for문으로 보여준다 
        for (int i = 0; i < lives; i++)
        {
            livesGo[i].SetActive(true);
        }
        // lives가 3 일경우 0, 1, 2 보여준다 
        // lives가 2 일경우는 0, 1 보여준다 
        // lives가 1 일경우는 0 보여준다 
        // lives가 0 일경우는 안보여준다
    }

    public void UpdateBoomItemsGo(int booms)
    {
        //모두 안보여준다 
        foreach(GameObject boomGo in boomsGo)
            boomGo.SetActive(false);
        
        //for문으로 보여준다 
        for (int i = 0; i < booms; i++)
        {
            boomsGo[i].SetActive(true);
        }
        // booms가 3 일경우 0, 1, 2 보여준다 
        // booms가 2 일경우는 0, 1 보여준다 
        // booms가 1 일경우는 0 보여준다 
        // booms가 0 일경우는 안보여준다
    }

    public void UpdateScoreText()
    {
        
    }


}