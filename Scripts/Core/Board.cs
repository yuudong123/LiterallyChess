using UnityEngine;

namespace RetroChess.Core {
    public class Board {
        public Piece[,] squares = new Piece[8,8];
        public Side SideToMove = Side.White;

        // 캐슬링 권리
        public bool WhiteCastleK = true, WhiteCastleQ = true;
        public bool BlackCastleK = true, BlackCastleQ = true;

        // 앙파상 타깃(직전 더블무브의 ‘지나친 칸’)
        public Vector2Int? EnPassantTarget = null;

        // 무승부/수순
        public int HalfmoveClock = 0;   // 폰 이동/캡처 시 0
        public int FullmoveNumber = 1;  // 흑이 둔 후 +1

        static Piece Make(Side s, PieceType t) => new Piece { Side = s, Type = t };

        public static Board CreateInitial() {
            var b = new Board();

            // 백
            b.squares[0,0] = Make(Side.White, PieceType.Rook);
            b.squares[1,0] = Make(Side.White, PieceType.Knight);
            b.squares[2,0] = Make(Side.White, PieceType.Bishop);
            b.squares[3,0] = Make(Side.White, PieceType.Queen);
            b.squares[4,0] = Make(Side.White, PieceType.King);
            b.squares[5,0] = Make(Side.White, PieceType.Bishop);
            b.squares[6,0] = Make(Side.White, PieceType.Knight);
            b.squares[7,0] = Make(Side.White, PieceType.Rook);
            for (int f=0; f<8; f++) b.squares[f,1] = Make(Side.White, PieceType.Pawn);

            // 흑
            b.squares[0,7] = Make(Side.Black, PieceType.Rook);
            b.squares[1,7] = Make(Side.Black, PieceType.Knight);
            b.squares[2,7] = Make(Side.Black, PieceType.Bishop);
            b.squares[3,7] = Make(Side.Black, PieceType.Queen);
            b.squares[4,7] = Make(Side.Black, PieceType.King);
            b.squares[5,7] = Make(Side.Black, PieceType.Bishop);
            b.squares[6,7] = Make(Side.Black, PieceType.Knight);
            b.squares[7,7] = Make(Side.Black, PieceType.Rook);
            for (int f=0; f<8; f++) b.squares[f,6] = Make(Side.Black, PieceType.Pawn);

            // 권리/카운터 초기화
            b.WhiteCastleK = b.WhiteCastleQ = true;
            b.BlackCastleK = b.BlackCastleQ = true;
            b.EnPassantTarget = null;
            b.HalfmoveClock = 0;
            b.FullmoveNumber = 1;

            return b;
        }
    }
}