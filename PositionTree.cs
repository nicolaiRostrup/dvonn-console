

using System;
using System.Collections.Generic;

namespace Dvonn_Console
{

    public class PositionTree
    {
        Node root;
        public List<Node> allNodes = new List<Node>();
        public List<Node> endPoints = new List<Node>();

        int nodeCounter = 0;
        int depthCounter = 0;


        public PositionTree(Position rootPosition)
        {
            root = new Node(rootPosition);
            endPoints.Add(root);
            allNodes.Add(root);
        }


        public Node InsertChild(Position position, Node parent )
        { 
            Node childNode = new Node(position);
            parent.children.Add(childNode);
            childNode.parent = parent;
            allNodes.Add(childNode);

            return childNode;
        }


        private string DisplayTree(Node root)
        {
            if (root == null) return "Root is null";
            
            string result = "";

            depthCounter++;

            while (nodeCounter < 1000)
            {
                foreach (Node node in root.children)
                {
                    nodeCounter++;
                    result += "Position #" + nodeCounter + " at depth " + depthCounter + " contains " + node.position.NumberOfStacks() + " stacks and evaluates to " + node.position.evaluation + "\n";
                    DisplayTree(node);
                }
            }

            return result;
        }

        public override string ToString() 
        {
            nodeCounter = 0;
            depthCounter = 0;
            return DisplayTree(root);
        }

    }

}
