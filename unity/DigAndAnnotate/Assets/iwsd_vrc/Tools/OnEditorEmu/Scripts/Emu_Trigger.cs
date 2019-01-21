using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Iwsd
{

    [Serializable]
    class Val_Trigger
    {
        // TODO trigger description viewer for play mode
        
        // string InteractText;
        [SerializeField]
        internal List<VRCSDK2.VRC_Trigger.TriggerEvent> Triggers;
        // Proximity;

        internal Val_Trigger(VRCSDK2.VRC_Trigger vrcTrigger)
        {
            this.Triggers = DeepCopyHelper.DeepCopy<List<VRCSDK2.VRC_Trigger.TriggerEvent>>(vrcTrigger.Triggers); 
        }
    }
    
    
    class Emu_Trigger : MonoBehaviour
    {
        // [SerializeField] // TODO Make trigger definition visible with Unity inspector to debug scene.
        Val_Trigger vrcTrigger;

        // VRCSDK2.VRC_Trigger
        private int pairedTriggerInstanceId;

        public string debugString;

        void Awake()
        {
            // Iwlog.Warn(gameObject, "Awake.");

            // search pairing VRC_Trigger by myself
            foreach(var orig in GetComponents<VRCSDK2.VRC_Trigger>())
            {
                var origid = orig.GetInstanceID();
                if (FindPairedBrother(origid) == null)
                {
                    SetupFrom(orig);
                    return;
                }
            }
            Iwlog.Error(gameObject, "Not found pairing VRC_Trigger");
        }

        private Emu_Trigger FindPairedBrother(int origid)
        {
            foreach(var bro in GetComponents<Emu_Trigger>())
            {
                if (bro.pairedTriggerInstanceId == origid)
                {
                    return bro;
                }
            }
            return null;
        }
            
        // It seems that VRCSDK2.VRC_Trigger destroy itself runtime on UnityEditor.
        // So copy its definition content to Val_Trigger.
        private void SetupFrom(VRCSDK2.VRC_Trigger from)
        {
            pairedTriggerInstanceId = from.GetInstanceID();

            vrcTrigger = new Val_Trigger(from);

            layerMask_OnEnterTrigger  = GetLayerMaskOf(VRCSDK2.VRC_Trigger.TriggerType.OnEnterTrigger);
            layerMask_OnExitTrigger   = GetLayerMaskOf(VRCSDK2.VRC_Trigger.TriggerType.OnExitTrigger);
            layerMask_OnEnterCollider = GetLayerMaskOf(VRCSDK2.VRC_Trigger.TriggerType.OnEnterCollider);
            layerMask_OnExitCollider  = GetLayerMaskOf(VRCSDK2.VRC_Trigger.TriggerType.OnExitCollider);


            var timers = SearchTrigger_NoArg(VRCSDK2.VRC_Trigger.TriggerType.OnTimer);
            // CHECK what happens if two or more OnTimer entry on original?
            if (timers.Any())
            {
                var trigger = timers.First();
                timerExecuter = new TimerExecuter(trigger, ExecuteTriggerActions);
                if (timers.Skip(1).Any())
                {
                    Iwlog.Warn(gameObject, "Only one OnTimer trigger supported.");
                }
            }
        }

        private TimerExecuter timerExecuter;

        ////////////////////////////////////////////////////////////
        // Public interface
        
        void OnEnable()
        {
            Iwlog.Trace(gameObject, "Emu_Trigger: OnEnable");
            ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnEnable);

            if (timerExecuter != null)
            {
                timerExecuter.OnEnable();
            }
        }

        void Start()
        {
            Iwlog.Trace(gameObject, "Emu_Trigger:Start");
        }


        void Update()
        {
            ExecuteTriggers_Key();
            
            if (timerExecuter != null)
            {
                timerExecuter.Update();
            }
        }

        void OnDisable()
        {
            Iwlog.Trace(gameObject, "Emu_Trigger: OnDisable");
            ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnDisable);
        }

        ////////////////////////////////////////
        // VRCSDK2.VRC_Trigger+TriggerType

        // Custom,  // from program (Animation, uGUI)
        void ExecuteCustomTrigger(string name)
        {
            Iwlog.Trace("Emu_Trigger:ExecuteCustomTrigger name='" + name + "'");
            ExecuteTriggers_Named(VRCSDK2.VRC_Trigger.TriggerType.Custom, name);
        }

        // Relay,
        // x OnEnable, // See above. timer reset
        // x OnDestroy, // DestroyObject action
        // x OnSpawn, // SpawnObject action
        // OnNetworkReady,
        // OnPlayerJoined,
        // OnPlayerLeft,
        // x OnPickupUseDown, // from Player
        // x OnPickupUseUp, // from Player
        // x OnTimer,

        // OnEnterTrigger, // Collider.OnTriggerEnter
        void OnTriggerEnter(Collider other)
        {
            Iwlog.Trace2(gameObject, "Emu_Trigger: OnTriggerEnter");

            int layer = other.gameObject.layer;
            LayerMask layerMask = 1 << layer;
            if ((layerMask_OnExitTrigger & layerMask) != 0)
            {
                countFor_OnExitTrigger++;
            }
            if ((layerMask_OnEnterTrigger & layerMask) != 0)
            {
                ExecuteTriggers_Collision(VRCSDK2.VRC_Trigger.TriggerType.OnEnterTrigger,
                                          layerMask,  countFor_OnEnterTrigger++);
            }
        }

        // OnExitTrigger, // Collider.OnTriggerExit
        void OnTriggerExit(Collider other)
        {
            Iwlog.Trace2(gameObject, "Emu_Trigger: OnTriggerExit");

            int layer = other.gameObject.layer;
            int layerMask = 1 << layer;
            if ((layerMask_OnEnterTrigger & layerMask) != 0)
            {
                --countFor_OnEnterTrigger;
            }
            if ((layerMask_OnExitTrigger & layerMask) != 0)
            {
                ExecuteTriggers_Collision(VRCSDK2.VRC_Trigger.TriggerType.OnExitTrigger,
                                          layerMask,  --countFor_OnExitTrigger);
            }
        }
        
        // x OnKeyDown, // see Update method
        // x OnKeyUp, // see Update method
        // x OnPickup, // from Player
        // x OnDrop, // from Player
        // x OnInteract, // from Player
        // OnEnterCollider, // Collider.OnCollisionEnter
        void OnCollisionEnter(Collision collision)
        {
            Iwlog.Trace2(gameObject, "Emu_Trigger: OnCollisionEnter");

            int layer = collision.collider.gameObject.layer;
            int layerMask = 1 << layer;
            if ((layerMask_OnExitCollider & layerMask) != 0)
            {
                countFor_OnExitCollider++;
            }
            if ((layerMask_OnEnterCollider & layerMask) != 0)
            {
                ExecuteTriggers_Collision(VRCSDK2.VRC_Trigger.TriggerType.OnEnterCollider,
                                          layerMask,  countFor_OnEnterCollider++);
            }
        }
        // OnExitCollider, // Collider.OnCollisionExit
        void OnCollisionExit(Collision collision)
        {
            Iwlog.Trace2(gameObject, "Emu_Trigger: OnCollisionExit");

            int layer = collision.collider.gameObject.layer;
            int layerMask = 1 << layer;
            if ((layerMask_OnEnterCollider & layerMask) != 0)
            {
                --countFor_OnEnterCollider;
            }
            if ((layerMask_OnExitCollider & layerMask) != 0)
            {
                ExecuteTriggers_Collision(VRCSDK2.VRC_Trigger.TriggerType.OnExitCollider,
                                          layerMask,  --countFor_OnExitCollider);
            }
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
        // x OnDisable, // see OnDisable method
        // OnOwnershipTransfer,
        // OnParticleCollision, // MonoBehaviour.OnParticleCollision
        void OnParticleCollision(GameObject other)
        {
            ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnParticleCollision);
        }

        ////////////////////////////////////////////////////////////
        // Internal interface

        internal bool HasTriggerOf(VRCSDK2.VRC_Trigger.TriggerType triggerType)
        {
            var triggers = SearchTrigger_NoArg(triggerType);
            return triggers.GetEnumerator().MoveNext();
        }

        // You have to choose appropriate method comparing with trigger type if you want to consider trigger conditions.
        // For example, if you call this method with OnEnterTrigger, it select trigger-action definition ignoring layers condition.
        internal void ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType triggerType)
        {
            ExecuteTriggers(SearchTrigger_NoArg(triggerType));
        }

        internal void ExecuteTriggers_Named(VRCSDK2.VRC_Trigger.TriggerType triggerType, string name)
        {
            ExecuteTriggers(SearchTrigger_Named(triggerType, name));
        }

        internal int ExecuteTriggers_Collision(VRCSDK2.VRC_Trigger.TriggerType triggerType, LayerMask layer, int count)
        {
            return ExecuteTriggers(SearchTrigger_Collision(triggerType, layer, count));
        }

        internal int ExecuteTriggers(IEnumerable<VRCSDK2.VRC_Trigger.TriggerEvent> triggers)
        {
            foreach (var triggerDef in triggers) {
                Iwlog.Trace(gameObject, "Emu_Trigger:ExecuteTriggers type=" + triggerDef.TriggerType);
                ExecuteTriggerActions(triggerDef);
            }
            return triggers.Count();
        }

        internal int ExecuteTriggers_Key()
        {
            // REFINE presearch related trigger definition. (Avoid searching for each frame)
            int count = 0;
            foreach (var triggerDef in SearchTrigger_NoArg(VRCSDK2.VRC_Trigger.TriggerType.OnKeyDown))
            {
                if (Input.GetKeyDown(triggerDef.Key))
                {
                    ExecuteTriggerActions(triggerDef);
                    count++;
                }
            }

            foreach (var triggerDef in SearchTrigger_NoArg(VRCSDK2.VRC_Trigger.TriggerType.OnKeyUp))
            {
                if (Input.GetKeyUp(triggerDef.Key))
                {
                    ExecuteTriggerActions(triggerDef);
                    count++;
                }
            }
            return count;
        }

        // private void EnsureHavingTrigger()
        // {
        //     if (this.vrcTrigger == null)
        //     {
        //         Iwlog.Error(this.gameObject, "this.vrcTrigger==null");
        //     }
        // }
    

        ////////////////////////////////////////////////////////////
        // Private implementation

        ////////////////////////////////////////
        // Collision layer

        // LIMITATION non-TriggerIndividuals behavior may odd if multiple triggers exists with same TriggerType.
        // Though multiple trigger is not allowed with editor currently, it is possible to have such case as component data.
        // This implementation holds only one counter for one TriggerType. 
        
        private LayerMask layerMask_OnEnterTrigger;
        private LayerMask layerMask_OnExitTrigger;
        private LayerMask layerMask_OnEnterCollider;
        private LayerMask layerMask_OnExitCollider;

        // count for non-TriggerIndividuals
        private int countFor_OnEnterTrigger;
        private int countFor_OnExitTrigger;
        private int countFor_OnEnterCollider;
        private int countFor_OnExitCollider;

        private LayerMask GetLayerMaskOf(VRCSDK2.VRC_Trigger.TriggerType triggerType)
        {
            int layerMask = 0;
            foreach (var trigger in SearchTrigger_NoArg(triggerType))
            {
                layerMask |= trigger.Layers.value;
            }
            return layerMask;
        }
        

        ////////////////////////////////////////
        // Accessing trigger-action definiton

        private IEnumerable<VRCSDK2.VRC_Trigger.TriggerEvent> SearchTrigger_NoArg(VRCSDK2.VRC_Trigger.TriggerType triggerType)
        {
            if (vrcTrigger == null)
            {
                Iwlog.Error(gameObject, "vrcTrigger == null");
                return Enumerable.Empty<VRCSDK2.VRC_Trigger.TriggerEvent>();
            }
            
            var query = vrcTrigger.Triggers
                .Where(x => (x.TriggerType == triggerType));
            return query;
        }
        
        // For Physics collision triggers
        // ( OnEnterTrigger, OnExitTrigger, OnEnterCollider, OnExitCollider )
        // Conditions
        // System.Boolean TriggerIndividuals;
        // UnityEngine.LayerMask Layers;

        private IEnumerable<VRCSDK2.VRC_Trigger.TriggerEvent> SearchTrigger_Collision(VRCSDK2.VRC_Trigger.TriggerType triggerType, LayerMask layerMask, int count)
        {
            // It's possible to optimize by using pre-limited list (previously searched triggers by TriggerType) than vrcTrigger.Triggers.
            // (That search is actually done on initialization to get unified layer mask)
            // But for simplicity, I use vrcTrigger.Triggers for now.
            
            var query = vrcTrigger.Triggers
                .Where(x => (x.TriggerType == triggerType)
                       && ((x.Layers.value & layerMask.value) != 0)
                       && ((x.TriggerIndividuals)? true: (count == 0)));
            
            Iwlog.Debug("Collision query.Count=" + query.Count());
            return query;
        }

        private IEnumerable<VRCSDK2.VRC_Trigger.TriggerEvent> SearchTrigger_Named(VRCSDK2.VRC_Trigger.TriggerType triggerType, string name)
        {
            var query = vrcTrigger.Triggers
                .Where(x => (x.TriggerType == triggerType) && (x.Name == name));

            if (EmulatorSettings.ReportTriggersNotOneNamedMatch)
            {
                switch (query.Count())
                {
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
            }
            return query;
        }

        
        ////////////////////////////////////////
        // Execute TriggerEvent (i.e. a trigger definition)

        static System.Random random = new System.Random(0);

        private void ExecuteTriggerActions(VRCSDK2.VRC_Trigger.TriggerEvent triggerEvent)
        {
            if (triggerEvent.Probabilities.Length == 0)
            {
                foreach (var vrcEvent in triggerEvent.Events)
                {
                    ExecuteTriggerAction(vrcEvent);
                }
            }
            else
            {
                // assumes probabilities is nomarilized
                float psum = triggerEvent.Probabilities.Sum();
                double rand = random.NextDouble();
                Iwlog.Trace2("Do Rundomized trigger: probabilities psum=" + psum + ", rand=" + rand);
                for (int i = 0; i < triggerEvent.Probabilities.Length; i++)
                {
                    // CHECK include equal case?
                    rand -= triggerEvent.Probabilities[i];
                    if (rand <= 0)
                    {
                        Iwlog.Trace2("Do Rundomized trigger: idx=" + i);
                        ExecuteTriggerAction(triggerEvent.Events[i]);
                        break;
                    }
                }
            }
        }

        //////////////////////////////////////////////////
        // Execution of VrcEvent (i.e. action)
        
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
                //  RunConsoleCommand, NOTE Incomplete implementation for debug // hidden
                case VRCSDK2.VRC_EventHandler.VrcEventType.RunConsoleCommand:
                    return receivers.Select(r => Execute_RunConsoleCommand(r, vrcEvent));
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
            GameObject[] receivers = vrcEvent.ParameterObjects;

            // For backward compatibility. "ParameterObject" is maybe deprecated old style
            if ((vrcEvent.ParameterObject != null) && (receivers.Length == 0))
            {
                receivers = new [] {vrcEvent.ParameterObject};
            }
            
            if (receivers.Length == 0)
            {
                receivers = new [] {this.gameObject};
            }

            return receivers.Where(x => x != null);
        }


        //////////////////////////////////////////////////////////////////////
        // Each action

        // NOTE This is not equivalent with original. This is implemented for debug and test emulator itself.
        private ActionResult Execute_RunConsoleCommand(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            if (!EmulatorSettings.EnableIncompatibleRunConsoleCommand)
            {
                return ActionResult.Success;
            }

            Iwlog.Debug(gameObject, "RCC:" + vrcEvent.ParameterString);
            return ActionResult.Success;
        }

        
        ////////////////////////////////////////
        // Unity basics

        // ParameterBoolOp : assign value
        private ActionResult Execute_SetGameObjectActive(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            Iwlog.Debug(gameObject, "Execute_SetGameObjectActive");

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
            // true: includeInactive
            foreach (var comp in newOne.GetComponentsInChildren<VRCSDK2.VRC_Trigger>(true))
            {
                comp.gameObject.AddComponent<Emu_Trigger>();
            }

            foreach (var comp in newOne.GetComponentsInChildren<Emu_Trigger>(false))
            {
                comp.ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnSpawn);
            }
            
            return ActionResult.Success;
        }


        private ActionResult Execute_DestroyObject(GameObject receiver, VRCSDK2.VRC_EventHandler.VrcEvent vrcEvent)
        {
            // CHECK What happens if receiver is inactive?

            // I think MonoBehaviour.OnDestroy() is not good place to implement OnDestroy. So I placed here. 
            // (When OnDestroy() called, it may possibly almost be unable to do any thing as trigger-action system)
            foreach (var comp in receiver.GetComponentsInChildren<Emu_Trigger>())
            {
                if (comp.HasTriggerOf(VRCSDK2.VRC_Trigger.TriggerType.OnDestroy))
                {
                    comp.ExecuteTriggers(VRCSDK2.VRC_Trigger.TriggerType.OnDestroy);
                }
            }
            
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
            receiverComp.ExecuteCustomTrigger(name);

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

            var destination = receiver.transform;

            switch (vrcEvent.ParameterBoolOp) {
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.False:
                    LocalPlayerContext.TeleportPlayer(destination, false);
                    break;
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.True:
                    LocalPlayerContext.TeleportPlayer(destination, true);
                    break;
                case VRCSDK2.VRC_EventHandler.VrcBooleanOp.Toggle:
                    Iwlog.Warn(gameObject, "Invalid ParameterBoolOp. value='" + vrcEvent.ParameterBoolOp + "' for TeleportPlayer action");
                    LocalPlayerContext.TeleportPlayer(destination, false); // exec anyway
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

                // Iwlog.Debug(gameObject, " paramTypes[" + i + "]=" + paramTypes[i].FullName);
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

    

    //////////////////////////////////////////////////////////////////////
    // Misc impl

    ////////////////////////////////////////
    // Timer

    class TimerExecuter
    {
        // If negative timer is stoped
        private float targetTime;
        private float timeElapsed;
        
        private VRCSDK2.VRC_Trigger.TriggerEvent triggerEvent;
        // System.Boolean Repeat;
        // System.Single LowPeriodTime;
        // System.Single HighPeriodTime;
        // System.Boolean ResetOnEnable;

        private Action<VRCSDK2.VRC_Trigger.TriggerEvent> fireTriggerAction;

        public TimerExecuter(VRCSDK2.VRC_Trigger.TriggerEvent triggerEvent,
                             Action<VRCSDK2.VRC_Trigger.TriggerEvent> fireTriggerAction)
        {
            // REFINE Should I check argument range?
            this.triggerEvent = triggerEvent;
            this.fireTriggerAction = fireTriggerAction;
        }
            
        static System.Random random = new System.Random(0);

        private void Setup()
        {
            targetTime = triggerEvent.LowPeriodTime
                + (triggerEvent.HighPeriodTime - triggerEvent.LowPeriodTime) * (float)random.NextDouble();
            timeElapsed = 0.0f;
        }

        public void OnEnable()
        {
            if (triggerEvent.ResetOnEnable)
            {
                Setup();
            }
        }
        
        public void Update()
        {
            if (targetTime < 0)
            {
                return;
            }
            
            timeElapsed += Time.deltaTime;
            
            if (targetTime <= timeElapsed)
            {
                fireTriggerAction(triggerEvent);
                    
                if (triggerEvent.Repeat)
                {
                    Setup();
                }
                else
                {
                    targetTime = -1.0f;
                }
            }
            
        }
    }

}
