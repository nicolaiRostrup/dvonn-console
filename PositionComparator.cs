using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{

    class PositionComparator
    {

        private PieceID aiColor;
        private Board compareBoard = new Board();
        private Rules ruleBook;

        public PositionComparator(PieceID aiColor)
        {
            this.aiColor = aiColor;
            compareBoard.InstantiateFields();
            compareBoard.CalculatePrincipalMoves();
            ruleBook = new Rules(compareBoard);
        }

        //Improvement is allways measured for AI side. If, AI is generally computing move for black, 
        //and responsible color for this function is white and the move leads to an improvement to white, the answer will be negative.
        public float EvaluateImprovement(Node thisNode)
        {
            Position positionBefore = thisNode.parent.resultingPosition;
            Position positionAfter = thisNode.resultingPosition;
            Move move = thisNode.move;
            PreMove beforePremove = thisNode.parent.premove; //informative when analyzing the position before move is perfomed
            PreMove afterPremove = thisNode.premove; //informative when analyzing the position after move has been performed
            PieceID movingColor = move.responsibleColor;
            bool isMaximumGeneration = movingColor == aiColor;

            PositionReport beforeReport;
            PositionReport afterReport;


            if (thisNode.parent.positionReport != null)
            {
                beforeReport = thisNode.parent.positionReport;
            }
            else
            {
                compareBoard.ReceivePosition(positionBefore);
                beforeReport = ManufacturePositionReport(movingColor, beforePremove);
                thisNode.parent.positionReport = beforeReport;
            }
            if (thisNode.positionReport != null)
            {
                afterReport = thisNode.positionReport;
            }
            else
            {
                compareBoard.ReceivePosition(positionAfter);
                afterReport = ManufacturePositionReport(movingColor, afterPremove);
                thisNode.positionReport = afterReport;
            }
            int totalstackCount = positionBefore.NumberOfStacks();
            float evaluation = 0f;
            bool isEndGame = totalstackCount < 20;

            //opening game environment:
            if (isEndGame == false)
            {
                //if moving color places a piece on opposite color:
                //if (positionBefore.TopPiece(move.target) != movingColor.ToChar()) evaluation += 200;

                //if (positionAfter.stacks[move.target].Length > 2) evaluation -= 300;
                float maxStackFactor = (49f / totalstackCount) + 1.5f;
                
                if (afterReport.maxHeightOfOwnStacks > maxStackFactor) evaluation -= 1000;

                if (movingColor == PieceID.White)
                {
                    evaluation *= afterReport.zebraCountWhite;
                }
                if (movingColor == PieceID.Black)
                {
                    evaluation *= afterReport.zebraCountBlack;
                }

                if (afterReport.controlledStackCount > beforeReport.controlledStackCount) evaluation += 100;

                //if (afterReport.meanDistanceToDvonn < beforeReport.meanDistanceToDvonn) evaluation += 100;

                //if (afterReport.possibleMoves >= beforeReport.possibleMoves) evaluation += 100;

                if (afterReport.dvonnLanders > beforeReport.dvonnLanders) evaluation += 150;


            }
            //endgame environment:
            else
            {
                if (afterReport.dvonnLanders > beforeReport.dvonnLanders) evaluation += 300;

                if (afterReport.controlledStackCount > beforeReport.controlledStackCount) evaluation += 200;

                if (afterReport.possibleMoves > beforeReport.possibleMoves) evaluation += 200;

                if (movingColor == PieceID.White)
                {
                    if (afterReport.whiteScore - beforeReport.whiteScore > 4) evaluation += 200;
                }
                if (movingColor == PieceID.Black)
                {
                    if (afterReport.blackScore - beforeReport.blackScore > 4) evaluation += 200;
                }
            }

            if (isMaximumGeneration) return evaluation;
            else return -evaluation;


        }

        private PositionReport ManufacturePositionReport(PieceID movingColor, PreMove premove)
        {
            PositionReport report = new PositionReport();

            report.zebraCountWhite = compareBoard.GetZebraCount(PieceID.White);
            report.zebraCountBlack = compareBoard.GetZebraCount(PieceID.Black);

            List<int> controlledStacks = compareBoard.ControlledStacks(movingColor);
            report.controlledStackCount = controlledStacks.Count;
            report.meanDistanceToDvonn = compareBoard.GetMeanDistanceToDvonn(controlledStacks);
            report.maxHeightOfOwnStacks = compareBoard.GetMaxHeight(controlledStacks);
            report.possibleMoves = premove.legalMoves.Count;
            //report.trueLegalSources = premove.trueLegalSources.Count;
            report.dvonnLanders = compareBoard.DvonnLanders(premove);
            report.whiteScore = ruleBook.GetScore(PieceID.White);
            report.blackScore = ruleBook.GetScore(PieceID.Black);

            return report;
        }

        public class PositionReport
        {
            public int zebraCountWhite = 0;
            public int zebraCountBlack = 0;
            public int controlledStackCount = 0;
            public float meanDistanceToDvonn = 0f;
            public int maxHeightOfOwnStacks = 0;
            public int possibleMoves = 0;
            //public int trueLegalSources = 0;
            public int dvonnLanders = 0;
            public int whiteScore = 0;
            public int blackScore = 0;

        }



    }


}
