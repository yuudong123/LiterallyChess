using System.Collections.Generic;
using UnityEngine;

namespace RetroChess.Core {
    public static class PseudoMoveGen {
        static readonly (int df,int dr)[] KnightDirs = {
            (1,2),(2,1),(2,-1),(1,-2),(-1,-2),(-2,-1),(-2,1),(-1,2)
        };
        static readonly (int df,int dr)[] KingDirs = {
            (1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1)
        };
        static readonly (int df,int dr)[] BishopDirs = {
            (1,1),(1,-1),(-1,1),(-1,-1)
        };
        static readonly (int df,int dr)[] RookDirs = {
            (1,0),(-1,0),(0,1),(0,-1)
        };

        public static void Generate(Board b, int f, int r, out List<Vector2Int> moves, out List<Vector2Int> captures) {
            moves = new List<Vector2Int>();
            captures = new List<Vector2Int>();

            var p = b.squares[f, r];
            if (p.IsEmpty) return;

            switch (p.Type) {
                case PieceType.Pawn:   PawnMoves(b, f, r, p.Side, moves, captures); break;
                case PieceType.Knight: ForEachDirOnce(b, f, r, p.Side, KnightDirs, moves, captures); break;
                case PieceType.Bishop: SlideDirs(b, f, r, p.Side, BishopDirs, moves, captures); break;
                case PieceType.Rook:   SlideDirs(b, f, r, p.Side, RookDirs, moves, captures); break;
                case PieceType.Queen:
                    SlideDirs(b, f, r, p.Side, BishopDirs, moves, captures);
                    SlideDirs(b, f, r, p.Side, RookDirs, moves, captures);
                    break;
                case PieceType.King:
                    ForEachDirOnce(b, f, r, p.Side, KingDirs, moves, captures);
                    AddCastlingMoves(b, f, r, p.Side, moves);
                    break;
            }
        }

        static void ForEachDirOnce(Board b, int f, int r, Side side, (int df,int dr)[] dirs, List<Vector2Int> moves, List<Vector2Int> captures) {
            foreach (var (df, dr) in dirs) {
                int nf = f + df, nr = r + dr;
                if (!InBoard(nf, nr)) continue;
                var target = b.squares[nf, nr];
                if (target.IsEmpty) moves.Add(new Vector2Int(nf, nr));
                else if (target.Side != side) captures.Add(new Vector2Int(nf, nr));
            }
        }

        static void SlideDirs(Board b, int f, int r, Side side, (int df,int dr)[] dirs, List<Vector2Int> moves, List<Vector2Int> captures) {
            foreach (var (df, dr) in dirs) {
                int nf = f + df, nr = r + dr;
                while (InBoard(nf, nr)) {
                    var target = b.squares[nf, nr];
                    if (target.IsEmpty) moves.Add(new Vector2Int(nf, nr));
                    else {
                        if (target.Side != side) captures.Add(new Vector2Int(nf, nr));
                        break;
                    }
                    nf += df; nr += dr;
                }
            }
        }

        static void PawnMoves(Board b, int f, int r, Side side, List<Vector2Int> moves, List<Vector2Int> captures) {
            int dir = side == Side.White ? 1 : -1;
            int startRank = side == Side.White ? 1 : 6;
            int one = r + dir;

            if (InBoard(f, one) && b.squares[f, one].IsEmpty) {
                moves.Add(new Vector2Int(f, one));
                int two = r + 2 * dir;
                if (r == startRank && InBoard(f, two) && b.squares[f, two].IsEmpty)
                    moves.Add(new Vector2Int(f, two));
            }

            int left = f - 1, right = f + 1;
            if (InBoard(left, one)) {
                var t = b.squares[left, one];
                if (!t.IsEmpty && t.Side != side) captures.Add(new Vector2Int(left, one));
            }
            if (InBoard(right, one)) {
                var t = b.squares[right, one];
                if (!t.IsEmpty && t.Side != side) captures.Add(new Vector2Int(right, one));
            }

            if (b.EnPassantTarget.HasValue) {
                var ep = b.EnPassantTarget.Value;
                if (InBoard(left, one) && b.squares[left, one].IsEmpty && ep.x == left && ep.y == one)
                    captures.Add(new Vector2Int(left, one));
                if (InBoard(right, one) && b.squares[right, one].IsEmpty && ep.x == right && ep.y == one)
                    captures.Add(new Vector2Int(right, one));
            }
        }

        static void AddCastlingMoves(Board b, int f, int r, Side side, List<Vector2Int> moves) {
            if (!(f == 4 && (r == 0 || r == 7))) return;
            if (Rules.IsInCheck(b, side)) return;

            int rank = (side == Side.White) ? 0 : 7;
            Side opp = (side == Side.White) ? Side.Black : Side.White;

            bool canK = (side == Side.White ? b.WhiteCastleK : b.BlackCastleK);
            if (canK) {
                if (Empty(b, 5, rank) && Empty(b, 6, rank) &&
                    HasRookAt(b, 7, rank, side) &&
                    !Rules.IsSquareAttacked(b, 5, rank, opp) &&
                    !Rules.IsSquareAttacked(b, 6, rank, opp)) {
                    moves.Add(new Vector2Int(6, rank));
                }
            }

            bool canQ = (side == Side.White ? b.WhiteCastleQ : b.BlackCastleQ);
            if (canQ) {
                if (Empty(b, 1, rank) && Empty(b, 2, rank) && Empty(b, 3, rank) &&
                    HasRookAt(b, 0, rank, side) &&
                    !Rules.IsSquareAttacked(b, 3, rank, opp) &&
                    !Rules.IsSquareAttacked(b, 2, rank, opp)) {
                    moves.Add(new Vector2Int(2, rank));
                }
            }
        }

        static bool HasRookAt(Board b, int f, int r, Side side) {
            if (!InBoard(f, r)) return false;
            var p = b.squares[f, r];
            return !p.IsEmpty && p.Side == side && p.Type == PieceType.Rook;
        }

        static bool Empty(Board b, int f, int r) => InBoard(f, r) && b.squares[f, r].IsEmpty;
        static bool InBoard(int f, int r) => f >= 0 && f < 8 && r >= 0 && r < 8;
    }
}