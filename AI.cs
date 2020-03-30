using System;
using System.Collections.Generic;

namespace Dvonn_Console
{
    class AI
    {
        Board aiBoard;
        Rules ruleBook;
        PositionTree dvonnTree;

        PieceID playerToMove;
        PieceID tempPlayerToMove;

        private PositionComparator comparator;

        //public int maxDepth = 6;
        //maxendpoints may in reality be considerarbly larger, as the process is only stopped after entire generation branching.
        private int maxEndPoints = 5000;

        List<Node> leaves = new List<Node>();


        public AI()
        {
            aiBoard = new Board();
            aiBoard.InstantiateFields();
            aiBoard.CalculatePrincipalMoves();
            ruleBook = new Rules(aiBoard);

        }

        public Move ComputeAiMove(Position currentPosition, PieceID playerToMove)
        {
            this.playerToMove = playerToMove;
            tempPlayerToMove = playerToMove;

            //comparator = new PositionComparator(playerToMove);

            //var watch_1 = System.Diagnostics.Stopwatch.StartNew();
            //CreateTree(currentPosition, maxEndPoints);
            //watch_1.Stop();
            //var elapsedMs_1 = watch_1.ElapsedMilliseconds;

            //var watch_2 = System.Diagnostics.Stopwatch.StartNew();
            ////PruneBranches();
            //EvaluateEndPoints();
            //watch_2.Stop();
            //var elapsedMs_2 = watch_2.ElapsedMilliseconds;

            //Console.WriteLine();
            //Console.WriteLine("Tree creation took: " + elapsedMs_1 + " milliseconds");
            //Console.WriteLine("Evaluation of endpoints took: " + elapsedMs_2 + " milliseconds");
            //Console.WriteLine("Total calculation time: " + (elapsedMs_1 + elapsedMs_2) + " milliseconds");

            //return PickBestMove();

            CreateTree(currentPosition);
            GetAllLeaves(dvonnTree.root);
            Console.WriteLine("Leave aquisition complete. Leaves count: " + leaves.Count);



            return UseRegularAlgorithm(currentPosition);
        }

        private Move UseRegularAlgorithm(Position currentPosition)
        {
            Move chosenMove = null;
            Position beforePosition = currentPosition;
            aiBoard.ReceivePosition(beforePosition);
            PreMove beforePremove = ruleBook.ManufacturePreMove(playerToMove);


            foreach (Move move in beforePremove.legalMoves)
            {
                Position afterPosition = new Position();
                afterPosition.Copy(beforePosition);
                afterPosition.MakeMove(move);
                aiBoard.ReceivePosition(afterPosition);
                PreMove afterPremove = ruleBook.ManufacturePreMove(playerToMove);

                if (beforePosition.TopPiece(move.target) != playerToMove.ToChar()) move.evaluation += 500;
                if (afterPosition.stacks[move.target].Length == 2) move.evaluation += 500;
                if (afterPosition.stacks[move.target].Length > 3) move.evaluation -= 500;
                if (aiBoard.isDvonnLander(move.target, afterPremove)) move.evaluation += 500;

            }
            int foundEval = 0;
            foreach (Move move in beforePremove.legalMoves)
            {
                if (move.evaluation > foundEval)
                {
                    chosenMove = move;
                    foundEval = move.evaluation;
                }
            }

            return chosenMove;
        }

        private void CreateTree(Position currentPosition)
        {
            dvonnTree = new PositionTree(currentPosition);
            int depthCounter = 0;
            while (true)
            {
                long createNodeCount = BranchEndpoints(depthCounter);
                dvonnTree.depthReach = depthCounter;
                if (dvonnTree.currentEndPoints.Count > maxEndPoints || createNodeCount == 0) break;
                else depthCounter++;
            }

        }

        private long BranchEndpoints(int depthCounter)
        {
            long createNodeCounter = 0L;
            long foundEndPoints = dvonnTree.currentEndPoints.Count;

            Console.WriteLine();
            Console.WriteLine("AI: Branching begun at depth " + depthCounter);
            Console.WriteLine("AI: Color to move: " + tempPlayerToMove.ToString());
            Console.WriteLine("AI: Endpoints found: " + foundEndPoints);

            PositionTree.GenerationAccount thisGenerationAccount = new PositionTree.GenerationAccount(depthCounter);

            for (int i = 0; i < foundEndPoints; i++)
            {
                Node endPoint = dvonnTree.currentEndPoints[i];
                thisGenerationAccount.parentNodes.Add(endPoint);
                aiBoard.ReceivePosition(endPoint.resultingPosition);
                PreMove premove = ruleBook.ManufacturePreMove(tempPlayerToMove);
                endPoint.premove = premove;

                foreach (Move move in premove.legalMoves)
                {
                    Position newPosition = new Position();
                    newPosition.Copy(endPoint.resultingPosition);
                    newPosition.MakeMove(move);

                    //Insert child additionally adds it to current endpoints
                    dvonnTree.InsertChild(new Node(move, newPosition), endPoint);
                    createNodeCounter++;
                }
            }
            //Clean up current endpoints
            foreach (Node parent in thisGenerationAccount.parentNodes)
            {
                dvonnTree.currentEndPoints.Remove(parent);
            }

            thisGenerationAccount.childCount = createNodeCounter;
            dvonnTree.generationAccounts.Add(thisGenerationAccount);

            Console.WriteLine();
            Console.WriteLine("AI: Branching for depth level " + depthCounter + " is complete.");
            Console.WriteLine("AI: added total number of nodes: " + createNodeCounter);
            Console.WriteLine("AI: new number of end points: " + dvonnTree.currentEndPoints.Count);

            tempPlayerToMove = tempPlayerToMove.ToOpposite();

            return createNodeCounter;

        }

