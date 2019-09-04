using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using Iwsd.UnityYamlObjects;
using UnityEditor.IMGUI.Controls;

/*

VRC_Iwsd / Unity YAML Asset Browser

by naqtn


### TODO

- better tree parent-child construction
    - animation controller (.controller)
        - link parent-child relation
        - AnimatorController - AnimatorStateMachine - AnimatorState - AnimatorStateTransition

- check wheather binary format (when reading file? project setting?)
    - asset serialization (Edit > Project Settings > Editor Settings)

- improve navigation (search and select) feature
    - explicit "fileID:", "guid:" prefix format
    - search context menu (inside YAML text area)
        - pointing LIIF
        - pointing fileID and guid
    - select result resource (specified by guid in YAML) in UnityEditor (ping)
    - line jump (like "line:184")
    - YAML document block up/down button, (at the same time, open and select item in tree. if select option is on)
    - enter liif, then show appearance points list
    - enter path as text, then expand tree item
    - Show current selected object path
- select tree item from Hierarchy view (while opening same scene file)
    - manual select button
    - auto follow toggle
- navigation history
    - save visited tree node path
    - add back and forward button
    - show history popup, select to jump

- Test and check "stripped" case handling (delete some child object in Prefab ussage)
    - https://unity3d.com/unity/qa/patch-releases/5.0.0p2
        - > (663873), (672462) - Editor: Stripped prefab instance objects are now marked as stripped in the scene file, so the loading code does not try to upgrade objects which would issue wrong warnings and in worst case crashes.
    - https://forum.unity.com/threads/scene-files-invalid-yaml.355653/
    - https://forum.unity.com/threads/smart-merge-not-working.315903/

- support for file merging, resolving conflicts (two file view)
    - show(browse) diff files (LIIF aware)
- Help button contents

- Better Prefab object support in scene file
    - derefer name from m_ParentPrefab
    - prefab instance to GameObject tree
    - m_Modifications
        - propertyPath: m_RootOrder

- multi column tree
    - (For what feature? necessary?)



### Implemented Features

- Open window from unity menu
    - Supports multiple window. Each can load different asset.

- Supported file type and extensions:
    - prefab (.prefab)
    - scene (.unity)
        - support for non component object
            - (This is implicit in Hierarchy and Project view)
    - animation (.anim)
    - animation controller (.controller)
    - material (.mat)
    - render texture (.renderTexture)
    - scriptable object (.asset)

- load YAML from selected object in Unity Editor
    - select an object in Hierarchy view or Project view as usual
    - open prefab if selected object uses prefab

- asset path field
    - a text field that shows what resource is currently loaded
    - text is copyable
    - accept drop and load if it can
        - This is only way to load for some file type (.asset)
        - It's also able to accept file from outside of Unity Editor (Explorer on Windows)
        - Hint: scene icon in Hierarchy view to load scene file

- Show YAML objects (documents) in tree view
    - show icon representing object type
    - show object's name (if it has) or type name
    - show MonoBehaviour's script name
    - show line number where the object comes from
    - tree structure
        - show GameObject tree
            - (internally tree is made by transform)
        - also show component as a tree node (different style than hierarchy view)
        - components first, GameObjects next
        - components order (GameObjects component list)
        - GameObjects order (m_RootOrder of transform)


- show YAML text of selected item in tree view
    - text are scrollable over whole file

- Filter tree item by text matching
    - tree itams are flatten when filtering 

- search by fileID (Local Identifier in file)
    - show path
        - path notation is like "/GameObj1/GameObj2<ComponentName>"
    - select found item in tree (and YAML text) if select option is on
- search by guid
    - can find system asset
    - can find project local asset (.cs)

- a button for re-select (popup) currently loaded object in editor
- Help button
- dragable two pane (upper for tree, lower for detail)


#### internal features

- Load YAML file and holds each yaml documents as objects
    - deserialize to an instance. that class is originally implemented
    - holds line number where the object come from in YAML file
    - holds YAML file content as text
- load asset info file
    - dereference guid-fildID to name string

### Known issue

- Prefab instances (in scene file) are not appears in GameObject hierarchy tree.
- Stripped prefab in scene file is not handled well
    - (Stripped prefab seems relate to delete something from prefab)



### MEMO

- Icon,  GUIContent
    - EditorGUIUtility.IconContent
    - EditorGUIUtility.ObjectContent
    - [Unity Editor Built-in Icons](https://github.com/halak/unity-editor-icons)
    - [Show Built In Resources](http://wiki.unity3d.com/index.php/Show_Built_In_Resources)
- Menu
    - IHasCustomMenu AddItemsToMenu
- IMGUI
    - [IMGUI crash course](https://github.com/Bunny83/Unity-Articles/blob/master/IMGUI%20crash%20course.md)
- FileID
    - first four bytes of MD4 digest of {115 :32bit int class ID for MonoScript} + Namespace + Name
        - https://forum.unity.com/threads/yaml-fileid-hash-function-for-dll-scripts.252075/#post-3779584
        - https://www.robinryf.com/blog/2017/10/30/unity-behaviour-in-dlls.html
    - UnityScript to C#
        - https://forum.unity.com/threads/problems-compiling-dlls-from-monodevelop.148617/#post-1024523
        - https://forum.unity.com/threads/reducing-script-compile-time-or-a-better-workflow-to-reduce-excessive-recompiling.148078/#post-1026639
    - DLLSwitcher
        - https://assetstore.unity.com/packages/tools/utilities/dllswitcher-40370
    - Asset internal info
        - https://forum.unity.com/threads/yaml-fileid-hash-function-for-dll-scripts.252075/#post-3779584
        > there is now a ".info" file created in the Library\metadata folder for each external assembly
    - "Resources/unity_builtin_extra", "Library/unity default resources"
        - https://answers.unity.com/questions/1377941/getassetpath-returning-incomplete-path-for-default.html
        - https://stackoverflow.com/questions/56159442/how-to-get-the-path-and-file-size-of-unity-built-in-assets
- Tools
    - [ReferenceViewer](https://github.com/anchan828/ReferenceViewer)
        - https://qiita.com/akihiro_0228/items/4dc0d12b90629a5fdcac
- TreeView multi column
    - https://github.com/anchan828/texture-tree-view-sample
    - http://light11.hatenadiary.com/entry/2019/02/07/010146
- "String too long for TextMeshGenerator. Cutting off characters." on TextArea
    - https://forum.unity.com/threads/string-too-long-for-textmeshgenerator-cutting-off-characters.351852/
    - 65535/4 = 16383.75 is limit

 */


namespace Iwsd.UnityYamlObjects {

    // Local Identifier In File
    public class YamlLocalId : IEquatable<YamlLocalId>
    {
        private long val;
        public YamlLocalId(long i)
        {
            this.val = i;
        }

        internal YamlLocalId(string s)
            : this(long.Parse(s))
        {
        }

        // IEquatable
        public bool Equals(YamlLocalId other)
        {
            if (other == null)
            {
                return false;
            }
            return this.val == other.val;
        }

        // Not only implementing IEquatable,
        // Overriding GetHashCode is also needed for begin used as Dictionary key.
        // https://fyts.hatenadiary.org/entry/20071026/asp
        public override int GetHashCode()
        {
            return this.val.GetHashCode();
        }

        public bool IsNull()
        {
            return val == 0;
        }

        public static explicit operator YamlLocalId (long i) {return new YamlLocalId(i);}
        public static string operator+(string l, YamlLocalId r) {return l + r.ToString();}
        public static string operator+(YamlLocalId l, string r) {return l.ToString() + r;}

        public override string ToString()
        {
            return GetType() + " <" + val.ToString() + ">";
        }
    }

    // Working context for YamlObject
    internal interface WorkingContext
    {
        // subset of System.Collections.Generic.IReadOnlyDictionary<YamlLocalId, YamlObject>
        // (IReadOnlyDictionary doesn't exist in Unity 2017.4.28f enviroment)
        YamlObject this[YamlLocalId key] { get; }
        bool ContainsKey(YamlLocalId key);
        bool TryGetValue(YamlLocalId key, out YamlObject value);

        string GetYamlLine(int lineNo);

        void DebugWarn(string message);
    }

    internal class YamlLines
    {
        static internal readonly Regex c_directives_end_unity_re = new Regex(@"^--- !u!(\d+) &(\d+)");
        static internal readonly Regex object_name_re = new Regex(@"^  m_Name: (.+)$");  // GameObject
        static internal readonly Regex gameobject_re = new Regex(@"^  m_GameObject: {fileID: (\d+)}"); // Component
        static internal readonly Regex father_re = new Regex(@"^  m_Father: {fileID: (\d+)}"); // Transform, RectTransform
        static internal readonly Regex root_order_re = new Regex(@"^  m_RootOrder: ([0-9]+)"); // Transform
        static internal readonly Regex component_re = new Regex(@"^  m_Component:$");
        static internal readonly Regex child_component_re = new Regex(@"^  - component: {fileID: (\d+)}");
        static internal readonly Regex child_component_old_re = new Regex(@"^  - \d+: {fileID: (\d+)}");
        static internal readonly Regex root_gameobject_re = new Regex(@"^  m_RootGameObject: {fileID: (\d+)}");
        static internal readonly Regex script_re = new Regex(@"  m_Script: {fileID: (-?[0-9]+), guid: ([0-9a-f]{32}), type: \d}");
    }

    //////////////////////////////
    // Unity Object system relative classes

    public class YamlObject
    {
        // "Local Identifier In File"
        public YamlLocalId Liif { get; }

        public int LineNo { get; }

        internal UnityTypeInfo _UnityTypeInfo { get; }
        public string UnityTypeName { get {return _UnityTypeInfo.Name;} }
        public Type UnityType { get {return _UnityTypeInfo.CsType;} }

        // like Object.GetInstanceID
        public int GetInstanceID()
        {
            return LineNo;  // reuse LineNo as quick hack for now.
        }

        internal WorkingContext Context { get; }

        internal YamlObject(WorkingContext context, YamlLocalId liif, int lineNo, UnityTypeInfo unityTypeInfo)
        {
            Context = context;
            Liif = liif;
            LineNo = lineNo;
            _UnityTypeInfo = unityTypeInfo;
        }

        string _name;
        // like  UnityEngine.Object.name
        public string name { get {return _name;}}

        internal virtual void ParseLine(string line)
        {
            Match match;
            if ((match = YamlLines.object_name_re.Match(line)).Success)
            {
                _name = match.Groups[1].Value;
            }
        }

