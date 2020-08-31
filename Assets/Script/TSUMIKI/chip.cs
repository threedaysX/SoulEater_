using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/*
碎片限制
1.三角形擺放不得超過三層
2.碎片必連貫且不得中空
*/
/*
 程式碼講解

 使用方法:
 這個檔案chip.cs是給任何形狀的碎片使用
 只要將那個碎片物件裝上polygon collider 2D和Rigidbody 2D和此chip即可使用
 
 原理:
 碎片擺定位置後，根據佔用哪些星星點，即可知道碎片的形狀
 目前的座標表示方式如圖(星星座標.jpg)
 可知 當   x+y是偶數  即為  下三角  產生的邊有  左右上
                奇數        上三角              左右下
 
 當碎片更改位置或形狀(任何會更改使用星星的狀況)後，需手動呼叫newStart()重新計算
 
    
 執行newStart()後，會自動往下依序執行
 1. FreshNeighborStars()    >>  根據占用的星星推估此碎片有"哪些鄰居星星"和"觸發邊位置(鄰居星星的角度)"，
                                最後得到對於這個碎片哪些星星構成一條邊，存入neighborRelative
 2. ThisReDetectEdge()      >>  判斷哪些邊有完全觸發，triggerCount++
 3. NeighborReDetectEdge()  >>  提醒鄰居也要重新計算觸發邊
 */

/// <summary>
/// 現在
/// 不需要知道有幾條邊觸發
/// 但需要知道，哪些邊有觸發，哪些Affix可以使用
/// </summary>

//所有碎片繼承此物件
public class Chip : Singleton<Chip>
{
    public void PutOn(Fragment theF, List<int> _coverStarsID)       //放進碎片
    {
        theF.m_Data.touchStarsID.Clear();
        for (int i = 0; i < _coverStarsID.Count; i++)
            theF.m_Data.touchStarsID.Add(_coverStarsID[i]);

        LockStar(theF);

        FreshNeighborStars(theF);       //鄰居綁訂星星位置
        ThisReDetectAffix(theF);
        NeighborReDetectEdge(theF);

    }

    public void PullUp(Fragment theF)                       //拿出碎片
    {
        UnlockStar(theF);
        theF.m_Data.touchStarsID.Clear();

        NeighborReDetectEdge(theF);
        FreshNeighborStars(theF);
        ThisReDetectAffix(theF);

    }

    public void LockStar(Fragment theF)////////////////////////////////////////////////////////////OK
    {
        for (int i = 0; i < theF.m_Data.touchStarsID.Count; i++)
        {
            //star變黑色
            AllStar.Instance.stars[theF.m_Data.touchStarsID[i]].gameObject.GetComponent<Image>().color = theF.m_Data.fragColor;
            //更改Star狀態
            AllStar.Instance.stars[theF.m_Data.touchStarsID[i]].isLocked = true;
            //Star綁定chip_script
            AllStar.Instance.stars[theF.m_Data.touchStarsID[i]].fragID = theF.m_Data.fragmentID;
        }
    }

    public void UnlockStar(Fragment theF)//////////////////////////////////////////////////////////OK
    {
        for (int i = 0; i < theF.m_Data.touchStarsID.Count; i++)
        {
            //star變回白色
            AllStar.Instance.stars[theF.m_Data.touchStarsID[i]].gameObject.GetComponent<Image>().color = Color.white;
            //更改Star狀態
            AllStar.Instance.stars[theF.m_Data.touchStarsID[i]].isLocked = false;
            //Star取消綁定chip_script
            AllStar.Instance.stars[theF.m_Data.touchStarsID[i]].fragID = -1;
        }
    }

    //重新計算此碎片的觸發邊
    /*public void ThisReDetectEdge(Fragment theF)/////////////////////////////////////////////////////////////////OK
    {
        theF.m_Data.triggerCount = 0;               //中多少條邊就triggerCount++

        //先觀察有哪些鄰居存在
        for (int i = 0; i < theF.m_Data.neighborRelativeInfo.Count; i++)         //幾個邊
        {
            bool AllOK = true;
            for (int j = 0; j < theF.m_Data.neighborRelativeInfo[i].neighborRelative_id.Count; j++)  //幾個點組成一個邊
            {
                if (AllStar.Instance.stars[theF.m_Data.neighbors[theF.m_Data.neighborRelativeInfo[i].neighborRelative_id[j]].starID].fragID == -1)
                {
                    AllOK = false;
                    break;
                }
            }
            if (AllOK)
            {
                theF.m_Data.triggerCount++;
            }
        }
    }*/

