using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;
    
    public List<GameObject> playerBullet0Prefabs;
    public List<GameObject> playerBullet1Prefabs;
    public List<GameObject> playerBullet2Prefabs;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
