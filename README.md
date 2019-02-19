# 3DPerceptionStudy
Example Usage of DXR framework for 3D visualiztions. Currently just a playground to prototype possible visualizations of SE Austrailian weather data 

## Getting Started
### Unity
- I use Unity 2017.2 due to compatibility issues between Unity 2018 and HoloToolkit. We probably aren't using HoloLens, but I don't see any reason to use 2018. [Unity Download Archive](https://unity3d.com/get-unity/download/archive).
- I also typically only push the Assets folder, so start a new project called "3DPerceptionStudy" and then pull to that directory. When pushing, just push "Assets" (this may change)

### [DXR](https://sites.google.com/view/dxr-vis/home)
The DXR package, is NOT included in this repo. You will have to download and import yourself:
- First download the unitypackage file [here](https://sites.google.com/view/dxr-vis/download?authuser=0)
- Now open Unity, go to the "3DPerceptionStudy" project and go to "Assets" -> "Import Package" -> "Custom Package"  and find the .unitypackage file you just downloaded. Click through the defaults to finish (This may take several minutes)

### Poke Around
- In Unity, find the scene 3DPerceptionScenes/AussieExample.unity, double click to open.
- You should be able to click the "play" button and have it load some visualizations. If that doesn't work and there are some errors within the "HoloToolkit" directory, let Matt know--he can usually fix that pretty quickly. Down the road, we'll probably delete that folder altogether.
- You should notice some visualizations load. To add a new one, place Assets/DXR/Prefabs/DxRVis.prefab into the hierarchy view (left panel).
- In the Inspector view (right panel) you should see an editable field for "Vis Specs URL", where it references a .json that defines the specs of the visualization. This file should be located in Assets/StreamingAssets/DxRSpecs, and will reference a .csv file located in Assets/StreamingAssets/DxRData. 
- At this point, you should be able to begin poking. Most poking at this point is just done in the .json files. So see some of those json files in DxRSpecs for reference and see [here](https://sites.google.com/view/dxr-vis/grammar-docs) for detailed reference