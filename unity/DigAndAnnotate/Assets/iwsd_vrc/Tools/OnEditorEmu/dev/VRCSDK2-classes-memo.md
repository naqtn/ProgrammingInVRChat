# VRC classes Reflection

// component
VRCSDK2.VRC_Trigger
{
  string
    InteractText;
  List<VRCSDK2.VRC_Trigger+TriggerEvent>
    Triggers;
  Proximity;
  
}


// trigger in VRC trigger-action system
// General term seems "event" in event-driven architecture.
// So this is "trigger evnet"
VRCSDK2.VRC_Trigger+TriggerEvent
{
  // OnEnable, OnDestroy, etc.
  VRCSDK2.VRC_Trigger+TriggerType
    TriggerType;

  VRCSDK2.VRC_EventHandler+VrcBroadcastType
    BroadcastType;

  // action
  List<VRCSDK2.VRC_EventHandler+VrcEvent>
    Events;

  string
    Name;
  Others
  Key
  Layers
  LowPeriodTime
  HighPeriodTime
  ResetOnEnable
  Duration
  
  float[]
  Probabilities;
  
  MidiChannel
  MidiNote
  OscAddr
}



// action
VRCSDK2.VRC_EventHandler+VrcEvent
{
  string
    Name;
  VRCSDK2.VRC_EventHandler+VrcEventType
    EventType;
  string
    ParameterString;
  VRCSDK2.VRC_EventHandler+VrcBooleanOp
    ParameterBoolOp;
  float
    ParameterFloat;
  int
    ParameterInt;

  // action Recievers
  UnityEngine.GameObject[]
    ParameterObjects;
}



enum VRCSDK2.VRC_Trigger+TriggerType
{
  Custom,
  Relay,
  OnEnable,
  OnDestroy,
  OnSpawn,
  OnNetworkReady,
  OnPlayerJoined,
  OnPlayerLeft,
  OnPickupUseDown,
  OnPickupUseUp,
  OnTimer,
  OnEnterTrigger,
  OnExitTrigger,
  OnKeyDown,
  OnKeyUp,
  OnPickup,
  OnDrop,
  OnInteract,
  OnEnterCollider,
  OnExitCollider,
  OnDataStorageChange,
  OnDataStorageRemove,
  OnDataStorageAdd,
  OnAvatarHit,
  OnStationEntered,
  OnStationExited,
  OnVideoStart,
  OnVideoEnd,
  OnVideoPlay,
  OnVideoPause,
  OnDisable,
  OnOwnershipTransfer,
  OnParticleCollision,
}

enum VRCSDK2.VRC_EventHandler+VrcBroadcastType
{
  Always,
  Master,
  Local,
  Owner,
  AlwaysUnbuffered,
  MasterUnbuffered,
  OwnerUnbuffered,
  AlwaysBufferOne,
  MasterBufferOne,
  OwnerBufferOne,
}


enum VRCSDK2.VRC_EventHandler+VrcEventType
{
  MeshVisibility,
  AnimationFloat,
  AnimationBool,
  AnimationTrigger,
  AudioTrigger,
  PlayAnimation,
  SendMessage,
  SetParticlePlaying,
  TeleportPlayer,
  RunConsoleCommand,
  SetGameObjectActive,
  SetWebPanelURI,
  SetWebPanelVolume,
  SpawnObject,
  SendRPC,
  ActivateCustomTrigger,
  DestroyObject,
  SetLayer,
  SetMaterial,
  AddHealth,
  AddDamage,
  SetComponentActive,
  AnimationInt,
  AnimationIntAdd,
  AnimationIntSubtract,
  AnimationIntMultiply,
  AnimationIntDivide,
  AddVelocity,
  SetVelocity,
  AddAngularVelocity,
  SetAngularVelocity,
  AddForce,
  SetUIText,
}


enum VRCSDK2.VRC_EventHandler+VrcBooleanOp
{
  False,  
  True,
  Toggle,
  Unused,
}



 
FullName:VRCSDK2.VRC_SceneDescriptor
  BaseType:VRCSDK2.VRC_Behaviour
  BaseType:UnityEngine.MonoBehaviour
  BaseType:UnityEngine.Behaviour
  BaseType:UnityEngine.Component
  BaseType:UnityEngine.Object
  BaseType:System.Object
