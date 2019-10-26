using System;
using System.Collections.Generic;
using System.Linq;


namespace Dvonn_Console
{
    public class Position
    {

        public List<Piece>[] stacks = new List<Piece>[49];
        public float evaluation = 0f;

        public Piece TopPiece(int fieldID)
        {
            int stackCount = stacks[fieldID].Count;
            if (stackCount > 0) return stacks[fieldID][ stackCount - 1];
            else return null;
        }

        public int NumberOfStacks()
        {
            int counter = 0;
            foreach(List<Piece> pieceList in stacks)
            {
                if (pieceList == null || pieceList.Count == 0) continue;
                counter++;

            }
            return counter;

        }

        public bool IsEdge(int fieldID)
        {
            int[] edgeFields = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 18, 19, 29, 30, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48 };
            return edgeFields.Contains(fieldID);
        }

    }


}
