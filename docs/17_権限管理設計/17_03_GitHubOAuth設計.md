# 17_03_GitHubOAuth設計

## 1. 目的

GitHub OAuthを利用したログイン処理方針を定義する。

## 2. 基本方針

- 外部認証としてGitHubを利用する。
- アプリ内部では独自User情報を保持する。
- 外部IDと内部IDを分離する。

## 3. Login Flow

1. GitHub認証
2. User情報取得
3. 内部User確認
4. Session発行
5. 利用開始

## 4. 管理項目

- Provider
- Provider User ID
- User ID

## 5. 完了条件

外部認証と内部User管理が分離されている。
