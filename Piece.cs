using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    // Denne enum er lagt i namespacet, så alle classes kan bruge den.
    public enum pieceID
    {
        Dvonn, White, Black
    }

    public class Piece
    {
        public pieceID pieceType;


        public Piece(pieceID pieceType)
        {
            this.pieceType = pieceType;
        }
    }

}
