using UnityEngine;

public class ObjGenerator : MonoBehaviour
{
    public GameObject[] blockPrefabs; //生成プレハブ

    public Vector2 spawnPoint = new(0, 0); // 初期 y は0固定、x は Update で変わる

    //移動
    public float moveSpeed = 5f;          // 左右移動速度 (unit/sec)
    public float minX = -5f;              // 移動制限　左
    public float maxX = 5f;               // 移動制限　右

    public AreaManagerDynamic areaManager; //対象のエリア

    /* ----- 内部状態 ----- */
    private GameObject nextPreview;        // プレビュー座標
    private float previewX;                // プレビューの現在 x 座標

    void Start()
    {        
        ChooseNext();//最初のプレハブを選定
    }

    void Update()
    {
        HandleMoveInput(); //ジェネレーターの移動
        HandleDropInput(); //マウスイベント待ち
    }

    //プレハブの移動
    void HandleMoveInput()
    {
        if (nextPreview == null) return;

        float h = Input.GetAxisRaw("Horizontal");          // ←→ / A・D キー
        if (Mathf.Abs(h) > 0.01f)
        {
            previewX += h * moveSpeed * Time.deltaTime; //Xの理論値を計算
            previewX = Mathf.Clamp(previewX, minX, maxX); // Xの理論値に範囲制限をかける

            Vector2 pos = nextPreview.transform.position; //プレハブの現在地を取得
            pos.x = previewX; //プレビューのX座標は理論値に差し替え
            nextPreview.transform.position = pos; //生成したプレハブの位置を変更
        }
    }

    
    //マウスイベント待ち
    void HandleDropInput()
    {
        if (Input.GetMouseButtonDown(0))
            DropCurrentAndChooseNext(); //マウスが押されるとドロップ＆次の選定
    }

    //次のプレハブ選定
    void ChooseNext()
    {
        //ランダムにチョイスして生成（nextPreviewに登録)
        int rand = Random.Range(0, blockPrefabs.Length);
        nextPreview = Instantiate(blockPrefabs[rand],
                                  spawnPoint,
                                  Quaternion.identity);

        previewX = spawnPoint.x;          // X を初期化
        MakePreviewStyle(nextPreview); //生成したプレハブをプレビュー表示（半透明）
    }

    //ドロップ＆次の選定
    void DropCurrentAndChooseNext()
    {
        if (nextPreview == null) return;
        
        MakeRealStyle(nextPreview); //プレビュー表示をやめて投下

        var col = nextPreview.GetComponent<PolygonCollider2D>(); //ポリゴンコライダーの取得

        if (col) areaManager.AddBlock(col); //エリアマネージャーのリストに投下したコライダーを追加
        nextPreview = null;               // 投下完了

        ChooseNext();                     // 次のプレビュー生成
    }


    //生成プレハブをプレビュー表示（半透明）にして止める
    void MakePreviewStyle(GameObject go)
    {
        foreach (var r in go.GetComponentsInChildren<SpriteRenderer>())
        {
            var c = r.color; c.a = 0.4f; r.color = c; //半透明にする
        }
        go.GetComponent<Rigidbody2D>().simulated = false; //rigidbodyを停止(重力を復活）
    }

    //生成プレハブのプレビュー表示をやめて（不透明）投下
    void MakeRealStyle(GameObject go)
    {
        foreach (var r in go.GetComponentsInChildren<SpriteRenderer>())
        {
            var c = r.color; c.a = 1f; r.color = c; //透明度を直す
        }
        go.GetComponent<Rigidbody2D>().simulated = true; //rigidbodyを復活(重力を復活）
    }
}
