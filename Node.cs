

using System.Collections.Generic;

namespace Dvonn_Console
{
    public class Node
    {
        public Position position;
        public List<Node> children;
        public Node parent = null;


        public Node(Position position)
        {
            this.position = position;

        }
    }


}
