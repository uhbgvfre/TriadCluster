## Introduction
**Triad Cluster** is a powerful triangle search and tracking tool, specifically developed for touchscreen interaction projects. Leveraging cluster analysis and parallel comparison of similar triangles, combined with an event-driven architecture, Triad Cluster offers efficient and effective performance for a variety of applications.

## Note
When dealing with numerous points, traditional cluster analysis and search methods can become highly inefficient. To optimize computational efficiency, Triad Cluster uses Unity's JobSystem for parallel processing. In our demo, we include 7, 12, 19, and 91 types of triangles, with each triangle consisting of 3 points, serving as a stress test. Typically, the system can smoothly track fewer than 20 types of triangles.

## Features

- **Distinct Triangle Identification:** Differentiate triangles based on their internal angle combinations, with optional area matching settings.
- **Triangle Coordinates and Orientation:** Obtain triangle coordinates, with orientation determined by the largest angle, in formats such as NormalizeVector2 or EulerAngleDegree.
- **Cluster Analysis:** Utilize advanced cluster analysis to identify patterns and relationships between triangles.
- **High Performance:** Leverage parallel computing to compare similar triangles rapidly.

## External Dependencies

Triad Cluster relies on the following external dependencies:
- `com.unity.nuget.newtonsoft-json` or [Json.NET](https://www.newtonsoft.com/json)
- [UniTask](https://github.com/Cysharp/UniTask)
- [Cysharp.MessagePipe](https://github.com/Cysharp/MessagePipe)

## Get & Install

1. **Get on AssetStore:**
   - [AssetStore - TriadCluster](https://assetstore.unity.com/packages/slug/305618)

2. **Import the dependencies:**
    - Add `com.unity.nuget.newtonsoft-json` or import [Json.NET](https://www.newtonsoft.com/json) into your Unity project.
    - Import [UniTask](https://github.com/Cysharp/UniTask) into your Unity project.
    - Import [Cysharp.MessagePipe](https://github.com/Cysharp/MessagePipe) into your Unity project.

3. **Import Triad Cluster package:**
    - Import `TriadCluster` from Unity Package Manager.

## Configuration

### TrianglePresets

In `TriadClusterUnitPresetsLoader`, predefined specifications for the real-world prop touch points forming triangles are set in JSON format using PlayerPrefs. During runtime, the program compares and tracks all touch points on the screen based on these specifications.

- Customize any number of prop triangles with unique internal angle combinations. The following example demonstrates specifications for 3 prop triangles.
- Different internal angle combinations are considered different triangle IDs.
- The vector from the circumcenter to the vertex with the largest angle is the OrientedVector, which determines the orientation of the prop.

```json
[{
    "id": 1,
    "radius": 100,
    "angleWeights": [
        90,
        60,
        30
    ]
},
{
    "id": 2,
    "radius": 100,
    "angleWeights": [
        62,
        60,
        58
    ]
},
{
    "id": 3,
    "radius": 100,
    "angleWeights": [
        110,
        50,
        20
    ]
}]
```

### ModuleConfig

The `TriadClusterConfig` uses PlayerPrefs in JSON format to set the parameters for the Resolver search algorithm:

- **AnalyzeCycleFrameInterval:** `[Default: 1]` Triangle match algorithm execution interval expected value.
- **MinTriangleCircumradius:** Triangle match algorithm only evaluates points with distances greater than this value for potential triangles.
- **MaxTriangleCircumradius:** Triangle match algorithm only evaluates points with distances less than this value for potential triangles. This also affects the internal parameter "Epsilon." Epsilon = MaxTriangleCircumradius * 2 + 0.0001. Higher Epsilon values allow more distant points to be considered as potential triangles, increasing computation cost. Typically, Epsilon should be set to just over the diameter of the largest TriangleUnitPreset, so set MaxTriangleCircumradius to slightly exceed the radius of the largest TriangleUnitPreset.
- **MaxAbsDeltaAngleDegree:** `[Recommended: 2]` In two triangle match cases, if any angle difference is greater than this value, it will be ignored.
- **MaxAbsDeltaAreaRatio:** `[Recommended: 0.0001]` In two triangle match cases, if the area ratio difference is greater than this value, it will be ignored.
- **FrameCountForNoRespondsThreshold:** `[Default: 2]` The criterion for OnTriangleUpEvent determination. If a triangle unit on the screen exceeds this frame count without detection, it will be removed from the existing triangle list.
- **IgnoreTriangleAreaMatching:** If TRUE (! 0), MaxAbsDeltaAreaRatio will have no effect and only the triangle interior angle combinations will be compared, resulting in triangles with the same interior angle combinations being likely to be matched. If you want to maintain size sensitivity, set it to FALSE

## Usage

### Basic Setup

1. **Add `TriadClusterRuntime` prefab to scene**

2. **Option: Add `TriadClusterTouchPointsPublisher` prefab to scene**
   - This prefab automatically publish `UpdateInputPointsEvent` every frame by current touches.

3. **Use event to get MatchedTriangle:**
   - TriadClusterTransmitter on TriadClusterRuntime will publish OnTriangleDown/Stay/UpEvent
   - TriadClusterTransmitter's public UnityEvent Also work 

### Event Handling

Triad Cluster supports an event-driven architecture. Below are the primary events you can handle:

- **OnTriangleDownEvent**
- **OnTriangleStayEvent**
- **OnTriangleUpEvent**

Subscribe to these events to manage triangle interactions:

```csharp
using HoyarCreation.TriadCluster;
using MessagePipe;
using System;
using UnityEngine;

public class UseCase: MonoBehaviour
{
    ISubscriber<OnTriangleDownEvent> m_OnTriangleDown => GlobalMessagePipe.GetSubscriber<OnTriangleDownEvent>();

    IDisposable m_DisposableEvents;

    void Start() => m_DisposableEvents = m_OnTriangleDown.Subscribe(OnTriangleDown);

    void OnDestroy() => m_DisposableEvents.Dispose();

    void OnTriangleDown(OnTriangleDownEvent e)
    {
        print($"[TriDown] id:{tri.idLabel} pos: {tri.circumcentre}");
    }
}
```
To update the input points on the screen, you can publish the `UpdateInputPointsEvent` or add the TriadClusterTouchPointsPublisher prefab to your scene to automatically publish touch points every frame.
```csharp
IPublisher<UpdateInputPointsEvent> m_UpdateInputPointsEventSender => GlobalMessagePipe.GetPublisher<UpdateInputPointsEvent>();

void Update()
{
    List<Vector2> touchPts = GetCurrentTouches();
    var e = new UpdateInputPointsEvent(touchPts);
    m_UpdateInputPointsEventSender.Publish(e);
}
```

## Performance Test

For my simple test, search 3 triangle unit presets in 273 points is over 200FPS.
 - AMD Ryzen7 5700X 8 Core
 - NVIDIA RTX3060