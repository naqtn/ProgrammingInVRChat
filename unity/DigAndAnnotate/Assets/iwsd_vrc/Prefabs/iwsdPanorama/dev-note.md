
# ワールド情報

wrld_d4f0b670-d502-4f44-afb0-88a1ac2924fb
vrchat://launch?id=wrld_d4f0b670-d502-4f44-afb0-88a1ac2924fb:184~public
https://www.vrchat.net/launch?worldId=wrld_d4f0b670-d502-4f44-afb0-88a1ac2924fb&instanceId=184~public


Currently I'm working on this prefab and its demo world: https://www.vrchat.net/launch?worldId=wrld_d4f0b670-d502-4f44-afb0-88a1ac2924fb&instanceId=184~public Though more work is needed to distribute (suppressing flickers, rearrange asset names, documentation etc.), basically I think it's completed as working examples. Any feedback is welcome.

今取り組んでいるパノラマ関連プレハブのデモワールド https://www.vrchat.net/launch?worldId=wrld_d4f0b670-d502-4f44-afb0-88a1ac2924fb&instanceId=184~public 配布するにはまだ作業が必要なのですが（チラつき抑制、文書作成など）使用例としてはだいたいそろったと思うのでワールドIDを公開してみます。不具合など有りましたら教えていただくと幸い。



# 参考文献、Web リソース

- https://wiki.panotools.org/Panorama_formats
- https://docs.unity3d.com/2018.1/Documentation/ScriptReference/MaterialPropertyDrawer.html
- https://qiita.com/Fuji0k0/items/f5e5d3651391f9b16327


### パノラマ画像を skybox に適用する方法
http://simplifyvr.net/documentation/rendering-a-360-panorama-as-a-skybox-background-in-unity

