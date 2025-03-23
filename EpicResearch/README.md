# EpicResearch Mod
Modifies research requirements for a slower game progression.

## Features
- Slower tech progression.
- Some unlocks happen further down the tech tree to allow the player to use all different kinds of setups the game implements.
- Try on a Minimal resources 64 star seed, with 100 multiple and adjustments turned on for a hard challenge.
    - it is expected to run out or resources and having to move systems several times before unlocking warp and logistic warp.
    - it is expected to have a pre-warp logistics between systems at yellow and purple tech.
- Allows to specify additional resource multipler.
- Playable with Fog set to max difficulty.

## Configuration
bool AdjustTechUnlocks = true

- Change Logistic Warp to be availble at level 6 (instead of 4).
- Move rare receipts to be available further in the tech tree.
- Add 6 degree of sphere stress allowance when Gravity Lenses are availble (Gravity Wave refraction).
- At levels 3-5, the speed of Vessels is greately improved.
- Vessel capacity is increased faster at levels 3-4.
- Increased bonuses to mecha core capacity and recharge, mecha speed, drone speed and count, Vein Utilization that are gained at early upgrades levels by 2-3X to help manage larger-scale factories.


bool  AdjustTechCosts = true
- Signifficantly change technology costs. These changes include:
    - Universe Exploration 3 (6ly scanning) is availble as a Red tech.
    - Proliferator Mk3 and Fusion require Purple tech.

int TechCostMultiple = 100
- Apply additional multiplier to hash and cube costs of technologies. Valid values are: {1, 5, 20, 50, 100}.

float ResourceMultiple = 1
- Additional resource multipler for the galaxy. Affects all ore, gas giants, oil. Oil is affected by sqrt of this value.

## Installation Note
This mod depends on BepInEx.
Is not compatible with the background autosave feature of BulletTime mod.

## Bug Reports, Comments, Suggestions
Reach out to mrrvlad#0925 on discord / official DSP server.

## Changelog
- v0.4.2
    - Moved advanced unlocks for warpers and particle containers from mission complete to universe matrix.
    - Reduced Sail Attachment tech costs.
    - Reduced later VU tech costs.

- v0.4.1
    - Fixed exception when researching Combustible Units and Gauss Cannons.

- v0.4.0
    - Adjusted early game tech costs to survive max Fog difficulty.

- v0.3.8
    - Removed logi speed 4 dependence on drive engine 4.
    - Yellow balancing tweaks: logi speed bonus, availability of advanced receipts. 

- v0.3.6
    - Yellow balancing tweaks.

- v0.3.5
    - Minor balancing tweaks.

- v0.3.4
    - First pass for yellow tech balancing. 
    - Allow basic Dyson Frame construction at late-yellow tech.
    - Improve Sail lifetime signifficantly.
    - Removed dependency of Qchip on Information Matrix.
    - Deuteron Rods are now availble with Particle Collider tech.

- v0.3.3
    - QOL changes to the pre-automation game.

- v0.3.2
    - All oil reserves, including starter planet, are now affected by sqrt of resource multipler.

- v0.3.1
    - Increased bonuses to mecha core capacity and recharge, mecha speed, drone speed and count, Vein Utilization that are gained at early upgrades levels by 2-3X to help manage larger-scale factories.

- v0.3.0
	- Tech cost balancing pass - should be playable at minimal resources.
    - Increased logistic speed bonus from 1500 to 3000 for level 3 to 5.
    - Graphene from fire ice availble earlier at purple tech.

- v0.2.0
	- Added ability to specify planet resource multipler. Applied after game's resource multipler. Affects all ores, gas giants, oil. Remote oil is affected as a sqrt of this value.
    - Signifficantly reduce pre-automation research requirements.

- v0.1.1
	- WIP. Initial version. (Game Version 0.9.27.15466)