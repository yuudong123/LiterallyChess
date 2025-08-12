using System.Collections;
using UnityEngine;

public partial class BoardView2D
{
    // 목적지 말 스케일 펀치(짧고 가벼운 손맛)
    public IEnumerator PunchPiece(Vector2Int sq, float amp = 0.12f, float dur = 0.12f) {
        if (sq.x < 0 || sq.x >= 8 || sq.y < 0 || sq.y >= 8) yield break;
        var go = pieceObjs[sq.x, sq.y];
        if (go == null) yield break;

        var t = go.transform;
        Vector3 baseScale = t.localScale;
        Vector3 peak = baseScale * (1f + amp);

        float half = dur * 0.5f;
        float t1 = 0f;
        while (t1 < half) {
            t1 += Time.deltaTime;
            float p = Mathf.Clamp01(t1 / half);
            t.localScale = Vector3.Lerp(baseScale, peak, p);
            yield return null;
        }
        float t2 = 0f;
        while (t2 < half) {
            t2 += Time.deltaTime;
            float p = Mathf.Clamp01(t2 / half);
            t.localScale = Vector3.Lerp(peak, baseScale, p);
            yield return null;
        }
        t.localScale = baseScale;
    }

    // 타일 플래시(캡처 등 강조, 빠르게 나타났다 사라짐)
    public IEnumerator FlashTile(Vector2Int sq, Color color, float dur = 0.18f) {
        if (sq.x < 0 || sq.x >= 8 || sq.y < 0 || sq.y >= 8) yield break;

        var go = new GameObject("FX_TileFlash");
        go.transform.SetParent(boardRoot, false);
        go.transform.localPosition = SquareToLocal(sq.x, sq.y, -0.065f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = tileLight ?? tileDark;
        sr.color = color;
        sr.sortingOrder = OrderUnderPiece(sq.y);

        float t = 0f;
        Color c0 = color;
        while (t < dur) {
            t += Time.deltaTime;
            float a = Mathf.Lerp(c0.a, 0f, t / dur);
            sr.color = new Color(c0.r, c0.g, c0.b, a);
            yield return null;
        }

        if (Application.isPlaying) Destroy(go);
        else DestroyImmediate(go);
    }
}