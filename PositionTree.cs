

using System;
using System.Collections.Generic;

namespace Dvonn_Console
{

    public class PositionTree
    {
        Node root;
        public List<Node> allNodes = new List<Node>();


        public PositionTree(Position rootPosition)
        {
            root = new Node(rootPosition);
            root.depth = 0;
            allNodes.Add(root);
        }


        public void InsertChild(Position position, Node parent, int depth)
        {
            Node childNode = new Node(position);
            childNode.depth = depth;
            parent.children.Add(childNode);
            childNode.parent = parent;
            allNodes.Add(childNode);
            
        }


        public string DisplayTree(int maxNodes)
        {
            if (allNodes.Count == 0) return "Tree is empty.";

            string result = "";

            int nodeCounter = 0;

            for (int i = 0; i < allNodes.Count; i++)
            {
                result += "Position #" + nodeCounter + " at depth " + allNodes[i].depth + " contains " + allNodes[i].position.NumberOfStacks() + " stacks and evaluates to " + allNodes[i].position.evaluation + "\n";
                nodeCounter++;
                if (nodeCounter == maxNodes) break;

            }

            return result;

        }

    }

}