        // like  UnityEngine.Object.bool
        public static bool operator true(YamlObject o){return o != null;}
        public static bool operator false(YamlObject o){return o == null;}


        private string formatLine(int fi, string fl)
        {
            return fi.ToString().PadLeft(5, ' ') + " " + fl + "\n";
        }

        public string GetYamlText()
        {
            int i = LineNo;
            var l = Context.GetYamlLine(i);
            var s = formatLine(i++, l);
            while (((l = Context.GetYamlLine(i)) != null) && !l.StartsWith("---"))
            {
                s += formatLine(i++, l);
            }
            return s;
        }


        public override string ToString()
        {
            return GetType() + "{UnityTypeName'" + UnityTypeName + ", LineNo=" + LineNo + ", Liif=" + Liif + "}";
        }

    }


    public class YamlComponent : YamlObject
    {
        internal YamlComponent(WorkingContext context, YamlLocalId liif, int lineNo, UnityTypeInfo unityTypeInfo)
            : base(context, liif, lineNo, unityTypeInfo) { }

        YamlLocalId GameObjectId;
        internal override void ParseLine(string line)
        {
            base.ParseLine(line);
            Match match;
            if ((match = YamlLines.gameobject_re.Match(line)).Success)
            {
                GameObjectId = new YamlLocalId(match.Groups[1].Value);  // m_GameObject: {} => GameObjectId
            }

        }

        // Component.transform
        public virtual YamlTransform transform {
            get { return ((YamlGameObject)Context[GameObjectId]).transform; }
        }

        // Component.gameObject
        public YamlGameObject gameObject {
            get {
                // FIXME GameObject could be absent for "stripped" object?
                // http://mokuapps.com/develop/?p=668
                if (GameObjectId == null)
                {
                    Context.DebugWarn("GameObjectId == null. type=" + this.GetType() + ", LineNo=" + LineNo);
                    return null; // FIXME or NullGameObject?
                }
                else if (!Context.ContainsKey(GameObjectId))
                {
                    // CHECK occurs normally for .asset case? (FileID = 0)
                    Context.DebugWarn("unknown GameObjectId=" + GameObjectId + ", type=" + this.GetType() + ", LineNo=" + LineNo);
                    return null; // FIXME or NullGameObject?
                }
                return (YamlGameObject)Context[GameObjectId];
            }
        }
    }


    public class YamlTransform : YamlComponent
    {
        internal YamlTransform(WorkingContext context, YamlLocalId liif, int lineNo, UnityTypeInfo unityTypeInfo)
            : base(context, liif, lineNo, unityTypeInfo) { }

        YamlLocalId FatherId;

        // Transform YAML doc has children transforms array, though this impl doen't parse it currently.
        // List<YamlLocalId> ChildrenId;

        int _RootOrder;
        public int RootOrder {get {return _RootOrder;}}

        internal override void ParseLine(string line)
        {
            base.ParseLine(line);
            Match match;
            if ((match = YamlLines.father_re.Match(line)).Success)
            {
                FatherId = new YamlLocalId(match.Groups[1].Value);  // m_Father: {} => FatherId
            }
            else if ((match = YamlLines.root_order_re.Match(line)).Success)
            {
                _RootOrder = int.Parse(match.Groups[1].Value);
            }
        }

        // Component.transform
        public override YamlTransform transform { get {return this;} }
        // Transform.parent
        public YamlTransform parent { get { return FatherId.IsNull()? null: (YamlTransform)Context[FatherId];} }
    }


    public class YamlRectTransform : YamlTransform
    {
        internal YamlRectTransform(WorkingContext context, YamlLocalId liif, int lineNo, UnityTypeInfo unityTypeInfo)
            : base(context, liif, lineNo, unityTypeInfo) { }
    }


    public class YamlGameObject : YamlObject
    {
        internal YamlGameObject(WorkingContext context, YamlLocalId liif, int lineNo, UnityTypeInfo unityTypeInfo)
            : base(context, liif, lineNo, unityTypeInfo) { }

        public List<YamlLocalId> ComponentIds = new List<YamlLocalId>();

        bool parsingComponents = false;
        internal override void ParseLine(string line)
        {
            bool consumed = false;
            Match match;
            if (parsingComponents)
            {
                if ((match = YamlLines.child_component_re.Match(line)).Success)
                {
                    ComponentIds.Add(new YamlLocalId(match.Groups[1].Value));
                    consumed = true;
                }
                else if ((match = YamlLines.child_component_old_re.Match(line)).Success)
                {
                    ComponentIds.Add(new YamlLocalId(match.Groups[1].Value));
                    consumed = true;
                }
                else
                {
                    parsingComponents = false;
                }
            }
            else
            {
                if ((match = YamlLines.component_re.Match(line)).Success)
                {
                    parsingComponents = true;
                    consumed = true;
                }
            }
            if (!consumed)
            {
                base.ParseLine(line);
            }
        }

        public YamlTransform transform { get {
                if (ComponentIds.Count < 1) { // could be happen for "stripped" case // CHECK
                    Context.DebugWarn("GameObject ComponentIds.Count < 1 : lineNo=" + LineNo + ", type=" + this.GetType());
                    return null;
                }
                return (YamlTransform) Context[ComponentIds[0]];
            }}
    }

    // Unity doesn't have public Prefab object
    public class YamlPrefab : YamlObject
    {
        internal YamlPrefab(WorkingContext context, YamlLocalId liif, int lineNo, UnityTypeInfo unityTypeInfo)
            : base(context, liif, lineNo, unityTypeInfo) { }

        YamlLocalId RootGameObjectId;

        internal override void ParseLine(string line)
        {
            base.ParseLine(line);
            Match match;
            if ((match = YamlLines.root_gameobject_re.Match(line)).Success)
            {
                RootGameObjectId = new YamlLocalId(match.Groups[1].Value);  // m_RootGameObject: {} => RootGameObjectId
            }
        }

        // like Component.gameObject
        public YamlGameObject gameObject { get { return (YamlGameObject) Context[RootGameObjectId]; }}
    }


    public class YamlBehaviour : YamlComponent
    {
        internal YamlBehaviour(WorkingContext context, YamlLocalId liif, int lineNo, UnityTypeInfo unityTypeInfo)
            : base(context, liif, lineNo, unityTypeInfo) { }
        //  m_Enabled:
    }


    public class YamlMonoBehaviour : YamlBehaviour
    {
        internal YamlMonoBehaviour(WorkingContext context, YamlLocalId liif, int lineNo, UnityTypeInfo unityTypeInfo)
            : base(context, liif, lineNo, unityTypeInfo) { }

        // m_Script: {fileID, guid}
        // fileID is fixed value 11500000 for .cs file in the project

        long _fileID;
        public long Script_fileID {get {return _fileID;}}
        string _guid;
        public string Script_guid {get {return _guid;}}
        
        internal override void ParseLine(string line)
        {
            base.ParseLine(line);
            Match match;
            if ((match = YamlLines.script_re.Match(line)).Success)
            {
                _fileID = long.Parse(match.Groups[1].Value);
                _guid = match.Groups[2].Value;
            }
        }

    }


    public class YamlSomeComponent : YamlComponent
    {
        internal YamlSomeComponent(WorkingContext context, YamlLocalId liif, int lineNo, UnityTypeInfo unityTypeInfo)
            : base(context, liif, lineNo, unityTypeInfo) { }
    }


    public class YamlOtherObject : YamlObject
    {
        internal YamlOtherObject(WorkingContext context, YamlLocalId liif, int lineNo, UnityTypeInfo unityTypeInfo)
            : base(context, liif, lineNo, unityTypeInfo) { }
    }



    class WorkingContextImpl : WorkingContext
    {
        public void DebugWarn(string message)
        {
            // Debug.LogWarning(message);
        }

        //////////////////////////////
        // Object set
        private Dictionary<YamlLocalId, YamlObject> Objs = new Dictionary<YamlLocalId, YamlObject>();

        public YamlObject this[YamlLocalId key] { get { return Objs[key]; } }
        public bool ContainsKey(YamlLocalId key)
        {
            return Objs.ContainsKey(key);
        }
        public bool TryGetValue(YamlLocalId key, out YamlObject value)
        {
            return Objs.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<YamlLocalId, YamlObject>> GetEnumerator()
        {
            return Objs.GetEnumerator();
        }

        internal int ObjectsCount { get {return Objs.Count;} }
        internal void Add(YamlLocalId id , YamlObject obj)
        {
            Objs.Add(id, obj);
        }

        //////////////////////////////
        // Source YAML
        List<string> SourceText = new List<string>();

        // 1 for first line (not zero).
        public string GetYamlLine(int lineNo)
        {
            lineNo -= 1;
            return (SourceText.Count <= lineNo)? null: SourceText[lineNo];
        }

        internal void AddYamlLine(string line)
        {
            SourceText.Add(line);
        }

        internal List<string> GetAllYamlLines()
        {
            // TODO return copy?
            // *IReadOnlyList<string> is not available. new ReadOnlyCollection<string>(SourceText);
            return SourceText;
        }

        //////////////////////////////
        internal void Clear()
        {
            Objs.Clear();
            SourceText.Clear();
        }
    }

    public class YamlAssetParser // : IEnumerable<KeyValuePair<YamlLocalId, YamlObject>>
    {
        static YamlAssetParser()
        {
            InitUnityTypeInfo();
        }

        WorkingContextImpl context = new WorkingContextImpl();

        public YamlObject this[YamlLocalId liif] { get { return context[liif]; } }
        public IEnumerator<KeyValuePair<YamlLocalId, YamlObject>> GetEnumerator() { return context.GetEnumerator(); }
        public int Count { get {return context.ObjectsCount;} }


        private YamlObject _RootObject;
        public YamlObject RootObject {get {return _RootObject;}}

        public void Clear()
        {
            context.Clear();
            _RootObject = null;
        }

        public List<string> GetAllYamlLines()
        {
            return context.GetAllYamlLines();
        }

