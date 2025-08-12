// Assets/Scripts/Game/Chess/ChessGame.Core.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RetroChess.Core;

public partial class ChessGame : MonoBehaviour {
    public enum GamePhase { Free, Playing, Ended }
    public GamePhase gamePhase { get; private set; } = GamePhase.Free;

    // 방법 A: 진행 여부(bool) 전달 이벤트
    public event Action<bool> OnGamePhaseChanged;

    bool gameOverSfxPlayed = false;

    public BoardView2D view;

    Board board;

    public Side preferredHumanSide = Side.White;

    Vector2Int? selected;
    List<Vector2Int> cachedMoves = new();
    List<Vector2Int> cachedCaps  = new();

    bool waitingPromotion = false;
    Vector2Int pendingPromotionSq;
    Side pendingPromotionSide;

    readonly Dictionary<ulong,int> repetition = new();
    Vector2Int? lastHover = null;
    bool isDragging = false, dragCandidate = false;
    Vector2Int dragFrom;
    Vector3 pressScreenPos;
    const float dragStartPixels = 8f;

    readonly List<Snapshot> timeline = new();
    readonly List<string> timelineLabels = new();
    public IReadOnlyList<string> TimelineLabels => timelineLabels;

    bool previewMode = false;
    int previewIndex = -1;
    Board previewBoard = new Board();
    Snapshot startSnapshot;
    Snapshot lastPreSnap;

    Vector2Int lastMoveFrom, lastMoveTo;
    Piece lastMovingPiece;
    bool lastIsEnPassant, lastDidCapture, lastDidCastle;

    public Action<IReadOnlyList<string>, int, bool> TimelineChanged;
    void NotifyTimelineChanged() => TimelineChanged?.Invoke(TimelineLabels, previewIndex, previewMode);

    string lastResultMessage = null;

    void Awake() {
        if (view == null) view = FindObjectOfType<BoardView2D>();
    }

    void Start() {
        board = Board.CreateInitial();
        view.RenderAll(board);
        startSnapshot = Snapshot.Capture(board);

        gamePhase = GamePhase.Free;
        gameOverSfxPlayed = false;
        lastResultMessage = null;

        OnGamePhaseChanged?.Invoke(false); // 진행 아님

        ResetRepetitionHistory();
        UpdateCheckAndEnd();
        NotifyTimelineChanged();
    }

    public void OnClickNewGame() => NewGame();

    void NewGame() {
        waitingPromotion = false;

        board = Board.CreateInitial();
        view.RenderAll(board);
        startSnapshot = Snapshot.Capture(board);
        ClearSelection();

        timeline.Clear();
        timelineLabels.Clear();
        ExitPreview();

        gamePhase = GamePhase.Free;
        gameOverSfxPlayed = false;
        lastResultMessage = null;

        OnGamePhaseChanged?.Invoke(false); // 진행 아님

        ResetRepetitionHistory();
        UpdateCheckAndEnd();
        NotifyTimelineChanged();
    }

    public void OnClickBtnWhite() {
        if (gamePhase == GamePhase.Playing) {
            Debug.LogWarning("[Menu] 진행 중에는 White/Black 전환이 불가합니다.");
            return;
        }
        preferredHumanSide = Side.White;
        view.bottomIsWhite = true;
        ResetToOrientation();
    }

    public void OnClickBtnBlack() {
        if (gamePhase == GamePhase.Playing) {
            Debug.LogWarning("[Menu] 진행 중에는 White/Black 전환이 불가합니다.");
            return;
        }
        preferredHumanSide = Side.Black;
        view.bottomIsWhite = false;
        ResetToOrientation();
    }

    void ResetToOrientation() {
        waitingPromotion = false;
        ClearSelection();
        view.HideCheck();
        view.HideResult();

        isVsAI = false;
        aiThinking = false;

        gamePhase = GamePhase.Free;
        gameOverSfxPlayed = false;
        lastResultMessage = null;

        OnGamePhaseChanged?.Invoke(false); // 진행 아님

        timeline.Clear();
        timelineLabels.Clear();
        previewIndex = -1;
        NotifyTimelineChanged();

        board = Board.CreateInitial();
        view.RenderAll(board);
        startSnapshot = Snapshot.Capture(board);

        ResetRepetitionHistory();
    }
}