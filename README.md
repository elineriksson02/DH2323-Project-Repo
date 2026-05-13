# DH2323-Project-Repo - Water Simulation in Unity
This is my project repo for the course DH2323 Computer Graphics and Interaction VT26

## Overview
This project explores real-time water simulation in Unity by comparing:
- Gerstner wave-based water simulation
- A simplified Navier–Stokes (shallow water) approach

## Project Goal
The goal of this project is to explore water simulation, which is considered one of the most challenging problems in computer graphics, especially in games and films. There are two main approaches to simulating water: wave-based models and physics-based models. To better understand the differences between these two approaches, this project aims to compare their performance and visual realism in a real-time Unity environment.

## Features
- Gerstner wave implementation
- Physics-based shallow water simulation (simplified Navier–Stokes)
- FPS performance comparison

## Technologies
- Unity (version 6.4)
- C#

## Results
| Scenario | Gerstner FPS | NS-based FPS |
|---|---|---|
| Water waves only | ~315 | ~736 |
| Floating object | ~293 | ~702 |

The results show that the NS-based model achieves higher frame rates due to its simpler per-frame computation, while Gerstner waves produce smoother and more structured large-scale wave patterns. However, the NS model provides better floating object interaction through height field coupling.
See the [full report](Report/report.pdf) for more details.

## Screenshots
### Gerstner Waves
![Water Simulation](Video/Gernst.gif)
### Gerstner Waves with Object
![Water Simulation](Video/GernstObject.gif)
### NS-based model
![Water Simulation](Video/NavierSimple.gif)
### NS-based model with Object
![Water Simulation](Video/NavierSimpleObject.gif)

#Blog Part 

## Development Log

**Week 1:** Read literature on wave-based and 
physics-based water simulation methods. I wanted 
to do something with water because I knew it is 
a big topic in the game and film industry. My 
personal goal was to better understand why it is 
actually hard to implement. After some research 
I focused on two main approaches that kept 
appearing in the literature.

**Week 2:** Wrote the literature section and 
studied how to implement both models by reading 
blogs, reports and watching YouTube videos to 
see how different people approach the problem. 
At first I thought I had found a good solution, 
but later weeks would show I needed a different 
approach.

**Week 3:** Started implementing the Gerstner 
wave model. Had some trouble at first getting 
the waves to move smoothly. Started with one 
wave and once that worked, added more waves to 
create more realistic interference patterns, 
which is a common approach in the literature.

**Week 4–5:** Added the SWE model and floating 
object interaction. I originally had a different 
Navier–Stokes simplification in mind but when I 
started developing it, it didn't work at all,  
it kept blowing up and producing invalid values. 
After more research I found the Shallow Water 
Equation model which is more tractable because 
it only simulates surface height. But that was 
still hard to implement. The main challenge was 
numerical blow-up, which I solved by implementing 
the CFL condition to calculate a stable timestep 
and subdividing each frame into substeps. After 
solving that, the next problem was that the water 
didn't move at all. After more research I found 
that a driving source was needed, so I added a 
continuous wave boundary condition on the grid 
edges to produce visible wave motion.

**Week 6:** Analysed results and ran FPS 
measurements. The evaluation was mainly visual 
and based on my own judgment, which is a 
limitation, ideally other people would evaluate 
it too, which is something the proposed perceptual 
study in the report addresses. It was interesting 
to think about the results because I kept coming 
back to the question of where you would actually 
use each model, and the answer was that neither 
is strictly better, they just suit different 
situations.

**Week 7:** Finished and polished the report.

## Reflections

### What I learned
- Implementing physics-based simulations is 
  significantly harder than wave-based approaches. 
  Getting numerical stability right requires 
  understanding the mathematics behind the 
  simulation, not just the code.
- The CFL condition and staggered grids are 
  standard tools in computational fluid dynamics 
  for good reason — without them the simulation 
  simply does not work.
- Comparing two implementations fairly is harder 
  than it sounds. Small differences in how each 
  model represents height or couples with objects 
  can make the comparison unfair without you 
  realising it.

### What I would do differently

- Start validating the SWE implementation against 
  known analytical solutions before integrating 
  it into Unity. Debugging a physics simulation 
  inside a game engine is much harder than 
  debugging it in isolation. I should have done 
  more research on how to evaluate an 
  implementation before building it in Unity — 
  this would have saved time that could have been 
  used for user testing or improving the visual 
  output.

- Spend more time on the visual output — adding 
  a proper water shader with reflections and 
  transparency would make both simulations look 
  significantly more realistic.

- Implement a more complete floating object 
  system. It would also be interesting to 
  optimise the interaction separately for each 
  model and then compare the results — this could 
  reveal whether the choice of interaction method 
  changes which model performs better in 
  different scenarios.

- Test with more people rather than only 
  evaluating the results myself. Having external 
  participants evaluate the simulations would 
  give more reliable and less biased results, 
  which is also what the proposed perceptual 
  study in the report aims to address.
