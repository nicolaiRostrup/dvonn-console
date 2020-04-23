using System.Collections.Generic;
using System.Text;

namespace Dvonn_Console
{
    class Node
    {
        public Move move = null;
        public Position resultingPosition;

        public int depth = 0;
        public Node parent = null;
        public List<Node> children = new List<Node>();

        public int alpha = int.MinValue;
        public int beta = int.MaxValue;

        //for debug purposes:
        public string name = "";
        public int testValue = 0;

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

        //For game over node
        public Node(Move move)
        {
            this.move = move;

        }

        //For test purposes
        public Node()
        {

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine();
            sb.AppendLine( "Node data for node in generation: " + depth);
            if (move != null)
            {
                sb.AppendLine( "Node holds move: " + move.ToString());
                sb.AppendLine( "which has an evaluation of: " + move.evaluation);
            }

            if (parent == null)
            {
                sb.AppendLine( "This node has no parent");
                sb.AppendLine( "It is most probably the root node.");
            }
            else
            {
                sb.AppendLine( "The parent of this node has " + parent.children.Count + " children.");
                sb.AppendLine( "Of this bunch, this node is number: " + parent.children.IndexOf(this));
            }
            if (children.Count == 0)
            {
                sb.AppendLine( "This node is an end-node.");
            }
            else
            {
                sb.AppendLine( "This node itself has: " + children.Count + " children");
            }

            return sb.ToString();
        }
    }

}
