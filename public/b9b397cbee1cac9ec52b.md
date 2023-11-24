---
title: 【Unity用】ノベルゲーム・アドベンチャーゲーム制作ライブラリYarn Spinnerを使ってみた　その2
tags:
  - Unity3D
  - Unity
  - ','
  - AdventCalendar2022
  - YarnSpinner
private: false
updated_at: '2022-12-17T13:11:08+09:00'
id: b9b397cbee1cac9ec52b
organization_url_name: null
slide: false
ignorePublish: false
---
:::note
この記事は[【Unity用】ノベルゲーム・アドベンチャーゲーム制作ライブラリYarn Spinnerを使ってみた　その1 ](https://qiita.com/amenone_games/private/c01bb5c1e62d8a59fe9f)の続きです。
:::

# 自作コマンドの導入（起動速度を上げる方法）
先述のようにアトリビュートで自作コマンドを追加している場合、yarnはアセンブリの情報を走査してメソッドを探します。
そのため、プロジェクトが大規模になると起動速度に影響が出る可能性があります。
起動速度を確保するために、DialogueRunnerクラスに直接メソッドを登録する方法が用意されているので、ご紹介します。

1. 実装
DialogueRunnerの参照を取り、直接Action型のメソッドを引き渡します。
このとき、DialogueRunner.AddCommandHandlerメソッドを呼んでください。
引数があるときには、AddCommandHandler<T>を使い、引数の型を指定してください。
複数引数がある場合は、AddCommandHandler<T1,T2>となります。
例えばSpriteRendererのSpriteを変更するメソッドは以下のようにして渡します。
    
    ``` CSharp
    
        public class CommandAdder : MonoBehaviour
        {
    
            [SerializeField] private DialogueRunner _dialogueRunner;
            [SerializeField] private SpriteRenderer _renderer;
            
            private void Start()
            {
                //Startメソッドでコマンドを登録します。
                _dialogueRunner.AddCommandHandler<string>("SetSprite", SetSprite);
            }
            
            private void SetSprite(string resourcesAddress)
            {
                _renderer.sprite = Resources.Load<Sprite>(resourcesAddress);
            }
    
        }
    ```
    
    yarn script 側では、```<<SetSprite スプライトの場所>>```と書くだけで実行できます。
    yarn側での記載も、GameObjectを指定する必要がなくなり、シンプルになります。

2. yarnPrjectの設定
実装を切り替えたら、今度はyarn Project側の設定から、アセンブリの自動走査を外す必要があります。
インスペクターから、下の方にあるSearch All Assembliesのチェックを外してください。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/bf49dd07-6f5a-0d58-2691-00bab0a1ddd0.png)

3. 組み込み関数の再登録
チェックを外すと、Assemblies To Searchという欄が出てきます。項目を追加して、YarnSpinner.Unityを指定してください。
これで、dice()などの組み込みファンクションのみが読み込まれるようになります。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/a15e3825-ef7e-d1fc-f744-dcb985d12949.png)

これで実装完了です！

:::note
コルーチンを登録する場合には、コルーチン型を渡す必要があります。
例えば引数にstringを持つコルーチンを登録する場合は、以下のようになります。
```CSharp
_commandReceivable.AddCommandHandler<string>("コマンド名", x => StartCoroutine(PopUp(x)));

IEnumerator PopUp(string name)
{
    //Todo
}
```
:::
:::note
返り値を持つメソッドを登録することもできます。
以下のように、.AddFunction<引数の型, 戻り値の型>を使って登録してください。
```CSharp
_dialogueRunner.AddFunction<float, string>("GetName", GetName);
public string GetName(float no)
 {
     return no + "番"; //数字の末尾に"番"という文字をつけて文字列として返す
 }
```
:::


# イベントトリガーの登録
Dialogue Runnerは下の方にEventsという項目を持っています。
ここにファンクションを登録すれば、Nodeの開始や終了にフックした処理を書くことが可能です。

![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/2834b30b-5c24-7b27-5ca8-28732f9c3e47.png)

また、OnNodeStartとOnNodeCompleteは、それぞれのノードのノード名(string型)を引数に取ります。
例えば、以下の様に書くことができます。

``` CSharp
public void OnNodeStart(string nodeName)
{
    Debug.Log(nodeName); //開始しようとしているノード名をログに出す
}

public void OnNodeComplete(string nodeName)
{
    Debug.Log(nodeName); //たった今完了したノード名をログに出す
}
```

# C#スクリプトからのyarnの起動
DialogueRunner.StartDialogue(nodeName)　でスクリプトからyarn script を起動できます。
既に起動中だった場合エラーになりますので、以下のように書くのがおすすめです。

``` CSharp
    if(_dialogueRunner.IsDialogueRunning) _dialogueRunner.Stop();
    _dialogueRunner.StartDialogue(nodeName);
```

