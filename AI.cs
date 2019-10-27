using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class AI
    {
        Board aiBoard;
        Rules ruleBook;
        PositionTree dvonnTree;
        int nodeCounter = 0;
        PieceID currentPlayer;

        public AI()
        {
            aiBoard = new Board();
            aiBoard.InstantiateFields();
            aiBoard.CalculatePrincipalMoves();
            ruleBook = new Rules(aiBoard);
        }

        public void CreateTree(Position currentPosition, int maxDepth)
        {

            dvonnTree = new PositionTree(currentPosition);
            nodeCounter = 0;

            //computer allways has black, so branching begins with black...
            currentPlayer = PieceID.Black;

            for (int i = 0; i < maxDepth; i++)
            {

                Console.Write("AI: Branching begun at depth " + (i + 1) + "\n");
                BranchAllEndpoints(currentPlayer, i + 1);

            }


        }

        void BranchAllEndpoints(PieceID player, int depth)
        {
            List<Node> currentEndPoints = new List<Node>();

            foreach(Node node in dvonnTree.allNodes)
            {
                if (node.children.Count == 0) currentEndPoints.Add(node);
            }
            Console.Write("Endpoints found: " + currentEndPoints.Count + "\n");

            foreach (Node endPoint in currentEndPoints)
            {
                aiBoard.ReceivePosition(endPoint.position);
                List<int> legalSources = ruleBook.LegalSources(player);
                //Removes all the legal sources that do not have at least one legal target.
                List<int> trueLegalSources = legalSources.FindAll(src => ruleBook.LegalTargets(src, aiBoard.entireBoard[src].stack.Count).Count != 0);

                foreach (int sourceIndex in trueLegalSources)
                {   
                    int sourceStackCount = aiBoard.entireBoard[sourceIndex].stack.Count;
                    List<int> legalTargets = ruleBook.LegalTargets(sourceIndex, sourceStackCount);

                    if(nodeCounter % 1000 == 0) Console.WriteLine("Possible moves count for generation " + depth + " sample = " + trueLegalSources.Count * legalTargets.Count );

                    foreach (int targetIndex in legalTargets)
                    {
                        int[] moveCombo = { sourceIndex, targetIndex };
                        aiBoard.MakeMove(moveCombo);

                        Position newPosition = aiBoard.SendPosition();
                        dvonnTree.InsertChild(newPosition, endPoint, depth);
                        aiBoard.UndoMove(moveCombo, sourceStackCount);
                        nodeCounter++;
                        if (nodeCounter > 4000000) return;

                    }
                    

                }
                
            }

            Console.Write("AI: added " + nodeCounter + " positions to position tree for color " + player + "\n");
            Console.Write("AI: current number of endpoints in tree = " + currentEndPoints.Count + "\n");
            Console.Write("AI: total number of nodes in tree = " + dvonnTree.allNodes.Count + "\n");
            nodeCounter = 0;
            AlternatePlayer();

        }

        public string PrintTree(int maxNodes)
        {
            return dvonnTree.DisplayTree(maxNodes);
        }

        void AlternatePlayer()
        {
            if (currentPlayer == PieceID.Black) currentPlayer = PieceID.White;
            else currentPlayer = PieceID.Black;

        }

        public float EvaluatePosition(Position position)
        {

            return 0f;

        }

    }
}
