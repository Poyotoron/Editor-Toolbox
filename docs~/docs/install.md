# インストール

## 動作環境

| 項目 | 要件 |
|---|---|
| Unity | 2022.3 |
| VRChat SDK | 不要です |

パッケージ全体として必要なパッケージはありません。次の機能だけ、ほかのパッケージが必要です。

| 機能 | 必要なパッケージ |
|---|---|
| VRChat SDK への自動入力 | VRChat SDK - Avatars 3.10.0 以上、4.0.0 未満 |
| 完了通知の「VRChat へのアップロード」 | VRChat SDK 3.10.0 以上、4.0.0 未満 |
| 完了通知の「Continuous Avatar Uploader の連続アップロード」 | Continuous Avatar Uploader 0.3.8 以上、0.4.0 未満 |

## VCC / ALCOM から入れる

1. VCC または ALCOM に `https://vpm.maaaaa.net/index.json` を追加します。
2. プロジェクトの Manage Packages を開き、「Poyo's Editor Toolbox」を追加します。

## .unitypackage から入れる

[リリースページ](https://github.com/Poyotoron/Editor-Toolbox/releases)から `.unitypackage` をダウンロードし、Unity にドラッグ＆ドロップしてインポートします。

## 開き方

入れると、次のメニューができます。

```
Tools > Poyo's Editor Toolbox > Preferences…
Tools > Poyo's Editor Toolbox > Project Settings…
```

入れた直後は、どの機能もオフです。`Preferences…` で使う機能をオンにしてください（[設定画面](settings.md)）。

## 単体版の Irodori Colorizer が入っているとき

単体版の Irodori Colorizer が入っている間は、二重に色が付かないように、このパッケージの色付けを止めます。単体版を削除すると、付けた色と設定を引き継いで、このパッケージの色付けが動きます。

## 更新する

VCC / ALCOM で更新するか、新しい `.unitypackage` をインポートします。設定は更新しても残ります。
