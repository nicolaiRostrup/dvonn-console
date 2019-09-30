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


        public string InspectStack()
        {
            return "";
        }


        public Piece TopPiece()
        {
            if (stack.Count > 0) return stack[stack.Count - 1];
            else return null;
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

    }
}
