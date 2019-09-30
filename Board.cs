﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    public enum directionID
    {
        NE, EA, SE, SW, WE, NW
    }

    class Board
    {

        public Field[] entireBoard = new Field[49];
        
        //int 1: source field (in entireBoard), int 2: target field in entireBoard, int3: number of fields jumped.
        public List<Tuple<int, int, int>> allPrincipalMoves = new List<Tuple<int, int, int>>();

        public void CalculatePrincipalMoves()
        {
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
                            allPrincipalMoves.Add(Tuple.Create(i, nextField.index, jump));
                            sourceField = nextField;
                        }

                        //AllPrincipalMoves.Count=786

                    }
                }
            }
        }

        public void InstantiateFields()
        {
            Field A0 = new Field(); Field A1 = new Field(); Field A2 = new Field(); Field A3 = new Field(); Field A4 = new Field(); Field A5 = new Field(); Field A6 = new Field(); Field A7 = new Field(); Field A8 = new Field();
            Field B0 = new Field(); Field B1 = new Field(); Field B2 = new Field(); Field B3 = new Field(); Field B4 = new Field(); Field B5 = new Field(); Field B6 = new Field(); Field B7 = new Field(); Field B8 = new Field(); Field B9 = new Field();
            Field C0 = new Field(); Field C1 = new Field(); Field C2 = new Field(); Field C3 = new Field(); Field C4 = new Field(); Field C5 = new Field(); Field C6 = new Field(); Field C7 = new Field(); Field C8 = new Field(); Field C9 = new Field(); Field CT = new Field();
            Field D0 = new Field(); Field D1 = new Field(); Field D2 = new Field(); Field D3 = new Field(); Field D4 = new Field(); Field D5 = new Field(); Field D6 = new Field(); Field D7 = new Field(); Field D8 = new Field(); Field D9 = new Field();
            Field E0 = new Field(); Field E1 = new Field(); Field E2 = new Field(); Field E3 = new Field(); Field E4 = new Field(); Field E5 = new Field(); Field E6 = new Field(); Field E7 = new Field(); Field E8 = new Field();


            A0.index = 0; A0.NW = null; A0.NE = null; A0.EA = A1; A0.SE = B1; A0.SW = B0; A0.WE = null; A0.isEdge = true; entireBoard[0] = A0;
            A1.index = 1; A1.NW = null; A1.NE = null; A1.EA = A2; A1.SE = B2; A1.SW = B1; A1.WE = A0; A1.isEdge = true; entireBoard[1] = A1;
            A2.index = 2; A2.NW = null; A2.NE = null; A2.EA = A3; A2.SE = B3; A2.SW = B2; A2.WE = A1; A2.isEdge = true; entireBoard[2] = A2;
            A3.index = 3; A3.NW = null; A3.NE = null; A3.EA = A4; A3.SE = B4; A3.SW = B3; A3.WE = A2; A3.isEdge = true; entireBoard[3] = A3;
            A4.index = 4; A4.NW = null; A4.NE = null; A4.EA = A5; A4.SE = B5; A4.SW = B4; A4.WE = A3; A4.isEdge = true; entireBoard[4] = A4;
            A5.index = 5; A5.NW = null; A5.NE = null; A5.EA = A6; A5.SE = B6; A5.SW = B5; A5.WE = A4; A5.isEdge = true; entireBoard[5] = A5;
            A6.index = 6; A6.NW = null; A6.NE = null; A6.EA = A7; A6.SE = B7; A6.SW = B6; A6.WE = A5; A6.isEdge = true; entireBoard[6] = A6;
            A7.index = 7; A7.NW = null; A7.NE = null; A7.EA = A8; A7.SE = B8; A7.SW = B7; A7.WE = A6; A7.isEdge = true; entireBoard[7] = A7;
            A8.index = 8; A8.NW = null; A8.NE = null; A8.EA = null; A8.SE = B9; A8.SW = B8; A8.WE = A7; A8.isEdge = true; entireBoard[8] = A8;

            B0.index = 9; B0.NW = null; B0.NE = A0; B0.EA = B1; B0.SE = C1; B0.SW = C0; B0.WE = null; B0.isEdge = true; entireBoard[9] = B0;
            B1.index = 10; B1.NW = A0; B1.NE = A1; B1.EA = B2; B1.SE = C2; B1.SW = C1; B1.WE = B0; B1.isEdge = false; entireBoard[10] = B1;
            B2.index = 11; B2.NW = A1; B2.NE = A2; B2.EA = B3; B2.SE = C3; B2.SW = C2; B2.WE = B1; B2.isEdge = false; entireBoard[11] = B2;
            B3.index = 12; B3.NW = A2; B3.NE = A3; B3.EA = B4; B3.SE = C4; B3.SW = C3; B3.WE = B2; B3.isEdge = false; entireBoard[12] = B3;
            B4.index = 13; B4.NW = A3; B4.NE = A4; B4.EA = B5; B4.SE = C5; B4.SW = C4; B4.WE = B3; B4.isEdge = false; entireBoard[13] = B4;
            B5.index = 14; B5.NW = A4; B5.NE = A5; B5.EA = B6; B5.SE = C6; B5.SW = C5; B5.WE = B4; B5.isEdge = false; entireBoard[14] = B5;
            B6.index = 15; B6.NW = A5; B6.NE = A6; B6.EA = B7; B6.SE = C7; B6.SW = C6; B6.WE = B5; B6.isEdge = false; entireBoard[15] = B6;
            B7.index = 16; B7.NW = A6; B7.NE = A7; B7.EA = B8; B7.SE = C8; B7.SW = C7; B7.WE = B6; B7.isEdge = false; entireBoard[16] = B7;
            B8.index = 17; B8.NW = A7; B8.NE = A8; B8.EA = B9; B8.SE = C9; B8.SW = C8; B8.WE = B7; B8.isEdge = false; entireBoard[17] = B8;
            B9.index = 18; B9.NW = A8; B9.NE = null; B9.EA = null; B9.SE = CT; B9.SW = C9; B9.WE = B8; B9.isEdge = true; entireBoard[18] = B9;

            C0.index = 19; C0.NW = null; C0.NE = B0; C0.EA = C1; C0.SE = D0; C0.SW = null; C0.WE = null; C0.isEdge = true; entireBoard[19] = C0;
            C1.index = 20; C1.NW = B0; C1.NE = B1; C1.EA = C2; C1.SE = D1; C1.SW = D0; C1.WE = C0; C1.isEdge = false; entireBoard[20] = C1;
            C2.index = 21; C2.NW = B1; C2.NE = B2; C2.EA = C3; C2.SE = D2; C2.SW = D1; C2.WE = C1; C2.isEdge = false; entireBoard[21] = C2;
            C3.index = 22; C3.NW = B2; C3.NE = B3; C3.EA = C4; C3.SE = D3; C3.SW = D2; C3.WE = C2; C3.isEdge = false; entireBoard[22] = C3;
            C4.index = 23; C4.NW = B3; C4.NE = B4; C4.EA = C5; C4.SE = D4; C4.SW = D3; C4.WE = C3; C4.isEdge = false; entireBoard[23] = C4;
            C5.index = 24; C5.NW = B4; C5.NE = B5; C5.EA = C6; C5.SE = D5; C5.SW = D4; C5.WE = C4; C5.isEdge = false; entireBoard[24] = C5;
            C6.index = 25; C6.NW = B5; C6.NE = B6; C6.EA = C7; C6.SE = D6; C6.SW = D5; C6.WE = C5; C6.isEdge = false; entireBoard[25] = C6;
            C7.index = 26; C7.NW = B6; C7.NE = B7; C7.EA = C8; C7.SE = D7; C7.SW = D6; C7.WE = C6; C7.isEdge = false; entireBoard[26] = C7;
            C8.index = 27; C8.NW = B7; C8.NE = B8; C8.EA = C9; C8.SE = D8; C8.SW = D7; C8.WE = C7; C8.isEdge = false; entireBoard[27] = C8;
            C9.index = 28; C9.NW = B8; C9.NE = B9; C9.EA = CT; C9.SE = D9; C9.SW = D8; C9.WE = C8; C9.isEdge = false; entireBoard[28] = C9;
            CT.index = 29; CT.NW = B9; CT.NE = null; CT.EA = null; CT.SE = null; CT.SW = D9; CT.WE = C9; CT.isEdge = true; entireBoard[29] = CT;

            D0.index = 30; D0.NW = C0; D0.NE = C1; D0.EA = D1; D0.SE = E0; D0.SW = null; D0.WE = null; D0.isEdge = true; entireBoard[30] = D0;
            D1.index = 31; D1.NW = C1; D1.NE = C2; D1.EA = D2; D1.SE = E1; D1.SW = E0; D1.WE = D0; D1.isEdge = false; entireBoard[31] = D1;
            D2.index = 32; D2.NW = C2; D2.NE = C3; D2.EA = D3; D2.SE = E2; D2.SW = E1; D2.WE = D1; D2.isEdge = false; entireBoard[32] = D2;
            D3.index = 33; D3.NW = C3; D3.NE = C4; D3.EA = D4; D3.SE = E3; D3.SW = E2; D3.WE = D2; D3.isEdge = false; entireBoard[33] = D3;
            D4.index = 34; D4.NW = C4; D4.NE = C5; D4.EA = D5; D4.SE = E4; D4.SW = E3; D4.WE = D3; D4.isEdge = false; entireBoard[34] = D4;
            D5.index = 35; D5.NW = C5; D5.NE = C6; D5.EA = D6; D5.SE = E5; D5.SW = E4; D5.WE = D4; D5.isEdge = false; entireBoard[35] = D5;
            D6.index = 36; D6.NW = C6; D6.NE = C7; D6.EA = D7; D6.SE = E6; D6.SW = E5; D6.WE = D5; D6.isEdge = false; entireBoard[36] = D6;
            D7.index = 37; D7.NW = C7; D7.NE = C8; D7.EA = D8; D7.SE = E7; D7.SW = E6; D7.WE = D6; D7.isEdge = false; entireBoard[37] = D7;
            D8.index = 38; D8.NW = C8; D8.NE = C9; D8.EA = D9; D8.SE = E8; D8.SW = E7; D8.WE = D7; D8.isEdge = false; entireBoard[38] = D8;
            D9.index = 39; D9.NW = C9; D9.NE = CT; D9.EA = null; D9.SE = null; D9.SW = E8; D9.WE = D8; D9.isEdge = true; entireBoard[39] = D9;

            E0.index = 40; E0.NW = D0; E0.NE = D1; E0.EA = E1; E0.SE = null; E0.SW = null; E0.WE = null; E0.isEdge = true; entireBoard[40] = E0;
            E1.index = 41; E1.NW = D1; E1.NE = D2; E1.EA = E2; E1.SE = null; E1.SW = null; E1.WE = E0; E1.isEdge = true; entireBoard[41] = E1;
            E2.index = 42; E2.NW = D2; E2.NE = D3; E2.EA = E3; E2.SE = null; E2.SW = null; E2.WE = E1; E2.isEdge = true; entireBoard[42] = E2;
            E3.index = 43; E3.NW = D3; E3.NE = D4; E3.EA = E4; E3.SE = null; E3.SW = null; E3.WE = E2; E3.isEdge = true; entireBoard[43] = E3;
            E4.index = 44; E4.NW = D4; E4.NE = D5; E4.EA = E5; E4.SE = null; E4.SW = null; E4.WE = E3; E4.isEdge = true; entireBoard[44] = E4;
            E5.index = 45; E5.NW = D5; E5.NE = D6; E5.EA = E6; E5.SE = null; E5.SW = null; E5.WE = E4; E5.isEdge = true; entireBoard[45] = E5;
            E6.index = 46; E6.NW = D6; E6.NE = D7; E6.EA = E7; E6.SE = null; E6.SW = null; E6.WE = E5; E6.isEdge = true; entireBoard[46] = E6;
            E7.index = 47; E7.NW = D7; E7.NE = D8; E7.EA = E8; E7.SE = null; E7.SW = null; E7.WE = E6; E7.isEdge = true; entireBoard[47] = E7;
            E8.index = 48; E8.NW = D8; E8.NE = D9; E8.EA = null; E8.SE = null; E8.SW = null; E8.WE = E7; E8.isEdge = true; entireBoard[48] = E8;

        }

        public void VisualizeBoard()
        {
            Console.Write("    ");

            for (int i = 0; i < 49; i++)
            {
                List<Piece> thisStack = entireBoard[i].stack;

                if (thisStack.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("(  )");
                }

                else
                {
                    Piece Toppiece = entireBoard[i].TopPiece();
                    bool containsDvonn = thisStack.Any(item => item.pieceType == pieceID.Dvonn);

                    if (Toppiece.pieceType == pieceID.Dvonn)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("(");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("DV");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(")");
                    }
                    if (containsDvonn == false && Toppiece.pieceType == pieceID.White)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("(");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write((thisStack.Count + "W"));
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(")");
                    }
                    if (containsDvonn == false && Toppiece.pieceType == pieceID.Black)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("(");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write((thisStack.Count + "B"));
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(")");
                    }
                    if (containsDvonn == true && Toppiece.pieceType == pieceID.White)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("(");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(thisStack.Count);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("W");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(")");
                    }
                    if (containsDvonn == true && Toppiece.pieceType == pieceID.Black)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("(");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(thisStack.Count);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("B");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(")");
                    }

                }
                int[] newLines = { 8, 18, 29, 39 };

                if (newLines.Contains(i))
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
