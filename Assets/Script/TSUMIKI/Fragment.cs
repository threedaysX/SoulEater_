using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Fragment", menuName = "Endowment/FagmentModel", order = 1)]
public class Fragment : ScriptableObject
{
    public F_Data m_Data;
}

public enum edgeTrigger { 上, 下, 左, 右 }

[System.Serializable]
public class neighborRelative
{
    public List<triggerInfo> relativeInfo = new List<triggerInfo>();
    public Affix theAffix;
    public bool check=false;
    public neighborRelative(neighborRelative copy)
    {
        relativeInfo = new List<triggerInfo>(copy.relativeInfo);
        theAffix = copy.theAffix;
        check = copy.check;
    }
}

[System.Serializable]
public class triggerInfo
{
    public Vector2 triPos;
    public edgeTrigger edgeT;
    public triggerInfo(triggerInfo copy)
    {
        triPos = copy.triPos;
        edgeT = copy.edgeT;
    }
}

//在寫touchStars(相對位置)時
//(0,0)一定要是正三角形!!!
[System.Serializable]
public class F_Data
{

    [Header("碎片名稱")]
    public string fName;
    [Header("碎片相對位置(原點自定)")]
    public List<Vector2> touchPos_v2;
    [Header("碎片顏色")]
    public Color fragColor;
    [Header("碎片圖片")]
    public Sprite fragImage;
    [Header("碎片索引圖片")]
    public Sprite fragIndexImage;

    //int edgeCount;
    //int triggerAffixIndex;

    [HideInInspector]
    public Vector2 centerAbsPos;
    [HideInInspector]
    public int fragmentID;
    [HideInInspector]
    public int triggerCount;

    //*****star(絕對位置)*****
    [HideInInspector]
    public List<int> touchStarsID = new List<int>();                          //用來記錄此碎片和哪個star交疊

    //*****相對位置*****
    [Header("組成碎片的三角形中，哪些邊一組是一個邊")]
    [SerializeField]
    public List<neighborRelative> neighborRelativeInfo = new List<neighborRelative>();  //此碎片邊上的鄰居關係 

    [HideInInspector]
    public List<NeighborInfo> neighbors = new List<NeighborInfo>();         //相對位置上 所有鄰居

    

    public void init(List<Vector2> _touchPos_v2, List<neighborRelative> _neighborRelativeInfo, int _id,Sprite _fragImage)   //回傳此Fragment在AllFragment的id
    {
        touchPos_v2 = _touchPos_v2;
        neighborRelativeInfo = _neighborRelativeInfo.ConvertAll(x => new neighborRelative(x));
        fragmentID = _id;
        fragImage = _fragImage;
        FindNeighbors();
        //NeighborRelative();
    }

    void FindNeighbors()////////////////////////////////////////////////////////////////////////////////////////////////////
    {
        neighbors.Clear();
        for (int i = 0; i < touchPos_v2.Count; i++)
        {
            if ((touchPos_v2[i].x + touchPos_v2[i].y) % 2 != 0)     //x+y是偶數 >> touchStars是 倒三角 >> 鄰居在左右上
            {
                NeighborFunc(new Vector2(touchPos_v2[i].x - 1, touchPos_v2[i].y), 1, 2);   //左邊的star pos 是正三角 觸發的邊是正三角的右邊
                NeighborFunc(new Vector2(touchPos_v2[i].x + 1, touchPos_v2[i].y), 1, 3);   //右邊的star pos 是正三角 觸發的邊是正三角的左邊
                NeighborFunc(new Vector2(touchPos_v2[i].x, touchPos_v2[i].y + 1), 1, 0);   //上邊的star pos 是正三角 觸發的邊是正三角的下邊  
            }
            else                                                        //x+y是奇數 >> touchStars是 正三角 >> 鄰居在左右下
            {
                NeighborFunc(new Vector2(touchPos_v2[i].x - 1, touchPos_v2[i].y), 0, 2);   //左邊的star pos 是倒三角 觸發的邊是倒三角的右邊
                NeighborFunc(new Vector2(touchPos_v2[i].x + 1, touchPos_v2[i].y), 0, 3);   //右邊的star pos 是倒三角 觸發的邊是倒三角的左邊
                NeighborFunc(new Vector2(touchPos_v2[i].x, touchPos_v2[i].y - 1), 0, 1);   //下邊的star pos 是倒三角 觸發的邊是倒三角的上邊
            }
        }
    }
    void NeighborFunc(Vector2 Star_pos, int _tri, int _to)//////////////////////////////////////////////////////////////////
    {
        int id = touchPos_v2.FindIndex(x => x == Star_pos);  //檢查是否屬於此碎片的一部分
        if (id != -1)       //此碎片有此點
            return;         //跳過

        NeighborInfo temp = new NeighborInfo(neighbors.Count, Star_pos, _tri, _to);
        neighbors.Add(temp);
    }

