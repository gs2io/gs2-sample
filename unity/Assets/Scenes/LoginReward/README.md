# ログインボーナス

GS2-LoginReward を使用してログインボーナス処理を実装します。
このサンプルでは7日でループし、毎日100の無償通貨を入手できますが、7日目だけ300の無償通貨が手に入るように設定されています。

## セットアップ方法

[GS2-Deploy テンプレート](template.yaml) を GS2-Deploy にアップロードし、リソースをセットアップします。

Login.unity を Unity Editor で開きます。
シーン内に配置された「GameObject」に割り当てられた「ClientId」「ClientSecret」に自身のプロジェクトのクライアントIDとクライアントシークレットを設定します。
