using System;


namespace Dvonn_Console
{
    
    public class Piece
    {
        public PieceID pieceType;

        public Piece(PieceID pieceType)
        {
            this.pieceType = pieceType;
        }
    }

    public enum PieceID
    {
        Dvonn, White, Black
    }

    public static class PieceIDExtensions
    {
        public static char ToChar(this PieceID id)
        {
            if (id == PieceID.Dvonn) return 'D';
            if (id == PieceID.White) return 'W';
            if (id == PieceID.Black) return 'B';

            throw new ArgumentException("Unexpected piece id received: " + id.ToString());

        }

        public static PieceID ToOpposite(this PieceID id)
        {
            if (id == PieceID.White) return PieceID.Black;
            if (id == PieceID.Black) return PieceID.White;

            throw new ArgumentException("Unexpected piece id received: " + id.ToString());

        }

    }

    

}
