---
title: Source Generatorでプロパティを自動実装してみた
tags:
  - 'Unity'
  - 'C#'
private: true
updated_at: ''
id: null
organization_url_name: unity-game-dev-guild
slide: false
ignorePublish: true
---
# なんの記事？
メンバ変数からプロパティを自動実装するSource Generatorを作りました！
具体的には、以下のコードが、
```csharp
        [SerializeField]
        private float _brabra;
        public float Brabra
        {
            get => _brabra;
            private set => _brabra = value;   
        }
```

次のように書けるようになります！
```csharp
        [AutoProp(AXS.PublicGetPrivateSet),SerializeField]
        private float _brabra;
```


# 動作環境
Unity 2022.3.4f1
Rider　2023.2　での動作を確認しています。

※Unityはおそらく　Unity2021.2　以上で動作するかと思います。
　IDEはVSCode VisualStudioでも動作するようですが、追加工程が必要な場合があるようです。

# 導入
導入は簡単です。

* 以下のGithubにアクセスします

https://github.com/amenonegames/AutoGenerateProperty

* 右側のリリースページにアクセスします。
  ![名称未設定-2.jpg](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/6581edee-59a1-bcc6-ca74-53eeedd42467.jpeg)

  
* AutoGenerateProperty.dll をDLします

  ![名称未設定-2.jpg](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/831e6e83-6232-b8db-1f48-f5c880d108a9.jpeg)
  
  
* Assts以下の好きな場所に配置し、InspecterからGeneralとSelect platforms for pluginのチェックをすべて外します。
  ![名称未設定-2.jpg](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/f3363e46-8833-fe46-2bb1-d3bb8e305a17.jpeg)
  
  
* 同じくインスペクターから、 RoslynAnalyzer という名前でラベルを付与します。
                                                                                                                                                                                                                                                                                                                                                                                                              ![名称未設定-2.jpg](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/ab20baef-9ed5-289d-6d8c-208fec076a84.jpeg)

以上で導入完了です！

ただし使っているIDEによっては、IDE側に設定が反映しない場合もあるそうです。
Cysharp様が公開されているCsprojModifierを設定することでIDEに反映できるとのことですので、併せて紹介しておきます。

https://github.com/Cysharp/CsprojModifier

# 機能紹介

###  [AutoProp]アトリビュート

以下のように変数に**AutoProp**属性をつけると、コンパイル時に自動でプロパティを生成します。
変数を格納するクラスは **partial class** で記載してください。

:::note warn
[AutoProp]をつける変数は、文頭を小文字から始めるか、アンダースコア(_)を付した後、すぐに小文字から書き始めてください。
他の文字列でも生成はされますが、理解し辛いので運用上オススメできません。
:::

```csharp
public partial class HogeHolder
{
    [AutoProp]private float _hogehoge;

    //例えば以下のようにアクセスできます。
    private void DebugHoge()
    {
        Debug.log(Hoge.ToString());
    }
    
}
```
:::note warn
コンパイルが終わるまでプロパティへのアクセスがエラー扱いになる場合があります。
数秒待つとプロパティが生成され、エラーが消えるはずです。
:::

:::note info
元とする変数名と生成されるプロパティ名は以下のようになります。
○　変数名：hogehoge　 →プロパティ名：Hogehoge
○　変数名：_hogehoge　→プロパティ名：Hogehoge
☓　変数名：Hogehoge　→プロパティ名：HOgehoge
☓　変数名：HOGEHOGE　→プロパティ名：NoLetterCanUppercase
:::

###  アクセスレベルのコントロール
**引数無しで登録したAutoPropは、privateなGetterだけを実装します。**

GetterやSetterのアクセスレベルを制御する場合は、引数にAXSというEnumを渡してください。
例えば **AXS.PublicGetSet** を指定すると、publicなGetterとSetterを実装します。
コード例を示します。

```csharp
public partial class HogeHolder
{
    //AXSを引数として渡す
    [AutoProp(AXS.PublicGetSet)]private float _hogehoge;

    private void DebugHoge()
    {
        // Setterがあるので書き込みもできる
        Hoge = 0.5f;
        // 当然読み込みもできる
        Debug.log(Hoge.ToString());
    }
    
}
```


他にも、GetterがPublic、SetterがPrivateなどの選択肢を用意しています。
名前を見ればわかるようになっていますので、AXSの一覧のみ掲載します。

<details><summary>AXS一覧</summary>

```csharp
        PublicGet,
        PublicGetSet,
        PublicGetPrivateSet,
        PrivateGet,
        PrivateGetSet,
        ProtectedGet,
        ProtectedGetSet,
        ProtectedGetPrivateSet,
        InternalGet,
        InternalGetSet,
        InternalGetPrivateSet,
        ProtectedInternalGet,
        ProtectedInternalGetSet,
        ProtectedInternalGetPrivateSet,
```

</details>

### プロパティアクセス時の自動キャスト
Unity側でシリアライズする値はプリミティブ型にしたいけれど、
なるだけ早い段階で自作のValueObject型にしてしまいたい場合ってありませんか？
ありますよね？　わかります。ありますよね。

