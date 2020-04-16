using System;
using System.Collections.Generic;
using System.Linq;

namespace Dvonn_Console
{
    class AI
    {
        private Board aiBoard;
        private Rules ruleBook;
        private PositionTree dvonnTree;

        private PieceID playerToMove;
        private PieceID tempPlayerToMove;

        private int endgameLimit = 8;


        public AI()
        {
            aiBoard = new Board();
            aiBoard.InstantiateFields();
            aiBoard.CalculatePrincipalMoves();
            ruleBook = new Rules(aiBoard);

        }

        public Move ComputeAiMove(Position currentPosition, PieceID playerToMove)
        {
            Move chosenMove;
            int depthCounter;
            this.playerToMove = playerToMove;
            tempPlayerToMove = playerToMove;

            dvonnTree = new PositionTree(currentPosition);
            aiBoard.ReceivePosition(currentPosition);
            PreMove premove = ruleBook.ManufacturePreMove(playerToMove);

            int maxEndPoints = 160000 / premove.legalMoves.Count;

            if (premove.legalMoves.Count > endgameLimit)
            {
                var watch_1 = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine();
                Console.WriteLine("AI: Normal analysis (with alfa/beta pruning) begun.");
                depthCounter = CreateTree(maxEndPoints);
                PruneAndEvaluate();
                chosenMove = PerformMiniMax();
                Console.WriteLine("AI: Normal analysis (with alfa/beta pruning) ended.");
                watch_1.Stop();
                var elapsedMs_1 = watch_1.ElapsedMilliseconds;

                Console.WriteLine();
                Console.WriteLine("Normal analysis time: " + (elapsedMs_1) + " milliseconds");
                Console.WriteLine("Last generation branched: " + depthCounter);
                Console.WriteLine("Outer endpoint depth: " + dvonnTree.GetDepthReach());
            }
            else
            {
                var watch_2 = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine();
                Console.WriteLine("AI: Endgame analysis begun.");
                depthCounter = CreateTree(maxEndPoints);
                EvaluateEndPoints();
                chosenMove = PerformMiniMax();
                Console.WriteLine("AI: Endgame analysis ended.");
                watch_2.Stop();
                var elapsedMs_2 = watch_2.ElapsedMilliseconds;

                Console.WriteLine();
                Console.WriteLine("Endgame analysis took: " + elapsedMs_2 + " milliseconds");
                Console.WriteLine("Last generation branched: " + depthCounter);
                Console.WriteLine("Outer endpoint depth: " + dvonnTree.GetDepthReach());
            }

            WaitForUser();
            return chosenMove;

        }


        private int CreateTree(int maxEndPoints)
        {
            int depthCounter = 0;

            while (true)
            {
                int createdNodes = BranchEndpoints(depthCounter);
                if (createdNodes > maxEndPoints || createdNodes == 0) break;
                else depthCounter++;
            }
            return depthCounter;

        }


        private int BranchEndpoints(int depthCounter)
        {
            int newNodeCounter = 0;
            List<Node> allLeaves = dvonnTree.GetAllLeaves();

            Console.WriteLine();
            Console.WriteLine("AI: Branching begun at depth " + depthCounter);
            Console.WriteLine("AI: Color to move: " + tempPlayerToMove.ToString());
            Console.WriteLine("AI: Endpoints found: " + allLeaves.Count);

            foreach (Node endPoint in allLeaves)
            {
                aiBoard.ReceivePosition(endPoint.resultingPosition);
                if (endPoint == dvonnTree.root) ruleBook.CheckDvonnCollapse(null, false);
                else ruleBook.CheckDvonnCollapse(endPoint.move, false);

                PreMove premovePlayer = ruleBook.ManufacturePreMove(tempPlayerToMove);
                PreMove premoveOpponent = ruleBook.ManufacturePreMove(tempPlayerToMove.ToOpposite());

                //Check if neither players have any legal moves, if yes, this endpoint doesn't branch. 
                if (ruleBook.GameEndCondition(premovePlayer, premoveOpponent) == true)
                {
                    continue;
                }

                //Check if moving player has no legal moves, if yes, a pass move is inserted in the tree
                if (ruleBook.PassCondition(premovePlayer) == true)
                {
                    Move passMove = new Move(tempPlayerToMove);
                    dvonnTree.InsertChild(new Node(passMove, endPoint.resultingPosition), endPoint);
                    newNodeCounter++;
                    continue;
                }

                foreach (Move move in premovePlayer.legalMoves)
                {
                    Position newPosition = new Position();
                    newPosition.Copy(endPoint.resultingPosition); //is this necessary?
                    newPosition.MakeMove(move);

                    dvonnTree.InsertChild(new Node(move, newPosition), endPoint);
                    newNodeCounter++;
                }
            }

            Console.WriteLine("AI: Branching for depth level " + depthCounter + " is complete.");
            Console.WriteLine("AI: added total number of nodes: " + newNodeCounter);

            tempPlayerToMove = tempPlayerToMove.ToOpposite();

            return newNodeCounter;

        }



