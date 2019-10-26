﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class Rules
    {

        Writer typeWriter = new Writer();

        Board dvonnBoard;

        public Rules(Board dvonnBoard)
        {
            this.dvonnBoard = dvonnBoard;
        }

        public List<int> LegalSources(pieceID Color)
        {
            List<int> legalSources = new List<int>();

            for (int i = 0; i < 49; i++)
            {
                if (dvonnBoard.entireBoard[i].stack.Count > 0 && EnclosureCondition(i) == false && dvonnBoard.entireBoard[i].TopPiece().pieceType == Color)
                {
                    legalSources.Add(i);
                }

            }
            return legalSources;
        }

        public List<int> LegalTargets(int fieldID, int pieceCount)
        {
            List<int> legalTargets = new List<int>();
            // For all tupples in AllPrincipalMoves...
            for (int i = 0; i < 786; i++)
            {
                if (dvonnBoard.allPrincipalMoves[i].Item1 == fieldID && dvonnBoard.entireBoard[dvonnBoard.allPrincipalMoves[i].Item2].stack.Count > 0 && dvonnBoard.allPrincipalMoves[i].Item3 == pieceCount)
                {
                    legalTargets.Add(dvonnBoard.allPrincipalMoves[i].Item2);
                }

            }
            return legalTargets;
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

        public int[] Score()
        {
            int whiteScore = 0;
            int blackScore = 0;
            int[] score = { 0, 0, 0 };

            for (int i = 0; i < 49; i++)
            {
                Field chosenField = dvonnBoard.entireBoard[i];

                if (chosenField.stack.Count == 0) continue;
                if (chosenField.TopPiece().pieceType == pieceID.White) whiteScore += chosenField.stack.Count;
                if (chosenField.TopPiece().pieceType == pieceID.Black) blackScore += chosenField.stack.Count;
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
                if (field.stack.Any(p => p.pieceType == pieceID.Dvonn))
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

        public bool GameEndCondition()
        {
            if (LegalMoves(pieceID.White) == 0 && LegalMoves(pieceID.Black) == 0) return true;
            else return false;
        }
        public bool PassCondition(pieceID Color)
        {
            if (LegalMoves(Color) == 0) return true;
            else return false;
        }

        public int LegalMoves(pieceID Color)
        {
            int legalMoves = 0;

            foreach (int fieldID in LegalSources(Color)) 
            {
                legalMoves = legalMoves + LegalTargets(fieldID, dvonnBoard.entireBoard[fieldID].stack.Count).Count; // this line counts all legal targets for current selected position on the board...

            }
            return legalMoves; 
        }

        


    }





}
