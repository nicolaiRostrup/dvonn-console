using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Dvonn_Console
{

    class AI
    {
        private Board aiBoard = new Board();
        private PositionTree dvonnTree = new PositionTree();
        private Evaluator evaluator = new Evaluator();
        private GamePhase currentGamePhase;

        public readonly string aiEngineName = "DvonnDomina";
        public readonly string aiEngineVersion = "0.2";

        private PieceID playerToMove;
        private PieceID tempPlayerToMove;

        //for debug:
        private int depthCounter = 0;

      

        public Move ComputeAiMove(Position currentPosition, PieceID playerToMove)
        {
            Move chosenMove = new Move();
            depthCounter = 0;
            this.playerToMove = playerToMove;
            tempPlayerToMove = playerToMove;
            dvonnTree = new PositionTree(currentPosition);
            aiBoard.ReceivePosition(currentPosition);
            int legalMoveCount = aiBoard.FindLegalMoves(playerToMove).Count;
            int emptyStackCount = 49 - aiBoard.FindNotEmptyStacks().Count;
            currentGamePhase = DetermineGamePhase(emptyStackCount);

            Console.WriteLine();
            Console.WriteLine("AI: Legal moves count: " + legalMoveCount);
            Console.WriteLine("AI: Empty stacks count: " + emptyStackCount);
            Console.WriteLine("AI: Game Phase: " + currentGamePhase);
            Console.WriteLine();
            Console.WriteLine("AI: Computation begun.");
            var watch_1 = System.Diagnostics.Stopwatch.StartNew();

            if (currentGamePhase == GamePhase.EarlyGame)
            {
                CreateTree(3, 8000);
                PruneAndEvaluate();
                dvonnTree.RefreshAlphaBeta();
                if (dvonnTree.GetAllLeaves().Count < 1500)
                {
                    CreateTree(2, 8000);
                    PruneAndEvaluate();
                }
                chosenMove = PerformMiniMax();
                
            }

            if (currentGamePhase == GamePhase.Apex)
            {
                CreateTree(3, 7000);
                PruneAndEvaluate();
                dvonnTree.RefreshAlphaBeta();
                if (dvonnTree.GetAllLeaves().Count < 1000)
                {
                    CreateTree(2, 5000);
                    PruneAndEvaluate();
                }
                chosenMove = PerformMiniMax();
                
            }

            if (currentGamePhase == GamePhase.PostApex)
            {
                int allLeavesCount;
                do
                {
                    CreateTree(3, 9000);
                    PruneAndEvaluate();
                    dvonnTree.RefreshAlphaBeta();
                    allLeavesCount = dvonnTree.GetAllLeaves().Count;

                } while (allLeavesCount < 2500);

                int percentage = (int)((allLeavesCount - 1000) * 100 / (float)allLeavesCount);
                Console.WriteLine("AI: Hard prune percentage set to: " + percentage);
                HardPrune(percentage);
                dvonnTree.RemoveAllStubs();
                CreateTree(2, 3500);
                PruneAndEvaluate();
                chosenMove = PerformMiniMax();
                
            }

            if (currentGamePhase == GamePhase.EndGame)
            {
                CreateTree(15, 10000);
                EvaluateEndPoints();

                chosenMove = PerformMiniMax();

            }

            watch_1.Stop();
            var elapsedMs_1 = watch_1.ElapsedMilliseconds;
            Console.WriteLine("AI: Computation ended.");
            Console.WriteLine("AI: Total computation took: " + elapsedMs_1 + " milliseconds");
            Console.WriteLine("Outer endpoint depth: " + dvonnTree.GetDepthReach());
            WaitForUser();

            return chosenMove;

        }

        private GamePhase DetermineGamePhase(int emptyStackCount)
        {
            if (0 <= emptyStackCount && emptyStackCount < 6) return GamePhase.EarlyGame;
            else if (6 <= emptyStackCount && emptyStackCount < 12) return GamePhase.Apex;
            else if (12 <= emptyStackCount && emptyStackCount < 28) return GamePhase.PostApex;
            else return GamePhase.EndGame;
        }

        private void CreateTree(int maxDepth, int maxEndPoints)
        {
            for (int i = 0; i < maxDepth; i++)
            {
                bool stopBranching = BranchEndpoints(maxEndPoints);
                if (stopBranching) return;

            }

        }

        private bool BranchEndpoints(int maxEndPoints)
        {
            int newNodeCounter = 0;
            int gameOverMoveCounter = 0;
            bool gameOver = false;
            List<Node> allLeaves = dvonnTree.GetOuterLeaves();

            //Console.WriteLine();
            //Console.WriteLine("AI: Branching begun at depth " + depthCounter);
            //Console.WriteLine("AI: Color to move: " + tempPlayerToMove.ToString());
            //Console.WriteLine("AI: Endpoints found: " + allLeaves.Count);

            foreach (Node endPoint in allLeaves)
            {
                //if endpoint is already a 'gameOverMove' the move will be copied as the only child 
                if (endPoint.move != null && endPoint.move.isGameOverMove)
                {
                    Move gameOverMove = new Move(true, endPoint.move.whiteScore, endPoint.move.blackScore);
                    gameOverMove.gameOverMoveDepth = endPoint.move.gameOverMoveDepth;
                    dvonnTree.InsertChild(new Node(gameOverMove), endPoint);
                    newNodeCounter++;
                    gameOverMoveCounter++;
                    if (gameOverMoveCounter == allLeaves.Count)
                    {
                        Console.WriteLine();
                        Console.WriteLine("AI: stopping branching. All endpoints are 'game over moves'.");
                        gameOver = true;
                    }
                    continue;
                }

                //Checks for dvonn collapse and saves it to endpoint position.
                aiBoard.ReceivePosition(endPoint.resultingPosition);
                if (endPoint == dvonnTree.root) aiBoard.CheckDvonnCollapse(null, false);
                else aiBoard.CheckDvonnCollapse(endPoint.move, false);
                endPoint.resultingPosition = aiBoard.SendPosition();

                List<Move> playerLegalMoves = aiBoard.FindLegalMoves(tempPlayerToMove);
                List<Move> opponentLegalMoves = aiBoard.FindLegalMoves(tempPlayerToMove.ToOpposite());

                //Check if neither players have any legal moves, if yes, a game over move is created
                if (playerLegalMoves.Count == 0 && opponentLegalMoves.Count == 0)
                {
                    int whiteScore = endPoint.resultingPosition.GetScore(PieceID.White);
                    int blackScore = endPoint.resultingPosition.GetScore(PieceID.Black);
                    Move gameOverMove = new Move(true, whiteScore, blackScore);
                    gameOverMove.gameOverMoveDepth = endPoint.depth + 1;
                    dvonnTree.InsertChild(new Node(gameOverMove), endPoint);
                    newNodeCounter++;
                    continue;
                }

                //Check if moving player has no legal moves, if yes, a pass move is inserted in the tree
                if (playerLegalMoves.Count == 0)
                {
                    Move passMove = new Move(tempPlayerToMove, true);
                    dvonnTree.InsertChild(new Node(passMove, endPoint.resultingPosition), endPoint);
                    newNodeCounter++;
                    continue;
                }

                foreach (Move move in playerLegalMoves)
                {
                    Position newPosition = new Position();
                    newPosition.Copy(endPoint.resultingPosition);
                    newPosition.MakeMove(move);

                    dvonnTree.InsertChild(new Node(move, newPosition), endPoint);
                    newNodeCounter++;
                }
            }

            Console.WriteLine("AI: Branching for depth level " + depthCounter + " is complete.");
            Console.WriteLine("AI: added total number of nodes: " + newNodeCounter);

            tempPlayerToMove = tempPlayerToMove.ToOpposite();
            depthCounter++;

            return newNodeCounter == 0 || newNodeCounter > maxEndPoints || gameOver;

        }

        private void HardPrune(int percentage)
        {
            List<Node> allLeaves = dvonnTree.GetOuterLeaves();
            List<Node> badLeaves = new List<Node>();

            int minimumEvaluation = 0;
            int maximumEvaluation = 0;
            bool evaluationSpanInitiated = false;
            Console.WriteLine();
            Console.WriteLine("AI: Hard pruning initiated");

            foreach (Node node in allLeaves)
            {
                if (node.move.isGameOverMove) continue;
                if (evaluationSpanInitiated == false)
                {
                    maximumEvaluation = node.move.evaluation;
                    minimumEvaluation = node.move.evaluation;
                    evaluationSpanInitiated = true;
                }
                else
                {
                    if (node.move.evaluation > maximumEvaluation) maximumEvaluation = node.move.evaluation;
                    if (node.move.evaluation < minimumEvaluation) minimumEvaluation = node.move.evaluation;
                }

            }
            bool removeBelow = allLeaves[0].parent.depth % 2 == 0;
            if (removeBelow)
            {
                float percentile = ((maximumEvaluation - minimumEvaluation) * (percentage / 100f)) + minimumEvaluation;
                //Console.WriteLine("AI: Percentile set to: " + percentile);
                //Console.WriteLine("AI: Removing below percentile");
                foreach (Node node in allLeaves)
                {
                    if (node.move.evaluation < percentile)
                    {
                        badLeaves.Add(node);
                    }

                }

            }
            else
            {
                float percentile = ((maximumEvaluation - minimumEvaluation) * ((100 - percentage) / 100f)) + minimumEvaluation;
                //Console.WriteLine("AI: Percentile set to: " + percentile);
                //Console.WriteLine("AI: Removing above percentile");
                foreach (Node node in allLeaves)
                {
                    if (node.move.evaluation > percentile)
                    {
                        badLeaves.Add(node);
                    }

                }

            }
            int pruneCounter = 0;
            foreach (Node node in badLeaves)
            {
                node.parent.children.Remove(node);
                pruneCounter++;

            }
            Console.WriteLine("AI: Hard pruned: " + pruneCounter + " nodes");
        }


        //Perform alpha beta pruning on lower branches to minimize number of endpoints (i.e. leaves) to be evaluated.
        //See also: https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
        public void PruneAndEvaluate()
        {
            TreeCrawler treeCrawler = new TreeCrawler(dvonnTree, currentGamePhase, playerToMove);
            treeCrawler.AlphaBetaPruning();

        }

        public void EvaluateEndPoints()
        {
            List<Node> outerLeavesOnly = dvonnTree.GetOuterLeaves();

            //for debug:
            int minimumEvaluation = 0;
            int maximumEvaluation = 0;
            bool evaluationSpanInitiated = false;
            int gameOverCounter = 0;
            Console.WriteLine();
            Console.WriteLine("AI: Full evaluation begun");

            int nodeCounter = 0;
            foreach (Node endPoint in outerLeavesOnly)
            {
                int thisEval = evaluator.EvaluatePosition(endPoint, currentGamePhase, playerToMove);
                endPoint.move.evaluation = thisEval;
                nodeCounter++;

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

            }

            Console.WriteLine();
            Console.WriteLine("AI: Full evaluation complete.");
            Console.WriteLine("AI: Evaluated " + nodeCounter + " positions");
            if (gameOverCounter > 0) Console.WriteLine("AI: Skipped " + gameOverCounter + " nodes, as 'game over' moves.");
            Console.WriteLine("AI: Minimum evaluation value found: " + minimumEvaluation);
            Console.WriteLine("AI: Maximum evaluation value found: " + maximumEvaluation);

        }

        //Finds best move in children of gen 0 (i.e.upcoming AI move) by analyzing PositionTree using
        //minimax algorithm : https://en.wikipedia.org/wiki/Minimax
        public Move PerformMiniMax()
        {
            List<Node> outerLeavesOnly = dvonnTree.GetOuterLeaves();
            List<Node> parentGeneration = dvonnTree.GetParents(outerLeavesOnly);

            while (!(parentGeneration.Count == 1 && parentGeneration[0] == dvonnTree.root))
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

            }

            return EndMiniMax();
        }


        private Move EndMiniMax()
        {
            Move chosenMove = null;
            int bestMoveEvaluation = FindValueAmongstChildren(dvonnTree.root);
            List<Node> candidates = dvonnTree.root.children.FindAll(node => node.move.evaluation == bestMoveEvaluation);

            if (candidates.Count == 1)
            {
                chosenMove = candidates[0].move;
                Console.WriteLine("AI: Found single candidate moves");
            }
            else
            {
                chosenMove = ChooseAmongstCandidates(candidates, dvonnTree.root.resultingPosition);
            }

            Console.WriteLine("AI: Minimax complete. Best move found and has an evaluation of: " + chosenMove.evaluation);
            Console.WriteLine("AI: The move is: " + chosenMove.ToString());

            return chosenMove;

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

        //Algorithm used if a multitude of candidate moves have even evaluation.
        private Move ChooseAmongstCandidates(List<Node> candidates, Position beforePosition)
        {
            foreach (Node node in candidates)
            {
                evaluator.SecondaryEvaluation(node, currentGamePhase, playerToMove, beforePosition);

            }

            candidates.Sort((x, y) => y.move.secondaryEvaluation.CompareTo(x.move.secondaryEvaluation));

            Console.WriteLine();
            Console.WriteLine("AI: Sorted list of moves, and chose best move");
            Console.WriteLine("AI: Found " + candidates.Count + " candidate moves");
            Console.WriteLine("Best candidate move eval: " + candidates[0].move.evaluation + ", secondary eval: " + candidates[0].move.secondaryEvaluation);
            Console.WriteLine("Worst candidate move eval: " + candidates.LastOrDefault().move.evaluation + ", secondary eval: " + candidates.LastOrDefault().move.secondaryEvaluation);
            Console.WriteLine("Chose : " + candidates[0].move.ToString() + " with a secondary evaluation of " + candidates[0].move.secondaryEvaluation);

            return candidates[0].move;

        }

        public void WaitForUser()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }


        //for test purposes:
        //--------------------------------------------------------------


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
            int[] evalutations = { 5, 6, 7, 4, 5, 3, 6, 6, 9, 7, 5, 9, 8, 6 };
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

    }


}