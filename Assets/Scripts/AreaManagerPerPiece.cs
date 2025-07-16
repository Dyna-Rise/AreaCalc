using System.Collections.Generic;
using UnityEngine;
using Clipper2Lib;

public class AreaManagerDynamic : MonoBehaviour
{
    public PolygonCollider2D regionCollider; //母体となるコリジョン

    public TMPro.TextMeshProUGUI txtTotal; //UI出力先

    private const double SCALE = 1000.0;

    // 動的に追加されるプレハブたちのコライダー情報をリスト化
    private readonly List<PolygonCollider2D> blocks = new();

    // 領域情報をキャッシュ
    private PathD regionPath;
    private double regionAreaScaled;
    private ClipperD clipper = new();

    void Start()
    {
        regionPath = ToPathD(regionCollider); //ToPathD 自作メソッド Clipperライブラリが扱える形に変換
        regionAreaScaled = Clipper.Area(regionPath); //Clipperライブラリの機能でまずは背景の面積を計算
    }

    void Update()
    {
        double occupied = 0;

        // ブロック数が増えても O(N) で回すだけ
        foreach (var b in blocks)
            occupied += IntersectArea(regionPath, b);

        //占有率を掲載
        double rateTotal = occupied / regionAreaScaled * 100.0;

        //占有率をUI表示
        if (txtTotal) txtTotal.text = $"Rate:{rateTotal:0.0}%";
    }

    //ジェネレーターがプレハブを生成した時にリストに追加
    public void AddBlock(PolygonCollider2D blk)
    {
        //登録されていなければリストに追加
        if (blk && !blocks.Contains(blk))
            blocks.Add(blk);
    }

    //プレハブが画面外に落ちた際にリストから消去
    public void RemoveBlock(PolygonCollider2D blk)
    {
        if (blk) blocks.Remove(blk);
    }

    //PolygonCollider2Dの持つ頂点をClipper2が扱えるPathD型(doble座標のパス）に変換
    PathD ToPathD(PolygonCollider2D col)
    {
        var local = col.GetPath(0); //コライダーの0番目のパスを取得 ※大体は1つのパスで図形を囲ってしまうので１つしかない = 0番目の情報取得でほぼ固定のため

        PathD p = new(local.Length); //取得したパスの頂点数だけあらかじめ容量確保（まだ情報は空）

        foreach (var v in local)
        {
            Vector2 w = col.transform.TransformPoint(v); //ローカル座標情報をワールド座標に変換
            p.Add(new PointD(w.x * SCALE, w.y * SCALE)); //変換したワールド座標を1000倍して正式にpに情報格納、これでPathD型に変換された各座標として、Clipperが扱える値になる
            //Clipperは整数または十分大きい値でないと誤差が丸められてしまう可能性があるので
            //分子も分母も1000倍しておく
        }
        return p; //Clipperが扱えるPathD型の情報として返す
    }

    //Clipper2ライブラリを使って面積計算
    double IntersectArea(PathD region, PolygonCollider2D block)
    {
        if (block == null) return 0;

        // 速度が十分小さいときだけ正確に計算（負荷削減）
        if (block.attachedRigidbody.velocity.sqrMagnitude > 0.05f)
            return 0;

        clipper.Clear(); //前の計算に使ったインスタンスをリセット
        clipper.AddSubject(region); //PathD型の変数を再利用
        clipper.AddClip(ToPathD(block)); //指定したコライダーをPathD型に変換してクリップに固定

        PathsD sol = new();

        //実際に ブール演算：Intersection を実行し形を認識、結果を solution（複数パスもあり得る）
        clipper.Execute(ClipType.Intersection, FillRule.NonZero, sol);

        double a = 0;

        //交差結果が複数ポリゴンに分かれる場合があるので 全部の面積を合算
        foreach (var path in sol) a += Clipper.Area(path);

        return a; //面積の計案結果を返す
    }
}