        //Perform alpha beta pruning on lower branches to minimize number of endpoints (i.e. leaves) to be evaluated.
        //See also: https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
        public void PruneAndEvaluate()
        {
            TreeCrawler treeCrawler = new TreeCrawler(dvonnTree, this);
            treeCrawler.AlphaBetaPruning();

        }


        public void EvaluateEndPoints()
        {
            //List<Node> allLeaves = dvonnTree.GetAllLeaves(); - might this be better? should non-outer leaves be ignored??
            List<Node> outerLeavesOnly = dvonnTree.GetOuterLeaves();

            //for debug:
            int minimumEvaluation = 0;
            int maximumEvaluation = 0;
            bool evaluationSpanInitiated = false;
            Console.WriteLine();
            Console.WriteLine("AI: Deep evaluation begun");

            foreach (Node endPoint in outerLeavesOnly)
            {
                int thisEval = EvaluatePosition(endPoint.move, endPoint.resultingPosition);
                endPoint.move.evaluation = thisEval;

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
            Console.WriteLine("AI: Deep evaluation complete.");
            Console.WriteLine("AI: Evaluated " + outerLeavesOnly.Count + " positions");
            Console.WriteLine("AI: Minimum evaluation value found: " + minimumEvaluation);
            Console.WriteLine("AI: Maximum evaluation value found: " + maximumEvaluation);

        }

        //Position is evaluated through the options of both 'next player' (after the move has been executed).
        //If the resulting position gives the next player many options and a good position, the evaluation will be positive and high.
        //If the position gives equal chances to both players, it will be evaluated to 0.
        //IMPORTANT: finally, the evaluation is biased according to the generation depth. This means that if the responsible color is equal to class variable 'playerToMove'
        //the evaluation will be multiplied by -1; But if the responsible color is equal to 'next player', the evaluation will remain unaltered. 
        public int EvaluatePosition(Move move, Position resultingPosition)
        {
            int thisEval = 0;
            bool doAlgorithm = true;
            aiBoard.ReceivePosition(resultingPosition);
            ruleBook.CheckDvonnCollapse(move, false);

            PieceID movingColor = move.responsibleColor;
            PieceID nextPlayer = move.responsibleColor.ToOpposite();

            PreMove nextPlayerOptions = ruleBook.ManufacturePreMove(nextPlayer);
            PreMove samePlayerOptions = ruleBook.ManufacturePreMove(movingColor);

            //Check whether move ends game. If true, return appropriate value 
            if (ruleBook.GameEndCondition(nextPlayerOptions, samePlayerOptions) == true)
            {
                doAlgorithm = false;
                int whiteScore = ruleBook.GetScore(PieceID.White);
                int blackScore = ruleBook.GetScore(PieceID.Black);

                if (whiteScore > blackScore)
                {
                    if (movingColor == PieceID.White) thisEval = int.MaxValue;
                    else thisEval = int.MinValue;
                }
                if (blackScore > whiteScore)
                {
                    if (movingColor == PieceID.Black) thisEval = int.MaxValue;
                    else thisEval = int.MinValue;
                }
            }

            if (doAlgorithm)
            {
                //bonus for controlledstacks
                List<int> controlledStacks = aiBoard.ControlledStacks(nextPlayer);
                //thisEval += controlledStacks.Count * 100;

                //bonus for legal moves
                thisEval += nextPlayerOptions.legalMoves.Count * 200;

                //bonus for single stacks
                List<int> singles = aiBoard.GetSingles(controlledStacks);
                thisEval += singles.Count * 250;

                //bonus for singles that land on dvonn
                List<int> dvonnLanders = aiBoard.DvonnLanders(nextPlayerOptions);
                foreach (int single in singles)
                {
                    if (dvonnLanders.Contains(single)) thisEval += 800;
                }

                //bonus for dvonn landers
                thisEval += dvonnLanders.Count * 300;

                //bonus for double dvonn landers
                var dvonnLandersGrouping = dvonnLanders.GroupBy(i => i);
                foreach (var grp in dvonnLandersGrouping)
                {
                    if (grp.Count() > 1) thisEval += 800;

                }

                //bonus for landers of landers
                List<int> dvonnLandersLanders = new List<int>();
                foreach (int lander in dvonnLanders)
                {
                    List<int> landerLanders = aiBoard.GetLanders(lander, nextPlayerOptions);
                    dvonnLandersLanders.AddRange(landerLanders);
                    thisEval += landerLanders.Count * 100;
                }

                //bonus for dvonn dominance (per dvonn tower)
                List<int> dvonnTowers = aiBoard.GetDvonnStacks();
                int nextPlayerControlledDvonnTowers = 0;

                foreach (int tower in dvonnTowers)
                {
                    List<int> nextPlayerLanders = aiBoard.GetLanders(tower, nextPlayerOptions);
                    List<int> samePlayerLanders = aiBoard.GetLanders(tower, samePlayerOptions);
                    List<int> nextPlayerLostLanders = new List<int>();
                    List<int> samePlayerLostLanders = new List<int>();

                    foreach (int nextLander in nextPlayerLanders)
                    {
                        List<int> nextPlayerLanderLanders = aiBoard.GetLanders(nextLander, nextPlayerOptions);
                        List<int> samePlayerLanderLanders = aiBoard.GetLanders(nextLander, samePlayerOptions);

                        if (samePlayerLanderLanders.Count > nextPlayerLanderLanders.Count) nextPlayerLostLanders.Add(nextLander);

                    }
                    nextPlayerLanders.RemoveAll(lander => nextPlayerLostLanders.Contains(lander));


                    foreach (int sameLander in samePlayerLanders)
                    {
                        List<int> samePlayerLanderLanders = aiBoard.GetLanders(sameLander, samePlayerOptions);
                        List<int> nextPlayerLanderLanders = aiBoard.GetLanders(sameLander, nextPlayerOptions);
                        if (nextPlayerLanderLanders.Count > samePlayerLanderLanders.Count) samePlayerLostLanders.Add(sameLander);
                    }
                    samePlayerLanders.RemoveAll(lander => samePlayerLostLanders.Contains(lander));

                    if (nextPlayerLanders.Count > samePlayerLanders.Count) nextPlayerControlledDvonnTowers++;

                }
                thisEval += nextPlayerControlledDvonnTowers * 2000;

                //TODO: Dead towers could have a bearing on the evaluation....
                //Board.DeadTowerAnalysis deadTowerAnalysis = aiBoard.ManufactureDeadTowerAnalysis(ruleBook, nextPlayer);

            }

            int biasMultiplier = 0;
            if (playerToMove == movingColor) biasMultiplier = -1;
            else biasMultiplier = 1;

            if (thisEval == int.MaxValue && biasMultiplier == -1) return int.MinValue;
            if (thisEval == int.MaxValue && biasMultiplier == 1) return int.MaxValue;
            if (thisEval == int.MinValue && biasMultiplier == -1) return int.MaxValue;
            if (thisEval == int.MinValue && biasMultiplier == 1) return int.MinValue;

            return thisEval * biasMultiplier;

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
            List<Node> nodes = dvonnTree.root.children.FindAll(node => node.move.evaluation == bestMoveEvaluation);

            List<Move> candidateMoves = new List<Move>();
            foreach (Node node in nodes)
            {
                candidateMoves.Add(node.move);
            }

            if (candidateMoves.Count == 1)
            {
                chosenMove = candidateMoves[0];
                Console.WriteLine("AI: Found single candidate moves");
            }
            else
            {
                chosenMove = ChooseAmongstCandidates(candidateMoves, dvonnTree.root.resultingPosition);
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
        private Move ChooseAmongstCandidates(List<Move> moveList, Position beforePosition)
        {
            aiBoard.ReceivePosition(beforePosition);
            ruleBook.CheckDvonnCollapse(null, false);
            PreMove beforeOptions = ruleBook.ManufacturePreMove(playerToMove); //the options for moving color before considered move has been made
            int beforeDvonnLanders = aiBoard.DvonnLanders(beforeOptions).Count;

            foreach (Move move in moveList)
            {
                Position afterPosition = new Position();
                afterPosition.Copy(beforePosition);
                afterPosition.MakeMove(move);
                aiBoard.ReceivePosition(afterPosition);
                ruleBook.CheckDvonnCollapse(move, false);
                PreMove afterOptions = ruleBook.ManufacturePreMove(playerToMove); //the options for moving color after considered move has been made

                if (aiBoard.DvonnLanders(afterOptions).Count > beforeDvonnLanders) move.secondaryEvaluation += 500;
                if (afterPosition.stacks[move.target].Length == 2) move.secondaryEvaluation += 400;
                if (beforePosition.TopPiece(move.target) != playerToMove.ToChar()) move.secondaryEvaluation += 300;

            }

            moveList.Sort((x, y) => y.secondaryEvaluation.CompareTo(x.secondaryEvaluation));

            Console.WriteLine();
            Console.WriteLine("AI: Sorted list of moves, and chose best move");
            Console.WriteLine("AI: Found " + moveList.Count + " candidate moves");
            Console.WriteLine("Best candidate move eval: " + moveList[0].evaluation + ", secondary eval: " + moveList[0].secondaryEvaluation);
            Console.WriteLine("Worst candidate move eval: " + moveList.LastOrDefault().evaluation + ", secondary eval: " + moveList.LastOrDefault().secondaryEvaluation);
            Console.WriteLine("Chose : " + moveList[0].ToString() + " with a secondary evaluation of " + moveList[0].secondaryEvaluation);

            return moveList[0];

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
