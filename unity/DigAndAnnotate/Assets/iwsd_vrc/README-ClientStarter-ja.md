# Client Starter

このエディタ拡張は、ワールドのアップロード（publish）が完了すると自動的に VRChat クライアントを起動します。
アップロードされたワールドに直ぐに入りテストを開始できます。


## 作成背景
これはある意味で、VRChat SDK の機能であるローカルテストと同じではある。
ローカルテストは信用ならない場合があり、通常のクライアントでのテストも必要。
そしてクイックメニューからワールドを選択するのは苦痛。というわけでこれを作りました。

## 使い方

* Unity パッケージをインストールする。それだけ。
    * ワールドのアップロード処理が完了すると、VRChat クライアントが自動的に起動する
    * 初期状態では、デスクトップモードで起動する
* 設定を変更したい場合は、Unity メニュー `Window > VRC_Iwsd > Client Starter` から設定画面を開く
* 設定画面は他の機能も提供している。例えば：
    * vrchat.com のにあるワールド管理ページを開く
    * ワールド ID で指定された他のワールドを開く


## 機能詳細

### 簡易UIモードでの機能詳細

![fig.simpleUI](Doc/ClientStarter-simpleUI.PNG "simple UI mode sample")

* `Start After Publish` チェックボックス
    * 有効の場合、ワールドのアップロード（publish）処理が完了すると自動的に VRChat クライアントを起動する
    * 設定画面を開いていなくても動作する。設定が終わった後は閉じて良い。
    * VRChat SDK が表示する "Content Successfully Uploaded! / Manage World in Browser" のウィンドウをこのツールは閉じる
        * 内部的にはこのウィンドウの出現がクライアントを開始するトリガーになっている
        * このウィンドウを使いたい時は、この `Start After Publish` 設定を無効にする
* `Access` 選択
    * invite, invite+ などのアクセスレベルを選択する
* `Start Published World` ボタン
    * 手動でVRChat クライアントを開始する
    * `Access` 選択で public 以外を選んだ場合はログインしていることが必要。していない場合は VRChat SDK の Setting ウィンドウを開く
* `Open Manage Page` ボタン
    * vrchat.com のにあるワールド管理ページを開く
* `Advanced` チェックボックス
    * 詳細UIモードを有効にする


### 詳細UIモードでの機能詳細

![fig.advancedUI](Doc/ClientStarter-advancedUI.PNG "advanced UI mode sample")

* `World ID (read only)` 欄
    * 何か操作すると現在編集中のワールドの ID が自動的に表示される。編集は出来ない
    * コピー可能
* `Copy Launch Link` ボタン
    * ワールドの launch link (`vrchat://...` という形式の URI) 文字列をクリップボードにコピーする
* `Start another world` セクション
    * ID 欄に手で入力したワールドを取り扱う
    * 編集中とは別のワールドを比較のために参照したりするのに使う
* `World ID` 欄
    * ワールドを選択するために world ID を入力する
    * 「world ID」とは次のような形式の文字列である `wrld_b51f016d-1073-4c75-930d-9f44222c7fc3`
    * 余分な文字があっても許容する。それらは単に無視される。
        * 例えば、次のような形式の shared link そのものを入力できる
        `https://vrchat.com/home/launch?worldId=wrld_b51f016d-1073-4c75-930d-9f44222c7fc3`
        余分な先頭部分を取り除く必要はない
* `Use SDK's 'Client Path'` チェックボックス
    * 有効の場合： `Installed Client Path` で指定されるプログラムを使って開始する
        * これは VRChat SDK の Setting ウィンドウにあるもので、そこの `Edit` ボタンで変更できる。
        * もし Occurs クライアントしかインストールしていなくて（つまり Steam 版を入れてなくて）
        `Installed Client Path` にパスを指定していない（つまりデフォルトのままにしている）場合、
        おそらく起動に失敗する。
            * この事態を取り扱うプログラムを書くための情報不足のため
            * 回避策：`Installed Client Path` に VRChat.exe がある場所を指定する
    * 無効の場合： launch link (vrchat://...) を開く
        * デフォルトでは、Windows が `{VRChat がインストールされているフォルダ}/launch.bat` を走らせ、それが VRChat.exe を走らせる
* `Desktop mode` チェックボックス
    * 有効の場合、VRChat をデスクトップモードで実行する
    * この選択は `Use SDK's 'Client Path'` が有効である場合にのみ機能する


## リリース ノート
- ClientStarter-20190708
    - 初版リリース
- ClientStarter-20190718
    - Access の選択が保存されていなかったのを修正


## 謝辞
Thanks VRCPrefabs and iwashi farm people for testing and suggestions.


## ライセンスなど

* ライセンスは MIT License
* https://github.com/naqtn/ProgrammingInVRChat で開発を進めています
* 不具合報告や機能提案が有りましたら Twitter (https://twitter.com/naqtn) でお知らせいただくか、
 GitHub issue (https://github.com/naqtn/ProgrammingInVRChat/issues) に投稿をお願いします。


<!--
## 内部処理メモ
（英語版を参照）
-->
