using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ArrowType
{
  Default,
  Thin,
  Double,
  Triple,
  Solid,
  Fat,
  ThreeD,
}

public static class DrawArrow
{
  // 기존 3D 메서드(ForGizmo, ForDebug ...)는 질문 코드 그대로 유지

  // =========================
  // 2D helpers
  // =========================
  static Vector3 ToV3(Vector2 v, float z = 0f) => new Vector3(v.x, v.y, z);

  // dir을 단위벡터로 가정하고, 시계 90도 회전한 수직 벡터를 반환
  static Vector3 Perp2D(Vector3 dir3)
  {
    // XY 평면에서의 수직: (x, y) -> (-y, x)
    return new Vector3(-dir3.y, dir3.x, 0f);
  }

  static void Compute2DHeads(Vector3 dir3, float headAngleDeg, float headLen,
                             out Vector3 headRight, out Vector3 headLeft)
  {
    var dirNorm = dir3.sqrMagnitude > 0f ? dir3.normalized : Vector3.right;
    headRight = Quaternion.AngleAxis(180f + headAngleDeg, Vector3.forward) * dirNorm * headLen;
    headLeft  = Quaternion.AngleAxis(180f - headAngleDeg, Vector3.forward) * dirNorm * headLen;
  }

  // =========================
  // 2D Gizmo (Scene 뷰에서 보임)
  // =========================
  public static void ForGizmo2D(Vector2 pos, Vector2 direction, Color? color = null,
                                ArrowType type = ArrowType.Default,
                                float arrowHeadLength = 0.2f, float arrowHeadAngle = 20f, float z = 0f)
  {
    var c = color ?? Color.white;
    Gizmos.color = c;

    Vector3 pos3 = ToV3(pos, z);
    Vector3 dir3 = ToV3(direction, 0f);

    if (dir3 == Vector3.zero)
      return;

    // 기본 몸통 또는 타입별 몸통
    float width = 0.01f;
    Vector3 dirNorm = dir3.normalized;
    Vector3 perp = Perp2D(dirNorm);

    // 머리 벡터
    Compute2DHeads(dir3, arrowHeadAngle, arrowHeadLength, out var headRight, out var headLeft);

    void DrawHeadAt(Vector3 tip)
    {
      Gizmos.DrawRay(tip, headRight);
      Gizmos.DrawRay(tip, headLeft);
    }

    switch (type)
    {
      case ArrowType.Default:
      case ArrowType.Thin:
        Gizmos.DrawRay(pos3, dir3);
        DrawHeadAt(pos3 + dir3);
        break;

      case ArrowType.Double:
        Gizmos.DrawRay(pos3 + perp * width, dir3 * (1f - width));
        Gizmos.DrawRay(pos3 - perp * width, dir3 * (1f - width));
        DrawHeadAt(pos3 + dir3 + (-dirNorm) * width); // 살짝 뒤로
        DrawHeadAt(pos3 + dir3 + ( dirNorm) * width); // 살짝 앞으로
        break;

      case ArrowType.Triple:
        Gizmos.DrawRay(pos3, dir3);
        Gizmos.DrawRay(pos3 + perp * width, dir3 * (1f - width));
        Gizmos.DrawRay(pos3 - perp * width, dir3 * (1f - width));
        DrawHeadAt(pos3 + dir3);
        break;

      case ArrowType.Solid:
        int increments = 20;
        for (int i = 0; i < increments; i++)
        {
          float t = i / (float)increments;
          float disp = Mathf.Lerp(-width, +width, t);
          var offset = perp * disp;
          Gizmos.DrawRay(pos3 + offset, dir3);
          DrawHeadAt(pos3 + dir3 + offset);
        }
        break;

      case ArrowType.Fat:
        // 간단한 굵은 선 효과: 여러 줄 겹쳐 그리기
        for (int i = -2; i <= 2; i++)
        {
          var offset = perp * (i * width);
          Gizmos.DrawRay(pos3 + offset, dir3);
        }
        DrawHeadAt(pos3 + dir3);
        break;

      case ArrowType.ThreeD:
        // 2D 컨텍스트에서는 Default와 동일 처리
        Gizmos.DrawRay(pos3, dir3);
        DrawHeadAt(pos3 + dir3);
        break;
    }
  }

  // =========================
  // 2D Debug (Game 뷰에서도, 재생 중 라인 표시)
  // =========================
  public static void ForDebug2D(Vector2 pos, Vector2 direction, float duration = 0.5f,
                                Color? color = null, ArrowType type = ArrowType.Default,
                                float arrowHeadLength = 0.2f, float arrowHeadAngle = 30f, float z = 0f)
  {
    var c = color ?? Color.white;
    duration = duration / Mathf.Max(Time.timeScale, 0.0001f);

    Vector3 pos3 = ToV3(pos, z);
    Vector3 dir3 = ToV3(direction, 0f);
    if (dir3 == Vector3.zero)
      return;

    float width = 0.01f;
    Vector3 dirNorm = dir3.normalized;
    Vector3 perp = Perp2D(dirNorm);

    Compute2DHeads(dir3, arrowHeadAngle, arrowHeadLength, out var headRight, out var headLeft);
    Vector3 tip = pos3 + dir3;

    void DL(Vector3 a, Vector3 b) => Debug.DrawLine(a, b, c, duration);

    // 화살촉
    DL(tip, tip + headRight);
    DL(tip, tip + headLeft);

    switch (type)
    {
      case ArrowType.Default:
      case ArrowType.Thin:
        DL(pos3, pos3 + dir3);
        break;

      case ArrowType.Double:
        DL(pos3 + perp * width, pos3 + perp * width + dir3 * (1f - width));
        DL(pos3 - perp * width, pos3 - perp * width + dir3 * (1f - width));
        // 두 번째 화살촉(살짝 오프셋)
        Vector3 tip2 = tip - dirNorm * width;
        DL(tip2, tip2 + headRight);
        DL(tip2, tip2 + headLeft);
        break;

      case ArrowType.Triple:
        DL(pos3, pos3 + dir3);
        DL(pos3 + perp * width, pos3 + perp * width + dir3 * (1f - width));
        DL(pos3 - perp * width, pos3 - perp * width + dir3 * (1f - width));
        break;

      case ArrowType.Solid:
        int increments = 20;
        for (int i = 0; i < increments; i++)
        {
          float t = i / (float)increments;
          float disp = Mathf.Lerp(-width, +width, t);
          var offset = perp * disp;
          DL(pos3 + offset, pos3 + offset + dir3);
          Vector3 tipN = tip + offset;
          DL(tipN, tipN + headRight);
          DL(tipN, tipN + headLeft);
        }
        break;

      case ArrowType.Fat:
        for (int i = -2; i <= 2; i++)
        {
          var offset = perp * (i * width);
          DL(pos3 + offset, pos3 + offset + dir3);
        }
        break;

      case ArrowType.ThreeD:
        DL(pos3, pos3 + dir3);
        break;
    }
  }

  // 중심과 반지름으로 2D 원 둘레 방향 화살표를 배치하는 유틸
  public static void ForDebug2DCircleArrow(Vector2 center, float radius, float angleDeg, float length,
                                           float duration = 0.5f, Color? color = null,
                                           float arrowHeadLength = 0.2f, float arrowHeadAngle = 30f)
  {
    var dir = new Vector2(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad));
    var start = center + dir * radius;
    var tangent = new Vector2(-dir.y, dir.x).normalized * length;
    ForDebug2D(start, tangent, duration, color, ArrowType.Default, arrowHeadLength, arrowHeadAngle, 0f);
  }
}
