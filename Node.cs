using System.Collections.Generic;

namespace Dvonn_Console
{
    class Node
    {
        public Position position;
        public List<Node> children = new List<Node>();
        public Node parent = null;
        public Move lastMove; //the move that resulted in this position
        public bool isEndPoint = true;
        public bool isStub = false;
        public int depth;
        public long id;

        public Node(Position position, Move lastMove)
        {
            this.position = position;
            this.lastMove = lastMove;

        }

        public Node(Position position)
        {
            this.position = position;

        }
    }

}