        private YamlObject CreateYamlObject(long liif, int yamlClassId, int lineNo)
        {
            if (liif == 0)
            {
                throw new ArgumentException("Not allowed. liif=" + liif);
            }

            var id = new YamlLocalId(liif);

            YamlObject obj;

            UnityTypeInfo tyinfo = ClassIdToTypeInfo[yamlClassId];
            Type type = tyinfo.YamlImplType;
            if (type != null)
            {
                ConstructorInfo ctor
                    = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                          null, new[] {typeof(WorkingContext), typeof(YamlLocalId), typeof(int), typeof(UnityTypeInfo)}, null);
                obj = (YamlObject)ctor.Invoke(new object[]{context, id, lineNo, tyinfo});
            }
            else
            {
                if (tyinfo.IsComponent)
                {
                    obj = new YamlSomeComponent(context, id, lineNo, tyinfo);
                }
                else
                {
                    obj = new YamlOtherObject(context, id, lineNo, tyinfo);
                }
            }

            context.Add(id, obj);

            // Set first object to the root;
            if (_RootObject == null)
            {
                _RootObject = obj;
            }

            return obj;
        }


        // parse Unity serialization YAML
        // This is not a general YAML parser.
        public void ParseFile(string filePath)
        {
            using(var reader = new System.IO.StreamReader(filePath))
            {
                Clear();

                YamlObject curDoc = null;
                var lno = 0;
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    ++lno;
                    context.AddYamlLine(line);
                    Match match;
                    if ((match = YamlLines.c_directives_end_unity_re.Match(line)).Success)
                    {
                        var yamlClassId = int.Parse(match.Groups[1].Value);
                        var liif = long.Parse(match.Groups[2].Value);
                        curDoc = CreateYamlObject(liif, yamlClassId, lno);
                    }
                    else if (curDoc != null)
                    {
                        curDoc.ParseLine(line);
                    }
                }
            }
        }

        ////////////////////////////////////////
        // Utility

        // throws FormatException
        public YamlObject LiifToYamlObjectOrNull(long liif)
        {
            var liifObj = new YamlLocalId(liif);
            if (!context.ContainsKey(liifObj)) {
                return null;
            }
            return context[liifObj];
        }


        public string AbsolutePathOf(YamlObject obj)
        {
            var pathArray = BuildAbsolutePathOf(obj);
            return PathArrayToPathString(pathArray);
        }

        string PathArrayToPathString(List<YamlObject> arr)
        {
            string r = "";
            for (var i = 0; i < arr.Count; i++)
            {
                var o = arr[i];
                if ((o is YamlTransform) && (i != arr.Count-1)) // notation of last YamlTransform is its type
                {
                    var go = ((YamlTransform)o).gameObject;
                    r += "/" + go.name;
                }
                else
                {
                    r += "/<" + o.UnityTypeName + ">";
                }
            }
            return r;
        }

        const int DEFAULT_MAX_OBJECT_PATH_DEPTH = 184;

        List<YamlObject> BuildAbsolutePathOf(YamlObject target, int maxDepth = DEFAULT_MAX_OBJECT_PATH_DEPTH)
        {
            var r = new List<YamlObject>();

            if (maxDepth <= 0)
            {
                return r;
            }
            r.Add(target);
            if (--maxDepth <= 0)
            {
                return r;
            }

            YamlTransform t;
            if (target is YamlGameObject)
            {
                t = ((YamlGameObject)target).transform;
            }
            else if (target is YamlComponent)
            {
                t = ((YamlComponent)target).transform;
            }
            else
            {
                return r;
            }

            // actually, maxDepth is a guard against infinite loop
            while ((0 < maxDepth--) && (t != null))
            {
                r.Add(t);
                t = t.parent;
            }

            r.Reverse();
            return r;
        }


