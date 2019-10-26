using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class AI
    {
        Board aiBoard = new Board();
        Rules ruleBook;
        PositionTree dvonnTree;
        int nodeCounter = 0;
        pieceID currentPlayer;

        void CreateTree(Position currentPosition, int maxDepth)
        {
            dvonnTree = new PositionTree(currentPosition);
            nodeCounter = 0;

            //computer allways has black, so branching begins with black...
            currentPlayer = pieceID.Black;

            for (int i = 0; i < maxDepth; i++)
            {

                Console.Write("AI: Branching begun at depth " + i );
                BranchAllEndpoints(currentPlayer);

            }


        }

        void BranchAllEndpoints(pieceID player)
        {
            List<Node> removableEndPoints = new List<Node>();
            List<Node> newEndPoints = new List<Node>();

            foreach (Node endPoint in dvonnTree.endPoints)
            {
                aiBoard.ReceivePosition(endPoint.position);
                ruleBook = new Rules(aiBoard);
                removableEndPoints.Add(endPoint);

                foreach (int sourceIndex in ruleBook.LegalSources(player))
                {
                    int sourceStackCount = aiBoard.entireBoard[sourceIndex].stack.Count;
                    List<int> legalTargets = ruleBook.LegalTargets(sourceIndex, sourceStackCount);

                    foreach (int targetIndex in legalTargets)
                    {
                        int[] moveCombo = { sourceIndex, targetIndex };
                        aiBoard.MakeMove(moveCombo, player);
                        Position newPosition = aiBoard.SendPosition();
                        Node childNode = dvonnTree.InsertChild(newPosition, endPoint);
                        newEndPoints.Add(childNode);
                        aiBoard.UndoMove(moveCombo, sourceStackCount);
                        nodeCounter++;

                    }

                }
            }

            foreach (Node node in removableEndPoints) dvonnTree.endPoints.Remove(node);
            foreach (Node node in newEndPoints) dvonnTree.endPoints.Add(node);

            Console.Write("AI: added " + nodeCounter + " positions to position tree.");
            nodeCounter = 0;
            AlternatePlayer();

        }

        void AlternatePlayer()
        {
            if (currentPlayer == pieceID.Black) currentPlayer = pieceID.White;
            else currentPlayer = pieceID.Black;

        }

        public float EvaluatePosition(Position position)
        {

            return 0f;

        }

    }
}
