using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dvonn_Console
{


    class Controller
    {
        Writer typeWriter = new Writer();
        Game dvonnGame;
        Board dvonnBoard;
        Rules ruleBook;



        public void BeginNewGame()
        {
            typeWriter.WelcomeText();

            dvonnBoard = new Board();
            dvonnBoard.InstantiateFields();
            dvonnGame = new Game();
            Position randomPosition = dvonnGame.RandomPopulate(3, 23, 23);
            dvonnBoard.ReceivePosition(randomPosition);

            ruleBook = new Rules();
            ruleBook.dvonnBoard = dvonnBoard;

            dvonnBoard.CalculatePrincipalMoves();
            dvonnBoard.VisualizeBoard();

            RunGameMenu();
        }

        public void RunGameMenu()
        {
            bool gamerunning = true;

            while (gamerunning)
            {

                typeWriter.MenuText();

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": // Enter move ...

                        int[] moveCombo = GetUserMoveInput();

                        if (moveCombo[0] == 0 && moveCombo[1] == 0) // a special situation, where user wants to go back to menu
                        {
                            dvonnBoard.VisualizeBoard();
                            break;
                        }
                        else
                        {
                            dvonnBoard.MakeMove(moveCombo, pieceID.White); // else, if user doesn't want to go back to menu, execute move...
                            dvonnBoard.VisualizeBoard();

                        }
                        //Do a check for dvonn collapse, and if true, execute and make comment.
                        ruleBook.CheckDvonnCollapse();

                        //Check whether game has ended. 
                        if (ruleBook.GameEndCondition() == true)
                        {
                            typeWriter.GameEndText(ruleBook.Score());
                            gamerunning = false;
                            break; // when returned, close console
                        }

                        //Check if black has any legal moves
                        if (ruleBook.PassCondition(pieceID.Black) == true)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Computer has no legal moves. It's your turn again.");
                            break;
                        }
                        // else computer gets to move:
                        Console.WriteLine();
                        Console.WriteLine("Black is ready to move");
                        WaitForUser();
                        dvonnBoard.MakeMove(CreateRandomMove(), pieceID.Black);
                        dvonnBoard.VisualizeBoard();
                        ruleBook.CheckDvonnCollapse();

                        //Again, this time after blacks move, check whether game has ended.
                        if (ruleBook.GameEndCondition() == true)
                        {
                            WaitForUser();
                            typeWriter.GameEndText(ruleBook.Score());
                            gamerunning = false; // when returned, close console
                            break;
                        }

                        //Check if white has any legal moves
                        if (ruleBook.PassCondition(pieceID.White) == true)
                        {
                            RepeatedRandomMove();
                            if (ruleBook.LegalMoves(pieceID.White) != 0) break; // Hvis hvid evt. skulle have fået et gyldigt træk, føres program pointeren til main menu...
                            typeWriter.GameEndText(ruleBook.Score()); // Ellers er spillet slut.
                            gamerunning = false; // when returned, close console
                            break;
                        }
                        break;

                    case "2":
                        //TODO: confer with user to get intended field id.
                        dvonnBoard.InspectStack(0);
                        break;

                    case "3":
                        Console.WriteLine("If game was to end now, the score would be: ");
                        Console.WriteLine("White: {0} \t Black: {1}", ruleBook.Score()[0], ruleBook.Score()[1]);
                        Console.WriteLine();
                        break;

                    case "4": // Visualize board
                        dvonnBoard.VisualizeBoard();
                        break;

                    case "5": //Read the rules
                        typeWriter.Rules();
                        break;

                    case "6": //Exit
                        gamerunning = false;
                        break;

                    case "7": // "Secret" option that lets user create endgame scenario for test purposes
                        dvonnBoard.Clear();
                        Position partialDvonnGame = dvonnGame.RandomPopulate(3, 8, 8);
                        dvonnBoard.ReceivePosition(partialDvonnGame);
                        ruleBook.CheckDvonnCollapse();
                        break;

                    default:
                        Console.WriteLine("Unable to read input, please try again.");
                        break;

                }
            }

        }

        public int[] GetUserMoveInput()
        {
            int[] moveCombo = { 0, 0 };
            string move;
            bool userInputCorrect = false;
            string[] sourceAndTarget;

            while (userInputCorrect == false)
            {
                moveCombo[0] = 0; moveCombo[1] = 0;

                Console.WriteLine("Please enter your move like this: a1/a2, where");
                Console.WriteLine("a1 is the source field, and a2 is the target field.");
                Console.WriteLine("Enter 'menu' to return to main menu");
                move = Console.ReadLine().ToUpper();

                if (move == "MENU")
                {
                    return moveCombo;
                }
                if (move.Equals("RULES"))
                {
                    typeWriter.Rules();
                    WaitForUser();
                    dvonnBoard.VisualizeBoard();
                    Console.WriteLine();
                    continue;
                }
                if (move.Length != 5 || !move.Contains("/"))
                {
                    Console.WriteLine("Your input does not compute.");
                    continue;
                }

                char delimiterChar = '/';
                sourceAndTarget = move.Split(delimiterChar);

                if (!dvonnBoard.entireBoard.Any(field => field.fieldName == sourceAndTarget[0]))
                {
                    Console.WriteLine("Source field is not entered correctly.");
                    continue;
                }
                if (!dvonnBoard.entireBoard.Any(field => field.fieldName == sourceAndTarget[1]))
                {
                    Console.WriteLine("Target field is not entered correctly.");
                    continue;
                }

                moveCombo[0] = dvonnBoard.entireBoard.First(field => field.fieldName == sourceAndTarget[0]).index;
                moveCombo[1] = dvonnBoard.entireBoard.First(field => field.fieldName == sourceAndTarget[1]).index;

                if (!ruleBook.LegalSources(pieceID.White).Contains(moveCombo[0]))
                {
                    Console.WriteLine("The source field is not valid.");
                    Console.WriteLine("If in doubt, please consult Dvonn rules (enter 'rules')");
                    continue;
                }
                if (!ruleBook.LegalTargets(moveCombo[0], dvonnBoard.entireBoard[moveCombo[0]].stack.Count).Contains(moveCombo[1]))
                {
                    Console.WriteLine("The target field is not valid.");
                    Console.WriteLine("If in doubt, please consult Dvonn rules (enter 'rules')");
                    continue;
                }

                userInputCorrect = true;
            }

            return moveCombo; 

        }
        public int[] CreateRandomMove()
        {
            Random rGen = new Random();

            List<int> legalSources = ruleBook.LegalSources(pieceID.Black);

            //Removes all the legas sources that do not have at least one legal target.
            List<int> trueLegalSources = legalSources.FindAll(src => ruleBook.LegalTargets(src, dvonnBoard.entireBoard[src].stack.Count).Count != 0);

            int randomSourceField = trueLegalSources[rGen.Next(0, trueLegalSources.Count)];
            List<int> legalTargets = ruleBook.LegalTargets(randomSourceField, dvonnBoard.entireBoard[randomSourceField].stack.Count);
            int randomTargetField = legalTargets[rGen.Next(0, legalTargets.Count)];
            int[] randomMove = { randomSourceField, randomTargetField };

            return randomMove;

        }

        public void RepeatedRandomMove()
        {
            Console.WriteLine("Human player has no legal moves, computer will continue playing");
            do
            {
                WaitForUser();
                dvonnBoard.MakeMove(CreateRandomMove(), pieceID.Black);
                dvonnBoard.VisualizeBoard();

                //TODO: write how the game should end, game end text, etc...
                if (ruleBook.GameEndCondition() == true) return;

            } while (ruleBook.LegalMoves(pieceID.White) == 0);

        }

        public void WaitForUser()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }



    }

}
