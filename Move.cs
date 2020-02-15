namespace Dvonn_Console
{
    public class Move
    {
        public int source;
        public int target;
        public PieceID responsibleColor;

        public Move(int source, int target, PieceID responsibleColor)
        {
            this.source = source;
            this.target = target;
            this.responsibleColor = responsibleColor; //that is, the 'hand' that has executed the move, or is thought to execute the move
        }

    }
}
