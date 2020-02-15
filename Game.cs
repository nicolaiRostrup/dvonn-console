using System;


namespace Dvonn_Console
{
    class Game
    {
        //Currently human player is allways white
        //TODO: write option to choose piece color, and enter name...
        //public Player whitePlayer;
        //public Player blackPlayer;

        public readonly bool isAiDriven = false;
        public readonly bool isRandomPopulated = true;
        public readonly PieceID humanPlayerColor;

        public Game(bool isAiDriven, bool isRandomPopulated, PieceID humanPlayerColor)
        {
            this.isAiDriven = isAiDriven;
            this.isRandomPopulated = isRandomPopulated;
            this.humanPlayerColor = humanPlayerColor;
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
                int rEdgeFieldID = position.edgeFields[rNum];

                if (position.stacks[rEdgeFieldID].Length == 0)
                {
                    position.stacks[rEdgeFieldID] += GetChar(PieceID.White);
                }
                else pieceCount++; //the field was occupied, run the loop once again.
            }

            foreach(int edgeFieldId in position.edgeFields)
            {
                if(position.stacks[edgeFieldId].Length == 0)
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
    }
}