    #region Old
    /*
    void NeighborRelative()//////////////////////////////////////////////////////////////////////////////////////////////////
    {
        List<NeighborInfo> nei_D_U = new List<NeighborInfo>();//存放 下三角形的鄰居中，以 上邊 觸發的三角形
        List<NeighborInfo> nei_D_R = new List<NeighborInfo>();//                          右邊
        List<NeighborInfo> nei_D_L = new List<NeighborInfo>();//                          左邊
        List<NeighborInfo> nei_U_L = new List<NeighborInfo>();//     上三角               左邊
        List<NeighborInfo> nei_U_R = new List<NeighborInfo>();//                          右邊
        List<NeighborInfo> nei_U_D = new List<NeighborInfo>();//                          下邊

        //分六大類 nei_D_U、nei_D_R、nei_D_L、nei_U_L、nei_U_R、nei_U_D
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (neighbors[i].tri == 0)  //下三角形們
            {
                switch (neighbors[i].toggleEdge)
                {
                    case 1:
                        nei_D_U.Add(neighbors[i]);
                        break;
                    case 2:
                        nei_D_R.Add(neighbors[i]);
                        break;
                    case 3:
                        nei_D_L.Add(neighbors[i]);
                        break;
                }
            }
            else
            {
                switch (neighbors[i].toggleEdge)
                {
                    case 3:
                        nei_U_L.Add(neighbors[i]);
                        break;
                    case 2:
                        nei_U_R.Add(neighbors[i]);
                        break;
                    case 0:
                        nei_U_D.Add(neighbors[i]);
                        break;
                }
            }
        }

        //排序 nei_D_U >>以x座標大小排序，由小到大
        bubbleSort(nei_D_U);
        bubbleSort(nei_U_D);
        bubbleSort(nei_D_R);
        bubbleSort(nei_D_L);
        bubbleSort(nei_U_L);
        bubbleSort(nei_U_R);


        CutEdge(nei_D_U, "nei_D_U");    //只要中間斷掉 即得到一個邊
        CutEdge(nei_U_D, "nei_U_D");
        CutEdge(nei_D_R, "nei_D_R");
        CutEdge(nei_D_L, "nei_D_L");
        CutEdge(nei_U_L, "nei_U_L");
        CutEdge(nei_U_R, "nei_U_R");

    }
    */
    /*void CutEdge(List<NeighborInfo> list, string _n)////////////////////////////////////////////////////////////////////////////////
    {
        if (list.Count == 0) return;

        neighborRelative temp=new neighborRelative();
        temp.neighborRelative_id.Add(list[0].neighborInfoID);

        if (list.Count == 1)
        {
            neighborRelativeInfo.Add(temp);
            return;
        }

        for (int i = 1; i < list.Count; i++)
        {
            bool juge = false;
            //nei_D_U   >>判斷連續方式  >>x+2
            //nei_U_D   >>判斷連續方式  >>x+2
            //nei_D_R   >>判斷連續方式  >>x+1，y+1
            //nei_D_L   >>判斷連續方式  >>x+1，y-1
            //nei_U_L   >>判斷連續方式  >>x+1，y+1
            //nei_U_R   >>判斷連續方式  >>x+1，y-1

            switch (_n)
            {
                case "nei_D_U":
                    juge = (list[i].pos.x == list[i - 1].pos.x + 2);
                    break;
                case "nei_U_D":
                    juge = (list[i].pos.x == list[i - 1].pos.x + 2);
                    break;
                case "nei_D_R":
                    juge = (list[i].pos.x == list[i - 1].pos.x + 1) && (list[i].pos.y == list[i - 1].pos.y + 1);
                    break;
                case "nei_U_L":
                    juge = (list[i].pos.x == list[i - 1].pos.x + 1) && (list[i].pos.y == list[i - 1].pos.y + 1);
                    break;
                case "nei_D_L":
                    juge = (list[i].pos.x == list[i - 1].pos.x + 1) && (list[i].pos.y == list[i - 1].pos.y - 1);
                    break;
                case "nei_U_R":
                    juge = (list[i].pos.x == list[i - 1].pos.x + 1) && (list[i].pos.y == list[i - 1].pos.y - 1);
                    break;
                default:
                    Debug.LogError("error");
                    break;
            }

            if (juge)
            {
                temp.neighborRelative_id.Add(list[i].neighborInfoID);
            }
            else//斷掉了
            {
                neighborRelativeInfo.Add(temp);
                temp.neighborRelative_id.Clear();
                temp.neighborRelative_id.Add(list[i].neighborInfoID);
            }
            if (i == list.Count - 1)        //最後一個
                neighborRelativeInfo.Add(temp);


        }
    }
    */
    /*void bubbleSort(List<NeighborInfo> list)//////////////////////////////////////////////////////////// //以x座標大小排序，由小到大
    {
        if (list.Count <= 1) return;
        for (int i = 0; i < list.Count; i++)
            for (int j = i + 1; j < list.Count; j++)
            {
                if (list[i].pos.x > list[j].pos.x)
                {
                    //swap
                    NeighborInfo temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }
    }*/
    #endregion

