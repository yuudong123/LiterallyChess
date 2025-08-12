using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// SAN 기보를 "1. e4    e5"처럼 한 줄(백/흑)로 묶어 보여주는 리스트 어댑터
// - 프리팹 구조(추천):
//   Root(TimelinePairItem, Horizontal Layout Group)
//     - MoveNo (TextMeshProUGUI)  예: "1."
//     - WhiteBtn (Button)
//         - WhiteText (TextMeshProUGUI)
//     - BlackBtn (Button)
//         - BlackText (TextMeshProUGUI)
// - 각 버튼은 해당 수로 미리보기(ChessGame.OnClickTimelineItem)로 이동
public class TimelinePairListUI : MonoBehaviour
{
    [Header("참조")]
    public ChessGame chess;             // GameRoot의 ChessGame
    public RectTransform content;       // Scroll View/Viewport/Content
    public ScrollRect scrollRect;       // Scroll View
    public GameObject pairItemPrefab;   // TimelinePairItem 프리팹

    [Header("하이라이트 색상")]
    public Color normalText = Color.white;
    public Color selectedText = new Color(1f, 0.95f, 0.6f, 1f);
    public Color normalBg = new Color(1, 1, 1, 0.05f);
    public Color selectedBg = new Color(1, 1, 1, 0.15f);

    readonly List<GameObject> pool = new();
    int lastPairCount = 0;
    int previewIndex = -1; bool previewMode = false;

    void OnEnable()
    {
        if (chess != null) chess.TimelineChanged += OnTimelineChanged;
        // 초기 그리기
        OnTimelineChanged(chess != null ? chess.TimelineLabels : null, -1, false);
    }

    void OnDisable()
    {
        if (chess != null) chess.TimelineChanged -= OnTimelineChanged;
    }

    void OnTimelineChanged(IReadOnlyList<string> labels, int pIndex, bool pMode)
    {
        previewIndex = pIndex; previewMode = pMode;
        Rebuild(labels);
        if (!previewMode && scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
        else AutoScrollToCurrent();

    }

    void Rebuild(IReadOnlyList<string> labels)
    {
        if (labels == null) labels = System.Array.Empty<string>();

        int pairs = (labels.Count + 1) / 2;
        EnsurePool(pairs);

        int poolIdx = 0;
        for (int i = 0; i < labels.Count; i += 2)
        {
            var go = pool[poolIdx++];
            go.SetActive(true);

            // 참조 가져오기
            var moveNo = FindInChildren<TextMeshProUGUI>(go.transform, "MoveNo");
            var wBtn = FindInChildren<Button>(go.transform, "WhiteBtn");
            var wText = FindInChildren<TextMeshProUGUI>(go.transform, "WhiteBtn/WhiteText");
            var bBtn = FindInChildren<Button>(go.transform, "BlackBtn");
            var bText = FindInChildren<TextMeshProUGUI>(go.transform, "BlackBtn/BlackText");
            var wBg = wBtn ? wBtn.GetComponent<Image>() : null;
            var bBg = bBtn ? bBtn.GetComponent<Image>() : null;

            int plyWhite = i;       // 0-based ply index
            int plyBlack = i + 1;   // 0-based, 없을 수 있음
            int moveNoVal = (plyWhite + 2) / 2;

            if (moveNo) moveNo.text = moveNoVal.ToString() + ".";

            // 백
            if (wText) wText.text = labels[plyWhite];
            if (wBtn)
            {
                int idx = plyWhite;
                wBtn.onClick.RemoveAllListeners();
                wBtn.onClick.AddListener(() => chess.OnClickTimelineItem(idx));

                bool selected = (previewMode && previewIndex == idx);
                if (wText) wText.color = selected ? selectedText : normalText;
                if (wBg) wBg.color = selected ? selectedBg : normalBg;
                wBtn.interactable = true;
            }

            // 흑
            if (plyBlack < labels.Count)
            {
                if (bText) bText.text = labels[plyBlack];
                if (bBtn)
                {
                    int idx = plyBlack;
                    bBtn.onClick.RemoveAllListeners();
                    bBtn.onClick.AddListener(() => chess.OnClickTimelineItem(idx));

                    bool selected = (previewMode && previewIndex == idx);
                    if (bText) bText.color = selected ? selectedText : normalText;
                    if (bBg) bBg.color = selected ? selectedBg : normalBg;
                    bBtn.interactable = true;
                }
            }
            else
            {
                // 흑 수가 아직 없으면 빈 상태로 표시/비활성
                if (bText) bText.text = "";
                if (bBtn)
                {
                    bBtn.onClick.RemoveAllListeners();
                    bBtn.interactable = false;
                    var img = bBtn.GetComponent<Image>();
                    if (img) img.color = new Color(0, 0, 0, 0);
                }
            }
        }

        // 남는 풀 숨기기
        for (int k = poolIdx; k < pool.Count; k++) pool[k].SetActive(false);
        lastPairCount = pairs;
    }

    void EnsurePool(int requiredPairs)
    {
        while (pool.Count < requiredPairs)
        {
            var go = Instantiate(pairItemPrefab, content);
            pool.Add(go);
        }
    }

    void AutoScrollToCurrent()
    {
        if (!previewMode || previewIndex < 0) return;
        int pairLine = previewIndex / 2;
        if (pairLine < 0 || pairLine >= pool.Count) return;
        if (scrollRect == null || scrollRect.viewport == null) return;

        var item = pool[pairLine].GetComponent<RectTransform>();
        if (item == null) return;

        float contentH = content.rect.height;
        float vpH = scrollRect.viewport.rect.height;
        float center = -item.anchoredPosition.y + item.rect.height * 0.5f;
        float t = Mathf.Clamp01((center - vpH * 0.5f) / Mathf.Max(1f, contentH - vpH));
        scrollRect.verticalNormalizedPosition = 1f - t;
    }

    // 안전한 경로 탐색(없으면 null 반환)
    T FindInChildren<T>(Transform root, string path) where T : Component
    {
        var t = root.Find(path);
        return t ? t.GetComponent<T>() : null;
    }
}