namespace RetroChess.Core {
    public enum Side { White, Black }
    public enum PieceType { None, Pawn, Knight, Bishop, Rook, Queen, King }

    public struct Piece {
        public PieceType Type;
        public Side Side;
        public bool IsEmpty => Type == PieceType.None;
        public static readonly Piece Empty = new Piece { Type = PieceType.None };
    }
}