イベントトリガーの登録機能と併用すれば、特定条件で指定のノードを経由するような機能を実装できます。

:::note
アドベンチャーゲームでyarn spinnerを使う場合、
NPC一人一人にスタートノードの文字列を持たせ、話しかけた時点で文字列を取得してDialogueRunnerに渡す処理が考えられます。
公式のサンプルはこの方法で実装されています。
:::

# C#スクリプトからのyarn変数の操作
1. ```<<set ~>>```で代入された変数の取得
Dialogue Runnerはもともと、同じDialogue Systemにアタッチされている
InMemoryVariableStorageをyarn scriptの変数格納庫として指定しています。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/f1a7eff8-adb9-e3bf-842f-fc4aaeb8265a.png)    
    yarn scriptで宣言した変数をC#から取得するためには、このInMemoryVariableStorageの参照を取って、
InMemoryVariableStorage.TryGetValue( "$変数名" , out 型名 変数名))　を呼び出してください。

    :::note warn
    yarn script内で宣言するときと同様に、変数名には ```$``` が必要です。
        :::

2. ```<<declare ~>>```で宣言された変数の取得
```<<declare ~>>```で宣言された変数はコンパイル時にYarnProject内に格納され、
```<<set ~>>```処理が行われるまでVariableStorageに書き込まれません。
まだ```<<set ~>>```されていない値をTryGetValueで取得するためには、DialogueRunner.SetInitialVariables()を呼び出して、
すべての引数をVariableStorageに格納してください。

3. 変数の代入
変数の代入はInMemoryVariableStorage.SetValue(string $変数名, 型名 値)で指定可能です。
上述の通り、そのままでは```<<declare ~>>```しただけの変数に値を代入できません。
DialogueRunner.SetInitialVariables()を呼び出しておきましょう。

# 自作変数ストレージの実装
1. ストレージの用意
ここまで、わかりやすいように変数の操作はInMemoryVariableStoragクラスのメソッドとして説明してきました。
しかし、DialogueRunnerで実際に呼び出されているのは、その抽象クラスに当たるVariableStorageBehaviourです。

    つまり、VariableStorageBehaviourを継承したクラスを作り、新たにDialogueRunnerに登録してやれば、
意外と簡単に独自の変数ストレージを実装できます。

    ひとまずInMemoryVariableStorageの中身をコピーして用意してしまいましょう。

2. SetValueの応用
VariableStorageBehaviour.SetValueは、yarn script内で```<<set ~>>```を実行したときにも呼び出されます。
ここに独自の処理を挟めば、```<<set ~>>```に応じて起動する処理を書くことができます。
例えば、Observerパターンの通知処理などの実装が考えられます。

3. Save/Loadの応用
DialogueRunnerは標準でSaveStateToPlayerPrefsというメソッドとLoadStateFromPlayerPrefsというメソッドを用意しています。
VariableStorageの中身をPlayerPrefsに書き込む処理のようですが、私は使ったことがないので詳細を把握していません。
私の場合はアセットを利用しているので別途セーブ・ロード処理を書く必要がありました。
こういった処理のためにも、独自ストレージの実装は便利です。
なお、VariableStorageBehaviourは、GetAllVariablesというメソッドと、SetAllVariablesというメソッドを実装しています。
全変数の取得とセットができるので、便利に使えます。

# dialogue viewのカスタマイズ
DialogueRunnerはDialogueViewBaseの配列を参照しています。

![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/8978ecf3-6751-2545-12c4-3ba5abf619e9.png)

__通常テキストを読み込む場合、この配列内の全要素に対して、RunLineというメソッドが叩かれます。__
__また、選択肢を読み込む場合には、同様にRunOptionsというメソッドが叩かれます。__

逆に言えば、DialogueViewBaseを継承して、これらのメソッドさえ書ければ、好きな実装ができるということです。

標準の設定では、LineViewが通常テキストの表示を、
OptionListViewが選択肢の表示を受け持っています。
まずはLineViewに注目しながら、DialogueViewBaseの実装メソッドを見ていきましょう。

## LineViewのメソッド
### RunLine
行を表示するための基本的なメソッドです。
引数として```LocalizedLine dialogueLine```と、```Action onDialogueLineFinished```を持っています。
表示するべき文字情報は、```LocalizedLine```の中に以下の通り格納されています。

```
LocalizedLine.CharacterName　:キャラクター名が格納されています
LocalizedLine.TextWithoutCharacterName.Text　：キャラ名を除いた行の内容が入っています
LocalizedLine.Text.Text　　　：キャラ名とテキスト部分を区別せず、全てのテキストが入っています
LocalizedLine.Metadata　　　 ：その行にタグ情報(yarn scriptで#で記載された情報)がある場合には、配列で入っています
```

