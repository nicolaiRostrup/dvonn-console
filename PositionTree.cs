
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dvonn_Console
{

    class PositionTree
    {
        public Node root = null;

        private List<GenerationAccount> generationAccounts = new List<GenerationAccount>();
        private List<Node> allLeaves = new List<Node>();
        private int depthCounter = 0;


        public PositionTree(Position rootPosition)
        {
            root = new Node(rootPosition); //root has no move, because what came before the root position(resulting position of root node) is irrelevant to AI.

        }

        //For test purposes...
        public PositionTree()
        {
            root = new Node();

        }

        public void InsertChild(Node childNode, Node parent)
        {
            parent.children.Add(childNode);
            childNode.parent = parent;
            childNode.depth = parent.depth +1;

        }

        private void GetGenerationAccounts(Node node)
        {
           
            GenerationAccount thisGeneration = generationAccounts.FirstOrDefault(account => account.depth == node.depth);

            if (thisGeneration == null)
            {
                GenerationAccount newGeneration = new GenerationAccount(node.depth);
                newGeneration.depthNodesCount++;
                newGeneration.childCount += node.children.Count;
                generationAccounts.Add(newGeneration);
            }
            else
            {
                thisGeneration.depthNodesCount++;
                thisGeneration.childCount += node.children.Count;

            }

            if (node.children.Count == 0)
            {
                return;
            }
            else
            {
                foreach (Node child in node.children)
                {
                    GetGenerationAccounts(child);
                }
            }

        }

        public int GetDepthReach()
        {
            depthCounter = 0;
            GetDepth(root);
            return depthCounter;
        }

        private void GetDepth(Node node)
        {
            if (node.children.Count == 0 && node.depth > depthCounter)
            {
                depthCounter = node.depth;
                return;
            }
            else
            {
                foreach (Node child in node.children)
                {
                    GetDepth(child);
                }
            }

        }

        private long GetTotalNodeCount()
        {
            long totalNodeCounter = 1L; //= root
            foreach(GenerationAccount account in generationAccounts)
            {
                totalNodeCounter += account.childCount;

            }
            return totalNodeCounter;
        }

        //Retrieves children in outermost generation (max depth).
        private long OuterLeavesCount()
        {
            return generationAccounts[generationAccounts.Count - 1].depthNodesCount;
        }

        public List<Node> GetAllLeaves()
        {
            allLeaves.Clear();
            GetLeaves(root);
            return allLeaves;
        }

        private void GetLeaves(Node node)
        {
            if (node.children.Count == 0)
            {
                allLeaves.Add(node);
            }
            else
            {
                foreach (Node child in node.children)
                {
                    GetLeaves(child);
                }
            }
            
        }

        public List<Node> GetParents(List<Node> theseNodes)
        {
            List<Node> parents = new List<Node>();
            foreach (Node node in theseNodes)
            {
                if ( !parents.Contains(node.parent)) parents.Add(node.parent);
            }
            return parents;
        }

        private class GenerationAccount
        {
            public int depth;
            public long depthNodesCount = 0L;
            public long childCount = 0L; //should equal depthNodesCount in next generation.

            public GenerationAccount(int depth)
            {
                this.depth = depth;
            }

        }


        //For developer purposes...
        public override string ToString()
        {
            generationAccounts.Clear();
            GetGenerationAccounts(root);
            List<Node> allLeaves = GetAllLeaves();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("This is a report of the contents of current AI position tree");
            sb.AppendLine("The tree contains a total of " + GetTotalNodeCount() + " nodes.");
            sb.AppendLine("The tree has " + allLeaves.Count + " number of active endpoints.");
            sb.AppendLine("The tree has " + OuterLeavesCount() + " endpoints in deepest depth.");
            sb.AppendLine("The tree has a depth reach of " + GetDepthReach());

            foreach (GenerationAccount account in generationAccounts)
            {
                sb.AppendLine("At depth " + account.depth + " is placed " + account.depthNodesCount + " nodes, which have a total of " + account.childCount + " children nodes.");
            }
            sb.AppendLine();

            if (allLeaves.Count <= 1)
            {
                sb.AppendLine("The tree currently has no leaves");

            }
            else
            {
                int leaveCounter = 1;
                foreach (Node leave in allLeaves)
                {
                    sb.AppendLine("Leave " + leaveCounter + ": has test evaluation:  " + leave.testValue + " and parent in generation: " + leave.parent.depth);
                    leaveCounter++;
                }
            }

            //long totalEvaluation = 0L;
            //long totalStackCount = 0L;
            //foreach (Node endPoint in currentEndPoints)
            //{
            //    totalEvaluation += endPoint.evaluation;
            //    totalStackCount += endPoint.resultingPosition.NumberOfStacks();
            //}
            //float meanEvaluation = (float)totalEvaluation / currentEndPoints.Count;
            //float meanStackCount = (float)totalStackCount / currentEndPoints.Count;

            //sb.AppendLine("The tree has  " + currentEndPoints.Count + " leaves.");
            //sb.AppendLine("The leaves have a mean evaluation of: " + meanEvaluation);
            //sb.AppendLine("The leaves have a mean stack count of: " + meanStackCount);

            return sb.ToString();

        }
    }
}
