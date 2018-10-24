using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
    
namespace Iwsd
{

    // http://d.hatena.ne.jp/tekk/20100131/1264913887
    public static class DeepCopyHelper
    {
        public static T DeepCopy<T>(T target)
        {

            T result;
            var formatter = new BinaryFormatter();
            var mem = new MemoryStream();

            var selector  = new SurrogateSelector();
            var context   = new StreamingContext(StreamingContextStates.All);
            selector.AddSurrogate(typeof(UnityEngine.LayerMask), context, new LayerMaskSurrogate());
            selector.AddSurrogate(typeof(UnityEngine.GameObject), context, new GameObjectSurrogate());
            
            formatter.SurrogateSelector = selector;

            try
            {
                formatter.Serialize(mem, target);
                mem.Position = 0;
                result = (T)formatter.Deserialize(mem);
            }
            finally
            {
                mem.Close();
            }

            return result;
        }
    }

    // CHECK Is this surrogates working well really?

    // Ref.  https://devlights.hatenablog.com/entry/20120406/p2
    // UnityEngine.LayerMask
    class LayerMaskSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var targetObj = obj as UnityEngine.LayerMask?;
            info.AddValue("value",  targetObj.Value.value);
        }
        
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var targetObj = obj as UnityEngine.LayerMask?;
            targetObj = info.GetInt32("value");
            return targetObj;
        }
    }

    // UnityEngine.GameObject
    class GameObjectSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var targetObj = obj as UnityEngine.GameObject;
            info.AddValue("id",  targetObj.GetInstanceID());
        }
        
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
#if UNITY_EDITOR
            var targetObj = UnityEditor.EditorUtility.InstanceIDToObject(info.GetInt32("id"));
            return targetObj;
#else
            // REFINE support not in UNITY_EDITOR (?)
            return obj;
#endif
        }
    }

    class Vector4_SerializationSurrogate : ISerializationSurrogate {

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            UnityEngine.Vector4 v = (UnityEngine.Vector4) obj;
            info.AddValue("x", v.x);
            info.AddValue("y", v.y);
            info.AddValue("z", v.z);
            info.AddValue("w", v.w);
        }
 
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            UnityEngine.Vector4 v = (UnityEngine.Vector4) obj;
            v.x = info.GetSingle("x");
            v.y = info.GetSingle("y");
            v.z = info.GetSingle("z");
            v.w = info.GetSingle("w");
            return v;
        }
    }

    class Vector3_SerializationSurrogate : ISerializationSurrogate {

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            UnityEngine.Vector3 v = (UnityEngine.Vector3) obj;
            info.AddValue("x", v.x);
            info.AddValue("y", v.y);
            info.AddValue("z", v.z);
        }
 
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            UnityEngine.Vector3 v = (UnityEngine.Vector3) obj;
            v.x = info.GetSingle("x");
            v.y = info.GetSingle("y");
            v.z = info.GetSingle("z");
            return v;
        }
    }

    class ParameterBytesDeserializer {
        public static object Deserialize(byte[] bytes)
        {
            var formatter = new BinaryFormatter();
            
            var selector  = new SurrogateSelector();
            var context   = new StreamingContext(StreamingContextStates.All);
            selector.AddSurrogate(typeof(UnityEngine.Vector4), context, new Vector4_SerializationSurrogate());
            selector.AddSurrogate(typeof(UnityEngine.Vector3), context, new Vector3_SerializationSurrogate());
            formatter.SurrogateSelector = selector;

            var mem = new MemoryStream(bytes);

            object result;
            try
            {
                result = formatter.Deserialize(mem);
            }
            finally
            {
                mem.Close();
            }
            return result;
        }
    }

}
