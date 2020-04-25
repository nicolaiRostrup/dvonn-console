using System;


namespace Dvonn_Console
{
    class TreeCrawler
    {
        private PositionTree tree;
        private GamePhase gamePhase;
        private PieceID aiResponsibleColor;
        private Evaluator evaluator = new Evaluator();
        private Node currentNode = null;
        

        //for debug purposes:
        private int pruneCounter = 0;
        private int leavesBefore = 0;
        private int leavesAfter = 0;
        private int evaluatedPositions = 0;

        //for debug, regarding endpoint evaluations
        int minimumEvaluation = 0;
        int maximumEvaluation = 0;
        bool evaluationSpanInitiated = false;


        public TreeCrawler(PositionTree tree, GamePhase gamePhase, PieceID aiResponsibleColor)
        {
            this.tree = tree;
            this.gamePhase = gamePhase;
            this.aiResponsibleColor = aiResponsibleColor;
        }

        public void AlphaBetaPruning()
        {
            Console.WriteLine();
            Console.WriteLine("TreeCrawler: Alpha Beta Pruning begun");

            leavesBefore = tree.GetAllLeavesCount();
            GoToRoot();

            //Starts off a chain of methods, which when finished will have performed alpha beta pruning of tree.
            GoToFirstEndpointParent();

        }

        private void GoToRoot()
        {
            currentNode = tree.root;

        }

        private void GoToFirstEndpointParent()
        {
            while (HasGrandChildren(currentNode))
            {
                Proceed(0);
            }

            HandleGroupOfEndPoints();

        }

        private void Proceed(int index)
        {
            int currentAlpha = currentNode.alpha;
            int currentBeta = currentNode.beta;

            currentNode = currentNode.children[index];
            currentNode.alpha = currentAlpha;
            currentNode.beta = currentBeta;

        }

        //Eventually the tree crawler will in fact reach the root in this method, and the process will end.
        private void GoTowardsRoot()
        {
            while (ExistSibling() == false)
            {
                if (currentNode.parent == null)
                {
                    leavesAfter = tree.GetAllLeavesCount();
                    Console.WriteLine("TreeCrawler: Alpha Beta Pruning Complete");
                    Console.WriteLine("Pruned " + pruneCounter + " branches. ");
                    Console.WriteLine("Leaves before: " + leavesBefore);
                    Console.WriteLine("Leaves after: " + leavesAfter);
                    Console.WriteLine("AI: Evaluated " + evaluatedPositions + " positions");
                    Console.WriteLine("AI: Minimum evaluation value found: " + minimumEvaluation);
                    Console.WriteLine("AI: Maximum evaluation value found: " + maximumEvaluation);
                    return;
                }

                Back();

            }

            GoToNextSibling();

        }

        private void Back()
        {
            int measuredAlpha = currentNode.alpha;
            int measuredBeta = currentNode.beta;
            int index = GetNodeIndex(currentNode);

            currentNode = currentNode.parent;
            ProcessAlphaBetaValues(measuredAlpha, measuredBeta, index, true);

        }

        private void ProcessAlphaBetaValues(int measuredAlpha, int measuredBeta, int index, bool doPruning)
        {
            if (IsMaximumGeneration(currentNode))
            {
                if (measuredBeta > currentNode.alpha) currentNode.alpha = measuredBeta;
            }
            else
            {
                if (measuredAlpha < currentNode.beta) currentNode.beta = measuredAlpha;

            }
            if (doPruning)
            {
                if (currentNode.alpha >= currentNode.beta)
                {
                    PruneRemainingSiblings(index);
                }
            }

        }

        private void HandleGroupOfEndPoints()
        {
            int childCounter = 0;

            while (ExistMoreEndnodes(childCounter))
            {
                int value = EvaluatePosition(childCounter);

                if (IsMaximumGeneration(currentNode))
                {
                    if (value > currentNode.alpha) currentNode.alpha = value;
                }
                else
                {
                    if (value < currentNode.beta) currentNode.beta = value;

                }

                if (currentNode.alpha >= currentNode.beta && IsSingleChild(currentNode) == false)
                {
                    PruneRemainingSiblings(childCounter);
                }

                childCounter++;

            }
            GoTowardsRoot();
        }

        private int EvaluatePosition(int i)
        {
            int thisEval;
            Node endPoint = currentNode.children[i];

            thisEval = evaluator.EvaluatePosition(endPoint, gamePhase, aiResponsibleColor);
            endPoint.move.evaluation = thisEval;
            evaluatedPositions++;

            //for debug:
            if (evaluationSpanInitiated == false)
            {
                maximumEvaluation = thisEval;
                minimumEvaluation = thisEval;
                evaluationSpanInitiated = true;
            }
            else
            {
                if (thisEval > maximumEvaluation) maximumEvaluation = thisEval;
                if (thisEval < minimumEvaluation) minimumEvaluation = thisEval;
            }

            return thisEval;

        }

        private void GoToNextSibling()
        {
            int currentAlpha = currentNode.alpha;
            int currentBeta = currentNode.beta;
            int childIndex = GetNodeIndex(currentNode);
            currentNode = currentNode.parent;
            ProcessAlphaBetaValues(currentAlpha, currentBeta, childIndex, false);

            if (currentNode.alpha >= currentNode.beta)
            {
                PruneRemainingSiblings(childIndex);
                GoTowardsRoot();
            }

            else
            {
                Proceed(childIndex + 1);

                if (HasGrandChildren(currentNode) == false)
                {
                    HandleGroupOfEndPoints();
                }
                else
                {
                    GoToFirstEndpointParent();
                }

            }

        }


        private void PruneRemainingSiblings(int goodSibling)
        {
            if (goodSibling == currentNode.children.Count - 1)
            {
                //Console.WriteLine("AI: Checked for pruning, but nothing to Prune");

            }
            else
            {
                int firstChild = goodSibling + 1;
                int lastChild = currentNode.children.Count - 1;
                int range = lastChild - goodSibling;

                currentNode.children.RemoveRange(firstChild, range);
                pruneCounter += range;
                //Console.WriteLine("AI: Pruned no. of children: " + range);
            }
        }

        private bool IsMaximumGeneration(Node node)
        {
            return node.depth % 2 == 0;
        }

        private int GetNodeIndex(Node node)
        {
            return node.parent.children.IndexOf(node);
        }

        private bool HasGrandChildren(Node node)
        {
            if (node.children.Count == 0) return false;
            else if (node.children[0].children.Count > 0) return true;
            else return false;
        }

        private bool ExistParent()
        {
            return currentNode.parent != null;
        }

        private bool ExistSibling()
        {
            if (ExistParent())
            {
                int childIndex = GetNodeIndex(currentNode);
                int lastIndex = currentNode.parent.children.Count - 1;
                if (childIndex < lastIndex) return true;
            }
            return false;
        }

        private bool ExistMoreEndnodes(int index)
        {
            int lastIndex = currentNode.children.Count - 1;
            return index <= lastIndex;
        }

        private bool IsEndNode(Node node)
        {
            return node.children.Count == 0;
        }

        private bool IsSingleChild(Node node)
        {
            if (node == tree.root) return true;
            else return node.parent.children.Count == 1;

        }

    }
}