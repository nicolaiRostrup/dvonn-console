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
        


        public void BeginNewGame()
        {
            typeWriter.WelcomeText();
            
            dvonnGame = new Game(3, 23, 23);
            dvonnBoard = new Board();

            Console.Clear();
            typeWriter.Coordinates();
            dvonnBoard.CalculatePrincipalMoves();
            dvonnBoard.VisualizeBoard(dvonnGame);
            

        }

        public void GameMenu()
        {

            bool gamerunning = true;
            while (gamerunning == true)
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

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1": // Enter move ...
                        int[] moveCombo = UserMoveInput();
                        if (moveCombo[0] == 0 && moveCombo[1] == 0) // a special situation, where user wants to go back to menu
                        {
                            Write.VisualizeBoard();
                            break;
                        }
                        MakeMove(moveCombo, pieceID.White); // else, if user doesn't want to go back to menu, execute move...
                        Write.VisualizeBoard();
                        MoveComment();
                        Calculate.DvonnCollapse();

                        if (Calculate.GameEndCondition() == true) // Efter hvert hvidt træk, tjekkes der for om spillet er slut.
                        {
                            Write.GameEndText();
                            gamerunning = false;
                            break; // when returned, close console
                        }

                        if (Calculate.PassCondition(pieceID.Black) == true) // Efter hvert hvidt træk, tjekkes der for om sort ikke har nogen gyldige træk.
                        {
                            Console.WriteLine();
                            Console.WriteLine("Computer has no legal moves. It's your turn again.");
                            break;
                        }
                        // else computer gets to move:
                        Console.WriteLine();
                        Console.WriteLine("Black is ready to move");
                        WaitForUser();
                        MakeMove(CreateRandomMove(), pieceID.Black);
                        Write.VisualizeBoard();
                        MoveComment();
                        Calculate.DvonnCollapse();

                        if (Calculate.GameEndCondition() == true) // Efter hvert sort træk, tjekkes der for om spillet er slut.
                        {
                            WaitForUser();
                            Write.GameEndText();
                            gamerunning = false; // when returned, close console
                            break;
                        }

                        if (Calculate.PassCondition(pieceID.White) == true) // Efter hvert sort træk, tjekkes der for om hvid ikke har nogen gyldige træk.
                        {
                            RepeatedRandomMove();
                            if (Calculate.LegalMoves(pieceID.White) != 0) break; // Hvis hvid evt. skulle have fået et gyldigt træk, føres program pointeren til main menu...
                            Write.GameEndText(); // Ellers er spillet slut.
                            gamerunning = false; // when returned, close console
                            break;
                        }
                        break;

                    case "2":
                        Calculate.InspectStack();
                        break;

                    case "3":
                        Console.WriteLine("If game was to end now, the score would be: ");
                        Console.WriteLine("White: {0} \t Black: {1}", Calculate.Score()[0], Calculate.Score()[1]);
                        Console.WriteLine();
                        break;

                    case "4": // Visualize board
                        Write.VisualizeBoard();
                        break;

                    case "5": //Read the rules
                        Write.Rules();
                        break;

                    case "6": //Exit
                        gamerunning = false;
                        break;

                    case "7": // "Secret" option that lets user create endgame scenario for test purposes
                        Stack.AllStacks.Clear();
                        Game PartialDvonnGame = new Game(3, 8, 8);
                        Calculate.DvonnCollapse();
                        break;

                    default:
                        Console.WriteLine("Unable to read input, please try again.");
                        break;

                }
            }

        }
        public void MakeMove(int[] moveCombo, pieceID Color)
        {
            List<Piece> SourceList = Calculate.PieceList(moveCombo[0]);
            List<Piece> TargetList = Calculate.PieceList(moveCombo[1]);

            TargetList.AddRange(SourceList);
            SourceList.Clear();

            moveComment = Color.ToString() + " move, " + Calculate.AllFieldIDs[moveCombo[0]] + " / " + Calculate.AllFieldIDs[moveCombo[1]] + " has been executed."; // Ændrer static string moveComment, som udskrives senere...

        }
        public int[] UserMoveInput()
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
                    Write.Rules();
                    WaitForUser();
                    Write.VisualizeBoard();
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

                if (!Calculate.AllFieldIDs.Contains(sourceAndTarget[0]))
                {
                    Console.WriteLine("Source field is not entered correctly.");
                    continue;
                }
                if (!Calculate.AllFieldIDs.Contains(sourceAndTarget[1]))
                {
                    Console.WriteLine("Target field is not entered correctly.");
                    continue;
                }

                // efter at ovenstående kode har sikret at formatteringen af inputtet er korrekt, analyseres de angivne field IDs for at vurdere om trækket er tilladt i henhold til Dvonn reglerne:
                moveCombo[0] = Calculate.IDFromString(sourceAndTarget[0]);
                moveCombo[1] = Calculate.IDFromString(sourceAndTarget[1]);

                if (!Calculate.LegalSources(pieceID.White).Contains(moveCombo[0]))
                {
                    Console.WriteLine("The source field is not valid.");
                    Console.WriteLine("If in doubt, please consult Dvonn rules (enter 'rules')");
                    continue;
                }
                if (!Calculate.LegalTargets(moveCombo[0], Stack.AllStacks[moveCombo[0]].PieceList.Count).Contains(moveCombo[1]))
                {
                    Console.WriteLine("The target field is not valid.");
                    Console.WriteLine("If in doubt, please consult Dvonn rules (enter 'rules')");
                    continue;
                }

                userInputCorrect = true;
            }

            return moveCombo; // Kun hvis både source field og target field er helt korrekte og tilladte, returneres trækket som en int[] bestående af to field IDs.

        }
        public int[] CreateRandomMove()
        {
            // først indhentes alle legal sources for sort
            List<int> LegalSources = Calculate.LegalSources(pieceID.Black);

            // så fratrækkes alle de legalsources, som ikke har mindst eet legal target
            int legalsourceCount = LegalSources.Count; // (denne integer oprettes for at RemoveAt funktionen ikke skal ændre ved antallet af loop iterationer).

            for (int i = 0; i < legalsourceCount; i++)
            {
                if (Calculate.LegalTargets(LegalSources[i], Calculate.StackCount(LegalSources[i])).Count == 0) LegalSources.RemoveAt(i);
            }

            // et tilfældigt source field vælges (field ID)
            Random rGen = new Random();
            int randomSourceField = LegalSources[rGen.Next(0, LegalSources.Count)];

            // så indhentes alle de targets som passer til det valgte source field

            List<int> LegalTargets = Calculate.LegalTargets(randomSourceField, Calculate.StackCount(randomSourceField));

            // et tilfældigt target field vælges (field ID)
            int randomTargetField = LegalTargets[rGen.Next(0, LegalTargets.Count)];

            // til sidst opsættes kombinationen af de to
            int[] randomMove = { randomSourceField, randomTargetField };

            return randomMove;
        }
        public void RepeatedRandomMove()
        {
            Console.WriteLine("Human player has no legal moves, computer will continue playing");
            do
            {
                WaitForUser();
                MakeMove(CreateRandomMove(), pieceID.Black);
                Write.VisualizeBoard();
                MoveComment();
                if (Calculate.GameEndCondition() == true) return; //Hvis spillet er slut, skal program pointeren returnere...

            } while (Calculate.LegalMoves(pieceID.White) == 0); //Hvis Hvid har bare eet tilladt træk, skal program pointeren returnere...
        }
        public void MoveComment()
        {
            Console.WriteLine();
            Console.WriteLine(moveComment);
            Console.WriteLine();
        }
        public void WaitForUser()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

}

}
