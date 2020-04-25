using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Dvonn_Console
{
    class AI
    {
        private Board aiBoard;
        private Rules ruleBook;
        private PositionTree dvonnTree;
        private GamePhase currentGamePhase;

        public readonly string aiEngineName = "DvonnDomina";
        public readonly string aiEngineVersion = "0.2";

        private PieceID playerToMove;
        private PieceID tempPlayerToMove;

        //for debug:
        private int depthCounter = 0;



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
            currentGamePhase = DetermineGamePhase(emptyStackCount);

            Console.WriteLine();
            Console.WriteLine("AI: Legal moves count: " + legalMoveCount);
            Console.WriteLine("AI: Empty stacks count: " + emptyStackCount);
            Console.WriteLine("AI: Game Phase: " + currentGamePhase);

            if (currentGamePhase == GamePhase.EarlyGame)
            {
                var watch_1 = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine();
                Console.WriteLine("AI: Two pass branching begun.");

                CreateTree(3, 8000);
                PruneAndEvaluate();
                dvonnTree.RefreshAlphaBeta();
                if (dvonnTree.GetAllLeaves().Count < 1500)
                {
                    CreateTree(2, 8000);
                    PruneAndEvaluate();
                }
                chosenMove = PerformMiniMax();

                watch_1.Stop();
                var elapsedMs_1 = watch_1.ElapsedMilliseconds;
                Console.WriteLine("AI: Two pass branching ended.");
                Console.WriteLine("Two pass branching and evaluation took: " + elapsedMs_1 + " milliseconds");
                Console.WriteLine("Outer endpoint depth: " + dvonnTree.GetDepthReach());
                WaitForUser();
                return chosenMove;
            }

            if (currentGamePhase == GamePhase.Apex)
            {

                var watch_1 = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine();
                Console.WriteLine("AI: Two pass branching begun.");

                CreateTree(3, 7000);
                PruneAndEvaluate();
                dvonnTree.RefreshAlphaBeta();
                if (dvonnTree.GetAllLeaves().Count < 1000)
                {
                    CreateTree(2, 5000);
                    PruneAndEvaluate();
                }
                chosenMove = PerformMiniMax();

                watch_1.Stop();
                var elapsedMs_1 = watch_1.ElapsedMilliseconds;
                Console.WriteLine("AI: Two pass branching  ended.");
                Console.WriteLine("Two pass branching and evaluation took: " + elapsedMs_1 + " milliseconds");
                Console.WriteLine("Outer endpoint depth: " + dvonnTree.GetDepthReach());
                WaitForUser();
                return chosenMove;
            }

            if (currentGamePhase == GamePhase.PostApex)
            {
                var watch_1 = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine();
                Console.WriteLine("AI: Chain branching with hard pruning begun.");

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

                watch_1.Stop();
                var elapsedMs_1 = watch_1.ElapsedMilliseconds;
                Console.WriteLine("AI: Chain branching with hard pruning ended.");
                Console.WriteLine("Chain branching evaluation took: " + elapsedMs_1 + " milliseconds");
                Console.WriteLine("Outer endpoint depth: " + dvonnTree.GetDepthReach());
                WaitForUser();
                return chosenMove;
            }

            if (currentGamePhase == GamePhase.EndGame)
            {

                var watch_1 = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine();
                Console.WriteLine("AI: End game style branching begun.");

                CreateTree(15, 10000);
                EvaluateEndPoints();

                chosenMove = PerformMiniMax();

                watch_1.Stop();
                var elapsedMs_1 = watch_1.ElapsedMilliseconds;
                Console.WriteLine("AI: End game style branching ended.");
                Console.WriteLine("End game style branching took: " + elapsedMs_1 + " milliseconds");
                Console.WriteLine("Outer endpoint depth: " + dvonnTree.GetDepthReach());
                WaitForUser();
                return chosenMove;

            }

            throw new Exception("Unexpected erroneous production of AI move");

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

            Console.WriteLine();
            Console.WriteLine("AI: Branching begun at depth " + depthCounter);
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
            int gameOverCounter = 0;
            Console.WriteLine();
            Console.WriteLine("AI: Full evaluation begun");

            int nodeCounter = 0;
            foreach (Node endPoint in outerLeavesOnly)
            {
                int thisEval = EvaluatePosition(endPoint);
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

        //Position is evaluated through the options of both 'next player' (after the move has been executed).
        //If the resulting position gives the next player many options and a good position, the evaluation will be positive and high.
        //If the position gives equal chances to both players, it will be evaluated to 0.
        //IMPORTANT: finally, the evaluation is biased according to the generation depth. This means that if the responsible color is equal to class variable 'playerToMove'
        //the evaluation will be multiplied by -1; But if the responsible color is equal to 'next player', the evaluation will remain unaltered. 
        public int EvaluatePosition(Node endPoint)
        {
            Move move = endPoint.move;
            Position resultingPosition = endPoint.resultingPosition;
            int thisEval = 0;

            if (move.isGameOverMove)
            {
                thisEval = ComputeGameOverValue(move);
                return thisEval;
            }

            //Checks for dvonn collapse and saves result in the node.
            aiBoard.ReceivePosition(resultingPosition);
            ruleBook.CheckDvonnCollapse(move, false);
            endPoint.resultingPosition = aiBoard.SendPosition();

            PieceID movingColor = move.responsibleColor;
            PieceID nextPlayer = move.responsibleColor.ToOpposite();

            List<Move> nextPlayerLegalMoves = ruleBook.FindLegalMoves(nextPlayer);
            List<Move> samePlayerLegalMoves = ruleBook.FindLegalMoves(movingColor);


            if (currentGamePhase == GamePhase.EarlyGame)
            {
                //bonus for controlledstacks
                List<int> controlledStacks = aiBoard.ControlledStacks(nextPlayer);
                thisEval += controlledStacks.Count * 100;

                //bonus for single stacks
                List<int> singles = aiBoard.GetSingles(controlledStacks);
                thisEval += singles.Count * 250;

                //bonus for singles that land on dvonn
                List<int> dvonnLanders = aiBoard.DvonnLanders(nextPlayerLegalMoves);
                foreach (int single in singles)
                {
                    if (dvonnLanders.Contains(single)) thisEval += 800;
                }

                //bonus for dvonn landers in general
                thisEval += dvonnLanders.Count * 300;

                //bonus for double dvonn landers
                var dvonnLandersGrouping = dvonnLanders.GroupBy(i => i);
                foreach (var grp in dvonnLandersGrouping)
                {
                    if (grp.Count() > 1) thisEval += 800;

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

            }

            if (currentGamePhase == GamePhase.Apex || currentGamePhase == GamePhase.PostApex)
            {
                //bonus for controlledstacks
                List<int> controlledStacks = aiBoard.ControlledStacks(nextPlayer);
                thisEval += controlledStacks.Count * 100;

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

                //bonus for dvonn landers in general
                thisEval += dvonnLanders.Count * 300;

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



            if (currentGamePhase == GamePhase.EndGame)
            {
                //bonus for legal moves
                thisEval += nextPlayerLegalMoves.Count * 200;

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

        public int ComputeGameOverValue(Move move)
        {
            int thisEval = 0;
            bool isMaximumGeneration = move.gameOverMoveDepth % 2 != 0;

            if (move.whiteScore == move.blackScore) thisEval = 0;

            if (move.whiteScore > move.blackScore)
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
            if (move.whiteScore < move.blackScore)
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

            //List<Move> candidateMoves = new List<Move>();
            //foreach (Node node in nodes)
            //{
            //    candidateMoves.Add(node.move);
            //}

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

                Move thisMove = node.move;

                if (currentGamePhase == GamePhase.EarlyGame)
                {
                    if (beforePosition.TopPiece(thisMove.target) == playerToMove.ToOpposite().ToChar()) thisMove.secondaryEvaluation += 1000;
                    else if (beforePosition.TopPiece(thisMove.target) == 'D') thisMove.secondaryEvaluation += 500;

                    aiBoard.ReceivePosition(beforePosition);
                    int beforeDvonnDistance = aiBoard.ShortestDistanceToDvonn(thisMove.source);
                    aiBoard.ReceivePosition(node.resultingPosition);
                    int afterDvonnDistance = aiBoard.ShortestDistanceToDvonn(thisMove.target);
                    thisMove.secondaryEvaluation += (beforeDvonnDistance - afterDvonnDistance) * 400;

                }

                if (currentGamePhase == GamePhase.Apex || currentGamePhase == GamePhase.PostApex)
                {
                    if (beforePosition.TopPiece(thisMove.target) == playerToMove.ToOpposite().ToChar()) thisMove.secondaryEvaluation += 800;
                    else if (beforePosition.TopPiece(thisMove.target) == 'D') thisMove.secondaryEvaluation += 800;

                    aiBoard.ReceivePosition(beforePosition);
                    int beforeDvonnDistance = aiBoard.ShortestDistanceToDvonn(thisMove.source);
                    aiBoard.ReceivePosition(node.resultingPosition);
                    int afterDvonnDistance = aiBoard.ShortestDistanceToDvonn(thisMove.target);
                    thisMove.secondaryEvaluation += (beforeDvonnDistance - afterDvonnDistance) * 800;

                }

                if (currentGamePhase == GamePhase.EndGame)
                {
                    int playerColorScore;
                    int opponentScore;
                    if (node.move.isGameOverMove)
                    {
                        if (node.move.whiteScore == node.move.blackScore)
                        {
                            thisMove.secondaryEvaluation = 0;
                            continue;
                        }
                        if (playerToMove == PieceID.White)
                        {
                            playerColorScore = node.move.whiteScore;
                            opponentScore = node.move.blackScore;
                        }
                        else
                        {
                            playerColorScore = node.move.blackScore;
                            opponentScore = node.move.whiteScore;
                        }

                    }
                    else
                    {
                        playerColorScore = node.resultingPosition.GetScore(playerToMove);
                        opponentScore = node.resultingPosition.GetScore(playerToMove.ToOpposite());
                    }
                    thisMove.secondaryEvaluation += playerColorScore - opponentScore;
                }
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