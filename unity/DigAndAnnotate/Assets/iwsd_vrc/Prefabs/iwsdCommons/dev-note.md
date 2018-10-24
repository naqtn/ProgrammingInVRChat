
# master local 作業メモ

結局最後には CyanLaser 式を導入したので、採用されなかったわけだがメモを残しておく。ぐっちゃぐちゃだけど。


master local
名前に悩む

ActivateGate
MasterActivation
Gate
InvokeRequest


self trans はまずいのでは？

---

{OnEnable,OnPlayerLeft}[Local] => Custom[Local] => Custom[Master]
はうまくいく

OnInteract[Always] => Custom[Local] => Custom[Master]
は上手くいかない
master 操作で non-master で press がおきてしまう。
（non-master 操作で master local は OK）

まさか non-master で custom を実行している時に
initiated master が情報として残っていて最後の[Master]をすり抜ける？！
（そんな実装しているかなぁ？）

だめもと always => master
だめ。master, non-master とも全員のを起動

---

個別発生するトリガー（つまりmasterが観測できない場合もある。例えば OnInteract）から
Master Local 処理を実行したい場合を考える。

まずnon-master が操作した場合を考える。
このトリガー自体が取りうるのは Local か Always である。
（Master, Owner は条件が成立したりしなかったりするので考える必要がない。
発展的課題として Owner を絡めてもよいのかもしれないがさしあたり実行条件に入れないので、考えない）
ここから master マシンでの処理につなげる必要がある。Local のままでは意味がない。
Local => Always は期待通り稼働する。
VRC_Trigger の範囲で考える限り、
この段階のトリガーは Always ないしは Local => Always で、Always にしたものになる。
（前処理がしたいのなら Local => Always を選択することになるだろう）

今考えている機構の汎用的な部分の入り口は custom trigger だと仮定する。 
つまり Always で ActivateCustomTrigger を実行することになる。
ここまでの検討により non-master から master での custom trigger 処理が開始される。
副作用として他クライアントでも custom trigger 処理が始まる。
（アクションが実行されるかは custom trigger の broadcast type 次第。これは以下で検討する。）

この custrom trigger の broadcast type がとりうる意味のある値は Local か Master であろう。
（Always で再び広げるのは意味がなく、Owner は考えなくてよい。）
Always => Local, Always => Master は期待した通りに稼働する。

Master では action の実行が広がるので、必要がないならば Local にするのが適当だろう。
hatsuca's MasterLocal パターンではこの先 Master broadcast type を使う。
Local => Custom[Master] は幸いなことに意図したとおりに動く。

master での動作は良いとして、
non-master で最初の操作から Always => Local とつながり、
この MasterLocal 機構は non-master でも動く。


結局のところ入り口は local のまま、Always でたたけばいいだけか？だよな？


---

「他のトリガーでもこの機構は動かせる」と書いたものの、OnInteractなどの特定クライアントのみで発生する場合にはマスターへ伝達する必要があり、実装方法を模索していた。機構を実装する部品側をLocalなCustomで実装し、OnInteractを素直にAlwaysUnbufferedにして駆動すると上手くいかない。（続く

マスター操作で他クライアントでもMasterLocalにしたいアクションが、なぜか実行されてしまう。（ログを見ると確かに二つ目のboolをtrueにしている。MasterUnbufferedにしてガードしているところが機能していないように見える）
試行錯誤したら、原理がいまいち分からないが動いた。（続く

動いた構成： OnInteractをAlwaysUnbufferedにして各クライアントへ配りAnimationBoolを実行、animation eventでExecuteTriggerTypeでトリガー発火、そのトリガーはDelayを設定しておき（←ポイント）あとはActivateCustomTrigger。

一旦区切ったことで最初のOnInteractのAlwaysUnbufferedに影響されなくなり MasterUnbuffered のガードが意図したように機能するようになったと思うんだが…。
間に Unity animation が挟まり、さらに異なる GameObject なのに影響受けている（のか？）。実行コンテキストみたいなものがあるのか、もしかして？

#VRChat, Does VRC_Trigger have "execution context" or something like that?


---

Animation param Entry 要らない？
Local にしておきたい場合も
OnInteract[Local] => Custom[AlwaysUnbuffered]  => Custom[Local,non zero Delay] => Custom[Master]
でいけるのでは？


今回の場合はさておき
OnInteract[Local] => Custom[AlwaysUnbuffered,non zero Delay] => Custom[Master]
と変形可能なのだろうか？

まさか
OnInteract[AlwaysUnbuffered] => Custom[Master] => Custom[Local,non zero Delay]
で MasterLocal が実現できるとかないだろうか？
さらに入り口を問わなければ
OnInteract[Master] => Custom[Local,non zero Delay] とか。

⇒ さすがにそれは無かった。

まとめ：
先に書いた、OnInteractのようにnon-masterでも起こるトリガーからMasterLocalを駆動する方法、もっとシンプルにできた。
OnInteract[AlwaysUnbuffered] => Custom[Local,non zero Delay] => Custom[Master]{AnimationBool}
これは動く。（前に書いたのと同じくDelayは必須）

起点をLocalにしたい場合次もOK。
OnInteract[Local] => Custom[AlwaysUnbuffered]  => Custom[Local,non zero Delay] => Custom[Master]{AnimationBool}
多分これが一般化されたパターンだと思う。



hatsuca さんが作られた MasterLocal のプレハブですけども。自分が作っているパノラマのワールドで使いたいので改変整備してみました（VRC_Panorama の NextPano などが MasterLocal でないと安定動作しないため）。この仕組みは大変有用だと思うのでパノラマとは別に、汎用部品集としてプレハブ配布（およびサンプルワールドの公開）をしたいと思うのですが、よろしいでしょうか？
wrld_442580ff-7cd2-48f5-adc3-133f1807dcaf



iwsdSimpleRegacy_ScaleIn_wShiftXp
iwsdSimpleRegacy_ScaleOut_wShiftXm
