﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class AI
    {
        Board aiBoard;
        Rules ruleBook;
        PositionTree dvonnTree;
        int nodeCounter = 0;
        PieceID currentPlayer;

        public AI()
        {
            aiBoard = new Board();
            aiBoard.InstantiateFields();
            aiBoard.CalculatePrincipalMoves();
            ruleBook = new Rules(aiBoard);
        }

        public void CreateTree(Position currentPosition, int maxDepth)
        {

            dvonnTree = new PositionTree(currentPosition);

            //computer allways has black, so branching begins with black...
            currentPlayer = PieceID.Black;

            for (int i = 0; i < maxDepth; i++)
            {
                Console.Write("AI: Branching begun at depth " + (i + 1) + "\n");
                BranchAllEndpoints(currentPlayer, i + 1);

            }


        }

        void BranchAllEndpoints(PieceID player, int depth)
        {
            List<Node> formerEndPoints = new List<Node>();
            nodeCounter = 0;

            Console.Write("AI: endpoints found: " + dvonnTree.currentEndPoints.Count + "\n");

            foreach (Node endPoint in dvonnTree.currentEndPoints)
            {
                aiBoard.ReceivePosition(endPoint.position);
                List<int> legalSources = ruleBook.LegalSources(player);

                int comboCounter = 0;

                foreach (int sourceIndex in legalSources)
                {
                    comboCounter++;
                    if (comboCounter > 2) break;

                    int sourceStackCount = aiBoard.entireBoard[sourceIndex].stack.Count;
                    List<int> legalTargets = ruleBook.LegalTargets(sourceIndex, sourceStackCount);

                    if (nodeCounter == 0 || nodeCounter % 1000 == 0) Console.WriteLine("Possible moves count for generation " + depth + " sample = " + legalSources.Count * legalTargets.Count);

                    int localCounter = 0;
                    foreach (int targetIndex in legalTargets)
                    {
                        localCounter++;

                        if (localCounter > 2) break;

                        int[] moveCombo = { sourceIndex, targetIndex };
                        Position newPosition = aiBoard.SendPosition();
                        newPosition.MakeMove(moveCombo);

                        dvonnTree.InsertChild(new Node(newPosition), endPoint);
                        nodeCounter++;


                    }

                }
                formerEndPoints.Add(endPoint);

            }

            foreach (Node endPoint in formerEndPoints) dvonnTree.FinishBranching(endPoint);

            Console.Write("AI: added " + nodeCounter + " positions to position tree for color " + player + "\n");
            Console.Write("AI: new number of endpoints in tree = " + dvonnTree.currentEndPoints.Count + "\n");
            Console.Write("AI: total number of nodes in tree = " + dvonnTree.totalNodes + "\n");
            nodeCounter = 0;
            AlternatePlayer();

        }

        public string PrintTree(int maxNodes)
        {
            return dvonnTree.DisplayTree(maxNodes);
        }

        void AlternatePlayer()
        {
            if (currentPlayer == PieceID.Black) currentPlayer = PieceID.White;
            else currentPlayer = PieceID.Black;

        }

        public float EvaluatePosition(Position position)
        {
            /*Should count as POSITIVE:
            Short distance to dvonn piece.
            Number of controlled stacks.
            Low number of opponent controlled stacks.
            Number of possible moves.
            Low number of opponent possible moves.

            Number of movable stacks
            Low number of opponent movable stacks
            High score/low score for opponent

            If a piece can land on top of dvonn piece or tower with dvonn piece.
            + Number of pieces that can do that.

            (In the endgame: to isolate a large stack under your control).
            Capture pieces that are about to capture important stacks.

            Moving a piece towards a dvonn piece.

            In the early phase build low. One own color piece on top of one enemy piece.
            */

            /*Should count as NEGATIVE:
            To build high towers

            */

            /*
            PiecesToHitDvonn(pieceColor); regardless of its a legal move and regardless of the dvonn piece being somehwere within a tower.

            ControlledStacks(pieceColor); has color on top, regardless of its a legal source

            MovableStacks(pieceColor); = legal sources

            PossibleMoves(pieceColor); legalsources * legaltargets...

            Score(pieceColor)
            
            DistanceToDvonnpiece() return int - the smaller the better

            bool isOwnPieceOnOpponentPiece




            */





            return 0f;

        }

    }
}
