using System;


namespace Dvonn_Console
{
    class Writer
    {

        string[] fieldCoordinates = { "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CT", "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8" }; 
        public void WelcomeText()
        {
            Console.WriteLine();
            Console.WriteLine("*****************************************************");
            Console.WriteLine("Welcome to Dvonn");
            Console.WriteLine("- a mind-bending abstract boardgame");
            Console.WriteLine();
            Console.WriteLine("C# application developed by Nicolai Rostrup, 2020");
            Console.WriteLine("Original game design by Kris Burm, 2001");
            Console.WriteLine("*****************************************************");
            Console.WriteLine();
            Console.WriteLine("Version 5.0, computer optionally plays with AI.");
            Console.WriteLine("Human player is always white. White always begins.");

            WaitForUser();
        }


        public void Coordinates()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    (a0)(a1)(a2)(a3)(a4)(a5)(a6)(a7)(a8)");
            Console.WriteLine("  (b0)(b1)(b2)(b3)(b4)(b5)(b6)(b7)(b8)(b9)");
            Console.WriteLine("(c0)(c1)(c2)(c3)(c4)(c5)(c6)(c7)(c8)(c9)(ct)");
            Console.WriteLine("  (d0)(d1)(d2)(d3)(d4)(d5)(d6)(d7)(d8)(d9)");
            Console.WriteLine("    (e0)(e1)(e2)(e3)(e4)(e5)(e6)(e7)(e8)");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
        }
        public void MenuText()
        {
            Console.WriteLine();
            Console.WriteLine("Please select function:");
            Console.WriteLine("1 Enter move");
            Console.WriteLine("2 Inspect stack");
            Console.WriteLine("3 Calculate score");
            Console.WriteLine("4 Visualize board");
            Console.WriteLine("5 Read the rules");
            Console.WriteLine("6 Exit");
            Console.WriteLine();

        }
        public void Rules()
        {
            Console.WriteLine();
            Console.WriteLine("Equipment: \n Dvonn is played on a board with 49 spaces.One player has 23 black pieces to play, the other player has 23 white pieces.There are also 3 red pieces, called Dvonn pieces.These pieces are neutral and has special powers.");
            Console.WriteLine();
            Console.WriteLine("Object: \n The object of the game is to control more pieces than your opponent at the end of the game. You are in control of a stack of pieces if your color is on top, regardless of any pieces below.You receive points per total number of pieces in stack – not only your own color.");
            Console.WriteLine();
            Console.WriteLine("Placement phase: \n This version of Dvonn automatically places all pieces on the board in a randomized manner.");
            Console.WriteLine();
            Console.WriteLine("Movement phase: \n In this version of Dvonn White always begins. The movement phase involves the building of stacks of pieces (a single piece is also considered a stack) by moving stacks onto other stacks. A stack is controlled by a player if his color is on top.");
            Console.WriteLine("A stack is immobile if it is surrounded by 6 neighboring stacks. Any mobile stack of height n (with n > 0) can be moved(in a straight line) in any one of the 6 directions by exactly n spaces by the player controlling it, if it lands on another stack. Jumping over empty spaces is allowed, as long as the tower does not land on an empty space.");
            Console.WriteLine("Single Dvonn pieces can not be moved, but they can be once they are part of a stack. After each move all stacks which are not connected via a chain of neighboring stacks to any stack containing a Dvonn piece are removed from the board.");
            Console.WriteLine();
            Console.WriteLine("Passing and game end: \n A player who has no legal move must pass, and a player may only pass when no legal move is available.The game ends when both players have no legal moves.All stacks controlled by one player are collected into one tower.The winner is the player with the higher tower. The game ends in a draw in case both players own an equal number of stones in their tower.");
            Console.WriteLine();
            Console.WriteLine("Strategy: \n Capturing pieces that could capture a Dvonn piece can be important, as a moving Dvonn piece controlled by the opponent can isolate a large group of pieces. However, mobility is the most important aspect of Dvonn; one needs to keep one's options open. Building a tall stack early in the movement phase is a mistake. Most often, the game is won by the player who is capable of making the last move(s). Maintaining some single stones until the end phase is, therefore, very often a good strategy.");
            Console.WriteLine("The first few moves should correct any place where your position must be fixed: helping or moving pieces that could become isolated and moving outside pieces in order to give inside ones mobility. A general rule for selecting which move to make is that you should not make a move which the opponent cannot prevent you from making at a later time.");
            Console.WriteLine("Taking a Dvonn piece is especially desirable if it is possible to move it toward one's own pieces and away from one's opponent's. However, every capture counts, so big captures and maintaining mobility is more important than just taking Dvonn pieces. After about 20 moves the board clears and settles. It becomes much more important to look ahead to see how every move affects the overall situation.As the game progresses and the board stagnates, precise deep calculation becomes crucial.");
            Console.WriteLine();
            Console.WriteLine("Dvonn looks like a simple game, but is really a serious mind bender. Please, enjoy…");
        }

        public void DvonnCollapseText(int[] account)
        {
            Console.WriteLine();
            Console.WriteLine("A Dvonn collapse has happened: \n" + account[0] + " stacks has been removed, containing " + account[1] + " pieces.");
            Console.WriteLine();
            WaitForUser();
            
        }


        public void GameEndText(int[] gameScore)
        {
            //int[] gameScore = Calculate.Score();

            if (gameScore[2] == 0)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("*****************************************************");
                Console.WriteLine("The game is over");
                Console.WriteLine("Human player has won");
                Console.WriteLine("The end score was: ");
                Console.WriteLine("White: {0} \t Black: {1}", gameScore[0], gameScore[1]);
                Console.WriteLine();
                Console.WriteLine("*****************************************************");
            }
            if (gameScore[2] == 1)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("*****************************************************");
                Console.WriteLine("The game is over");
                Console.WriteLine("Computer has won, playing random moves");
                Console.WriteLine("The end score was: ");
                Console.WriteLine("White: {0} \t Black: {1}", gameScore[0], gameScore[1]);
                Console.WriteLine();
                Console.WriteLine("*****************************************************");
            }
            if (gameScore[2] == 2)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("*****************************************************");
                Console.WriteLine("The game is over");
                Console.WriteLine("The game was a tie.");
                Console.WriteLine("The end score was: ");
                Console.WriteLine("White: {0} \t Black: {1}", gameScore[0], gameScore[1]);
                Console.WriteLine();
                Console.WriteLine("*****************************************************");
            }
            WaitForUser();
        }

        public void WaitForUser()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public void MoveComment(Move thisMove, PieceID Color)
        {
            Console.WriteLine();
            Console.WriteLine(Color.ToString() + " move, " + fieldCoordinates[thisMove.source] + " / " + fieldCoordinates[thisMove.target] + " has been executed.");

        }
        

    }
}
