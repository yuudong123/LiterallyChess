// Assets/Scripts/Game/Chess/ChessGame.Util.cs
using UnityEngine;
using RetroChess.Core;

public partial class ChessGame {
    Board CloneBoard(Board src) {
        var dst = new Board();
        for (int f=0; f<8; f++) for (int r=0; r<8; r++) dst.squares[f,r] = src.squares[f,r];
        dst.SideToMove      = src.SideToMove;
        dst.WhiteCastleK    = src.WhiteCastleK; dst.WhiteCastleQ = src.WhiteCastleQ;
        dst.BlackCastleK    = src.BlackCastleK; dst.BlackCastleQ = src.BlackCastleQ;
        dst.EnPassantTarget = src.EnPassantTarget;
        dst.HalfmoveClock   = src.HalfmoveClock;
        dst.FullmoveNumber  = src.FullmoveNumber;
        return dst;
    }

    bool IsLegalAfter(Board b, Vector2Int from, Vector2Int to) {
        var temp = CloneBoard(b);
        var moving = temp.squares[from.x, from.y];

        temp.squares[from.x, from.y] = Piece.Empty;

        if (moving.Type == PieceType.Pawn &&
            to.x != from.x &&
            temp.squares[to.x, to.y].IsEmpty &&
            temp.EnPassantTarget.HasValue &&
            temp.EnPassantTarget.Value.x == to.x &&
            temp.EnPassantTarget.Value.y == to.y) {
            int dir = (moving.Side == Side.White) ? 1 : -1;
            temp.squares[to.x, to.y - dir] = Piece.Empty;
        }

        temp.squares[to.x, to.y] = moving;

        if (moving.Type == PieceType.King && Mathf.Abs(to.x - from.x) == 2) {
            int rank = (moving.Side == Side.White) ? 0 : 7;
            if (to.x == 6) {
                var rook = temp.squares[7, rank]; temp.squares[7, rank] = Piece.Empty; temp.squares[5, rank] = rook;
            } else if (to.x == 2) {
                var rook = temp.squares[0, rank]; temp.squares[0, rank] = Piece.Empty; temp.squares[3, rank] = rook;
            }
        }

        return !Rules.IsInCheck(temp, moving.Side);
    }
}