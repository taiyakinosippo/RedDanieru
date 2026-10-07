# オンライン不具合修正 引継ぎ資料（Sugi_Nozomi ブランチ）

最終更新: 2026-10-08

## 1. 概要

| 項目 | 内容 |
|---|---|
| ブランチ | `Sugi_Nozomi`（main へは未マージ） |
| Unity | 6000.0.78f1 |
| Unity Hub で開くフォルダ | `RedDanieru/RedDanieru`（リポジトリの一番上ではない） |
| 状態 | 自動テスト（1台で3人分を起動）では全項目OK。エディタ上での手動確認はまだ |

直したもの:
- マッチング：ルームIDの使い回し、人数のずれ、連続マッチングの不具合
- 攻撃の同期：アニメーション・ダメージ・ステッカーの奪い合い
- スポーン位置：毎回左端に出る問題
- 3人プレイ：クリアUIが他の人に出ない、HPバーが減らない、HUDが消える、ネームプレートが表示されない
- プレイヤー：敵に囲まれると上に押し出されて浮く
- 追加機能：PHPサーバーなしで動くテストモード

## 2. 別のPCで始める手順

1. このブランチを取得します。
   ```bash
   git fetch origin
   git checkout Sugi_Nozomi
   git pull
   ```
2. フォントは Git LFS で管理しているので、LFS の中身も取得します。
   ```bash
   git lfs pull
   ```
3. Unity Hub で `RedDanieru/RedDanieru` を 6000.0.78f1 で開きます。
   - リポジトリの一番上を開くと、そこに空のプロジェクトが作られてしまうので注意してください。

## 3. テストプレイの方法（PHPサーバーなし）

### テストモードを有効にする
- エディタ：メニュー「RedDanieru > オンラインテストモード（サーバーなし）」にチェックを付けます。
- ビルドした exe：起動引数に `-localtest` を付けます。

テストモード中は画面左上に `TEST MODE` と表示されます。

テストモードで変わること:
- ルーム一覧は、同じPC内のファイルで共有します。同じPCで複数起動したゲーム同士で見えます。
- ステージは、マップエディタでローカル保存したものと、同梱の `TestStage`（`Assets/StreamingAssets/TestStages/TestStage.json`）から選べます。
- 通信は Photon Cloud を使うので、インターネット接続は必要です。
- 学校のサーバーで遊ぶときは、チェックを外してください。

### 1台のPCで複数人分を起動する
- **方法A：エディタだけで試す**
  1. 「Window > Multiplayer > Multiplayer Play Mode」を開き、Player 2・3 にチェックを入れます。
  2. **`TitleScene` を開いて**再生します。
  - マッチングシーンを直接開いて再生すると、名前が「Player3」のような仮の名前になります。
- **方法B：ビルドして複数起動する**
  1. 「File > Build Profiles」で Windows 用にビルドします。
  2. 人数分起動します。
     ```bash
     "ビルド先/RedDanieru.exe" -localtest -screen-fullscreen 0 -screen-width 960 -screen-height 540 &
     ```

### ゲーム内の操作
- **ホスト**
  1. タイトルで「ダンジョンで遊ぶ」を押します。
  2. ステージを選んで「マルチ」を押します。
  3. 人数・パスワード（空欄なら公開ルーム）・ルームID（空欄なら自動）を決めて「作成」→「はい」を押します。
- **参加者**
  1. 同じステージで「マルチ」→「参加」タブを押すと、公開ルームの一覧が出ます。
  2. 部屋を選んで「はい」を押します。
  - 一覧は「参加」タブを押したときだけ更新されます。部屋が見当たらないときは、もう一度押してください。
  - ルームIDを入力して検索から入ることもできます。
- 2人以上集まると、ホストの「ゲームスタート」が押せます。

### 自作マップのスタート地点
- プレイヤーは、マップに保存されたスタート地点（RespawnPoint）に出ます。
- マップエディタでスタート地点の目印を動かさずに保存すると、初期位置の (-3, 1.2, 12)（マップ左外の入口付近）に出ます。好きな場所に動かしてから保存してください。

