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

    int overFragID = -1;

    void Update()
    {
        if (chip2Mouse != null)
        {
            Vector3 temp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            chip2Mouse.transform.position = new Vector3(temp.x, temp.y, 10);
        }

        if (!EventSystem.current.IsPointerOverGameObject())
            return;

        //點到範圍外
        if (Input.GetMouseButtonDown(0))
        {
            if (chip2Mouse.sprite != null && ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.tag != "putFrag" && ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.tag != "slot")
            {
                currentFragmentID = -1;
                //碎片未用，放回原位------------------------------------------------------------------------
                if (f_m == null) return;
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
        {
            //全部碎片反亮
            for (int i = 0; i < AllFragment.Instance.fragments.Count; i++)//其他的碎片就暗下來
            {
                Chip.Instance.LightUpStar(AllFragment.Instance.fragments[i]);
            }
            return;
        }

        if (currentFragmentID == -1)            //手中沒有碎片
        {
            //滑鼠滑到天賦盤上
            star getTemp = ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.GetComponent<star>();

            if (overFragID != getTemp.fragID)   //此刻 和 上一刻 不在同一個碎片上
            {
                overFragID = getTemp.fragID;

                //沒有在碎片上，顏色改變後，之後事情都不用做
                if (getTemp.fragID == -1)
                {
                    for (int i = 0; i < AllFragment.Instance.fragments.Count; i++)//其他的碎片就暗下來
                    {
                        Chip.Instance.LightUpStar(AllFragment.Instance.fragments[i]);
                        FragInfoControl.Instance.fragInfoBackground.SetActive(false);
                    }
                    return;
                }

                //有在碎片上，顏色重新計算
                for (int i = 0; i < AllFragment.Instance.fragments.Count; i++)//其他的碎片就暗下來
                {
                    if (i == overFragID)
                    {
                        Fragment targetFrag = AllFragment.Instance.fragments[i];
                        FragInfoControl.Instance.ShowFragInfo(targetFrag.m_Data.fragIndexImage, targetFrag.m_Data.GetFragAllAffixsInfo());
                        Chip.Instance.LightUpStar(targetFrag);
                        continue;
                    }
                    Chip.Instance.DarkDownStar(AllFragment.Instance.fragments[i]);
                }

                //顯示當前碎片的原始圖案
                //AllFragment.Instance.fragments[overFragID].m_Data.ShowOriginPicture();
            }



            /*if (Input.GetMouseButtonDown(1))    //滑鼠右鍵，查看觸發多少條邊
            {
                if (AllFragment.Instance.fragments.Count == 0)
                    return;
                star getTemp = ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.GetComponent<star>();
                if (overFragID == -1) return;

                Debug.Log( AllFragment.Instance.fragments[overFragID].m_Data.PrintAndExeAffixs());

                return;
            }*/
            if (Input.GetMouseButtonDown(0))    //拿起來
            {
                //star getTemp = ExtendedStandaloneInputModule.GetPointerEventData().pointerCurrentRaycast.gameObject.GetComponent<star>();
                if (getTemp.isLocked)
                {
                    currentFragmentID = overFragID;
                    //Debug.Log("拿起來currentFragmentID:" + currentFragmentID);
                    //Debug.Log("拿起來fragmentID:" + overFragID);
                    Chip.Instance.PullUp(AllFragment.Instance.fragments[currentFragmentID]);
                    chip2Mouse.gameObject.SetActive(true);
                    chip2Mouse.sprite = AllFragment.Instance.fragments[currentFragmentID].m_Data.fragImage;
                    //Debug.LogWarning("overFragID---" + currentFragmentID);
                }
                return;
            }

        }
        else
        {                              //手中有碎片
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////確認好位置要放下了
            if (Input.GetMouseButtonDown(0) && !positionError)  //放下
            {
                Fragment fragment = AllFragment.Instance.fragments[currentFragmentID];
                if (f_m.putFrag != null)
                {
                    fragment.m_Data.fName = f_m.putFrag.m_Data.fName;
                    fragment.m_Data.fragIndexImage = f_m.putFrag.m_Data.fragIndexImage;
                }
                fragment.m_Data.centerAbsPos = currentPos;
                Chip.Instance.PutOn(fragment, coverStarsID);

                if (chip2Mouse != null)
                {
                    chip2Mouse.gameObject.SetActive(false);
                    //原本位置(左右上下)的資料清空-------------------------------------------------------
                    f_m.Clean();
                    f_m.gameObject.SetActive(false);
                }

                //Debug.Log("放下currentFragmentID:" + currentFragmentID);
                //Debug.Log("放下ChipID:" + AllFragment.Instance.fragments[currentFragmentID].m_Data.fragmentID);
                coverStarsID.Clear();
                currentFragmentID = -1;
                lastStarID = -1;

                CallAllTriggeredAffixs.Instance._CallAllTriggeredAffixs();

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
