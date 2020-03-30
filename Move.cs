namespace Dvonn_Console
{
    class Move
    {
        public int source;
        public int target;
        public PieceID responsibleColor;
        public int evaluation;

        public Move(int source, int target, PieceID responsibleColor)
        {
            this.source = source;
            this.target = target;
            this.responsibleColor = responsibleColor; //that is, the 'hand' that has executed the move, or is thought to execute the move
        }

        public override string ToString()
        {
            string[] fieldCoordinates = { "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CT", "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8" };
            return responsibleColor.ToString() + " moves a stack from " + fieldCoordinates[source] + " to " + fieldCoordinates[target];
            
        }

    }
}
