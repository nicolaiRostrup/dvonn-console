﻿using System.Linq;
using System.Text;

namespace Dvonn_Console
{
    //This class is thought of as a notation format for exchanging dvonn board positions
    //It is more lightweight than the board class and can be added to evaluation trees...

    public class Position
    {
        
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
            return BoardProperties.edgeFields.Contains(fieldID);
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

        public int GetScore(PieceID color)
        {
            int score = 0;
            for (int i = 0; i < 49; i++)
            {
                int stackCount = stacks[i].Length;
                if (stackCount == 0) continue;
                if (TopPiece(i) == color.ToChar()) score += stackCount;
                
            }
            return score;
        }

        

        //position notation 2:
        //"W/W/BW/-/B/-/-/-/B"
        //"/-/-/-/-/-/WBDBBWWW/BW/-/B/-/-/-/B"
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 49; i++)
            {
                if (stacks[i].Length == 0) sb.Append("/-");
                else sb.Append("/" + stacks[i]);

                if (i == 8 || i == 18 || i == 29 || i == 39 || i == 48) sb.AppendLine("/");
            }
            return sb.ToString();
        }

    }


}
