## リリースへ向けて

- ドキュメント作成
- 記述の動作確認
- ライセンス文整備

- 完成版ワールドアップロード、 
- パッケージアップロード
    - world ID は一次的にデタッチ
    - パッケージ作成
- public 申請

- blog 記事公開
- Twitter, Discord 投稿

- Canny に不具合報告


# とりあえずめも

- 中央視野と周辺視野の二枚使ったらどうだろう？

- 回転方向が逆じゃないか疑惑

- perspective repro コード整理
    - X 方向で作っていた昔のコードを削除
    - 関数名を整える

- wide FOV のステレオ化

- カメラに HDR のスイッチあるやんけ
> Enables High Dynamic Range rendering for this camera.

- spot black の階調が足りないように見えるのは HDR で解消するのか？

- カメラの culling mask の調整
    - six sidex 
	- perspective
	- 

- サンプルシーンでのカメラのデプス調整
    - ドキュメントで言及


- TODO マテリアル作り直し
属性名を名前を変えてきたので古いのが残ってしまっている。
作り直すか、古いのを削る。一つやれば古いの分かる？


- オンラインリアルタイム生成の画像を何かロードするようにしたい
本日の地球の天候を表示したい
リアルタイム画像アクセスして問題ない所はないだろうか？

リアルタイムの地球の雲合成
https://static.die.net/earth/rectangular/1600.jpg
趣味レベル。こいつをキャッシュしてやればいいんだろうが…。



- cam center front cull と back cull を張り合わせて使って、
中に入ると違う世界、
みたいなのはなにかできないかな。

- カメラの enable コントロールで、止まっている世界動いている世界が表現できる。
出入りと共に何かできないかな。
（jiritsu イワシアイ）



- 実写サンプル？


- VRC_Panorama によるローディングタイミングについて調査
- URL リロードの方法はあるのか？
コンポーネント toggle ?
GameObject toggle ?
- プロトコル（HTTP, HTTPS）の確認


- 6side にワールド内テレポートをくっつけた例を作成
cull front の sphere でいいかな？


- LiveSkybox のデモで large object をオミットするように
    - 必要ない？


- OnEnable で mesh on の tips を書く。
編集に邪魔だったら、の話。
サンプルワールドではうんぬん


- ワールド依存の pickupable を追加する
ObjectDirectionWeight = 0


- Exit 表示が見にくいのでピクト を探してくる


- MasterLocal の仕組みを導入してちらつきをなくす
- non-master で切り替えられなくなってる？

- 結局 object sync は必要なのだろうか？
いまはプレハブに入っている。
入れたままにするなら、注意書きをする。
VRC_Panorama component は require はしていない。要らないとみるべき？
お？
ビルドが警告を出しやがる
"{object name} has a VRC_DataStorage and VRC_ObjectSync, with SynchronizePhysics enabled."
ほほう。VRC_DataStorage が依存という形なのか。




- VR で見た時かなりきつい。というか飛び出しすぎ。
撮影時の目の距離と違うから、かな？

- 飛び出し感をあとからマイルドにする方法ってあるんだろうか??
近く感じて辛いのだから、少し視線を外側に外せばいいのか？
遠距離で見えていたところがきびしくなるわけだが..？

- onModel を VR で見た時に、modelの視差とどうしても合わない。
ふちをぼんやりさせるオプション？
normal を見て視線方向との角度がきつかったら Alpha で溶かす？

- both side のステレオは、裏に行くとでっぱりへっこみが逆になる。
ある意味正しいが、実用的にはどうなんじゃろ？


- Unity 2017.3. に PanoramicShader が入ったらしい
https://github.com/Unity-Technologies/SkyboxPanoramicShader
https://docs.google.com/document/d/1JjOQ0dXTYPFwg6eSOlIAdqyPo6QMLqh-PETwxf8ZVD8/edit

 "Skybox/PanoramicBeta"
用語は同じにしておきたい
  3D Layout: Side-by-Side or Over-Under
  Mapping: Latitude Longitude Layout

 sampler2D _Tex;
 float4 _Tex_TexelSize;
 half4 _Tex_HDR; これなに？
 half4 tex = tex2D (_Tex, tc);
half3 c = DecodeHDR (tex, _Tex_HDR);

