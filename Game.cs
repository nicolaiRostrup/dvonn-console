using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class Game
    {

        //public Player whitePlayer;
        //public Player blackPlayer;


        public Position RandomPopulate(int dvonnCount, int whiteCount, int blackCount)
        {
            Position position = new Position();

            position = DistributePieces(position, dvonnCount, PieceID.Dvonn);
            position = DistributePieces(position, whiteCount, PieceID.White);
            position = DistributePieces(position, blackCount, PieceID.Black);

            return position;
        }

        public Position DistributePieces(Position position, int pieceCount, PieceID pieceColor)
        {
            Random rGen = new Random();

            for (int i = 0; i < pieceCount; i++)
            {
                int rNum = rGen.Next(0, 49);

                if (position.stacks[rNum].Length == 0)
                {
                    position.stacks[rNum] += GetChar(pieceColor);
                }
                else pieceCount++; //the field was occupied, run the loop once again.
            }

            return position;

        }

        char GetChar(PieceID pieceColor)
        {
            if (pieceColor == PieceID.Black) return 'B';
            if (pieceColor == PieceID.White) return 'W';
            if (pieceColor == PieceID.Dvonn) return 'D';

            else return 'e'; //e for error;

        }

    }

}
