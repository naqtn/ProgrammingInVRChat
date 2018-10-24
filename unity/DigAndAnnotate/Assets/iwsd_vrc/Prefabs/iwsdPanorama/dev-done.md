# format (mono or stereo) は property で enum 表現できないのか？
⇒ [Enum] がまさにそれ


# 6sidedの shader に ProjectToModel など復活

# sixsided camera render texture をカメラ側設定を変えて一枚にできないか？
⇒ だめぽ。
https://docs.unity3d.com/ScriptReference/Camera-targetTexture.html
https://forum.unity.com/threads/rendering-into-part-of-a-render-texture.425371/
そもそもここをけちる必要あるのか？
生きているカメラを減らしたいならともかく
頑張るならプレイヤーの見る方向を取って
遠隔でカメラを動かした方がいい。
ローカルで制御すれば実際には全球撮っていないけどそのように見えるはず。


# マテリアルファイルの置き場所と名前にさんざん悩む
問題はマテリアルという単位が多義的な意味を持つところにあるように思う
- マテリアルファイルの存在はプレハブ利用者に対する外部インタフェースでありつつ、内部的なものもある
- Asset 名はグローバルである一方、整理をするならフォルダ名に区分名を入れたい、すると重複感がある
- シェーダーと結びつきつつ、そうでないのもある


# rename and rearenge assets for SingleCamera
wide FOV
single sided wide FOV

iwsdPanorama_SingleCamera
iwsdWideFOVPanorama
iwsdWideFOVCamPanorama
iwsdWideFOVProjection
iwsdWideFOVReprojection
iwsdPerspectiveReprojection

iwsdPanorama_PerspectiveReprojection
iwsdPanorama_PerspectiveProjectionReprojection

iwsdPerspectiveReprojection