        // https://docs.unity3d.com/Manual/ClassIDReference.html
        // https://forum.unity.com/threads/yaml-class-id-reference.501959/
        // http://hecres.hatenablog.com/entry/2018/03/17/152620
        // > UnityEditor.UnityType
        private static readonly Dictionary<int, UnityTypeInfo> ClassIdToTypeInfo = new Dictionary<int, UnityTypeInfo>();
        private static void InitUnityTypeInfo()
        {
            foreach(var ti in UnityTypeInfoArray)
            {
                ClassIdToTypeInfo.Add(ti.ID, ti);
            }
        }

#pragma warning disable 618         // (CS0618 A class member was marked with the Obsolete attribute)
        private static readonly UnityTypeInfo[] UnityTypeInfoArray = new UnityTypeInfo[] {
            new UnityTypeInfo(0, false, "Object", typeof(UnityEngine.Object), typeof(YamlObject)),
            new UnityTypeInfo(1, false, "GameObject", typeof(UnityEngine.GameObject), typeof(YamlGameObject)),
            new UnityTypeInfo(2, true, "Component", typeof(UnityEngine.Component), typeof(YamlComponent)),
            new UnityTypeInfo(3, false, "LevelGameManager", null),
            new UnityTypeInfo(4, true, "Transform", typeof(UnityEngine.Transform), typeof(YamlTransform)),
            new UnityTypeInfo(5, false, "TimeManager", null), // hidden typeof(UnityEditor.TimeManager)
            new UnityTypeInfo(6, false, "GlobalGameManager", null),
            new UnityTypeInfo(8, true, "Behaviour", typeof(UnityEngine.Behaviour), typeof(YamlBehaviour)),
            new UnityTypeInfo(9, false, "GameManager", null),
            new UnityTypeInfo(11, false, "AudioManager", null), // hidden typeof(UnityEditor.AudioManager)
            new UnityTypeInfo(12, true, "ParticleAnimator", typeof(UnityEngine.ParticleAnimator)),
            new UnityTypeInfo(13, false, "InputManager", null), // hidden typeof(UnityEditor.InputManager)
            new UnityTypeInfo(15, true, "EllipsoidParticleEmitter", typeof(UnityEngine.EllipsoidParticleEmitter)),
            new UnityTypeInfo(18, false, "EditorExtension", null),
            new UnityTypeInfo(19, false, "Physics2DSettings", null), // hidden typeof(UnityEditor.Physics2DSettings)
            new UnityTypeInfo(20, true, "Camera", typeof(UnityEngine.Camera)),
            new UnityTypeInfo(21, false, "Material", typeof(UnityEngine.Material)),
            new UnityTypeInfo(23, true, "MeshRenderer", typeof(UnityEngine.MeshRenderer)),
            new UnityTypeInfo(25, true, "Renderer", typeof(UnityEngine.Renderer)),
            new UnityTypeInfo(26, true, "ParticleRenderer", typeof(UnityEngine.ParticleRenderer)),
            new UnityTypeInfo(27, false, "Texture", typeof(UnityEngine.Texture)),
            new UnityTypeInfo(28, false, "Texture2D", typeof(UnityEngine.Texture2D)),
            new UnityTypeInfo(29, false, "OcclusionCullingSettings", null),
            new UnityTypeInfo(30, false, "GraphicsSettings", typeof(UnityEngine.Rendering.GraphicsSettings)),
            new UnityTypeInfo(33, true, "MeshFilter", typeof(UnityEngine.MeshFilter)),
            new UnityTypeInfo(41, true, "OcclusionPortal", typeof(UnityEngine.OcclusionPortal)),
            new UnityTypeInfo(43, false, "Mesh", typeof(UnityEngine.Mesh)),
            new UnityTypeInfo(45, true, "Skybox", typeof(UnityEngine.Skybox)),
            new UnityTypeInfo(47, false, "QualitySettings", typeof(UnityEngine.QualitySettings)),
            new UnityTypeInfo(48, false, "Shader", typeof(UnityEngine.Shader)),
            new UnityTypeInfo(49, false, "TextAsset", typeof(UnityEngine.TextAsset)),
            new UnityTypeInfo(50, true, "Rigidbody2D", typeof(UnityEngine.Rigidbody2D)),
            new UnityTypeInfo(53, true, "Collider2D", typeof(UnityEngine.Collider2D)),
            new UnityTypeInfo(54, true, "Rigidbody", typeof(UnityEngine.Rigidbody)),
            new UnityTypeInfo(55, false, "PhysicsManager", null), // hidden typeof(UnityEditor.PhysicsManager)
            new UnityTypeInfo(56, true, "Collider", typeof(UnityEngine.Collider)),
            new UnityTypeInfo(57, true, "Joint", typeof(UnityEngine.Joint)),
            new UnityTypeInfo(58, true, "CircleCollider2D", typeof(UnityEngine.CircleCollider2D)),
            new UnityTypeInfo(59, true, "HingeJoint", typeof(UnityEngine.HingeJoint)),
            new UnityTypeInfo(60, true, "PolygonCollider2D", typeof(UnityEngine.PolygonCollider2D)),
            new UnityTypeInfo(61, true, "BoxCollider2D", typeof(UnityEngine.BoxCollider2D)),
            new UnityTypeInfo(62, false, "PhysicsMaterial2D", typeof(UnityEngine.PhysicsMaterial2D)),
            new UnityTypeInfo(64, true, "MeshCollider", typeof(UnityEngine.MeshCollider)),
            new UnityTypeInfo(65, true, "BoxCollider", typeof(UnityEngine.BoxCollider)),
            new UnityTypeInfo(66, true, "CompositeCollider2D", typeof(UnityEngine.CompositeCollider2D)),
            new UnityTypeInfo(68, true, "EdgeCollider2D", typeof(UnityEngine.EdgeCollider2D)),
            new UnityTypeInfo(70, true, "CapsuleCollider2D", typeof(UnityEngine.CapsuleCollider2D)),
            new UnityTypeInfo(72, false, "ComputeShader", typeof(UnityEngine.ComputeShader)),
            new UnityTypeInfo(74, false, "AnimationClip", typeof(UnityEngine.AnimationClip)),
            new UnityTypeInfo(75, true, "ConstantForce", typeof(UnityEngine.ConstantForce)),
            new UnityTypeInfo(76, true, "WorldParticleCollider", null), // hidden typeof(UnityEngine.WorldParticleCollider)
            new UnityTypeInfo(78, false, "TagManager", null), // hidden typeof(UnityEditor.TagManager)
            new UnityTypeInfo(81, true, "AudioListener", typeof(UnityEngine.AudioListener)),
            new UnityTypeInfo(82, true, "AudioSource", typeof(UnityEngine.AudioSource)),
            new UnityTypeInfo(83, false, "AudioClip", typeof(UnityEngine.AudioClip)),
            new UnityTypeInfo(84, false, "RenderTexture", typeof(UnityEngine.RenderTexture)),
            new UnityTypeInfo(86, false, "CustomRenderTexture", typeof(UnityEngine.CustomRenderTexture)),
            new UnityTypeInfo(87, true, "MeshParticleEmitter", typeof(UnityEngine.MeshParticleEmitter)),
            new UnityTypeInfo(88, true, "ParticleEmitter", typeof(UnityEngine.ParticleEmitter)),
            new UnityTypeInfo(89, false, "Cubemap", typeof(UnityEngine.Cubemap)),
            new UnityTypeInfo(90, false, "Avatar", typeof(UnityEngine.Avatar)),
            new UnityTypeInfo(91, false, "AnimatorController", typeof(UnityEditor.Animations.AnimatorController)),
            new UnityTypeInfo(92, true, "GUILayer", typeof(UnityEngine.GUILayer)),
            new UnityTypeInfo(93, false, "RuntimeAnimatorController", typeof(UnityEngine.RuntimeAnimatorController)),
            new UnityTypeInfo(94, false, "ScriptMapper", null),
            new UnityTypeInfo(95, true, "Animator", typeof(UnityEngine.Animator)),
            new UnityTypeInfo(96, true, "TrailRenderer", typeof(UnityEngine.TrailRenderer)),
            new UnityTypeInfo(98, false, "DelayedCallManager", null),
            new UnityTypeInfo(102, true, "TextMesh", typeof(UnityEngine.TextMesh)),
            new UnityTypeInfo(104, false, "RenderSettings", typeof(UnityEngine.RenderSettings)),
            new UnityTypeInfo(108, true, "Light", typeof(UnityEngine.Light)),
            new UnityTypeInfo(109, false, "CGProgram", null),
            new UnityTypeInfo(110, false, "BaseAnimationTrack", null),
            new UnityTypeInfo(111, true, "Animation", typeof(UnityEngine.Animation)),
            new UnityTypeInfo(114, true, "MonoBehaviour", typeof(UnityEngine.MonoBehaviour), typeof(YamlMonoBehaviour)),
            new UnityTypeInfo(115, false, "MonoScript", typeof(UnityEditor.MonoScript)),
            new UnityTypeInfo(116, false, "MonoManager", null), // hidden typeof(UnityEditor.MonoManager)
            new UnityTypeInfo(117, false, "Texture3D", typeof(UnityEngine.Texture3D)),
            new UnityTypeInfo(118, false, "NewAnimationTrack", null),
            new UnityTypeInfo(119, true, "Projector", typeof(UnityEngine.Projector)),
            new UnityTypeInfo(120, true, "LineRenderer", typeof(UnityEngine.LineRenderer)),
            new UnityTypeInfo(121, false, "Flare", typeof(UnityEngine.Flare)),
            new UnityTypeInfo(122, true, "Halo", null), // hidden typeof(UnityEngine.Halo)
            new UnityTypeInfo(123, true, "LensFlare", typeof(UnityEngine.LensFlare)),
            new UnityTypeInfo(124, true, "FlareLayer", typeof(UnityEngine.FlareLayer)),
            new UnityTypeInfo(125, false, "HaloLayer", null),
            new UnityTypeInfo(126, false, "NavMeshProjectSettings", null),
            new UnityTypeInfo(128, false, "Font", typeof(UnityEngine.Font)),
            new UnityTypeInfo(129, false, "PlayerSettings", typeof(UnityEditor.PlayerSettings)),
            new UnityTypeInfo(130, false, "NamedObject", null),
            new UnityTypeInfo(131, true, "GUITexture", typeof(UnityEngine.GUITexture)),
            new UnityTypeInfo(132, true, "GUIText", typeof(UnityEngine.GUIText)),
            new UnityTypeInfo(133, true, "GUIElement", typeof(UnityEngine.GUIElement)),
            new UnityTypeInfo(134, false, "PhysicMaterial", typeof(UnityEngine.PhysicMaterial)),
            new UnityTypeInfo(135, true, "SphereCollider", typeof(UnityEngine.SphereCollider)),
            new UnityTypeInfo(136, true, "CapsuleCollider", typeof(UnityEngine.CapsuleCollider)),
            new UnityTypeInfo(137, true, "SkinnedMeshRenderer", typeof(UnityEngine.SkinnedMeshRenderer)),
            new UnityTypeInfo(138, true, "FixedJoint", typeof(UnityEngine.FixedJoint)),
            new UnityTypeInfo(141, false, "BuildSettings", null),
            new UnityTypeInfo(142, false, "AssetBundle", typeof(UnityEngine.AssetBundle)),
            new UnityTypeInfo(143, true, "CharacterController", typeof(UnityEngine.CharacterController)),
            new UnityTypeInfo(144, true, "CharacterJoint", typeof(UnityEngine.CharacterJoint)),
            new UnityTypeInfo(145, true, "SpringJoint", typeof(UnityEngine.SpringJoint)),
            new UnityTypeInfo(146, true, "WheelCollider", typeof(UnityEngine.WheelCollider)),
            new UnityTypeInfo(147, false, "ResourceManager", null),
            new UnityTypeInfo(148, true, "NetworkView", typeof(UnityEngine.NetworkView)),
            new UnityTypeInfo(149, false, "NetworkManager", null),
            new UnityTypeInfo(150, false, "PreloadData", null),
            new UnityTypeInfo(152, false, "MovieTexture", typeof(UnityEngine.MovieTexture)),
            new UnityTypeInfo(153, true, "ConfigurableJoint", typeof(UnityEngine.ConfigurableJoint)),
            new UnityTypeInfo(154, true, "TerrainCollider", typeof(UnityEngine.TerrainCollider)),
            new UnityTypeInfo(155, false, "MasterServerInterface", null),
            new UnityTypeInfo(156, false, "TerrainData", typeof(UnityEngine.TerrainData)),
            new UnityTypeInfo(157, false, "LightmapSettings", typeof(UnityEngine.LightmapSettings)),
            new UnityTypeInfo(158, false, "WebCamTexture", typeof(UnityEngine.WebCamTexture)),
            new UnityTypeInfo(159, false, "EditorSettings", typeof(UnityEditor.EditorSettings)),
            new UnityTypeInfo(162, false, "EditorUserSettings", typeof(UnityEditor.EditorUserSettings)),
            new UnityTypeInfo(164, true, "AudioReverbFilter", typeof(UnityEngine.AudioReverbFilter)),
            new UnityTypeInfo(165, true, "AudioHighPassFilter", typeof(UnityEngine.AudioHighPassFilter)),
            new UnityTypeInfo(166, true, "AudioChorusFilter", typeof(UnityEngine.AudioChorusFilter)),
            new UnityTypeInfo(167, true, "AudioReverbZone", typeof(UnityEngine.AudioReverbZone)),
            new UnityTypeInfo(168, true, "AudioEchoFilter", typeof(UnityEngine.AudioEchoFilter)),
            new UnityTypeInfo(169, true, "AudioLowPassFilter", typeof(UnityEngine.AudioLowPassFilter)),
            new UnityTypeInfo(170, true, "AudioDistortionFilter", typeof(UnityEngine.AudioDistortionFilter)),
            new UnityTypeInfo(171, false, "SparseTexture", typeof(UnityEngine.SparseTexture)),
            new UnityTypeInfo(180, true, "AudioBehaviour", typeof(UnityEngine.AudioBehaviour)),
            new UnityTypeInfo(181, false, "AudioFilter", null),
            new UnityTypeInfo(182, true, "WindZone", typeof(UnityEngine.WindZone)),
            new UnityTypeInfo(183, true, "Cloth", typeof(UnityEngine.Cloth)),
            new UnityTypeInfo(184, false, "SubstanceArchive", typeof(UnityEditor.SubstanceArchive)),
            new UnityTypeInfo(185, false, "ProceduralMaterial", typeof(UnityEngine.ProceduralMaterial)),
            new UnityTypeInfo(186, false, "ProceduralTexture", typeof(UnityEngine.ProceduralTexture)),
            new UnityTypeInfo(187, false, "Texture2DArray", typeof(UnityEngine.Texture2DArray)),
            new UnityTypeInfo(188, false, "CubemapArray", typeof(UnityEngine.CubemapArray)),
            new UnityTypeInfo(191, true, "OffMeshLink", typeof(UnityEngine.AI.OffMeshLink)),
            new UnityTypeInfo(192, true, "OcclusionArea", typeof(UnityEngine.OcclusionArea)),
            new UnityTypeInfo(193, true, "Tree", typeof(UnityEngine.Tree)),
            new UnityTypeInfo(195, true, "NavMeshAgent", typeof(UnityEngine.AI.NavMeshAgent)),
            new UnityTypeInfo(196, false, "NavMeshSettings", null),
            new UnityTypeInfo(198, true, "ParticleSystem", typeof(UnityEngine.ParticleSystem)),
            new UnityTypeInfo(199, true, "ParticleSystemRenderer", typeof(UnityEngine.ParticleSystemRenderer)),
            new UnityTypeInfo(200, false, "ShaderVariantCollection", typeof(UnityEngine.ShaderVariantCollection)),
            new UnityTypeInfo(205, true, "LODGroup", typeof(UnityEngine.LODGroup)),
            new UnityTypeInfo(206, false, "BlendTree", typeof(UnityEditor.Animations.BlendTree)),
            new UnityTypeInfo(207, false, "Motion", typeof(UnityEngine.Motion)),
            new UnityTypeInfo(208, true, "NavMeshObstacle", typeof(UnityEngine.AI.NavMeshObstacle)),
            new UnityTypeInfo(210, true, "SortingGroup", typeof(UnityEngine.Rendering.SortingGroup)),
            new UnityTypeInfo(212, true, "SpriteRenderer", typeof(UnityEngine.SpriteRenderer)),
            new UnityTypeInfo(213, false, "Sprite", typeof(UnityEngine.Sprite)),
            new UnityTypeInfo(214, false, "CachedSpriteAtlas", null),
            new UnityTypeInfo(215, true, "ReflectionProbe", typeof(UnityEngine.ReflectionProbe)),
            new UnityTypeInfo(218, true, "Terrain", typeof(UnityEngine.Terrain)),
            new UnityTypeInfo(220, true, "LightProbeGroup", typeof(UnityEngine.LightProbeGroup)),
            new UnityTypeInfo(221, false, "AnimatorOverrideController", typeof(UnityEngine.AnimatorOverrideController)),
            new UnityTypeInfo(222, true, "CanvasRenderer", typeof(UnityEngine.CanvasRenderer)),
            new UnityTypeInfo(223, true, "Canvas", typeof(UnityEngine.Canvas)),
            new UnityTypeInfo(224, true, "RectTransform", typeof(UnityEngine.RectTransform), typeof(YamlRectTransform)),
            new UnityTypeInfo(225, true, "CanvasGroup", typeof(UnityEngine.CanvasGroup)),
            new UnityTypeInfo(226, false, "BillboardAsset", typeof(UnityEngine.BillboardAsset)),
            new UnityTypeInfo(227, true, "BillboardRenderer", typeof(UnityEngine.BillboardRenderer)),
            new UnityTypeInfo(228, false, "SpeedTreeWindAsset", null), // hidden typeof(UnityEngine.SpeedTreeWindAsset)
            new UnityTypeInfo(229, true, "AnchoredJoint2D", typeof(UnityEngine.AnchoredJoint2D)),
            new UnityTypeInfo(230, true, "Joint2D", typeof(UnityEngine.Joint2D)),
            new UnityTypeInfo(231, true, "SpringJoint2D", typeof(UnityEngine.SpringJoint2D)),
            new UnityTypeInfo(232, true, "DistanceJoint2D", typeof(UnityEngine.DistanceJoint2D)),
            new UnityTypeInfo(233, true, "HingeJoint2D", typeof(UnityEngine.HingeJoint2D)),
            new UnityTypeInfo(234, true, "SliderJoint2D", typeof(UnityEngine.SliderJoint2D)),
            new UnityTypeInfo(235, true, "WheelJoint2D", typeof(UnityEngine.WheelJoint2D)),
            new UnityTypeInfo(236, false, "ClusterInputManager", null),
            new UnityTypeInfo(237, false, "BaseVideoTexture", null),
            new UnityTypeInfo(238, false, "NavMeshData", typeof(UnityEngine.AI.NavMeshData)),
            new UnityTypeInfo(240, false, "AudioMixer", typeof(UnityEngine.Audio.AudioMixer)),
            new UnityTypeInfo(241, false, "AudioMixerController", null),
            new UnityTypeInfo(243, false, "AudioMixerGroupController", null),
            new UnityTypeInfo(244, false, "AudioMixerEffectController", null),
            new UnityTypeInfo(245, false, "AudioMixerSnapshotController", null),
            new UnityTypeInfo(246, true, "PhysicsUpdateBehaviour2D", typeof(UnityEngine.PhysicsUpdateBehaviour2D)),
            new UnityTypeInfo(247, true, "ConstantForce2D", typeof(UnityEngine.ConstantForce2D)),
            new UnityTypeInfo(248, true, "Effector2D", typeof(UnityEngine.Effector2D)),
            new UnityTypeInfo(249, true, "AreaEffector2D", typeof(UnityEngine.AreaEffector2D)),
            new UnityTypeInfo(250, true, "PointEffector2D", typeof(UnityEngine.PointEffector2D)),
            new UnityTypeInfo(251, true, "PlatformEffector2D", typeof(UnityEngine.PlatformEffector2D)),
            new UnityTypeInfo(252, true, "SurfaceEffector2D", typeof(UnityEngine.SurfaceEffector2D)),
            new UnityTypeInfo(253, true, "BuoyancyEffector2D", typeof(UnityEngine.BuoyancyEffector2D)),
            new UnityTypeInfo(254, true, "RelativeJoint2D", typeof(UnityEngine.RelativeJoint2D)),
            new UnityTypeInfo(255, true, "FixedJoint2D", typeof(UnityEngine.FixedJoint2D)),
            new UnityTypeInfo(256, true, "FrictionJoint2D", typeof(UnityEngine.FrictionJoint2D)),
            new UnityTypeInfo(257, true, "TargetJoint2D", typeof(UnityEngine.TargetJoint2D)),
            new UnityTypeInfo(258, false, "LightProbes", typeof(UnityEngine.LightProbes)),
            new UnityTypeInfo(259, true, "LightProbeProxyVolume", typeof(UnityEngine.LightProbeProxyVolume)),
            new UnityTypeInfo(271, false, "SampleClip", null),
            new UnityTypeInfo(272, false, "AudioMixerSnapshot", typeof(UnityEngine.Audio.AudioMixerSnapshot)),
            new UnityTypeInfo(273, false, "AudioMixerGroup", typeof(UnityEngine.Audio.AudioMixerGroup)),
            new UnityTypeInfo(290, false, "AssetBundleManifest", typeof(UnityEngine.AssetBundleManifest)),
            new UnityTypeInfo(300, false, "RuntimeInitializeOnLoadManager", null), // hidden typeof(UnityEngine.RuntimeInitializeOnLoadManager)
            new UnityTypeInfo(301, false, "CloudWebServicesManager", null),
            new UnityTypeInfo(303, false, "UnityAnalyticsManager", null),
            new UnityTypeInfo(304, false, "CrashReportManager", null),
            new UnityTypeInfo(305, false, "PerformanceReportingManager", null),
            new UnityTypeInfo(310, false, "UnityConnectSettings", null),
            new UnityTypeInfo(319, false, "AvatarMask", typeof(UnityEngine.AvatarMask)),
            new UnityTypeInfo(320, true, "PlayableDirector", typeof(UnityEngine.Playables.PlayableDirector)),
            new UnityTypeInfo(328, true, "VideoPlayer", typeof(UnityEngine.Video.VideoPlayer)),
            new UnityTypeInfo(329, false, "VideoClip", typeof(UnityEngine.Video.VideoClip)),
            new UnityTypeInfo(331, true, "SpriteMask", typeof(UnityEngine.SpriteMask)),
            new UnityTypeInfo(362, true, "WorldAnchor", typeof(UnityEngine.XR.WSA.WorldAnchor)),
            new UnityTypeInfo(363, false, "OcclusionCullingData", null),
            new UnityTypeInfo(1001, false, "Prefab", null, typeof(YamlPrefab)),
            new UnityTypeInfo(1002, false, "EditorExtensionImpl", null),
            new UnityTypeInfo(1003, false, "AssetImporter", typeof(UnityEditor.AssetImporter)),
            new UnityTypeInfo(1004, false, "AssetDatabaseV1", null),
            new UnityTypeInfo(1005, false, "Mesh3DSImporter", null),
            new UnityTypeInfo(1006, false, "TextureImporter", typeof(UnityEditor.TextureImporter)),
            new UnityTypeInfo(1007, false, "ShaderImporter", typeof(UnityEditor.ShaderImporter)),
            new UnityTypeInfo(1008, false, "ComputeShaderImporter", null),
            new UnityTypeInfo(1020, false, "AudioImporter", typeof(UnityEditor.AudioImporter)),
            new UnityTypeInfo(1026, false, "HierarchyState", null),
            new UnityTypeInfo(1028, false, "AssetMetaData", null),
            new UnityTypeInfo(1029, false, "DefaultAsset", typeof(UnityEditor.DefaultAsset)),
            new UnityTypeInfo(1030, false, "DefaultImporter", null),
            new UnityTypeInfo(1031, false, "TextScriptImporter", null),
            new UnityTypeInfo(1032, false, "SceneAsset", typeof(UnityEditor.SceneAsset)),
            new UnityTypeInfo(1034, false, "NativeFormatImporter", null),
            new UnityTypeInfo(1035, false, "MonoImporter", typeof(UnityEditor.MonoImporter)),
            new UnityTypeInfo(1038, false, "LibraryAssetImporter", null),
            new UnityTypeInfo(1040, false, "ModelImporter", typeof(UnityEditor.ModelImporter)),
            new UnityTypeInfo(1041, false, "FBXImporter", null),
            new UnityTypeInfo(1042, false, "TrueTypeFontImporter", typeof(UnityEditor.TrueTypeFontImporter)),
            new UnityTypeInfo(1044, false, "MovieImporter", typeof(UnityEditor.MovieImporter)),
            new UnityTypeInfo(1045, false, "EditorBuildSettings", typeof(UnityEditor.EditorBuildSettings)),
            new UnityTypeInfo(1048, false, "InspectorExpandedState", null),
            new UnityTypeInfo(1049, false, "AnnotationManager", null),
            new UnityTypeInfo(1050, false, "PluginImporter", typeof(UnityEditor.PluginImporter)),
            new UnityTypeInfo(1051, false, "EditorUserBuildSettings", typeof(UnityEditor.EditorUserBuildSettings)),
            new UnityTypeInfo(1055, false, "IHVImageFormatImporter", typeof(UnityEditor.IHVImageFormatImporter)),
            new UnityTypeInfo(1101, false, "AnimatorStateTransition", typeof(UnityEditor.Animations.AnimatorStateTransition)),
            new UnityTypeInfo(1102, false, "AnimatorState", typeof(UnityEditor.Animations.AnimatorState)),
            new UnityTypeInfo(1105, false, "HumanTemplate", typeof(UnityEditor.HumanTemplate)),
            new UnityTypeInfo(1107, false, "AnimatorStateMachine", typeof(UnityEditor.Animations.AnimatorStateMachine)),
            new UnityTypeInfo(1108, false, "PreviewAnimationClip", null),
            new UnityTypeInfo(1109, false, "AnimatorTransition", typeof(UnityEditor.Animations.AnimatorTransition)),
            new UnityTypeInfo(1110, false, "SpeedTreeImporter", typeof(UnityEditor.SpeedTreeImporter)),
            new UnityTypeInfo(1111, false, "AnimatorTransitionBase", typeof(UnityEditor.Animations.AnimatorTransitionBase)),
            new UnityTypeInfo(1112, false, "SubstanceImporter", typeof(UnityEditor.SubstanceImporter)),
            new UnityTypeInfo(1113, false, "LightmapParameters", typeof(UnityEditor.LightmapParameters)),
            new UnityTypeInfo(1120, false, "LightingDataAsset", typeof(UnityEditor.LightingDataAsset)),
            new UnityTypeInfo(1124, false, "SketchUpImporter", null),
            new UnityTypeInfo(1125, false, "BuildReport", null),
            new UnityTypeInfo(1126, false, "PackedAssets", null),
            new UnityTypeInfo(1127, false, "VideoClipImporter", typeof(UnityEditor.VideoClipImporter)),
            new UnityTypeInfo(100000, false, "int", null),
            new UnityTypeInfo(100001, false, "bool", null),
            new UnityTypeInfo(100002, false, "float", null),
            new UnityTypeInfo(100003, false, "MonoObject", null),
            new UnityTypeInfo(100004, false, "Collision", typeof(UnityEngine.Collision)),
            new UnityTypeInfo(100005, false, "Vector3f", null),
            new UnityTypeInfo(100006, false, "RootMotionData", null),
            new UnityTypeInfo(100007, false, "Collision2D", typeof(UnityEngine.Collision2D)),
            new UnityTypeInfo(100008, false, "AudioMixerLiveUpdateFloat", null),
            new UnityTypeInfo(100009, false, "AudioMixerLiveUpdateBool", null),
            new UnityTypeInfo(100010, false, "Polygon2D", null),
            new UnityTypeInfo(100011, false, "void", null),
            new UnityTypeInfo(19719996, true, "TilemapCollider2D", typeof(UnityEngine.Tilemaps.TilemapCollider2D)),
            new UnityTypeInfo(156049354, true, "Grid", typeof(UnityEngine.Grid)),
            new UnityTypeInfo(483693784, true, "TilemapRenderer", typeof(UnityEngine.Tilemaps.TilemapRenderer)),
            new UnityTypeInfo(638013454, false, "SpriteAtlasDatabase", null),
            new UnityTypeInfo(644342135, false, "CachedSpriteAtlasRuntimeData", null),
            new UnityTypeInfo(668709126, false, "BuiltAssetBundleInfoSet", null),
            new UnityTypeInfo(687078895, false, "SpriteAtlas", null),
            new UnityTypeInfo(1152215463, false, "AssemblyDefinitionAsset", null),
            new UnityTypeInfo(1268269756, false, "GameObjectRecorder", null),
            new UnityTypeInfo(1325145578, false, "LightingDataAssetParent", null),
            new UnityTypeInfo(1480428607, false, "LowerResBlitTexture", null),
            new UnityTypeInfo(1571458007, false, "RenderPassAttachment", null),
            new UnityTypeInfo(1742807556, true, "GridLayout", typeof(UnityEngine.GridLayout)),
            new UnityTypeInfo(1766753193, false, "AssemblyDefinitionImporter", null),
            new UnityTypeInfo(1839735485, true, "Tilemap", typeof(UnityEngine.Tilemaps.Tilemap)),
            new UnityTypeInfo(2089858483, false, "ScriptedImporter", null),
            new UnityTypeInfo(2126867596, false, "TilemapEditorUserSettings", null), // hidden typeof(UnityEditor.TilemapEditorUserSettings)
        };
    }
