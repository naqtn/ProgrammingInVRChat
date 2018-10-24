/*
* 配布目的で Unity パッケージを export する際に、以下の手順を行うと必要なアセットだけ拾えて便利。
* 
* 1. Project ウィンドウでサンプルのシーンを選択
* 2. 右クリック > Select Dependencies してから
* 3. 右クリック > Export Package...
* 
* だがサンプルのシーンでは使われていないアセット、例えば説明文書などがこれでは入らない。
* 別途シフトクリックで追加すればいいのだが楽をしたい。
* 
* という事に対応するファイルを保持するためのコンポーネント。
* 
* ただし拡張子に制限がある。この目的においては実質 .txt だけ。
* https://docs.unity3d.com/ja/560/Manual/class-TextAsset.html
* 
*/

#if UNITY_EDITOR

using UnityEngine;

namespace Iwsd_vrc.Support
{
    public class TextFileHolder : MonoBehaviour
    {
	[SerializeField] TextAsset[] textAssets;
    }
}

#endif
