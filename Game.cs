using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class Game
    {

        public Game(int dvonnCount, int whiteCount, int blackCount)
        {
            CreateStacks();
            RandomPopulate(dvonnCount, whiteCount, blackCount);
          

        }
        public void CreateStacks()
        {
            Stack A0 = new Stack(); Stack A1 = new Stack(); Stack A2 = new Stack(); Stack A3 = new Stack(); Stack A4 = new Stack(); Stack A5 = new Stack(); Stack A6 = new Stack(); Stack A7 = new Stack(); Stack A8 = new Stack();
            Stack B0 = new Stack(); Stack B1 = new Stack(); Stack B2 = new Stack(); Stack B3 = new Stack(); Stack B4 = new Stack(); Stack B5 = new Stack(); Stack B6 = new Stack(); Stack B7 = new Stack(); Stack B8 = new Stack(); Stack B9 = new Stack();
            Stack C0 = new Stack(); Stack C1 = new Stack(); Stack C2 = new Stack(); Stack C3 = new Stack(); Stack C4 = new Stack(); Stack C5 = new Stack(); Stack C6 = new Stack(); Stack C7 = new Stack(); Stack C8 = new Stack(); Stack C9 = new Stack(); Stack CT = new Stack();
            Stack D0 = new Stack(); Stack D1 = new Stack(); Stack D2 = new Stack(); Stack D3 = new Stack(); Stack D4 = new Stack(); Stack D5 = new Stack(); Stack D6 = new Stack(); Stack D7 = new Stack(); Stack D8 = new Stack(); Stack D9 = new Stack();
            Stack E0 = new Stack(); Stack E1 = new Stack(); Stack E2 = new Stack(); Stack E3 = new Stack(); Stack E4 = new Stack(); Stack E5 = new Stack(); Stack E6 = new Stack(); Stack E7 = new Stack(); Stack E8 = new Stack();

            //Her defineres hvilke nabofelter (stacks) hvert felt har. Hver stack får desuden et fieldID, og en angivelse af om den er et kant-felt. Endelig indsættes stack'en i array'en AllStacks.
            A0.NW = null; A0.NE = null; A0.EA = A1; A0.SE = B1; A0.SW = B0; A0.WE = null; A0.IsEdge = true;
            A1.NW = null; A1.NE = null; A1.EA = A2; A1.SE = B2; A1.SW = B1; A1.WE = A0; A1.IsEdge = true;
            A2.NW = null; A2.NE = null; A2.EA = A3; A2.SE = B3; A2.SW = B2; A2.WE = A1; A2.IsEdge = true;
            A3.NW = null; A3.NE = null; A3.EA = A4; A3.SE = B4; A3.SW = B3; A3.WE = A2; A3.IsEdge = true;
            A4.NW = null; A4.NE = null; A4.EA = A5; A4.SE = B5; A4.SW = B4; A4.WE = A3; A4.IsEdge = true;
            A5.NW = null; A5.NE = null; A5.EA = A6; A5.SE = B6; A5.SW = B5; A5.WE = A4; A5.IsEdge = true;
            A6.NW = null; A6.NE = null; A6.EA = A7; A6.SE = B7; A6.SW = B6; A6.WE = A5; A6.IsEdge = true;
            A7.NW = null; A7.NE = null; A7.EA = A8; A7.SE = B8; A7.SW = B7; A7.WE = A6; A7.IsEdge = true;
            A8.NW = null; A8.NE = null; A8.EA = null; A8.SE = B9; A8.SW = B8; A8.WE = A7; A8.IsEdge = true;

            B0.NW = null; B0.NE = A0; B0.EA = B1; B0.SE = C1; B0.SW = C0; B0.WE = null; B0.IsEdge = true;
            B1.NW = A0; B1.NE = A1; B1.EA = B2; B1.SE = C2; B1.SW = C1; B1.WE = B0; B1.IsEdge = false;
            B2.NW = A1; B2.NE = A2; B2.EA = B3; B2.SE = C3; B2.SW = C2; B2.WE = B1; B2.IsEdge = false;
            B3.NW = A2; B3.NE = A3; B3.EA = B4; B3.SE = C4; B3.SW = C3; B3.WE = B2; B3.IsEdge = false;
            B4.NW = A3; B4.NE = A4; B4.EA = B5; B4.SE = C5; B4.SW = C4; B4.WE = B3; B4.IsEdge = false;
            B5.NW = A4; B5.NE = A5; B5.EA = B6; B5.SE = C6; B5.SW = C5; B5.WE = B4; B5.IsEdge = false;
            B6.NW = A5; B6.NE = A6; B6.EA = B7; B6.SE = C7; B6.SW = C6; B6.WE = B5; B6.IsEdge = false;
            B7.NW = A6; B7.NE = A7; B7.EA = B8; B7.SE = C8; B7.SW = C7; B7.WE = B6; B7.IsEdge = false;
            B8.NW = A7; B8.NE = A8; B8.EA = B9; B8.SE = C9; B8.SW = C8; B8.WE = B7; B8.IsEdge = false;
            B9.NW = A8; B9.NE = null; B9.EA = null; B9.SE = CT; B9.SW = C9; B9.WE = B8; B9.IsEdge = true;

            C0.NW = null; C0.NE = B0; C0.EA = C1; C0.SE = D0; C0.SW = null; C0.WE = null; C0.IsEdge = true;
            C1.NW = B0; C1.NE = B1; C1.EA = C2; C1.SE = D1; C1.SW = D0; C1.WE = C0; C1.IsEdge = false;
            C2.NW = B1; C2.NE = B2; C2.EA = C3; C2.SE = D2; C2.SW = D1; C2.WE = C1; C2.IsEdge = false;
            C3.NW = B2; C3.NE = B3; C3.EA = C4; C3.SE = D3; C3.SW = D2; C3.WE = C2; C3.IsEdge = false;
            C4.NW = B3; C4.NE = B4; C4.EA = C5; C4.SE = D4; C4.SW = D3; C4.WE = C3; C4.IsEdge = false;
            C5.NW = B4; C5.NE = B5; C5.EA = C6; C5.SE = D5; C5.SW = D4; C5.WE = C4; C5.IsEdge = false;
            C6.NW = B5; C6.NE = B6; C6.EA = C7; C6.SE = D6; C6.SW = D5; C6.WE = C5; C6.IsEdge = false;
            C7.NW = B6; C7.NE = B7; C7.EA = C8; C7.SE = D7; C7.SW = D6; C7.WE = C6; C7.IsEdge = false;
            C8.NW = B7; C8.NE = B8; C8.EA = C9; C8.SE = D8; C8.SW = D7; C8.WE = C7; C8.IsEdge = false;
            C9.NW = B8; C9.NE = B9; C9.EA = CT; C9.SE = D9; C9.SW = D8; C9.WE = C8; C9.IsEdge = false;
            CT.NW = B9; CT.NE = null; CT.EA = null; CT.SE = null; CT.SW = D9; CT.WE = C9; CT.IsEdge = true;

            D0.NW = C0; D0.NE = C1; D0.EA = D1; D0.SE = E0; D0.SW = null; D0.WE = null; D0.IsEdge = true;
            D1.NW = C1; D1.NE = C2; D1.EA = D2; D1.SE = E1; D1.SW = E0; D1.WE = D0; D1.IsEdge = false;
            D2.NW = C2; D2.NE = C3; D2.EA = D3; D2.SE = E2; D2.SW = E1; D2.WE = D1; D2.IsEdge = false;
            D3.NW = C3; D3.NE = C4; D3.EA = D4; D3.SE = E3; D3.SW = E2; D3.WE = D2; D3.IsEdge = false;
            D4.NW = C4; D4.NE = C5; D4.EA = D5; D4.SE = E4; D4.SW = E3; D4.WE = D3; D4.IsEdge = false;
            D5.NW = C5; D5.NE = C6; D5.EA = D6; D5.SE = E5; D5.SW = E4; D5.WE = D4; D5.IsEdge = false;
            D6.NW = C6; D6.NE = C7; D6.EA = D7; D6.SE = E6; D6.SW = E5; D6.WE = D5; D6.IsEdge = false;
            D7.NW = C7; D7.NE = C8; D7.EA = D8; D7.SE = E7; D7.SW = E6; D7.WE = D6; D7.IsEdge = false;
            D8.NW = C8; D8.NE = C9; D8.EA = D9; D8.SE = E8; D8.SW = E7; D8.WE = D7; D8.IsEdge = false;
            D9.NW = C9; D9.NE = CT; D9.EA = null; D9.SE = null; D9.SW = E8; D9.WE = D8; D9.IsEdge = true;

            E0.NW = D0; E0.NE = D1; E0.EA = E1; E0.SE = null; E0.SW = null; E0.WE = null; E0.IsEdge = true;
            E1.NW = D1; E1.NE = D2; E1.EA = E2; E1.SE = null; E1.SW = null; E1.WE = E0; E1.IsEdge = true;
            E2.NW = D2; E2.NE = D3; E2.EA = E3; E2.SE = null; E2.SW = null; E2.WE = E1; E2.IsEdge = true;
            E3.NW = D3; E3.NE = D4; E3.EA = E4; E3.SE = null; E3.SW = null; E3.WE = E2; E3.IsEdge = true;
            E4.NW = D4; E4.NE = D5; E4.EA = E5; E4.SE = null; E4.SW = null; E4.WE = E3; E4.IsEdge = true;
            E5.NW = D5; E5.NE = D6; E5.EA = E6; E5.SE = null; E5.SW = null; E5.WE = E4; E5.IsEdge = true;
            E6.NW = D6; E6.NE = D7; E6.EA = E7; E6.SE = null; E6.SW = null; E6.WE = E5; E6.IsEdge = true;
            E7.NW = D7; E7.NE = D8; E7.EA = E8; E7.SE = null; E7.SW = null; E7.WE = E6; E7.IsEdge = true;
            E8.NW = D8; E8.NE = D9; E8.EA = null; E8.SE = null; E8.SW = null; E8.WE = E7; E8.IsEdge = true;

        }
        public void RandomPopulate(int dvonnCount, int whiteCount, int blackCount)
        {
            Random rGen = new Random();

            // For hver brik-type køres nu et for-loop, der fordeler brikkerne på tomme tilfældigt valgte felter.
            for (int i = 0; i < dvonnCount; i++)
            {
                int rNum = rGen.Next(0, 49);
                if (Stack.AllStacks[rNum].PieceList.Count == 0) // hvis det tilfældigt valgte felt er tomt, skabes et brik-objekt, som tildeles den pågældende stacks piecelist.
                {
                    Piece Dvonn = new Piece(pieceID.Dvonn);
                    Stack.AllStacks[rNum].PieceList.Add(Dvonn);
                }
                else dvonnCount = dvonnCount + 1; //hvis det tilfældigt valgte felt ikke er tomt, køres loopet igen, og dvonnCount tillægges et trin mere, for at sikre, at der til sidst vil være det korrekte antal brikker af den pågældende briktype, her 3 stk dvonn brikker...
            }
            for (int i = 0; i < whiteCount; i++)
            {
                int rNum = rGen.Next(0, 49);
                if (Stack.AllStacks[rNum].PieceList.Count == 0)
                {
                    Piece White = new Piece(pieceID.White);
                    Stack.AllStacks[rNum].PieceList.Add(White);
                }
                else whiteCount = whiteCount + 1;
            }
            for (int i = 0; i < blackCount; i++)
            {
                int rNum = rGen.Next(0, 49);
                if (Stack.AllStacks[rNum].PieceList.Count == 0)
                {
                    Piece Black = new Piece(pieceID.Black);
                    Stack.AllStacks[rNum].PieceList.Add(Black);
                }
                else blackCount = blackCount + 1;
            }
        }

    }

}
