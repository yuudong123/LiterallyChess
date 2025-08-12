// Assets/Scripts/Game/Chess/ChessGame.Move.cs
using UnityEngine;
using RetroChess.Core;

public partial class ChessGame {
    bool MovePiece(Vector2Int from, Vector2Int to) {
        var moving = board.squares[from.x, from.y];
        var targetBefore = board.squares[to.x, to.y];

        lastPreSnap = Snapshot.Capture(board);
        lastMoveFrom = from; lastMoveTo = to; lastMovingPiece = moving;

        bool isEnPassant = false;
        if (moving.Type == PieceType.Pawn &&
            to.x != from.x &&
            targetBefore.IsEmpty &&
            board.EnPassantTarget.HasValue &&
            board.EnPassantTarget.Value.x == to.x &&
            board.EnPassantTarget.Value.y == to.y) {
            isEnPassant = true;
        }
        lastIsEnPassant = isEnPassant;

        board.squares[from.x, from.y] = Piece.Empty;
        if (isEnPassant) {
            int dir = (moving.Side == Side.White) ? 1 : -1;
            var capSq = new Vector2Int(to.x, to.y - dir);
            board.squares[capSq.x, capSq.y] = Piece.Empty;
        }
        board.squares[to.x, to.y] = moving;

        bool didCastle = false;
        if (moving.Type == PieceType.King && Mathf.Abs(to.x - from.x) == 2) {
            int rank = (moving.Side == Side.White) ? 0 : 7;
            if (to.x == 6) {
                var rook = board.squares[7, rank]; board.squares[7, rank] = Piece.Empty; board.squares[5, rank] = rook;
            } else if (to.x == 2) {
                var rook = board.squares[0, rank]; board.squares[0, rank] = Piece.Empty; board.squares[3, rank] = rook;
            }
            didCastle = true;
        }
        lastDidCastle = didCastle;

        if (moving.Type == PieceType.King) {
            if (moving.Side == Side.White) { board.WhiteCastleK = false; board.WhiteCastleQ = false; }
            else { board.BlackCastleK = false; board.BlackCastleQ = false; }
        }
        if (moving.Type == PieceType.Rook) {
            if (moving.Side == Side.White) {
                if (from.x == 7 && from.y == 0) board.WhiteCastleK = false;
                if (from.x == 0 && from.y == 0) board.WhiteCastleQ = false;
            } else {
                if (from.x == 7 && from.y == 7) board.BlackCastleK = false;
                if (from.x == 0 && from.y == 7) board.BlackCastleQ = false;
            }
        }
        if (!targetBefore.IsEmpty && targetBefore.Type == PieceType.Rook) {
            if (targetBefore.Side == Side.White) {
                if (to.x == 7 && to.y == 0) board.WhiteCastleK = false;
                if (to.x == 0 && to.y == 0) board.WhiteCastleQ = false;
            } else {
                if (to.x == 7 && to.y == 7) board.BlackCastleK = false;
                if (to.x == 0 && to.y == 7) board.BlackCastleQ = false;
            }
        }

        board.EnPassantTarget = null;
        if (moving.Type == PieceType.Pawn) {
            if (Mathf.Abs(to.y - from.y) == 2) {
                int midY = (to.y + from.y) / 2;
                board.EnPassantTarget = new Vector2Int(from.x, midY);
            }
        }

        bool didCapture = (!targetBefore.IsEmpty) || isEnPassant;
        lastDidCapture = didCapture;
        if (moving.Type == PieceType.Pawn || didCapture) board.HalfmoveClock = 0;
        else board.HalfmoveClock++;

        bool willPromote =
            (moving.Type == PieceType.Pawn) &&
            ((moving.Side == Side.White && to.y == 7) || (moving.Side == Side.Black && to.y == 0));

        var opp = (moving.Side == Side.White) ? Side.Black : Side.White;
        bool oppInCheck = Rules.IsInCheck(board, opp);

        view.RenderAll(board);
        ClearSelection();

        if (willPromote) {
            if (isVsAI && gamePhase == GamePhase.Playing && board.SideToMove == aiSide) {
                waitingPromotion = true;
                pendingPromotionSq = to;
                pendingPromotionSide = moving.Side;
                return false;
            }
            waitingPromotion = true;
            pendingPromotionSq = to;
            pendingPromotionSide = moving.Side;
            view.ShowPromotionPopup(pendingPromotionSide, to, OnPromotionChosen);
            return false;
        }

        SfxManager.PlayMoveFx(view, from, to, didCapture, didCastle, oppInCheck);

        {
            Board pre = new Board(); Snapshot.Restore(pre, lastPreSnap);
            string label = BuildSAN(pre, board, lastMovingPiece, lastMoveFrom, lastMoveTo,
                                    lastDidCapture, lastDidCastle, lastIsEnPassant, false, PieceType.None);
            AddTimelineEntry(label);
        }

        return true;
    }

    void ForcePromoteAfterMove(PieceType chosen) {
        if (!waitingPromotion) return;
        board.squares[pendingPromotionSq.x, pendingPromotionSq.y] = new Piece {
            Side = pendingPromotionSide, Type = chosen
        };
        view.RenderAll(board);
        waitingPromotion = false;

        var opp = (pendingPromotionSide == Side.White) ? Side.Black : Side.White;
        bool checkAfter = Rules.IsInCheck(board, opp);
        SfxManager.PlayPromoteFx(view, pendingPromotionSq, checkAfter);

        {
            Board pre = new Board(); Snapshot.Restore(pre, lastPreSnap);
            string label = BuildSAN(pre, board, lastMovingPiece, lastMoveFrom, lastMoveTo,
                                    lastDidCapture, lastDidCastle, lastIsEnPassant, true, chosen);
            AddTimelineEntry(label);
        }

        EndTurn();
    }

    void OnPromotionChosen(PieceType chosen) {
        board.squares[pendingPromotionSq.x, pendingPromotionSq.y] = new Piece {
            Side = pendingPromotionSide, Type = chosen
        };
        view.RenderAll(board);
        waitingPromotion = false;

        var opp = (pendingPromotionSide == Side.White) ? Side.Black : Side.White;
        bool checkAfter = Rules.IsInCheck(board, opp);
        SfxManager.PlayPromoteFx(view, pendingPromotionSq, checkAfter);

        {
            Board pre = new Board(); Snapshot.Restore(pre, lastPreSnap);
            string label = BuildSAN(pre, board, lastMovingPiece, lastMoveFrom, lastMoveTo,
                                    lastDidCapture, lastDidCastle, lastIsEnPassant, true, chosen);
            AddTimelineEntry(label);
        }

        EndTurn();
    }
}