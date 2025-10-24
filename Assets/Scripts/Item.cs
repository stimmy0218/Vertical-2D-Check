using UnityEngine;

public class Item : MonoBehaviour
{
   public enum ItemType
   {
      Boom,
      Coin,
      Power
   }
    
   public ItemType itemType;
   public float speed = 1;

   void Update()
   {
      transform.Translate(Vector3.down * speed * Time.deltaTime);
      
      if (transform.position.y < -5.9f)
      {
         Destroy(this.gameObject);
      }
   }
}
