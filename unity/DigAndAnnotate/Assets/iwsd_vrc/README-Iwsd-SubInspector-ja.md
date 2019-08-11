# Iwsd / Sub-Inspector

VRChat ワールド作成向けに、イベント処理定義の順番を変えられる UI などを提供するツール。

![SubInspectorUI](doc/sub-inspector.PNG)

インスペクタ（画像右側）が表示しているのと同じオブジェクトについて、
インスペクタでの編集を補助する UI を Sub Inspector（画像左側）は提供する。


## 使い方
1. パッケージをインポートする
2. ユニティのメニュー Window > VRC_Iwsd > "Open Sub Inspector" から SubInspector ビューを開く
3. 通常通り、ヒエラルキービューで編集するオブジェクトを選択する
4. SubInspector ビューに、オブジェクトに付随したコンポーネントに応じて現れた UI を利用する


## 機能

### リストの並べ替え

以下のコンポーネントにおけるリストの編集を提供する。 

| Component     | list                             |
|---------------|----------------------------------|
| `VRC_Trigger` | `Triggers`                       |
| `Button`      | `OnClick`                        |
| `InputField`  | `OnValueChanged` and `OnEndEdit` |


補足：
`Button` と `InputField` に関しては、
Merlin-san が作成された  [EasyEventEditor](https://github.com/Merlin-san/EasyEventEditor)
というエディタ拡張もあります。


### VRC_Trigger のトリガー定義のコピー＆ペースト

VRC_Trigger に設定した内容の一部指定して、他の VRC_Trigger へコピー＆ペーストする機能。

- コピー操作
    1. トリガー定義の中からコピーしたい部分を選択する
    2. 「Copy to clipboard」ボタンを押す
    - データはクリップボードにテキストで格納される。（技術的にはとあるフォーマットの JSON になる）
- ペースト操作
    1. コピー先の GameObject を選択する。まだ VRC_Trigger が無いのであれば追加する
    2.「Paste from to clipboard」ボタンを押す。
    - リストの最後にクリップボードのデータが追加される

補足：
プロジェクトを超えたペーストも可能である。ただし GameObject の部分は None になる。
（たまたま instanceID が合致して None にならない場合もあるかもしれない。その場合には不適切なものを指すことになる）
いずれにしてもペーストされた内容を精査する必要がある。

### Inspector による編集保護

GameObject やコンポーネントをインスペクタで編集できないようにする機能。
望まない編集や誤操作を防ぐもの。
インスペクタにのみ影響する。シーンビューなど他のビューでは編集できる。

補足：
技術的には [HideFlags](https://docs.unity3d.com/ScriptReference/HideFlags.html) の NotEditable マスクを変更している。


### オブジェクトのパスの表示

シーンのルートからのオブジェクトのパスを表示する機能。

コピー可能。編集する機能は今のところ実装されていない。

補足：
アニメーションクリップを直接編集するときなどに便利。


## Licence etc.

MIT licence
by naqtn (https://twitter.com/naqtn)
Hosted at https://github.com/naqtn/ProgrammingInVRChat

This software includes SimpleJSON (https://github.com/Bunny83/SimpleJSON) licenced under MIT licence.

---
end
