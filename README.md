# Dvonn

## - C# applikation


### Hvad er Dvonn?

Dvonn er et abstrakt brætspil, udviklet som fysisk spil af spildesigneren Kris Burm i 2001.
Dvonn minder lidt om dam eller kina-skak, men har et meget dybere strategisk niveau end disse spil, som til gengæld minder mere om det strategiske niveau i skak eller go.

Dvonn spilles på det særlige Dvonn bræt, som ser sådan ud:


<img src=/dvonn_board.jpg width="800">


Brikkerne fordeles tilfældigt på brættets felter1. Hvid og sort har 23 brikker hver, som de skiftevis flytter. Der er desuden tre røde brikker, Dvonn brikker, som har særlig betydning.

Hver stack flyttes ligeså mange felter, som den indeholder brikker. For at være et gyldigt træk, skal stack’en desuden have spillerens farve som øverste brik, samt lande på et felt, som i forvejen har en eller flere brikker.

Den nærværende udgave af applikationen er en ren konsol applikation.  Brættet ”visualiseres” ved hjælp af tekstfragmenter i forskellige farver. Console.Clear metoden sikrer at brættet altid er øverst i konsollen. Sammen med brættet vises altid en oversigt over koordinaterne. Disse anvendes ved indkodning af træk.
Under brættet findes en menu, hvor man kan vælge enten at flytte en stack eller inspicere en stack (få informationer om, hvilke brikker der er i stacken). Man kan desuden også få den nuværende score fremvist, samt få fremvist reglerne.

Hvid (menneske) og Sort (computer) skiftes til at flytte brikkerne i forhold til reglerne, som kan ses her.
I den nærværende udgave af spillet, vælger computeren tilfældige (men dog altid legale) træk.

I slutfasen er programmet i stand til at håndtere Dvonn collapse, Pass Condition samt Game End.

