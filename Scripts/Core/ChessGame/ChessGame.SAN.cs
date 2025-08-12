using UnityEngine;
using RetroChess.Core;
using RetroChess.AI;

public partial class ChessGame {
    string BuildSAN(Board preBoard, Board postBoard, Piece moving, Vector2Int from, Vector2Int to,
                    bool didCapture, bool didCastle, bool isEnPassant,
                    bool promoted = false, PieceType promoType = PieceType.None) {
        if (moving.Type == PieceType.King && didCastle) return (to.x == 6) ? "O-O" : "O-O-O";

        string p = moving.Type switch {
            PieceType.Knight => "N", PieceType.Bishop => "B", PieceType.Rook => "R",
            PieceType.Queen  => "Q", PieceType.King   => "K", _ => ""
        };

        string dis = "";
        if (moving.Type != PieceType.Pawn && !didCastle) {
            if (HasAnotherAttacker(preBoard, moving, from, to, out bool needFile, out bool needRank)) {
                if (needFile) dis += (char)('a' + from.x);
                if (needRank) dis += (from.y + 1).ToString();
                if (!needFile && !needRank) dis = ((char)('a' + from.x)).ToString();
            }
        }

        string cap = "";
        if (didCapture) {
            if (moving.Type == PieceType.Pawn) cap = $"{(char)('a' + from.x)}x";
            else cap = "x";
        }

        string dst = SquareName(to.x, to.y);

        string promo = "";
        if (promoted && promoType != PieceType.None) {
            char c = promoType switch {
                PieceType.Knight => 'N', PieceType.Bishop => 'B', PieceType.Rook => 'R',
                PieceType.Queen  => 'Q', _ => 'Q'
            };
            promo = $"={c}";
        }

        var opp = (moving.Side == Side.White) ? Side.Black : Side.White;
        bool oppInCheck = Rules.IsInCheck(postBoard, opp);
        int oppLegal = CountAllLegalMoves(postBoard, opp);
        string checkSuffix = "";
        if (oppLegal == 0 && oppInCheck) checkSuffix = "#";
        else if (oppInCheck) checkSuffix = "+";

        string ep = isEnPassant ? " e.p." : "";

        if (moving.Type == PieceType.Pawn && !didCapture) {
            return $"{dst}{promo}{checkSuffix}";
        }
        return $"{p}{dis}{cap}{dst}{promo}{checkSuffix}{ep}";
    }

    bool HasAnotherAttacker(Board pre, Piece moving, Vector2Int from, Vector2Int to,
                            out bool needFile, out bool needRank) {
        needFile = false; needRank = false;
        bool anotherSameFile = false, anotherSameRank = false;
        bool anyOther = false;

        for (int f=0; f<8; f++) for (int r=0; r<8; r++) {
            if (f == from.x && r == from.y) continue;
            var p = pre.squares[f,r];
            if (p.IsEmpty || p.Side != moving.Side || p.Type != moving.Type) continue;

            var src = new Vector2Int(f, r);
            PseudoMoveGen.Generate(pre, f, r, out var pm, out var pc);
            bool can = false;
            foreach (var v in pm) if (v == to && IsLegalAfter(pre, src, to)) { can = true; break; }
            if (!can) foreach (var v in pc) if (v == to && IsLegalAfter(pre, src, to)) { can = true; break; }

            if (can) {
                anyOther = true;
                if (f == from.x) anotherSameFile = true;
                if (r == from.y) anotherSameRank = true;
            }
        }

        if (!anyOther) return false;

        if (anotherSameFile && anotherSameRank) { needFile = true; needRank = true; }
        else if (anotherSameFile) { needRank = true; }
        else if (anotherSameRank) { needFile = true; }
        else { needFile = true; }
        return true;
    }

    private static string SquareName(int file, int rank) => $"{(char)('a' + file)}{rank + 1}";
}