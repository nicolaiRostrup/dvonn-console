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

        public bool isEdge;

        public Field NE;
        public Field EA;
        public Field SE;
        public Field SW;
        public Field WE;
        public Field NW;



        public Piece TopPiece()
        {
            if (stack.Count > 0) return stack[stack.Count - 1];
            else return null;
        }

        public void DeleteTopPiece()
        {
            if (stack.Count > 0) stack.RemoveAt(stack.Count - 1);

        }


        public Field NextField(directionID direction)
        {
            if (direction == directionID.NE) return NE;
            if (direction == directionID.EA) return EA;
            if (direction == directionID.SE) return SE;
            if (direction == directionID.SW) return SW;
            if (direction == directionID.WE) return WE;
            if (direction == directionID.NW) return NW;

            else return null;
        }

        public List<Field> GetNeighbours()
        {
            List<Field> theseNeighbours = new List<Field>();
            if (NE != null) theseNeighbours.Add(NE);
            if (EA != null) theseNeighbours.Add(EA);
            if (SE != null) theseNeighbours.Add(SE);
            if (SW != null) theseNeighbours.Add(SW);
            if (WE != null) theseNeighbours.Add(WE);
            if (NW != null) theseNeighbours.Add(NW);

            return theseNeighbours;
        }

        public Field GetNeighbour(directionID dir)
        {
            if (dir == directionID.NE) return (NE != null) ? NE: null;
            if (dir == directionID.EA) return (EA != null) ? EA : null;
            if (dir == directionID.SE) return (SE != null) ? SE : null;
            if (dir == directionID.SW) return (SW != null) ? SW : null;
            if (dir == directionID.WE) return (WE != null) ? WE : null;
            if (dir == directionID.NW) return (NW != null) ? NW : null;
            else throw new ArgumentException("Direction id not supported : " + dir.ToString());
        }

        public override string ToString()
        {
            string returnString;
            if (stack.Count == 0) returnString = "This field is currently empty";
            else
            {
                returnString = "Field " + fieldName + " has a stack that consists of " + stack.Count + " pieces. They are: (From bottom and up) ";
                foreach (Piece p in stack)
                {
                    returnString += p.pieceType + ", ";
                }

            }
            return returnString;
        }

    }
}
