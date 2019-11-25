

using System.Collections.Generic;

namespace Dvonn_Console
{
    class Node
    {
        public Position position;
        public List<Node> children = new List<Node>();
        public Node parent = null;
        public int depth;
        public Move lastMove; //the move that resulted in this position


        public Node(Position position, Move lastMove)
        {
            this.position = position;
            this.lastMove = lastMove;

        }

        public Node()
        {
            

        }
    }


}
