using UnityEngine;
using TMPro;

public partial class BoardView2D
{
    void RenderLabels()
    {
        ClearLabels();
        if (!showLabels || boardRoot == null) return;

        labelsRoot = new GameObject("LabelsRoot");
        labelsRoot.transform.SetParent(boardRoot, false);

        // 파일(a~h) — 아래
        for (int f = 0; f < 8; f++)
        {
            int fileIndex = bottomIsWhite ? f : (7 - f);
            char fileChar = (char)('a' + fileIndex);

            var go = new GameObject($"Label_File_{f}");
            go.transform.SetParent(labelsRoot.transform, false);

            float x = f * 1f + 0.5f;
            float y = -fileMargin;
            if (labelPixelSnap && ppu > 0f) { x = Mathf.Round(x * ppu) / ppu; y = Mathf.Round(y * ppu) / ppu; }
            go.transform.localPosition = new Vector3(x, y, -0.02f);

            var tmp = go.AddComponent<TextMeshPro>();
            // 폰트는 지정하지 않음(전역 TMP 기본 폰트 사용)
            tmp.text = fileChar.ToString();
            tmp.fontSize = fileFontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = labelColor;
            tmp.rectTransform.sizeDelta = new Vector2(1f, 0.8f);

            var mr = go.GetComponent<MeshRenderer>();
            mr.sortingOrder = labelSortingOrder;

            labelObjs.Add(go);
        }

        // 랭크(1~8) — 왼쪽
        for (int r = 0; r < 8; r++)
        {
            int rankNum = bottomIsWhite ? (r + 1) : (8 - r);

            var go = new GameObject($"Label_Rank_{r}");
            go.transform.SetParent(labelsRoot.transform, false);

            float x = -rankMargin;
            float y = r * 1f + 0.5f;
            if (labelPixelSnap && ppu > 0f) { x = Mathf.Round(x * ppu) / ppu; y = Mathf.Round(y * ppu) / ppu; }
            go.transform.localPosition = new Vector3(x, y, -0.02f);

            var tmp = go.AddComponent<TextMeshPro>();
            // 폰트 지정 없음(전역 적용)
            tmp.text = rankNum.ToString();
            tmp.fontSize = rankFontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = labelColor;
            tmp.rectTransform.sizeDelta = new Vector2(0.8f, 1f);

            var mr = go.GetComponent<MeshRenderer>();
            mr.sortingOrder = labelSortingOrder;

            labelObjs.Add(go);
        }
    }
}