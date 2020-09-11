# Dvonn

### Purpose
This is a simple console application that excersises the rules of the game and enables manual turnbased interaction. It is a component in a larger project that will also include an AI.

### The board

<img src=/dvonn_board.jpg width="800">


### The game
DVONN is played on an elongated hexagonal board, with 23 white, 23 black and 3 red DVONN-pieces. In the beginning the board is empty. The players place the pieces on empty spaces of the board, without restrictions. They place the DVONN-pieces first and their own pieces next. Then they start stacking pieces on top of each other. A single piece may be moved 1 space in any direction, a stack of two pieces may be moved two spaces, etc. A stack must always be moved as a whole and a move must always end on top of another piece or stack. If pieces or stacks lose contact with the DVONN-pieces, they must be removed from the board. The game ends when no more moves can be made. The players put the stacks they control on top of each other and the one with the highest stack is the winner.

