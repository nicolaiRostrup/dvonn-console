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

    public class Piece
    {
        public PieceID pieceType;


        public Piece(PieceID pieceType)
        {
            this.pieceType = pieceType;
        }
    }

}
