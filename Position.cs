using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class Position
    {

        public List<Piece>[] stacks = new List<Piece>[49];

        public Piece TopPiece(int fieldID)
        {
            int stackCount = stacks[fieldID].Count;
            if (stackCount > 0) return stacks[fieldID][ stackCount - 1];
            else return null;
        }

        public bool IsEdge(int fieldID)
        {
            int[] edgeFields = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 18, 19, 29, 30, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48 };
            return edgeFields.Contains(fieldID);
        }

    }


}
