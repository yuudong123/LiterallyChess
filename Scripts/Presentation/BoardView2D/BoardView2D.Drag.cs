using UnityEngine;

public partial class BoardView2D
{
    void SetPieceAlpha(Vector2Int sq, float a)
    {
        if (sq.x < 0 || sq.x >= N || sq.y < 0 || sq.y >= N) return;
        var go = pieceObjs[sq.x, sq.y];
        if (go == null) return;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color; c.a = a; sr.color = c;
        }
    }

    public void BeginDrag(Sprite pieceSprite, Vector2Int from)
    {
        EndDrag(true);
        dragFromSq = from;
        SetPieceAlpha(from, 0.4f);

        dragGO = new GameObject("DragPiece");
        dragGO.transform.SetParent(boardRoot, false);
        var sr = dragGO.AddComponent<SpriteRenderer>();
        sr.sprite = pieceSprite;
        sr.sortingOrder = 1000;

        float yOff = (pieceBottomY01 - 0.5f) * 1f;
        var pos = SquareToLocal(from.x, from.y, -0.02f);
        pos.y += yOff;
        dragGO.transform.localPosition = pos;
    }

    public void UpdateDrag(Vector3 worldPos)
    {
        if (dragGO == null) return;
        dragGO.transform.position = new Vector3(worldPos.x, worldPos.y, -0.02f);
    }

    public void EndDrag(bool restoreOriginal = true)
    {
        if (dragGO != null)
        {
            if (Application.isPlaying) Destroy(dragGO);
            else DestroyImmediate(dragGO);
            dragGO = null;
        }
        if (restoreOriginal && dragFromSq.HasValue)
        {
            SetPieceAlpha(dragFromSq.Value, 1f);
        }
        dragFromSq = null;
    }
}