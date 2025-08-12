using UnityEngine;
using RetroChess.Core;

public partial class BoardView2D
{
    void RenderTiles()
    {
        for (int f = 0; f < N; f++)
        for (int r = 0; r < N; r++)
        {
            var go = new GameObject($"Tile_{f}_{r}");
            go.transform.SetParent(boardRoot, false);
            go.transform.localPosition = SquareToLocal(f, r, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ((f + r) % 2 == 0) ? tileDark : tileLight; // a1(0,0)=어두움
            sr.sortingOrder = 0;
        }
    }

    void RenderPieces(Board b)
    {
        float yOff = (pieceBottomY01 - 0.5f) * 1f;

        for (int f = 0; f < N; f++)
        for (int r = 0; r < N; r++)
        {
            var p = b.squares[f, r];
            if (p.IsEmpty) continue;

            var sp = GetPieceSprite(p.Side, p.Type);
            if (sp == null) continue;

            int order = OrderPiece(r);

            // 그림자
            if (shadowSprite != null)
            {
                var sh = new GameObject($"Shadow_{f}_{r}");
                sh.transform.SetParent(boardRoot, false);
                var posSh = SquareToLocal(f, r, -0.09f);
                posSh.y += yOff;
                sh.transform.localPosition = posSh;

                var srSh = sh.AddComponent<SpriteRenderer>();
                srSh.sprite = shadowSprite;
                srSh.color = new Color(0f, 0f, 0f, 0.85f);
                srSh.sortingOrder = OrderUnderPiece(r);
                sh.transform.localScale = new Vector3(0.95f, 0.65f, 1f);
            }

            // 기물
            var go = new GameObject($"Piece_{f}_{r}");
            go.transform.SetParent(boardRoot, false);
            var pos = SquareToLocal(f, r, -0.1f);
            pos.y += yOff;
            go.transform.localPosition = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = order;

            pieceObjs[f, r] = go;
        }
    }

    public Sprite GetPieceSprite(Side side, PieceType type)
    {
        if (side == Side.White)
        {
            return type switch {
                PieceType.Pawn => wPawn,
                PieceType.Knight => wKnight,
                PieceType.Bishop => wBishop,
                PieceType.Rook => wRook,
                PieceType.Queen => wQueen,
                PieceType.King => wKing,
                _ => null
            };
        }
        else
        {
            return type switch {
                PieceType.Pawn => bPawn,
                PieceType.Knight => bKnight,
                PieceType.Bishop => bBishop,
                PieceType.Rook => bRook,
                PieceType.Queen => bQueen,
                PieceType.King => bKing,
                _ => null
            };
        }
    }

    // === 타임라인 미리보기 OverLay ===
    void EnsurePreviewOverlay()
    {
        if (!usePreviewOverlay || previewOverlayGO != null) return;
        if (boardRoot == null) return;

        var go = new GameObject("PreviewOverlay");
        go.transform.SetParent(boardRoot, false);
        go.transform.localPosition = new Vector3(4f, 4f, -0.045f); // 타일(0) 위, 기물(200~) 아래
        var sr = go.AddComponent<SpriteRenderer>();

        var baseSprite = tileLight ?? tileDark;
        if (baseSprite != null)
        {
            sr.sprite = baseSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(8f, 8f);
        }
        else
        {
            var tex = Texture2D.whiteTexture;
            var rect = new Rect(0, 0, tex.width, tex.height);
            sr.sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
            sr.drawMode = SpriteDrawMode.Sliced;
        }

        sr.color = previewOverlayColor;
        sr.sortingOrder = 1;

        previewOverlayGO = go;
        previewOverlayGO.SetActive(false);
    }

    public void SetTimelinePreview(bool on)
    {
        if (!usePreviewOverlay) return;
        EnsurePreviewOverlay();
        if (previewOverlayGO != null)
        {
            previewOverlayGO.SetActive(on);
            var sr = previewOverlayGO.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = previewOverlayColor; // Inspector에서 색 변경 시 즉시 반영
        }
    }
}