
### Spike_VRC_UiShape

- VRC_UiShape を Canvas に付加することで操作できるようになる。
    - VR モードでも、デスクトップ モードでも操作可能
- VRC_UiShape を付加していなくても Canvas の表示はされる
- ScreenSpace - Camera な Canvas も使える
    - この場合 Canvas 以下の構造がプレイヤーの視線の先に常時位置することになる


#### WorldSpace な Canvas

WorldSpace な Canvasに VRC_UiShape を付加し、レイヤーを UI 以外にすると、
通常のクイックメニューのようにレイキャストが可視化されたものがアバターの手から出て、それで Button を押せる。
通常、VRChat のクイックメニューはメニュー操作した側の手に現れ反対側の手のレイキャストで操作するが、
これがクイックメニューを消してもあたかも“手に残るように”動作する。
ただしレイは Canvas に当たっている間のみ可視化される（デスクトップではちょうど、これが青白いもやもやの表現になっていると思われる）。
なお起動直後は右手でのレイキャストが有効になっている。

レイヤーは Collision Matrix で何も衝突しないようなものにしても Default と同様に動作しました。
つまり「UI」が例外的に「メニュー表示中にのみ反応し、そうでなければ邪魔にならない」という扱いになっているように思えます。

なお WorldSpace Canvas でも VRC_UiShape は必要なので「VRC_UiShapeはCanvasのカメラのつじつま合わせしているだけ」ではない。

VRC_UiShape は disenable にしていても上記を動作する。
これには実は機能は無くてマーカー（それが付いているオブジェクトを対象物として認識する）なのかもしれません。

UI部品を見つける機構はEventSystemのInput Moduleと呼ばれる部分のようです。
参考： http://isemito.hatenablog.com/entry/2016/12/18/000000


#### ScreenSpace - Camera な Canvas

- Screen Space-Camera にした Canvas において VRC_UiShape を付加することで、
  プレイヤーの視点である Camera と Canvas の関係が適切に確立される。
- 応用：Canvas は通常 2D な UI を表現する機構であるが、結果的に Canvas 以下の構造を 3D 形状のまま、
  プレイヤー視点前方指定距離に常に位置させられる。

- Camera は指定しなくて（None で）もよい。（指定するように Unity の警告は出るが）


https://twitter.com/zi_zi_neet/status/1002637390932873216
> BoxColliderが勝手につくみたいで、それを防止するためにdisableのBoxColliderをつけておくといいらしいです 
通常の UI としてのどうさを行わせないための話か？

https://twitter.com/naqtn/status/1002681161968046081
> Plane Distanceが0になっているのが解せないのですがこれは？ 
> 0にするといいらしいです(理由までは聞けてない) 

https://twitter.com/zi_zi_neet/status/1001846340161191943
「リファレンスカメラをオフにして」というのはなぜ？必須ではなさそうだが。
