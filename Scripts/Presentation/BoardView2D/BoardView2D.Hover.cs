using UnityEngine;

public partial class BoardView2D
{
    public void ShowHoverIndicator(Vector2Int sq)
    {
        // 뒤쪽 상단 — 기물 뒤
        if (indicatorTopBack != null)
        {
            if (hoverTop == null)
            {
                hoverTop = new GameObject("IndicatorTopBack");
                hoverTop.transform.SetParent(boardRoot, false);
                var srTopInit = hoverTop.AddComponent<SpriteRenderer>();
                srTopInit.sprite = indicatorTopBack;
            }
            hoverTop.SetActive(true);
            var srTop = hoverTop.GetComponent<SpriteRenderer>();
            srTop.sortingOrder = OrderUnderPiece(sq.y);
            var posTop = SquareToLocal(sq.x, sq.y, -0.04f);
            posTop.y += .59f; // 칸 윗변
            hoverTop.transform.localPosition = posTop;
        }

        // 앞쪽 사각 — 기물 위
        if (indicatorFront != null)
        {
            if (hoverFront == null)
            {
                hoverFront = new GameObject("IndicatorFront");
                hoverFront.transform.SetParent(boardRoot, false);
                var srFrontInit = hoverFront.AddComponent<SpriteRenderer>();
                srFrontInit.sprite = indicatorFront;
            }
            hoverFront.SetActive(true);
            var srFront = hoverFront.GetComponent<SpriteRenderer>();
            srFront.sortingOrder = OrderOverPiece(sq.y);
            var posFront = SquareToLocal(sq.x, sq.y, -0.03f);
            posFront.y += 0.125f; // 중앙 조금 위
            hoverFront.transform.localPosition = posFront;
        }
    }

    public void HideHoverIndicator()
    {
        if (hoverTop != null) hoverTop.SetActive(false);
        if (hoverFront != null) hoverFront.SetActive(false);
    }
}