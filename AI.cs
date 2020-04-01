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

            //CreateTree(currentPosition);
            CreateTestTree();
            AssignValuesToLeaves();
            Console.WriteLine(dvonnTree.ToString());
            WaitForUser();


            //Console.Clear();
            //aiBoard.VisualizeBoard();
            //PerformMiniMax();
            //Console.WriteLine(dvonnTree.ToString());
            //WaitForUser();


            Console.Clear();
            aiBoard.VisualizeBoard();
            AlphaBetaPruning();
            Console.WriteLine(dvonnTree.ToString());
            WaitForUser();

            Console.Clear();
            aiBoard.VisualizeBoard();
            PerformMiniMax();
            Console.WriteLine(dvonnTree.ToString());
            WaitForUser();


            return UseRegularAlgorithm(currentPosition);
        }



        private Move UseRegularAlgorithm(Position currentPosition)
        {
            Move chosenMove = null;
            Position beforePosition = currentPosition;
            aiBoard.ReceivePosition(beforePosition);
            PreMove beforeOptions = ruleBook.ManufacturePreMove(playerToMove); //the options for moving color before any move has been chosen.


            foreach (Move move in beforeOptions.legalMoves)
            {
                Position afterPosition = new Position();
                afterPosition.Copy(beforePosition);
                afterPosition.MakeMove(move);
                aiBoard.ReceivePosition(afterPosition);
                PreMove afterOptions = ruleBook.ManufacturePreMove(playerToMove); //the options for moving color after considered move has been made

                if (beforePosition.TopPiece(move.target) != playerToMove.ToChar()) move.evaluation += 500;
                if (afterPosition.stacks[move.target].Length == 2) move.evaluation += 500;
                if (afterPosition.stacks[move.target].Length > 3) move.evaluation -= 500;
                if (aiBoard.isDvonnLander(move.target, afterOptions)) move.evaluation += 500;

            }
            int foundEval = 0;
            foreach (Move move in beforeOptions.legalMoves)
            {
                if (move.evaluation > foundEval)
                {
                    chosenMove = move;
                    foundEval = move.evaluation;
                }
            }

            return chosenMove;
        }

        //private void CreateTree(Position currentPosition)
        //{
        //    dvonnTree = new PositionTree(currentPosition);
        //    int depthCounter = 0;
        //    while (true)
        //    {
        //        long createNodeCount = BranchEndpoints(depthCounter);
        //        dvonnTree.depthReach = depthCounter;
        //        if (dvonnTree.currentEndPoints.Count > maxEndPoints || createNodeCount == 0) break;
        //        else depthCounter++;
        //    }

        //}

        private void CreateTestTree()
        {
            dvonnTree = new PositionTree();
            Node a = new Node();
            Node b = new Node();
            Node c = new Node();
            Node d = new Node();
            Node e = new Node();
            Node f = new Node();
            Node g = new Node();
            Node h = new Node();
            Node i = new Node();
            Node j = new Node();
            Node k = new Node();
            Node l = new Node();
            Node m = new Node();
            Node n = new Node();
            Node o = new Node();
            Node p = new Node();
            Node q = new Node();
            Node r = new Node();
            Node s = new Node();
            
            dvonnTree.root = a;
            dvonnTree.InsertChild(b, a);
            dvonnTree.InsertChild(c, a);
            dvonnTree.InsertChild(d, a);

            dvonnTree.InsertChild(e, b);
            dvonnTree.InsertChild(f, b);

            dvonnTree.InsertChild(g, c);
            dvonnTree.InsertChild(h, c);

            dvonnTree.InsertChild(i, d);
            dvonnTree.InsertChild(j, d);

            dvonnTree.InsertChild(k, e);
            dvonnTree.InsertChild(l, e);

            dvonnTree.InsertChild(m, f);

            dvonnTree.InsertChild(n, g);
            dvonnTree.InsertChild(o, g);

            dvonnTree.InsertChild(p, h);

            dvonnTree.InsertChild(q, i);

            dvonnTree.InsertChild(r, j);
            dvonnTree.InsertChild(s, j);

            dvonnTree.InsertChild(new Node(), k);
            dvonnTree.InsertChild(new Node(), k);

            dvonnTree.InsertChild(new Node(), l);
            dvonnTree.InsertChild(new Node(), l);
            dvonnTree.InsertChild(new Node(), l);

            dvonnTree.InsertChild(new Node(), m);

            dvonnTree.InsertChild(new Node(), n);

            dvonnTree.InsertChild(new Node(), o);
            dvonnTree.InsertChild(new Node(), o);

            dvonnTree.InsertChild(new Node(), p);

            dvonnTree.InsertChild(new Node(), q);

            dvonnTree.InsertChild(new Node(), r);
            dvonnTree.InsertChild(new Node(), r);

            dvonnTree.InsertChild(new Node(), s);
        }

        private void AssignValuesToLeaves()
        {
            int[] evalutations = { 5, 6, 7, 4, 5, 3, 6, 6, 9, 7, 5, 9, 8, 6  };
            string[] names = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N" };
            List<Node> leaves = dvonnTree.GetAllLeaves();
            for (int i = 0; i < 14; i++)
            {
                leaves[i].testValue = evalutations[i];
                leaves[i].name = names[i];
            }


        }

        private void Branch(Node node, int depth, int branchCount, int maxDepth)
        {
            if (depth >= maxDepth)
            {
                return;
            }
            else
            {
                for (int i = 0; i < branchCount; i++)
                {
                    dvonnTree.InsertChild(new Node(), node);
                }

                depth++;

                foreach (Node child in node.children)
                {
                    Branch(child, depth, branchCount, maxDepth);
                }
            }

        }

        //private long BranchEndpoints(int depthCounter)
        //{
        //    long createNodeCounter = 0L;
        //    long foundEndPoints = dvonnTree.currentEndPoints.Count;

        //    Console.WriteLine();
        //    Console.WriteLine("AI: Branching begun at depth " + depthCounter);
        //    Console.WriteLine("AI: Color to move: " + tempPlayerToMove.ToString());
        //    Console.WriteLine("AI: Endpoints found: " + foundEndPoints);

        //    PositionTree.GenerationAccount thisGenerationAccount = new PositionTree.GenerationAccount(depthCounter);

        //    for (int i = 0; i < foundEndPoints; i++)
        //    {
        //        Node endPoint = dvonnTree.currentEndPoints[i];
        //        thisGenerationAccount.parentNodes.Add(endPoint);
        //        aiBoard.ReceivePosition(endPoint.resultingPosition);
        //        PreMove premove = ruleBook.ManufacturePreMove(tempPlayerToMove);
        //        endPoint.premove = premove;

        //        foreach (Move move in premove.legalMoves)
        //        {
        //            Position newPosition = new Position();
        //            newPosition.Copy(endPoint.resultingPosition);
        //            newPosition.MakeMove(move);

        //            //Insert child additionally adds it to current endpoints
        //            dvonnTree.InsertChild(new Node(move, newPosition), endPoint);
        //            createNodeCounter++;
        //        }
        //    }
        //    //Clean up current endpoints
        //    foreach (Node parent in thisGenerationAccount.parentNodes)
        //    {
        //        dvonnTree.currentEndPoints.Remove(parent);
        //    }

        //    thisGenerationAccount.childCount = createNodeCounter;
        //    dvonnTree.generationAccounts.Add(thisGenerationAccount);

        //    Console.WriteLine();
        //    Console.WriteLine("AI: Branching for depth level " + depthCounter + " is complete.");
        //    Console.WriteLine("AI: added total number of nodes: " + createNodeCounter);
        //    Console.WriteLine("AI: new number of end points: " + dvonnTree.currentEndPoints.Count);

        //    tempPlayerToMove = tempPlayerToMove.ToOpposite();

        //    return createNodeCounter;

        //}





        //Perform alpha beta pruning on lower branches to minimize number of endpoints (i.e. leaves) to be evaluated.
        public void AlphaBetaPruning()
        {
            TreeCrawler treeCrawler = new TreeCrawler(dvonnTree);
            
            treeCrawler.AlphaBetaPruning();



        }


        //public void EvaluateEndPoints()
        //{
        //    long nodeCounter = 0L;
        //    long skipCounter = 0L;

        //    //for debug:
        //    float minimumEvaluation = 0f;
        //    float maximumEvaluation = 0f;
        //    bool evaluationSpanInitiated = false;

        //    bool preParseEndPoints = dvonnTree.currentEndPoints.Count > 20000;

        //    Console.WriteLine();
        //    Console.WriteLine("AI: Deep evaluation begun");

        //    foreach (Node endPoint in dvonnTree.currentEndPoints)
        //    {
        //        //skip every other endpoint if many endpoints
        //        if (preParseEndPoints)
        //        {
        //            if (nodeCounter % 2 == 0)
        //            {
        //                endPoint.isSkippable = true;
        //                skipCounter++;
        //                nodeCounter++;
        //                continue;
        //            }
        //        }

        //        //todo: player to move could be the same, if opponent has no legal moves...
        //        PieceID toMoveColor = endPoint.move.responsibleColor;

        //        aiBoard.ReceivePosition(endPoint.resultingPosition);
        //        endPoint.premove = ruleBook.ManufacturePreMove(endPoint.move.responsibleColor);

        //        float thisEval = comparator.EvaluateImprovement(endPoint);

        //        //for debug:
        //        if (evaluationSpanInitiated == false)
        //        {
        //            maximumEvaluation = thisEval;
        //            minimumEvaluation = thisEval;
        //            evaluationSpanInitiated = true;
        //        }
        //        else
        //        {
        //            if (thisEval > maximumEvaluation) maximumEvaluation = thisEval;
        //            if (thisEval < minimumEvaluation) minimumEvaluation = thisEval;
        //        }

        //        endPoint.evaluation = thisEval;
        //        nodeCounter++;

        //    }

        //    Console.WriteLine();
        //    Console.WriteLine("AI: Deep evaluation complete.");
        //    Console.WriteLine("AI: Evaluated " + nodeCounter + " positions");
        //    Console.WriteLine("AI: Skipped " + skipCounter + " positions");
        //    Console.WriteLine("AI: Minimum evaluation value found: " + minimumEvaluation);
        //    Console.WriteLine("AI: Maximum evaluation value found: " + maximumEvaluation);

        //}

        //Finds best move in children of gen 0 (i.e.upcoming AI move) by analyzing PositionTree using
        //minimax algorithm : https://en.wikipedia.org/wiki/Minimax
        public void PerformMiniMax()
        {
            List<Node> allLeaves = dvonnTree.GetAllLeaves();
            List<Node> parentGeneration = dvonnTree.GetParents(allLeaves);

            while (true)
            {

                foreach (Node parent in parentGeneration)
                {

                    if (parent.children.Count == 1)
                    {
                        parent.move.evaluation = parent.children[0].move.evaluation;
                        continue;
                    }
                    else
                    {
                        parent.move.evaluation = FindValueAmongstChildren(parent);

                    }

                }

                parentGeneration = dvonnTree.GetParents(parentGeneration);
                if (parentGeneration.Count == 1 && parentGeneration[0] == dvonnTree.root)
                {
                    dvonnTree.root.move.evaluation = FindValueAmongstChildren(dvonnTree.root);
                    break;
                }

            }

            //Move bestMove = dvonnTree.root.children.Find(child => child.evaluation == dvonnTree.root.evaluation).move;
            //Console.WriteLine();
            //Console.WriteLine("AI: The best AI move for color: " + playerToMove + " is found.");
            //Console.WriteLine("AI: It has an evaluation of: " + dvonnTree.root.evaluation);
            //Console.WriteLine("AI: The move is: " + bestMove.ToString());

            Console.WriteLine("AI: Minimax complete. Root now has an an evaluation of: " + dvonnTree.root.move.evaluation);


        }

        private int FindValueAmongstChildren(Node parent)
        {
            int searchedValue = 0;
            bool valueSpanInitiated = false;
            bool isMaximumGeneration = parent.depth % 2 == 0;

            foreach (Node child in parent.children)
            {

                if (valueSpanInitiated == false)
                {
                    searchedValue = child.move.evaluation;
                    valueSpanInitiated = true;

                }
                else
                {
                    if (isMaximumGeneration)
                    {
                        if (child.move.evaluation > searchedValue) searchedValue = child.move.evaluation;
                    }
                    else
                    {
                        if (child.move.evaluation < searchedValue) searchedValue = child.move.evaluation;
                    }

                }
            }
            return searchedValue;

        }

        public void WaitForUser()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

    }


}
