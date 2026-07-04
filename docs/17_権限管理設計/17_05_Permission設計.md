# 17_05_Permission設計

## 1. 目的

Roleから具体的な操作可否を判断するPermission仕様を定義する。

## 2. 基本方針

- Roleと操作判定を分離する。
- APIではPermission単位で確認する。
- 将来的な細分化へ対応する。

## 3. Permission例

Project:
- Read
- Update
- Delete

Flow:
- Read
- Create
- Update
- Delete

Template:
- Read
- Apply
- Update

## 4. 完了条件

RoleとPermissionの責務が分離されている。
