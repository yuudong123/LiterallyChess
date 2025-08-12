// Assets/Scripts/Game/Chess/ChessGame.AI.cs
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using RetroChess.Core;
using RetroChess.AI;

public partial class ChessGame {
    [Header("AI Settings")]
    public bool isVsAI = false;
    public Side aiSide = Side.Black;

    [Range(1,8)] public int aiDepth = 5;
    public float aiMinMoveTime = 1.0f;
    public bool aiDelayUseRealtime = true;
    public float aiMaxThinkTime = 2.0f;

    [Header("AI Weak Mode")]
    [Range(600, 3000)] public int aiWeakElo = 1200;
    [Range(-1, 20)]   public int aiWeakSkill = -1;
    public bool aiUseLimitStrength = true;

    bool aiThinking = false;
    public bool IsAIThinking => aiThinking;

    UciEngine _uci;
    CancellationTokenSource _uciCts;

    float Now() => aiDelayUseRealtime ? Time.unscaledTime : Time.time;

    public void StartVsAI(Side humanSide = Side.White) {
        OnClickNewGame();

        isVsAI = true;
        aiSide = (humanSide == Side.White) ? Side.Black : Side.White;

        if (_uci == null) _uci = new UciEngine();
        _uci.ConfigureWeak(aiWeakElo, aiWeakSkill, aiUseLimitStrength);
        if (!_uci.IsRunning) _uci.Start();

        gamePhase = GamePhase.Playing;
        OnGamePhaseChanged?.Invoke(true); // 진행 시작

        TryStartAITurn();
    }

    public void StartVsAI() { StartVsAI(preferredHumanSide); }

    public void GiveUpVsAI() { ConcedeByHuman(); }

    void OnDestroy() { _uci?.Stop(); _uci = null; }

    void TryStartAITurn() {
        if (!isVsAI) return;
        if (gamePhase != GamePhase.Playing) return;
        if (waitingPromotion) return;
        if (previewMode) return;
        if (aiThinking) return;
        if (board.SideToMove != aiSide) return;

        StartCoroutine(AIThinkAndMoveAsync());
    }

    IEnumerator AIThinkAndMoveAsync() {
        aiThinking = true;
        yield return null;

        float tStart = Now();

        string fen = UciUtils.ToFen(board);
        string[] hist = UciUtils.EmptyMoveList();

        _uciCts?.Cancel();
        _uciCts = new CancellationTokenSource();
        var token = _uciCts.Token;

        var bestTask = _uci.GetBestMoveAsync(
            fenOrStartPos: fen,
            uciMoves: hist,
            depth: aiDepth,
            movetimeMs: Mathf.RoundToInt(aiMaxThinkTime * 1000f),
            ct: token
        );

        while (!bestTask.IsCompleted) { yield return null; }

        string bestUci = bestTask.Result;

        float elapsed = Now() - tStart;
        float remain = Mathf.Max(0f, aiMinMoveTime - elapsed);
        if (remain > 0f) {
            if (aiDelayUseRealtime) yield return new WaitForSecondsRealtime(remain);
            else yield return new WaitForSeconds(remain);
        }

        if (!isVsAI || waitingPromotion || previewMode || gamePhase != GamePhase.Playing || board.SideToMove != aiSide) {
            aiThinking = false; yield break;
        }

        if (!string.IsNullOrEmpty(bestUci)) {
            var (from, to, promo) = UciUtils.ParseUciMove(bestUci);
            if (from.x >= 0 && to.x >= 0) {
                bool endTurn = MovePiece(from, to);
                if (endTurn) {
                    EndTurn();
                } else {
                    if (promo != PieceType.None) {
                        ForcePromoteAfterMove(promo);
                    }
                }
            }
        }

        aiThinking = false;
        TryStartAITurn();
    }
}