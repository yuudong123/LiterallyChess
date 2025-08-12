namespace RetroChess.Core {
    public static class Rules {
        static readonly (int df, int dr)[] KnightDirs = {
            (1,2),(2,1),(2,-1),(1,-2),(-1,-2),(-2,-1),(-2,1),(-1,2)
        };
        static readonly (int df, int dr)[] KingDirs = {
            (1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1)
        };
        static readonly (int df, int dr)[] BishopDirs = { (1,1),(1,-1),(-1,1),(-1,-1) };
        static readonly (int df, int dr)[] RookDirs   = { (1,0),(-1,0),(0,1),(0,-1) };

        public static bool IsInCheck(Board b, Side side) {
            if (!FindKing(b, side, out int kf, out int kr)) return false;
            Side opp = side == Side.White ? Side.Black : Side.White;
            return IsSquareAttacked(b, kf, kr, opp);
        }

        public static bool FindKing(Board b, Side side, out int kf, out int kr) {
            for (int f=0; f<8; f++) for (int r=0; r<8; r++) {
                var p = b.squares[f,r];
                if (!p.IsEmpty && p.Side == side && p.Type == PieceType.King) {
                    kf = f; kr = r; return true;
                }
            }
            kf = -1; kr = -1; return false;
        }

        public static bool IsSquareAttacked(Board b, int f, int r, Side by) {
            foreach (var (df,dr) in KnightDirs) {
                int nf = f + df, nr = r + dr;
                if (!InBoard(nf,nr)) continue;
                var p = b.squares[nf,nr];
                if (!p.IsEmpty && p.Side==by && p.Type==PieceType.Knight) return true;
            }
            if (AttackedBySliding(b, f, r, by, BishopDirs, PieceType.Bishop)) return true;
            if (AttackedBySliding(b, f, r, by, RookDirs, PieceType.Rook)) return true;
            foreach (var (df,dr) in KingDirs) {
                int nf=f+df, nr=r+dr;
                if (!InBoard(nf,nr)) continue;
                var p = b.squares[nf,nr];
                if (!p.IsEmpty && p.Side==by && p.Type==PieceType.King) return true;
            }
            int dir = (by == Side.White) ? 1 : -1;
            for (int df=-1; df<=1; df+=2) {
                int nf = f + df, nr = r - dir;
                if (!InBoard(nf,nr)) continue;
                var p = b.squares[nf,nr];
                if (!p.IsEmpty && p.Side==by && p.Type==PieceType.Pawn) return true;
            }
            return false;
        }

        static bool AttackedBySliding(Board b, int f, int r, Side by, (int,int)[] dirs, PieceType baseType) {
            foreach (var (df,dr) in dirs) {
                int nf=f+df, nr=r+dr;
                while (InBoard(nf,nr)) {
                    var p = b.squares[nf,nr];
                    if (!p.IsEmpty) {
                        if (p.Side==by && (p.Type==baseType || p.Type==PieceType.Queen)) return true;
                        break;
                    }
                    nf+=df; nr+=dr;
                }
            }
            return false;
        }

        static bool InBoard(int f, int r) => f>=0 && f<8 && r>=0 && r<8;

        public static bool IsInsufficientMaterial(Board b) {
            int pawns=0, rooks=0, queens=0, knights=0, bishops=0;
            bool bishopLight=false, bishopDark=false;

            for (int f=0; f<8; f++) for (int r=0; r<8; r++) {
                var p = b.squares[f,r];
                if (p.IsEmpty || p.Type==PieceType.King) continue;
                switch (p.Type) {
                    case PieceType.Pawn: pawns++; break;
                    case PieceType.Rook: rooks++; break;
                    case PieceType.Queen: queens++; break;
                    case PieceType.Knight: knights++; break;
                    case PieceType.Bishop:
                        bishops++;
                        bool isLight = ((f + r) % 2) == 0;
                        if (isLight) bishopLight = true; else bishopDark = true;
                        break;
                }
            }

            if (pawns>0 || rooks>0 || queens>0) return false;
            if (knights==0 && bishops==0) return true;
            if (knights==1 && bishops==0) return true;
            if (knights==0 && bishops==1) return true;
            if (knights==0 && bishops>=1 && !(bishopLight && bishopDark)) return true;

            return false;
        }
    }
}