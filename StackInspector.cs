using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class StackInspector
    {
        
        Board thisBoard;
        Rules ruleBook;


        public StackInspector(Board dvonnBoard)
        {
            thisBoard = dvonnBoard;
            ruleBook = new Rules(dvonnBoard);
        }

        public void InspectStack(string fieldName)
        {
            int fieldID = thisBoard.entireBoard.First(field => field.fieldName == fieldName).index;

            List<Piece> chosenPieceList = thisBoard.entireBoard[fieldID].stack;

            List<int> legalTargets = ruleBook.FindLegalTargets(fieldID);


            if (chosenPieceList.Count == 0)
            {
                Console.WriteLine("This field is empty. Nothing to inspect");
            }

            if (chosenPieceList.Count == 1)
            {
                Console.WriteLine("This field is a one piece stack. The color is " + chosenPieceList[0].pieceType);

                if (chosenPieceList[0].pieceType != PieceID.Dvonn)
                {
                    if (legalTargets.Count == 0) Console.WriteLine(fieldName + " has currently no legal targets:");
                    else
                    {
                        Console.WriteLine(fieldName + " has currently " + legalTargets.Count + " legal targets:");
                        foreach (int target in legalTargets)
                        {
                            Console.Write(thisBoard.entireBoard[target].fieldName + ", ");
                        }
                    }
                }
                Console.WriteLine();

            }
            if (chosenPieceList.Count > 1)
            {
                Console.WriteLine(fieldName + " is currently a stack of " + chosenPieceList.Count + ". Top color is " + thisBoard.entireBoard[fieldID].TopPiece().pieceType + ".");
                Console.WriteLine("The stack comprises: " + chosenPieceList.Count(p => p.pieceType == PieceID.White) + " White pieces and " + chosenPieceList.Count(p => p.pieceType == PieceID.Black) + " Black pieces.");
                if (chosenPieceList.Count(p => p.pieceType == PieceID.Dvonn) > 0) Console.WriteLine("The stack also contains " + chosenPieceList.Count(p => p.pieceType == PieceID.Dvonn) + " Dvonn pieces.");
                else Console.WriteLine("The stack contains no Dvonn pieces.");

                if (legalTargets.Count == 0) Console.WriteLine(fieldName + " has currently no legal targets:");
                else
                {
                    Console.WriteLine(fieldName + " has currently " + legalTargets.Count + " legal targets:");
                    foreach (int target in legalTargets)
                    {
                        Console.Write(thisBoard.entireBoard[target].fieldName + ", ");
                    }
                }

            }
            Console.WriteLine();

        }


    }
}
