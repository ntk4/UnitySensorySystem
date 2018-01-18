![Sensor](https://github.com/ntk4/UnitySensorySystem/blob/master/Documentation/Screenshots/Sensor.png?raw=true)

# Sensory Systems in Game AI #
The purpose of a Sensory System in Game Artificial Intelligence is to provide Senses (Vision, Hearing etc.) to AI agents in order to perceive specific types of stimuli (hereafter signals) in the game world. 
The stimuli can be nearly anything that is perceivable by a set of senses, such as:

* Player or other agent (NPC) movement in an agent's field of vision
* Sounds of unexpected explosions
* Other NPCs calling for help.

The result of such process depends on the game logic, for example a player becoming visible to enemy NPCs would possibly lead them to attack.

For more information on Sensory Systems in general read these:

1. [Sensory System in Thief](http://www.gamasutra.com/view/feature/2888/building_an_ai_sensory_system_.php?print=1). Some of the ideas here and especially the terminology have been followed here for conformity reasons.
2. [Corbin Percy's blog entry on Thief system](http://pfhunk.blogspot.be/2012/12/introduction-to-sensory-systems-in-game.html)
3. [Sommeregger2013](http://theses.fh-hagenberg.at/system/files/pdf/Sommeregger13.pdf) Master's thesis by Bernd Sommeregger

# Overview #
This project is a modular sensory system for Unity, written in C#. For a guide how to use the system read [here](https://bitbucket.org/ntk4/unitysensorysystem/wiki/Scene%20set%20up) 

The purpose of the present system is:

1. To provide Scripts that can be attached to GameObjects and easily flexibly set up the sensory behavior 
2. Perform the complete sensory logic to evaluate the possible signals and inform the game logic via callbacks about the ones that need custom handling.
3. To serve as a system that can aid level and game designers build fast prototypes or use in actual games. However the "callbacks" are programming elements that need to be developed.

# Concepts #
1. ***Sense***: Vision and Hearing are supported.
2. ***Sensor***: Implementation of one sense as a script. For example a GameObject could have a Vision sensor and optionally another for Hearing. This script is where most of the system is set up. See more details later on.
3. ***Signal Object***: Implementation of a stimuli/signal as a script. Attaching this to a GameObject means that the game object will be evaluated automatically by the system. Each Signal Object refers to one sense, so we can have multiple if a GameObject produces visual signal as well as sound.
4. ***Sensor Manager***: A script that boostraps the system and performs all evaluations.
5. ***Sense Link***: As in the Thief design, the result of an Sensor evaluating nearby signals. This is the information passed to the game logic via the callbacks.
6. ***Awareness***: Defined as None-Low-Medium-High it's the level by which a Sensor has recognized a signal. For example we could set up a sensor in a way that the signals straight ahead result to High awareness, while the ones at the edge of her vision result to Low. Ultimately the awareness is only information passed as part of a Sense Link to the game logic. It's up to the game logic alone to handle it. The awareness also cools down by time, which is also configurable.
7. ***View Cone***: A field of vision of an NPC as it would be for a real person. An NPC can have more than one to indicate different levels of awareness. All view cones are set up as part of a Sensor.

# Demo scene #
Please open the sample scene included in the project. Read [here](https://bitbucket.org/ntk4/unitysensorysystem/wiki/Scene%20set%20up) how it's set up.

If you run it you'll notice two characters:

* The blue one is the PC, controllable with the arrows.
* The red one is the enemy NPC, who for the purpose of the demo scene is stationary.

By moving the player around the NPC log messages appear in the Unity console. The messages are e.g. "I see you! High awareness" when the player passes exactly in front, "I see you! Low awareness" when the player is at a very close distance and behind the NPC. 
![](https://github.com/ntk4/UnitySensorySystem/blob/master/Documentation/Screenshots/scene2.png?raw=true)

# Prerequisites #
This guide assumes that you have opened the demo scene.

# Overview #
This page describes how to set up the sensory system in your own project by explaining the demo scene configuration. 

1. The *SensorManager* GameObject holds the *Sensor Manager* script, the entry point to the system.
2. The player or game object that can be detected by sensors has a Vision *Signal Object*
3. The enemy NPC has a Vision *Sensor* that evaluates differently each area around her.

# Sensor Manager #
This script evaluates the *Signals* for each concerned *Sensor*.

Attributes:

* **Frames delay**: the delay of the evaluation cycle in frames. By default 30 frames, so the minimum time for any sensor to detect any signal is ideally 0.5s.
![SensorManager.png](https://github.com/ntk4/UnitySensorySystem/blob/master/Documentation/Screenshots/SensorManager.png?raw=true)

# Signal Object #
This script denotes a *Signal* that a *Sensor* can detect.

Attributes:

* **Signal Type**: The Sense, Vision or Hearing
* **Audio Signal Range**: The maximum range of a sound signal in world units
* **Audio Signal Attentuated by Obstacles**: Is the sound sound signal attenuated by obstacles between a sensor and the signal? 

Please note that the sound related attributes are likely to be redefined in the near future, as the sound signals are currently *not implemented*.

![Signal%20Object.png](https://github.com/ntk4/UnitySensorySystem/blob/master/Documentation/Screenshots/Signal%20Object.png?raw=true)

# Sensor #
The "Enemy" GameObject of the demo scene is defined as the only one having a Vision Sensor. This sensor defines 3 view cones, see below a detailed explanation.

Sensor attributes:

* **Sense Type**: The Sense, Vision or Hearing
* **Full cooldown in seconds**: The time required for a sensor to cool down fully from *High* awareness to *None*, when no signal is detected. The time is equally split among the awareness levels, so a cooldown time of 3 seconds means that after 1 sec a High Awareness will turn to Medium, then after 1 sec to Low, finally after 1 sec to None.
* **Signal Handler**: The actual callback that informs our game logic about signals that were detected by sensors. The Unity editor detects automatically suitable public event handlers in attached Monobehaviors that have a matching signature (by arguments and return type, the method name doesn't matter): 

```
void ReactToSignal(SenseLink senseLink) {...}
```

* **Preview View Cones**: Enables/Disables the editor preview of all View cones for a Vision Sensor

 The overall set up is shown here:


![Sensor.png](https://github.com/ntk4/UnitySensorySystem/blob/master/Documentation/Screenshots/Sensor.png?raw=true)

## View Cone ##
A view cone is practically an area of specific visual awareness. The sensor in this example has 3 for the areas of Low, Medium and High awareness around the GameObject. 

The cones can be overlapping as in the previous screenshot. In that case the one with higher awareness prevails. Above, if a signal is detected in the orange area that is covered by both Red (High) and Yellow (Medium) awareness, the evaluation result will be High, which will be fed to the game logic.

The cones are added exclusively by the Sensor button "Add new View Cone" and removed by the View Cone button "Remove". 

Let's examine the set up of the one for High awareness (View Cone 2):


![ViewConeHigh2.png](https://github.com/ntk4/UnitySensorySystem/blob/master/Documentation/Screenshots/ViewConeHigh2.png?raw=true)

View Cone attributes:

* **FoV (degrees)**: The field of view of the view cone expressed in degrees around the forward vector.
* **Range of sight**: The max distance of visibility, measured in world units.
* **Awareness level**: The awareness tha characterizes this view cone. It makes sense to define only one view cone per awareness level per sensor.
* **Horizontal offset**: The clockwise rotation of the View cone relative to the forward vector. See example below.
* **Recognition delay**: The delay before a view cone can actually recognize a signal and trigger the Sensor's event 
* **Scene color**: The color used for this view cone in the Unity scene view. Make sure to define an RGB as well as a meaningful value for alpha here.
* **Draw**: Controls whether a particular view cone will be drawn or not.

### Horizontal offset example ###
There are cases when we need to set up a view cone the direction of which does not coincide with the forward vector. A typical example would be to express perception of signals/moving objects behind the Sensor. The horizontal offset in that case can have a non-zero value, such as 180 degrees in the "View Cone 3" of the demo sensor:

![ViewConeLow2.png](https://github.com/ntk4/UnitySensorySystem/blob/master/Documentation/Screenshots/ViewConeLow2.png?raw=true)

