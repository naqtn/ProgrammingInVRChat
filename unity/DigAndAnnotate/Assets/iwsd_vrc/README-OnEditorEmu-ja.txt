# OnEditorEmu: On Unity Editor VRChat Client Emulator


## これは何？

これは、Unity エディタ上で動く VRChat クライアントのエミュレータです。
デバッグ作業を改善することを目的としています。
Unity エディタの機能を使って VRChat ワールドのデバッグが行えるようになります。


例えばワールドに凝ったギミックを作る時に、Unity のアニメーションを使うことがあると思います。
アニメーションで時間の制御や状態遷移を扱い、Animation Event で VRChat コンポーネントを呼び出すかもしれません。

この場合に、Unity アニメーションと VRChat コンポーネントの組み合わせのデバッグは Unity エディタ上では行えません。
VRChat のコンポーネントは Unity エディタ上で動作しないためです。
アニメーションの内部状態やトリガーの駆動を VRChat クライアントで行うのは面倒な作業です。

このエミュレータを使うと、通常の Unity アプリケーション開発と同様にアニメーションの動作確認を
（状態遷移の様子を観察したり、パラメタを手動で入力したりして） Unity エディタ上で行えるようになります。



## 注意（プロジェクトのバックアップを！）

このソフトウェアは開発の初期段階にあります。
実装の間違いのために、利用者の Unity プロジェクトを壊してしまう可能性があります。
バックアップすることなしに、大事なプロジェクト使わないようにしてください。


## 使い方

* リリースされている Unity パッケージを、ワールドを作成しているプロジェクトに import してください
    * （GitHub からリリース前の最新版を得て使いたい場合は、Asset フォルダーにファイルをコピーしてください）
* Unity のメニュー  Window > VRC_Iwsd > Emulator を選択し "Emulator Setting" ウィンドウを開きます
    * "Enabled" チェックボックスをオンにします
    * Unity エディタの再生ボタンを押してエミュレータを開始します
    * （この二つの操作を一度に行う "Set enable and start" ボタンもあります）
* Unity エディタの Game ウィンドウにフォーカスがあると、プレイヤーキャラクターを操作できます
    * WASD の各キーで移動
    * マウス移動で左右回転と視線の上下
    * スペースキーでジャンプ
* オブジェクトの interact は、視界の中央にそれを入れてマウスの左ボタンを押します
* Q キーでクイックメニューが開きます
    * （オリジナルの VRChat クライアントと同様に ESC キーでもクイックメニューを開きます。
      ただし Unity エディタ中では ESC はマウスカーソルの有効化もおこないます。Q キーを使う事でその振る舞いを避けることができます。）
    * クイックメニューから、プレイヤーのリスポーンとエミュレータの終了が行えます
* TAB キーでマウスカーソルの表示がトグルします
    * マウスカーソルが表示されていると、play モードでの Unity エディタの通常操作が行えます
* Unity の再生ボタンでもエミュレータの開始終了が行えます
* VRChat SDK の "Build Control Panel" を使った publish 操作の前には、"Emulator Setting" の "Enabled" チェックボックスを無効化する必要があります。
    * （無効化しない場合、publish ボタンは不完全な状態でエミュレータを開始してしまいます）
    * （このようになるのは、SDK の publish 処理とこのエミュレータが共に Unity としては「ゲーム」として作成されているためです）


## 開発状況概要

* 現在の所、シングルプレイヤーのみ
    * ゆえに broadcast type は全く考慮していない
* トリガーとアクションの実装状況：
    * ほとんどすべてのアクションは実装済み
    * Relay と OnVideoStart のような VRC コンポーネントに依存するものを除き、ほとんどの Trigger は実装済み
    * （詳細については Features.txt や  Emu_Trigger.cs を参照）
* VRC_Trigger コンポーネントの ExecuteCustomTrigger は Unity の animation trigger から利用可能
* 現在の所基本的に VRC_Trigger コンポーネントのみをエミュレートしている
    * （いくつかのコンポーネントについては特段の変更なしに動くようだ。例えば VRC_AudioBank は RPC を通して動作する）


## このツールによって得られること、目的、制約 など

* VRChat ワールド作成におけるデバッグを容易にする
* デバッグ実行を開始するまでの時間を短縮する（ビルドが不要）
* ワールドを最適化したり改良するのに、Unity エディタ環境でのツールを使えるようにする
* 現在の所デスクトップクライアントのエミュレートのみ。VR 対応の優先度は低め。


## 特記事項

### OnDestroy トリガーが NullReferenceException を発生させる

OnDestroy トリガーを使うと以下のエラーメッセージが Unity の Console ウィンドウに表示される。
これは無視してよい。

    NullReferenceException: Object reference not set to an instance of an object
    VRCSDK2.VRC_Trigger.ExecuteTriggerType (TriggerType triggerType)
    VRCSDK2.VRC_Trigger.OnDestroy ()

<!--
オリジナルの VRC_Trigger は実行時に自身をシーンから消去するようになっている。
Unity エディタ環境では正しく初期化されない。
おそらくそれがこのエラーが出る理由だと思われる。
-->


## ライセンスなど

MIT licence
by naqtn (https://twitter.com/naqtn)

開発版 https://github.com/naqtn/ProgrammingInVRChat

不具合の報告や機能要望がある場合は、 GitHub issue (https://github.com/naqtn/ProgrammingInVRChat/issues) へ投稿してください。

---
end
