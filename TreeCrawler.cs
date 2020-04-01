using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class TreeCrawler
    {
        private Node currentNode = null;
        public PositionTree tree;
        private int pruneCounter = 0;

        private int backCounter = 0;
        private int proceedCounter = 0;


        public TreeCrawler(PositionTree tree)
        {
            this.tree = tree;
        }

        public void GoToRoot()
        {
            currentNode = tree.root;

        }

        public void GoToFirstEndpointParent()
        {
            Console.WriteLine("In method: GoToFirstEndpointParent");
            PrintNode();

            while (HasGrandChildren(currentNode))
            {
                Proceed();
            }

            HandleGroupOfEndPoints();

        }

        public void Proceed()
        {
            Console.WriteLine("In method: Proceed");
            PrintNode();

            int currentAlpha = currentNode.alpha;
            int currentBeta = currentNode.beta;

            currentNode = currentNode.children[0];
            currentNode.alpha = currentAlpha;
            currentNode.beta = currentBeta;

            proceedCounter++;

        }

        public void Proceed(int index)
        {
            Console.WriteLine("In method: Proced(index)");
            PrintNode();

            int currentAlpha = currentNode.alpha;
            int currentBeta = currentNode.beta;

            currentNode = currentNode.children[index];
            currentNode.alpha = currentAlpha;
            currentNode.beta = currentBeta;

            proceedCounter++;

            GoToFirstEndpointParent();

        }

        public void GoTowardsRoot()
        {
            Console.WriteLine("In method: GoToWardsRoot");
            PrintNode();

            while (ExistSibling() == false)
            {
                if (currentNode.parent == null)
                {
                    Console.WriteLine("TreeCrawler: Alpha Beta Pruning Complete");
                    Console.WriteLine("Pruned " + pruneCounter + " branches. ");
                    Console.WriteLine("Proceeded " + proceedCounter + " times. ");
                    Console.WriteLine("Backed " + backCounter + " times. ");
                    return;
                }

                Back();

            }

            GoToNextSibling();

        }

        public void Back()
        {
            Console.WriteLine("In method: Back");
            PrintNode();

            int measuredAlpha = currentNode.alpha;
            int measuredBeta = currentNode.beta;
            int index = GetNodeIndex(currentNode);

            currentNode = currentNode.parent;

            if (IsMaximumGeneration(currentNode))
            {
                if (measuredAlpha > currentNode.alpha) currentNode.alpha = measuredAlpha;
            }
            else
            {
                if (measuredBeta < currentNode.beta) currentNode.beta = measuredBeta;

            }

            if (currentNode.alpha >= currentNode.beta)
            {
                PruneRemainingSiblings(index);
            }

            backCounter++;

        }

        //Starts off a chain of methods, which when finished will have performed alpha beta pruning of tree.
        public void AlphaBetaPruning()
        {
            GoToRoot();

            GoToFirstEndpointParent();

        }

        private void HandleGroupOfEndPoints()
        {
            Console.WriteLine("In method: HandleGroupOfEndPoints");
            PrintNode();

            if (currentNode.parent.children.Count == 1)
            {
                int value = CopyEndPointValue(0);

                if (IsMaximumGeneration(currentNode))
                {
                    if (value > currentNode.alpha) currentNode.alpha = value;
                }
                else
                {
                    if (value < currentNode.beta) currentNode.beta = value;

                }

            }
            else
            {
                int childCounter = 0;

                while (ExistMoreChildren(childCounter))
                {

                    int value = CopyEndPointValue(childCounter);

                    if (IsMaximumGeneration(currentNode))
                    {
                        if (value > currentNode.alpha) currentNode.alpha = value;
                    }
                    else
                    {
                        if (value < currentNode.beta) currentNode.beta = value;

                    }
                    //Actual alpha beta pruning
                    if (currentNode.alpha >= currentNode.beta)
                    {
                        PruneRemainingSiblings(childCounter);
                    }
                    childCounter++;

                }
            }

            GoTowardsRoot();
        }

        private int CopyEndPointValue(int i)
        {
            return currentNode.children[i].testValue;

        }


        public void GoToNextSibling()
        {

            Console.WriteLine("In method: GoToNextSibling");
            PrintNode();

            int currentAlpha = currentNode.alpha;
            int currentBeta = currentNode.beta;
            int childIndex = GetNodeIndex(currentNode);
            currentNode = currentNode.parent;

            backCounter++;

            currentNode = currentNode.children[childIndex + 1];
            currentNode.alpha = currentAlpha;
            currentNode.beta = currentBeta;

            proceedCounter++;

            if (HasGrandChildren(currentNode) == false)
            {
                HandleGroupOfEndPoints();
            }
            else
            {
                GoToFirstEndpointParent();
            }

        }


        public void PruneRemainingSiblings(int lastSibling)
        {
            int firstChild = lastSibling + 1;
            int range = (currentNode.children.Count - 1) - firstChild;

            currentNode.children.RemoveRange(firstChild, range);
            pruneCounter += range;
            Console.WriteLine("Pruned no. of children: " + range);

        }

        private bool IsMaximumGeneration(Node node)
        {
            return node.depth % 2 == 0;
        }

        private int GetNodeIndex(Node node)
        {
            return node.parent.children.IndexOf(node);
        }

        public bool HasGrandChildren(Node node)
        {
            if (node.children.Count == 0) return false;
            else if (node.children[0].children.Count > 0) return true;
            else return false;
        }

        public bool ExistParent()
        {
            return currentNode.parent != null;
        }

        public bool ExistSibling()
        {
            if (ExistParent())
            {
                int childIndex = GetNodeIndex(currentNode);
                if (ExistMoreChildren(childIndex)) return true;
            }
            return false;
        }

        public bool ExistMoreChildren(int index)
        {
            int lastIndex = currentNode.parent.children.Count - 1;
            return index < lastIndex;
        }

        public bool IsEndNode(Node node)
        {
            return node.children.Count == 0;
        }


        public void PrintNode()
        {
            Console.WriteLine(currentNode.ToString());
        }


    }
}
