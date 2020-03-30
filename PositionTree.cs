
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
            root = new Node(rootPosition); //root has no move, because what came before the root position(resulting position of root node) is irrelevant to AI.
            currentEndPoints.Add(root);
            totalNodes++;
        }

        public void InsertChild(Node childNode, Node parent)
        {
            parent.children.Add(childNode);
            childNode.parent = parent;
            currentEndPoints.Add(childNode);
            totalNodes++;
        }

        public class GenerationAccount
        {
            public int generationNumber;
            public List<Node> parentNodes = new List<Node>();
            public long childCount = 0;

            public GenerationAccount(int generationNumber)
            {
                this.generationNumber = generationNumber;
            }
       
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

            foreach (GenerationAccount account in generationAccounts)
            {
                sb.AppendLine("At depth " + account.generationNumber + " is placed " + account.parentNodes.Count + " nodes, which have a total of " + account.childCount + " children nodes.");
            }
            double totalEvaluation = 0.0;
            long totalStackCount = 0L;
            foreach (Node endPoint in currentEndPoints)
            {
                totalEvaluation += endPoint.evaluation;
                totalStackCount += endPoint.resultingPosition.NumberOfStacks();
            }
            float meanEvaluation = (float)totalEvaluation / currentEndPoints.Count;
            float meanStackCount = (float)totalStackCount / currentEndPoints.Count;

            sb.AppendLine("The tree has  " + currentEndPoints.Count + " leaves.");
            sb.AppendLine("The leaves have a mean evaluation of: " + meanEvaluation);
            sb.AppendLine("The leaves have a mean stack count of: " + meanStackCount);

            return sb.ToString();

        }
    }
}
