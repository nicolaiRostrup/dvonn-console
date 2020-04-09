using System;
using System.Collections.Generic;

namespace Dvonn_Console
{
    class AI
    {
        private Board aiBoard;
        private Rules ruleBook;
        private PositionTree dvonnTree;

        private PieceID playerToMove;
        private PieceID tempPlayerToMove;
        private Move chosenMove = null;

        //maxendpoints may in reality be considerarbly larger, as the process is only stopped after entire generation branching.
        private int maxEndPoints = 1000;


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


            var watch_1 = System.Diagnostics.Stopwatch.StartNew();
            CreateTree(currentPosition);
            watch_1.Stop();
            var elapsedMs_1 = watch_1.ElapsedMilliseconds;

            var watch_2 = System.Diagnostics.Stopwatch.StartNew();
            EvaluateEndPoints();
            PerformMiniMax();
            watch_2.Stop();
            var elapsedMs_2 = watch_2.ElapsedMilliseconds;

            Console.WriteLine();
            Console.WriteLine("Tree creation took: " + elapsedMs_1 + " milliseconds");
            Console.WriteLine("Evaluation of endpoints took: " + elapsedMs_2 + " milliseconds");
            Console.WriteLine("Total calculation time: " + (elapsedMs_1 + elapsedMs_2) + " milliseconds");

            WaitForUser();

            return chosenMove;

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


            return UseSimpleAlgorithm(currentPosition);
        }


        //Algorithm used in beginning of game
        private Move UseSimpleAlgorithm(Position currentPosition)
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
                ruleBook.DoDvonnCollapseRoutine();
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

        private void CreateTree(Position currentPosition)
        {
            dvonnTree = new PositionTree(currentPosition);
            int depthCounter = 0;
            while (true)
            {
                long createNodeCount = BranchEndpoints(depthCounter);
                if (createNodeCount > maxEndPoints || createNodeCount == 0) break;
                else depthCounter++;
            }

        }


        private long BranchEndpoints(int depthCounter)
        {
            long createNodeCounter = 0L;
            List<Node> allLeaves = dvonnTree.GetAllLeaves();

            Console.WriteLine();
            Console.WriteLine("AI: Branching begun at depth " + depthCounter);
            Console.WriteLine("AI: Color to move: " + tempPlayerToMove.ToString());
            Console.WriteLine("AI: Endpoints found: " + allLeaves.Count);

            foreach (Node endPoint in allLeaves)
            {
                aiBoard.ReceivePosition(endPoint.resultingPosition);
                ruleBook.DoDvonnCollapseRoutine();

                PreMove premovePlayer = ruleBook.ManufacturePreMove(tempPlayerToMove);
                PreMove premoveOpponent = ruleBook.ManufacturePreMove(tempPlayerToMove.ToOpposite());

                //Check whether game has ended. 
                if (ruleBook.GameEndCondition(premovePlayer, premoveOpponent) == true)
                {
                    continue;
                }

                //Check if moving player has any legal moves
                if (ruleBook.PassCondition(premovePlayer) == true)
                {
                    Move passMove = new Move(tempPlayerToMove);
                    dvonnTree.InsertChild(new Node(passMove, endPoint.resultingPosition), endPoint);
                    createNodeCounter++;
                    continue;
                }

                foreach (Move move in premovePlayer.legalMoves)
                {
                    Position newPosition = new Position();
                    newPosition.Copy(endPoint.resultingPosition);
                    newPosition.MakeMove(move);

                    dvonnTree.InsertChild(new Node(move, newPosition), endPoint);
                    createNodeCounter++;
                }
            }

            Console.WriteLine();
            Console.WriteLine("AI: Branching for depth level " + depthCounter + " is complete.");
            Console.WriteLine("AI: added total number of nodes: " + createNodeCounter);

            tempPlayerToMove = tempPlayerToMove.ToOpposite();

            return createNodeCounter;

        }



        //Perform alpha beta pruning on lower branches to minimize number of endpoints (i.e. leaves) to be evaluated.
        //See also: https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
        public void AlphaBetaPruning()
        {
            TreeCrawler treeCrawler = new TreeCrawler(dvonnTree);
            treeCrawler.AlphaBetaPruning();

        }


        public void EvaluateEndPoints()
        {
            List<Node> allLeaves = dvonnTree.GetAllLeaves();

            //for debug:
            float minimumEvaluation = 0f;
            float maximumEvaluation = 0f;
            bool evaluationSpanInitiated = false;
            Console.WriteLine();
            Console.WriteLine("AI: Deep evaluation begun");

            foreach (Node endPoint in allLeaves)
            {
                int thisEval = EvaluatePosition(endPoint.move, endPoint.resultingPosition);

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

                endPoint.move.evaluation = thisEval;
            }

            Console.WriteLine();
            Console.WriteLine("AI: Deep evaluation complete.");
            Console.WriteLine("AI: Evaluated " + allLeaves.Count + " positions");
            Console.WriteLine("AI: Minimum evaluation value found: " + minimumEvaluation);
            Console.WriteLine("AI: Maximum evaluation value found: " + maximumEvaluation);

        }

