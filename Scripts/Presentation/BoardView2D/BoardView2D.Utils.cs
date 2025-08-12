using UnityEngine;

public partial class BoardView2D
{
    // 화면→논리 칸(시점 전환/보드 원점 보정)
    public Vector2Int ScreenToSquare(Camera cam, Vector3 screenPos)
    {
        var world = cam.ScreenToWorldPoint(screenPos);
        var origin = boardRoot != null ? boardRoot.position : Vector3.zero;

        float localX = world.x - origin.x;
        float localY = world.y - origin.y;

        int fx = Mathf.FloorToInt(localX / 1f);
        int ry = Mathf.FloorToInt(localY / 1f);

        if (bottomIsWhite) {
            return new Vector2Int(fx, ry);
        } else {
            return new Vector2Int(7 - fx, 7 - ry);
        }
    }

    // 논리→로컬(시점 전환 반영)
    Vector3 SquareToLocal(int f, int r, float z = 0f)
    {
        int df = bottomIsWhite ? f : (7 - f);
        int dr = bottomIsWhite ? r : (7 - r);
        return new Vector3(df * 1f + 0.5f, dr * 1f + 0.5f, z);
    }

    // 논리→월드 공개
    public Vector3 SquareToWorld(int f, int r) {
        var origin = boardRoot != null ? boardRoot.position : Vector3.zero;
        return origin + SquareToLocal(f, r, 0f);
    }
}