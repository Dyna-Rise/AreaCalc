# 2Dパズルで背景に対して投下したブロックの面積占有率を計算するプロジェクト
![概要画像](images/img1.png)
  
## ヒエラルキー構成
* AreaManager（CreateEmpty） ← AreaManagerDynamic.cs
　- back(背景オブジェクト）← PolygonCollider2Dの所持が必要
* OjbGenerator (CreateEmpty) ← ObjGenerator.cs
  
## パズルブロックはプレハブ化 （OjbGenerator.csで順番に呼び出される）
* サンプルでは何種類かの図形 ← PolygonCollider2Dの所持が必要

  
##スクリプト概要

  
### OjbGenerator.cs
パズルブロックの生産者
GameObject配列 blockPrefabsに登録したオブジェクトをランダムに選んで画面上部に待機させる
  
* ChooseNextメソッド
プレハブを選定してInstantiate、MakepreviewStyle()で半透明にする

* HandleMoveInputメソッド
プレハブが生成済みであればHorizontalに応じてプレハブの左右の座標を変更する
ただし、minX変数とmaxX変数にとどまるよう位置はClampメソッドで制御されている

* HandleDropInputメソッド
マウスの左クリックイベント待って、DropCurrentAndChooseNext()を発動
  
* DropCurrentAndChooseNextメソッド
MakeRealStyle()でプレビューをやめて重力を有効にする
同時にAreaManagerオブジェクトのスクリプト(AreaManagerDynamic.cs)のblocksリストにPolygonCollider2Dの情報を登録させる
次にそなえてChooseNext()を最後に発動

* MakePreviewStyle()メソッド
半透明にして重力を無効にする

* MakeRealStyleメソッド
半透明を解いて重力を有効に戻す

  
### AreaManagerDynamic.cs
using Clipper2Lib によりClipper2ライブラリの機能を活用して  
①背景オブジェクトの面積
②OjbGeneratorより登録されたプレハブたちの面積合計
を算出する
①、②の割合から面積占有率をUIに表示する
  
StartでClipper.Area()を使い、背景の面積を計算しておく（変数：regionAreaScaled)
UpdateではつねにIntersectArea()で算出したオブジェクトの面積を変数に合算していく
※blocksリストのオブジェクトだけforeachで繰り返して変数に合算 (変数：occupied)

* AddBlockメソッド
プレハブが投下される度にOjbGenerator.cs側から呼び出される
投下されたプレハブをblocksリストに加える
  
* IntersectAreaメソッド
Clipper2ライブラリを活用した面積計算
1. if (block.attachedRigidbody.velocity.sqrMagnitude > 0.05f) return
引数に与えたblock(PolygonCollider2D情報）の動きが収まった時
2. clipper.Clear()～clipper.AddClip(ToPathD(block))
引数のblock(PolygonCollider2D情報）をClipper2が理解できる形に変換しながらセッティング
3. clipper.Execute(ClipType.Intersection, FillRule.NonZero, sol)
セッティングした対象の形をあらためて認識、もし複数に区切られて認識されるようなら複数形として情報取得
4. foreach (var path in sol) a += Clipper.Area(path)
(3.)で取得した情報に対してClipper.Area()で面積を取得、retrunする

* ToPathDメソッド
対象のオブジェクトのPolygonCollider2Dの情報をClipper2ライブラリが処理できる型に変換する
ざっくり説明すると、PathD型の受け皿となる変数pを作成、
一方ワールド座標に変換したPolygonCollider2Dの頂点情報(変数w)のスケールを1000倍にしておき、
PathD型の受け皿pにAddメソッドでwを情報格納させる （PolygonCollider2Dがもつ頂点座標情報をClipper2Dが扱えるdouble型の構造体に変換）
このメソッドを通すことでClipper2D関連のメソッドの引数として渡せるようになっていく
  
