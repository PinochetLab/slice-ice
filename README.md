# Slice Ice

Slice Ice is a physics-based puzzle game where the player cuts pieces off an ice floe while keeping all penguins together.

Penguins continuously move across the ice and bounce off its edges. The goal is to remove the required amount of ice without cutting the floe in a way that separates the penguins.

## Gameplay

The player draws a cutting line across the ice floe. If the cut is valid, a section of the ice is removed and the remaining playable area becomes smaller.

At the same time, penguins move around the surface and bounce off the boundaries. The player has to choose cuts carefully to reduce the size of the ice while keeping all penguins on the same remaining section.

## Technical Highlights

- Runtime cutting of the ice geometry
- Detection and separation of resulting polygon regions
- Dynamic mesh generation after each cut
- Physics-based penguin movement and boundary collisions
- Validation of cuts based on penguin positions
- Level progression based on the remaining ice area

## Tech Stack

- Unity
- C#
- Unity Physics

## About the Project

Slice Ice is a personal Unity project focused on combining runtime geometry manipulation with simple physics-based puzzle mechanics.

The main technical challenge was implementing dynamic cuts and determining which part of the ice should remain while ensuring that the penguins are not separated.