https://docs.unity3d.com/jp/current/Manual/SL-PropertiesInPrograms.html
> テクスチャ HDR パラメーター
> {TextureName}_HDR - float4 プロパティー。
> 使用されているカラー空間によっては、HDR 可能な (例えば RGBM エンコード) テクスチャにデコードする方法についての情報を持つ。
> UnityCG.cginc シェーダー include ファイルの DecodeHDR 関数を参照してください。

> [Gamma] 属性を加えて、それが sRGB 空間に指定されていることを示すことが可能

https://www.eizo.co.jp/eizolibrary/color_management/hdr/index.html
> HDRとは、High Dynamic Range（ハイダイナミックレンジ）の略称で、
> 従来のSDR（スタンダードダイナミックレンジ）に比べてより広い明るさの幅（ダイナミックレンジ）を表現できる表示技術です



# 同期問題ふたたび

- 同期問題
    - always unbufferd にしてあるが、sync 入れているのならもしかして local でいいのか？
    - ⇒どうも sync 関係ない。non master だと奇妙。 master local が必要なケース。
	- どうしようもないので
	  non-master で切り替え時にちらつくことがある。
	  とドキュメント？
- hatsuca master private みてみたが、解せぬ


de-sync, you again!

- master unbuffered にして late joiner に同期した
    - つまりバッファで同期しているのではない
- Local の場合 non-master で、当初は操作できない
    - 抜けるきっかけは不明
- non-master で操作できるようになった後、
  local では操作は出来るが伝播しない。
- master の操作は local であっても伝播して同期する

ということは master local が望まれるパターン。

多少ぎこちなくなるが、AlwaysUnbuffer で乗り切るしかない
だろう？だよね？


# シェーダー実装まわり技術面

- Shader Tag
- αの扱い これはバリアントとして作れないのか？
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
組み込めるのならばサンプル

RenderType: Opaque, Transparent

https://forum.unity.com/threads/opaque-version-and-transparent-version-shader-in-one-shader-file.364436/
https://forum.unity.com/threads/surface-shader-multi-compile-with-different-transparency-parameters.469173/
http://tips.hecomi.com/entry/2017/10/24/014057

設定の GUI を custom editor として作る。 Standard Shader を参照
あるいは共通をくくりだして手で書くか
Standard を見てみると Rendering Mode を変えると  Render Queue が変わるな。

- check shader Tag, esp  "Queue".  "Geometry"?

- alpha は limitation として書いておけばいいか



- 関数部分を別ファイルにして inlcude
Panorama と Skybox の両シェーダでと共通化

・ガンマおかしい疑惑
デガンマ実装した。よいのかな？
Panorama の方にも反映

取り込み画像で panorama で degamma のサンプル（およびマテリアル）を追加


# Canny

### Renderer が任意のものがさせない。自身じゃないと動かない。プレハブ壊れてる



### Worng UV calculation in VRChat/Panosphere shader for monoral image

If you use VRChat/Panosphere shader with "Stereo Enabled" checked off, you'll get twice rendererd image for U-axis (which is usually horizonal axis). This is caused by worng calculation in the shader.


VRChat/Panosphere shader is "VRCSDK/Examples/Sample Assets/PanoViewer/Panosphere.shader".
In its MonoPanoProjection function, it calcurate UV coordinates as: 
>  float2 sphereCoords = float2(longitude, latitude) * float2(1.0/UNITY_PI, 1.0/UNITY_PI);
>  sphereCoords = float2(1.0,1.0) - sphereCoords;
>  return (sphereCoords + float4(0, 1-unity_StereoEyeIndex,1,1.0).xy) * float4(0, 1-unity_StereoEyeIndex,1,1.0).zw;

I think this should be:
>  float2 sphereCoords = float2(longitude, latitude) * float2(0.5/UNITY_PI, 1.0/UNITY_PI);
>  sphereCoords = float2(0.5,1.0) - sphereCoords;
>  return sphereCoords;

* The return value range of atan2 function is [-PI,PI]. So coefficient converting to U-axis is 1/(2*PI) 
* The shift value from [-0.5,0.5] to [0,1.0] is 0.5 . Not 1.0
* This function is for monoral image case. There's no difference between left and right eyes. It's nonsense to calcurate with unity_StereoEyeIndex.




# NOW

- カメラのトグル制御作ろう
- トグルボタンを common に作ろう

- 振動アニメ
> 安静時の健康な成人の平均的な呼吸数は、毎分12～20回
(/ 60.0 12)5.0
(/ 60.0 20)3.0

- クリップの長さかえるのどうやるん？