#pragma warning restore 618

    internal class UnityTypeInfo
    {
        internal int ID {get;}
        internal bool IsComponent {get;}
        internal string Name {get;}
        internal Type CsType {get;}
        internal Type YamlImplType {get;}

        internal UnityTypeInfo(int id, bool isComponent, string name, Type csType, Type yamlImplType = null)
        {
            ID = id;
            IsComponent = isComponent;
            Name = name;
            CsType = csType;
            YamlImplType = yamlImplType;
        }
    }

    internal class AssetInfo
    {
        static internal readonly Regex localIdentifier_re = new Regex(@"^    localIdentifier: (-?[0-9]+)");
        static internal readonly Regex scriptClassName_re = new Regex(@"^    scriptClassName: (.+)");

        private Dictionary<long, string> idToClassName = new Dictionary<long, string>();

        private void readFromInfoFile(string filePath)
        {
            using(var reader = new System.IO.StreamReader(filePath))
            {
                long localId = -1;
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    Match match;
                    if ((match = localIdentifier_re.Match(line)).Success)
                    {
                        localId = long.Parse(match.Groups[1].Value);
                    }
                    else if ((match = scriptClassName_re.Match(line)).Success)
                    {
                        string scriptClassName = match.Groups[1].Value;
                        try {
                            idToClassName.Add(localId, scriptClassName);
                        }
                        catch (ArgumentException)
                        {
                            Debug.Log("localId=" + localId + " n=" + scriptClassName);
                        }

                    }
                }
            }
        }

        internal void Load(string guid)
        {
            var h2 = guid.Substring(0, 2);
            var fname = Application.dataPath + "/../Library/metadata/" + h2 + "/" + guid + ".info";
            try
            {
                readFromInfoFile(fname);
            }
            catch (System.IO.FileNotFoundException)
            {
                Debug.LogWarning("FileNotFound. wong guid?");
                // TODO show in GUI
            }
        }

        internal  string GetName(long fileID)
        {
            if (idToClassName.ContainsKey(fileID))
            {
                return idToClassName[fileID];
            }
            else
            {
                // TODO return (out) value and isAvailable? or return null?
                return "(not found. fileID=" + fileID + ")";
            }
        }
    }

    // fileID:
    // 8400000 : renderTexture

    public class AssetInfoDatabase
    {
        static private Dictionary<string, AssetInfo> guidToInfo = new Dictionary<string, AssetInfo>();

        static public string NameOf(string guid, long fileID)
        {
            AssetInfo info;
            if (guidToInfo.ContainsKey(guid))
            {
                info = guidToInfo[guid];
            }
            else
            {
                info = new AssetInfo();
                info.Load(guid);
                guidToInfo.Add(guid, info);
            }

            return info.GetName(fileID);
        }
    }


}


