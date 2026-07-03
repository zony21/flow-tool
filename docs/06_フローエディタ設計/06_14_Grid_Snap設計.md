# 06_14_Grid_Snap設計

## 1. 本書の目的

GridとSnapの設計仕様を定義する。
Canvas上のNode配置を見やすく整え、設計図としての可読性を高める。

## 2. Grid

Canvas背景にGridを表示できる。
Grid表示はユーザー設定でON/OFF可能とする。

## 3. Snap

Node移動時にGridへ吸着できる。
Snapはユーザー設定でON/OFF可能とする。

## 4. 設定項目

- gridEnabled
- snapEnabled
- gridSize

## 5. 保存対象

GridとSnap設定はUserSettingに保存する。
Node座標はSnap後の値を保存する。

## 6. テスト観点

- Gridを表示できること
- Snap有効時に座標が補正されること
- 設定が保存されること

## 7. 完了条件

GridとSnapによりNode配置を整えられること。
