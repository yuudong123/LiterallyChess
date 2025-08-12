using UnityEngine;

namespace RetroChess.Core {
    public struct Snapshot {
        public Piece[,] Squares;
        public Side SideToMove;
        public bool WCK, WCQ, BCK, BCQ;
        public Vector2Int? EnPassantTarget;
        public int HalfmoveClock;
        public int FullmoveNumber;

        public Snapshot(Piece[,] squares, Side stm, bool wck, bool wcq, bool bck, bool bcq,
                        Vector2Int? ep, int half, int full) {
            Squares = squares; SideToMove = stm;
            WCK = wck; WCQ = wcq; BCK = bck; BCQ = bcq;
            EnPassantTarget = ep;
            HalfmoveClock = half; FullmoveNumber = full;
        }

        public static Snapshot Capture(Board b) {
            var copy = new Piece[8,8];
            System.Array.Copy(b.squares, copy, b.squares.Length);
            return new Snapshot(copy, b.SideToMove,
                b.WhiteCastleK, b.WhiteCastleQ, b.BlackCastleK, b.BlackCastleQ,
                b.EnPassantTarget, b.HalfmoveClock, b.FullmoveNumber);
        }

        public static void Restore(Board b, in Snapshot s) {
            System.Array.Copy(s.Squares, b.squares, b.squares.Length);
            b.SideToMove = s.SideToMove;
            b.WhiteCastleK = s.WCK; b.WhiteCastleQ = s.WCQ;
            b.BlackCastleK = s.BCK; b.BlackCastleQ = s.BCQ;
            b.EnPassantTarget = s.EnPassantTarget;
            b.HalfmoveClock = s.HalfmoveClock;
            b.FullmoveNumber = s.FullmoveNumber;
        }
    }
}