Public Fields {
public UnityEngine.Transform[] spawns;
public VRCSDK2.VRC_SceneDescriptor+SpawnOrder spawnOrder;
public VRCSDK2.VRC_SceneDescriptor+SpawnOrientation spawnOrientation;
public UnityEngine.GameObject ReferenceCamera;
public System.Single RespawnHeightY;
public VRCSDK2.VRC_SceneDescriptor+RespawnHeightBehaviour ObjectBehaviourAtRespawnHeight;
public System.Boolean ForbidUserPortals;
public System.Boolean UseCustomVoiceFalloffRange;
public System.Single VoiceFalloffRangeNear;
public System.Single VoiceFalloffRangeFar;
public System.Boolean autoSpatializeAudioSources;
public UnityEngine.Vector3 gravity;
public System.Boolean[] layerCollisionArr;
public System.Int32 capacity;
public System.Boolean contentSex;
public System.Boolean contentViolence;
public System.Boolean contentGore;
public System.Boolean contentOther;
public System.Boolean releasePublic;
public System.String unityVersion;
public System.String Name;
public System.Boolean NSFW;
public UnityEngine.Vector3 SpawnPosition;
public UnityEngine.Transform SpawnLocation;
public System.Single DrawDistance;
public System.Boolean useAssignedLayers;
public System.Collections.Generic.List`1[[UnityEngine.GameObject, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] DynamicPrefabs;
public System.Collections.Generic.List`1[[UnityEngine.Material, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] DynamicMaterials;
public System.Int32 UpdateTimeInMS;
public UnityEngine.Texture2D[] LightMapsNear;
public UnityEngine.Texture2D[] LightMapsFar;
public UnityEngine.LightmapsMode LightMode;
public UnityEngine.Color RenderAmbientEquatorColor;
public UnityEngine.Color RenderAmbientGroundColor;
public System.Single RenderAmbientIntensity;
public UnityEngine.Color RenderAmbientLight;
public UnityEngine.Rendering.AmbientMode RenderAmbientMode;
public UnityEngine.Rendering.SphericalHarmonicsL2 RenderAmbientProbe;
public UnityEngine.Color RenderAmbientSkyColor;
public System.Boolean RenderFog;
public UnityEngine.Color RenderFogColor;
public UnityEngine.FogMode RenderFogMode;
public System.Single RenderFogDensity;
public System.Single RenderFogLinearStart;
public System.Single RenderFogLinearEnd;
public System.Single RenderHaloStrength;
public System.Single RenderFlareFadeSpeed;
public System.Single RenderFlareStrength;
public UnityEngine.Cubemap RenderCustomReflection;
public UnityEngine.Rendering.DefaultReflectionMode RenderDefaultReflectionMode;
public System.Int32 RenderDefaultReflectionResolution;
public System.Int32 RenderReflectionBounces;
public System.Single RenderReflectionIntensity;
public UnityEngine.Material RenderSkybox;
public VRCSDK2.VRC_SceneDescriptor+IntializationDelegate Initialize;
public System.Object apiWorld;
}  
declared Members {
Method Name=GetPrefab
Method Name=GetMaterial
Method Name=get_Instance
Constructor Name=.ctor
Property Name=Instance
Field Name=spawns
Field Name=spawnOrder
Field Name=spawnOrientation
Field Name=ReferenceCamera
Field Name=RespawnHeightY
Field Name=ObjectBehaviourAtRespawnHeight
Field Name=ForbidUserPortals
Field Name=UseCustomVoiceFalloffRange
Field Name=VoiceFalloffRangeNear
Field Name=VoiceFalloffRangeFar
Field Name=autoSpatializeAudioSources
Field Name=gravity
Field Name=layerCollisionArr
Field Name=capacity
Field Name=contentSex
Field Name=contentViolence
Field Name=contentGore
Field Name=contentOther
Field Name=releasePublic
Field Name=unityVersion
Field Name=Name
Field Name=NSFW
Field Name=SpawnPosition
Field Name=SpawnLocation
Field Name=DrawDistance
Field Name=useAssignedLayers
Field Name=DynamicPrefabs
Field Name=DynamicMaterials
Field Name=UpdateTimeInMS
Field Name=LightMapsNear
Field Name=LightMapsFar
Field Name=LightMode
Field Name=RenderAmbientEquatorColor
Field Name=RenderAmbientGroundColor
Field Name=RenderAmbientIntensity
Field Name=RenderAmbientLight
Field Name=RenderAmbientMode
Field Name=RenderAmbientProbe
Field Name=RenderAmbientSkyColor
Field Name=RenderFog
Field Name=RenderFogColor
Field Name=RenderFogMode
Field Name=RenderFogDensity
Field Name=RenderFogLinearStart
Field Name=RenderFogLinearEnd
Field Name=RenderHaloStrength
Field Name=RenderFlareFadeSpeed
Field Name=RenderFlareStrength
Field Name=RenderCustomReflection
Field Name=RenderDefaultReflectionMode
Field Name=RenderDefaultReflectionResolution
Field Name=RenderReflectionBounces
Field Name=RenderReflectionIntensity
Field Name=RenderSkybox
Field Name=Initialize
Field Name=apiWorld
NestedType Name=IntializationDelegate
NestedType Name=RespawnHeightBehaviour
NestedType Name=SpawnOrientation
NestedType Name=SpawnOrder
}


---------------------
FullName:VRCSDK2.VRC_SceneDescriptor
  BaseType:VRCSDK2.VRC_SceneDescriptor
  BaseType:VRCSDK2.VRC_Behaviour
  BaseType:UnityEngine.MonoBehaviour
  BaseType:UnityEngine.Behaviour
  BaseType:UnityEngine.Component
  BaseType:UnityEngine.Object
  BaseType:System.Object

Public Fields {
 public UnityEngine.Transform[] spawns;
 public VRCSDK2.VRC_SceneDescriptor+SpawnOrder spawnOrder;
 public VRCSDK2.VRC_SceneDescriptor+SpawnOrientation spawnOrientation;
 public UnityEngine.GameObject ReferenceCamera;
 public System.Single RespawnHeightY;
 public VRCSDK2.VRC_SceneDescriptor+RespawnHeightBehaviour ObjectBehaviourAtRespawnHeight;
 public System.Boolean ForbidUserPortals;
 public System.Boolean UseCustomVoiceFalloffRange;
 public System.Single VoiceFalloffRangeNear;
 public System.Single VoiceFalloffRangeFar;
[UnityEngine.HideInInspector] public System.Boolean autoSpatializeAudioSources;
[UnityEngine.HideInInspector] public UnityEngine.Vector3 gravity;
[UnityEngine.HideInInspector] public System.Boolean[] layerCollisionArr;
[UnityEngine.HideInInspector] public System.Int32 capacity;
[UnityEngine.HideInInspector] public System.Boolean contentSex;
[UnityEngine.HideInInspector] public System.Boolean contentViolence;
[UnityEngine.HideInInspector] public System.Boolean contentGore;
[UnityEngine.HideInInspector] public System.Boolean contentOther;
[UnityEngine.HideInInspector] public System.Boolean releasePublic;
 public System.String unityVersion;
[System.ObsoleteAttribute][UnityEngine.HideInInspector] public System.String Name;
[UnityEngine.HideInInspector][System.ObsoleteAttribute] public System.Boolean NSFW;
[UnityEngine.HideInInspector] public UnityEngine.Vector3 SpawnPosition;
[UnityEngine.HideInInspector] public UnityEngine.Transform SpawnLocation;
[UnityEngine.HideInInspector] public System.Single DrawDistance;
[UnityEngine.HideInInspector] public System.Boolean useAssignedLayers;
 public System.Collections.Generic.List`1[[UnityEngine.GameObject, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] DynamicPrefabs;
 public System.Collections.Generic.List`1[[UnityEngine.Material, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] DynamicMaterials;
[UnityEngine.RangeAttribute] public System.Int32 UpdateTimeInMS;
[UnityEngine.HideInInspector] public UnityEngine.Texture2D[] LightMapsNear;
[UnityEngine.HideInInspector] public UnityEngine.Texture2D[] LightMapsFar;
[UnityEngine.HideInInspector] public UnityEngine.LightmapsMode LightMode;
[UnityEngine.HideInInspector] public UnityEngine.Color RenderAmbientEquatorColor;
[UnityEngine.HideInInspector] public UnityEngine.Color RenderAmbientGroundColor;
[UnityEngine.HideInInspector] public System.Single RenderAmbientIntensity;
[UnityEngine.HideInInspector] public UnityEngine.Color RenderAmbientLight;
[UnityEngine.HideInInspector] public UnityEngine.Rendering.AmbientMode RenderAmbientMode;
[UnityEngine.HideInInspector] public UnityEngine.Rendering.SphericalHarmonicsL2 RenderAmbientProbe;
[UnityEngine.HideInInspector] public UnityEngine.Color RenderAmbientSkyColor;
[UnityEngine.HideInInspector] public System.Boolean RenderFog;
[UnityEngine.HideInInspector] public UnityEngine.Color RenderFogColor;
[UnityEngine.HideInInspector] public UnityEngine.FogMode RenderFogMode;
[UnityEngine.HideInInspector] public System.Single RenderFogDensity;
[UnityEngine.HideInInspector] public System.Single RenderFogLinearStart;
[UnityEngine.HideInInspector] public System.Single RenderFogLinearEnd;
[UnityEngine.HideInInspector] public System.Single RenderHaloStrength;
[UnityEngine.HideInInspector] public System.Single RenderFlareFadeSpeed;
[UnityEngine.HideInInspector] public System.Single RenderFlareStrength;
[UnityEngine.HideInInspector] public UnityEngine.Cubemap RenderCustomReflection;
[UnityEngine.HideInInspector] public UnityEngine.Rendering.DefaultReflectionMode RenderDefaultReflectionMode;
[UnityEngine.HideInInspector] public System.Int32 RenderDefaultReflectionResolution;
[UnityEngine.HideInInspector] public System.Int32 RenderReflectionBounces;
[UnityEngine.HideInInspector] public System.Single RenderReflectionIntensity;
[UnityEngine.HideInInspector] public UnityEngine.Material RenderSkybox;
 public VRCSDK2.VRC_SceneDescriptor+IntializationDelegate Initialize;
[UnityEngine.HideInInspector] public System.Object apiWorld;
}

Members {
 Method Name=GetPrefab
 Method Name=GetMaterial
 Method Name=get_Instance
 Constructor Name=.ctor
 Property Name=Instance
 Field Name=spawns
 Field Name=spawnOrder
 Field Name=spawnOrientation
 Field Name=ReferenceCamera
 Field Name=RespawnHeightY
 Field Name=ObjectBehaviourAtRespawnHeight
 Field Name=ForbidUserPortals
 Field Name=UseCustomVoiceFalloffRange
 Field Name=VoiceFalloffRangeNear
 Field Name=VoiceFalloffRangeFar
 Field Name=autoSpatializeAudioSources
 Field Name=gravity
 Field Name=layerCollisionArr
 Field Name=capacity
 Field Name=contentSex
 Field Name=contentViolence
 Field Name=contentGore
 Field Name=contentOther
 Field Name=releasePublic
 Field Name=unityVersion
 Field Name=Name
 Field Name=NSFW
 Field Name=SpawnPosition
 Field Name=SpawnLocation
 Field Name=DrawDistance
 Field Name=useAssignedLayers
 Field Name=DynamicPrefabs
 Field Name=DynamicMaterials
 Field Name=UpdateTimeInMS
 Field Name=LightMapsNear
 Field Name=LightMapsFar
 Field Name=LightMode
 Field Name=RenderAmbientEquatorColor
 Field Name=RenderAmbientGroundColor
 Field Name=RenderAmbientIntensity
 Field Name=RenderAmbientLight
 Field Name=RenderAmbientMode
 Field Name=RenderAmbientProbe
 Field Name=RenderAmbientSkyColor
 Field Name=RenderFog
 Field Name=RenderFogColor
 Field Name=RenderFogMode
 Field Name=RenderFogDensity
 Field Name=RenderFogLinearStart
 Field Name=RenderFogLinearEnd
 Field Name=RenderHaloStrength
 Field Name=RenderFlareFadeSpeed
 Field Name=RenderFlareStrength
 Field Name=RenderCustomReflection
 Field Name=RenderDefaultReflectionMode
 Field Name=RenderDefaultReflectionResolution
 Field Name=RenderReflectionBounces
 Field Name=RenderReflectionIntensity
 Field Name=RenderSkybox
 Field Name=Initialize
 Field Name=apiWorld
 NestedType Name=IntializationDelegate
 NestedType Name=RespawnHeightBehaviour
 NestedType Name=SpawnOrientation
 NestedType Name=SpawnOrder
}


------------------------------

Name:VRC_Pickup
FullName:VRCSDK2.VRC_Pickup
Public Fields {
 public System.Boolean DisallowTheft;
 public UnityEngine.Rigidbody physicalRoot;
 public UnityEngine.Transform ExactGun;
 public UnityEngine.Transform ExactGrip;
 public System.Boolean allowManipulationWhenEquipped;
 public VRCSDK2.VRC_Pickup+PickupOrientation orientation;
 public VRCSDK2.VRC_Pickup+AutoHoldMode AutoHold;
 public System.String UseText;
[UnityEngine.HideInInspector][System.ObsoleteAttribute] public VRCSDK2.VRC_EventHandler+VrcBroadcastType useEventBroadcastType;
[UnityEngine.HideInInspector][System.ObsoleteAttribute] public System.String UseDownEventName;
[System.ObsoleteAttribute][UnityEngine.HideInInspector] public System.String UseUpEventName;
[System.ObsoleteAttribute][UnityEngine.HideInInspector] public VRCSDK2.VRC_EventHandler+VrcBroadcastType pickupDropEventBroadcastType;
[UnityEngine.HideInInspector][System.ObsoleteAttribute] public System.String PickupEventName;
[System.ObsoleteAttribute][UnityEngine.HideInInspector] public System.String DropEventName;
 public System.Single ThrowVelocityBoostMinSpeed;
 public System.Single ThrowVelocityBoostScale;
[UnityEngine.HideInInspector] public UnityEngine.Component currentlyHeldBy;
[UnityEngine.HideInInspector] public VRCSDK2.VRC_PlayerApi currentLocalPlayer;
 public System.Boolean pickupable;
[UnityEngine.RangeAttribute] public System.Single proximity;
 public VRCSDK2.VRC_Pickup+AwakeDelegate OnAwake;
 public VRCSDK2.VRC_Pickup+ForceDropDelegate ForceDrop;
 public VRCSDK2.VRC_Pickup+OnDestroyedDelegate OnDestroyed;
 public VRCSDK2.VRC_Pickup+HapticEventDelegate HapticEvent;
[UnityEngine.HideInInspector] public System.Boolean originalKinematic;
[UnityEngine.HideInInspector] public System.Boolean originalGravity;
[UnityEngine.HideInInspector] public System.Boolean originalTrigger;
[UnityEngine.HideInInspector] public UnityEngine.Transform originalParent;
 public System.Func`2[[VRCSDK2.VRC_Pickup, VRCSDK2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=67033c44591afb45],[VRCSDK2.VRC_Pickup+PickupHand, VRCSDK2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=67033c44591afb45]] _GetPickupHand;
 public System.Func`2[[VRCSDK2.VRC_Pickup, VRCSDK2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=67033c44591afb45],[VRCSDK2.VRC_PlayerApi, VRCSDK2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=67033c44591afb45]] _GetCurrentPlayer;
}
  BaseType:VRCSDK2.VRC_Pickup
  BaseType:VRCSDK2.VRC_Behaviour
  BaseType:UnityEngine.MonoBehaviour
  BaseType:UnityEngine.Behaviour
  BaseType:UnityEngine.Component
  BaseType:UnityEngine.Object
  BaseType:System.Object
Members {
 Method Name=get_NetworkID
 Method Name=set_NetworkID
 Method Name=get_currentPlayer
 Method Name=get_IsHeld
 Method Name=get_currentHand
 Method Name=Drop
 Method Name=Drop
 Method Name=GenerateHapticEvent
 Method Name=RevertPhysics
 Method Name=PlayHaptics
 Method Name=ProvideEvents
 Constructor Name=.ctor
 Property Name=NetworkID
 Property Name=currentPlayer
 Property Name=IsHeld
 Property Name=currentHand
 Field Name=DisallowTheft
 Field Name=physicalRoot
 Field Name=ExactGun
 Field Name=ExactGrip
 Field Name=allowManipulationWhenEquipped
 Field Name=orientation
 Field Name=AutoHold
 Field Name=UseText
 Field Name=useEventBroadcastType
 Field Name=UseDownEventName
 Field Name=UseUpEventName
 Field Name=pickupDropEventBroadcastType
 Field Name=PickupEventName
 Field Name=DropEventName
 Field Name=ThrowVelocityBoostMinSpeed
 Field Name=ThrowVelocityBoostScale
 Field Name=currentlyHeldBy
 Field Name=currentLocalPlayer
 Field Name=pickupable
 Field Name=proximity
 Field Name=OnAwake
 Field Name=ForceDrop
 Field Name=OnDestroyed
 Field Name=HapticEvent
 Field Name=originalKinematic
 Field Name=originalGravity
 Field Name=originalTrigger
 Field Name=originalParent
 Field Name=_GetPickupHand
 Field Name=_GetCurrentPlayer
 NestedType Name=HapticEventDelegate
 NestedType Name=OnDestroyedDelegate
 NestedType Name=ForceDropDelegate
 NestedType Name=AwakeDelegate
 NestedType Name=PickupHand
 NestedType Name=AutoHoldMode
 NestedType Name=PickupOrientation
}


------------
VRCSDK2.VRC_Pickup+AutoHoldMode, VRCSDK2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=67033c44591afb45
Enum {
AutoDetect,
Yes,
No,
}
  
