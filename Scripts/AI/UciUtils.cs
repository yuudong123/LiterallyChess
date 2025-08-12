// Assets/Scripts/AI/UciUtils.cs
using System.Text;
using RetroChess.Core;
using UnityEngine;

namespace RetroChess.AI {
    public static class UciUtils {
        public static string ToFen(Board b) {
            var sb = new StringBuilder();
            for (int r = 7; r >= 0; r--) {
                int empty = 0;
                for (int f = 0; f < 8; f++) {
                    var p = b.squares[f, r];
                    if (p.IsEmpty) { empty++; continue; }
                    if (empty > 0) { sb.Append(empty); empty = 0; }
                    sb.Append(PieceChar(p));
                }
                if (empty > 0) sb.Append(empty);
                if (r > 0) sb.Append('/');
            }
            sb.Append(' ').Append(b.SideToMove == Side.White ? 'w' : 'b').Append(' ');
            string castle = "";
            if (b.WhiteCastleK) castle += "K";
            if (b.WhiteCastleQ) castle += "Q";
            if (b.BlackCastleK) castle += "k";
            if (b.BlackCastleQ) castle += "q";
            sb.Append(castle.Length == 0 ? "-" : castle).Append(' ');
            if (b.EnPassantTarget.HasValue) sb.Append(SqName(b.EnPassantTarget.Value.x, b.EnPassantTarget.Value.y));
            else sb.Append('-');
            sb.Append(' ').Append(b.HalfmoveClock).Append(' ').Append(b.FullmoveNumber);
            return sb.ToString();
        }

        static char PieceChar(Piece p) {
            char c = p.Type switch {
                PieceType.Pawn => 'p', PieceType.Knight => 'n', PieceType.Bishop => 'b',
                PieceType.Rook => 'r', PieceType.Queen => 'q', PieceType.King => 'k', _ => '.'
            };
            return p.Side == Side.White ? char.ToUpper(c) : c;
        }

        static string SqName(int f, int r) => $"{(char)('a'+f)}{r+1}";

        public static (Vector2Int from, Vector2Int to, PieceType promo) ParseUciMove(string uci) {
            if (string.IsNullOrEmpty(uci) || uci.Length < 4) return (new Vector2Int(-1,-1), new Vector2Int(-1,-1), PieceType.None);
            int f1 = uci[0] - 'a', r1 = uci[1] - '1';
            int f2 = uci[2] - 'a', r2 = uci[3] - '1';
            PieceType promo = PieceType.None;
            if (uci.Length >= 5) {
                promo = uci[4] switch {
                    'q'=>PieceType.Queen, 'r'=>PieceType.Rook, 'b'=>PieceType.Bishop, 'n'=>PieceType.Knight, _=>PieceType.None
                };
            }
            return (new Vector2Int(f1,r1), new Vector2Int(f2,r2), promo);
        }

        public static string[] EmptyMoveList() => System.Array.Empty<string>();
    }
}