ここから必要な文字列を取りだして、テキストを表示する処理をかけば、とりあえず自作ビューになります。
処理を次の行に進めるためには、引数である```onDialogueLineFinished```を起動する方法と、
事前に登録されている```requestInterrupt```を起動する方法があります。

```onDialogueLineFinished```は、オートモードなどで、テキスト表示完了後に自動で呼び出されるメソッドです。
登録された**全ての**```DialogueViewBase```でこの処理が呼ばれると、後述の```DismissLine```が起動します。

ユーザーの文字送りを待つ場合には、
```onDialogueLineFinished```は起動せず、yield returnで待機するか、単にReturnで処理を抜けます。

### UserRequestedViewAdvancement
この処理は、ユーザーが文字送り入力をしたときに呼び出される処理です。
事前に登録されている```requestInterrupt```を呼び出します。
```requestInterrupt```は、```DialogueRunner```で登録された処理を経由して```InterruptLine```を呼び出します。

::: note
すべてのViewで起動されるまで待つ```onDialogueLineFinished```とは対照的に
```requestInterrupt```がいずれか一つのViewで呼び出されると、
すべてのViewmの```InterruptLine```を起動します。
:::

### InterruptLine
この処理は、文字の表示途中に入力を受けた場合に備えて、一気に全テキストを表示します。
また、フェードなどのエフェクトの処理を止める役割も担っています。

引数の一つである　```onInterruptLineFinished```は処理を先に進める役割を担っています。
```onInterruptLineFinished```は、```DialogueRunner```を経由して```DismissLine```を起動します。

### DismissLine
この行の終了処理になります。
引数にもっている　```onDismissalComplete```　は、処理を次の行に進める役割を担っています。
終了処理が終わったら必ず実行しましょう。

### LineViewのまとめ
以上がLineViewに実装されているクラスの機能です。
これらのメソッドの機能を抑えれば、自作のLineViewも比較的簡単に実装できます。

標準のLineViewはFade機能やTypewriter機能の実装でちょっとごちゃごちゃしているので、
ギリギリまで削ぎ落としたLineView的なサンプルクラスを作ってみました。

<details><summary><font color="green">サンプルクラス MyLineView</font></summary>

```CSharp
using System;
using TMPro;
using Yarn.Unity;
using UnityEngine;

public class MyLineView : DialogueViewBase
{

    [SerializeField] private TextMeshProUGUI _nameField;
    
    [SerializeField]
    private TextMeshProUGUI _messageField;
    
    [SerializeField]
    private bool _autoPlay = true;

    public override void DismissLine(Action onDismissalComplete)
    {
        //autoで進んだときもUserRequestedViewAdvancementが呼ばれたときも通過する終了処理
        onDismissalComplete?.Invoke();
    }
    
    public override void InterruptLine(LocalizedLine dialogueLine, Action onInterruptLineFinished)
    {
        //UserRequestedViewAdvancementが呼ばれたときの処理
        onInterruptLineFinished?.Invoke();
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        if (dialogueLine.CharacterName is null) //もしキャラクター名がなければ
            _messageField.text = dialogueLine.TextWithoutCharacterName.Text; //メッセージフィールドのみ更新
        else //キャラ名があれば
        {
            _nameField.text = dialogueLine.CharacterName; //キャラ名フィールド更新
            _messageField.text = dialogueLine.TextWithoutCharacterName.Text; //メッセージフィールドも更新
        }
         
        //文字送りの遅延すら無いため、オートプレイだと1フレームで次の行に行ってしまう。実際には遅延処理が必要。
        if(_autoPlay) onDialogueLineFinished?.Invoke();

    }

    public override void UserRequestedViewAdvancement()
    {
        requestInterrupt?.Invoke();
    }
    
}
```

実際に使う場合は、別途ボタンを用意してUserRequestedViewAdvancementを登録しておいてください。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/8b2c6dd1-c407-e15a-3706-348d4ee0744f.png)


</details>

## OptionListViewのメソッド

### RunLine
```OptionListView```は、```RunLine```で```LocalizedLine```を毎行受け取り、直前の行のみをクラス内に保持します。
格納が終わると、```onDialogueLineFinished```を起動します。

以下、実際にかかれているコードを示します。

<details><summary><font color="green">実際のコード</font></summary>

```CSharp
RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
{
    lastSeenLine = dialogueLine; //メンバ変数に引数として渡されたdialogueLineを格納し、
    onDialogueLineFinished(); //LineFinishedを通知する。
}
```
</details>

### RunOptions
選択肢の情報を受取り、ビューに表示するメソッドです。
引数として、```DialogueOption[] dialogueOptions```と```Action<int> onOptionSelected```を持っています。

