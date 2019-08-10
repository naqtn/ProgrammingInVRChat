# Iwsd / Sub Inspector

VRChat ワールド作成向けに、イベント処理定義の順番を変えられる UI などを提供するツール。


## 使い方
1. パッケージをインポートする
2. ユニティのメニュー Window > VRC_Iwsd > "Open Sub Inspector" から SubInspector ビューを開く
3. 通常通り、ヒエラルキービューで編集するオブジェクトを選択する
4. SubInspector ビューに、オブジェクトに付随したコンポーネントに応じて現れた UI を利用する


## 機能

- リストの並べ替え。対応コンポーネントおよびリスト：
    - VRC_Trigger  Triggers
    - Button  OnClick
    - InputField  OnValueChanged
- オブジェクトのパスの表示
    - コピー可能。編集する機能は今のところ実装されていない。
- VRC_Trigger のトリガー定義のコピー＆ペースト
    - コピーは対象を選択して「Copy to clipboard」ボタンを押す
    - データはクリップボードにテキストで格納される。
    - ペーストは「Paste from to clipboard」ボタンを押す。クリップボードのデータが最後に追加される。
    - プロジェクトを超えたペーストも可能である。ただし GameObject の部分は None になるので、ペーストされた内容を精査する必要がある。
    （たまたま instanceID が合致して None にならない場合もあるかもしれない）

## Licence etc.

MIT licence
by naqtn (https://twitter.com/naqtn)
Hosted at https://github.com/naqtn/ProgrammingInVRChat

This software includes SimpleJSON (https://github.com/Bunny83/SimpleJSON) licenced under MIT licence.

---
end
