using System.Linq;
using System.Text;

namespace Dvonn_Console
{
    //This class is thought of as a notation format for exchanging dvonn board positions
    //It is more lightweight than the board class and can be added to evaluation trees...

    class Position
    {
        public int[] edgeFields = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 18, 19, 29, 30, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48 };
        public string[] stacks = new string[49];

        public Position()
        {
            for (int i = 0; i < 49; i++) stacks[i] = "";
        }

        public char? TopPiece(int fieldID)
        {
            int stackCount = stacks[fieldID].Length;
            if (stackCount > 0) return stacks[fieldID][stackCount - 1];
            else return null;
        }

        public int NumberOfStacks()
        {
            int counter = 0;
            foreach(string pieceList in stacks)
            {
                if (pieceList.Length == 0) continue;
                counter++;

            }
            return counter;

        }

        public bool IsEdge(int fieldID)
        {
            return edgeFields.Contains(fieldID);
        }

        public void MakeMove(Move move)
        {
            stacks[move.target] += stacks[move.source];
            stacks[move.source] = "";
        }

        public void Copy(Position positionToCopy)
        {
            for(int i=0; i < 49; i++)
            {
                stacks[i] = positionToCopy.stacks[i];
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Content of position: ");
            foreach(string stack in stacks)
            {
                if (stack.Length == 0) sb.Append("(), ");
                else sb.Append(stack + ", ");
            }
            return sb.ToString();
        }

    }


}
