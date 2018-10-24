# 修復した VRCPanorama プレハブ

<!--
repaired
fixed
VRCPanorama

-->


(TODO ver 記載)現在 VRCPanorama が壊れているので、動く様に再構築した。

これは、シンプルなパノラマ画像プレイヤーである。

あらかじめ用意したパノラマ画像
（いわゆる360度パノラマ画像
ある場所からの全ての方向
全球の映像）
を表示する機能。
それらを切り替える機能を持つ。


## プレハブの使い方

- パノラマ画像を用意する
    - アセットとして取り込みテクスチャにする
    - あるいは Internet において HTTP アクセスできるようにする
- シーンにプレハブを置く
- VRC_Panorama の xxx に画像を登録する
- ひとつの場合はこれでおしまい

- 切り替えのつくりかた
- CustomMethod を定義しているので呼び出す
    - NextPano
    - PrevPano
    - panoAt(int)
        - 先頭の場合 0 、次は 1

- 同期される
    - おそらく VRC_DataStorage と VRC_ObjectSync の組み合わせによる作用。
    - CHECK 逆に取り除いたら個別表示できるのか？VRC_ObjectSync  が必要であることを動作確認

- すべてのプレイヤーに同じ画像を見せる

- 注意事項
FAQ model weight を使う場合は、 batching static をはずせ。さもないとテクスチャのおかしなところを投影したように見える。


## プレハブの仕様補足
<!--
使い方のうち advanced な話題に分解はこっちに
-->


- OnDataStorageChange 切り替わったことの反応
    - CHECK 複数の VRC_Panorama でちゃんと動く？

- mesh は任意のものに変えて良い
- 裏側に描画するようになっている
    - デフォルトの 3D Object を配置して内側から見る、ことを想定している
    - 外から見たい場合は、添付しているシェーダーの Cull Front を変える
        - CHECK 書き換えて動作確認。具体値をここに記載。両面描画のチェック

- 画像は Latitude/Longitude 形式
    - VRChat 内 Panorama ボタンで撮ったものがそのまま使える
- 全て同じである必要がある
- material のオプションで切り替えられる
    - TODO monoral 版の material を作成


- メッシュが編集時に邪魔なので disenable 状態であることを許している
OnEnable, OnDisable で変更している
MeshRenderer コンポーネントを

- unlit である。ライティングやシャドウを使っていない。
マテリアルを設定している。

- Tiling を設定することで、一枚のテクスチャに複数の画像を納めることができる。

- panoAt(int) 0 以外が必要な場合
Button の OnClick() の設定の
ShowPanoAt 関数呼び出しの下の入力欄を変更の事




## 添付のサンプルシーンについて
- OnDataStorageChange で描画する
- 組み込みの

- 最初はSixSidedはoffにしている。
パフォーマンスの問題
Always で変えているが、実用的には Local を検討すべき。 
いわゆる local mirror と同じ話


# 技術資料

## シェーダーの仕様
- lat lon 形式
- ステレオは above below 方式モノラルの場合
- TODO モノラルの動作確認
- TODO HMD でステレオの確認

- 裏面を描画している
- オブジェクトにパノラマテクスチャが固定的に張り付いているのではなくて、
- 視線のワールド座標での角度のみが使われる
- オブジェクトとの位置関係は（裏面が見えている）事以外使ってない
- プレイヤーが移動しても、常にパノラマ映像を取った時の視点位置に居るように見える




## VRCPanorama プレハブ および VRC_Panorama コンポーネントが壊れているポイント

- VRC_Panorama が持つテクスチャの設定先 renerer フィールドの値は、
  その VRC_Panorama の兄弟でないと、現状では VRC_Panorama が動かない。
    - VRCPanorama プレハブでは、子である Sphere になっているので動かない。
- VRC_Trigger から NextPano よび PrevPano の RPC を中途半端に呼び出そうと書かれているが、
  NextPano よび PrevPano は RPC になっていないので SendRPC アクションでは呼び出せない。
    - 関数として存在はしているが RPC アトリビュートが設定されていない。
    - ドキュメントに記載のある ShowPanoAt RPC も同様
- ドキュメントにある以下の項目が実際には存在していない
    - Renderer Left, Renderer Right は現在の Single Pass Stereo Rendering においては意味がないので
      無効になったものが放置されていると思われる
    - Layout は存在しない。相当する内容はシェーダにおいては StereoEnabled のトグルになっている。
      （現状ではつまり、画像ごとに指定する機能を持たずマテリアルで共通に設定する）
- シェーダ Panosphere とマテリアル Sphere （いずれも VRCSDK/Examples/Sample Assets）は
  全体として座標計算が間違っていて、歪んだ表示になる
    - マテリアルは MainTex の Tiling と Offset の Y に 0.5 が指定されていて、
      片目分の領域を指定したいのかもしれないが、
      シェーダは与えられたテクスチャの領域の中を自分で割って片目分を求めている。
    - シェーダは stereo disabled で u の計算中の係数が間違っているので二周してしまっている。
- シェーダ Panosphere の実装ではテクスチャの切れ目が見えることがある


オリジナル VRCPanorama を使おうとするとどうなるんだっけ？
⇒そもそも Sphere オブジェクトが子供なので VRC_Panorama は自分自身にしか設定できない
⇒変更：MeshFilter と MeshRenderer をコピーVRC_Panorama.Renderer を自身に変更（scale は適当に）
⇒一応表示される。ただしu方向に二周している。これはシェーダのプログラムの間違い。
初期状態で 
タイリングとオフセットは StereoEnabled=false, tiling Y=0.5, Offset Y=0.5 で
片側イメージだけを使っている、ので正しいのか？
CHECK オリジナルシェーダのコードを見て確認。


## VRC_Panorama 仕様解析

- VRC_Panorama が持つテクスチャの設定先 Renderer フィールドの値は、
  その VRC_Panorama の兄弟でなければならない。
    - None は省略値として自身のオブジェクトを指すようなので、現状では None にしておくのが良い。
        - （Unity エディタ上で実行すると値が入るのが観察される）

- VRC_Panorama の Panoramas の要素は、Url 文字列と Texture からなる
    - Texture に直接アセットへの参照を入れるか URL を指定する
    - Url に URL 文字列を入れた場合は、動的ロードしてきて Texture に設定される
         - CHECK 条件は文字列？
- "Missing (Texture 2D)" になっているのは、一度設定した後に Url 文字列を空にした場合

- Texture が入っていればそれがつかわれる。（Url は無視される）


- 現在位置を変更する関数
    - NextPano, PrevPano, ShowPanoAt(int) 
    - RPC にはなっていないので、uGUI などを使用して呼び出す必要がある。
    - ShowPanoAt(int) はセット
    - VRC_DataStorage は駆動される
        - OnDataStorageChange は発生する

- ロードは切り替えるタイミング？

- DataStorage の Data のサイズがゼロだと、Update で毎回例外が発生。ログがあふれる。





# Licence
MIT

Degamma UUUPA



# Thanks to
https://github.com/makitsune/realtime-clock




## ドキュメントネタ

- マテリアルは一個しか指定できない。
これは VRC_Panorama の制限事項とおもわれ。
と、ドキュメント

- on model の時は batching static を無効にすること。
と注意書きを書く


- 「prefab のテクスチャは切り替えられる」「けど、マテリアルは一つしかない」
つまりパラメタは同じでないといけない。
ステレオとモノを混ぜるとか、
と limitation を記述

アニメータで頑張る手はないわけではないが、メンテナンス大変だろう。


- mplus font の表記
- 画像部分は MIT から外すと宣言
- spot model



