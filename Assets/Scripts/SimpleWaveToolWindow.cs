// Assets/Editor/SimpleWaveToolWindow.cs
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json; // ★ Newtonsoft.Json 사용

public class SimpleWaveToolWindow : EditorWindow
{
    SimpleWaveTimeline asset;               // 선택된 Wave 에셋
    SerializedObject so;                    // asset을 감싸는 SerializedObject
    SerializedProperty eventsProp;          // events 배열
    ReorderableList list;

    // 타임라인/프리뷰 조작
    float previewTime;                      // 새 이벤트 기본 시간
    float scrubTime;                        // 씬 스크럽 시간(초)
    bool showFuture = true;                 // 미래 이벤트 연하게 표시
    bool fadeByTime = true;                 // 시간차 페이드
    float spawnMarkerRadius = 0.18f;        // 마커 크기
    bool allowDrag = true;                  // 씬에서 드래그 허용

    [MenuItem("Window/Shooter/Simple Wave Tool")]
    static void Open() => GetWindow<SimpleWaveToolWindow>("Simple Wave Tool");

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        if (asset != null) SafeLoadAsset(asset);
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            var newAsset = (SimpleWaveTimeline)EditorGUILayout.ObjectField("Wave", asset, typeof(SimpleWaveTimeline), false);
            if (newAsset != asset)
            {
                asset = newAsset;
                SafeLoadAsset(asset);
            }

            if (GUILayout.Button("Create", GUILayout.Width(80)))
            {
                var path = EditorUtility.SaveFilePanelInProject("Create SimpleWaveTimeline", "SimpleWave", "asset", "");
                if (!string.IsNullOrEmpty(path))
                {
                    var a = CreateInstance<SimpleWaveTimeline>();
                    AssetDatabase.CreateAsset(a, path);
                    AssetDatabase.SaveAssets();
                    asset = a;
                    SafeLoadAsset(asset);
                }
            }

            GUILayout.FlexibleSpace();

