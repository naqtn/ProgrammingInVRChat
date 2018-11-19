# Need to check items

Items Need to investigate original specs.


## Trigger

### CHECK What happends if multiple VRC_Trigger compnent exists in case of original VRChat client?
Simply they do them work independently?

### Activate custom trigger infinite looping
Make a testcase.

### Execution order of OnSpawn and OnEnable


## Action

### Multiple locations for SpawnObject
Make a testcase.


## Collision

### CHECK Does Layers of OnEnterTrigger affect collision matrix?

I guess no, and it just likes a filer. Is it right?
(Also for similar triggers)


### CHECK If a colliding object is destroied while inside of VRC_Trigger containing object, what happens for "TriggerIndividuals" feature?


### CHECK Trigger collider is inside at startup. OnExitTrigger happens regularly?


### CHECK If an object layer changes while inside of trigger collider, OnExitTrigger doesnt happens?
OnEnterTrigger count and OnExitTrigger count can be unbalanced?


### CHECK Does changing layer affect to TriggerIndividuals-off behavior?
1. Object-C OnExitTrigger layer is Foo
2. Move Object-A with layer Foo to inside C
3. Move Object-B with layer Bar to inside C, and change layer to Foo
4. Move Object-A outside C, OnExitTrigger happens or not?



## Lifecycle

### TEST Destory while holding (pickuped)

---
end