## 4. 不具合と原因・対応

| 不具合 | 原因 | 主な修正ファイル |
|---|---|---|
| 連続でマッチングすると壊れる | NetworkRunner は1回しか使えないのに使い回していた。一時停止メニューの「タイトルへ」が接続を切らずにシーンを移動し、古い接続が部屋に残っていた | `FusionLauncher.cs`, `GameStopManager.cs` |
| 同じルームIDが使われる | ルーム検索の「入室」が、前回の部屋ID（static の `RoomInfo.RoomId`）に接続していた | `DungeonUIManager.cs`, `RoomListLoader.cs` |
| 人数が実際より増える | DBの人数を参加/退出のたびに +1/-1 していて、切断時に減らなかった。人数上限も未設定だった | `RoomDBUploader.cs`（ホストが実接続数に合わせる）, `FusionLauncher.cs`（上限3人） |
| 他の人の攻撃が反映されない | アニメのトリガーが同期されていなかった。敵が各PCで別々に動いていて、ダメージもそのPCの敵にしか入らなかった | `PlayerAnimation.cs`, `NetworkAuthorityController.cs`, `NetworkGameState.Enemy.cs`, `EnemyBase.cs` |
| ステッカーが同期されない | 自分のPCの敵からしか剥がしていなかった | `StickerInteractor.cs`, `StickerState.cs` |
| 毎回左端にスポーンする | 固定の Start オブジェクトに出していた。さらに、Fusion が原点に作ってから位置を移すため、CharacterController が原点（マップ左下）に引き戻されていた | `PlayerSpawner.cs`, `PlayerSpawnPoint.cs` |
| 3人でクリアUIが他の人に出ない | ホスト以外が [Networked] の値を直接書いていた（ホストしか書けない） | `GoalClear.cs`, `NetworkGameState.cs` |
| HPバーが減らない（マルチ） | 他の人の満タンのHPバーが、自分の画面に重なって表示されていた | `NetworkAuthorityController.cs` |
| 自分のHUDが消える（3人） | `UIManager` の重複処理が、自分のHUDを `Destroy` していた | `UIManager.cs` |
| ネームプレートが出ない | 10/07 のコミット `c1871a1` で `NameRoot` に画面用 Canvas が誤って追加され、名前の文字が約700m先に描かれていた | `PlayerArmature.prefab`（Canvas を外して元の位置に戻した） |
| 敵に囲まれると浮く | CharacterController が敵とぶつかり、押し出されて敵の上に乗っていた | `PlayerMovement.cs` |
| マルチで動きが速い・不安定 | 自分のキャラが `FixedUpdate` と `FixedUpdateNetwork` の両方で動いていた | `PlayerActor.cs` |
| ホストが抜けるとゲーム状態が消える | NetworkGameState の設定が「ホスト退出で破棄」になっていた | `NetworkGameState.prefab`（Master Client Object に変更） |

## 5. メンバーに伝えること（担当別）

### 全員
- ファイル名・フォルダ名を直しました（`.meta` ごと移動したので、シーンや Prefab の参照は切れていません）。
  - `PlayerSpawn.cs` → `PlayerSpawner.cs`
  - `NetworkPlayerController.cs` → `NetworkAuthorityController.cs`
  - `Billboard.cs` → `NameBillboard.cs`
  - `DungeonUplloader.cs` → `DungeonUploader.cs`
  - `PLayerEvade.cs` → `PlayerEvade.cs`
  - `DungetonData/` → `DungeonData/`
  - `Cusor/` → `Cursor/`
- 自分のブランチに main を取り込むとき、上のファイルを触っていると衝突します。
- PHPサーバーのURLは `ServerApi.cs` に1か所でまとめました。

### Miyamoto（プレイヤー）
- 敵とは物理的にぶつからない設定になりました。重なった分は横方向だけに押し出されます（`PlayerMovement.EnemyLayers`）。
- アニメのトリガーは `SetTriggerSynced` を通すと、他の人の画面にも再生されます。新しいトリガーを追加するときもこれを使ってください。
- マルチでは、自分のキャラは `FixedUpdate` だけで動きます（`FixedUpdateNetwork` では動かしません）。
- Prefab の `NameRoot` には Canvas を付けないでください（付けると名前が画面外に飛びます）。

