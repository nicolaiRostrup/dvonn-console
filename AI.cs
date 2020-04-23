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

        public readonly string aiEngineName = "DvonnDomina";
        public readonly string aiEngineVersion = "0.2";

        private PieceID playerToMove;
        private PieceID tempPlayerToMove;

        //for debug:
        private int depthCounter = 0;

        //specific ai engine parameters:
        private int aiStrengthModifier = 1;
        private int hardPrunePercentage = 60;


        public AI()
        {
            aiBoard = new Board();
            aiBoard.InstantiateFields();
            aiBoard.CalculatePrincipalMoves();
            ruleBook = new Rules(aiBoard);

        }

        public enum GamePhase
        {
            EarlyGame, Apex, PostApex, EndGame
        }



        public Move ComputeAiMove(Position currentPosition, PieceID playerToMove)
        {
            Move chosenMove;
            depthCounter = 0;
            this.playerToMove = playerToMove;
            tempPlayerToMove = playerToMove;
            dvonnTree = new PositionTree(currentPosition);
            aiBoard.ReceivePosition(currentPosition);
            int legalMoveCount = ruleBook.FindLegalMoves(playerToMove).Count;
            int emptyStackCount = 49 - ruleBook.FindNotEmptyStacks().Count;

            Console.WriteLine();
            Console.WriteLine("AI: Legal moves count: " + legalMoveCount);
            Console.WriteLine("AI: Not empty stacks count: " + emptyStackCount);

            GamePhase gamePhase = DetermineGamePhase(emptyStackCount);
            BranchingRecipe thisRecipe = DetermineBranchingRecipe(gamePhase, legalMoveCount);

            if (thisRecipe.twoPassMethod)
            {
                Console.WriteLine("AI: Game Phase: " + gamePhase);
                Console.WriteLine("AI: First pass depth set to: " + thisRecipe.firstPassDepth + ". Second pass depth set to: " + thisRecipe.secondPassDepth);
                var watch_1 = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine();
                Console.WriteLine("AI: Normal analysis (with alfa/beta pruning) begun.");

                CreateTree(thisRecipe.firstPassDepth, thisRecipe.firstPassMaxEndpoints);
                PruneAndEvaluate();
                dvonnTree.RefreshAlphaBeta();

                HardPrune(hardPrunePercentage);
                dvonnTree.RemoveAllStubs();

                CreateTree(thisRecipe.secondPassDepth, thisRecipe.secondPassMaxEndpoints);
                PruneAndEvaluate();
                chosenMove = PerformMiniMax();

                Console.WriteLine("AI: Normal analysis (with alfa/beta pruning) ended.");
                watch_1.Stop();
                var elapsedMs_1 = watch_1.ElapsedMilliseconds;

                Console.WriteLine();
                Console.WriteLine("Normal analysis time: " + elapsedMs_1 + " milliseconds");
                Console.WriteLine("Outer endpoint depth: " + dvonnTree.GetDepthReach());
                //WaitForUser();
                //Console.WriteLine();
                //Console.WriteLine(dvonnTree.ToString());
            }
            else
            {
                var watch_2 = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine();
                Console.WriteLine("AI: Endgame analysis begun.");
                CreateTree(thisRecipe.firstPassDepth, thisRecipe.firstPassMaxEndpoints);
                EvaluateEndPoints();
                chosenMove = PerformMiniMax();
                Console.WriteLine("AI: Endgame analysis ended.");
                watch_2.Stop();
                var elapsedMs_2 = watch_2.ElapsedMilliseconds;

                Console.WriteLine();
                Console.WriteLine("Endgame analysis took: " + elapsedMs_2 + " milliseconds");
                Console.WriteLine("Outer endpoint depth: " + dvonnTree.GetDepthReach());
            }

            WaitForUser();
            return chosenMove;

        }

        private GamePhase DetermineGamePhase(int emptyStackCount)
        {
            if (0 <= emptyStackCount && emptyStackCount < 6) return GamePhase.EarlyGame;
            else if (6 <= emptyStackCount && emptyStackCount < 12) return GamePhase.Apex;
            else if (12 <= emptyStackCount && emptyStackCount < 20) return GamePhase.PostApex;
            else return GamePhase.EndGame;
        }

        private BranchingRecipe DetermineBranchingRecipe(GamePhase gamePhase, int legalMovesCount)
        {

            if (gamePhase == GamePhase.EarlyGame)
            {
                if (30 <= legalMovesCount && legalMovesCount < 45) return new BranchingRecipe(3, 2, 2000, 8000);
                else if (45 <= legalMovesCount && legalMovesCount < 48) return new BranchingRecipe(3, 2, 2000, 5000);
                else if (48 <= legalMovesCount && legalMovesCount < 54) return new BranchingRecipe(3, 2, 2000, 3000);
                else if (54 <= legalMovesCount && legalMovesCount < 65) return new BranchingRecipe(2, 2, 10000, 3000);
                else return new BranchingRecipe(2, 1, 5000, 3000);
            }
            if (gamePhase == GamePhase.Apex)
            {
                if (40 <= legalMovesCount && legalMovesCount < 45) return new BranchingRecipe(3, 2, 1500, 8000);
                else if (45 <= legalMovesCount && legalMovesCount < 56) return new BranchingRecipe(3, 2, 1500, 3000);
                else if (56 <= legalMovesCount && legalMovesCount < 70) return new BranchingRecipe(2, 2, 1500, 3000);
                else return new BranchingRecipe(2, 1, 1500, 2000);
            }
            if (gamePhase == GamePhase.PostApex)
            {
                if (30 <= legalMovesCount && legalMovesCount < 45) return new BranchingRecipe(3, 4, 10000, 8000);
                else if (45 <= legalMovesCount && legalMovesCount < 52) return new BranchingRecipe(3, 3, 10000, 8000);
                else if (52 <= legalMovesCount && legalMovesCount < 58) return new BranchingRecipe(2, 3, 10000, 8000);
                else return new BranchingRecipe(2, 2, 8000, 3000);
            }
            if (gamePhase == GamePhase.EndGame)
            {
                if (0 <= legalMovesCount && legalMovesCount < 15) return new BranchingRecipe(15, 30000);
                else if (15 <= legalMovesCount && legalMovesCount < 25) return new BranchingRecipe(15, 10000);
                else if (25 <= legalMovesCount && legalMovesCount < 40) return new BranchingRecipe(8, 8000);
                else return new BranchingRecipe(5, 8000);
            }
            throw new ArgumentException("Unexpected combination of gamephase and legalmovescount: " + gamePhase.ToString() + ", " + legalMovesCount);

        }

        private class BranchingRecipe
        {
            public bool twoPassMethod = true;
            public int firstPassDepth = 0;
            public int secondPassDepth = 0;
            public int firstPassMaxEndpoints = 0;
            public int secondPassMaxEndpoints = 0;

            public BranchingRecipe(int firstPassDepth, int secondPassDepth, int firstPassMaxEndpoints, int secondPassMaxEndpoints)
            {
                twoPassMethod = true;
                this.firstPassDepth = firstPassDepth;
                this.secondPassDepth = secondPassDepth;
                this.firstPassMaxEndpoints = firstPassMaxEndpoints;
                this.secondPassMaxEndpoints = secondPassMaxEndpoints;

            }

            public BranchingRecipe(int firstPassDepth, int firstPassMaxEndpoints)
            {
                twoPassMethod = false;
                this.firstPassDepth = firstPassDepth;
                this.firstPassMaxEndpoints = firstPassMaxEndpoints;

            }

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
                    gameOverMove.evaluation = endPoint.move.evaluation;
                    dvonnTree.InsertChild(new Node(gameOverMove), endPoint);
                    newNodeCounter++;
                    continue;
                }

                aiBoard.ReceivePosition(endPoint.resultingPosition);
                if (endPoint == dvonnTree.root) ruleBook.CheckDvonnCollapse(null, false);
                else ruleBook.CheckDvonnCollapse(endPoint.move, false);

                List<Move> playerLegalMoves = ruleBook.FindLegalMoves(tempPlayerToMove);
                List<Move> opponentLegalMoves = ruleBook.FindLegalMoves(tempPlayerToMove.ToOpposite());

                //Check if neither players have any legal moves, if yes, a game over move is created
                if (playerLegalMoves.Count == 0 && opponentLegalMoves.Count == 0)
                {
                    int whiteScore = endPoint.resultingPosition.GetScore(PieceID.White);
                    int blackScore = endPoint.resultingPosition.GetScore(PieceID.Black);

                    Move gameOverMove = new Move(true, whiteScore, blackScore);
                    gameOverMove.evaluation = ComputeGameOverValue(endPoint);
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

            return newNodeCounter == 0 || newNodeCounter > maxEndPoints;

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
                Console.WriteLine("AI: Percentile set to: " + percentile);
                Console.WriteLine("AI: Removing below percentile");
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
                Console.WriteLine("AI: Percentile set to: " + percentile);
                Console.WriteLine("AI: Removing above percentile");
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
            TreeCrawler treeCrawler = new TreeCrawler(dvonnTree, this);
            treeCrawler.AlphaBetaPruning();

        }


        public void EvaluateEndPoints()
        {
            List<Node> outerLeavesOnly = dvonnTree.GetOuterLeaves();

            //for debug:
            int minimumEvaluation = 0;
            int maximumEvaluation = 0;
            bool evaluationSpanInitiated = false;
            Console.WriteLine();
            Console.WriteLine("AI: Deep evaluation begun");

            int nodeCounter = 0;
            foreach (Node endPoint in outerLeavesOnly)
            {
                //int thisEval = 0;
                //if (endPoint.move.isGameOverMove) thisEval = ComputeGameOverValue(endPoint);
                //else thisEval = EvaluatePosition(endPoint.move, endPoint.resultingPosition);

                if (endPoint.move.isGameOverMove) continue;

                int thisEval = EvaluatePosition(endPoint.move, endPoint.resultingPosition);
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
            Console.WriteLine("AI: Deep evaluation complete.");
            Console.WriteLine("AI: Evaluated " + nodeCounter + " positions");
            Console.WriteLine("AI: Minimum evaluation value found: " + minimumEvaluation);
            Console.WriteLine("AI: Maximum evaluation value found: " + maximumEvaluation);

        }

        public int ComputeGameOverValue(Node endPoint)
        {
            int thisEval = 0;
            bool isMaximumGeneration = tempPlayerToMove == playerToMove;

            if (endPoint.move.whiteScore == endPoint.move.blackScore) thisEval = 0;

            if (endPoint.move.whiteScore > endPoint.move.blackScore)
            {
                if (isMaximumGeneration)
                {
                    if (playerToMove == PieceID.White) thisEval = int.MaxValue;
                    else thisEval = int.MinValue;
                }
                else
                {
                    if (playerToMove == PieceID.White) thisEval = int.MinValue;
                    else thisEval = int.MaxValue;
                }

            }
            if (endPoint.move.whiteScore < endPoint.move.blackScore)
            {
                if (isMaximumGeneration)
                {
                    if (playerToMove == PieceID.White) thisEval = int.MinValue;
                    else thisEval = int.MaxValue;
                }
                else
                {
                    if (playerToMove == PieceID.White) thisEval = int.MaxValue;
                    else thisEval = int.MinValue;
                }

            }
            return thisEval;

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

            List<Move> nextPlayerLegalMoves = ruleBook.FindLegalMoves(nextPlayer);
            List<Move> samePlayerLegalMoves = ruleBook.FindLegalMoves(movingColor);

            //Check whether move ends game. If true, return appropriate value 
            if (nextPlayerLegalMoves.Count == 0 && samePlayerLegalMoves.Count == 0)
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
                thisEval += nextPlayerLegalMoves.Count * 200;

                //bonus for single stacks
                List<int> singles = aiBoard.GetSingles(controlledStacks);
                thisEval += singles.Count * 250;

                //bonus for singles that land on dvonn
                List<int> dvonnLanders = aiBoard.DvonnLanders(nextPlayerLegalMoves);
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
                    List<int> landerLanders = aiBoard.GetLanders(lander, nextPlayerLegalMoves);
                    dvonnLandersLanders.AddRange(landerLanders);
                    thisEval += landerLanders.Count * 100;
                }

                //bonus for dvonn dominance (per dvonn tower)
                List<int> dvonnTowers = aiBoard.GetDvonnStacks();
                foreach (int tower in dvonnTowers)
                {
                    List<int> nextPlayerLanders = aiBoard.GetLanders(tower, nextPlayerLegalMoves);
                    List<int> samePlayerLanders = aiBoard.GetLanders(tower, samePlayerLegalMoves);

                    if (nextPlayerLanders.Count > samePlayerLanders.Count) thisEval += 2000;
                    else if (nextPlayerLanders.Count == samePlayerLanders.Count) thisEval += 1000;

                }

                //bonus for dead tower dominance 
                List<int> deadTowers = aiBoard.DeadTowers(ruleBook);
                foreach (int deadTower in deadTowers)
                {
                    List<int> nextPlayerLanders = aiBoard.GetLanders(deadTower, nextPlayerLegalMoves);
                    List<int> samePlayerLanders = aiBoard.GetLanders(deadTower, samePlayerLegalMoves);

                    if (nextPlayerLanders.Count > samePlayerLanders.Count) thisEval += 200 * aiBoard.entireBoard[deadTower].stack.Count;
                    else if (nextPlayerLanders.Count == samePlayerLanders.Count) thisEval += 75 * aiBoard.entireBoard[deadTower].stack.Count;
                }

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
            List<Move> beforeLegalMoves = ruleBook.FindLegalMoves(playerToMove);
            int beforeDvonnLanders = aiBoard.DvonnLanders(beforeLegalMoves).Count;

            foreach (Move move in moveList)
            {
                Position afterPosition = new Position();
                afterPosition.Copy(beforePosition);
                afterPosition.MakeMove(move);
                aiBoard.ReceivePosition(afterPosition);
                ruleBook.CheckDvonnCollapse(move, false);
                List<Move> afterLegalMoves = ruleBook.FindLegalMoves(playerToMove);

                if (aiBoard.DvonnLanders(afterLegalMoves).Count > beforeDvonnLanders) move.secondaryEvaluation += 500;
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
