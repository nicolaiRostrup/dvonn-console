using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    // Denne enum er lagt i namespacet, så alle classes kan bruge den.
    public enum PieceID
    {
        Dvonn, White, Black, Neutral
    }

    public static class PieceIDExtensions
    {
        public static char ToChar(this PieceID id)
        {
            if (id == PieceID.Dvonn) return 'D';
            if (id == PieceID.White) return 'W';
            if (id == PieceID.Black) return 'B';
            if (id == PieceID.Neutral) return 'N';

            throw new ArgumentException("Unexpected piece id received: " + id.ToString());

        }

        public static PieceID ToOpposite(this PieceID id)
        {
            if (id == PieceID.White) return PieceID.Black;
            if (id == PieceID.Black) return PieceID.White;

            throw new ArgumentException("Unexpected piece id received: " + id.ToString());

        }

    }

    public class Piece
    {
        public PieceID pieceType;


        public Piece(PieceID pieceType)
        {
            this.pieceType = pieceType;
        }
    }

}