そこで活躍するのがこの機能です。
仮に、floatにキャスト可能なHPというStructを定義したとして例示します。

#### Getterでのキャスト
以下のように引数にTypeを渡してやると、
floatをHPにキャストして値を取り出すGetterが実装されます。

```csharp
public partial class HogeHolder
{
    //Typeを引数として渡す
    [AutoProp(typeof(HP)),SerializeField]
    private float _hogehoge;

    private void DebugHoge()
    {
        //Getter内でHP型にキャストしてから出てくる
        HP currentHp = Hogehoge;
        //逆にfloatに戻すにはキャストが必要
        float HPFloat = (float)Hogehoge;
    }
}
```

#### Setterでのキャスト
また、以下のようにセッターも実装する指示をしてやると、
HPをfloatにキャストして_hogehogeに格納するSetterも実装されます。

```csharp
public partial class HogeHolder
{
    //Setterの実装を指示する
    [AutoProp(typeof(HP),AXS.PublicGetSet),SerializeField]
    private float _hogehoge;

    private void DebugHoge()
    {
        HP newHp = 50f;
        //Setter内でキャストが入るので、float型でもHP型でも代入可能
        Hogehoge = newHp;
        Hogehoge = 50f;
    }
}
```

#### 前提条件
public struct HP
{
    readonly float value;
    public static explicit operator float(HP value) => value.value;
    public static explicit operator HP(float value) => new HP(value);
    
    public Hp(float value)
    {
        this.value = value;
    }
}
```

:::note info
もちろん```implicit Operator``` でもOKです。
:::

##  デフォルトアクセスレベルの変更

デフォルトのアクセスレベルを変更したい場合は、プロジェクトのソースコードをダウンロードするかCloneして頂いて、以下の三箇所を書き換えてください。
仮にPublicGetSetを標準にしたい場合の例を示します。

```diff_c_sharp:AutoPropertyGeneratorクラス SetDefaultAttributeメソッド内
        // デフォルトアクセスレベルを変える場合は、ここを変更する
-        public AutoPropAttribute(AXS access = AXS.PrivateGet)
+        public AutoPropAttribute(AXS access = AXS.PublicGetSet)
        {
            AXSType = access;
        }

        // デフォルトアクセスレベルを変える場合は、ここを変更する
-        public AutoPropAttribute(Type type, AXS access = AXS.PrivateGet)
+        public AutoPropAttribute(Type type, AXS access = AXS.PublicGetSet)
        {
            Type = type;
            AXSType = access;
        }

```

```diff_c_sharp:AutoPropertyGeneratorクラス Executeメソッド内
        var arguments = field.attr.ArgumentList?.Arguments;
        (IFieldSymbol field, ITypeSymbol sourceType , ITypeSymbol targetType , AXS acess) result =
-        (fieldSymbol, fieldSymbol.Type, fieldSymbol.Type, AXS.PrivateGet); // デフォルトアクセスレベルを変える場合は、ここを変更する
+        (fieldSymbol, fieldSymbol.Type, fieldSymbol.Type, AXS.PublicGetSet);
```

その上であたらしいDLLをビルドして、上記と同じ手順で導入して貰えればOKです。

# おわりに
記事の内容は以上です。
何度も繰り返し書くような処理はSourceGeneratorで省略して書けるようにしてあげるとスッキリして気持ちいいですね！
もしよろしければ試してみてください！！

# 謝辞

本記事は、以下の記事のアイデアに大きく影響を受けて制作いたしました。
記事を書いていただいた@RyotaMurohoshiさまにこの場を借りてお礼申し上げます。

https://qiita.com/RyotaMurohoshi/items/05ba5a7e01b3e66f68ab

また、型の自動キャスト機能は、UnitGeneratorを使いながら、
Unityのシリアライズを便利に使うために作ってみました。
UnitGeneratorは非常に強力なValue Objectの生成支援機能ですので、
ご興味のある方はぜひ覗いてみてください。

https://github.com/Cysharp/UnitGenerator

Source Generatorの実装には複数のサイトを見させていただきましたが、中でも以下のゆっち～さまの記事が特に実践的でわかりやすかったです。
ご興味のある方はぜひ覗いてみてください。なお、```AutoPropertyGenerator```クラスにメソッドを一つ追加する必要がありますので、私がつくったものを置いておきます。

https://forpro.unity3d.jp/unity_pro_tips/2022/12/19/4225/#UnitySource_Generator

```csharp:AutoPropertyGeneratorへの追加コード
private string GetPropertyName(string fieldName)
    {
        //プロパティ名がアンダースコアから始まる場合は、先頭一文字を削除
        if(fieldName.StartsWith("_")) fieldName = fieldName.Substring(1);
        //フィールド名の先頭を大文字にしてプロパティ名にする
        return Regex.Replace(fieldName, @"^\w", m => m.Value.ToUpper());
    }
```

※最適とは言えませんが、処理内容が理解しやすいものを置いておきます。