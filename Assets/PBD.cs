using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class Constraint{

// }

public class PBD : MonoBehaviour
{   
    /************/
    /* 初期設定 */
    /**********/

    //地面
    public GameObject Ground;
    //減衰率、空気抵抗？
    public float damp=0.99f;//(%)
    //重力加速度
    public float gravity=9.8f;//(m/s2)
    public float k_ground=0.8f;
    public float k_dist=1.0f;

    public float el = 0.2f;

    /*********************/
    /* 固定変数、関連関数 */
    /*******************/

    //幾何拘束
    List<(GameObject, GameObject,float)> Ms = new List<(GameObject, GameObject, float)>();
    bool[] check;

    void Collect_Ms(
        int limx, int limy, int limz, GameObject start
    ){
      
        Collect_Ms_recursive(limx, limy, limx, start);
    }
    void Collect_Ms_recursive(
        int limx, int limy, int limz, GameObject start
    ){
        int x = int.Parse(start.name[0].ToString());
        int y = int.Parse(start.name[1].ToString());
        int z = int.Parse(start.name[2].ToString());

        int i = (x*(limy+1)*(limz+1) + y*(limz+1) + z);
        // Debug.Log(i);
        
        if(!check[i]){
            check[i] = true;
            if(x < limx){
                Ms.Add(
                    (
                        start, mp(i + (limy+1)*(limz+1)), 1.5f
                        )
                    );
                Collect_Ms_recursive(limx, limy, limx, mp(i + (limy+1)*(limz+1)));
            }
            if(y < limy){
                Ms.Add(
                    (
                        start, mp(i + (limz+1)), 1.5f
                        )
                    );
                Collect_Ms_recursive(limx, limy, limx, mp(i + (limz+1)));

            }
            if(z < limz){
                Ms.Add(
                    (
                        start, mp(i + 1), 1.5f
                        )
                    );
                Collect_Ms_recursive(limx, limy, limx, mp(i + 1));
            }

            if(x < limx && y < limy){
                Ms.Add(
                    (
                        start, mp(i + (limz+1) +  (limy+1)*(limz+1)), Mathf.Sqrt(1.5f*1.5f*2)
                        )
                    );
            }

            if(z < limz && y < limy){
                Ms.Add(
                    (
                        start, mp(i + 1 + (limz+1)), Mathf.Sqrt(1.5f*1.5f*2)
                        )
                    );
            }

            if(z < limz && x < limx){
                Ms.Add(
                    (
                        start, mp(i + 1 + (limy+1)*(limz+1)), Mathf.Sqrt(1.5f*1.5f*2)
                        )
                    );

            }
        }
    }

    /************/
    /* 保持変数 */
    /***********/

    //現在位置
    Dictionary<GameObject, Vector3> defaultPos= new Dictionary<GameObject, Vector3>();
    //前フレームの位置
    Dictionary<GameObject, Vector3> previousPos= new Dictionary<GameObject, Vector3>();
    //速度
    Dictionary<GameObject, Vector3> Velocity= new Dictionary<GameObject, Vector3>();

    Vector3 SolveCollide_Plane( KeyValuePair<GameObject, (Vector3 p, Vector3 qc, Vector3 nc)> Mcoll){
        Vector3 p = Mcoll.Value.p; Vector3 qc = Mcoll.Value.qc; Vector3 nc = Mcoll.Value.nc;

        return -(Vector3.Dot(p-qc, nc)/Vector3.Dot(nc, nc)) * nc;
    }


    GameObject mp(int i){
        return this.transform.GetChild(i).gameObject;
    }
    bool if_Collide_Plane(GameObject Plane, Vector3 x, Vector3 p){
        //static
        if(x.y < Plane.transform.position.y){
            return true;
        }
        //continuous
        else if(p.y < Plane.transform.position.y){
            return true;
        }
        else{
            return false;
        }
    }
    Vector3 detect_qc(GameObject Plane, Vector3 x, Vector3 p){
        //static
        if(x.y < Plane.transform.position.y){
            return new Vector3(p.x, Plane.transform.position.y, p.z);
        }
        //continuous
        else{
            // //https://qiita.com/edo_m18/items/c8808f318f5abfa8af1e
            Vector3 n = Vector3.up; float h = Vector3.Dot(x, n);
            Vector3 m = p - x;

            float rate = (h - Vector3.Dot(x, n))/(Vector3.Dot(m, n));

            return x + rate*m;
        }
    }
    bool ViolateGeometry_Plane(GameObject Plane, GameObject Sphere, Vector3 p){
        if(Plane.transform.position.y > p.y - (Sphere.transform.localScale.x)/2){
            Debug.Log("Violate");
            return true;
        }
        else{
            return false;
        }
    }
    Vector3 SolveGeometry_Plane(GameObject Plane, GameObject Sphere, Vector3 p){
        float k = k_ground;

        Vector3 p1 = p;
        Vector3 p2 = p1; p2.y = Plane.transform.position.y;
        float d = Sphere.transform.localScale.x;
        Vector3 dp1;
        if((p1-p2).magnitude != 0){
             dp1= -((p1-p2).magnitude - d) * ( (p1-p2) / (p1-p2).magnitude);
        }
        else{
            //これどうするんだ？
            dp1= -((p1-p2).magnitude - d) * Vector3.up;
        }
        return dp1*k;
    }

    // bool ViolateGeometry_Dist(Vector3 p1, Vector3 p2 ,float d){
    //     return true;
    //     // float d = 1.5f;        
    //     if((p1-p2).magnitude < d){
    //         Debug.Log("Violate");
    //         return true;
    //     }
    //     else{
    //         return false;
    //     }
    // }
    bool ViolateGeometry_Dist(Vector3 p1, Vector3 p2 ,float d){ 
        if((p1-p2).magnitude < d*(1.0f-el) || d*(1.0f+el) < (p1-p2).magnitude){
            Debug.Log("Violate");
            return true;
        }
        else{
            return false;
        }
    }
    // (Vector3, Vector3) SolveGeometry_Dist(Vector3 p1, Vector3 p2, float d){
    //     float k = k_dist;

    //     // float d = 1.5f;
    //     Vector3 dp1;
    //     Vector3 dp2;
    //     if((p1-p2).magnitude != 0){
    //         dp1= -((p1-p2).magnitude - d) * 0.5f * ( (p1-p2) / (p1-p2).magnitude);
    //         dp2= ((p1-p2).magnitude - d) * 0.5f * ( (p1-p2) / (p1-p2).magnitude);
    //     }
    //     else{
    //         //これどうするんだ？
    //         Debug.Log("warning");
    //         dp1= -((p1-p2).magnitude - d) * Vector3.up;
    //         dp2= -((p1-p2).magnitude - d) * Vector3.down;
    //     }
    //     return (dp1*k, dp2*k);
    // }
    (Vector3, Vector3) SolveGeometry_Dist(Vector3 p1, Vector3 p2, float d){
        float k = k_dist;
        if((p1-p2).magnitude < d*(1.0f-el)) d = d*(1.0f-el);
        else d =d*(1.0f+el);

        // float d = 1.5f;
        Vector3 dp1;
        Vector3 dp2;
        if((p1-p2).magnitude != 0){
            dp1= -((p1-p2).magnitude - d) * 0.5f * ( (p1-p2) / (p1-p2).magnitude);
            dp2= ((p1-p2).magnitude - d) * 0.5f * ( (p1-p2) / (p1-p2).magnitude);
        }
        else{
            //これどうするんだ？
            Debug.Log("warning");
            dp1= -((p1-p2).magnitude - d) * Vector3.up;
            dp2= -((p1-p2).magnitude - d) * Vector3.down;
        }
        return (dp1*k, dp2*k);
    }
    void Start()
    {
        //全質点の初期位置を取得
        for(int i=0; i< this.transform.childCount; i++){;
            defaultPos.Add(mp(i), mp(i).transform.position);
            previousPos.Add(mp(i), mp(i).transform.position);
        }

        string LastName=mp(this.transform.childCount-1).gameObject.name;
        check = new bool[this.transform.childCount];
        for(int i=0; i<check.Length; i++) check[i] = false;

        Collect_Ms(int.Parse(LastName[0].ToString()),
                    int.Parse(LastName[1].ToString()),
                        int.Parse(LastName[2].ToString()),
                                                        mp(0));  

        // foreach( (GameObject, GameObject, float) M in Ms){
        //     Debug.Log(M.Item1.name+", "+M.Item2.name+", "+M.Item3);
        // }

    }

    // Update is called once per frame
    void Update()
    {
        // foreach( (GameObject, GameObject, float) M in Ms){
        //     Debug.Log(M.Item1.name+", "+M.Item2.name+", "+
        //         (M.Item1.transform.position - M.Item2.transform.position).magnitude+", "+
        //         M.Item3
        //     );
        // }

        //予測位置
        Dictionary<GameObject, Vector3> predictPos  =  new Dictionary<GameObject, Vector3>();
        //予測位置からの修正量
        Dictionary<GameObject, Vector3> deltaPos = new Dictionary<GameObject, Vector3>();

        /************************************************/
        /* 各質点についてExplicit Euler法で次の位置を予測 */
        /************************************************/
        for(int i=0; i< this.transform.childCount; i++){
            //対象の質点
            // GameObject mp= this.transform.GetChild(i).gameObject;

            //位置から速度を導く
            Velocity[mp(i)]=(defaultPos[mp(i)]-previousPos[mp(i)])/Time.deltaTime;
            // if(Velocity[mp(i)].magnitude < 0.5f){
            //     Velocity[mp(i)] = Vector3.zero;
            // }

            //重力加速度を反映させて次フレームの速度を更新
            Velocity[mp(i)] += Vector3.down * gravity * Time.deltaTime;

            //減衰率を反映させて次フレームの速度を更新
            Velocity[mp(i)] *= damp;

            //現在位置を前フレーム位置として記録(時間を進める)
            previousPos[mp(i)] = defaultPos[mp(i)];

            //予測位置を速度から更新
            predictPos.Add(mp(i), defaultPos[mp(i)] + Velocity[mp(i)]*Time.deltaTime);

            //予測位置修正なし(初期化)
            deltaPos.Add(mp(i), Vector3.zero);     
        }

        /************/
        /* 衝突検知 */
        /************/
        Dictionary<GameObject, (Vector3 p, Vector3 qc, Vector3 nc)> Mcolls =
                             new Dictionary<GameObject, (Vector3 p, Vector3 qc, Vector3 nc)>();
        for(int i=0; i< this.transform.childCount; i++){
            if(
                if_Collide_Plane(Ground, defaultPos[mp(i)], predictPos[mp(i)])
            )
            {
                Mcolls.Add(
                    mp(i), 
                    (
                        p:predictPos[mp(i)], 
                        qc:detect_qc(Ground, defaultPos[mp(i)], predictPos[mp(i)]), 
                        nc:Vector3.up
                    )
                );
            }
        }

        //newton法
        bool flag=true;
        
        int count = 0;
        
        while(flag){
            count+=1;
            Dictionary<GameObject, Vector3> prev_predictPos = new Dictionary<GameObject, Vector3>(predictPos);
            //衝突拘束
            foreach( KeyValuePair<GameObject, (Vector3 p, Vector3 qc, Vector3 nc)> Mcoll in Mcolls){
                if(if_Collide_Plane(Ground, defaultPos[Mcoll.Key], predictPos[Mcoll.Key])){
                    deltaPos[Mcoll.Key] = SolveCollide_Plane(Mcoll);
                    predictPos[Mcoll.Key]  += deltaPos[Mcoll.Key];
                }
            }
            // //更新
            // for(int i=0; i< this.transform.childCount; i++){
            //     predictPos[mp(i)]  += deltaPos[mp(i)];
            // }

            //幾何拘束
            //これ本当に質点ごとでいいの？
            for(int i=0; i< this.transform.childCount; i++){
                if(ViolateGeometry_Plane(Ground, mp(i), predictPos[mp(i)])){
                    deltaPos[mp(i)] = SolveGeometry_Plane(
                        Ground, mp(i), predictPos[mp(i)]
                    );
                    predictPos[mp(i)]  += deltaPos[mp(i)];
                }
            }
            // //更新
            // for(int i=0; i< this.transform.childCount; i++){
            //     predictPos[mp(i)]  += deltaPos[mp(i)];
            // }
            foreach( (GameObject, GameObject, float) M in Ms){
                if(ViolateGeometry_Dist(
                    predictPos[M.Item1], predictPos[M.Item2], M.Item3
                    )){
                        (Vector3 d1, Vector3 d2)= 
                            SolveGeometry_Dist(
                                predictPos[M.Item1], predictPos[M.Item2], M.Item3
                            );
                        deltaPos[M.Item1] = d1;
                        deltaPos[M.Item2] = d2;

                        predictPos[M.Item1] += deltaPos[M.Item1];
                        predictPos[M.Item2] += deltaPos[M.Item2];
                }
            }   
            

            //判定
            float e = 0.1f;
            flag=false;
            if(count > 10){
                Debug.Log("Clash");
                break;
            }
            for(int i=0; i< this.transform.childCount; i++){
                // Debug.Log(predictPos[mp(i)] - prev_predictPos[mp(i)]);
                if((predictPos[mp(i)] - prev_predictPos[mp(i)]).magnitude > e){
                    // Debug.Log("繰り返します");
                    flag=true;
                    break;
                }
            }
        }

        //最終反映
        Debug.Log("count:"+count);
        for(int i=0; i< this.transform.childCount; i++){
            defaultPos[mp(i)]=predictPos[mp(i)];
            mp(i).transform.position = defaultPos[mp(i)];
        }        
    }
}
