We converted all of our movement to be tile based rather than the velocity based movement of the previous version.
This allowed movement that very closely mimicked the original pacman and also made it easier to apply pathfinding to the ghosts.
The tiles are represented with a graph that uses connected nodes that are aware if they are a wall or not. They also know
if they are at an intersection or not. A* can be applied to this graph to allow the basic ghost pathfinding, and then
the original ghost behaviors as described in the first link were applied to each ghost.

The movement for the ghosts was done in waves between timed scattering and chasing sections. When scattering, they each
go to their own corner and when chasing, they do whatever behavior that ghost did in the original pacman. This was represented
with a 3 state FSM as shown in the file FSM.png.


Group Members:
Kirsten Pilla
Jared Okun
Sam Gould

Sites used: http://gameinternals.com/post/2072558330/understanding-pac-man-ghost-behavior
https://github.com/danielmccluskey/A-Star-Pathfinding-Tutorial