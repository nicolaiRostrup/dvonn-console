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

        
        public void RandomPopulate(int dvonnCount, int whiteCount, int blackCount, ref Board thisBoard)
        {
            Random rGen = new Random();

            // For each type of piece, a loop is run to distribute all 49 pieces...
            for (int i = 0; i < dvonnCount; i++)
            {
                int rNum = rGen.Next(0, 49);
                Field selectedField = thisBoard.entireBoard[rNum];
                if (selectedField.stack.Count == 0) 
                {
                    selectedField.stack.Add(new Piece(pieceID.Dvonn));
                }
                else dvonnCount++; //the field was occupied, run the loop once again.
            }

            for (int i = 0; i < whiteCount; i++)
            {
                int rNum = rGen.Next(0, 49);
                Field selectedField = thisBoard.entireBoard[rNum];
                if (selectedField.stack.Count == 0)
                {
                    selectedField.stack.Add(new Piece(pieceID.White));
                }
                else whiteCount++;
            }

            for (int i = 0; i < blackCount; i++)
            {
                int rNum = rGen.Next(0, 49);
                Field selectedField = thisBoard.entireBoard[rNum];
                if (selectedField.stack.Count == 0)
                {
                    selectedField.stack.Add(new Piece(pieceID.Black));
                }
                else blackCount++;
            }
        }

    }

}
