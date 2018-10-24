# iwsd Common prefabs

## Prefabs

### IndicatorLamp

### MasterLamp

A gimmick activated only in master's world automatically.

- If you are master of this instance, red cube appears.

- マスターの世界だけで自動的に有効化するギミックの例
- 自分がマスターであると赤いキューブがあわらわれる


#### detail
- This uses OnEnable and OnPlayerLeft triggers.
- In an instance, only one player becomes the master, then it's possible to make a mechanism collecting informations and deciding what happens.

- OnEnable と OnPlayerLeft トリガーを使用している
- マスターはインスタンスに一人だけなので、
  「インスタンスに常に一つだけあって、集約的に条件判断する装置」
  といったようなものが作れる。


  
### MasterLocalGate

- _Callback に書くよ

- broadcast type, Delay の設定は重要なので誤って変えないでね
AdvanceMode のチェックを誤ってさわるだけで壊れるから注意してね

- 別のオブジェクトの Custom に書いた方がお勧めだ


- OnInteract のように特定クライアントで起こるものの場合

- Custom の _Callback に処理内容を書く
あ。これ、オブジェクトを分離したほうがいいな。

- uGUI 版もある。

- ObjectActive 版を作ろう

> AnimationBoolかSetGameObjectActive


- 0.000184
The value itself is not important.
Small plus non-zero value is the point.

(If you set Delay to zero, this gimmick doesn't work properlly.
it seems that custom trigger with broadcast Master executes on non-master client.
)


NonmasterLocal
 non-master の元で発火して、自身にのみアクションが実行される
である
Master を字句的に置き換えた
non-master でのみ発火して、そのアクションは全てのクライアントで実行される
ではない。


_Callback_MasterLocal
_Callback_NonmasterLocal






## Thanks to

hatsuca.
Cyan Laser


<!-- ================================================================================ -->

## Samples

### Master Lamp

This cube becomes red if you are the master.

- simple example use of MasterLocalGate prefab
- OnPlayerLeft



## リリースへ向けて

- ドキュメント作成
- 記述の動作確認
- ライセンス文整備

- 完成版ワールドアップロード、 world ID は一次的にデタッチ
- パッケージアップロード
- 展開して動作確認
    - local, upload
- public 申請

- blog 記事公開
- Twitter, Discord 投稿



