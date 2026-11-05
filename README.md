# Poyo's Editor Toolbox

VRChat のアバター改変で使う Unity エディタを少し使いやすくする機能をまとめたパッケージです。機能ごとにオン・オフできます。

使い方: <https://ghp.maaaaa.net/Editor-Toolbox/>

## 動作環境

- Unity 2022.3
- パッケージ全体として必要なパッケージはありません。次の機能だけ、ほかのパッケージが必要です。
  - VRChat SDK への自動入力: VRChat SDK - Avatars 3.10.0 以上、4.0.0 未満
  - 完了通知の「VRChat へのアップロード」: VRChat SDK 3.10.0 以上、4.0.0 未満
  - 完了通知の「Continuous Avatar Uploader の連続アップロード」: Continuous Avatar Uploader 0.3.8 以上、0.4.0 未満
  - 距離クリッピングキャンセラーの lilToon: lilToon 2.0.0 以上、3.0.0 未満
  - 距離クリッピングキャンセラーの Poiyomi で、ロック中のマテリアルを変える: ThryEditor 2.74.3 以上、3.0.0 未満

## インストール

1. VCC / ALCOM に `https://vpm.maaaaa.net/index.json` を追加します。
2. プロジェクトの Manage Packages で「Poyo's Editor Toolbox」を追加します。

VCC を使わない場合は、[リリースページ](https://github.com/Poyotoron/Editor-Toolbox/releases)の `.unitypackage` をインポートしてください。

## 設定

入れた直後は、どの機能もオフです。使う機能をオンにしてください。

| メニュー | 内容 |
|---|---|
| `Tools > Poyo's Editor Toolbox > Preferences…` | 全体設定。PC のすべてのプロジェクトで共通のオン・オフと、完了通知の設定 |
| `Tools > Poyo's Editor Toolbox > Project Settings…` | このプロジェクトだけの設定 |

## 機能

| 機能 | 内容 |
|---|---|
| Hierarchy のチェックボックス | Hierarchy の行の右端で、表示（Active）と EditorOnly を切り替える |
| Prefab Variant の名前 | Variant を作ったとき、名前の末尾に「Variant」を付けない |
| シーンのライティング | アバターの色を確かめやすいライティングでシーンを作る |
| VRChat SDK への自動入力 | 初めてアップロードするアバターの名前とサムネイルを入れる |
| Inspector を別ウィンドウで開く | 選んだオブジェクトに固定した Inspector を、別のウィンドウで開く |
| コピーを保存 | 保存先と名前を選んで、アセットのコピーを作る |
| ブレンドシェイプのスライダー | 下限と上限を決めたスライダーで、ブレンドシェイプを動かす |
| 完了通知 | アップロード、連続アップロード、もちふぃった～ の衣装変換、Play モード、インポートとコンパイルが終わったら、音と Webhook で知らせる |
| 距離クリッピングキャンセラー | lilToon と Poiyomi の距離クリッピングキャンセラーを、まとめて切り替える |
| Irodori Colorizer | Project と Hierarchy の行に色のラベルを付ける |

## ライセンス

MIT License（[LICENSE](LICENSE)）
