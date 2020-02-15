using System;
using System.Collections.Generic;
using System.Linq;


namespace Dvonn_Console
{

    class GameMaster
    {
        Writer typeWriter = new Writer();
        Game dvonnGame;
        Board dvonnBoard;
        Rules ruleBook;
        StackInspector stackInspector;
        AI aiAgent;

        public void BeginNewGame()
        {
            typeWriter.WelcomeText();

            //todo: should be an option to choose these two booleans
            //todo: should be an option to choose color
            dvonnGame = new Game(true, true, PieceID.White);

            dvonnBoard = new Board();
            dvonnBoard.InstantiateFields();
            dvonnBoard.CalculatePrincipalMoves();
            ruleBook = new Rules(dvonnBoard);
            stackInspector = new StackInspector(dvonnBoard);
            
            if(dvonnGame.isAiDriven) aiAgent = new AI();
            if(dvonnGame.isRandomPopulated)
            {
                Position randomPosition = dvonnGame.RandomPopulateWithCorrection();
                //Position randomPosition = dvonnGame.RandomPopulate(3, 23, 23);
                dvonnBoard.ReceivePosition(randomPosition);
            }
            
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

                        Move chosenMove = GetUserMoveInput();

                        if (chosenMove.source == 0 && chosenMove.target == 0) // a special situation, where user wants to go back to menu
                        {
                            dvonnBoard.VisualizeBoard();
                            break;
                        }
                        else
                        {   
                            // else, if user doesn't want to go back to menu, execute move...
                            dvonnBoard.MakeMove(chosenMove);
                            dvonnBoard.VisualizeBoard();
                            typeWriter.MoveComment(chosenMove, PieceID.White);

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
                        if (ruleBook.PassCondition(PieceID.Black) == true)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Computer has no legal moves. It's your turn again.");
                            break;
                        }
                        // else computer gets to move:
                        Console.WriteLine();
                        Console.WriteLine("Black is ready to move");
                        WaitForUser();
                        if (dvonnGame.isAiDriven)
                        {
                            Move aiMove = aiAgent.ComputeAiMove(dvonnBoard.SendPosition(), PieceID.Black);
                            WaitForUser();
                            dvonnBoard.MakeMove(aiMove);
                            dvonnBoard.VisualizeBoard();
                            typeWriter.MoveComment(aiMove, PieceID.Black);
                        }
                        else
                        {
                            Move randomMove = CreateRandomMove(PieceID.Black);
                            dvonnBoard.MakeMove(randomMove);
                            dvonnBoard.VisualizeBoard();
                            typeWriter.MoveComment(randomMove, PieceID.Black);
                        }
                        //Do a check for dvonn collapse, and if true, execute and make comment.
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
                        if (ruleBook.PassCondition(PieceID.White) == true)
                        {
                            RepeatedRandomMove();
                            if (ruleBook.LegalMoves(PieceID.White) != 0) break; // If white should have gotten a new opportunity to move, return to main menu...
                            typeWriter.GameEndText(ruleBook.Score()); // Otherwise, the game is over...
                            gamerunning = false; // when returned, close console
                            break;
                        }
                        break;

                    //Inspect stack:
                    case "2":
                        Console.WriteLine("Please enter stack field to inspect:");
                        string request = Console.ReadLine().ToUpper();

                        while (request.Length != 2)
                        {
                            Console.WriteLine("Entered fieldname is not two characters. Please enter fieldnames like this: 'a0' or 'A0'");
                            request = Console.ReadLine().ToUpper();
                        }

                        while (!dvonnBoard.entireBoard.Any(field => field.fieldName == request))
                        {
                            Console.WriteLine("Field ID is not valid");
                            request = Console.ReadLine().ToUpper();
                        }

                        stackInspector.InspectStack(request);
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

                    case "7": // "Secret" option that lets developer create endgame scenario for test purposes
                        dvonnBoard.ClearBoard();
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

        public Move GetUserMoveInput()
        {
            Move chosenMove = new Move(0, 0, PieceID.White);
            string move;
            bool userInputCorrect = false;
            string[] sourceAndTarget;

            while (userInputCorrect == false)
            {
                chosenMove.source = 0; chosenMove.target = 0;

                Console.WriteLine("Please enter your move like this: a1/a2, where");
                Console.WriteLine("a1 is the source field, and a2 is the target field.");
                Console.WriteLine("Enter 'menu' to return to main menu");
                move = Console.ReadLine().ToUpper();

                if (move == "MENU")
                {
                    //If chosenmove is default (0,0) it is handled as a 'return to menu'
                    return chosenMove;
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
                   
                int source = dvonnBoard.entireBoard.First(field => field.fieldName == sourceAndTarget[0]).index;
                int target = dvonnBoard.entireBoard.First(field => field.fieldName == sourceAndTarget[1]).index;

                if (!ruleBook.LegalSources(PieceID.White).Contains(source))
                {
                    Console.WriteLine("The source field is not valid.");
                    Console.WriteLine("If in doubt, please consult Dvonn rules (enter 'rules')");
                    continue;
                }
                if (!ruleBook.LegalTargets(source, dvonnBoard.entireBoard[source].stack.Count).Contains(target))
                {
                    Console.WriteLine("The target field is not valid.");
                    Console.WriteLine("If in doubt, please consult Dvonn rules (enter 'rules')");
                    continue;
                }

                userInputCorrect = true;
                chosenMove = new Move(source, target, PieceID.White);
            }

            return chosenMove;

        }
        public Move CreateRandomMove(PieceID player)
        {
            Random rGen = new Random();

            List<int> legalSources = ruleBook.LegalSources(player);

            //Removes all the legal sources that do not have at least one legal target.
            List<int> trueLegalSources = legalSources.FindAll(src => ruleBook.LegalTargets(src, dvonnBoard.entireBoard[src].stack.Count).Count != 0);

            int randomSourceField = trueLegalSources[rGen.Next(0, trueLegalSources.Count)];
            List<int> legalTargets = ruleBook.LegalTargets(randomSourceField, dvonnBoard.entireBoard[randomSourceField].stack.Count);
            int randomTargetField = legalTargets[rGen.Next(0, legalTargets.Count)];

            Move randomMove = new Move(randomSourceField, randomTargetField, PieceID.Black);
           
            return randomMove;
        }

        public void RepeatedRandomMove()
        {
            Console.WriteLine("Human player has no legal moves, computer will continue playing");
            do
            {
                WaitForUser();
                Move randomMove = CreateRandomMove(PieceID.Black);
                dvonnBoard.MakeMove(randomMove); 
                dvonnBoard.VisualizeBoard();
                typeWriter.MoveComment(randomMove, PieceID.Black);

                //TODO: write how the game should end, game end text, etc...
                if (ruleBook.GameEndCondition() == true) return;

            } while (ruleBook.LegalMoves(PieceID.White) == 0);

        }

        public void WaitForUser()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

    }

}
