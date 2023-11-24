---
title: Enumを継承したい！と思ったときに僕らが求めているもの
tags:
  - C#
  - Unity
private: false
updated_at: '2023-11-25T02:38:14+09:00'
id: 26ede420cf916ac0b916
organization_url_name: null
slide: false
ignorePublish: false
---
# Enumを継承したい！
エンジニアなら誰しも一度は「Enumを継承したい！」と思うものです。そうですよね？
しかしいくら調べても継承する方法はなく、
EnumっぽいStructを作る内容など眺めては、「これなのか…？」と首をひねる日々。

この記事は、私たちが求めているものを明確にし、
なるだけ近い機能をもったStructを作ってみようという試みです！

# Enumの魅力に迫る
継承できなくて不便だと思いながらもEnumを使ってしまう。
Enumの何が我々をそんなに惹きつけるのでしょうか？
何と言っても
<ul>
<li><font color=#f08300><strong>圧倒的作りやすさ</strong></font>
<li><font color=#f08300><strong>UnitySerializeとの相性の良さ</strong></font>
</ul>
でしょう！

# 継承して何をしたいんだ？
では、そんな魅力的なEnumを継承して、やりたいこととは何でしょう？
ずばり
<ul>
<li><font color=#f08300><strong>異なるEnumを既定クラスで指定して同質に扱いたい</strong></font>
</ul>
ひいては
<ul>
<li><font color=#f08300><strong>同質のEnumを引数に持つメソッドをまとめたい</strong></font>
</ul>

のです！

<font color="#c8c2be">え？そんなことはない？</font>
<font color="#c8c2be">ごめんなさい、そんな方にはこの記事は参考にならないかもしれません……</font>

# Enumを継承したい！と思ったときに僕らが求めているもの
話は簡単です。以下の条件を満たせば良いのです！

<ol>
<li><font color=#f08300><strong>Enumの圧倒的作りやすさは保つ</strong></font>
<li><font color=#f08300><strong>EnumのUnitySerializeとの相性の良さもそのままに</strong></font>
<li><font color=#f08300><strong>同質のEnumを引数に持つメソッドをまとることができる</strong></font>
</ol>

1番と2番を達成するためには、Enumをそのまま使うしかなさそうです。
そこに、3番を加えます。

Intにキャストすれば、まとめること自体は簡単です。
ただし、今回は、<font color=#f08300><strong>同質のEnum</strong></font>だけをまとめたいわけなので、
**同質のEnum間のキャストだけを受け付けるStructを作ってみようと思います！！**

# 同質性を整理する
ここからは、具体的なケースをもとに考えていきます。
まずは、まとめようとしているEnumがどういった点で同質なのかを整理します。
今回は、プレイヤーと敵の二者について、状態を司るEnumが以下のようにあるとします。

```csharp
public enum PlayerState
{
    Normal,
    Angry,
    Smile
}

public enum EnemyState
{
    Normal,
    Angry,
    Sad
}
```

# まとめたい処理を整理する

そしてこれらは、Presenterのイベント通知や、ロード時のデータなどを使って、
各キャラクターのスコープの外から特定のStateに応じた処理をリクエストされる可能性があるとします。
つまり、以下の三種類のメソッドがそれぞれのクラスに必要になります。

```csharp
public void ChangePlayerState ( PlayerState state){ //変更処理 }
```
```csharp
public void ChangeEnemyState( EnemyState state ){ //変更処理 }
```

これらのステートは同質のものを扱っていて、
変更処理の内部で行われることもほとんど同一だとします。
そこで、以下のように処理をまとめることができると、Interfaceをまとめたり、
基底クラスを利用したりする余地が生まれそうです。

```csharp
public void ChangeCharacterState( int stateNo ){ //変更処理 }
```

# 指定のEnumからしか生成できないStructを作る
しかしこれでは、もともとPlayerEnumだったIntを
誤ってEnemyのステートを変更するために使ってしまうリスクがありそうです。
無作為にどんなIntでも受け付けてしまう形は、いかにもバギーな感じがします。

