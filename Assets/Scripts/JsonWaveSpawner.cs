using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

/// <summary>
/// 빌드에서도 동작하는 JSON 기반 웨이브 스포너.
/// - TextAsset(JSON) 또는 외부 경로에서 JSON 로드(예: StreamingAssets)
/// - 프리팹 해석: Addressables(선택), Resources.Load, 수동 바인딩(이름→프리팹)
/// - vp 좌표(0~1)→월드 변환, count/interval/rotationDeg 반영
/// </summary>
public class JsonWaveSpawner : MonoBehaviour
{
    [Header("JSON Source")]
    public TextAsset jsonFile;         // 에셋에 포함된 JSON (권장: 간단)
    public string streamingAssetsFile; // StreamingAssets/ 경로 상대값 (선택)

    [Header("Camera & Z")]
    public Camera cam;                 // 비우면 Camera.main
    public float spawnZ = 0f;

    [Header("Prefab Resolve (Fallbacks)")]
    public bool useResources = true;   // PrefabRef.resourcePath 사용 시 Resources.Load
#if UNITY_ADDRESSABLES
    public bool useAddressables = true;// PrefabRef.address 사용 시 Addressables.LoadAssetAsync
#endif
    [Tooltip("이름(name)으로 찾을 때 사용할 수동 바인딩(최후 fallback)")]
    public PrefabBinding[] manualBindings;

    [Serializable] public struct PrefabBinding { public string name; public GameObject prefab; }

    // ---- JSON DTO (런타임 전용, 에디터 의존성 없음) ----
    // 기존 PrefabRefDTO 교체
    [Serializable]
    class PrefabRefDTO
    {
        public string guid;          // 에디터용 (참고)
        public string path;          // 에디터용 (참고)
        public string name;          // 참고/수동 바인딩용
        public string resourcePath;  // ★ 빌드 런타임용(Resources)
        public string address;       // ★ 빌드 런타임용(Addressables, 선택)
    }


    [Serializable] class EventDTO
    {
        public float time;
        public PrefabRefDTO prefab;
        public float[] vp;         // [x,y]
        public int count = 1;
        public float interval = 0f;
        public float rotationDeg = 0f;
    }

    [Serializable] class TimelineDTO
    {
        public List<EventDTO> events = new();
    }

    TimelineDTO timeline;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        string json = null;

        if (jsonFile != null)
        {
            json = jsonFile.text;
        }
        else if (!string.IsNullOrEmpty(streamingAssetsFile))
        {
            // 간단 동기 로드 (모바일/WebGL 등 비동기 필요 시 코루틴으로 전환)
            string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, streamingAssetsFile);
#if UNITY_ANDROID && !UNITY_EDITOR
            // Android의 경우 WWW/UnityWebRequest로 읽어야 할 수 있음 → 간단 구현
            StartCoroutine(LoadStreamingAssetsAndroid(fullPath));
            return;
#else
            if (System.IO.File.Exists(fullPath))
                json = System.IO.File.ReadAllText(fullPath);
            else
                Debug.LogError($"[JsonWaveSpawner] Not found: {fullPath}");
#endif
        }
        else
        {
            Debug.LogError("[JsonWaveSpawner] JSON 소스가 지정되지 않았습니다. (TextAsset 또는 StreamingAssets 경로)");
            return;
        }

        if (!TryParse(json, out timeline)) return;

        // 시간순 정렬 후 실행
        timeline.events.Sort((a, b) => a.time.CompareTo(b.time));
        StartCoroutine(Run());
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    IEnumerator LoadStreamingAssetsAndroid(string uri)
    {
        using (var req = UnityEngine.Networking.UnityWebRequest.Get(uri))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[JsonWaveSpawner] Read fail: {uri} / {req.error}");
                yield break;
            }
            if (!TryParse(req.downloadHandler.text, out timeline)) yield break;
            timeline.events.Sort((a, b) => a.time.CompareTo(b.time));
            StartCoroutine(Run());
        }
    }
#endif

    bool TryParse(string json, out TimelineDTO dto)
    {
        dto = null;
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("[JsonWaveSpawner] JSON 내용이 비었습니다.");
            return false;
        }
        try
        {
            dto = JsonConvert.DeserializeObject<TimelineDTO>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[JsonWaveSpawner] JSON 파싱 실패: {ex.Message}");
            return false;
        }
        if (dto == null || dto.events == null)
        {
            Debug.LogError("[JsonWaveSpawner] JSON에 events가 없습니다.");
            return false;
        }
        return true;
    }

    IEnumerator Run()
    {
        if (timeline == null || timeline.events == null || timeline.events.Count == 0) yield break;

        float t = 0f;
        int i = 0;
        while (i < timeline.events.Count)
        {
            t += Time.deltaTime;
            var e = timeline.events[i];

            if (t >= Mathf.Max(0f, e.time))
            {
                int count = Mathf.Max(1, e.count);
                float gap = Mathf.Max(0f, e.interval);

                // 프리팹 로드 (한 번만)
                GameObject prefab = null;
                yield return ResolvePrefab(e.prefab, result => prefab = result);

                for (int k = 0; k < count; k++)
                {
                    Vector3 pos = VpToWorld(e.vp, spawnZ);
                    Quaternion rot = Quaternion.AngleAxis(e.rotationDeg, Vector3.forward);

                    if (prefab != null) Instantiate(prefab, pos, rot);
                    else Debug.LogWarning("[JsonWaveSpawner] prefab 로드 실패 - 스폰 스킵");

                    if (gap > 0f && k < count - 1) yield return new WaitForSeconds(gap);
                }

                i++;
            }
            yield return null;
        }
    }

    Vector3 VpToWorld(float[] vp, float z)
    {
        Vector2 v = (vp != null && vp.Length >= 2) ? new Vector2(vp[0], vp[1]) : new Vector2(0.5f, 1.1f);
        var c = cam != null ? cam : Camera.main;
        if (c == null) return new Vector3(v.x, v.y, z);
        var p = c.ViewportToWorldPoint(new Vector3(v.x, v.y, Mathf.Abs(c.transform.position.z - z)));
        p.z = z;
        return p;
    }

    // ---------- Prefab Resolve ----------

    IEnumerator ResolvePrefab(PrefabRefDTO p, Action<GameObject> done)
    {
        // 1) Addressables (선택)
#if UNITY_ADDRESSABLES
        if (useAddressables && p != null && !string.IsNullOrEmpty(p.address))
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(p.address);
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                done?.Invoke(handle.Result);
                yield break;
            }
            else
            {
                Debug.LogWarning($"[JsonWaveSpawner] Addressables 로드 실패: {p.address}");
            }
        }
#endif
        // 2) Resources.Load
        if (useResources && p != null && !string.IsNullOrEmpty(p.resourcePath))
        {
            var go = Resources.Load<GameObject>(p.resourcePath);
            if (go != null) { done?.Invoke(go); yield break; }
            Debug.LogWarning($"[JsonWaveSpawner] Resources.Load 실패: {p.resourcePath}");
        }

        // 3) 수동 바인딩 (name 매칭)
        if (p != null && !string.IsNullOrEmpty(p.name) && manualBindings != null)
        {
            for (int i = 0; i < manualBindings.Length; i++)
            {
                if (manualBindings[i].prefab != null && manualBindings[i].name == p.name)
                {
                    done?.Invoke(manualBindings[i].prefab);
                    yield break;
                }
            }
        }

        // 전부 실패
        done?.Invoke(null);
        yield break;
    }
}
