using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Board initBoard = new Board();
            BoardProperties.CalculatePrincipalMoves(initBoard);

            GameMaster controller = new GameMaster();
            controller.BeginNewGame();

        }
    }

}
