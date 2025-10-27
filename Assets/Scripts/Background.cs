using UnityEngine;

public class Background : MonoBehaviour
{
    public float speed;
    public int startIndex;
    public int endIndex;
    public Transform[] sprites;
    private float viewHeight;
    
    void Start()
    {
        viewHeight = Camera.main.orthographicSize*2;
    }

    void Scrolling()
    {
        Vector3 curPos = transform.position;
        Vector3 nextPos = Vector3.down*speed*Time.deltaTime;
        transform.position = curPos + nextPos;
        
        if (sprites[endIndex].position.y < -viewHeight)
        {
            Vector3 backSpritePos = sprites[startIndex].localPosition;
            sprites[endIndex].localPosition = backSpritePos + Vector3.up*viewHeight;
            //아래 주석과 동일
            startIndex = endIndex;
            endIndex = (startIndex+1)% sprites.Length;
        }
        
        // int startIndexSave = startIndex;
        // startIndex = endIndex;
        //
        // if (startIndexSave - 1 == -1)
        // {
        //     endIndex = sprites.Length - 1;
        // }
        // else
        // {
        //     endIndex = startIndexSave -1 ;
        // }
        //
        // Debug.Log($"{startIndex} , {endIndex}");
    }
    
    void Update()
    {
        Scrolling();
    }
}