### skybox shader
[【Unity】Skyboxシェーダーの書き方](https://qiita.com/aa_debdeb/items/2739745b3ab1a003d767)
[Cg Programming/Unity/Skyboxes](https://en.wikibooks.org/wiki/Cg_Programming/Unity/Skyboxes)
[Simple procedural skybox with a moon](https://medium.com/@cyrilltoboe/simplest-gradient-skybox-with-moon-8581610c86c0)



Shader Properties に書ける
プロパティの項目のマテリアルの編集 GUI としてあらわれるクラス
https://docs.unity3d.com/ja/current/ScriptReference/MaterialPropertyDrawer.html
Toggle, Enum, KeywordEnum, PowerSlider, IntRange, Space, Header 


さらに複雑な設定をしたい場合は、全体を書く
https://docs.unity3d.com/ja/current/Manual/SL-CustomShaderGUI.html
シェーダ側には CustomEditor "{class name}" を書く


https://vrchat.com/home/launch?worldId=wrld_d4f0b670-d502-4f44-afb0-88a1ac2924fb&instanceId=42~private(f7dd441f-fe09-47ab-83d9-8713e86ace77)

"vrchat://launch/?ref=vrchat.com&id=wrld_d4f0b670-d502-4f44-afb0-88a1ac2924fb:184"
https://vrchat.com/home/launch?worldId=wrld_d4f0b670-d502-4f44-afb0-88a1ac2924fb&instanceId=184

MEMO rotation with quaternion in a shader

Shader functions to facilitate rotation of vertex around point with a quaternion (Unity / HLSL / Cg) 
 https://gist.github.com/patricknelson/f4dcaedda9eea5f5cf2c359f68aa35fd

How to Rotate a Vertex by a Quaternion in GLSL
 http://www.geeks3d.com/20141201/how-to-rotate-a-vertex-by-a-quaternion-in-glsl/

Euler angles (z-x-z extrinsic) → quaternion
 https://en.wikipedia.org/wiki/Rotation_formalisms_in_three_dimensions#Euler_angles_(z-x-z_extrinsic)_%E2%86%92_quaternion


Camera の出力先が RenderTexture の場合 Viewport Rect は無効。全域が描画される orz
https://docs.unity3d.com/ScriptReference/Camera-targetTexture.html
> When rendering into a texture, the camera always renders into the whole texture



Cg/HLSL



Perspective distortion
https://en.wikipedia.org/wiki/Perspective_distortion_(photography)
射影ひずみ
これかな？
透視投影（とうしとうえい、英: perspective projection）

https://ja.wikipedia.org/wiki/%E9%AD%9A%E7%9C%BC%E3%83%AC%E3%83%B3%E3%82%BA
> 魚眼レンズ
> ほとんどの魚眼レンズは、画面の中心からの距離と角度が比例する等距離射影方式（Equidistance Projection ）を採用している。天体位置測定[1]や雲量測定[1]に使用できる。

perspective projection による Perspective distortion という言い方でいいのかな？
透視投影による射影ひずみ
Perspective distortion by perspective projection

専用の項目合った
https://en.wikipedia.org/wiki/Perspective_projection_distortion


魚眼レンズの説明＆光路図 http://www.pierretoscani.com/fisheyes-(in-english).html 良く分かんないけどなんかすごいぞ

wide angle camera FOV

A shader that removes perspective distortion caused by perspective projection

I guess my shader is an equivalent to well-known one. But I can't google that name. "A shader correcting perspective distortion caused by perspective projection", "repoject perspective projection" hmm, not found.




全球をうまい事、歪感少なく表示する
https://github.com/shaunlebron/blinky
> Blinky
> Proof of concept to put peripheral vision into games (without VR goggles).
> Explore this interesting space by playing the Quake demo with fisheyes, panoramas, and cartographic projections.

http://shaunlebron.github.io/visualizing-projections/
> Visualizing Projections
射影する二次元に落として分かりやすくインタラクティブに説明


http://blog.yeezia.com/?post=91
> Converting a fisheye image into a panoramic or perspective projection


# toggle switch

UI だけからなら OnInteract をもつオブジェクトを二つ用意して、
それぞれ互いに SetGameObjectActive するだけでよい。
OnInteract を AlwaysBufferOne にすれば late joiner にも対応できる。


これをプログラムからも呼べるようにするには Delay の延滞を使う。 ← だよな？
切り替えるところを Custom に分離して Delay を付ける。
また、 SetComponentActive もよぶ。← だよな？

OnInteract は local にして共通 Custom trigger を呼ぶ。
共通 Custom trigger を AlwaysBufferOne にする。
プログラムインタフェースは、それを local から呼ぶ。
（On/Off 両方のそれを連続して呼ぶ。どちらかが動く）

こうなると、そもそも UI をごそっと切り替える必要なないのでは、という感じ。






## ログ

-  [VRC_TriggerInternal] 24.10798 Cube via OnInteract executing [ActivateCustomTrigger (False, 0.000, 0, "iwsdToggleSwitchWithLamp_ProgCallable", Toggle, 0)]
おしました
-  [VRC_EventDispatcherRFC] ActivateCustomTrigger:Toggle on (iwsdToggleSwitchWithLamp_ProgCallable)
カスタムへ

-  [VRC_TriggerInternal] 24.10798 iwsdToggleSwitchWithLamp_ProgCallable via Custom executing [ActivateCustomTrigger (False, 0.000, 0, "OffStateUI:OnStateUI", _Toggle, 0)]
-  [VRC_EventDispatcherRFC] ActivateCustomTrigger:_Toggle on (OffStateUI)
OffStateUI の _Toggle を実行


-  [VRC_TriggerInternal] 24.10798 OffStateUI via Custom executing [ActivateCustomTrigger (False, 0.000, 0, "iwsdToggleSwitchWithLamp_ProgCallable", OnActivate, 0)]
-  [VRC_EventDispatcherRFC] ActivateCustomTrigger:OnActivate on (iwsdToggleSwitchWithLamp_ProgCallable)
-  [VRC_TriggerInternal] 24.10798 iwsdToggleSwitchWithLamp_ProgCallable via Custom executing [PlayAnimation (False, 0.000, 0, "AnimHolder", iwsdSimpleRegacy_RotateY_Loop_5sec, 0)]
-  [VRC_EventDispatcherRFC] PlayAnimation:iwsdSimpleRegacy_RotateY_Loop_5sec on (AnimHolder)
アクションの実行（PlayAnimation）

-  [VRC_TriggerInternal] 24.10798 OffStateUI via Custom executing [ActivateCustomTrigger (False, 0.000, 0, "OffStateUI", _Alternate, 0)]
-  [VRC_EventDispatcherRFC] ActivateCustomTrigger:_Alternate on (OffStateUI)
_Alternate の実行（Delay がかかる）

-  [VRC_EventDispatcherRFC] ActivateCustomTrigger:_Toggle on (OnStateUI)
OnStateUI の _Toggle は、からぶり

_Alternate の実行
-  [VRC_TriggerInternal] 24.11901 OffStateUI via Custom executing [SetGameObjectActive (True, 0.000, 0, "OnStateUI", , 0)]
-  [VRC_EventDispatcherRFC] SetGameObjectActive:True on (OnStateUI)
-  [VRC_TriggerInternal] 24.11901 OffStateUI via Custom executing [SetComponentActive (True, 0.000, 0, "OnStateUI", VRCSDK2.VRC_Trigger, 0)]
-  [VRC_EventDispatcherRFC] SetComponentActive:VRCSDK2.VRC_Trigger on (OnStateUI) to True
-  [VRC_TriggerInternal] 24.11901 OffStateUI via Custom executing [SetComponentActive (False, 0.000, 0, "OffStateUI", VRCSDK2.VRC_Trigger, 0)]
-  [VRC_EventDispatcherRFC] SetComponentActive:VRCSDK2.VRC_Trigger on (OffStateUI) to False
-  [VRC_TriggerInternal] 24.11901 OffStateUI via Custom executing [SetGameObjectActive (False, 0.000, 0, "OffStateUI", , 0)]
-  [VRC_EventDispatcherRFC] SetGameObjectActive:False on (OffStateUI)




iwsdSimpleRegacy_RotateY_Loop_5sec
iwsdSimpleRegacy_BreathingScale_Loop_5sec


always で配っている中で PlayAnimation だと綺麗に再生されない。
でもなんで、理由が思いつかない
追試。
途中で止まってしまうんですよね？





