using System.Collections.Generic;
using System.Linq;


namespace Dvonn_Console
{
    class Evaluator
    {
        Board evaluatorBoard = new Board();


        //Position is evaluated through the options of both 'next player' (after the move has been executed).
        //If the resulting position gives the next player many options and a good position, the evaluation will be positive and high.
        //If the position gives equal chances to both players, it will be evaluated to 0.
        //IMPORTANT: finally, the evaluation is biased according to the generation depth. This means that if the responsible color is equal to class variable 'playerToMove'
        //the evaluation will be multiplied by -1; But if the responsible color is equal to 'next player', the evaluation will remain unaltered. 
        //
        // 'aiResponsibleColor' is the responsible color of the moves of the children of the root of current position tree. Could be human color, if eg. autofinishing game or suggesting hint.
        public int EvaluatePosition(Node endPoint, GamePhase gamePhase, PieceID aiResponsibleColor)
        {
            Move move = endPoint.move;
            Position resultingPosition = endPoint.resultingPosition;
            int thisEval = 0;

            if (move.isGameOverMove)
            {
                thisEval = ComputeGameOverValue(move, aiResponsibleColor);
                return thisEval;
            }

            //Checks for dvonn collapse and saves result in the node.
            evaluatorBoard.ReceivePosition(resultingPosition);
            evaluatorBoard.CheckDvonnCollapse(move, false);
            endPoint.resultingPosition = evaluatorBoard.SendPosition();

            PieceID movingColor = move.responsibleColor;
            PieceID nextPlayer = move.responsibleColor.ToOpposite();

            List<Move> nextPlayerLegalMoves = evaluatorBoard.FindLegalMoves(nextPlayer);
            List<Move> samePlayerLegalMoves = evaluatorBoard.FindLegalMoves(movingColor);


            if (gamePhase == GamePhase.EarlyGame)
            {
                //bonus for controlledstacks
                List<int> controlledStacks = evaluatorBoard.ControlledStacks(nextPlayer);
                thisEval += controlledStacks.Count * 100;

                //bonus for single stacks
                List<int> singles = evaluatorBoard.GetSingles(controlledStacks);
                thisEval += singles.Count * 250;

                //bonus for singles that land on dvonn
                List<int> dvonnLanders = evaluatorBoard.DvonnLanders(nextPlayerLegalMoves);
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
                List<int> dvonnTowers = evaluatorBoard.GetDvonnStacks();
                foreach (int tower in dvonnTowers)
                {
                    List<int> nextPlayerLanders = evaluatorBoard.GetLanders(tower, nextPlayerLegalMoves);
                    List<int> samePlayerLanders = evaluatorBoard.GetLanders(tower, samePlayerLegalMoves);

                    if (nextPlayerLanders.Count > samePlayerLanders.Count) thisEval += 2000;
                    else if (nextPlayerLanders.Count == samePlayerLanders.Count) thisEval += 1000;

                }

            }

            if (gamePhase == GamePhase.Apex || gamePhase == GamePhase.PostApex)
            {
                //bonus for controlledstacks
                List<int> controlledStacks = evaluatorBoard.ControlledStacks(nextPlayer);
                thisEval += controlledStacks.Count * 100;

                //bonus for legal moves
                thisEval += nextPlayerLegalMoves.Count * 200;

                //bonus for single stacks
                List<int> singles = evaluatorBoard.GetSingles(controlledStacks);
                thisEval += singles.Count * 250;

                //bonus for singles that land on dvonn
                List<int> dvonnLanders = evaluatorBoard.DvonnLanders(nextPlayerLegalMoves);
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
                    List<int> landerLanders = evaluatorBoard.GetLanders(lander, nextPlayerLegalMoves);
                    dvonnLandersLanders.AddRange(landerLanders);
                    thisEval += landerLanders.Count * 100;
                }

                //bonus for dvonn dominance (per dvonn tower)
                List<int> dvonnTowers = evaluatorBoard.GetDvonnStacks();
                foreach (int tower in dvonnTowers)
                {
                    List<int> nextPlayerLanders = evaluatorBoard.GetLanders(tower, nextPlayerLegalMoves);
                    List<int> samePlayerLanders = evaluatorBoard.GetLanders(tower, samePlayerLegalMoves);

                    if (nextPlayerLanders.Count > samePlayerLanders.Count) thisEval += 2000;
                    else if (nextPlayerLanders.Count == samePlayerLanders.Count) thisEval += 1000;

                }

                //bonus for dead tower dominance 
                List<int> deadTowers = evaluatorBoard.DeadTowers();
                foreach (int deadTower in deadTowers)
                {
                    List<int> nextPlayerLanders = evaluatorBoard.GetLanders(deadTower, nextPlayerLegalMoves);
                    List<int> samePlayerLanders = evaluatorBoard.GetLanders(deadTower, samePlayerLegalMoves);

                    if (nextPlayerLanders.Count > samePlayerLanders.Count) thisEval += 200 * evaluatorBoard.entireBoard[deadTower].stack.Count;
                    else if (nextPlayerLanders.Count == samePlayerLanders.Count) thisEval += 75 * evaluatorBoard.entireBoard[deadTower].stack.Count;
                }

            }

