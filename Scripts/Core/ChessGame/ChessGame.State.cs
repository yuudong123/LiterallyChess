// Assets/Scripts/Game/Chess/ChessGame.State.cs
using UnityEngine;
using RetroChess.Core;

public partial class ChessGame {
    void EndTurn() {
        if (board.SideToMove == Side.Black) board.FullmoveNumber++;
        board.SideToMove = (board.SideToMove == Side.White) ? Side.Black : Side.White;
        UpdateRepetitionHistory();
        UpdateCheckAndEnd();
        TryStartAITurn();
    }

    void ForceGameOverByConcede(Side winner, bool versusAI) {
        string msg;
        bool humanWin = false;
        bool isDraw = false;

        if (versusAI) {
            if (winner == preferredHumanSide) { msg = "win!"; humanWin = true; }
            else { msg = "Resign - You lose!"; humanWin = false; }
        } else {
            msg = $"Resign — {(winner == Side.White ? "White win!" : "Black win!")}";
        }
        SetGameOver(msg, humanWin, isDraw);
    }

    void SetGameOver(string message, bool humanWin, bool isDraw) {
        gamePhase = GamePhase.Ended;
        lastResultMessage = message;

        view.ShowResult(message);

        if (!gameOverSfxPlayed) {
            if (isVsAI && !isDraw && humanWin) SfxManager.PlayGameOverWin();
            else SfxManager.PlayGameOverDraw();
            gameOverSfxPlayed = true;
        }

        isVsAI = false;

        OnGamePhaseChanged?.Invoke(false); // 진행 아님
    }

    void UpdateCheckAndEnd() {
        if (gamePhase == GamePhase.Ended) {
            if (!string.IsNullOrEmpty(lastResultMessage)) view.ShowResult(lastResultMessage);
            return;
        }

        var side = board.SideToMove;

        bool inCheck = Rules.IsInCheck(board, side);
        if (inCheck) {
            if (Rules.FindKing(board, side, out int kf, out int kr))
                view.ShowCheck(new Vector2Int(kf, kr));
        } else {
            view.HideCheck();
        }

        bool terminal = false;
        string terminalMsg = null;
        bool humanWin = false;
        bool isDraw = false;

        int legalCount = CountAllLegalMoves(board, side);
        if (legalCount == 0) {
            if (inCheck) {
                Side winner = (side == Side.White) ? Side.Black : Side.White;
                if (isVsAI) {
                    if (winner == preferredHumanSide) { terminalMsg = "You win!"; humanWin = true; }
                    else { terminalMsg = "You lose!"; humanWin = false; }
                } else {
                    terminalMsg = $"{(winner == Side.White ? "White win!" : "Black win!")}";
                }
                terminal = true;
            } else {
                terminal = true; isDraw = true;
                terminalMsg = "Draw";
            }
        } else {
            if (board.HalfmoveClock >= 100) { terminal = true; isDraw = true; terminalMsg = "Draw"; }
            else if (IsThreefoldRepetition()) { terminal = true; isDraw = true; terminalMsg = "Draw"; }
            else if (Rules.IsInsufficientMaterial(board)) { terminal = true; isDraw = true; terminalMsg = "Draw"; }
        }

        if (gamePhase == GamePhase.Free) {
            view.HideResult();
            return;
        }

        if (terminal) {
            lastResultMessage = terminalMsg;
            gamePhase = GamePhase.Ended;
            view.ShowResult(terminalMsg);

            if (!gameOverSfxPlayed) {
                if (isVsAI && !isDraw && humanWin) SfxManager.PlayGameOverWin();
                else SfxManager.PlayGameOverDraw();
                gameOverSfxPlayed = true;
            }

            isVsAI = false;

            OnGamePhaseChanged?.Invoke(false); // 진행 아님
            return;
        }

        view.HideResult();
    }

    int CountAllLegalMoves(Board b, Side side) {
        int count = 0;
        for (int f=0; f<8; f++) for (int r=0; r<8; r++) {
            var p = b.squares[f,r];
            if (p.IsEmpty || p.Side != side) continue;

            PseudoMoveGen.Generate(b, f, r, out var moves, out var caps);
            foreach (var to in moves)  if (IsLegalAfter(b, new Vector2Int(f,r), to)) count++;
            foreach (var to in caps)   if (IsLegalAfter(b, new Vector2Int(f,r), to)) count++;
        }
        return count;
    }

    void ResetRepetitionHistory() {
        repetition.Clear();
        var h = Zobrist.Compute(board);
        repetition[h] = 1;
    }
    void UpdateRepetitionHistory() {
        var h = Zobrist.Compute(board);
        repetition.TryGetValue(h, out int c);
        repetition[h] = c + 1;
    }
    bool IsThreefoldRepetition() {
        var h = Zobrist.Compute(board);
        return repetition.TryGetValue(h, out int c) && c >= 3;
    }

    void ClearSelection() {
        selected = null;
        cachedMoves.Clear(); cachedCaps.Clear();
        view.ClearHighlights();
    }
}