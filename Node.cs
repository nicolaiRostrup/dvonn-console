using System.Collections.Generic;

namespace Dvonn_Console
{
    class Node
    {
        public Move move = null;
        public float evaluation = 0f;

        public Position resultingPosition;
        public PositionComparator.PositionReport positionReport = null;
        public PreMove premove = null;

        public Node parent = null;
        public List<Node> children = new List<Node>();
        
        public bool isSkippable = false;
        

        public Node(Move move, Position resultingPosition)
        {
            this.move = move;
            this.resultingPosition = resultingPosition;

        }

        //For root node
        public Node(Position resultingPosition)
        {
            this.resultingPosition = resultingPosition;

        }
    }

}
