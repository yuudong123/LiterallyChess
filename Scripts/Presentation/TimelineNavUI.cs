using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 4버튼 내비게이션(First/Prev/Next/Last)
// - Inspector에서 ChessGame, 4개 버튼, 각 버튼의 Normal/Pressed 스프라이트만 연결하면 동작합니다.
// - ChessGame.TimelineChanged를 구독해 버튼 활성/비활성(처음/마지막 도달 시)도 자동 반영합니다.
public class TimelineNavUI : MonoBehaviour
{
    [Header("참조")]
    public ChessGame chess;      // GameRoot의 ChessGame

    public Button btnFirst;
    public Button btnPrev;
    public Button btnNext;
    public Button btnLast;

    [Header("스프라이트(각 버튼의 Normal/Pressed)")]
    public Sprite firstNormal; public Sprite firstPressed;
    public Sprite prevNormal; public Sprite prevPressed;
    public Sprite nextNormal; public Sprite nextPressed;
    public Sprite lastNormal; public Sprite lastPressed;

    void Awake()
    {
        // 버튼 이미지/스테이트 적용 + onClick 바인딩
        SetupButton(btnFirst, firstNormal, firstPressed, chess != null ? chess.OnClickViewFirst : (UnityEngine.Events.UnityAction)null);
        SetupButton(btnPrev, prevNormal, prevPressed, chess != null ? chess.OnClickViewPrev : null);
        SetupButton(btnNext, nextNormal, nextPressed, chess != null ? chess.OnClickViewNext : null);
        SetupButton(btnLast, lastNormal, lastPressed, chess != null ? chess.OnClickViewLast : null);
    }

    void OnEnable()
    {
        if (chess != null) chess.TimelineChanged += OnTimelineChanged;
        // 초기 상태 반영
        OnTimelineChanged(chess != null ? chess.TimelineLabels : null, -1, false);
    }

    void OnDisable()
    {
        if (chess != null) chess.TimelineChanged -= OnTimelineChanged;
    }

    void SetupButton(Button btn, Sprite normal, Sprite pressed, UnityEngine.Events.UnityAction onClick)
    {
        if (btn == null) return;

        var img = btn.GetComponent<Image>();
        if (img != null && normal != null) img.sprite = normal;

        btn.transition = Selectable.Transition.SpriteSwap;
        var st = btn.spriteState;
        st.pressedSprite = pressed;      // Mouse Down 시 사용할 스프라이트
        btn.spriteState = st;

        btn.onClick.RemoveAllListeners();
        if (onClick != null) btn.onClick.AddListener(onClick);
    }

    void OnTimelineChanged(IReadOnlyList<string> labels, int previewIndex, bool previewMode)
    {
        int n = labels == null ? 0 : labels.Count;
        bool any = n > 0;
        if (!any) { SetInteractable(false, false, false, false); return; }

        if (previewMode)
        {
            bool atStart = previewIndex < 0;           // Start(-1)
            bool atFirst = previewIndex == 0;
            bool atLast = previewIndex >= n - 1;

            btnFirst.interactable = !atStart;          // Start면 비활성
            btnPrev.interactable = !(atStart);        // Start면 비활성
            btnNext.interactable = !(atLast);         // 마지막이면 비활성
            btnLast.interactable = true;              // 항상 라이브 복귀 가능
        }
        else
        {
            // 라이브: 모두 활성
            SetInteractable(true, true, true, true);
        }
    }

    void SetInteractable(bool f, bool p, bool n, bool l)
    {
        if (btnFirst != null) btnFirst.interactable = f;
        if (btnPrev != null) btnPrev.interactable = p;
        if (btnNext != null) btnNext.interactable = n;
        if (btnLast != null) btnLast.interactable = l;
    }
}