namespace Iwsd.YamlAssetBrowser
{
    // user resizable splited area
    //
    // - https://github.com/miguel12345/EditorGUISplitView/blob/master/Assets/EditorGUISplitView/Scripts/Editor/EditorGUISplitView.cs
    // - https://answers.unity.com/questions/546686/editorguilayout-split-view-resizable-scroll-view.html
    // - http://gram.gs/gramlog/creating-editor-windows-in-unity/
    internal class SplitPane
    {
        private float panelSizeRatio;
        private Rect thisRect;
        private Rect firstRect;
        private Rect handleRect;
        private float handleSize = 10.0f;
        private bool isResizing;

        public SplitPane()
        {
        }

        public void Begin(float splitRatio)
        {
            panelSizeRatio = splitRatio;

            Rect tmp = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            if (0f < tmp.width)
            {
                thisRect = tmp;
            }

            // Round fraction to avoid being blured
            firstRect = new Rect(thisRect.x, thisRect.y,
                                 thisRect.width, (int)((thisRect.height - handleSize) * panelSizeRatio));
            GUILayout.BeginArea(firstRect);
        }

        public void Split()
        {
            GUILayout.EndArea();

            // draws handle later

            Rect rect = new Rect(thisRect.x, (int)(thisRect.y + firstRect.height + handleSize),
                                 thisRect.width, (int)(thisRect.height - handleSize - firstRect.height));
            GUILayout.BeginArea(rect);
        }

        public float End()
        {
            GUILayout.EndArea();

            DrawHandle();
            ProcessEvents(Event.current);

            EditorGUILayout.EndVertical();

            return panelSizeRatio;
        }

        private void DrawHandle()
        {
            handleRect = new Rect(thisRect.x, (int)(thisRect.y + firstRect.height),
                               thisRect.width, handleSize);

            // must be in OnGUI()
            GUIStyle style = (GUIStyle)"WindowBottomResize";
            // style = new GUIStyle();
            // style.normal.background

            GUILayout.BeginArea(handleRect, style);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeVertical);
        }

        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if ((e.button == 0) && (handleRect.Contains(e.mousePosition)))
                    {
                        isResizing = true;
                    }
                    break;

