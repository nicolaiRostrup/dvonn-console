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
        //Move lastMove;
        //Move contemplatedMove;
        Move proposedMove;
        //public int maxDepth = 6;
        public int maxEndPoints = 5000;

        private int minimumQuality = 50; //a value that determines lower threshold for endpoint examination - in percentage of evaluation span.
        //Random rgen = new Random();

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

            var watch = System.Diagnostics.Stopwatch.StartNew();

            CreateTree(currentPosition, maxEndPoints);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine();
            Console.WriteLine("Tree creation took: " + elapsedMs + " milliseconds");
            Console.WriteLine(dvonnTree.ToString());

            PickBestMove();

            return proposedMove;
        }

        public void CreateTree(Position currentPosition, int maxEndPoints)
        {
            dvonnTree = new PositionTree(currentPosition);
            int depthCounter = 0;
            while (true)
            {
                BranchingCounter results = BranchRelevantEndpoints(depthCounter);
                dvonnTree.depthReach = depthCounter + 1;
                if (results.afterEndPointCount > maxEndPoints || results.createNodeCounter == 0) break;
                else depthCounter++;
            }

        }

        private class BranchingCounter
        {
            public int skipNodeCounter = 0;
            public int createNodeCounter = 0;
            public int beforeEndPointCount = 0;
            public int afterEndPointCount = 0;
        }

        private BranchingCounter BranchRelevantEndpoints(int depthCounter)
        {
            Console.WriteLine();
            Console.WriteLine("AI: Branching begun at depth " + depthCounter);
            Console.WriteLine("AI: Color to move: " + tempPlayerToMove.ToString());
            Console.WriteLine("AI: Endpoints found: " + dvonnTree.currentEndPoints.Count);

            BranchingCounter branching = new BranchingCounter();
            branching.beforeEndPointCount = dvonnTree.currentEndPoints.Count;

            float minimumEvaluation = 0f;
            float maximumEvaluation = 0f;
            bool evaluationSpanInitiated = false;

            float generationEvaluationThreshold = 0f;

            if (dvonnTree.generationAccounts.Count > 0)
            {
                PositionTree.GenerationAccount savedGenerationAccount = dvonnTree.generationAccounts.Find(account => account.generationNumber == depthCounter -1);
                generationEvaluationThreshold = ((savedGenerationAccount.maximumEvaluation - savedGenerationAccount.minimumEvaluation) * ((float) minimumQuality / 100)) + savedGenerationAccount.minimumEvaluation;
                Console.WriteLine("AI: Calculated generation evaluation threshold: " + generationEvaluationThreshold);

            }

            for (int i = 0; i < branching.beforeEndPointCount; i++)
            {
                Node endPoint = dvonnTree.currentEndPoints[i];

                //Only if root evaluate current endpoint, otherwise endpoint should have been evaluated, when branching.
                if (endPoint.id == 1)
                {
                    aiBoard.ReceivePosition(endPoint.position);
                    endPoint.position.premove = ruleBook.ManufacturePreMove(tempPlayerToMove);
                    endPoint.position.evaluation = EvaluatePosition(endPoint.position);

                }
                if(evaluationSpanInitiated == false)
                {
                    maximumEvaluation = endPoint.position.evaluation;
                    minimumEvaluation = endPoint.position.evaluation;
                    evaluationSpanInitiated = true;
                }
                //Optionally make stub based on earlier evalution
                

                if (endPoint.id != 1 && endPoint.position.evaluation < generationEvaluationThreshold)
                {
                    endPoint.isStub = true;
                    endPoint.isEndPoint = false;
                    branching.skipNodeCounter++;
                    continue;
                }
                
                //Branch endpoint and evaluate all children
                foreach (Move move in endPoint.position.premove.legalMoves)
                {
                    Position newPosition = new Position();
                    newPosition.Copy(endPoint.position);
                    newPosition.MakeMove(move);

                    aiBoard.ReceivePosition(newPosition);
                    newPosition.premove = ruleBook.ManufacturePreMove(tempPlayerToMove);
                    newPosition.evaluation = EvaluatePosition(newPosition);

                    if (newPosition.evaluation > maximumEvaluation) maximumEvaluation = newPosition.evaluation;
                    if (newPosition.evaluation < minimumEvaluation) minimumEvaluation = newPosition.evaluation;

                    dvonnTree.InsertChild(new Node(newPosition, move), endPoint);
                    branching.createNodeCounter++;

                }
                endPoint.isEndPoint = false;

            }

            PositionTree.GenerationAccount thisGenerationAccount = new PositionTree.GenerationAccount();
            thisGenerationAccount.generationNumber = depthCounter;
            thisGenerationAccount.totalNodeCount = branching.createNodeCounter + branching.skipNodeCounter;
            thisGenerationAccount.stubs = branching.skipNodeCounter;
            thisGenerationAccount.evalutatedNodes = branching.createNodeCounter;
            thisGenerationAccount.minimumEvaluation = minimumEvaluation;
            thisGenerationAccount.maximumEvaluation = maximumEvaluation;

            dvonnTree.generationAccounts.Add(thisGenerationAccount);

            Console.WriteLine();
            Console.WriteLine("AI: Intermediate evalutation account for generation: " + thisGenerationAccount.generationNumber);
            Console.WriteLine("AI: Total node count: " + thisGenerationAccount.totalNodeCount);
            Console.WriteLine("AI: Minimum evalutation: " + thisGenerationAccount.minimumEvaluation);
            Console.WriteLine("AI: Maximum evalutation: " + thisGenerationAccount.maximumEvaluation);

            //Prune irrelevant endpoints
            List<Node> relevantEndPoints = new List<Node>();

            foreach (Node thisNode in dvonnTree.currentEndPoints)
            {
                if (thisNode.isEndPoint == true) relevantEndPoints.Add(thisNode);

            }
            dvonnTree.currentEndPoints = relevantEndPoints;
            branching.afterEndPointCount = relevantEndPoints.Count;

            Console.WriteLine();
            Console.WriteLine("AI: Branching for depth level " + depthCounter + " is complete.");
            Console.WriteLine("AI: skipped total number of nodes: " + branching.skipNodeCounter);
            Console.WriteLine("AI: added total number of nodes: " + branching.createNodeCounter);
            Console.WriteLine("AI: corrected number of relevant end points: " + relevantEndPoints.Count);

            tempPlayerToMove = tempPlayerToMove.ToOpposite();

            return branching;

        }

        public void PickBestMove()
        {
            float highestEvaluation = 0f;
            foreach (Node child in dvonnTree.root.children)
            {
                if (child.isStub) continue;
                if (child.position.evaluation > highestEvaluation)
                {
                    highestEvaluation = child.position.evaluation;
                    proposedMove = child.lastMove;
                }

            }
            //proposedMove = Move;

            //Parse positions in PositionTree by evaluation 
            // and use minimax algorithm to choose best move.
            //https://en.wikipedia.org/wiki/Minimax

        }


        public float EvaluatePosition(Position position)
        {
            List<int> controlledStacks = ControlledStacks(position.premove.responsibleColor);
            int controlledStackCount = controlledStacks.Count;
            int possibleMoves = position.premove.legalMoves.Count;
            int trueLegalSources = position.premove.trueLegalSources.Count;
            float meanHeightOfOwnStacks = GetMeanHeight(controlledStacks);
            float meanDistanceToDvonn = aiBoard.GetMeanDistanceToDvonn(controlledStacks);
            int dvonnLanders = DvonnLanders(position.premove);
            int computerScore = ruleBook.GetScore(PieceID.Black);
            float computerScoreFactor = (float)computerScore / ruleBook.GetScore(PieceID.White);


            float parameter_controlledStackCount = controlledStackCount * 100;
            float parameter_possibleMoves = possibleMoves * 100;
            float parameter_trueLegalSources = trueLegalSources * 50;
            float parameter_meanHeightOfOwnStacks = 400 / meanHeightOfOwnStacks;
            float parameter_meanDistanceToDvonn = 400 / meanDistanceToDvonn;
            float parameter_dvonnLanders = dvonnLanders * 1000;
            float parameter_computerScoreFactor = computerScoreFactor * 30;

            float evaluation = parameter_controlledStackCount + parameter_possibleMoves + parameter_trueLegalSources + parameter_meanHeightOfOwnStacks + parameter_meanDistanceToDvonn + parameter_dvonnLanders + parameter_computerScoreFactor;


            /*
            
            Should count as POSITIVE:
            V - Short distance to dvonn piece.
            V - Number of controlled stacks.
            Low number of opponent controlled stacks.
            V -Number of possible moves.
            Low number of opponent possible moves.

            V - Number of movable stacks
            Low number of opponent movable stacks
            
            V - High score/low score for opponent

            If a piece can land on top of dvonn piece or tower with dvonn piece.
            + Number of pieces that can do that.

            (In the endgame: to isolate a large stack under your control).
            Capture pieces that are about to capture important stacks.

            Moving a piece towards a dvonn piece. //Is somewhat covered by 'short distance to dvonn'.

            V - In the early phase build low.
            
            One own color piece on top of one enemy piece.//Is somewhat covered by number of controlled stacks
            

            Should count as NEGATIVE:
            To build high towers //is somewhat covered by meanHeightOfOwnStacks
            
            */


            return evaluation;
        }


        List<int> ControlledStacks(PieceID color)
        {
            List<int> controlledStacks = new List<int>();
            for (int i = 0; i < 49; i++)
            {
                if (aiBoard.entireBoard[i].stack.Count == 0) continue;
                if (aiBoard.entireBoard[i].TopPiece().pieceType == color) controlledStacks.Add(i);
            }
            return controlledStacks;

        }

        float GetMeanHeight(List<int> theseStacks)
        {
            int totalHeight = 0;
            foreach (int stack in theseStacks)
            {
                totalHeight += aiBoard.entireBoard[stack].stack.Count;
            }
            return (float)totalHeight / theseStacks.Count;

        }


        int DvonnLanders(PreMove premove)
        {

            List<int> dvonnStacks = aiBoard.GetDvonnStacks();

            List<int> dvonnLanders = new List<int>();

            foreach (int fieldID in dvonnStacks)
            {
                List<int> landers = aiBoard.GetLanders(fieldID, premove);
                foreach (int landerID in landers)
                {
                    if (!dvonnLanders.Contains(landerID))
                    {
                        dvonnLanders.Add(landerID);
                    }
                }
            }
            return dvonnLanders.Count;
        }
    }
}
