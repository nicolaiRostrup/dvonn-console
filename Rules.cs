using System;
using System.Collections.Generic;
using System.Linq;


namespace Dvonn_Console
{
    class Rules
    {
        private Writer typeWriter = new Writer();
        private Board dvonnBoard;

        public Rules(Board dvonnBoard)
        {
            this.dvonnBoard = dvonnBoard;
        }

        public List<int> FindLegalTargets(int fieldID)
        {
            List<int> foundLegalTargets = new List<int>();

            int pieceCount = dvonnBoard.entireBoard[fieldID].stack.Count;
            if (pieceCount == 0) return foundLegalTargets;
            else
            {
                List<int> principalTargets = FindNotEmptyStacks();
                principalTargets.Remove(fieldID);
                foreach (int targetID in principalTargets)
                {
                    if (dvonnBoard.allPrincipalMoves.ContainsKey(Tuple.Create(fieldID, targetID, pieceCount)))
                    {
                        foundLegalTargets.Add(targetID);
                    }

                }

            }
            return foundLegalTargets;
        }

        public List<Move> FindLegalMoves(PieceID color)
        {
            List<Move> legalMoves = new List<Move>();
            List<int> notEmptyStacks = FindNotEmptyStacks();
            List<int> sources = LegalSources(color);

            foreach (int sourceID in sources)
            {
                int pieceCount = dvonnBoard.entireBoard[sourceID].stack.Count;

                foreach (int targetID in notEmptyStacks)
                {
                    if (targetID == sourceID) continue;
                    if (dvonnBoard.allPrincipalMoves.ContainsKey(Tuple.Create(sourceID, targetID, pieceCount)))
                    {
                        legalMoves.Add(new Move(sourceID, targetID, color));
                    }

                }

            }
            return legalMoves;
        }

        public List<int> FindNotEmptyStacks()
        {
            List<int> principalTargets = new List<int>();

            for (int i = 0; i < 49; i++)
            {
                if (dvonnBoard.entireBoard[i].stack.Count > 0)
                {
                    principalTargets.Add(i);
                }

            }
            return principalTargets;
        }

        private List<int> LegalSources(PieceID colorToMove)
        {
            List<int> notEmptyStacks = FindNotEmptyStacks();
            List<int> legalSources = new List<int>();

            foreach (int fieldID in notEmptyStacks)
            {
                if (dvonnBoard.entireBoard[fieldID].TopPiece().pieceType != colorToMove) continue;
                else if (EnclosureCondition(fieldID) == true) continue;
                else legalSources.Add(fieldID);

            }
            return legalSources;
        }

        public List<int> ExtractSources(List<Move> moveList)
        {
            List<int> trueLegalSources = new List<int>();

            foreach (Move move in moveList)
            {
                if (!trueLegalSources.Contains(move.source))
                {
                    trueLegalSources.Add(move.source);
                }
            }
            return trueLegalSources;
        }

        public List<int> ExtractTargets(List<Move> moveList)
        {
            List<int> trueLegalTargets = new List<int>();

            foreach (Move move in moveList)
            {
                if (!trueLegalTargets.Contains(move.target))
                {
                    trueLegalTargets.Add(move.target);
                }
            }
            return trueLegalTargets;
        }


        public bool EnclosureCondition(int fieldID)
        {
            // kant-felter kan ikke være "enclosed"
            if (dvonnBoard.entireBoard[fieldID].isEdge == true) return false;

            foreach (Field field in dvonnBoard.entireBoard[fieldID].GetNeighbours())
            {
                if (field.stack.Count == 0)
                {
                    return false;
                }

            }
            return true;
        }

        public int GetScore(PieceID color)
        {
            int scoreCounter = 0;
            for (int i = 0; i < 49; i++)
            {
                Field chosenField = dvonnBoard.entireBoard[i];
                if (chosenField.stack.Count == 0) continue;
                if (chosenField.TopPiece().pieceType == color) scoreCounter += chosenField.stack.Count;

            }
            return scoreCounter;
        }

        public void CheckDvonnCollapse(Move effectiveMove, bool writeText)
        {
            if (writeText == false)
            {
                int[] result = RemoveUnheldStacks(FindHeldStacks());
                if (result[0] > 0 && result[1] > 0)

                    if (effectiveMove != null)
                    {
                        effectiveMove.isCollapseMove = true;
                        effectiveMove.collapsedTowers = result[0];
                    }
            }
            else
            {
                int[] result = RemoveUnheldStacks(FindHeldStacks());
                if (result[0] > 0 && result[1] > 0)
                {
                    typeWriter.DvonnCollapseText(result);
                    if (effectiveMove != null)
                    {
                        effectiveMove.isCollapseMove = true;
                        effectiveMove.collapsedTowers = result[0];
                    }
                    dvonnBoard.VisualizeBoard();
                }

            }

        }


        public List<Field> FindHeldStacks()
        {
            List<Field> heldFields = new List<Field>();

            // gets the Fields that contains dvonn pieces and makes them held
            foreach (Field field in dvonnBoard.entireBoard)
            {
                if (field.stack.Any(p => p.pieceType == PieceID.Dvonn))
                {
                    heldFields.Add(field);
                }
            }

            //then iteratively add neighbour fields if they touch a dvonn field (or touch a field that touches a dvonn field),
            //untill no fields are added in one do-while cycle.
            int counter;
            do
            {
                counter = 0;

                for (int i = 0; i < heldFields.Count; i++)
                {
                    List<Field> neighbours = heldFields[i].GetNeighbours();

                    for (int j = 0; j < neighbours.Count; j++)
                    {
                        if (neighbours[j].stack.Count > 0 && !heldFields.Contains(neighbours[j]))
                        {
                            heldFields.Add(neighbours[j]);
                            counter++;
                        }
                    }
                }
            }
            while (counter > 0);

            return heldFields;
        }

        public int[] RemoveUnheldStacks(List<Field> heldFields)
        {
            int fieldCounter = 0;
            int pieceCounter = 0;

            foreach (Field field in dvonnBoard.entireBoard)
            {
                if (heldFields.Contains(field)) continue;
                else if (field.stack.Count == 0) continue;
                else
                {
                    pieceCounter += field.stack.Count;
                    field.stack.Clear();
                    fieldCounter++;
                }

            }
            int[] result = new int[2];
            result[0] = fieldCounter;
            result[1] = pieceCounter;

            return result;
        }

    }
}

