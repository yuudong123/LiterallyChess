using System;
using System.Collections.Generic;
using UnityEngine;
using RetroChess.Core;

public partial class BoardView2D
{
    [Header("Promotion Icons (Common)")]
    public Sprite promoIconQueen;
    public Sprite promoIconRook;
    public Sprite promoIconBishop;
    public Sprite promoIconKnight;

    [Header("Promotion Icons (White Override)")]
    public Sprite promoIconWQueen;
    public Sprite promoIconWRook;
    public Sprite promoIconWBishop;
    public Sprite promoIconWKnight;

    [Header("Promotion Icons (Black Override)")]
    public Sprite promoIconBQueen;
    public Sprite promoIconBRook;
    public Sprite promoIconBBishop;
    public Sprite promoIconBKnight;

    [Header("Promotion Layout")]
    public float promoStepY = 1.05f;
    public float promoIconScale = 1.0f;
    public int promoSortingOrder = 500;
    public float promoOffsetX = 0.0f;
    public float promoOffsetY = 0.0f;

    GameObject promoRoot;
    readonly List<(PieceType type, GameObject go)> promoItems = new();

    // 보드 위 프로모션 팝업(세로 배치)
    public void ShowPromotionPopup(Side side, Vector2Int tile, Action<PieceType> onChoose)
    {
        ClosePromotionPopup();

        promoRoot = new GameObject("PromotionPopup");
        promoRoot.transform.SetParent(boardRoot, false);

        // 1) 시점 전환(아래 진영)까지 반영된 '로컬 좌표'를 기준점으로 사용
        var basePos = SquareToLocal(tile.x, tile.y, -0.2f) + new Vector3(promoOffsetX, promoOffsetY, 0f);
        promoRoot.transform.localPosition = basePos;

        // 2) 세로 배치 방향은 '현재 화면 시점에서의 행'으로 결정
        //    아래쪽(<=3)이면 위로(+1), 위쪽(>3)이면 아래(-1) 배치 → 항상 보드 안쪽으로 쌓이게
        int localRow = bottomIsWhite ? tile.y : (7 - tile.y);
        float dirY = (localRow <= 3) ? 1f : -1f;

        var types = new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };

        for (int i = 0; i < types.Length; i++)
        {
            var t = types[i];
            var go = new GameObject($"Promo_{t}");
            go.transform.SetParent(promoRoot.transform, false);

            float y = dirY * (i * promoStepY);
            go.transform.localPosition = new Vector3(0f, y, 0f);
            go.transform.localScale = Vector3.one * promoIconScale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetPromotionIcon(side, t);
            sr.sortingOrder = promoSortingOrder;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.95f);

            promoItems.Add((t, go));
        }

        var clicker = promoRoot.AddComponent<PromotionClickHelper>();
        clicker.Init(this, promoItems, onChoose);
    }

    public void ClosePromotionPopup()
    {
        if (promoRoot != null)
        {
            if (Application.isPlaying) Destroy(promoRoot);
            else DestroyImmediate(promoRoot);
            promoRoot = null;
        }
        promoItems.Clear();
    }

    Sprite GetPromotionIcon(Side side, PieceType t)
    {
        if (side == Side.White) {
            switch (t) {
                case PieceType.Queen:  return promoIconWQueen  ?? promoIconQueen;
                case PieceType.Rook:   return promoIconWRook   ?? promoIconRook;
                case PieceType.Bishop: return promoIconWBishop ?? promoIconBishop;
                case PieceType.Knight: return promoIconWKnight ?? promoIconKnight;
                default: return null;
            }
        } else {
            switch (t) {
                case PieceType.Queen:  return promoIconBQueen  ?? promoIconQueen;
                case PieceType.Rook:   return promoIconBRook   ?? promoIconRook;
                case PieceType.Bishop: return promoIconBBishop ?? promoIconBishop;
                case PieceType.Knight: return promoIconBKnight ?? promoIconKnight;
                default: return null;
            }
        }
    }

    class PromotionClickHelper : MonoBehaviour
    {
        BoardView2D view;
        List<(PieceType type, GameObject go)> items;
        Action<PieceType> onChoose;
        Camera cam;

        public void Init(BoardView2D view, List<(PieceType, GameObject)> items, Action<PieceType> onChoose)
        {
            this.view = view;
            this.items = items;
            this.onChoose = onChoose;
            cam = Camera.main;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var wp = cam.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.OverlapPoint(new Vector2(wp.x, wp.y));
                if (hit != null)
                {
                    foreach (var it in items)
                    {
                        if (hit.gameObject == it.go)
                        {
                            onChoose?.Invoke(it.type);
                            view.ClosePromotionPopup();
                            Destroy(this);
                            return;
                        }
                    }
                }
            }
        }
    }
}