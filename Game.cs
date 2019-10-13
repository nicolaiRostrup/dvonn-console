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

            position = DistributePieces(position, dvonnCount, pieceID.Dvonn);
            position = DistributePieces(position, whiteCount, pieceID.White);
            position = DistributePieces(position, blackCount, pieceID.Black);

            return position;
        }

        public Position DistributePieces(Position position, int pieceCount, pieceID pieceColor)
        {
            Random rGen = new Random();

            for (int i = 0; i < pieceCount; i++)
            {
                int rNum = rGen.Next(0, 49);

                if (position.stacks[rNum] == null)
                {
                    position.stacks[rNum] = new List<Piece>();
                    position.stacks[rNum].Add(new Piece(pieceColor));
                }
                else pieceCount++; //the field was occupied, run the loop once again.
            }

            return position;

        }

    }

}