                case EventType.MouseUp:
                    isResizing = false;
                    break;
            }

            if (isResizing)
            {
                if ((e.mousePosition.x < thisRect.x) || (thisRect.x + thisRect.width < e.mousePosition.x))
                {
                    isResizing = false;
                }
            }
            if (isResizing)
            {
                float resizeAxPos = e.mousePosition.y - thisRect.y;
                if ((handleSize <= resizeAxPos) && (resizeAxPos <= thisRect.height))
                {
                    panelSizeRatio =  resizeAxPos / thisRect.height;
                }
                else
                {
                    isResizing = false;
                }
            }
        }
    }

    internal class LargeContentTextArea
    {
        private string _lineNumberFormat = "{0,5:#####} ";
        string lineNumberFormat {
            get {return _lineNumberFormat;}
            set { _lineNumberFormat = value; dirty = true;}
        }

        private string controlName;
        private List<string> linesBuffer;
        private Vector2 scrollPos = new Vector2(0, 0);

        private bool dirty;
        private string lastRenderedString;

        private int preLines;
        private int actualLines;
        private int postLines;
        private float lineHeight = EditorGUIUtility.singleLineHeight;
        
        internal LargeContentTextArea()
        {
            controlName = "LargeContentTextArea" + this.GetHashCode();
            linesBuffer = new List<string>();
            dirty = true;
        }

        public void Clear()
        {
            linesBuffer.Clear();
            SetScrollPositionAt(0);
            dirty = true;
        }

        public void SetScrollPositionAt(int lineNo)
        {
            // Workaround for displacement +2 line. I don't understand why constant +2 occurs.
            float y = (lineNo - 3) * lineHeight - (lineHeight * 0.4f);
            y = (y < 0)? 0: y;
            
            scrollPos = new Vector2(0, y);
            dirty = true;
        }
        
        public void SetMultilineString(string lines)
        {
            AddLines(lines.Split('\n'));
        }
    
        public void AddLine(string l)
        {
            linesBuffer.Add(l);
            dirty = true;
        }

        // @param lineNumberFormat format string of String.Format() for headdind line number of each line.
        // If null, not append line number.
        public void AddLines(IEnumerable<string> lines)
        {
            foreach (var l in lines)
            {
                linesBuffer.Add(l);
            }
            dirty = true;
        }
        
        // keyboardControl PageUp, PageDown
        // MEMO
        // - GUIUtility.GetControlID has FocusType parameter
        // - EditorGUI.SelectableLabel seems hold text while it has focus
        // - GUI.Label is almost DrawString (?)

        internal void OnGUI()
        {
    
            var scrollPos_tmp = EditorGUILayout.BeginScrollView(scrollPos);
            if (scrollPos_tmp != scrollPos)
            {
                scrollPos = scrollPos_tmp;
                dirty = true;

                // Workaround. If TextArea has focus, it doesn't change content appearance.
                // so remove focus if content refreshing is needed.
                if (GUI.GetNameOfFocusedControl() == controlName)
                {
                    GUI.FocusControl(null);
                }
            }

            if (dirty)
            {
                int windoLines = 50;
                int aboveRoom = 10;
                preLines = (int)(scrollPos.y / lineHeight) - aboveRoom;
                preLines = (preLines < 0)? 0: preLines;
                postLines = linesBuffer.Count - windoLines - preLines;
                postLines = (postLines < 0)? 0: postLines; // clamp

                var sb = new System.Text.StringBuilder();
                actualLines = 0;
                for (int i = preLines; (i < preLines + windoLines) && (i < linesBuffer.Count); i++)
                {
                    actualLines++;
                    if (lineNumberFormat != null)
                    {
                        sb.Append(string.Format(lineNumberFormat, i+1));
                    }
                    sb.AppendLine(linesBuffer[i]);
                }
                lastRenderedString = sb.ToString();
                dirty = false;
            }

            // TODO context menu to search pointing line reference
            // (It might necessary to replace TextArea with Label (or something) because there's no way to replace or add context menu of TextArea.
            // Or it's possible to override menu event behaviour completely? Anyway, I shoud investigate more)
            Event evt = Event.current;
            if (evt.type == EventType.ContextClick)
            {
                Debug.Log("ContextClick");
            }
            // evt.GetTypeForControl(id)
            
            GUILayout.Space(preLines * lineHeight);

            GUI.SetNextControlName(controlName);
            Rect textArea = EditorGUILayout.GetControlRect(false, lineHeight * actualLines);
            GUIContent content = new GUIContent(lastRenderedString);
            // var style = new GUIStyle(); // GUI.skin.textArea (GUIStyle)"OL TextField" //  (GUIStyle)"TextFieldDropDownText"
            // style.wordWrap = true;
            // style.alignment = TextAnchor.LowerRight; // UpperLeft;
            // GUI.DrawTexture(textArea, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill); // to check rect
            // GUI.Label(textArea, content, style);
            EditorGUI.TextArea(textArea, content.text); // , style);

            GUILayout.Space(postLines * lineHeight);

            EditorGUILayout.EndScrollView();
        }
    }

    internal class HelpPupupContent : PopupWindowContent
    {
        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical("", (GUIStyle)"HelpBox"); // "Tooltip", "VCS_StickyNote", "U2D.createRect", "grey_border"
            GUILayout.Space(10);
            EditorGUILayout.LabelField("VRC_Iwsd / YAML Asset Browser", (GUIStyle)"ProgressBarBar", GUILayout.ExpandWidth(true)); // new GUIStyle(){fontStyle = FontStyle.Bold});
            EditorGUILayout.LabelField("  ver. YamlAssetBrowser-20190904");
            GUILayout.Space(10);
            if (GUILayout.Button("Open online help", GUILayout.ExpandWidth(false)))
            {
                Help.BrowseURL("http://vrcprog.hatenablog.jp/entry/mywork-yaml-asset-browser");
            }
            GUILayout.Space(10);
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.ExpandWidth(false)))
            {
                editorWindow.Close();
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 130);
        }

    }

    public class Browser : EditorWindow
    {
        [MenuItem("Window/VRC_Iwsd/Yaml Asset Browser")]
        static void OpenYamlAssetBrowser()
        {
            var window = CreateInstance<Browser>();
            window.Show();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        Vector2 textAreaScrollPosition = new Vector2(0, 0);
        string searchInput = "";
        string[] infoAreaString = {"(info text)"};
        [SerializeField] bool selectAfterSearch = false;

        YamlAssetParser AssetParser;

        [SerializeField] TreeViewState m_TreeViewState;
        [SerializeField] string VisitedAssetFilePath;
        [SerializeField] string VisitedAssetName;
        [SerializeField] int? VisitedObjectInstanceId;

        SplitPane SplitPane;
        [SerializeField] float panelSizeRatio = 0.6f;

        YamlObjectsTreeView TreeView;
        SearchField SearchField;
    
        Rect helpButtonRect;
        GUIContent helpBtContent;
        HelpPupupContent helpPopupContent = new HelpPupupContent();
        LargeContentTextArea largeContentTextArea = new LargeContentTextArea();

        void OnEnable()
        {
            // create SerializeField instances if not exist 
            if (m_TreeViewState == null)
            {
                m_TreeViewState = new TreeViewState();
            }

            titleContent = new GUIContent("Asset Browser", "VRC_Iwsd / YAML Asset Browser");
                
            SplitPane = new SplitPane();

            AssetParser = new YamlAssetParser();
            if ((VisitedAssetFilePath != null) && (VisitedAssetFilePath != ""))
            {
                AssetParser.ParseFile(VisitedAssetFilePath);
            }
            TreeView = new YamlObjectsTreeView(m_TreeViewState, AssetParser, OnObjectSelectedInTreeView);

            largeContentTextArea = new LargeContentTextArea();
            largeContentTextArea.AddLines(AssetParser.GetAllYamlLines());

            SearchField = new SearchField();
            SearchField.downOrUpArrowKeyPressed += TreeView.SetFocusAndEnsureSelectedItem;

            helpBtContent = EditorGUIUtility.IconContent("_Help");

        }

        private void OnObjectSelectedInTreeView(YamlObject obj)
        {
            largeContentTextArea.SetScrollPositionAt(obj.LineNo);
        }

        void OnGUI()
        {
            if (TreeView == null) // avoid continuous NullReferenceException (sometimes I break it during development)
            {
                return;
            }

            OnGUI_Toolbar();
            SplitPane.Begin(panelSizeRatio);
            OnGUI_UpperPanel();
            SplitPane.Split();
            OnGUI_LowerPanel();
            panelSizeRatio = SplitPane.End();

        }

        private void OnGUI_Toolbar()
        {
            // from Unity TreeViewExamples.zip
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            // GUILayout.Space (10);

            var active = Selection.activeObject; // Selection.activeGameObject;
            EditorGUI.BeginDisabledGroup(active == null);
            if (GUILayout.Button("Load", EditorStyles.toolbarButton))
            {
                LoadRelatedAsset(active);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(VisitedObjectInstanceId == null);
            if (GUILayout.Button("Ping", EditorStyles.toolbarButton))
            {
                EditorGUIUtility.PingObject(VisitedObjectInstanceId.Value);
            }
            EditorGUI.EndDisabledGroup();

            // GUILayout.Space (100);
            GUILayout.FlexibleSpace();

            TreeView.searchString = SearchField.OnToolbarGUI(TreeView.searchString); // or OnGUI

            // Help Button
            if (GUILayout.Button(helpBtContent, EditorStyles.toolbarButton))
            {
                Rect rp = this.position;
                Vector2 cs = helpPopupContent.GetWindowSize();
                Rect rect = new Rect(rp.width/2 - cs.x/2, helpButtonRect.y + 40, 0, helpButtonRect.height);
                PopupWindow.Show(rect, helpPopupContent);
            }
            if (Event.current.type == EventType.Repaint)
            {
                helpButtonRect = GUILayoutUtility.GetLastRect();
            }

            GUILayout.EndHorizontal(); // toolbar
        }

        private void OnGUI_UpperPanel()
        {
            // DropAreaGUITest();
            // if (GUILayout.Button("TEST", GUILayout.ExpandWidth(false))) {
            //     if (Selection.activeObject)
            //     {
            //         infoAreaString[0] = Selection.activeObject.ToString();
            //     }
            // }

            EditorGUILayout.Space();

            // EditorGUILayout.SelectableLabel(VisitedAssetName, (GUIStyle)"TextFieldDropDownText");
            OnGUI_DropableVisitedLabel();
    
            // the tree view
            var rect = EditorGUILayout.GetControlRect(false, 184, GUILayout.ExpandHeight(true));
            TreeView.OnGUI(rect);
        }

        private void OnGUI_LowerPanel()
        {
            largeContentTextArea.OnGUI();
        
            // TODO YAML document up/down navigation
            // EditorGUILayout.BeginHorizontal();
            // if (GUILayout.Button("up", GUILayout.ExpandWidth(false))) {
            // }
            // if (GUILayout.Button("down", GUILayout.ExpandWidth(false))) {
            // }
            // EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            // Search
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search by fileID, guid, or fileID guid pair"); // 'Local Identifier In File'
            GUILayout.FlexibleSpace();
            selectAfterSearch = EditorGUILayout.ToggleLeft("and select it", selectAfterSearch, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            searchInput = EditorGUILayout.TextField(searchInput);
            if (EditorGUI.EndChangeCheck())
            {
                YamlObject searchFoundObj;
                infoAreaString[0] = Search(searchInput, out searchFoundObj);
                if ((searchFoundObj != null) && selectAfterSearch) {
                    TreeView.SelectAndRevealAndFrame(searchFoundObj);
                    largeContentTextArea.SetScrollPositionAt(searchFoundObj.LineNo);
                }
            }
            EditorGUI.indentLevel--;

            textAreaScrollPosition
                = EditorGUILayout.BeginScrollView(textAreaScrollPosition, // false, true, 
                                                  GUILayout.MinHeight(3 * EditorGUIUtility.singleLineHeight));
            EditorGUILayout.TextArea(infoAreaString[0], new GUIStyle(GUI.skin.textArea){wordWrap = true},
                                     GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
        }

        private void OnGUI_DropableVisitedLabel()
        {
            Rect drop_area = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
            EditorGUI.SelectableLabel(drop_area, VisitedAssetName, (GUIStyle)"TextFieldDropDownText");
            // GUI.Box(drop_area, "DropAreaGUITest");

            Event evt = Event.current;
            switch (evt.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                    {
                        break;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag();

                        // foreach (var obj in DragAndDrop.objectReferences) {
                        //     Debug.Log(" dragged_object: " + obj);
                        // }
                        // foreach (var path in DragAndDrop.paths) {
                        //     Debug.Log(" dragged_path: " + path);
                        // }

                        var objs = DragAndDrop.objectReferences;
                        var paths = DragAndDrop.paths;
                        if (0 < objs.Length)
                        {
                            LoadRelatedAsset(objs[0]);
                        }
                        else if (0 < paths.Length)
                        {
                            LoadAssetFile(paths[0]);
                        }
                    }
                    break;
            }
        }

        private void LoadRelatedAsset(UnityEngine.Object active)
        {
            var s = "Selected Object:\n";
            s += "  Name:" + active.name + "\n";
            var prefabParent = PrefabUtility.GetPrefabParent(active);
            s += "  PrefabParent:" + prefabParent + "\n";
            var loadFrom = prefabParent ?? active;
            var assetPath = AssetDatabase.GetAssetPath(loadFrom);
            s += "  AssetPath:'" + assetPath + "'\n";

            LoadAssetFile(assetPath, active, s);
        }

        private void LoadAssetFile(string assetPath, UnityEngine.Object relatedObj = null, string oplog = "")
        {
            var s = oplog;
            if ((assetPath != null) &&
                (assetPath.EndsWith(".prefab") // REFINE with Any
                 || assetPath.EndsWith(".unity")
                 || assetPath.EndsWith(".anim")
                 || assetPath.EndsWith(".controller")
                 || assetPath.EndsWith(".mat")
                 || assetPath.EndsWith(".renderTexture")
                 || assetPath.EndsWith(".asset")
                 ))
            {
                string filePath;
                if (System.IO.Path.IsPathRooted(assetPath))
                {
                    filePath = assetPath;
                }
                else
                {
                    filePath = Application.dataPath + "/../" + assetPath;
                }
                s += "\n";
                s += "FilePath to read:\n";
                s += "  '" + filePath + "'\n";

                AssetParser.ParseFile(filePath);
                s += "  Read " + AssetParser.Count + " objects.";

                TreeView.Reload();
                TreeView.CollapseAll();
                TreeView.searchString = "";

                VisitedAssetFilePath = filePath;
                VisitedAssetName = assetPath; // TODO last name part only is smart (?)
                if (relatedObj == null)
                {
                    VisitedObjectInstanceId = null;
                }
                else
                {
                    VisitedObjectInstanceId = relatedObj.GetInstanceID();
                }

                infoAreaString[0] = s;

                largeContentTextArea.Clear();
                largeContentTextArea.AddLines(AssetParser.GetAllYamlLines());
            }
        }


        // colon and (\d+) went worng. why?  new Regex(@"fileID: *(\d+).+guid: *([0-9a-f]{32})");
        static internal readonly Regex guid_fileID_pair_re = new Regex(@"fileID: *(-?[0-9]+).+guid: *([0-9a-f]{32})");
        static internal readonly Regex guid_re = new Regex(@"([0-9a-f]{32})");

        private string Search(string input, out YamlObject found)
        {
            found = null;

            Match match;
            if ((match = guid_fileID_pair_re.Match(input)).Success)
            {
                var fileID = long.Parse(match.Groups[1].Value);
                var guid = match.Groups[2].Value;
                return AssetInfoDatabase.NameOf(guid, fileID) + "\n(" + AssetDatabase.GUIDToAssetPath(guid) + ")";
            }

            if ((match = guid_re.Match(input)).Success)
            {
                var guid = match.Groups[1].Value;
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath != "")
                {
                    return "asset:" + assetPath;
                }
                else
                {
                    return "unknown asset in this project ?";
                }
            }

            // try LIIF
            long liifVal;
            try
            {
                liifVal = long.Parse(input);
            }
            catch (FormatException)
            {
                return "Illegal formated query string";
            }

            found = AssetParser.LiifToYamlObjectOrNull(liifVal);
            if (found == null)
            {
                return "Not found.";
            }
            else
            {
                return "local_obj:" + AssetParser.AbsolutePathOf(found);
            }
        }

    }


    public class YamlObjectsTreeViewItem : TreeViewItem
    {
        public YamlLocalId Liif {get;}

        public YamlObjectsTreeViewItem(int treeViewId, YamlLocalId liif) : base(treeViewId)
        {
            Liif = liif;
        }

        // for non YamlObject related node
        public YamlObjectsTreeViewItem() : base()
        {
        }
    }

    delegate void ObjectSelectedCallback(YamlObject obj);

    class YamlObjectsTreeView : TreeView
    {
        void DebugWarn(string message)
        {
            // Debug.LogWarning(message);
        }
    
        YamlAssetParser objs;
        ObjectSelectedCallback objectSelected;
    
        public YamlObjectsTreeView(TreeViewState treeViewState, YamlAssetParser objs, ObjectSelectedCallback callback)
            : base(treeViewState)
        {
            this.objs = objs;
            objectSelected = callback;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var items = new Dictionary<int, YamlObjectsTreeViewItem>();

            // 1. create TreeViewItem
            foreach (var keyval in objs)
            {
                YamlObject val = keyval.Value;

                var id = val.GetInstanceID();
                var item = new YamlObjectsTreeViewItem(id, val.Liif);
                items.Add(id, item);

                var s = "";
                if (val is YamlGameObject)
                {
                    var go = (YamlGameObject)val;
                    s += "'" + go.name + "'";
                }
                else if (val is YamlMonoBehaviour)
                {
                    var mb = (YamlMonoBehaviour)val;
                    var mbname = AssetInfoDatabase.NameOf(mb.Script_guid, mb.Script_fileID);
                    s += "MonoBehaviour '" + mbname + "'";
                }
                else
                {
                    var yo = (YamlObject)val;
                    s += yo.UnityTypeName;
                }
                s += "  line " + val.LineNo;
                item.displayName = s;

                item.icon = (Texture2D)EditorGUIUtility.ObjectContent(null, val.UnityType).image;
            }

            // 2. connect parent-child relation by finding parent from child
            var viewRoot = new YamlObjectsTreeViewItem{ id = 0, depth = -1, displayName = "TreeRoot" };
            var othersParent = new YamlObjectsTreeViewItem{ id = 1, depth = 0, displayName = "Others" };
            foreach (var item in items.Values)
            {
                var obj = objs[item.Liif];
                if (obj is YamlGameObject)
                {
                    var objTr = ((YamlGameObject)obj).transform;
                    if (objTr == null) // could be happen for "stripped" case // CHECK
                    {
                        othersParent.AddChild(item);
                    }
                    else
                    {
                        var parentTr = ((YamlGameObject)obj).transform.parent;
                        // parent transform is null for root GameObject of the Prefab
                        if (parentTr)
                        {
                            var parentItem = items[parentTr.gameObject.GetInstanceID()];
                            parentItem.AddChild(item);
                        }
                        else
                        {
                            // It is one of "root" in parsed objects.
                            viewRoot.AddChild(item);
                        }
                    }
                }
                else if (obj is YamlComponent)
                {
                    var go = ((YamlComponent)obj).gameObject;
                    if (go == null) // gameObject could be null for "stripped" case // CHECK really?
                    {
                        othersParent.AddChild(item);
                    }
                    else
                    {
                        var goItem = items[go.GetInstanceID()];
                        goItem.AddChild(item);
                    }
                }
                else
                {
                    othersParent.AddChild(item);
                }

            }

            // 3. adjust root things
            if ((othersParent.children != null)  // children could be null (!!)
                && (othersParent.children.Count != 0))
            {
                viewRoot.AddChild(othersParent);
            }
            if ((viewRoot.children == null) || (viewRoot.children.Count == 0))
            {
                viewRoot.AddChild(new YamlObjectsTreeViewItem{ id = 1, displayName = "Empty (Select a resource in Project or Hierarchy, and Press 'Load')"});
            }

            // 4. reorder amoung brothers
            ReorderAmoungBrothers(viewRoot);
            
            // 5. setup
            SetupDepthsFromParentsAndChildren(viewRoot);

            return viewRoot;
        }

        private void ReorderAmoungBrothers(TreeViewItem node)
        {
            if (node.children != null)
            {
                node.children.Sort(CompareYamlObjectsTreeViewItem);
                foreach (var child in node.children)
                {
                    ReorderAmoungBrothers(child);
                }
            }
        }
        
        private int CompareYamlObjectsTreeViewItem(TreeViewItem x, TreeViewItem y)
        {
            // Order GameObject children rule
            // 1. components first, GameObjects next.
            // 2. components order is same as in m_Component list in the GameObject.
            // 3. GameObjects order is determined by its Transform.RootOrder.

            var xi = ((YamlObjectsTreeViewItem)x).Liif;
            var yi = ((YamlObjectsTreeViewItem)y).Liif;

            // YamlObjects item first, pseudo item later
            if ((xi == null) && (yi != null))
            {
                return +1;
            }
            else if ((xi != null) && (yi == null))
            {
                return -1;
            }
            else if ((xi == null) && (yi == null))
            {
                return 0;
            }
            
            var xo = objs[xi];
            var yo = objs[yi];

            // Component first, GameObject later
            if ((xo is YamlGameObject) && (yo is YamlComponent))
            {
                return +1;
            }
            else if ((xo is YamlComponent) && (yo is YamlGameObject))
            {
                return -1;
            }
            else if ((xo is YamlComponent) && (yo is YamlComponent))
            {
                var go = ((YamlComponent)xo).gameObject;
                if (go == null)
                {
                    DebugWarn("go == null. xo.lineNo=" + xo.LineNo + "yo.lineNo=" + yo.LineNo);
                    return 0;
                }
                var xn = go.ComponentIds.IndexOf(xo.Liif);
                var yn = go.ComponentIds.IndexOf(yo.Liif);
                if ((xn == -1) || (yn == -1))
                {
                    // MEMO it seems relate stripped prefab
                    DebugWarn("unexpected. xn=" + xn + ", yn=" + yn + ", xo.Liif=" + xo.Liif + ", yo.Liif=" + yo.Liif);
                    return 0;
                }
                return xn - yn;
            }
            else if ((xo is YamlGameObject) && (yo is YamlGameObject))
            {
                var xg = (YamlGameObject)xo;
                var yg = (YamlGameObject)yo;
                var xt = xg.transform;
                var yt = yg.transform;
                if ((xt == null) || (yt == null))
                {
                    DebugWarn("null transform. xt=" + xt + ", yt=" + yt + "xo.lineNo=" + xo.LineNo + "yo.lineNo=" + yo.LineNo);
                    return 0;
                }
                return xt.RootOrder - yt.RootOrder;
            }
            else
            {
                // e.g. LightmapSettings, OcclusionCullingSettings in .scene file
                DebugWarn("other objects are equal. xi=" + xi + ", yi=" + yi);
                return 0;
            }
        }
        
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            // starting impl for single selection support
            if (selectedIds.Count < 1)
            {
                return;
            }

            int target = selectedIds[0];
            var item = (YamlObjectsTreeViewItem)FindItem(target, this.rootItem);

            if (item.Liif != null)  // could be null for non-YamlObject-related item ("Empty", "Others")
            {
                var yamlObj = objs[item.Liif];
                objectSelected(yamlObj);
            }
        }

        public void SelectAndRevealAndFrame(YamlObject obj)
        {
            SetSelection(new List<int> {obj.GetInstanceID()}, UnityEditor.IMGUI.Controls.TreeViewSelectionOptions.RevealAndFrame);
        }
    }
}
