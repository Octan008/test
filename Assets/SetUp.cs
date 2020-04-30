using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetUp : MonoBehaviour
{
    public Vector3 box;
    public GameObject seed;
    public void init(){
        Vector3 scale=seed.transform.localScale;
        int child_num=this.transform.childCount;
        // Debug.Log(scale);
        for(int i=0; i< child_num; i++){
            // this.transform.GetChild(i).gameObject.name = i.ToString();
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }
        
        for(int ix=0; ix < box.x; ix++){
            for(int iy=0; iy<box.y; iy++){
                for(int iz=0; iz<box.z; iz++){
                    // Debug.Log(ix*(int)box.y*(int)box.z + iy*(int)box.z + iz+"a");
                    Instantiate(seed, (new Vector3(ix, iy, iz))*1.5f*(scale.x), Quaternion.identity, this.transform);
                    // Debug.Log(ix*(int)box.y*(int)box.z + iy*(int)box.z + iz+"b");
                    this.transform.GetChild(ix*(int)box.y*(int)box.z + iy*(int)box.z + iz).gameObject.name = ix.ToString()+iy.ToString()+iz.ToString();
                    // Debug.Log(ix*(int)box.y*(int)box.z + iy*(int)box.z + iz+"c");
                }
            }
        }
    }
    

    [ContextMenu("init")]
    void Method(){
        init();
    }
    // Start is called before the first frame update

    
    void Start()
    {
        init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
