

using System.Collections.Generic;

namespace Dvonn_Console
{
    public class Node
    {
        public Position position;
        public List<Node> children = new List<Node>();
        public Node parent = null;
        public int depth;


        public Node(Position position)
        {
            this.position = position;

        }
    }


}