そこで、まずこのIntをValueObjectにしてみます。

```csharp
public struct StateInt 
{
    public readonly int Value { get; private set; }

    public StateInt( int value )
    {
        Value = value;
    }

}
```

先程よりだいぶマシになりましたが、あらゆるIntを受け付けることに変わりはありません。

そこで、このStateIntを、<font color=#f08300><strong>同質のEnum間のキャストだけを受け付けるStruct</strong></font>にしてみます。

```csharp
public struct StateInt 
{
    public readonly int Value { get; private set; }

    public StateInt( PlayerState value )
    {
        Value = value;
    }

    public StateInt( EnemyState value )
    {
        Value = value;
    }
}
```

# 生成時と同じ型へのキャスト以外はエラーを出す

さらに、もともとどのタイプからキャストされたかを示すEnumを作成し、

生成したときに、どのタイプからキャストされたかを保持するようにします。

```csharp
public struct StateInt 
{
    public readonly int Value { get; private set; }
    public readonly StateType Type { get; private set; }    

    public StateInt( PlayerState value )
    {
        Value = value;
        Type = StateType.Player;
    }

    public StateInt( EnemyState value )
    {
        Value = value;
        Type = StateType.Enemy;
    }
    
    private enum StateType
    {
        Player,
        Enemy,
    }
}
```

保持されたTypeを使って、キャスト元のEnumに戻すメソッドと、
それ以外の型にキャストされた時にエラーを出すメソッドを実装します。

```csharp
public struct StateInt 
{
    public readonly int Value { get; private set; }
    public readonly StateType Type { get; private set; }    

    //~中略~
    
    public static implicit operator PlayerState (StateInt stateInt)
    {
        if(stateInt.Type != StateType.Player) 
        {
            throw new InvalidOperationException( "生成時とは異なる型にキャストされようとしています。生成時の型：" + Type + "キャスト型" + CharacterType.Player );
        }
        return (PlayerState)stateInt.Value;
    }
    
    public static implicit operator EnemyState (StateInt stateInt)
    {
        if(stateInt.Type != StateType.Enemy) 
        {
            throw new InvalidOperationException( "生成時とは異なる型にキャストされようとしています。生成時の型：" + Type + "キャスト型" + CharacterType.Enemy );
        }
        return (EnemyState)stateInt.Value;
    }
}
```

# メソッドをまとめてみる
これで、不正なキャストがあれば明確に例外が発生するようになりました！
では、先程のメソッドをもったクラスをまとめてみましょう。

```csharp
public abstract class CharacterStateControllerBase: Monobehaviour
{
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    public void ChangeCharacterState(StateInt state)
    {
        Sprite stateAnimation = GetSprite(state);
        _spriteRenderer.sprite = stateAnimation;
    }
    protected abstract Sprite GetSprite(StateInt state);
}

public class PlayerStateController : CharacterStateControllerBase
{
    //実際はSerializedDictionaryなどを使ってください。
    //EnumをKeyにしたときのブロック化の問題は今回は割愛します。
    [SerializedField]
    private Dictionary<PlayerState,Sprite> _stateStandPictures;
    
    protected override Sprite GetSprite(StateInt state)
    {
        return _stateStandPictures[(PlayerState)state];
    }
}

public class EnemyStateController : CharacterStateControllerBase
{
    //実際はSerializedDictionaryなどを使ってください。
    //EnumをKeyにしたときのブロック化の問題は今回は割愛します。
    [SerializedField]
    private Dictionary<EnemyState,Sprite> _stateStandPictures;
    
    protected override Sprite GetSprite(StateInt state)
    {
        return _stateStandPictures[(EnemyState)state];
    }
}
```

# まとめ
以上で、Enumを継承したい！と思ったときに僕らが求めているものを実現することができました！

同質の処理をBaseクラスにまとめて、
データとして異なる部分だけを派生クラスに持たせることで、 保守のしやすい構造になったかと思います。
Enumを継承したい！と思ったときに思い出していただけると幸いです！
