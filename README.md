# dvonn-console
# Dvonn

## - C# applikation


### Hvad er Dvonn?

Dvonn er et abstrakt brætspil, udviklet som fysisk spil af spildesigneren Kris Burm i 2001.
Dvonn minder lidt om dam eller kina-skak, men har et meget dybere strategisk niveau end disse spil, som til gengæld minder mere om det strategiske niveau i skak eller go.

Dvonn spilles på det særlige Dvonn bræt, som ser sådan ud:


<img src=/dvonn_board.jpg width="800">


### Brikkerne fordeles tilfældigt på brættets felter1. Hvid og sort har 23 brikker hver, som de skiftevis flytter. Der er desuden tre røde brikker, Dvonn brikker, som har særlig betydning.

Hver stack flyttes ligeså mange felter, som den indeholder brikker. For at være et gyldigt træk, skal stack’en desuden have spillerens farve som øverste brik, samt lande på et felt, som i forvejen har en eller flere brikker.

Den nærværende udgave af applikationen er en ren konsol applikation.  Brættet ”visualiseres” ved hjælp af tekstfragmenter i forskellige farver. Console.Clear metoden sikrer at brættet altid er øverst i konsollen. Sammen med brættet vises altid en oversigt over koordinaterne. Disse anvendes ved indkodning af træk.
Under brættet findes en menu, hvor man kan vælge enten at flytte en stack eller inspicere en stack (få informationer om, hvilke brikker der er i stacken). Man kan desuden også få den nuværende score fremvist, samt få fremvist reglerne.

Hvid (menneske) og Sort (computer) skiftes til at flytte brikkerne i forhold til reglerne, som kan ses her.
I den nærværende udgave af spillet, vælger computeren tilfældige (men dog altid legale) træk.

I slutfasen er programmet i stand til at håndtere Dvonn collapse, Pass Condition samt Game End.

Hvordan er koden bygget op?
C# koden er opdelt i 7 classes:
    1) Program: Denne class har kun et formål: at opstarte Game class og Controller class.
    2) Game: New Game instantierer et object, som i constructoren skaber alle 49 stacks, samt fordeler de 49 farvede brikker på dem – tilfældigt.
    3) Controller: Denne class håndterer main menu og alle input fra bruger. Den styrer træk, og de metoder, der skal kaldes i mellem træk, samt administrerer random træk.
    4) Piece: en meget simpel class, som blot definerer Piece objektet og giver det en enum baseret piece type.
    5) Stack: Denne class definerer de fields, som en stack kan have. Eksempelvis defineres de seks retninger NW, NE, EA, SE, SW, WE her. Det er også her at den List af pieces, som hver stack har defineres. Hvis denne liste ikke indeholder nogen pieces, svarer det til at feltet på brættet er tomt.
    6) Calculate: Den mest komplekse og beregningstunge class. Denne class er gjort static, fordi der ikke er brug for objekt-instantieringer af den og fordi dens metoder ofte skal kaldes i andre classes. Det er her alle reglerne er implementeret, og her at Dvonn collapse beregnes, og All Principal Moves (tupple der indeholder alle 786 mulige træk på brættet) beregnes.
    7) Write: En anden static class, som i modsætning til Calculate ikke indeholder mange loops og statements. Tilgengæld kan den skrive en masse tekst til konsollen. 
