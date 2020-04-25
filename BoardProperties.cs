using System;
using System.Collections.Generic;


namespace Dvonn_Console
{
    public static class BoardProperties
    {
        //AllPrincipalMoves.Count=786
        //Item1 = source, Item2 = target, Item 3 = jumps (number of pieces in source stack).
        public static Dictionary<Tuple<int, int, int>, int> allPrincipalMoves = new Dictionary<Tuple<int, int, int>, int>();

        public static List<directionID> allDirections = new List<directionID> { directionID.NE, directionID.EA, directionID.SE, directionID.SW, directionID.WE, directionID.NW };

        public static string[] fieldCoordinates = { "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CT", "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8" };

        public static int[] edgeFields = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 18, 19, 29, 30, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48 };

        public static void CalculatePrincipalMoves(Board board)
        {
            List<directionID> allDirections = new List<directionID> { directionID.NE, directionID.EA, directionID.SE, directionID.SW, directionID.WE, directionID.NW };

            int principalMoveCounter = 1;
            // For all fields on the board...
            for (int i = 0; i < 49; i++)
            {
                // find all legal moves in any direction...
                foreach (directionID direction in allDirections)
                {
                    Field sourceField = board.entireBoard[i];

                    for (int jump = 1; jump < 11; jump++) // Max jumps is ten, the move: c0/ct.
                    {
                        Field nextField = sourceField.NextField(direction);

                        if (nextField == null)
                        {
                            break;
                        }
                        else
                        {
                            allPrincipalMoves.Add(Tuple.Create(i, nextField.index, jump), principalMoveCounter);
                            sourceField = nextField;
                            principalMoveCounter++;
                        }

                    }
                }
            }

        }
    }
}
