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
        //Move lastMove;
        //Move contemplatedMove;
        Move proposedMove;
        public int maxDepth = 6;

        private float minimumQuality = 0.85f; //a value that determines lower threshold for endpoint examination.
        Random rgen = new Random();

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

            var watch = System.Diagnostics.Stopwatch.StartNew();

            CreateTree(currentPosition, maxDepth);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine();
            Console.WriteLine("Tree Creation took: " + elapsedMs + " milliseconds");
            Console.WriteLine(dvonnTree.ToString());

            //todo: write and optimize AI flow (!)
            //PickBestMove();
            //(...)
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

            return proposedMove;
        }

        public void CreateTree(Position currentPosition, int maxDepth)
        {
            dvonnTree = new PositionTree(currentPosition);

            for (int i = 1; i <= maxDepth; i++)
            {
                BranchRelevantEndpoints(i);
            }

            EvaluateResultingEndPoints();
        }

        void BranchRelevantEndpoints(int depth)
        {
            Console.WriteLine();
            Console.WriteLine("AI: Branching begun at depth " + depth);
            Console.WriteLine("AI: Color to move: " + playerToMove.ToString());
            Console.WriteLine("AI: Endpoints found: " + dvonnTree.currentEndPoints.Count);

            int currentEndPointCount = dvonnTree.currentEndPoints.Count;
            int skipNodeCounter = 0;
            int createNodeCounter = 0;

            for (int i = 0; i < currentEndPointCount; i++)
            {
                Node endPoint = dvonnTree.currentEndPoints[i];

                //todo: evaluate position needs to be implemented properly, and a value for minimum quality needs to be properly determined.
                float evaluation = EvaluatePosition(endPoint.position);
                endPoint.position.evaluation = evaluation;

                if (endPoint.id != 1 && evaluation < minimumQuality)
                {
                    endPoint.isStub = true;
                    endPoint.isEndPoint = false;
                    skipNodeCounter++;
                    continue;
                }
                else
                {
                    aiBoard.ReceivePosition(endPoint.position);
                    PreMove thisPreMove = ruleBook.ManufacturePreMove(playerToMove);

                    foreach (Move move in thisPreMove.legalMoves)
                    {
                        Position newPosition = aiBoard.SendPosition();
                        newPosition.MakeMove(move);

                        dvonnTree.InsertChild(new Node(newPosition, move), endPoint);
                        createNodeCounter++;

                    }
                    endPoint.isEndPoint = false;

                }

            }

            //Prune irrelevant endpoints
            List<Node> relevantEndPoints = new List<Node>();

            foreach (Node thisNode in dvonnTree.currentEndPoints)
            {
                if (thisNode.isEndPoint == true) relevantEndPoints.Add(thisNode);

            }
            dvonnTree.currentEndPoints = relevantEndPoints;
            Console.WriteLine();
            Console.WriteLine("AI: Branching for depth level " + depth + " is complete.");
            Console.WriteLine("AI: skipped total number of nodes: " + skipNodeCounter);
            Console.WriteLine("AI: added total number of nodes: " + createNodeCounter);
            Console.WriteLine("AI: corrected number of relevant end points: " + relevantEndPoints.Count);

            playerToMove = playerToMove.ToOpposite();

        }

        void EvaluateResultingEndPoints()
        {
            foreach (Node thisNode in dvonnTree.currentEndPoints)
            {
                thisNode.position.evaluation = EvaluatePosition(thisNode.position);

            }

        }

        public void PickBestMove()
        {
            //Evaluate positions in PositionTree 
            // and use minimax algorithm to choose best move.
            //https://en.wikipedia.org/wiki/Minimax

        }


        public float EvaluatePosition(Position position)
        {
            /*Note: Most of these required methods are already written further below.

            Required Methods:
            Should count as POSITIVE:
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
            

            Should count as NEGATIVE:
            To build high towers
           
            
            Methods needed (all aply to situation after stack has been moved to chosen target, and concerns the properties of the resulting target:
         
            - DistanceToDvonn() return int - the smaller the better. Regardless of its a legal move and regardless of the dvonn piece being somehwere within a tower.

            - ControlledStacks(pieceColor); has color on top, regardless of its a legal source

            - MovableStacks(pieceColor); equals 'legal sources'
            //Use: premove.trueLegalSources

            - PossibleMoves(pieceColor); 
            //Use: premove.legalMoves.count

            - Score(pieceColor)
           
            int DvonnLanders - A lander is here defined as a stack that may hit a dvonn piece or a dvonn containing stack with a legal move.

            bool isOwnPieceOnOpponentPiece

            TowerHeight()


            */

            //As a test feature, this method currently returns a random float
            int randomNumber = rgen.Next(0, 100);
            return randomNumber / 100f;

        }

        int DistanceToDvonn(Position thisPosition, Move lastMove)
        {
            aiBoard.ReceivePosition(thisPosition);
            return aiBoard.GetDistanceToDvonn(lastMove);

        }

        int ControlledStacks(Position thisPosition, PieceID color)
        {
            int controlledStacks = 0;
            for (int i = 0; i < 49; i++)
            {
                if (thisPosition.stacks[i].Length == 0) continue;
                if (thisPosition.TopPiece(i) == color.ToChar()) controlledStacks++;
            }
            return controlledStacks;

        }

        int ResultingScore(Position thisPosition, Move lastMove)
        {
            aiBoard.ReceivePosition(thisPosition);
            int score = 0;
            int[] scoreArray = ruleBook.Score();
            if (lastMove.responsibleColor == PieceID.White) score = scoreArray[0];
            if (lastMove.responsibleColor == PieceID.Black) score = scoreArray[1];

            return score;
        }

        int DvonnLanders(Position thisPosition, PreMove premove)
        {
            aiBoard.ReceivePosition(thisPosition);
            
            List<int> dvonnStacks = aiBoard.GetDvonnStacks();

            List<int> dvonnLanders = new List<int>();

            foreach (int fieldID in dvonnStacks)
            {
                List<int> landers = aiBoard.GetLanders(fieldID, premove);
                foreach(int landerID in landers)
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
