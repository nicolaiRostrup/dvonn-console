using System;
using System.Collections.Generic;
using System.Linq;


namespace Dvonn_Console
{

    class GameMaster
    {
        private Writer typeWriter = new Writer();
        private Game dvonnGame;
        private Board dvonnBoard;
        private Rules ruleBook;
        private StackInspector stackInspector;
        private AI aiAgent;

        public GameMaster()
        {
            dvonnBoard = new Board();
            dvonnBoard.InstantiateFields();
            dvonnBoard.CalculatePrincipalMoves();
            ruleBook = new Rules(dvonnBoard);
            stackInspector = new StackInspector(dvonnBoard);
            aiAgent = new AI();

        }

        public void BeginNewGame()
        {
            typeWriter.WelcomeText();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("What color do you prefer to play? (White allways begins)");
            Console.WriteLine("Please enter W for white or B for black");
            string preferedColor = Console.ReadLine().ToUpper();

            Console.WriteLine();
            Console.WriteLine("What would you like to be called?");
            string playerName = Console.ReadLine();
            dvonnGame = new Game(preferedColor.ToPieceID(), playerName, aiAgent.aiEngineName, aiAgent.aiEngineVersion);

            Console.WriteLine();
            Console.WriteLine("Do you wish to begin from a predefined position, or a randomly generated position?");
            Console.WriteLine("Please enter P for predefined or R for random");
            string openingType = Console.ReadLine().ToUpper();

            if (openingType == "P")
            {
                Console.WriteLine("Please enter opening position by describing each row with 'W', 'B' or 'D'...");
                string allRows = "";

                allRows += GetRowRight(9, "Please enter first row (A0 to A8)");
                Console.WriteLine();
                allRows += GetRowRight(10, "Please enter second row (B0 to B9)");
                Console.WriteLine();
                allRows += GetRowRight(11, "Please enter third row (C0 to CT)");
                Console.WriteLine();
                allRows += GetRowRight(10, "Please enter fourth row (D0 to D9)");
                Console.WriteLine();
                allRows += GetRowRight(9, "Please enter fifth row (E0 to E8)");
                Console.WriteLine();

                Position predefinedPosition = new Position();
                for (int i = 0; i < 49; i++)
                {
                    predefinedPosition.stacks[i] = allRows[i].ToString();
                }
                dvonnGame.openingPosition = predefinedPosition;
                dvonnBoard.ReceivePosition(predefinedPosition);
            }

            else if (openingType == "R")
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
            List<Move> playerLegalMoves;
            List<Move> aiLegalMoves;
            PieceID humanColor = dvonnGame.humanPlayerColor;
            PieceID aiColor = humanColor.ToOpposite();

            if (humanColor == PieceID.Black)
            {
                Console.WriteLine();
                Console.WriteLine("Computer is ready to start game. Press any key to launch action.");
                WaitForUser();

                Move aiMove = aiAgent.ComputeAiMove(dvonnGame.openingPosition, aiColor);
                dvonnBoard.MakeMove(aiMove);
                dvonnGame.gameMoveList.Add(aiMove);
                dvonnBoard.VisualizeBoard();
                typeWriter.MoveComment(aiMove, aiColor);

                playerLegalMoves = ruleBook.FindLegalMoves(PieceID.Black);
                aiLegalMoves = ruleBook.FindLegalMoves(PieceID.White);
            }
            else
            {
                playerLegalMoves = ruleBook.FindLegalMoves(PieceID.White);
                aiLegalMoves = ruleBook.FindLegalMoves(PieceID.Black);
            }


            while (gamerunning)
            {
                typeWriter.MenuText();

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": // Enter move ...

                        Move chosenMove = GetUserMoveInput(playerLegalMoves);
                        //Move chosenMove = PickRandomMove(playerLegalMoves);

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
                            typeWriter.MoveComment(chosenMove, humanColor);

                        }
                        //Do a check for dvonn collapse, and if true, execute and make comment.
                        ruleBook.CheckDvonnCollapse(chosenMove, true);

                        //After White's move both players' options need to be analyzed.
                        playerLegalMoves = ruleBook.FindLegalMoves(humanColor);
                        aiLegalMoves = ruleBook.FindLegalMoves(aiColor);

                        //Check whether game has ended. 
                        if (aiLegalMoves.Count == 0 && playerLegalMoves.Count == 0)
                        {
                            DoEndOFGameRoutine();
                            gamerunning = false;
                            break; // when returned, close console
                        }

                        //Check if computer has any legal moves
                        if (aiLegalMoves.Count == 0)
                        {
                            dvonnGame.gameMoveList.Add(new Move(aiColor));
                            Console.WriteLine();
                            Console.WriteLine("Computer has no legal moves. It's your turn again.");
                            break;
                        }
                        // else computer gets to move:
                        Console.WriteLine();
                        Console.WriteLine(aiColor + " is ready to move");
                        WaitForUser();

                        Move aiMove = aiAgent.ComputeAiMove(dvonnBoard.SendPosition(), aiColor);
                        dvonnBoard.MakeMove(aiMove);
                        dvonnGame.gameMoveList.Add(aiMove);
                        dvonnBoard.VisualizeBoard();
                        typeWriter.MoveComment(aiMove, aiColor);
                        ruleBook.CheckDvonnCollapse(aiMove, true);

                        //After ai engine move both players' options need to be analyzed.
                        playerLegalMoves = ruleBook.FindLegalMoves(humanColor);
                        aiLegalMoves = ruleBook.FindLegalMoves(aiColor);

                        //Again, this time after blacks move, check whether game has ended.
                        if (aiLegalMoves.Count == 0 && playerLegalMoves.Count == 0)
                        {
                            DoEndOFGameRoutine();
                            gamerunning = false;
                            break; // when returned, close console
                        }

                        //Check if human has any legal moves
                        if (playerLegalMoves.Count == 0)
                        {
                            dvonnGame.gameMoveList.Add(new Move(humanColor));

                            List<Move> newMoves = RepeatedRandomMove(aiLegalMoves);
                            if (newMoves == null)
                            {
                                DoEndOFGameRoutine();
                                gamerunning = false; // when returned, close console
                                break;
                            }
                            else
                            {
                                // If white should have gotten a new opportunity to move, return to main menu...
                                playerLegalMoves = newMoves;
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
                        playerLegalMoves = ruleBook.FindLegalMoves(PieceID.White);
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
            typeWriter.GameEndText(whiteScore, blackScore, dvonnGame);
            Console.WriteLine(dvonnGame.ToString());
            WaitForUser();
        }
        
        //As autofinish plays for both colors, currentAiLegalMoves will represent human color options.
        //every second run through the loop.
        private void AutoFinish()
        {
            PieceID playerToMove = dvonnGame.humanPlayerColor;
            List<Move> currentAiLegalMoves;
            List<Move> opponentLegalMoves;

            Console.WriteLine();
            Console.WriteLine("AI will now finish the game");
            WaitForUser();

            do
            {
                currentAiLegalMoves = ruleBook.FindLegalMoves(playerToMove);
                if (currentAiLegalMoves.Count > 0)
                {
                    Move randomMove = PickRandomMove(currentAiLegalMoves);
                    dvonnBoard.MakeMove(randomMove);
                    dvonnGame.gameMoveList.Add(randomMove);
                    ruleBook.CheckDvonnCollapse(randomMove, false);
                }
                opponentLegalMoves = ruleBook.FindLegalMoves(playerToMove.ToOpposite());

                if (currentAiLegalMoves.Count == 0 && opponentLegalMoves.Count > 0)
                {
                    dvonnGame.gameMoveList.Add(new Move(playerToMove));
                }

                playerToMove = playerToMove.ToOpposite();

            } while ((currentAiLegalMoves.Count == 0 && opponentLegalMoves.Count == 0) == false);

            dvonnBoard.VisualizeBoard();
            DoEndOFGameRoutine();

        }

        public Move GetUserMoveInput(List<Move> playerLegalMoves)
        {
            Move chosenMove = new Move(0, 0, dvonnGame.humanPlayerColor);
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

                if (!playerLegalMoves.Contains(chosenMove))
                {
                    List<int> trueLegalSources = ruleBook.ExtractSources(playerLegalMoves);
                    List<int> trueLegalTargets = ruleBook.ExtractTargets(playerLegalMoves);
                    bool sourceInvalid = !trueLegalSources.Contains(chosenMove.source);
                    bool targetInvalid = !trueLegalTargets.Contains(chosenMove.target);

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
        public Move PickRandomMove(List<Move> theseMoves)
        {
            Random rGen = new Random();
            Move randomMove = theseMoves[rGen.Next(0, theseMoves.Count)];
            return randomMove;
        }

        public List<Move> RepeatedRandomMove(List<Move> aiMoves)
        {
            List<Move> playerLegalMoves;
            List<Move> aiLegalMoves = aiMoves;
            PieceID humanColor = dvonnGame.humanPlayerColor;
            PieceID aiColor = humanColor.ToOpposite();

            Console.WriteLine("Human player has no legal moves, computer will continue playing");
            do
            {
                WaitForUser();
                Move randomMove = PickRandomMove(aiLegalMoves);
                dvonnBoard.MakeMove(randomMove);
                dvonnGame.gameMoveList.Add(randomMove);
                dvonnBoard.VisualizeBoard();
                typeWriter.MoveComment(randomMove, aiColor);

                aiLegalMoves = ruleBook.FindLegalMoves(aiColor);
                playerLegalMoves = ruleBook.FindLegalMoves(humanColor);

                if (aiLegalMoves.Count == 0 && playerLegalMoves.Count == 0) return null;
                else if (aiLegalMoves.Count == 0) break;

                //Add pass move for human player:
                dvonnGame.gameMoveList.Add(new Move(humanColor));

            } while (playerLegalMoves.Count == 0);

            return playerLegalMoves;
        }

        private string GetRowRight(int length, string text)
        {
            bool rowAcknowledged = false;
            string thisRow = "";

            while (rowAcknowledged == false)
            {
                Console.WriteLine(text);
                thisRow = Console.ReadLine();
                if (thisRow.Length != length)
                {
                    Console.WriteLine("Row is not correct length");
                    continue;
                }
                if (AreLettersOkay(thisRow) == false)
                {
                    Console.WriteLine("Row includes incompatible letters");
                    continue;
                }
                rowAcknowledged = true;
            }
            return thisRow.ToUpper();
        }
        private bool AreLettersOkay(string letters)
        {
            foreach (char c in letters)
            {
                if (!(c == 'W' || c == 'w' || c == 'B' || c == 'b' || c == 'D' || c == 'd'))
                    return false;

            }
            return true;
        }

        public void WaitForUser()
        {
            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

    }

}
