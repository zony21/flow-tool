# 17_04_ProjectRole設計

## 1. 目的

Project単位のRole管理仕様を定義する。

## 2. 基本方針

- User単位ではなくProject参加単位でRoleを保持する。
- 同じUserでもProjectごとに異なるRoleを設定可能にする。
- Role変更は管理権限を持つ利用者のみ可能とする。

## 3. Role種類

Owner:
- Project管理
- Member管理
- 全操作

Editor:
- Flow編集
- Template編集

Viewer:
- 閲覧
- Export

## 4. 判定タイミング

- Project取得時
- Flow操作時
- API実行時

## 5. 完了条件

Project単位Role管理方針が定義されている。
