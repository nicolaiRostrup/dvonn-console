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
            //Console.WriteLine("In method: GoToFirstEndpointParent");
            //PrintNode();

            while (HasGrandChildren(currentNode))
            {
                Proceed();
            }

            HandleGroupOfEndPoints();

        }

        public void Proceed()
        {
            //Console.WriteLine("In method: Proceed");
            //PrintNode();

            int currentAlpha = currentNode.alpha;
            int currentBeta = currentNode.beta;

            currentNode = currentNode.children[0];
            currentNode.alpha = currentAlpha;
            currentNode.beta = currentBeta;

            proceedCounter++;

        }

        public void Proceed(int index)
        {
            //Console.WriteLine("In method: Proced(index)");
            //PrintNode();

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
            //Console.WriteLine("In method: GoToWardsRoot");
            //PrintNode();

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
            //PrintNode();
            Console.WriteLine("Current node name: " + currentNode.name);
            Console.WriteLine("Node alpha: " + currentNode.alpha);
            Console.WriteLine("Node beta: " + currentNode.beta);

            int measuredAlpha = currentNode.alpha;
            int measuredBeta = currentNode.beta;
            int index = GetNodeIndex(currentNode);

            currentNode = currentNode.parent;


            if (IsMaximumGeneration(currentNode))
            {
                if (measuredBeta > currentNode.alpha) currentNode.alpha = measuredBeta;
            }
            else
            {
                if (measuredAlpha < currentNode.beta) currentNode.beta = measuredAlpha;

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
            //PrintNode();
            Console.WriteLine("Parent of endpoints name: " + currentNode.name);
            Console.WriteLine("Parent alpha: " + currentNode.alpha);
            Console.WriteLine("Parent beta: " + currentNode.beta);

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

                while (ExistMoreEndnodes(childCounter))
                {
                    int value = CopyEndPointValue(childCounter);

                    if (IsMaximumGeneration(currentNode))
                    {
                        Console.WriteLine("maximum");
                        if (value > currentNode.alpha) currentNode.alpha = value;
                    }
                    else
                    {
                        Console.WriteLine("minimum");
                        if (value < currentNode.beta) currentNode.beta = value;

                    }

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
            Console.WriteLine("Visiting endnode: " + currentNode.children[i].name + ". Value: " + currentNode.children[i].testValue);
            return currentNode.children[i].testValue;

        }


        public void GoToNextSibling()
        {

            //Console.WriteLine("In method: GoToNextSibling");
            //PrintNode();

            int currentAlpha = currentNode.alpha;
            int currentBeta = currentNode.beta;
            int childIndex = GetNodeIndex(currentNode);
            currentNode = currentNode.parent;

            if (IsMaximumGeneration(currentNode))
            {
                if (currentBeta > currentNode.alpha) currentNode.alpha = currentBeta;
            }
            else
            {
                if (currentAlpha < currentNode.beta) currentNode.beta = currentAlpha;

            }
            backCounter++;

            if (currentNode.alpha >= currentNode.beta)
            {
                PruneRemainingSiblings(childIndex);
                GoTowardsRoot();
            }
            else
            {

                int childCurrentAlpha = currentNode.alpha;
                int childCurrentBeta = currentNode.beta;
                currentNode = currentNode.children[childIndex + 1];

                currentNode.alpha = childCurrentAlpha;
                currentNode.beta = childCurrentBeta;

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

        }


        public void PruneRemainingSiblings(int goodSibling)
        {
            if (goodSibling == currentNode.children.Count - 1)
            {
                PrintNode();
                Console.WriteLine("Checked for pruning, but nothing to Prune");

            }
            else
            {
                int firstChild = goodSibling + 1;
                int lastChild = currentNode.children.Count - 1;
                int range = lastChild - goodSibling;

                currentNode.children.RemoveRange(firstChild, range);
                pruneCounter += range;
                PrintNode();
                Console.WriteLine("Pruned no. of children: " + range);
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
                int lastIndex = currentNode.parent.children.Count - 1;
                if (childIndex < lastIndex) return true;
            }
            return false;
        }

        //public bool ExistMoreChildren(int index)
        //{
        //    int lastIndex = currentNode.parent.children.Count - 1;
        //    return index < lastIndex;
        //}

        public bool ExistMoreEndnodes(int index)
        {
            int lastIndex = currentNode.children.Count - 1;
            return index <= lastIndex;
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
