# VRCAudioVis
[![Generic badge](https://img.shields.io/badge/Unity-2019.4.31f1-informational.svg)](https://unity3d.com/unity/whats-new/2019.4.31)
[![Generic badge](https://img.shields.io/badge/SDK-WorldSDK3-informational.svg)](https://vrchat.com/home/download)
[![Generic badge](https://img.shields.io/badge/VRC-Udon%23-blue)](https://github.com/MerlinVR/UdonSharp)
[![Generic badge](https://img.shields.io/github/downloads/dragon99z/VRCAudioVis/total?label=Downloads)](https://github.com/dragon99z/VRCAudioVis/releases/latest)

i've made a little audio visualizer for lights, particle systems and generel objects. Its main use is in vrc worlds but you can also use it on vrc avatars with the record function

[![VRCAudioVis showcase](https://i.ytimg.com/vi/BM60__ZZemk/maxresdefault.jpg)](https://www.youtube.com/watch?v=BM60__ZZemk)


## AudioLink Settings
(only visible when AudioLink is available)

You can link your AudioLink to the AudioVis but it will only use the audio source
and if you want the audiolink colortheme for the light visualizer


## Visualizer Settings

The general visualizer settings where you add the objects you want to visualize, the height multiplier,
the number of samples and visualizer the algorithm 


## Light Visualizer

### Light Intensity

The light intensity changes to the music

### Light Hue

The light color changes to the music
you can use the lerp color to switch between the main color and the lerp color
or the shift to let the light cycle thru the colors


## Particle Visualizer

Let a particle system burst particle to the music
(you need to add one burst in the emission of the particle system)


## Transform Visualizer

Changes the transformation of the objects to the music like position, rotation, scale

## Orbit Settings

Let the onjects orbit around an center object


## Recorder Settings
(only visible without udon aka for avatars)

Records all changes made to the objects and adds them to an animation clip
