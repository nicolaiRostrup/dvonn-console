using System;
using System.Collections.Generic;
using System.Text;

namespace Dvonn_Console
{
    class Game
    {
        public string whitePlayerName = "";
        public string blackPlayerName = "";
        public bool isWhiteAI = false;
        public bool isBlackAI = false;
        public readonly PieceID humanPlayerColor;

        public DateTime? timeBegun = null;
        public DateTime? timeEnded = null;
        public Position openingPosition = new Position();
        public List<Move> gameMoveList = new List<Move>();
        public int gameResultWhite = 0;
        public int gameResultBlack = 0;

        private string aiEngineName;
        private string aiEngineVersion;

        public Game(PieceID humanPlayerColor, string humanPlayerName, string aiEngineName, string aiEngineVersion)
        {
            this.humanPlayerColor = humanPlayerColor;
            this.aiEngineName = aiEngineName;
            this.aiEngineVersion = aiEngineVersion;

            if (humanPlayerColor == PieceID.White)
            {
                whitePlayerName = humanPlayerName;
                blackPlayerName = aiEngineName + " (v. " + aiEngineVersion + " )";
            }
            else
            {
                blackPlayerName = humanPlayerName;
                whitePlayerName = aiEngineName + " (v. " + aiEngineVersion + " )";
            }

        }

        public Position RandomPopulate(int dvonnCount, int whiteCount, int blackCount)
        {
            Position position = new Position();

            position = DistributePieces(position, dvonnCount, PieceID.Dvonn);
            position = DistributePieces(position, whiteCount, PieceID.White);
            position = DistributePieces(position, blackCount, PieceID.Black);

            return position;
        }

        public Position RandomPopulateWithCorrection()
        {
            Position position = new Position();

            position = PlacePiecesEvenlyOnEdge(position);

            position = DistributePieces(position, 3, PieceID.Dvonn);
            position = DistributePieces(position, 11, PieceID.White);
            position = DistributePieces(position, 11, PieceID.Black);

            return position;
        }

        Position PlacePiecesEvenlyOnEdge(Position position)
        {
            Random rGen = new Random();
            int pieceCount = 12;

            for (int i = 0; i < pieceCount; i++)
            {
                int rNum = rGen.Next(0, 24);
                int rEdgeFieldID = BoardProperties.edgeFields[rNum];

                if (position.stacks[rEdgeFieldID].Length == 0)
                {
                    position.stacks[rEdgeFieldID] += GetChar(PieceID.White);
                }
                else pieceCount++; //the field was occupied, run the loop once again.
            }

            foreach (int edgeFieldId in BoardProperties.edgeFields)
            {
                if (position.stacks[edgeFieldId].Length == 0)
                {
                    position.stacks[edgeFieldId] += GetChar(PieceID.Black);
                }
            }

            return position;
        }

        Position DistributePieces(Position position, int pieceCount, PieceID pieceColor)
        {
            Random rGen = new Random();

            for (int i = 0; i < pieceCount; i++)
            {
                int rNum = rGen.Next(0, 49);

                if (position.stacks[rNum].Length == 0)
                {
                    position.stacks[rNum] += GetChar(pieceColor);
                }
                else pieceCount++; //the field was occupied, run the loop once again.
            }

            return position;

        }

        char? GetChar(PieceID pieceColor)
        {
            if (pieceColor == PieceID.Black) return 'B';
            if (pieceColor == PieceID.White) return 'W';
            if (pieceColor == PieceID.Dvonn) return 'D';

            else return null;
        }


        private string PrintOpeningPosition()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 49; i++)
            {
                if (i == 0 || i == 40) sb.Append("  |");
                if (i == 9 || i == 30) sb.Append(" |");
                if (i == 19) sb.Append("|");
                sb.Append(openingPosition.stacks[i]);
                if (i == 8 || i == 18 || i == 29 || i == 39 || i == 48) sb.AppendLine("|");
                else sb.Append(".");
            }
            return sb.ToString();

        }

        private string GameEndText()
        {
            string result = "[ " + gameResultWhite + ", " + gameResultBlack + " ]";
            if (gameResultWhite == gameResultBlack) return "Game over: Tie: " + result;
            else if (gameResultWhite > gameResultBlack) return "Game over: White won: " + result;
            else return "Game over: Black won: " + result;
        }

        public string ToDataFormat()
        {

            //            dvonngame summary
            //white; name
            // black; ai
            //  engine; dvonndomina
            //   engineversion; 1.2
            //opening; WBBWBBBDWBBBDWWBBBWBWBWBWBWWBB
            // gameover; y
            //  begundate; 15 - 04 - 2020 10:59:31
            //enddate; 15 - 04 - 2020 10:59:50
            //whiteresult; 18
            //blackresult; 16
            //movelist; 3 - 2; 13 - 14; 43 - 44; pass; 23 - 56;


            return "";
        }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("*****************************************************");
            sb.AppendLine("Dvonn game summary:");
            sb.AppendLine();
            sb.AppendLine("Game begun: " + timeBegun.ToString());
            sb.AppendLine("Game ended: " + timeEnded.ToString());
            sb.AppendLine("White: " + whitePlayerName);
            sb.AppendLine("Black: " + blackPlayerName);
            sb.AppendLine();
            sb.AppendLine("Opening position: ");
            sb.AppendLine(PrintOpeningPosition());

            int gameLength = gameMoveList.Count;
            int moveNumber = 1;

            for (int i = 0; i < gameLength; i++)
            {
                Move thisMove = gameMoveList[i];
                string thisColor = thisMove.responsibleColor.ToChar().ToString();
                if (thisMove.responsibleColor == PieceID.White && moveNumber < 10) sb.Append(" ");
                if (thisMove.responsibleColor == PieceID.White) sb.Append(moveNumber + "." + thisColor);
                else sb.Append(thisColor);
                if (thisMove.isPassMove) sb.Append(": pass");
                else if (thisMove.source < 10) sb.Append(":  " + thisMove.source + " -> " + thisMove.target);
                else sb.Append(": " + thisMove.source + " -> " + thisMove.target);
                if (thisMove.target < 10) sb.Append(" ");
                if (thisMove.isCollapseMove) sb.Append(" *" + thisMove.collapsedTowers);
                if (timeEnded != null && i == gameLength - 1)
                {
                    sb.AppendLine(" #");
                    sb.Append(GameEndText());
                    break;
                }
                if (thisMove.responsibleColor == PieceID.White && thisMove.isCollapseMove) sb.Append("   ");
                else if (thisMove.responsibleColor == PieceID.White) sb.Append("      ");
                else
                {
                    moveNumber++;
                    sb.AppendLine();
                }

            }
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("*****************************************************");

            return sb.ToString();
        }
    }
}
