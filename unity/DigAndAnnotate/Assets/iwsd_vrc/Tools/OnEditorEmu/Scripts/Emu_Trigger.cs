using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Iwsd
{

    class Val_Trigger
    {
        // string InteractText;
        public List<VRCSDK2.VRC_Trigger.TriggerEvent> Triggers;
        // Proximity;

        public Val_Trigger(VRCSDK2.VRC_Trigger vrcTrigger)
        {
            this.Triggers = DeepCopyHelper.DeepCopy<List<VRCSDK2.VRC_Trigger.TriggerEvent>>(vrcTrigger.Triggers); 
        }
    }
    
    
    class Emu_Trigger : MonoBehaviour
    {

        // TODO Make trigger definition visible with Unity inspector to debug scene.
        
        Val_Trigger vrcTrigger;
        public string debugString;

        // It seems that VRCSDK2.VRC_Trigger destroy itself runtime on UnityEditor.
        // So copy its definition content to Val_Trigger.
        public void SetupFrom(VRCSDK2.VRC_Trigger from)
        {
            vrcTrigger = new Val_Trigger(from);
        }
        
        ////////////////////////////////////////////////////////////
        // Public interface
        
        void OnEnable()
        {
            Iwlog.Trace(gameObject, "Emu_Trigger:OnEnable:" + debugString);
        }

        void Start()
        {
            Iwlog.Trace(gameObject, "Emu_Trigger:Start: debugString=" + debugString);
        }

        ////////////////////////////////////////
        // VRCSDK2.VRC_Trigger+TriggerType

        // Custom,  // from program (Animation, uGUI)
        void ExecuteCustomTrigger(string name)
        {
            Iwlog.Trace("Emu_Trigger:ExecuteCustomTrigger name='" + name + "'");
            ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.Custom, name);
        }

        // Relay,
        // OnEnable,
        // OnDestroy,
        // OnSpawn,
        // OnNetworkReady,
        // OnPlayerJoined,
        // OnPlayerLeft,
        // x OnPickupUseDown, // from Player
        // x OnPickupUseUp, // from Player
        // OnTimer,
        // OnEnterTrigger, // Collider.OnTriggerEnter
        void OnTriggerEnter(Collider other)
        {
            ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnEnterTrigger);
        }
        // OnExitTrigger, // Collider.OnTriggerExit
        void OnTriggerExit(Collider other)
        {
            ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnEnterTrigger);
        }
        // OnKeyDown,
        // OnKeyUp,
        // x OnPickup, // from Player
        // x OnDrop, // from Player
        // x OnInteract, // from Player
        // OnEnterCollider, // Collider.OnCollisionEnter
        void OnCollisionEnter(Collision collision)
        {
            ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnEnterCollider);
        }
        // OnExitCollider, // Collider.OnCollisionExit
        void OnCollisionExit(Collision collision)
        {
            ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnExitCollider);
        }
        // OnDataStorageChange,
        // OnDataStorageRemove, // hidden
        // OnDataStorageAdd, // hidden
        // OnAvatarHit,
        // OnStationEntered,
        // OnStationExited,
        // OnVideoStart,
        // OnVideoEnd,
        // OnVideoPlay,
        // OnVideoPause,
        // OnDisable,
        // OnOwnershipTransfer,
        // OnParticleCollision, // MonoBehaviour.OnParticleCollision
        void OnParticleCollision(GameObject other)
        {
            ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnParticleCollision);
        }

        ////////////////////////////////////////////////////////////
        // internal interface

        internal bool HasTriggerOf(VRCSDK2.VRC_Trigger.TriggerType triggerType)
        {
            var triggers = SearchTrigger(triggerType);
            return triggers.GetEnumerator().MoveNext();
        }

        // CHECK What happends if multiple VRC_Trigger compnent exists in case of original VRChat client?
        // Simply they do them work independently?

        internal void ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType triggerType, string name = null)
        {
            Iwlog.Trace(gameObject, "Emu_Trigger:ExecuteTriggers type=" + triggerType);

            var triggers = SearchTrigger(triggerType, name);
            foreach (var triggerDef in triggers) {
                ExecuteTriggerActions(triggerDef);
            }
        }

        ////////////////////////////////////////////////////////////
        // private implementation
        
        private IEnumerable<VRCSDK2.VRC_Trigger.TriggerEvent> SearchTrigger(VRCSDK2.VRC_Trigger.TriggerType triggerType, string name = null)
        {
            if (this.vrcTrigger == null) {
                Iwlog.Error(this.gameObject, "this.vrcTrigger==null");
                return Enumerable.Empty<VRCSDK2.VRC_Trigger.TriggerEvent>();
            }

            var query = vrcTrigger.Triggers
                .Where(x => (x.TriggerType == triggerType) && ((name == null) || (x.Name == name)));

            switch (query.Count()) {
                case 0:
                    Iwlog.Warn(this.gameObject, "Emu_Trigger: TriggerEvent not found. type=" + triggerType + ", name='" + name + "'");
                    break;
                case 1:
                    break;
                default:
                    // CHECK What happens in case of original VRChat client?
                    Iwlog.Warn(this.gameObject, "Emu_Trigger: Multiple TriggerEvent. type=" + triggerType + ", name='" + name + "'");
                    break;
            }
            return query;
        }

        static System.Random random = new System.Random(0);

        private void ExecuteTriggerActions(VRCSDK2.VRC_Trigger.TriggerEvent triggerEvent)
        {
            if (triggerEvent.Probabilities.Length == 0) {
                foreach (var vrcEvent in triggerEvent.Events) {
                    ExecuteTriggerAction(vrcEvent);
                }
            } else {
                // assumes probabilities is nomarilized
                float psum = triggerEvent.Probabilities.Sum();
                double rand = random.NextDouble();
                Iwlog.Trace2("Do Rundomized trigger: probabilities psum=" + psum + ", rand=" + rand);
                for (int i = 0; i < triggerEvent.Probabilities.Length; i++) {
                    rand -= triggerEvent.Probabilities[i];
                    if (rand <= 0) { // CHECK need equal?
                        Iwlog.Trace2("Do Rundomized trigger: idx=" + i);
                        ExecuteTriggerAction(triggerEvent.Events[i]);
                        break;
                    }
                }
            }
        }

        enum ActionResult
        {
            ComponentMissing,
            WrongParameter,
            IllegalParameter,
            UnexpectedParameterFormat,
            Success,
        }

        // dispatch
        private void ExecuteTriggerAction(VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            IEnumerable<ActionResult>  results = DispatchActions(vrcEvent);

            int c = results.Count(); // execute
            // TODO return result value (upto caller stack)

            Iwlog.Trace2("result count=" + c);
        }

        private IEnumerable<ActionResult> DispatchActions(VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            // REFINE To be more declarative code

            var receivers = GetRecievers(vrcEvent);
            switch (vrcEvent.EventType) {
                // MeshVisibility, // hidden

                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationFloat:
                    return receivers.Select(r => Execute_AnimationFloat(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationBool:
                    return receivers.Select(r => Execute_AnimationBool(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationTrigger:
                    return receivers.Select(r => Execute_AnimationTrigger(r, vrcEvent));

                case VRCSDK2.VRC_EventHandler.VrcEventType.AudioTrigger:
                    return receivers.Select(r => Execute_AudioTrigger(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.PlayAnimation:
                    return receivers.Select(r => Execute_PlayAnimation(r, vrcEvent));
                //  SendMessage, // hidden
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetParticlePlaying:
                    return receivers.Select(r => Execute_SetParticlePlaying(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.TeleportPlayer:
                    return receivers.Select(r => Execute_TeleportPlayer(r, vrcEvent));
                //  RunConsoleCommand, // hidden
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetGameObjectActive:
                    return receivers.Select(r => Execute_SetGameObjectActive(r, vrcEvent));
                    
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetWebPanelURI:
                    Iwlog.Warn(gameObject, "RIP WebPanel. Invalid action=" + vrcEvent.EventType);
                    return Enumerable.Empty<ActionResult>();
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetWebPanelVolume:
                    Iwlog.Warn(gameObject, "RIP WebPanel. Invalid action=" + vrcEvent.EventType);
                    return Enumerable.Empty<ActionResult>();
                    
                case VRCSDK2.VRC_EventHandler.VrcEventType.SpawnObject:
                    return receivers.Select(r => Execute_SpawnObject(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.SendRPC:
                    return receivers.Select(r => Execute_SendRPC(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.ActivateCustomTrigger:
                    return receivers.Select(r => Execute_ActivateCustomTrigger(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.DestroyObject:
                    return receivers.Select(r => Execute_DestroyObject(r, vrcEvent));

                case VRCSDK2.VRC_EventHandler.VrcEventType.SetLayer:
                    return receivers.Select(r => Execute_SetLayer(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetMaterial:
                    return receivers.Select(r => Execute_SetMaterial(r, vrcEvent));

                //  AddHealth, // TODO Depending VRC_DestructibleStandar and VRC_CombatSystem
                //  AddDamage,

                case VRCSDK2.VRC_EventHandler.VrcEventType.SetComponentActive:
                    return receivers.Select(r => Execute_SetComponentActive(r, vrcEvent));
                    
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationInt:
                    return receivers.Select(r => Execute_AnimationInt(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationIntAdd:
                    return receivers.Select(r => Execute_AnimationIntAdd(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationIntSubtract:
                    return receivers.Select(r => Execute_AnimationIntSubtract(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationIntMultiply:
                    return receivers.Select(r => Execute_AnimationIntMultiply(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.AnimationIntDivide:
                    return receivers.Select(r => Execute_AnimationIntDivide(r, vrcEvent));
                    
                case VRCSDK2.VRC_EventHandler.VrcEventType.AddVelocity:
                    return receivers.Select(r => Execute_AddVelocity(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetVelocity:
                    return receivers.Select(r => Execute_SetVelocity(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.AddAngularVelocity:
                    return receivers.Select(r => Execute_AddAngularVelocity(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.SetAngularVelocity:
                    return receivers.Select(r => Execute_SetAngularVelocity(r, vrcEvent));
                case VRCSDK2.VRC_EventHandler.VrcEventType.AddForce:
                    return receivers.Select(r => Execute_AddForce(r, vrcEvent));

                case VRCSDK2.VRC_EventHandler.VrcEventType.SetUIText:
                    return receivers.Select(r => Execute_SetUIText(r, vrcEvent));
                default:
                    Iwlog.Debug("Not implemented yet. action=" + vrcEvent.EventType);
                    return Enumerable.Empty<ActionResult>();
            }
        }


        private IEnumerable<GameObject> GetRecievers(VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            var receivers = vrcEvent.ParameterObjects;
            if (receivers.Length == 0) {
                receivers = new [] {this.gameObject};
            }
            return receivers.Where(x => x != null);
        }


        //////////////////////////////////////////////////////////////////////
        // Each action

        ////////////////////////////////////////
        // Unity basics

        // ParameterBoolOp : assign value
        private ActionResult Execute_SetGameObjectActive(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            switch (vrcEvent.ParameterBoolOp) {
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.False:
                    receiver.SetActive(false);
                    return ActionResult.Success;
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.True:
                    receiver.SetActive(true);
                    return ActionResult.Success;
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.Toggle:
                    receiver.SetActive(!receiver.activeSelf);
                    return ActionResult.Success;
                default:
                    Iwlog.Error(gameObject, "Unkonown ParameterBoolOp. value='" + vrcEvent.ParameterBoolOp + "'");
                    return ActionResult.IllegalParameter;
            }
        }


        // SetComponentActive,
        // ParameterString : Component name (ex. "UnityEngine.MeshRenderer")
        // ParameterBoolOp : assign value
        // "enabled" property for :  UnityEngine.Behaviour or UnityEngine.Collider or  UnityEngine.Renderer,  UnityEngine.Cloth
        // It seems that this action works with any component which has "enabled" property.
        private ActionResult Execute_SetComponentActive(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            string typeName = vrcEvent.ParameterString;

            typeName = StringUtil.LastPartOf(typeName, '.');

            // swap emulator component
            if (typeName == "VRC_Trigger") {
                typeName = "Emu_Trigger";
            }

            var receiverComp = receiver.GetComponent(typeName);
            if (receiverComp == null) {
                Iwlog.Warn(receiver, "Specified component not found on the receiver. type="
                           + typeName + " (caller='" + this.gameObject.name + "')");
                return ActionResult.ComponentMissing;
            }

            var type = receiverComp.GetType();
            System.Reflection.PropertyInfo pinfo = type.GetProperty("enabled");
            if (pinfo == null) {
                Iwlog.Warn(receiver, "Specified component doesn't support enabled property. type="
                           + typeName + " (caller='" + this.gameObject.name + "')");
                return ActionResult.IllegalParameter;
            }

            // https://stackoverflow.com/questions/34261518/cannot-using-getvalues-and-setvalues-in-project-c-sharp
            // > There is an overload for PropertyInfo.SetValue and PropertyInfo.GetValue without the indexer added in .NET 4.5.
            switch (vrcEvent.ParameterBoolOp) {
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.False:
                    pinfo.SetValue(receiverComp, false, null);
                    return ActionResult.Success;
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.True:
                    pinfo.SetValue(receiverComp, true, null);
                    return ActionResult.Success;
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.Toggle:
                    bool old = (bool)pinfo.GetValue(receiverComp, null);
                    pinfo.SetValue(receiverComp, !old, null);
                    return ActionResult.Success;
                default:
                    Iwlog.Error(gameObject, "Unkonown ParameterBoolOp. value='" + vrcEvent.ParameterBoolOp + "'");
                    return ActionResult.IllegalParameter;
            }
        }


        // ParameterInt : layer [0..31]
        private ActionResult Execute_SetLayer(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            int layer = vrcEvent.ParameterInt;
            receiver.layer = layer;

            return ActionResult.Success;
        }

        
        private ActionResult Execute_SetMaterial(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            var receiverComp = receiver.GetComponent<Renderer>();
            if (receiverComp == null) {
                Iwlog.Warn(receiver, "Renderer component not found on the receiver. (caller='" + this.gameObject.name + "')");
                return ActionResult.ComponentMissing;
            }

            string name = vrcEvent.ParameterString;
            Material material = LocalPlayerContext.GetMaterial(name);
            if (material == null) {
                Iwlog.Warn(gameObject, "SetMaterial: Material not found. name='" + name + "'");
                return ActionResult.IllegalParameter;
            }

            receiver.GetComponent<Renderer>().material = material;

            return ActionResult.Success;
        }


        private ActionResult Execute_SpawnObject(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            string name = vrcEvent.ParameterString;
            GameObject prefab = LocalPlayerContext.GetPrefab(name);
            if (prefab == null) {
                Iwlog.Warn(gameObject, "SpawnObject: Prefab not found. name='" + name + "'");
                return ActionResult.IllegalParameter;
            }

            var t = receiver.transform;
            GameObject newOne = (GameObject) UnityEngine.Object.Instantiate(prefab, t.position, t.rotation);

            // CHECK What happens multiple VRC_Trigger exists in one GameObject?
            
            // Hook emulation code. (see also. startup Setup_TriggersComponents())
            var triggerComps = newOne.GetComponentsInChildren<VRCSDK2.VRC_Trigger>(true); // includeInactive = true
            foreach (var comp in triggerComps)
            {
                var emu_trigger = comp.gameObject.AddComponent<Emu_Trigger>();
                emu_trigger.SetupFrom(comp);
            }
            
            return ActionResult.Success;
        }


        private ActionResult Execute_DestroyObject(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            UnityEngine.Object.Destroy(receiver);
            return ActionResult.Success;
        }
            
        ////////////////////////////////////////
        // Unity Physics related actions

        // REFINE Parameter extraction sould be done outside of receiver loop.
            
        private ActionResult Execute_VectorSub(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent,
                                                 Action<Rigidbody, Vector3, bool> action)
        {
            var receiverComp = receiver.GetComponent<Rigidbody>();
            if (receiverComp == null) {
                Iwlog.Warn(receiver, "Rigidbody component not found on the receiver. (caller='" + this.gameObject.name + "')");
                return ActionResult.ComponentMissing;
            }

            Vector3 velocity;
            bool useWorldSpace;
            ActionResult r = ExtractParameterBytes(vrcEvent, out velocity, out useWorldSpace);
            if (r != ActionResult.Success) {
                return r;
            }
            
            action(receiverComp, velocity, useWorldSpace);

            return ActionResult.Success;
        }

        // triggerEvent.ParameterBytes: Vector4(x,y,z,useWorldSpace(0:false))
        private ActionResult Execute_AddVelocity(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            return Execute_VectorSub(receiver, vrcEvent, (rigidbody, v, useWorldSpace) => {
                    if (useWorldSpace) {
                        rigidbody.velocity += v;
                    } else {
                        rigidbody.velocity += receiver.transform.TransformDirection(v);
                    }
                });
        }


        private ActionResult Execute_SetVelocity(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            return Execute_VectorSub(receiver, vrcEvent, (rigidbody, v, useWorldSpace) => {
                    if (useWorldSpace) {
                        rigidbody.velocity = v;
                    } else {
                        rigidbody.velocity = receiver.transform.TransformDirection(v);
                    }
                });
        }


        private ActionResult Execute_AddAngularVelocity(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            return Execute_VectorSub(receiver, vrcEvent, (rigidbody, v, useWorldSpace) => {
                    if (useWorldSpace) {
                        rigidbody.angularVelocity += v;
                    } else {
                        rigidbody.angularVelocity += receiver.transform.TransformDirection(v);
                    }
                });
        }


        private ActionResult Execute_SetAngularVelocity(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            return Execute_VectorSub(receiver, vrcEvent, (rigidbody, v, useWorldSpace) => {
                    if (useWorldSpace) {
                        rigidbody.angularVelocity = v;
                    } else {
                        rigidbody.angularVelocity = receiver.transform.TransformDirection(v);
                    }
                });
        }


        private ActionResult Execute_AddForce(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            return Execute_VectorSub(receiver, vrcEvent, (rigidbody, v, useWorldSpace) => {
                    if (useWorldSpace) {
                        rigidbody.AddForce(v);
                    } else {
                        rigidbody.AddForce(receiver.transform.TransformDirection(v));
                    }
                });
        }


        
        ////////////////////////////////////////
        // VRC specific actions

        // ParameterString : Custom Trigger name
        private ActionResult Execute_ActivateCustomTrigger(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            var receiverComp = receiver.GetComponent<Emu_Trigger>();
            if (receiverComp == null) {
                Iwlog.Warn(receiver, "VRCTrigger component not found on the receiver. (caller='" + this.gameObject.name + "')");
                return ActionResult.ComponentMissing;
            }

            // TODO Add some check avoiding infinite loop

            string name = vrcEvent.ParameterString;
            receiverComp.ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.Custom, name);

            // TODO getting and return proper result value
            return ActionResult.Success;
        }


        // TeleportPlayer
        // GameObject receiver: target position
        // ParameterBoolOp: AlignRoomToDestination. toggle is meaningless (?) 
        // Teleport local player
        // TODO What happens when multiple receiver in original implementation.
        private ActionResult Execute_TeleportPlayer(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            ActionResult result = ActionResult.Success;

            var position = receiver.transform.position;

            switch (vrcEvent.ParameterBoolOp) {
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.False:
                    LocalPlayerContext.TeleportPlayer(position, false);
                    break;
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.True:
                    LocalPlayerContext.TeleportPlayer(position, true);
                    break;
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.Toggle:
                    Iwlog.Warn(gameObject, "Invalid ParameterBoolOp. value='" + vrcEvent.ParameterBoolOp + "' for TeleportPlayer action");
                    LocalPlayerContext.TeleportPlayer(position, false); // exec anyway
                    break;
                default:
                    Iwlog.Error(gameObject, "Unkonown ParameterBoolOp. value='" + vrcEvent.ParameterBoolOp + "'");
                    result = ActionResult.IllegalParameter;
                    break;
            }

            return result;
        }


        /*
         * SendRPC
         * 
         * Edit-time spec:
         * - components commonly exist in all specified receivers
         * - null element in receivers is just ignored
         * - methods that are Instance, Public, not Inherited are included
         *     - BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
         *     - ("not Inherited" limitaion is nonsense.)
         * - Unity component method are excluded
         *     - ("Unity component" means Namespace of component class contains (!) "UnityEngine" or "UnityEditor" string)
         * - Methods of VRC component which doesn't have VRCSDK2.RPC custom attribute are excluded
         *     - ("VRC component" means Namespace of component class contains "VRCSDK2")
         * - Method list are marged by class Name (not FullName (!))
         * 
         * or
         * - Directly input method name (just as a string without class name)
         * 
         * 
         * Run-time spec:
         * - support only zoro or one argument (? not sure)
         * - need VRCSDK2.RPC custom attribute
         * 
         */

        // SendRPC
        // ParameterInt: Targets (VRCSDK2.VRC_EventHandler.VrcTargetType)
        // ParameterString: Method name
        //  GetSharedAccessibleMethodsOnGameObjects
        // ParameterBytes: argument (object[])
        //   if argument length < method parameter length => supply tailing (i.e. last) int (passing player id)
        // (ParameterObjects: receivers)
        private ActionResult Execute_SendRPC(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            // REFINE Move argument deserialization step to be out of receiver loop

            // REFINE reduce copy-pasted code (with ExtractParameterBytes)
            byte[] bytes = vrcEvent.ParameterBytes;
            var paramVals = ParameterBytesDeserializer.Deserialize(bytes) as object[];
            if (paramVals == null)
            {
                Iwlog.Error(gameObject, "Unexpected ParameterBytes format (not object[]).");
            }

            var paramTypes = new Type[paramVals.Length];
            for (int i = 0; i < paramVals.Length; i++) {
                paramTypes[i] = paramVals[i].GetType(); // FIXME need care for null? but how??

                Iwlog.Debug(gameObject, " paramTypes[" + i + "]=" + paramTypes[i].FullName);
            }

            var methodName = vrcEvent.ParameterString;

            bool matched = false;
            var comps = receiver.GetComponents<Component>();
            foreach (var comp in comps)
            {
                var type = comp.GetType();

                // TODO "last int as Player id" case.
                // (In that case, there's paramVals.Length is one less?)

                try
                {
                    var methodInfo = type.GetMethod(methodName, paramTypes);
                    if (methodInfo == null) {
                        // null is OK. just unmatched this component.
                        // Iwlog.Debug(gameObject, "no matched method '" + methodName + "' on type=" + type.FullName);
                    } else {
                        // TODO check RPC custom attribute
                        // Iwlog.Debug(gameObject, "Invoke '" + methodName + "' on type=" + type.FullName);
                        methodInfo.Invoke(comp, paramVals);
                        matched = true;
                    }
                }
                catch (System.Reflection.AmbiguousMatchException e)
                {
                    Iwlog.Warn(gameObject, "AmbiguousMatchException. Message='" + e.Message + "'");
                    // REFINE Should I treat this is "real" error ? (or just ignore?)
                }
            }

            if (!matched) {
                Iwlog.Warn(receiver, "No matching method for SendRPC found on this GameObject. method='" + methodName + "'");
            }
            return matched? ActionResult.Success: ActionResult.ComponentMissing;
        }



        ////////////////////////////////////////
        // Unity component actions

        // ParameterString : clip name
        private ActionResult Execute_PlayAnimation(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            // MEMO: Multiple Animation component in one GameObject is not arrowed by Unity
            var receiverComp = receiver.GetComponent<Animation>();
            if (receiverComp == null) {
                Iwlog.Warn(receiver, "Animation component not found on the receiver. (caller='" + this.gameObject.name + "')");
                return ActionResult.ComponentMissing;
            }

            string name = vrcEvent.ParameterString;
            bool r = receiverComp.Play(name);

            return r? ActionResult.Success: ActionResult.WrongParameter;
        }


        // ParameterString : clip name ( audioSource.clip.name )
        private ActionResult Execute_AudioTrigger(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            // Multiple AudioSource may added to one GameObject.
            var receiverComps = receiver.GetComponents<AudioSource>();
            if (receiverComps.Length == 0) {
                Iwlog.Warn(receiver, "AudioSource component(s) not found on the receiver. (caller='" + this.gameObject.name + "')");
                return ActionResult.ComponentMissing;
            }

            bool matched = false;
            string name = vrcEvent.ParameterString;
            foreach (var audioSource in receiverComps) {
                if ((audioSource.clip != null) && (audioSource.clip.name == name))  {
                    matched = true;
                    audioSource.Play();
                }
            }

            return matched? ActionResult.Success: ActionResult.WrongParameter;
        }
        

        // ParameterBoolOp : play or stop or toggle
        private ActionResult Execute_SetParticlePlaying(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            // MEMO: Multiple ParticleSystem component in one GameObject is not arrowed by Unity
            var receiverComp = receiver.GetComponent<ParticleSystem>();
            if (receiverComp == null) {
                Iwlog.Warn(receiver, "ParticleSystem component not found on the receiver. (caller='" + this.gameObject.name + "')");
                return ActionResult.ComponentMissing;
            }

            switch (vrcEvent.ParameterBoolOp) {
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.False:
                    receiverComp.Stop();
                    return ActionResult.Success;
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.True:
                    receiverComp.Play();
                    return ActionResult.Success;
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.Toggle:
                    if (receiverComp.isPlaying) {
                        receiverComp.Stop();
                    } else {
                        receiverComp.Play();
                    }
                    return ActionResult.Success;
                default:
                    Iwlog.Error(gameObject, "Unkonown ParameterBoolOp. value='" + vrcEvent.ParameterBoolOp + "'");
                    return ActionResult.IllegalParameter;
            }
        }
        

        // ParameterString : text string
        private ActionResult Execute_SetUIText(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            // MEMO: Multiple UI component in one GameObject is not arrowed by Unity
            var receiverComp = receiver.GetComponent<UnityEngine.UI.Text>();
            if (receiverComp == null) {
                Iwlog.Warn(receiver, "Text component not found on the receiver. (caller='" + this.gameObject.name + "')");
                return ActionResult.ComponentMissing;
            }

            string text = vrcEvent.ParameterString;
            receiverComp.text = text;

            return ActionResult.Success;
        }

     
        ////////////////////////////////////////
        // Animation actions
        static bool checkAnimationParameterType = true;

        private ActionResult GetAnimationAndCheckParameterName(GameObject receiver,
                                                               VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent,
                                                               AnimatorControllerParameterType animParamType,
                                                               out Animator animator)
        {
            animator = receiver.GetComponent<Animator>();
            if (animator == null) {
                Iwlog.Warn(receiver, "Animation component not found on the receiver. (caller='" + this.gameObject.name + "')");
                return ActionResult.ComponentMissing;
            }


            // Note:
            // Animator.GetParameter: argument is int index
            // Animator.GetBool: argument is string or id

            if (checkAnimationParameterType) {
                string paramStr = vrcEvent.ParameterString;

                int paramIdx =  Array.FindIndex(animator.parameters, x => x.name == paramStr);
                if (paramIdx < 0) {
                    Iwlog.Warn(gameObject, "Animation parameter not found. name='" + paramStr + "'");
                    return ActionResult.WrongParameter;
                } else {
                    var animParameter = animator.parameters[paramIdx];
                    if (animParameter.type != animParamType) {
                        Iwlog.Warn(gameObject, "Animation parameter type mismatched. name='" + paramStr
                                   + "', required=" + animParamType + ", actual=" + animParameter.type);
                        return ActionResult.WrongParameter;
                    }
                }
            }

            return ActionResult.Success;
        }

        
        // ParameterString : Animation parameter name. expects Float parameter.
        // ParameterFloat : store value
        private ActionResult Execute_AnimationFloat(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            Animator animator;
            var result = GetAnimationAndCheckParameterName(receiver, vrcEvent, AnimatorControllerParameterType.Float, out animator);
            if (result == ActionResult.Success) {

                animator.SetFloat(vrcEvent.ParameterString, vrcEvent.ParameterFloat);
            }
            return result;
        }


        // ParameterString : Animation parameter name. expects Trigger parameter.
        // (Set only. no way to Reset)
        private ActionResult Execute_AnimationTrigger(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            Animator animator;
            var result = GetAnimationAndCheckParameterName(receiver, vrcEvent, AnimatorControllerParameterType.Trigger, out animator);
            if (result == ActionResult.Success) {

                animator.SetTrigger(vrcEvent.ParameterString);
            }
            return result;
        }


        // ParameterString : Animation parameter name. expects Integer parameter.
        // ParameterInt : store value
        private ActionResult Execute_AnimationInt(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            Animator animator;
            var result = GetAnimationAndCheckParameterName(receiver, vrcEvent, AnimatorControllerParameterType.Int, out animator);
            if (result == ActionResult.Success) {

                animator.SetInteger(vrcEvent.ParameterString, vrcEvent.ParameterInt);
            }
            return result;
        }

        
        // ParameterString : Animation parameter name. expects Integer parameter.
        // ParameterInt : adding value
        private ActionResult Execute_AnimationIntAdd(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            Animator animator;
            var result = GetAnimationAndCheckParameterName(receiver, vrcEvent, AnimatorControllerParameterType.Int, out animator);
            if (result == ActionResult.Success) {

                int hashId = Animator.StringToHash(vrcEvent.ParameterString);
                animator.SetInteger(hashId, animator.GetInteger(hashId) + vrcEvent.ParameterInt);
            }
            return result;
        }

        
        // ParameterString : Animation parameter name. expects Integer parameter.
        // ParameterInt : value
        // (Constant only :( )
        private ActionResult Execute_AnimationIntSubtract(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            Animator animator;
            var result = GetAnimationAndCheckParameterName(receiver, vrcEvent, AnimatorControllerParameterType.Int, out animator);
            if (result == ActionResult.Success) {

                int hashId = Animator.StringToHash(vrcEvent.ParameterString);
                animator.SetInteger(hashId, animator.GetInteger(hashId) - vrcEvent.ParameterInt);
            }
            return result;
        }

        
        // ParameterString : Animation parameter name. expects Integer parameter.
        // ParameterInt : value
        private ActionResult Execute_AnimationIntMultiply(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            Animator animator;
            var result = GetAnimationAndCheckParameterName(receiver, vrcEvent, AnimatorControllerParameterType.Int, out animator);
            if (result == ActionResult.Success) {

                int hashId = Animator.StringToHash(vrcEvent.ParameterString);
                animator.SetInteger(hashId, animator.GetInteger(hashId) * vrcEvent.ParameterInt);
            }
            return result;
        }

        
        // ParameterString : Animation parameter name. expects Integer parameter.
        // ParameterInt : value
        private ActionResult Execute_AnimationIntDivide(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            Animator animator;
            var result = GetAnimationAndCheckParameterName(receiver, vrcEvent, AnimatorControllerParameterType.Int, out animator);
            if (result == ActionResult.Success) {

                int hashId = Animator.StringToHash(vrcEvent.ParameterString);
                animator.SetInteger(hashId, animator.GetInteger(hashId) / vrcEvent.ParameterInt);
            }
            return result;
        }

        
        // ParameterString : Animation parameter name. expects Bool parameter.
        // ParameterBoolOp : assign value
        private ActionResult Execute_AnimationBool(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            Animator animator;
            var result = GetAnimationAndCheckParameterName(receiver, vrcEvent, AnimatorControllerParameterType.Bool, out animator);
            if (result == ActionResult.Success) {

                int hashId = Animator.StringToHash(vrcEvent.ParameterString);
                switch (vrcEvent.ParameterBoolOp) {
                    case VRCSDK2.VRC_EventHandler.VrcBooleanOp.False:
                        animator.SetBool(hashId, false);
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcBooleanOp.True:
                        animator.SetBool(hashId, true);
                        break;
                    case VRCSDK2.VRC_EventHandler.VrcBooleanOp.Toggle:
                        animator.SetBool(hashId, !animator.GetBool(hashId));
                        break;
                    default:
                        Iwlog.Error(gameObject, "Unkonown ParameterBoolOp. value='" + vrcEvent.ParameterBoolOp + "'");
                        result = ActionResult.IllegalParameter;
                        break;
                }
                
            }
            return result;
        }


        //////////////////////////////////////////////////////////////////////
        // extract ParameterBytes

        private ActionResult ExtractParameterBytes(VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent, out Vector4 p0)
        {
            byte[] bytes = vrcEvent.ParameterBytes;

            var paramVals = ParameterBytesDeserializer.Deserialize(bytes) as object[];

            if (paramVals == null)
            {
                Iwlog.Error(gameObject, "Unexpected ParameterBytes format (not object[]).");
            }
            else if (paramVals.Length != 1)
            {
                Iwlog.Error(gameObject, "Unexpected ParameterBytes format (Length != 1).");
            }
            else
            {
                Vector4? param0 = paramVals[0] as Vector4?;
                if (param0 == null)
                {
                    Iwlog.Error(gameObject, "Unexpected ParameterBytes format (not Vector4).");
                }
                else
                {
                    p0 = param0.Value;
                    return ActionResult.Success;
                }
            }
            p0 = Vector4.zero;
            return ActionResult.UnexpectedParameterFormat;
        }

        private ActionResult ExtractParameterBytes(VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent, out Vector3 p0, out bool p1)
        {
            // MEMO Why packed in Vector4 ??? It's nonsense.
            Vector4 v4;
            ActionResult r = ExtractParameterBytes(vrcEvent, out v4);

            // anyway use out value
            p0 = v4; // Convert. Drop w.
            p1 = Convert.ToBoolean(v4.w);

            return r;
        }
    }
}
