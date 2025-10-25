using System.Collections;
using UnityEngine;

public class SimpleWaveSpawner : MonoBehaviour
{
    public SimpleWaveTimeline wave;
    public Camera cam;            // 비우면 Camera.main
    public float z = 0f;          // 소환 Z

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (wave != null) StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        if (wave.events == null || wave.events.Length == 0) yield break;

        // 시간순 정렬
        var arr = (SimpleWaveTimeline.Event[])wave.events.Clone();
        System.Array.Sort(arr, (a, b) => a.time.CompareTo(b.time));

        float t = 0f;
        int i = 0;
        while (i < arr.Length)
        {
            t += Time.deltaTime;
            var e = arr[i];
            if (t >= e.time)
            {
                int count = Mathf.Max(1, e.count <= 0 ? 1 : e.count);
                float gap = Mathf.Max(0f, e.interval);

                for (int k = 0; k < count; k++)
                {
                    Vector3 pos = VpToWorld(e.vp, z);
                    if (e.prefab != null)
                    {
                        var rot = Quaternion.AngleAxis(e.rotationDeg, Vector3.forward); // ★ 추가
                        Instantiate(e.prefab, pos, rot);
                    }
                    if (gap > 0f && k < count - 1) yield return new WaitForSeconds(gap);
                }


                i++;
            }
            yield return null;
        }
    }

    Vector3 VpToWorld(Vector2 vp, float zPos)
    {
        var c = cam != null ? cam : Camera.main;
        if (c == null) return new Vector3(vp.x, vp.y, zPos);
        var p = c.ViewportToWorldPoint(new Vector3(vp.x, vp.y, Mathf.Abs(c.transform.position.z - zPos)));
        p.z = zPos;
        return p;
    }
}