using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;
    
    public GameObject playerBullet0Prefabs;
    public GameObject playerBullet1Prefabs;
    public GameObject playerBullet2Prefabs;
    
    List<GameObject> playerBullet0List = new List<GameObject>();
    List<GameObject> playerBullet1List = new List<GameObject>();
    List<GameObject> playerBullet2List = new List<GameObject>();

    void Awake()
    {
        Instance = this;

        Generate();
    }

    void Generate()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject playerBullet0Go = Instantiate<GameObject>(playerBullet0Prefabs, transform);
            playerBullet0Go.SetActive(false);
            playerBullet0List.Add(playerBullet0Go);
            
            GameObject playerBullet1Go = Instantiate<GameObject>(playerBullet1Prefabs, transform);
            playerBullet1Go.SetActive(false);
            playerBullet1List.Add(playerBullet1Go);
            
            GameObject playerBullet2Go = Instantiate<GameObject>(playerBullet2Prefabs, transform);
            playerBullet2Go.SetActive(false);
            playerBullet2List.Add(playerBullet2Go);
        }
    }

    public GameObject GetPlayerBullet0()
    {
        GameObject foundPlayerBullet0Go = null;
        
        bool isAvailableBullet0 = false;
        
        //playerBullet0List 를 순회 하면서 사용가능한 총알이 있는지 검사 한다 
        for (int i = 0; i < playerBullet0List.Count; i++)
        {
            GameObject playerBullet0Go = playerBullet0List[i];
            
            if (!playerBullet0Go.activeSelf)
            {
                isAvailableBullet0 = true;
                break;
            }
        }

        //만약에 사용할수있는 총알이 없다면 만들어서 playerBullet0List에 추가 한다 
        if (isAvailableBullet0 == false)
        {
            GameObject go = Instantiate(playerBullet0Prefabs,  transform);
            go.SetActive(false);
            playerBullet0List.Add(go);
        }
        
        //playerBullet0List 를 순회 하면서 사용가능한 총알이 있는지 검사한다
        for (int i = 0; i < playerBullet0List.Count; i++)
        {
            GameObject playerBullet0Go = playerBullet0List[i];
            if (!playerBullet0Go.activeSelf)
            {
                playerBullet0Go.SetActive(true);
                return playerBullet0Go;
            }
        }
        
        return null;
    }

    public void ReleasePlayerBullet0Go(GameObject playerBullet0Go)
    {
        playerBullet0Go.SetActive(false);
        playerBullet0Go.transform.localPosition = Vector3.zero;
    }
    
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
}