            // ★ JSON Export / Import (Newtonsoft.Json)
            using (new EditorGUI.DisabledScope(asset == null))
            {
                if (GUILayout.Button("Export JSON", GUILayout.Width(110)))
                    ExportJson();
            }
            if (GUILayout.Button("Import JSON", GUILayout.Width(110)))
                ImportJson();
        }

        if (asset == null || so == null || eventsProp == null)
        {
            EditorGUILayout.HelpBox("Wave 에셋을 선택하거나 새로 생성하세요.", MessageType.Info);
            return;
        }

        so.Update();

        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            previewTime = EditorGUILayout.Slider("Preview Time", previewTime, 0f, MaxTime());
            if (GUILayout.Button("Sort", GUILayout.Width(60))) SortByTime();
            if (GUILayout.Button("Add",  GUILayout.Width(60))) AddEvent();
        }

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            float maxT = Mathf.Max(MaxTime() + 2f, 5f);
            scrubTime = EditorGUILayout.Slider("Scrub Time (Scene Preview)", scrubTime, 0f, maxT);
            using (new EditorGUILayout.HorizontalScope())
            {
                showFuture = EditorGUILayout.ToggleLeft("Show Future (faint)", showFuture, GUILayout.Width(150));
                fadeByTime = EditorGUILayout.ToggleLeft("Fade by Time Distance", fadeByTime, GUILayout.Width(180));
                spawnMarkerRadius = EditorGUILayout.Slider("Marker Radius", spawnMarkerRadius, 0.06f, 0.4f);
                allowDrag = EditorGUILayout.ToggleLeft("Drag Markers In Scene", allowDrag, GUILayout.Width(170));
            }

            if (Event.current.type == EventType.Repaint)
                SceneView.RepaintAll();
        }

        list?.DoLayoutList();
        so.ApplyModifiedProperties();
    }

    void SafeLoadAsset(SimpleWaveTimeline a)
    {
        if (a == null)
        {
            so = null;
            eventsProp = null;
            list = null;
            return;
        }

        so = new SerializedObject(a);
        eventsProp = so.FindProperty("events");
        if (eventsProp == null)
        {
            Debug.LogError("SimpleWaveTimeline에 'events' 필드를 찾을 수 없습니다. 필드명을 확인하세요.");
            so = null;
            return;
        }

        BuildList();
    }

    void BuildList()
    {
        list = new ReorderableList(so, eventsProp, true, true, true, true);
        list.drawHeaderCallback = r =>
            EditorGUI.LabelField(r, "Spawn Events  (time, prefab, vp, count, interval, rotationDeg)");
        list.onAddCallback = _ => AddEvent();
        list.onReorderCallback = _ => SortByTime();

        list.drawElementCallback = (rect, index, active, focused) =>
        {
            var e = eventsProp.GetArrayElementAtIndex(index);
            float y = rect.y + 2f;
            float h = EditorGUIUtility.singleLineHeight;

            var r1 = new Rect(rect.x, y, 60, h);
            var r2 = new Rect(r1.xMax + 4, y, 160, h);
            var r3 = new Rect(r2.xMax + 4, y, 160, h);
            var r4 = new Rect(r3.xMax + 4, y, 60, h);
            var r5 = new Rect(r4.xMax + 4, y, 70, h);
            var r6 = new Rect(r5.xMax + 4, y, 90, h);

            EditorGUI.PropertyField(r1, e.FindPropertyRelative("time"), GUIContent.none);
            EditorGUI.PropertyField(r2, e.FindPropertyRelative("prefab"), GUIContent.none);
            EditorGUI.PropertyField(r3, e.FindPropertyRelative("vp"), GUIContent.none);
            EditorGUI.PropertyField(r4, e.FindPropertyRelative("count"), GUIContent.none);
            EditorGUI.PropertyField(r5, e.FindPropertyRelative("interval"), GUIContent.none);
            EditorGUI.PropertyField(r6, e.FindPropertyRelative("rotationDeg"), GUIContent.none);
        };

        list.elementHeightCallback = _ => EditorGUIUtility.singleLineHeight + 6f;
    }

    void AddEvent()
    {
        if (so == null || eventsProp == null) return;
        int idx = eventsProp.arraySize;
        eventsProp.InsertArrayElementAtIndex(idx);

        var e = eventsProp.GetArrayElementAtIndex(idx);
        e.FindPropertyRelative("time").floatValue = Mathf.Round(previewTime * 10f) / 10f;
        e.FindPropertyRelative("prefab").objectReferenceValue = null;
        e.FindPropertyRelative("vp").vector2Value = new Vector2(0.5f, 1.1f);
        e.FindPropertyRelative("count").intValue = 1;
        e.FindPropertyRelative("interval").floatValue = 0f;
        var rotProp = e.FindPropertyRelative("rotationDeg");
        if (rotProp != null) rotProp.floatValue = 0f;

        so.ApplyModifiedProperties();
        SortByTime();
    }

    void SortByTime()
    {
        if (asset == null || asset.events == null) return;
        Array.Sort(asset.events, (a, b) => a.time.CompareTo(b.time));
        EditorUtility.SetDirty(asset);
        so?.Update();
    }

    float MaxTime()
    {
        float t = 5f;
        if (asset != null && asset.events != null)
            foreach (var e in asset.events)
                t = Mathf.Max(t, e.time + 0.5f);
        return t;
    }

    // -------- JSON Export / Import (Newtonsoft.Json) --------

    [Serializable]
    class PrefabRefDTO
    {
        public string guid;          // 에디터 편의
        public string path;          // 에디터 편의
        public string name;          // 수동 바인딩/디버그용
        public string resourcePath;  // ★ 빌드 런타임(Resources.Load용) ex) "Prefabs/EnemyA"
        public string address;       // ★ 빌드 런타임(Addressables 키)
    }

    [Serializable]
    class EventDTO
    {
        public float time;
        public PrefabRefDTO prefab;
        public float[] vp;         // [x,y]
        public int count = 1;
        public float interval = 0f;
        public float rotationDeg = 0f;
    }

    [Serializable]
    class TimelineDTO
    {
        public List<EventDTO> events = new();
    }

    void ExportJson()
    {
        if (asset == null) return;

        var dto = new TimelineDTO();
        if (asset.events != null)
        {
            foreach (var ev in asset.events)
            {
                var e = new EventDTO
                {
                    time = ev.time,
                    prefab = MakePrefabRefDTO(ev.prefab),
                    vp = new[] { ev.vp.x, ev.vp.y },
                    count = Mathf.Max(1, ev.count <= 0 ? 1 : ev.count),
                    interval = Mathf.Max(0f, ev.interval),
                    rotationDeg = ev.rotationDeg
                };
                dto.events.Add(e);
            }
        }

        string json = JsonConvert.SerializeObject(
            dto,
            Formatting.Indented,
            new JsonSerializerSettings
            {
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                NullValueHandling = NullValueHandling.Ignore
            }
        );

        string path = EditorUtility.SaveFilePanel("Export SimpleWave JSON", "", "simple_wave.json", "json");
        if (string.IsNullOrEmpty(path)) return;

        File.WriteAllText(path, json);
        EditorUtility.RevealInFinder(path);
        Debug.Log($"[SimpleWave] Exported JSON: {path}");
    }

    void ImportJson()
    {
        string path = EditorUtility.OpenFilePanel("Import SimpleWave JSON", "", "json");
        if (string.IsNullOrEmpty(path)) return;

        string json = File.ReadAllText(path);
        TimelineDTO dto = null;
        try
        {
            dto = JsonConvert.DeserializeObject<TimelineDTO>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SimpleWave] JSON 파싱 실패: {ex.Message}");
            return;
        }

        if (dto == null || dto.events == null)
        {
            Debug.LogError("[SimpleWave] JSON에 events가 없습니다.");
            return;
        }

        // DTO -> ScriptableObject 적용
        Undo.RecordObject(asset, "Import SimpleWave JSON");
        var list = new List<SimpleWaveTimeline.Event>(dto.events.Count);
        foreach (var e in dto.events)
        {
            var ev = new SimpleWaveTimeline.Event
            {
                time = Mathf.Max(0f, e.time),
                prefab = ResolvePrefab(e.prefab), // 에디터 미리보기 복원
                vp = (e.vp != null && e.vp.Length >= 2) ? new Vector2(e.vp[0], e.vp[1]) : new Vector2(0.5f, 1.1f),
                count = Mathf.Max(1, e.count),
                interval = Mathf.Max(0f, e.interval),
                rotationDeg = e.rotationDeg
            };
            list.Add(ev);
        }

        asset.events = list.ToArray();
        EditorUtility.SetDirty(asset);
        SafeLoadAsset(asset);
        Repaint();

        Debug.Log($"[SimpleWave] Imported JSON: {path}  (events: {asset.events.Length})");
    }

    // ★ Export 시 resourcePath/address까지 채워 넣기
    PrefabRefDTO MakePrefabRefDTO(GameObject prefab)
    {
        if (prefab == null) return null;
        var dto = new PrefabRefDTO();

#if UNITY_EDITOR
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        if (!string.IsNullOrEmpty(assetPath))
        {
            dto.path = assetPath;
            dto.guid = AssetDatabase.AssetPathToGUID(assetPath);

            // Resources 경로 자동 도출: Assets/Resources/ 이후를 resourcePath로
            const string key = "/Resources/";
            int i = assetPath.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (i >= 0)
            {
                string sub = assetPath.Substring(i + key.Length);        // "Prefabs/EnemyA.prefab"
                if (sub.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    sub = sub.Substring(0, sub.Length - ".prefab".Length);
                dto.resourcePath = sub;                                   // "Prefabs/EnemyA"
            }

            // (선택) Addressables 키 자동 채우기 - Addressables 사용시 주석 해제
            // #if UNITY_ADDRESSABLES
            // using UnityEditor.AddressableAssets;
            // using UnityEditor.AddressableAssets.Settings;
            // var settings = AddressableAssetSettingsDefaultObject.Settings;
            // if (settings != null)
            // {
            //     var guid = AssetDatabase.AssetPathToGUID(assetPath);
            //     var entry = settings.FindAssetEntry(guid);
            //     if (entry != null) dto.address = entry.address; // 그룹에서 지정한 address
            // }
            // #endif
        }
#endif
        dto.name = prefab.name;
        return dto;
    }

    // ★ Import 시에도 resourcePath 우선 사용(에디터 미리보기 일치)
    GameObject ResolvePrefab(PrefabRefDTO p)
    {
        if (p == null) return null;

        // 0) Resources 경로가 있으면 에디터에서도 우선 사용해 미리보기 일치
        if (!string.IsNullOrEmpty(p.resourcePath))
        {
            var go = Resources.Load<GameObject>(p.resourcePath);
            if (go != null) return go;
        }

        // 1) GUID
        if (!string.IsNullOrEmpty(p.guid))
        {
            string pathByGuid = AssetDatabase.GUIDToAssetPath(p.guid);
            if (!string.IsNullOrEmpty(pathByGuid))
            {
                var g = AssetDatabase.LoadAssetAtPath<GameObject>(pathByGuid);
                if (g != null) return g;
            }
        }

        // 2) Path
        if (!string.IsNullOrEmpty(p.path))
        {
            var g = AssetDatabase.LoadAssetAtPath<GameObject>(p.path);
            if (g != null) return g;
        }

        // 3) Name (프로젝트 내 검색)
        if (!string.IsNullOrEmpty(p.name))
        {
            var guids = AssetDatabase.FindAssets($"{p.name} t:prefab"); // t:GameObject 도 가능
            foreach (var g in guids)
            {
                string ap = AssetDatabase.GUIDToAssetPath(g);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(ap);
                if (obj != null && obj.name == p.name)
                    return obj;
            }
        }

        return null;
    }

    // ---------- 씬 미리보기 (스크럽/드래그/회전) ----------
    void OnSceneGUI(SceneView sv)
    {
        if (asset == null || asset.events == null) return;

        // 사용할 카메라 (MainCamera가 없으면 SceneView 카메라)
        var cam = Camera.main ?? sv.camera;
        if (cam == null) return;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        for (int i = 0; i < asset.events.Length; i++)
        {
            var e = asset.events[i];

            // 기본 정보
            int   total = Mathf.Max(1, e.count <= 0 ? 1 : e.count);
            float gap   = Mathf.Max(0f, e.interval);
            System.Func<int, float> tSpawn = k => e.time + (gap > 0f ? k * gap : 0f);

            // 기준점 (뷰포트→월드)
            Vector3 basePos = VpToWorld(e.vp, cam);

            // 위치 드래그 핸들
            if (allowDrag)
            {
                EditorGUI.BeginChangeCheck();
                float handleSize = HandleUtility.GetHandleSize(basePos) * 0.08f;
                var fmh_436_21_638968310901615292 = Quaternion.identity; var moved = Handles.FreeMoveHandle(
                    basePos,
                    Mathf.Max(0.05f, handleSize),
                    Vector3.zero,
                    Handles.SphereHandleCap
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(asset, "Move Spawn VP");
                    asset.events[i].vp = WorldToVp(moved, cam);
                    EditorUtility.SetDirty(asset);
                    Repaint();
                    basePos = moved; // 즉시 반영
                }
            }

            // 회전 디스크(2D: Z축)
            float angleDeg = e.rotationDeg;
            float discSize = HandleUtility.GetHandleSize(basePos) * 0.6f;
            Handles.color  = new Color(0.5f, 0.9f, 1f, 0.6f);

            EditorGUI.BeginChangeCheck();
            var q = Handles.Disc(
                Quaternion.AngleAxis(angleDeg, Vector3.forward),
                basePos,
                Vector3.forward,
                discSize,
                false,
                15f
            );
            float newZ = q.eulerAngles.z;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(asset, "Rotate Spawn Event");
                asset.events[i].rotationDeg = Mathf.Repeat(newZ, 360f);
                EditorUtility.SetDirty(asset);
                Repaint();
                angleDeg = asset.events[i].rotationDeg;
            }

            // 전방 화살표(회전 시각화) — 전방을 '위(Vector3.up)'로 가정
            var dir = Quaternion.AngleAxis(angleDeg, Vector3.forward) * Vector3.up;
            Handles.color = new Color(0.2f, 1f, 0.6f, 0.9f);
            Handles.DrawLine(basePos, basePos + dir * (discSize * 0.7f));
            Handles.ConeHandleCap(
                0,
                basePos + dir * (discSize * 0.7f),
                Quaternion.LookRotation(Vector3.forward, dir),
                discSize * 0.08f,
                EventType.Repaint
            );

            // 스폰 마커들(겹침 방지용으로 약간 퍼뜨려 그림)
            System.Func<int, Vector3> offsetPos = k =>
            {
                if (total == 1) return basePos;
                float ang = 137.50776405f * k * Mathf.Deg2Rad; // 황금각
                float r   = 0.12f * Mathf.Sqrt(k);
                return basePos + new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, 0f);
            };

            for (int k = 0; k < total; k++)
            {
                float ts = tSpawn(k);
                bool spawned = scrubTime >= ts;

                if (!spawned && !showFuture) continue;

                Color c = spawned ? new Color(0.2f, 0.9f, 0.2f, 0.95f)
                                  : new Color(1f, 0.7f, 0.2f, 0.5f);

                if (fadeByTime)
                {
                    float dt = Mathf.Abs(scrubTime - ts);
                    float a  = spawned ? Mathf.InverseLerp(1.2f, 0f, dt)
                                       : Mathf.InverseLerp(2.0f, 0f, dt);
                    c.a *= Mathf.Clamp01(0.25f + a);
                }

                using (new Handles.DrawingScope(c))
                {
                    var p = offsetPos(k);
                    Handles.DrawSolidDisc(p, Vector3.forward, spawnMarkerRadius);
                    Handles.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(c.a * 0.6f));
                    Handles.DrawWireDisc(p, Vector3.forward, spawnMarkerRadius + 0.04f);

                    // 각도 틱
                    var fwd = Quaternion.AngleAxis(angleDeg, Vector3.forward) * Vector3.up;
                    Handles.DrawLine(p, p + fwd * (spawnMarkerRadius * 0.9f));
                }
            }

            // 기준 라벨
            Handles.color = new Color(0f, 1f, 1f, 0.25f);
            Handles.DrawWireDisc(basePos, Vector3.forward, spawnMarkerRadius + 0.12f);

            Handles.color = new Color(1, 1, 1, 0.85f);
            Handles.Label(
                basePos + Vector3.up * (spawnMarkerRadius + 0.28f),
                $"t={e.time:0.0}s  x{Mathf.Max(1, e.count)}  rot={angleDeg:0}°"
            );
        }
    }

    // 좌표 변환 유틸
    static Vector3 VpToWorld(Vector2 vp, Camera cam)
    {
        var p = cam.ViewportToWorldPoint(new Vector3(vp.x, vp.y, Mathf.Abs(cam.transform.position.z)));
        p.z = 0f;
        return p;
    }
    static Vector2 WorldToVp(Vector3 world, Camera cam)
    {
        var p = cam.WorldToViewportPoint(world);
        return new Vector2(p.x, p.y);
    }
}
