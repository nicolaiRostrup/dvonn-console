

using System;
using System.Collections.Generic;

namespace Dvonn_Console
{

    class PositionTree
    {
        public Node root;
        public List<Node> currentEndPoints = new List<Node>();
        public uint totalNodes = 0;
        int nodeCounter = 0;

        public PositionTree(Position rootPosition)
        {
            root = new Node(rootPosition);
            root.depth = 0;
            currentEndPoints.Add(root);
            totalNodes++;

        }


        public void InsertChild(Node childNode, Node parent)
        {
            parent.children.Add(childNode);
            childNode.parent = parent;
            totalNodes++;

        }

        public void FinishBranching(Node parent)
        {
            currentEndPoints.Remove(parent);
            foreach (Node child in parent.children) currentEndPoints.Add(child);

        }


        public string DisplayTree(int maxNodes)
        {
            if (root == null) return "Tree is empty.";

            string result = "";

            nodeCounter = 0;
            while (nodeCounter < maxNodes)
            {
                result += PrintBranches(root);

            }

            return result;

        }

        string PrintBranches(Node thisNode)
        {
            string result = "";
            foreach (Node child in thisNode.children)
            {
                result += "Position #" + nodeCounter + " at depth " + thisNode.depth + " contains " + thisNode.position.NumberOfStacks() + " stacks and evaluates to " + thisNode.position.evaluation + "\n";
                nodeCounter++;
                result += PrintBranches(child);

            }

            return result;

        }

    }

}
