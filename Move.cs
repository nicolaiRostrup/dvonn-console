namespace Dvonn_Console
{
    public class Move
    {
        public int source;
        public int target;
        public PieceID responsibleColor;
        public bool isPassMove = false;
        public bool isGameOverMove = false;
        public int gameOverMoveDepth;
        public int whiteScore;
        public int blackScore;
        public int evaluation;
        public int secondaryEvaluation;

        //for debug:
        public bool isCollapseMove = false;
        public int collapsedTowers = 0;

        public Move()
        {

        }
        public Move(int source, int target, PieceID responsibleColor)
        {
            this.source = source;
            this.target = target;
            this.responsibleColor = responsibleColor; //that is, the 'hand' that has executes the move
        }

        public Move(PieceID responsibleColor, bool isPassMove)
        {
            this.isPassMove = isPassMove;
            this.responsibleColor = responsibleColor; //that is, the 'hand' that has executes the move
        }

        public Move(bool isGameOverMove, int whiteScore, int blackScore )
        {
            this.isGameOverMove = isGameOverMove;
            this.whiteScore = whiteScore;
            this.blackScore = blackScore;
            
        }

        public override string ToString()
        {
            return responsibleColor.ToString() + " moves a stack from " + BoardProperties.fieldCoordinates[source] + " to " + BoardProperties.fieldCoordinates[target];

        }

    }
}
