---
title: Enumを継承したい！と思ったときに僕らが求めているもの
tags:
  - ''
private: false
updated_at: ''
id: null
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
<li><font color=#f08300><strong>圧倒的作りやすさ</strong></color>
<li><font color=#f08300><strong>UnitySerializeとの相性の良さ</strong></color>
</ul>
でしょう！

# 継承して何をしたいんだ？
では、そんな魅力的なEnumを継承して、やりたいこととは何でしょう？
ずばり
<ul>
<li><font color=#f08300><strong>異なるEnumを既定クラスで指定して同質に扱いたい</strong></color>
</ul>
ひいては
<ul>
<li><font color=#f08300><strong>同質のEnumを引数に持つメソッドをまとめたい</strong></color>
</ul>

のです！

<font color=#c8c2be>え？そんなことはない？
<font color=#c8c2be>ごめんなさい、そんな方にはこの記事は参考にならないかもしれません……
</color>

# Enumを継承したい！と思ったときに僕らが求めているもの
話は簡単です。以下の条件を満たせば良いのです！

<ol>
<li><font color=#f08300><strong>Enumの圧倒的作りやすさは保つ</strong></color>
<li><font color=#f08300><strong>EnumのUnitySerializeとの相性の良さもそのままに</strong></color>
<li><font color=#f08300><strong>同質のEnumを引数に持つメソッドをまとることができる</strong></color>
</ol>

1番と2番を達成するためには、Enumをそのまま使うしかなさそうです。
そこに、3番を加えます。

Intにキャストすれば、まとめること自体は簡単です。
ただし、今回は、<font color=#f08300><strong>同質のEnum</strong></font>だけをまとめたいわけなので、
**同質のEnum間のキャストだけを受け付けるStructを作ってみようと思います！！**

# 同質性を整理する
ここからは、具体的なケースをもとに考えていきます。
まずは、まとめようとしているEnumがどういった点で同質なのかを整理します。
今回は、プレイヤーとモブと敵の三者について、状態を司るEnumが以下のようにあるとします。

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

public enum MobState
{
    Normal,
}

```

そしてこれらは、Presenterのイベント通知や、ロード時のデータなどを使って、
各キャラクターのスコープの外から特定のStateに応じた処理をリクエストされる可能性があるとします。
つまり、以下の三種類のメソッドがそれぞれのクラスに必要になります。

```csharp
public void ChangePlayerState ( PlayerState state){ //変更処理 }
```
```csharp
public void ChangeEnemyState( EnemyState state ){ //変更処理 }
```
```csharp
public void ChangeMobState( MobState state ){ //変更処理 }
```

これらのステートは同質のものを扱っていて、
変更処理の内部で行われることもほとんど同一だとします。
そこで、以下のように処理をまとめることができると、Interfaceをまとめたり、
基底クラスを利用したりする余地が生まれそうです。

```csharp
public void ChangeCharacterState( int stateNo ){ //変更処理 }
```

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

    public static implicit operator int( StateInt stateInt )
    {
        return stateInt.Value;
    }

    public static implicit operator StateInt( int value )
    {
        return new StateInt( value );
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

    public static implicit operator int( StateInt stateInt )
    {
        return stateInt.Value;
    }

    public static implicit operator StateInt( int value )
    {
        return new StateInt( value );
    }
}