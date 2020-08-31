using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CurrentData : Singleton<CurrentData>
{
    public int currentFragmentID;           //目前碎片ID
    public int currentStarID;               //存放當前點(三角形)在AllStar的ID<<中心點
    public Vector2 currentPos;              //存放當前點(三角形)在AllStar的ID<<中心點
    public int lastStarID;
    public List<int> coverStarsID = new List<int>();   //存放 此碎片會覆蓋到的點(三角形)們
    ///
    //public Text appearTriggerCount;
    //public Image textImage;
    //bool windowsAppear = false;           //顯示碎片數量、名稱的視窗

    bool positionError = false;             //放置碎片位置卡到，顯示紅色不能放置
    public Image chip2Mouse;                //碎片跟隨滑鼠
    public Frag_m f_m;                      //當前是抓哪個預留位置的資料
    private void Start()
    {
        chip2Mouse.gameObject.SetActive(false);
        currentFragmentID = -1;
    }
    void Update()
    {
        if (chip2Mouse != null)
        {
            Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            chip2Mouse.transform.position = new Vector3(temp.x, temp.y, 10) ;
        }

        if (!EventSystem.current.IsPointerOverGameObject())
            return;

        //點到範圍外
        if (Input.GetMouseButtonDown(0) )
        {
            if (chip2Mouse.sprite != null && ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.tag != "putFrag" && ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.tag != "slot")
            {
                currentFragmentID = -1;
                //碎片未用，放回原位------------------------------------------------------------------------
                if (f_m.putFrag != null)
                {
                    f_m.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    chip2Mouse.gameObject.SetActive(false);
                }
                for (int i = 0; i < coverStarsID.Count; i++)
                {
                    AllStar.Instance.stars[coverStarsID[i]].ExitColor();
                }
            }
            //先拿掉視窗顯示，改用Debug.Log
            /*else if (windowsAppear)
            {
                windowsAppear = false;
                appearTriggerCount.text = "";
                textImage.color = new Color(0, 0, 0, 0);
                return;
            }*/
        }

        if (ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.tag != "slot")
            return;

        if (currentFragmentID == -1)            //手中沒有碎片
        {
            if (Input.GetMouseButtonDown(1))    //查看觸發多少條邊
            {
                if (AllFragment.Instance.fragments.Count == 0)
                    return;
                star getTemp = ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.GetComponent<star>();
                if (getTemp.fragID == -1) return;
                //文字框顯示
                /*appearTriggerCount.text = AllFragment.Instance.fragments[getTemp.fragID].m_Data.PrintTriggerCount();
                appearTriggerCount.transform.position = Input.mousePosition;
                textImage.transform.position = Input.mousePosition;
                textImage.color = new Color(1, 1, 0, 0.7f);
                appearTriggerCount.color = new Color(0, 0, 0, 1);
                windowsAppear = true;
                */

                //Debug顯示
                Debug.Log( AllFragment.Instance.fragments[getTemp.fragID].m_Data.PrintAndExeAffixs());

                return;
            }
            if (Input.GetMouseButtonDown(0))    //拿起來
            {
                star getTemp = ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.GetComponent<star>();
                if (getTemp.isLocked)
                {
                    currentFragmentID = getTemp.fragID;
                    //Debug.Log("拿起來currentFragmentID:" + currentFragmentID);
                    //Debug.Log("拿起來fragmentID:" + getTemp.fragID);
                    Chip.Instance.PullUp(AllFragment.Instance.fragments[currentFragmentID]);
                    chip2Mouse.gameObject.SetActive(true);
                    chip2Mouse.sprite = AllFragment.Instance.fragments[currentFragmentID].m_Data.fragImage;
                    //Debug.LogWarning("getTemp.fragID---" + currentFragmentID);
                }
                return;
            }

        }
        else
        {                              //手中有碎片
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////確認好位置要放下了
            if (Input.GetMouseButtonDown(0) && !positionError)  //放下
            {
                if (chip2Mouse != null)
                {
                    chip2Mouse.gameObject.SetActive(false);
                    //原本位置(左右上下)的資料清空-------------------------------------------------------
                    f_m.Clean();
                }

                AllFragment.Instance.fragments[currentFragmentID].m_Data.centerAbsPos = currentPos;
                Chip.Instance.PutOn(AllFragment.Instance.fragments[currentFragmentID], coverStarsID);

                //Debug.Log("放下currentFragmentID:" + currentFragmentID);
                //Debug.Log("放下ChipID:" + AllFragment.Instance.fragments[currentFragmentID].m_Data.fragmentID);
                coverStarsID.Clear();
                currentFragmentID = -1;
                lastStarID = -1;
                return;
            }

            currentStarID = ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.GetComponent<star>().allStar_ID;
            if (lastStarID == currentStarID)
                return;
            lastStarID = currentStarID;

            //先將舊的刷白
            for (int i = 0; i < coverStarsID.Count; i++)
            {
                AllStar.Instance.stars[coverStarsID[i]].ExitColor();
            }
            coverStarsID.Clear();

            List<Vector2> temp_FragStars;

            //計算新的CoverStars
            temp_FragStars = AllFragment.Instance.fragments[currentFragmentID].m_Data.touchPos_v2;
            currentPos = AllStar.Instance.stars[currentStarID].pos;

            if ((currentPos.x + currentPos.y) % 2 != 0)
            {
                currentPos.x -= 1;
            }


            positionError = false;
            for (int i = 0; i < temp_FragStars.Count; i++)
            {
                Vector2 tempVec = new Vector2(temp_FragStars[i].x + currentPos.x, temp_FragStars[i].y + currentPos.y);
                int id = AllStar.Instance.stars.FindIndex(x => x.pos == tempVec);
                if (id != -1)//有此位置
                {
                    if (AllStar.Instance.stars[id].isLocked)
                    {
                        positionError = true;
                        continue;
                    }
                    coverStarsID.Add(id);
                }
                else            //無此位置>>碎片超出天賦盤範圍
                {
                    positionError = true;
                    break;
                }
            }
            //將新的上色
            if (positionError)
                for (int i = 0; i < coverStarsID.Count; i++)
                {
                    AllStar.Instance.stars[coverStarsID[i]].ErrorColor();
                }
            else
                for (int i = 0; i < coverStarsID.Count; i++)
                {
                    AllStar.Instance.stars[coverStarsID[i]].EnterColor();
                }

        }

    }

}
