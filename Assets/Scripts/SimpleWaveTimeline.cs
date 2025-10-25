using UnityEngine;

[CreateAssetMenu(menuName = "Shooter/SimpleWaveTimeline")]
public class SimpleWaveTimeline : ScriptableObject
{
    [System.Serializable]
    public struct Event
    {
        public float time;             // t초에
        public GameObject prefab;      // 이 프리팹을
        public Vector2 vp;             // VP 좌표에서
        public int count;              // 몇 마리
        public float interval;         // 순차 간격
        public float rotationDeg;      // ★ 추가: Z축 회전 각도(도)
    }

    public Event[] events;
}