### Nakamura（敵・ステッカー）
- マルチでは、敵のAIはホストの画面でだけ動きます。他の人の画面は位置・HP・ステッカーを表示するだけです。
- 敵へのダメージは `NetworkGameState.DamageEnemy(enemy, damage)` を通してください。
- 敵の攻撃がプレイヤーに当たったときは、`PlayerStatus.Damage` が操作している本人に自動で届けます。
- 突進ステッカーの当たり判定は、衝突ではなく重なり判定に変えています。
- 未対応：ホスト以外の画面では、突進の溜めエフェクトなど一部の演出が出ません。

### Katayama（マップ）
- `MapManager.PlaceObject` に、同期用のID付け（`NetworkSyncId.Assign`）を1行追加しました。
- `GoalClear` は、自分のキャラがゴールしたときだけ反応します。マルチではホストに「クリア」を依頼します。
- 爆発樽の「Spaceで爆発」は、ジャンプと同じキーなので既定でオフにしました（`debugExplodeWithSpace`）。
- Katayama 配下のスクリプトは Shift-JIS のままです。文字コードは変えていません。

### Takeshita（オンライン・UI）
- `FusionLauncher` は、マッチングのたびに Runner を作り直します。シーンをまたいで残しません。
- `FusionLauncher.Instance` から使えます。
- ルームの人数表示は、Fusion の実接続数です。
- ゲーム開始後は、ルームが一覧から消えて途中参加できなくなります。
- 開始ボタンを押せるのはホストだけです。
- コミット `c1871a1` で `NameRoot` に Canvas が付いていたので外しました。HUD の Sorting Order -1 はそのまま残しています。

### Shiromoto
- `Create Stick.cs` が、クリア・ゲームオーバー時（メインカメラが無いとき）に毎フレーム例外を出していたので、何もしないようにしました。

## 6. main にマージする前の確認

- [ ] エディタまたはビルドで、実際に3人で遊んで確認する
  - マッチング → 攻撃 → ステッカー → クリア → タイトルへ戻る、まで通す
- [ ] 学校のPHPサーバーがある状態で、通常モード（テストモードを外した状態）も確認する（未検証）
- [ ] プライベートルーム（パスワード付き）で参加できるか確認する（未検証）
- [ ] 各メンバーのブランチと衝突しないか確認する（リネームしたファイル）

## 7. 残っていること

- リポジトリ直下に、誤って作られた空の Unity プロジェクトが残っています。削除するときはリポジトリの一番上で実行してください。
  ```bash
  git rm -r Library Logs Packages ProjectSettings UserSettings
  ```
- Katayama 配下など約30ファイルが Shift-JIS です（他の人の環境で文字化けします）。変換するときは、各ブランチの作業が落ち着いてからにしてください。
- トラップ（物）に貼ったステッカーの物理的な動きは、PCごとに少しずれることがあります。

## 8. メンバー向け連絡文（そのまま貼れます）

> オンラインのバグ修正を `Sugi_Nozomi` ブランチに上げました（main にはまだマージしていません）。
> 直したもの：マッチング（ID・人数・連続マッチング）、攻撃・ダメージ・ステッカーの同期、スポーン位置、3人時のクリアUI・HPバー・ネームプレート、敵に押されて浮く問題。
> PHPサーバーなしで試せる「オンラインテストモード」も追加しました（メニュー RedDanieru から切り替え）。
> 手順と注意点は `docs/online-handover.md` にまとめています。特に次の点に注意してください。
> ・ファイル名を直したので、該当ファイルを触っている人はマージ時に衝突するかもしれません
> ・敵のAIはホストだけで動く仕組みになりました
> ・`PlayerArmature` の `NameRoot` に Canvas を付けないでください
> main に入れる前に、みんなで一度3人プレイの確認をお願いします。
