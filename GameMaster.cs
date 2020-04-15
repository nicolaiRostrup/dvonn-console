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

            if (dvonnGame.isAiDriven) aiAgent = new AI();
            if (dvonnGame.isRandomPopulated)
            {
                Position randomPosition = dvonnGame.RandomPopulateWithCorrection();
                dvonnGame.openingPosition = randomPosition;
                dvonnBoard.ReceivePosition(randomPosition);
            }

            dvonnBoard.VisualizeBoard();
            dvonnGame.timeBegun = DateTime.Now;
            RunGameMenu();
        }

        public void RunGameMenu()
        {
            bool gamerunning = true;
            PreMove premovePlayer = ruleBook.ManufacturePreMove(PieceID.White);
            PreMove premoveComputer;

            while (gamerunning)
            {
                typeWriter.MenuText();

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": // Enter move ...

                        Move chosenMove = GetUserMoveInput(premovePlayer);

                        if (chosenMove.source == 0 && chosenMove.target == 0) // a special situation, where user wants to go back to menu
                        {
                            dvonnBoard.VisualizeBoard();
                            break;
                        }
                        else
                        {
                            // else, if user doesn't want to go back to menu, execute move...
                            dvonnBoard.MakeMove(chosenMove);
                            dvonnGame.gameMoveList.Add(chosenMove);
                            dvonnBoard.VisualizeBoard();
                            typeWriter.MoveComment(chosenMove, PieceID.White);

                        }
                        //Do a check for dvonn collapse, and if true, execute and make comment.
                        ruleBook.CheckDvonnCollapse(chosenMove, true);

                        //After White's move both players' options need to be analyzed.
                        premovePlayer = ruleBook.ManufacturePreMove(PieceID.White);
                        premoveComputer = ruleBook.ManufacturePreMove(PieceID.Black);

                        //Check whether game has ended. 
                        if (ruleBook.GameEndCondition(premovePlayer, premoveComputer) == true)
                        {
                            DoEndOFGameRoutine();
                            gamerunning = false;
                            break; // when returned, close console
                        }
                        
                        //Check if black has any legal moves
                        if (ruleBook.PassCondition(premoveComputer) == true)
                        {
                            dvonnGame.gameMoveList.Add(new Move(PieceID.Black));
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
                            dvonnBoard.MakeMove(aiMove);
                            dvonnGame.gameMoveList.Add(aiMove);
                            dvonnBoard.VisualizeBoard();
                            typeWriter.MoveComment(aiMove, PieceID.Black);
                            ruleBook.CheckDvonnCollapse(aiMove, true);
                        }
                        else
                        {
                            Move randomMove = PickRandomMove(premoveComputer);
                            dvonnBoard.MakeMove(randomMove);
                            dvonnGame.gameMoveList.Add(randomMove);
                            dvonnBoard.VisualizeBoard();
                            typeWriter.MoveComment(randomMove, PieceID.Black);
                            ruleBook.CheckDvonnCollapse(randomMove, true);
                        }

                        //After Black's move both players' options need to be analyzed.
                        premovePlayer = ruleBook.ManufacturePreMove(PieceID.White);
                        premoveComputer = ruleBook.ManufacturePreMove(PieceID.Black);

                        //Again, this time after blacks move, check whether game has ended.
                        if (ruleBook.GameEndCondition(premovePlayer, premoveComputer) == true)
                        {
                            DoEndOFGameRoutine();
                            gamerunning = false;
                            break; // when returned, close console
                        }

                        //Check if white has any legal moves
                        if (ruleBook.PassCondition(premovePlayer) == true)
                        {
                            PreMove newMoveOption = RepeatedRandomMove(premoveComputer);
                            if (newMoveOption == null)
                            {
                                DoEndOFGameRoutine();
                                gamerunning = false; // when returned, close console
                                break;
                            }
                            else
                            {
                                // If white should have gotten a new opportunity to move, return to main menu...
                                premovePlayer = newMoveOption;
                                break;
                            }

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
                        Console.WriteLine("White: {0} \t Black: {1}", ruleBook.GetScore(PieceID.White), ruleBook.GetScore(PieceID.Black));
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
                        //Position partialDvonnGame = dvonnGame.RandomPopulate(3, 8, 8);
                        Position partialDvonnGame = new Position();
                        partialDvonnGame.stacks[1] = "BW";
                        partialDvonnGame.stacks[2] = "WB";
                        partialDvonnGame.stacks[3] = "WW";
                        partialDvonnGame.stacks[4] = "WW";
                        partialDvonnGame.stacks[5] = "BDBWB";
                        partialDvonnGame.stacks[6] = "BWW";
                        partialDvonnGame.stacks[7] = "BBB";
                        partialDvonnGame.stacks[11] = "WWBB";
                        partialDvonnGame.stacks[12] = "WBW";
                        partialDvonnGame.stacks[13] = "W";
                        partialDvonnGame.stacks[14] = "WW";
                        partialDvonnGame.stacks[15] = "BB";
                        partialDvonnGame.stacks[16] = "WW";
                        partialDvonnGame.stacks[21] = "W";
                        partialDvonnGame.stacks[22] = "WDBBB";
                        partialDvonnGame.stacks[23] = "W";
                        partialDvonnGame.stacks[24] = "W";
                        partialDvonnGame.stacks[24] = "B";
                        partialDvonnGame.stacks[24] = "B";
                        partialDvonnGame.stacks[38] = "DW";
                        partialDvonnGame.stacks[39] = "WW";
                        partialDvonnGame.stacks[46] = "W";
                        partialDvonnGame.stacks[47] = "WBWW";
                        partialDvonnGame.stacks[48] = "W";


                        dvonnBoard.ReceivePosition(partialDvonnGame);
                        premovePlayer = ruleBook.ManufacturePreMove(PieceID.White);
                        dvonnBoard.VisualizeBoard();
                        ruleBook.CheckDvonnCollapse(null, false);
                        break;

                    case "8": //Auto finish
                        AutoFinish();
                        gamerunning = false;
                        break;

                    default:
                        Console.WriteLine("Unable to read input, please try again.");
                        break;

                }
            }

        }

        private void DoEndOFGameRoutine()
        {
            int whiteScore = ruleBook.GetScore(PieceID.White);
            int blackScore = ruleBook.GetScore(PieceID.Black);
            dvonnGame.timeEnded = DateTime.Now;
            dvonnGame.gameResultWhite = whiteScore;
            dvonnGame.gameResultBlack = blackScore;
            typeWriter.GameEndText(whiteScore, blackScore);
            Console.WriteLine(dvonnGame.ToString());
            WaitForUser();
        }

        private void AutoFinish()
        {
            PieceID playerToMove = dvonnGame.humanPlayerColor;
            PreMove currentAiPreMove;
            PreMove opponentPremove;

            Console.WriteLine();
            Console.WriteLine("AI will now finish the game");
            WaitForUser();

            do
            {
                currentAiPreMove = ruleBook.ManufacturePreMove(playerToMove);
                if (currentAiPreMove.legalMoves.Count > 0)
                {
                    Move randomMove = PickRandomMove(currentAiPreMove);
                    dvonnBoard.MakeMove(randomMove);
                    dvonnGame.gameMoveList.Add(randomMove);
                    ruleBook.CheckDvonnCollapse(randomMove, false);
                }
                opponentPremove = ruleBook.ManufacturePreMove(playerToMove.ToOpposite());

                if(currentAiPreMove.legalMoves.Count == 0 && ruleBook.GameEndCondition(currentAiPreMove, opponentPremove) == false)
                {
                    dvonnGame.gameMoveList.Add(new Move(currentAiPreMove.responsibleColor));
                }
                
                playerToMove = playerToMove.ToOpposite();

            } while (ruleBook.GameEndCondition(currentAiPreMove, opponentPremove ) == false);
            
            dvonnBoard.VisualizeBoard();
            DoEndOFGameRoutine();
            
        }

        public Move GetUserMoveInput(PreMove premovePlayer)
        {
            Move chosenMove = new Move(0, 0, PieceID.White);
            string move;
            bool userInputCorrect = false;
            string[] sourceAndTarget;

            while (userInputCorrect == false)
            {
                chosenMove.source = 0; chosenMove.target = 0;

                Console.WriteLine("Please enter your move like this: 'a1/a2', where");
                Console.WriteLine("'a1' is the source field, and 'a2' is the target field.");
                Console.WriteLine("[Enter 'menu' to return to main menu]");
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

                chosenMove.source = dvonnBoard.entireBoard.First(field => field.fieldName == sourceAndTarget[0]).index;
                chosenMove.target = dvonnBoard.entireBoard.First(field => field.fieldName == sourceAndTarget[1]).index;

                if (!premovePlayer.legalMoves.Contains(chosenMove))
                {
                    bool sourceInvalid = !premovePlayer.trueLegalSources.Contains(chosenMove.source);
                    bool targetInvalid = !premovePlayer.trueLegalTargets.Contains(chosenMove.target);

                    if (sourceInvalid && targetInvalid)
                    {
                        Console.WriteLine("Both source and target field are invalid.");
                        Console.WriteLine("If in doubt, please consult Dvonn rules (enter 'rules')");
                        continue;
                    }
                    else if (sourceInvalid)
                    {
                        Console.WriteLine("The source field is not valid.");
                        Console.WriteLine("If in doubt, please consult Dvonn rules (enter 'rules')");
                        continue;
                    }
                    else if (targetInvalid)
                    {
                        Console.WriteLine("The target field is not valid.");
                        Console.WriteLine("If in doubt, please consult Dvonn rules (enter 'rules')");
                        continue;
                    }

                }

                userInputCorrect = true;
                
            }

            return chosenMove;

        }
        public Move PickRandomMove(PreMove premove)
        {
            Random rGen = new Random();
            Move randomMove = premove.legalMoves[rGen.Next(0, premove.legalMoves.Count)];
            return randomMove;
        }

        public PreMove RepeatedRandomMove(PreMove AIpreMove)
        {
            PreMove currentPlayerPreMove;
            PreMove currentAiPreMove = AIpreMove;

            Console.WriteLine("Human player has no legal moves, computer will continue playing");
            do
            {
                WaitForUser();
                Move randomMove = PickRandomMove(currentAiPreMove);
                dvonnBoard.MakeMove(randomMove);
                dvonnGame.gameMoveList.Add(randomMove);
                dvonnBoard.VisualizeBoard();
                typeWriter.MoveComment(randomMove, PieceID.Black);

                currentAiPreMove = ruleBook.ManufacturePreMove(PieceID.Black);
                currentPlayerPreMove = ruleBook.ManufacturePreMove(PieceID.White);

                if (ruleBook.GameEndCondition(currentPlayerPreMove, currentAiPreMove) == true) return null;
                else if (currentAiPreMove.legalMoves.Count == 0) break;

            } while (currentPlayerPreMove.legalMoves.Count == 0);

            return currentPlayerPreMove;
        }

        public void WaitForUser()
        {
            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

    }

}