        private void GetAllLeaves(Node node)
        {
            if (node.children.Count == 0)
            {
                leaves.Add(node);
            }
            else
            {
                foreach (Node child in node.children)
                {
                    GetAllLeaves(child);
                }
            }

        }



        //Perform alpha beta pruning on lower branches to minimize number of endpoints (i.e. leaves) to be evaluated.
        public void PruneBranches()
        {
            List<Node> badNodes = new List<Node>();

            foreach (Node firstMove in dvonnTree.root.children)
            {
                if (dvonnTree.root.resultingPosition.TopPiece(firstMove.move.target) != firstMove.move.responsibleColor.ToChar()) badNodes.Add(firstMove);
            }

            foreach (Node badNode in badNodes)
            {
                dvonnTree.root.children.Remove(badNode);
            }
        }

        private void EliminateNode(Node badMove)
        {


        }

        public void EvaluateEndPoints()
        {
            long nodeCounter = 0L;
            long skipCounter = 0L;

            //for debug:
            float minimumEvaluation = 0f;
            float maximumEvaluation = 0f;
            bool evaluationSpanInitiated = false;

            bool preParseEndPoints = dvonnTree.currentEndPoints.Count > 20000;

            Console.WriteLine();
            Console.WriteLine("AI: Deep evaluation begun");

            foreach (Node endPoint in dvonnTree.currentEndPoints)
            {
                //skip every other endpoint if many endpoints
                if (preParseEndPoints)
                {
                    if (nodeCounter % 2 == 0)
                    {
                        endPoint.isSkippable = true;
                        skipCounter++;
                        nodeCounter++;
                        continue;
                    }
                }

                //todo: player to move could be the same, if opponent has no legal moves...
                PieceID toMoveColor = endPoint.move.responsibleColor;

                aiBoard.ReceivePosition(endPoint.resultingPosition);
                endPoint.premove = ruleBook.ManufacturePreMove(endPoint.move.responsibleColor);

                float thisEval = comparator.EvaluateImprovement(endPoint);

                //for debug:
                if (evaluationSpanInitiated == false)
                {
                    maximumEvaluation = thisEval;
                    minimumEvaluation = thisEval;
                    evaluationSpanInitiated = true;
                }
                else
                {
                    if (thisEval > maximumEvaluation) maximumEvaluation = thisEval;
                    if (thisEval < minimumEvaluation) minimumEvaluation = thisEval;
                }

                endPoint.evaluation = thisEval;
                nodeCounter++;

            }

            Console.WriteLine();
            Console.WriteLine("AI: Deep evaluation complete.");
            Console.WriteLine("AI: Evaluated " + nodeCounter + " positions");
            Console.WriteLine("AI: Skipped " + skipCounter + " positions");
            Console.WriteLine("AI: Minimum evaluation value found: " + minimumEvaluation);
            Console.WriteLine("AI: Maximum evaluation value found: " + maximumEvaluation);

        }

        //Finds best move in children of gen 0 (i.e. upcoming AI move) by analyzing PositionTree using
        //minimax algorithm : https://en.wikipedia.org/wiki/Minimax
        public Move PickBestMove()
        {
            dvonnTree.generationAccounts.Reverse();
            Console.WriteLine();
            Console.WriteLine("AI: Minimax algorithm initiated");

            foreach (PositionTree.GenerationAccount account in dvonnTree.generationAccounts)
            {
                bool isMaximumGeneration = account.generationNumber % 2 == 0;

                foreach (Node parent in account.parentNodes)
                {
                    parent.evaluation = FindValueAmongstChildren(parent, isMaximumGeneration);

                }
                string minMax = "";
                if (isMaximumGeneration) minMax = "maximum";
                else minMax = "minimum";
                Console.WriteLine("AI: search for " + minMax + " values in generation " + account.generationNumber + " accomplished succesfully.");

            }

            Move bestMove = dvonnTree.root.children.Find(child => child.evaluation == dvonnTree.root.evaluation).move;
            Console.WriteLine();
            Console.WriteLine("AI: The best AI move for color: " + playerToMove + " is found.");
            Console.WriteLine("AI: It has an evaluation of: " + dvonnTree.root.evaluation);
            Console.WriteLine("AI: The move is: " + bestMove.ToString());

            return bestMove;

        }

        private float FindValueAmongstChildren(Node parent, bool isMaximum)
        {
            float searchedValue = 0f;

            foreach (Node child in parent.children)
            {
                if (child.isSkippable == true) continue;

                bool valueSpanInitiated = false;

                if (valueSpanInitiated == false)
                {
                    searchedValue = child.evaluation;
                    valueSpanInitiated = true;
                }
                else
                {
                    if (isMaximum)
                    {
                        if (child.evaluation > searchedValue) searchedValue = child.evaluation;
                    }
                    else
                    {
                        if (child.evaluation < searchedValue) searchedValue = child.evaluation;
                    }
                }
            }
            return searchedValue;

        }

    }
}
