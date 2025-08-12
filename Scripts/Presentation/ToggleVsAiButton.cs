using UnityEngine;
using UnityEngine.UI;

public class ToggleVsAiButton : MonoBehaviour {
    public enum ToggleMode { VsAI, GiveUp }

    public ChessGame chess;
    public Button button;
    public Image buttonImage;

    public Sprite vsNormal;
    public Sprite vsPressed;
    public Sprite giveupNormal;
    public Sprite giveupPressed;

    public ToggleMode startMode = ToggleMode.VsAI;

    ToggleMode mode;

    void Awake() {
        if (button == null) button = GetComponent<Button>();
        if (buttonImage == null && button != null) buttonImage = button.GetComponent<Image>();
        if (button != null) button.transition = Selectable.Transition.SpriteSwap;
    }

    void OnEnable() {
        if (chess != null) {
            chess.OnGamePhaseChanged += OnGamePhaseChanged;
            RefreshFromChess();
        } else {
            SetMode(startMode);
        }
    }

    void OnDisable() {
        if (chess != null) chess.OnGamePhaseChanged -= OnGamePhaseChanged;
    }

    void OnGamePhaseChanged(bool isPlaying) {
        RefreshFromChess();
    }

    public void RefreshFromChess() {
        if (chess == null) return;
        SetMode(chess.isVsAI ? ToggleMode.GiveUp : ToggleMode.VsAI);
    }

    void SetMode(ToggleMode m) {
        mode = m;

        if (buttonImage != null) {
            buttonImage.sprite = (mode == ToggleMode.VsAI) ? vsNormal : giveupNormal;
        }
        if (button != null) {
            var st = button.spriteState;
            st.pressedSprite = (mode == ToggleMode.VsAI) ? vsPressed : giveupPressed;
            button.spriteState = st;

            button.onClick.RemoveAllListeners();
            if (mode == ToggleMode.VsAI) {
                button.onClick.AddListener(() => {
                    if (chess != null) {
                        chess.StartVsAI();      // 현재 선택된 사람 색으로 시작
                        SetMode(ToggleMode.GiveUp);
                    }
                });
            } else {
                button.onClick.AddListener(() => {
                    if (chess != null) {
                        chess.ConcedeByHuman(); // 즉시 상대 승리 판정
                    }
                });
            }
        }
    }
}