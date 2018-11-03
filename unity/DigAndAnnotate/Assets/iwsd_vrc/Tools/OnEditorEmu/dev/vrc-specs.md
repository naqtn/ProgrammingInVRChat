# VRChat specifications

A note for trivial specifications.

## Others

### OnInteract and OnPickup can triggerd at onece
If VRC_Trigger has entries both OnPickup and OnPickup, They can be called at onece when user pickup.

Tested ver 2018.3.3


### SendRPC only holds method name
So if a object has two (or more) components that have identical name method, it is impossible to select one.
(or unbiguous or not obvious) 


## OnTimer trigger

TBD investigate again if VRChat client defect is closed.
> OnTimer "Reset On Enable" does not reset its started time
> https://vrchat.canny.io/bug-reports/p/ontimer-reset-on-enable-does-not-reset-its-started-time


It is not clearly defined especially for disbled options.

1) In inactive state, does it measure time (consume time) or doesn't
2) If answer of Q1 is yes, what happens after specified period
3) If "Reset On Enable" is false, what happens when GameObject become active
4) While the document reffers GameObject active/inactive, how about VRC_Trigger comonent enable/disenable

case 1: Repeat true, ResetOnEnable false

- Even if GameObject is inactive, it continue to measure (consume time)
    - If it activated again while in the period, it fires after specified time.
    - If timer expires while inactive, next activation immidiate fires trigger.
        - After that next repeated timer starts.
        - In other words, next period doesn't start though current period measurement continues.
    - It is same for VRC_Trigger disenabled.

case 2: Repeat true, ResetOnEnable true

- It reset timer on disable (!)
    - It starts next measurement from on disable.
    - > RepeatTrue-ResetOnEnableTrue-THEN-ResetOnDisable.mp4
    - If you inactivate-activete twice, the period starts from second inactivation.
- Same to case 1, even if GameObject is inactive, it continue to measure and fires on next activation.

case 3: Repeat false, ResetOnEnable false
case 4: Repeat false, ResetOnEnable true


---
end
