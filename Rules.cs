using System;
using System.Collections.Generic;
using System.Linq;


namespace Dvonn_Console
{
    class Rules
    {
        Writer typeWriter = new Writer();

        public Board dvonnBoard;

        public Rules(Board dvonnBoard)
        {
            this.dvonnBoard = dvonnBoard;
        }

        public List<int> GetLegalTargets(int fieldID)
        {
            List<int> foundLegalTargets = new List<int>();

            int pieceCount = dvonnBoard.entireBoard[fieldID].stack.Count;
            if (pieceCount == 0) return foundLegalTargets;
            else
            {
                List<int> principalTargets = FindNotEmptyStacks();
                principalTargets.Remove(fieldID);
                foreach(int targetID in principalTargets)
                {
                    if (dvonnBoard.allPrincipalMoves.ContainsKey(Tuple.Create(fieldID, targetID, pieceCount)))
                    {
                        foundLegalTargets.Add( targetID);
                    }

                }

            }
            return foundLegalTargets;
        }

        public PreMove ManufacturePreMove(PieceID color)
        {
            PreMove premove = new PreMove(color);
            List<int> notEmptyStacks = FindNotEmptyStacks();
            List<int> legalSources = LegalSources(color, notEmptyStacks);
            
            premove.legalMoves = FindLegalMoves(legalSources, color, notEmptyStacks);
            premove.trueLegalSources = GetTrueLegalSources(premove.legalMoves);
            premove.trueLegalTargets = GetTrueLegalTargets(premove.legalMoves);

            return premove;

        }

        private List<Move> FindLegalMoves(List<int> sources, PieceID color, List<int> notEmptyStacks)
        {
            List<Move> legalMoves = new List<Move>();

            foreach (int sourceID in sources)
            {
                int pieceCount = dvonnBoard.entireBoard[sourceID].stack.Count;
                List<int> principalTargets = notEmptyStacks;
                principalTargets.Remove(sourceID);

                foreach(int targetID in principalTargets)
                {
                    if (dvonnBoard.allPrincipalMoves.ContainsKey(Tuple.Create(sourceID, targetID, pieceCount)))
                    {
                        legalMoves.Add(new Move(sourceID, targetID, color));
                    }

                }

            }
            return legalMoves;
        }

        private List<int> FindNotEmptyStacks()
        {
            List<int> principalTargets = new List<int>();

            for (int i = 0; i < 49; i++)
            {
                if (dvonnBoard.entireBoard[i].stack.Count > 0 )
                {
                    principalTargets.Add(i);
                }

            }
            return principalTargets;
        }

        private List<int> LegalSources(PieceID colorToMove, List<int> notEmptyStacks)
        {
            List<int> legalSources = new List<int>();

            foreach(int fieldID in notEmptyStacks)
            {   
                if (dvonnBoard.entireBoard[fieldID].TopPiece().pieceType != colorToMove) continue;
                else if (EnclosureCondition(fieldID) == true) continue;
                else legalSources.Add(fieldID);

            }
            return legalSources;
        }

        private List<int> GetTrueLegalSources(List<Move> moveList)
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

        private List<int> GetTrueLegalTargets(List<Move> moveList)
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


        private bool EnclosureCondition(int fieldID)
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

        public int[] Score()
        {
            int whiteScore = 0;
            int blackScore = 0;
            int[] score = { 0, 0, 0 };

            for (int i = 0; i < 49; i++)
            {
                Field chosenField = dvonnBoard.entireBoard[i];

                if (chosenField.stack.Count == 0) continue;
                if (chosenField.TopPiece().pieceType == PieceID.White) whiteScore += chosenField.stack.Count;
                if (chosenField.TopPiece().pieceType == PieceID.Black) blackScore += chosenField.stack.Count;
            }

            score[0] = whiteScore;
            score[1] = blackScore;

            // the third integer in the array is a code for the game result
            if (whiteScore > blackScore) score[2] = 0;
            if (whiteScore < blackScore) score[2] = 1;
            if (whiteScore == blackScore) score[2] = 2;

            return score;
        }

        public void CheckDvonnCollapse()
        {
            int[] result = RemoveUnheldStacks(FindHeldStacks());
            if (result[0] > 0 && result[1] > 0)
            {
                typeWriter.DvonnCollapseText(result);
                dvonnBoard.VisualizeBoard();
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

        public bool GameEndCondition(PreMove premoveWhite, PreMove premoveBlack)
        {
            if (premoveWhite.trueLegalSources.Count == 0 && premoveBlack.trueLegalSources.Count == 0) return true;
            else return false;
        }

        public bool PassCondition(PreMove premove)
        {
            if (premove.trueLegalSources.Count == 0) return true;
            else return false;
        }


    }
}

