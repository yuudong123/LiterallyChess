using System.Collections.Generic;
using UnityEngine;
using TMPro;

public partial class BoardView2D
{
    GameObject CreateRectHL(Color c, int sortingOrder)
    {
        var go = new GameObject("HL_Rect");
        go.transform.SetParent(boardRoot, false);
        go.transform.localScale = new Vector3(0.98f, 0.98f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = tileLight ?? tileDark;
        sr.color = c;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    GameObject CreateDot(Sprite dotSprite, int sortingOrder)
    {
        var go = new GameObject("HL_Dot");
        go.transform.SetParent(boardRoot, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = dotSprite;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    GameObject GetOrCreateSelection()
    {
        if (selectionHL != null) return selectionHL;
        selectionHL = CreateRectHL(new Color(1f, 0.9f, 0.2f, 0.35f), 1);
        return selectionHL;
    }

    public void ShowSelection(Vector2Int sq)
    {
        var s = GetOrCreateSelection();
        s.SetActive(true);
        s.transform.localPosition = SquareToLocal(sq.x, sq.y, -0.05f);
        var sr = s.GetComponent<SpriteRenderer>();
        sr.sortingOrder = OrderUnderPiece(sq.y);
    }

    public void HighlightMoves(List<Vector2Int> moves, List<Vector2Int> captures)
    {
        ClearHighlightsOnly();

        var dot = moveDotWhite ?? moveDotBlack;
        foreach (var m in moves)
        {
            var go = CreateDot(dot, OrderUnderPiece(m.y));
            go.transform.localPosition = SquareToLocal(m.x, m.y, -0.07f);
            moveHL.Add(go);
        }
        foreach (var c in captures)
        {
            var go = CreateRectHL(new Color(1f, 0.25f, 0.25f, 0.35f), OrderUnderPiece(c.y));
            go.transform.localPosition = SquareToLocal(c.x, c.y, -0.06f);
            capHL.Add(go);
        }
    }

    public void ClearHighlights()
    {
        if (selectionHL != null) selectionHL.SetActive(false);
        ClearHighlightsOnly();
    }

    void ClearHighlightsOnly()
    {
        foreach (var go in moveHL) if (go != null) DestroySafe(go);
        foreach (var go in capHL) if (go != null) DestroySafe(go);
        moveHL.Clear(); capHL.Clear();
    }

    public void ShowCheck(Vector2Int kingSq)
    {
        if (checkHL == null) checkHL = CreateRectHL(new Color(1f, 0f, 0f, 0.45f), OrderUnderPiece(kingSq.y));
        checkHL.SetActive(true);
        var sr = checkHL.GetComponent<SpriteRenderer>();
        sr.sortingOrder = OrderUnderPiece(kingSq.y);
        checkHL.transform.localPosition = SquareToLocal(kingSq.x, kingSq.y, -0.05f);
    }

    public void HideCheck()
    {
        if (checkHL != null) checkHL.SetActive(false);
    }

    public void ShowResult(string message)
    {
        if (resultGO == null)
        {
            resultGO = new GameObject("ResultText");
            resultGO.transform.SetParent(boardRoot, false);
            resultGO.transform.localPosition = new Vector3(4f, 4.2f, -0.2f);
            var tmp = resultGO.AddComponent<TextMeshPro>();
            // 폰트 지정 없음(전역 기본 폰트)
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 12f;
            tmp.color = new Color(1f, 0.95f, 0.4f, 1f);
            tmp.text = message;
            var mr = resultGO.GetComponent<MeshRenderer>();
            mr.sortingOrder = 300;
            resultTMP = tmp;
        }
        else
        {
            resultTMP.text = message;
            resultGO.SetActive(true);
        }
    }

    public void HideResult()
    {
        if (resultGO != null) resultGO.SetActive(false);
    }
}