    /************************************************************/
    /************************************************************/
    /************************************************************/
    /************************************************************/
    /************************************************************/
    /************************************************************/
    /************************************************************/
    /************************************************************/

    //檢查哪些Affix可以用
    public void ThisReDetectAffix(Fragment theF)/////////////////////////////////////////////////////////////////OK
    {
        theF.m_Data.triggerCount = 0;               //中多少條邊就triggerCount++

        //先觀察有哪些鄰居存在
        for (int i = 0; i < theF.m_Data.neighborRelativeInfo.Count; i++)         //幾個邊
        {
            bool AllOK = true;
            for (int j = 0; j < theF.m_Data.neighborRelativeInfo[i].relativeInfo.Count; j++)  //幾個點組成一個邊
            {
                triggerInfo t = theF.m_Data.neighborRelativeInfo[i].relativeInfo[j];
                Vector2 temp;
                switch (t.edgeT)
                {
                    case edgeTrigger.上:
                        temp = new Vector2(theF.m_Data.centerAbsPos.x + t.triPos.x, theF.m_Data.centerAbsPos.y + t.triPos.y + 1);
                        break;
                    case edgeTrigger.下:
                        temp = new Vector2(theF.m_Data.centerAbsPos.x + t.triPos.x, theF.m_Data.centerAbsPos.y + t.triPos.y - 1);
                        break;
                    case edgeTrigger.右:
                        temp = new Vector2(theF.m_Data.centerAbsPos.x + t.triPos.x + 1, theF.m_Data.centerAbsPos.y + t.triPos.y);
                        break;
                    default://case edgeTrigger.左:
                        temp = new Vector2(theF.m_Data.centerAbsPos.x + t.triPos.x - 1, theF.m_Data.centerAbsPos.y + t.triPos.y);
                        break;
                }

                int sId = AllStar.Instance.stars.FindIndex(x => x.pos == temp);
                if (sId == -1)
                {
                    AllOK = false;
                    break;
                }
                if (AllStar.Instance.stars[sId].fragID == -1)
                {
                    AllOK = false;
                    break;
                }
            }
            if (AllOK)
            {
                theF.m_Data.triggerCount++;
                //Debug.Log(theF.m_Data.fragmentID + theF.name+ " 's Affix : " + theF.m_Data.neighborRelativeInfo[i].theAffix.description);
                theF.m_Data.neighborRelativeInfo[i].check = true;

                //邊亮起來
                //theF.m_Data.neighborRelativeInfo[i].relativeInfo[1].
                //ing


                //若要單一 一次對一個Affix觸發 則要在這做

            }
            else
            {
                theF.m_Data.neighborRelativeInfo[i].check = false;
            }
        }
    }
    

    /************************************************************/
    /************************************************************/
    /************************************************************/
    /************************************************************/
    /************************************************************/
    /************************************************************/
    /************************************************************/


    //鄰居碎片們重新計算邊的觸發
    public void NeighborReDetectEdge(Fragment theF)//////////////////////////////////////////////////////////OK
    {
        //if (theF.m_Data.neighborStars.Count == 0) return;
        //呼叫鄰居的ThisReDetectEdge
        for (int i = 0; i < theF.m_Data.neighbors.Count; i++)
        {
            int id = AllStar.Instance.stars[theF.m_Data.neighbors[i].starID].fragID;
            if (id != -1)
                ThisReDetectAffix(AllFragment.Instance.fragments[id]);
        }
    }

    public void FreshNeighborStars(Fragment theF)/////////////////////////////////////////////////////////// OK
    {
        for (int i = 0; i < theF.m_Data.neighbors.Count; i++)
        {
            Vector2 temp = new Vector2(theF.m_Data.neighbors[i].pos.x + CurrentData.Instance.currentPos.x, theF.m_Data.neighbors[i].pos.y + CurrentData.Instance.currentPos.y);
            //找到該點
            int id = AllStar.Instance.stars.FindIndex(x => x.pos == temp);
            if (id != -1)
            {
                theF.m_Data.neighbors[i].starID = id;
            }
        }
    }


}