    private Player player;
    public string PrintAndExeAffixs(string fragName) {
        string reternString = "";
        for (int i = 0; i < neighborRelativeInfo.Count; i++)
        {
            if (player == null)
            {
                player = GameObject.Find("Player").GetComponent<Player>();
            }
            Affix affix = neighborRelativeInfo[i].theAffix;
            affix.owner = player;
            affix.ResetAffectName(fragName + i.ToString());
            affix.Remove();
            if (neighborRelativeInfo[i].check)
            {
                //執行Affix
                //Test.Instance.AddBuffs(neighborRelativeInfo[i].theAffix.name);               
                affix.Trigger();
                reternString += " [" + neighborRelativeInfo[i].theAffix.description + "] ";
            }
        }
        if (reternString == "")
            return fName+" : 無";
        //return  "此碎片"+ fName+"觸發了" + triggerCount+"條邊";
        return fName + " : "+reternString;
    }

    public void ResetAffixOnStart()
    {
        for (int i = 0; i < neighborRelativeInfo.Count; i++)
        {
            Affix affix = neighborRelativeInfo[i].theAffix;
            affix.OnStart();
        }
    }

    public string GetFragAllAffixsInfo()
    {
        string resultString = "";
        for (int i = 0; i < neighborRelativeInfo.Count; i++)
        {
            resultString += (i + 1) + ". " + neighborRelativeInfo[i].theAffix.description + "\n\n";
        }
        return fName + "\n\n" + resultString;
    }
}



//[System.Serializable]
public class NeighborInfo//////////////////////////////////////////////////////////////////////////// OK
{
    public int neighborInfoID;  //自己在neighbors是第幾個
    public int starID;
    public Vector2 pos;
    public int tri;            //UpTriangle=1  //DownTriangle=0
    public int toggleEdge;     //Up    1       //Down   0          //Left   3          //Right  2
    public NeighborInfo(int _neighborInfoID, Vector2 _pos, int _tri, int _to)
    {
        neighborInfoID = _neighborInfoID;
        pos = _pos;
        tri = _tri;
        toggleEdge = _to;
    }
}