```DialogueOption[]```　の中に選択肢の情報が入っています。
```DialogueOptionクラス```は、内部に```LocalizedLine```を持っています。
選択肢のテキストやタグ要素は、この```LocalizedLine```から取り出すことができます。

まず、渡された```DialogueOption[]```の要素数になるまで、プレファブから選択肢を作ります。
<details><summary><font color="green">実際のコード</font></summary>

```CSharp
while (dialogueOptions.Length > optionViews.Count)
{
    var optionView = CreateNewOptionView();
    optionView.gameObject.SetActive(false);
}
```
</details>

その後、```DialogueOption[]　dialogueOptions```の要素から情報をゲームオブジェクトに渡して、アクティブにします。

<details><summary><font color="green">実際のコード</font></summary>

```CSharp
for (int i = 0; i < dialogueOptions.Length; i++)
{
    var optionView = optionViews[i];
    var option = dialogueOptions[i];

    if (option.IsAvailable == false && showUnavailableOptions == false) //選択肢が利用可能でないなら
    {
        // SetActiveせずにContinueする
        continue;
    }

    optionView.gameObject.SetActive(true);
    optionView.Option = option; //Optionプロパティのセッターでテキストも流し込む
    
    // 実際は最初の選択肢をフォーカスする処理が書かれているが、論旨とずれるため省略

    optionViewsCreated += 1;
}
```
</details>

続いて、LastLineというテキストフィールドに、直前の行の文字情報を流し込みます。
RunLineでLocalizedLineを格納しているのは、ここで使うためです。

<details><summary><font color="green">実際のコード</font></summary>

```CSharp
if (lastLineText != null)
{
    if (lastSeenLine != null) {
        lastLineText.gameObject.SetActive(true);
        lastLineText.text = lastSeenLine.Text.Text;
    } else {
        lastLineText.gameObject.SetActive(false);
    }
}
```
</details>

選択肢がクリックされたときの処理は```OptionViewWasSelected```に書かれています。
クリック時の処理を変更したくなった場合はここに書きましょう。

# DialogueRunnerの改造
最後にDialogueRunnerの改造について触れておきます。

DialogueRunnerは特に継承するように作られていません。
アレンジできるようにPackageの外に出したいところですが、
アクセス修飾子にinternalがよく使われているので、コピペしても書き換えも面倒ですし、
そもそもあまりpublicを増やしたくありませんよね。

Assemply Definition Referenceを使えばこれらの問題を回避できます。

### Assemply Definition
Assemply DiffinitionはUnityの機能の一つです。
指定のフォルダ階層と、それ以下の階層にあるスクリプトを一つのアセンブリにまとめることができます。
この機能によって、コンパイル時間を削減し、internalでアクセスを制限することができるようになります。

つまり、internalを残したまま独自のDialogueRunnerを実装するためには
自分の作ったスクリプトを、DialogueRunnerと同じアセンブリ内に潜り込ませる必要があります。

### Assemply Definition Reference
そこで活躍するのが、Assemply Definition Referenceです。
まずは、DialogueRunnerとして使いたいスクリプトを個別のフォルダに格納し、そのフォルダの中にAssemply Difinition Referenceを作成してください。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/13bad25f-f45f-41bc-719f-c702911be131.png)

作成できたら、インスペクターから、YarnSpinner.UnityのAssembly Definitionを指定してください。
![image.png](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/2955766/be3b6cda-8841-5871-a355-bcea0d7647e5.png)

これで、このフォルダ以下のスクリプトがYarnSpinner.Unityアセンブリに含まれるようになりました。
DialogueRunnerの内容をコピペして、アレンジしたい部分を書き換えて使ってみましょう！

::: note alert
YarnSpinner.Unityアセンブリは、自分自身とUnity.TextMeshProアセンブリの内容しか参照することができません。
他の箇所で自分が追加したクラスや、UniTask,UniRxなどの外部ライブラリは参照できず、
usingを書いてもエラーとなりますので、ご注意ください。
:::

# まとめ

お疲れ様でした！　ここまで読んで頂いてありがとうございます。
Yarn Spinnerがいかに拡張性に優れているか、わかっていただけたかと思います。
公式Discordの動きも活発ですので、ぜひ覗いてみてください。




### おまけ　-触れられなかった機能へのリンク
今回の記事ではMarkup記法、TagとMetaDataに関する内容に触れていませんので、
最後にリンクを貼っておきます。

https://docs.yarnspinner.dev/getting-started/writing-in-yarn/markup

https://docs.yarnspinner.dev/getting-started/writing-in-yarn/tags-metadata

また、ローカライズやボイスオーバーをフォローするLine Provider機能についてはこちらです。

https://docs.yarnspinner.dev/using-yarnspinner-with-unity/components/line-provider
