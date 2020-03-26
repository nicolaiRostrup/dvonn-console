
using System.Collections.Generic;
using System.Text;

namespace Dvonn_Console
{

    class PositionTree
    {
        public Node root;
        public List<Node> currentEndPoints = new List<Node>();
        public long totalNodes = 0;
        

        public PositionTree(Position rootPosition)
        {
            root = new Node(rootPosition);
            root.depth = 0;
            currentEndPoints.Add(root);
            totalNodes++;
            root.id = totalNodes;
        }


        public void InsertChild(Node childNode, Node parent)
        {
            parent.children.Add(childNode);
            childNode.parent = parent;
            childNode.depth = parent.depth + 1;
            currentEndPoints.Add(childNode);
            totalNodes++;
            childNode.id = totalNodes;
        }

        int GetDepthReach()
        {
            int depthCounter = 0;
            foreach (Node node in currentEndPoints)
            {
                if (node.depth > depthCounter) depthCounter = node.depth;

            }
            return depthCounter;
        }


        //For developer purposes...
        public override string ToString() {

            int depthReach = GetDepthReach();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("This is a report of the contents of current AI position tree");
            sb.AppendLine("The tree contains a total of " + totalNodes + " nodes.");
            sb.AppendLine("The tree has " + currentEndPoints.Count + " number of active endpoints.");
            sb.AppendLine("The tree has a depth reach of " + depthReach);
            sb.AppendLine("At depth 0, root position has an evaluation of: " + root.position.evaluation + " and contains " + root.position.NumberOfStacks() + " stacks");

            List<Node> parentNodes = new List<Node>();
            List<Node> childNodes = new List<Node>();
            parentNodes.Add(root);
            for (int i = 1; i <= depthReach; i++)
            {
                long generationNodeCounter = 0L;
                float totalEvaluation = 0f;
                long totalStackCount = 0L;
                
                foreach(Node parent in parentNodes)
                {
                    foreach(Node child in parent.children)
                    {
                        if (child.isStub) continue;
                        generationNodeCounter++;
                        totalEvaluation += child.position.evaluation;
                        totalStackCount += child.position.NumberOfStacks();

                        childNodes.Add(child);
                    }

                }
                float meanEvaluation = totalEvaluation / generationNodeCounter;
                float meanStackCount = totalStackCount / generationNodeCounter;

                sb.AppendLine("At depth " + i + " is placed " + generationNodeCounter + " number of nodes with a mean evaluation of: " + meanEvaluation + ", and a mean stack count of " + meanStackCount);

                parentNodes.Clear();
                foreach (Node child in childNodes)
                {
                    parentNodes.Add(child);
                }
                childNodes.Clear();
            }

            return sb.ToString();

        }

    }

}