        //Position is evaluated through the options of opponent after move has been executed
        //If the resulting position gives the opponent (human player) very few options and a bad position, the evaluation will be positive and high.
        //If on the other hand the resulting position is excellent for opponent, the evaluation will be negative and high.
        //If the position gives equal chances to both players, it will be evaluated to 0.
        private int EvaluatePosition(Move move, Position resultingPosition)
        {
            int thisEval = 0;
            aiBoard.ReceivePosition(resultingPosition);
            ruleBook.DoDvonnCollapseRoutine();

            //todo: player to move could be the same, if opponent has no legal moves...
            //
            //But as default the player to move (after the chosen move has been applied), is the opponent:
            //To move color, in this context, means the next player to move. As this is immediately after an AI move. This means
            //the opponent of the AI - the human player.
            PieceID humanPlayer = move.responsibleColor.ToOpposite();
            PreMove humanOptions = ruleBook.ManufacturePreMove(humanPlayer);
            PositionReport reportHumanPlayer = ManufacturePositionReport(humanPlayer, humanOptions);

            //Opponent color, in this context, means the opposite player of the next player to move. So this is the AI player (again).
            PreMove aiOptions = ruleBook.ManufacturePreMove(humanPlayer.ToOpposite());
            PositionReport reportAiPlayer = ManufacturePositionReport(humanPlayer.ToOpposite(), aiOptions);

            //Check whether move ends game. If true, return appropriate value 
            if (ruleBook.GameEndCondition(humanOptions, aiOptions) == true)
            {
                int whiteScore = ruleBook.GetScore(PieceID.White);
                int blackScore = ruleBook.GetScore(PieceID.Black);
                if (whiteScore == blackScore) return 0;
                if (whiteScore > blackScore)
                {
                    if (playerToMove == PieceID.White) return int.MaxValue;
                    else return int.MinValue;
                }
                if (blackScore > whiteScore)
                {
                    if (playerToMove == PieceID.Black) return int.MaxValue;
                    else return int.MinValue;
                }
            }

            Board.DeadTowerAnalysis deadTowerAnalysis = aiBoard.ManufactureDeadTowerAnalysis(ruleBook, humanPlayer);

            //controlledstacks: many is good
            if (reportHumanPlayer.controlledStackCount > reportAiPlayer.controlledStackCount) thisEval -= 200;
            if (reportHumanPlayer.controlledStackCount < reportAiPlayer.controlledStackCount) thisEval += 200;

            //mean distance to dvonn: low is good
            if (reportHumanPlayer.meanDistanceToDvonn > reportAiPlayer.meanDistanceToDvonn) thisEval += 100;
            if (reportHumanPlayer.meanDistanceToDvonn < reportAiPlayer.meanDistanceToDvonn) thisEval -= 100;

            //mean height of own stacks: low is good
            if (reportHumanPlayer.meanHeightOfOwnStacks > reportAiPlayer.meanHeightOfOwnStacks) thisEval += 200;
            if (reportHumanPlayer.meanHeightOfOwnStacks < reportAiPlayer.meanHeightOfOwnStacks) thisEval -= 200;

            //possible moves: many is good
            if (reportHumanPlayer.possibleMoves > reportAiPlayer.possibleMoves) thisEval -= 300;
            if (reportHumanPlayer.possibleMoves < reportAiPlayer.possibleMoves) thisEval += 300;

            //true legal sources: many is good
            if (reportHumanPlayer.trueLegalSources > reportAiPlayer.trueLegalSources) thisEval -= 300;
            if (reportHumanPlayer.trueLegalSources < reportAiPlayer.trueLegalSources) thisEval += 300;

            //dvonn landers: many is good
            if (reportHumanPlayer.dvonnLanders > reportAiPlayer.dvonnLanders) thisEval -= 500;
            if (reportHumanPlayer.dvonnLanders < reportAiPlayer.dvonnLanders) thisEval += 500;

            //dead tower gain: many is good
            if (deadTowerAnalysis.humanColorTowerGain > deadTowerAnalysis.aiColorTowerGain) thisEval -= 1000;
            if (deadTowerAnalysis.humanColorTowerGain < deadTowerAnalysis.aiColorTowerGain) thisEval += 1000;

            //dead tower control: more is good
            thisEval -= deadTowerAnalysis.humanColorDvonnTowerControl * 500;
            thisEval += deadTowerAnalysis.aiColorDvonnTowerControl * 500;

            return thisEval;
        }

        private PositionReport ManufacturePositionReport(PieceID movingColor, PreMove premove)
        {
            PositionReport report = new PositionReport();

            List<int> controlledStacks = aiBoard.ControlledStacks(movingColor);
            report.controlledStackCount = controlledStacks.Count;
            report.meanDistanceToDvonn = aiBoard.GetMeanDistanceToDvonn(controlledStacks);
            report.meanHeightOfOwnStacks = aiBoard.GetMeanHeight(controlledStacks);
            report.possibleMoves = premove.legalMoves.Count;
            report.trueLegalSources = premove.trueLegalSources.Count;
            report.dvonnLanders = aiBoard.DvonnLanders(premove);

            return report;
        }

        public class PositionReport
        {
            public int controlledStackCount = 0;
            public float meanDistanceToDvonn = 0f;
            public float meanHeightOfOwnStacks = 0f;
            public int possibleMoves = 0;
            public int trueLegalSources = 0;
            public int dvonnLanders = 0;

        }


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
                    int bestMoveEvaluation = FindValueAmongstChildren(dvonnTree.root);
                    chosenMove = dvonnTree.root.children.Find(node => node.move.evaluation == bestMoveEvaluation).move;
                    break;
                }

            }

            Console.WriteLine("AI: Minimax complete. Best move found and has an evaluation of: " + chosenMove.evaluation);
            Console.WriteLine("AI: The move is: " + chosenMove.ToString());

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

        //--------------------------------------------------------------

        public void WaitForUser()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

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
