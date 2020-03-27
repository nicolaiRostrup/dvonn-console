
using System.Collections.Generic;
using System.Text;

namespace Dvonn_Console
{

    class PositionTree
    {
        public Node root;
        public List<Node> currentEndPoints = new List<Node>();
        public List<GenerationAccount> generationAccounts = new List<GenerationAccount>();
        public long totalNodes = 0;
        public int depthReach = 0;


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

        public class GenerationAccount {

            public int generationNumber;
            public float minimumEvaluation;
            public float maximumEvaluation;
            public long totalNodeCount;
            public long stubs;
            public long evalutatedNodes;
        }


        //For developer purposes...
        public override string ToString()
        {

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
                long stubNodeCounter = 0L;
                float totalEvaluation = 0f;
                long totalStackCount = 0L;

                foreach (Node parent in parentNodes)
                {
                    foreach (Node child in parent.children)
                    {
                        if (child.isStub)
                        {
                            stubNodeCounter++;
                            continue;
                        }
                        generationNodeCounter++;
                        totalEvaluation += child.position.evaluation;
                        totalStackCount += child.position.NumberOfStacks();

                        childNodes.Add(child);
                    }

                }
                if (generationNodeCounter > 0)
                {
                    float meanEvaluation = totalEvaluation / generationNodeCounter;
                    float meanStackCount = totalStackCount / generationNodeCounter;

                    sb.AppendLine("At depth " + i + " is placed " + generationNodeCounter + " number of (non stub) nodes with a mean evaluation of: " + meanEvaluation + ", and a mean stack count of " + meanStackCount);

                }
                else
                {
                    sb.AppendLine("At depth " + i + " is placed " + childNodes.Count + " nodes, of which " + stubNodeCounter + " are stub nodes. ");
                }
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
