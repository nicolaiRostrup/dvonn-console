using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    public enum directionID
    {
        NW, NE, EA, SE, SW, WE
    }

    class Board
    {



        public Field[] entireBoard = new Field[49];
        //int 1: source field (in entireBoard), int 2: target field in entireBoard, int3: number of fields jumped.
        public List<Tuple<int, int, int>> allPrincipalMoves = new List<Tuple<int, int, int>>();

        public void CalculatePrincipalMoves()
        {
            Piece testPiece = new Piece(pieceID.Test);
            directionID[] Directions = { directionID.NE, directionID.EA, directionID.SE, directionID.SW, directionID.WE, directionID.NW };

            // find all legal moves in any direction for any field
            foreach (directionID Direction in Directions)
            {
                for (int i = 0; i < 49; i++) // For samtlige felter undersøges...
                {
                    Field sourceField = entireBoard[i];

                    for (int jump = 1; jump < 11; jump++) // Antal jumps er max 10, idet 10 er det længst mulige jump i Dvonn (c0/ct).
                    {
                        Field nextField = sourceField.NextField(Direction);
                        if (nextField != null)
                        {
                            allPrincipalMoves.Add(Tuple.Create(i, nextField.index), jump));
                        sourceField = nextField;
            }


            //AllPrincipalMoves.Count=786

        }

        public void VisualizeBoard(Game thisGame)
        {
            Console.Write("    ");

            for (int i = 0; i < 49; i++)
            {
                if (Stack.AllStacks[i].PieceList.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("(  )");
                }

                else
                {
                    Piece Toppiece = Calculate.TopPiece(i);
                    List<Piece> CurrentPieceList = Calculate.PieceList(i);
                    bool containsDvonn = CurrentPieceList.Any(item => item.PieceType == pieceID.Dvonn);

                    if (Toppiece.PieceType == pieceID.Dvonn)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("(");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("DV");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(")");
                    }
                    if (containsDvonn == false && Toppiece.PieceType == pieceID.White)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("(");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write((CurrentPieceList.Count + "W").ToString());
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(")");
                    }
                    if (containsDvonn == false && Toppiece.PieceType == pieceID.Black)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("(");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write((CurrentPieceList.Count + "B").ToString());
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(")");
                    }
                    if (containsDvonn == true && Toppiece.PieceType == pieceID.White)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("(");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write((CurrentPieceList.Count).ToString());
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("W");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(")");
                    }
                    if (containsDvonn == true && Toppiece.PieceType == pieceID.Black)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("(");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write((CurrentPieceList.Count).ToString());
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("B");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(")");
                    }

                }
                int[] newLines = { 8, 18, 29, 39 };

                if (newLines.Contains(i)) // Hvis iterationen i er lig med enten 8, 18, 29 eller 39 skal følgende kode gennemføres:
                {
                    Console.Write("\n");

                    int[] spaceIndents = new int[40];
                    spaceIndents[8] = 2; spaceIndents[18] = 0; spaceIndents[29] = 2; spaceIndents[39] = 4; //controls the number of spaces after each new line. Note that the first 4 spaces are entered separately in the top op the method.

                    for (int j = 0; j < spaceIndents[i]; j++) // this line only gets executed, when i is 8, 18, 29 or 39, which is why only 2, 0, 2 and 4 spaces gets printed after the new lines.
                    {
                        Console.Write(" ");
                    }
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }



    }
}