            if (gamePhase == GamePhase.EndGame)
            {
                //bonus for legal moves
                thisEval += nextPlayerLegalMoves.Count * 200;

                //bonus for dead tower dominance 
                List<int> deadTowers = evaluatorBoard.DeadTowers();
                foreach (int deadTower in deadTowers)
                {
                    List<int> nextPlayerLanders = evaluatorBoard.GetLanders(deadTower, nextPlayerLegalMoves);
                    List<int> samePlayerLanders = evaluatorBoard.GetLanders(deadTower, samePlayerLegalMoves);

                    if (nextPlayerLanders.Count > samePlayerLanders.Count) thisEval += 200 * evaluatorBoard.entireBoard[deadTower].stack.Count;
                    else if (nextPlayerLanders.Count == samePlayerLanders.Count) thisEval += 75 * evaluatorBoard.entireBoard[deadTower].stack.Count;
                }

            }

            int biasMultiplier = 0;
            if (aiResponsibleColor == movingColor) biasMultiplier = -1;
            else biasMultiplier = 1;

            if (thisEval == int.MaxValue && biasMultiplier == -1) return int.MinValue;
            if (thisEval == int.MaxValue && biasMultiplier == 1) return int.MaxValue;
            if (thisEval == int.MinValue && biasMultiplier == -1) return int.MaxValue;
            if (thisEval == int.MinValue && biasMultiplier == 1) return int.MinValue;

            return thisEval * biasMultiplier;

        }

        public int ComputeGameOverValue(Move move, PieceID aiResponsibleColor)
        {
            int thisEval = 0;
            bool isMaximumGeneration = move.gameOverMoveDepth % 2 != 0;

            if (move.whiteScore == move.blackScore) thisEval = 0;

            if (move.whiteScore > move.blackScore)
            {
                if (isMaximumGeneration)
                {
                    if (aiResponsibleColor == PieceID.White) thisEval = int.MaxValue;
                    else thisEval = int.MinValue;
                }
                else
                {
                    if (aiResponsibleColor == PieceID.White) thisEval = int.MinValue;
                    else thisEval = int.MaxValue;
                }

            }
            if (move.whiteScore < move.blackScore)
            {
                if (isMaximumGeneration)
                {
                    if (aiResponsibleColor == PieceID.White) thisEval = int.MinValue;
                    else thisEval = int.MaxValue;
                }
                else
                {
                    if (aiResponsibleColor == PieceID.White) thisEval = int.MaxValue;
                    else thisEval = int.MinValue;
                }

            }
            return thisEval;

        }

        public void SecondaryEvaluation(Node endPoint, GamePhase gamePhase, PieceID aiResponsibleColor, Position beforePosition)
        {

            Move thisMove = endPoint.move;

            if (gamePhase == GamePhase.EarlyGame)
            {
                if (beforePosition.TopPiece(thisMove.target) == aiResponsibleColor.ToOpposite().ToChar()) thisMove.secondaryEvaluation += 1000;
                else if (beforePosition.TopPiece(thisMove.target) == 'D') thisMove.secondaryEvaluation += 500;

                evaluatorBoard.ReceivePosition(beforePosition);
                int beforeDvonnDistance = evaluatorBoard.ShortestDistanceToDvonn(thisMove.source);
                evaluatorBoard.ReceivePosition(endPoint.resultingPosition);
                int afterDvonnDistance = evaluatorBoard.ShortestDistanceToDvonn(thisMove.target);
                thisMove.secondaryEvaluation += (beforeDvonnDistance - afterDvonnDistance) * 400;

            }

            if (gamePhase == GamePhase.Apex || gamePhase == GamePhase.PostApex)
            {
                if (beforePosition.TopPiece(thisMove.target) == aiResponsibleColor.ToOpposite().ToChar()) thisMove.secondaryEvaluation += 800;
                else if (beforePosition.TopPiece(thisMove.target) == 'D') thisMove.secondaryEvaluation += 800;

                evaluatorBoard.ReceivePosition(beforePosition);
                int beforeDvonnDistance = evaluatorBoard.ShortestDistanceToDvonn(thisMove.source);
                evaluatorBoard.ReceivePosition(endPoint.resultingPosition);
                int afterDvonnDistance = evaluatorBoard.ShortestDistanceToDvonn(thisMove.target);
                thisMove.secondaryEvaluation += (beforeDvonnDistance - afterDvonnDistance) * 800;

            }

            if (gamePhase == GamePhase.EndGame)
            {
                int playerColorScore;
                int opponentScore;
                if (endPoint.move.isGameOverMove)
                {
                    if (endPoint.move.whiteScore == endPoint.move.blackScore)
                    {
                        thisMove.secondaryEvaluation = 0;
                        return;
                        
                    }
                    if (aiResponsibleColor == PieceID.White)
                    {
                        playerColorScore = endPoint.move.whiteScore;
                        opponentScore = endPoint.move.blackScore;
                    }
                    else
                    {
                        playerColorScore = endPoint.move.blackScore;
                        opponentScore = endPoint.move.whiteScore;
                    }

                }
                else
                {
                    playerColorScore = endPoint.resultingPosition.GetScore(aiResponsibleColor);
                    opponentScore = endPoint.resultingPosition.GetScore(aiResponsibleColor.ToOpposite());
                }
                thisMove.secondaryEvaluation += playerColorScore - opponentScore;
            }
            

        }
    }
}
