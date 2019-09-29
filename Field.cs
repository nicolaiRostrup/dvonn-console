using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class Field
    {

        public string fieldName;
        public int index; //index in entireBoard array
        public List<Piece> stack = new List<Piece>();

        Field NE;
        Field E;
        Field SE;
        Field SW;
        Field W;
        Field NW;


        public string InspectStack()
        {
            return "";
        }


        public pieceID? TopPiece()
        {
            if (stack.Count == 0) return null;
            else return stack[stack.Count - 1].pieceType;
        }

        public Field NextField(directionID direction)
        {
            if(direction == NE) return...

                return null;
        }

    }
}
