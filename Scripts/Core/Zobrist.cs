using System;

namespace RetroChess.Core {
    public static class Zobrist {
        static readonly ulong[,,] pieceKeys = new ulong[2,6,64]; // side(0W/1B), piece(0..5), sq
        static readonly ulong sideKey;
        static readonly ulong[] castleKeys = new ulong[4]; // WK,WQ,BK,BQ
        static readonly ulong[] epFileKeys = new ulong[8]; // a..h

        static Zobrist() {
            var rnd = new Random(20240229);
            ulong Next() {
                var buffer = new byte[8];
                rnd.NextBytes(buffer);
                return BitConverter.ToUInt64(buffer, 0);
            }
            for (int s=0;s<2;s++)
                for (int p=0;p<6;p++)
                    for (int sq=0;sq<64;sq++)
                        pieceKeys[s,p,sq] = Next();

            sideKey = Next();
            for (int i=0;i<4;i++) castleKeys[i] = Next();
            for (int i=0;i<8;i++) epFileKeys[i] = Next();
        }

        static int PieceIndex(PieceType t) => t switch {
            PieceType.Pawn => 0,
            PieceType.Knight => 1,
            PieceType.Bishop => 2,
            PieceType.Rook => 3,
            PieceType.Queen => 4,
            PieceType.King => 5,
            _ => -1
        };

        public static ulong Compute(Board b) {
            ulong h = 0;
            for (int f=0; f<8; f++) for (int r=0; r<8; r++) {
                var p = b.squares[f,r];
                if (p.IsEmpty) continue;
                int pi = PieceIndex(p.Type);
                int s  = (p.Side == Side.White) ? 0 : 1;
                int sq = r*8 + f;
                if (pi >= 0) h ^= pieceKeys[s,pi,sq];
            }
            if (b.SideToMove == Side.Black) h ^= sideKey;
            if (b.WhiteCastleK) h ^= castleKeys[0];
            if (b.WhiteCastleQ) h ^= castleKeys[1];
            if (b.BlackCastleK) h ^= castleKeys[2];
            if (b.BlackCastleQ) h ^= castleKeys[3];
            if (b.EnPassantTarget.HasValue) h ^= epFileKeys[b.EnPassantTarget.Value.x];
            return h;
        }
    }
}