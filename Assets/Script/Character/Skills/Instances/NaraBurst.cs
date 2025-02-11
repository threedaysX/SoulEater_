﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaraBurst : DisposableSkill
{
    public Skill naraCircleBurstData;
    public float igniteDuration; // 擊中敵人【燃燒】異常的時間
    public float burstRadius;   // 爆炸的半徑
    public int burstLimitCount; // 爆炸的數量
    public AudioClip renderSound;
    public GameObject burstHint;
    public ColliderDetectControl detectArea;

    protected override void AddAffectEvent()
    {
        
    }

    public override void GenerateSkill(Character character, Skill skill)
    {
        base.GenerateSkill(character, skill);

        burstHint.SetActive(false);
        burstHint.transform.localScale = new Vector3(burstHint.transform.localScale.x * burstRadius, burstHint.transform.localScale.y * burstRadius, 0);  // 調整爆炸半徑
        ObjectPools.Instance.RenderObjectPoolsInParent(burstHint, burstLimitCount);
    }

    public override void CastSkill()
    {
        base.CastSkill();

        // Render.
        float rangeDelay = currentSkill.fixedCastTime.Value / burstLimitCount;
        float upperRangeDelay = rangeDelay * 1f;
        float lowerRangeDelay = rangeDelay * 0.4f;
        float currentDelay = 0;
        for (int i = 0; i < burstLimitCount; i++)
        {
            currentDelay += Random.Range(lowerRangeDelay, upperRangeDelay);
            StartCoroutine(RenderBurstHint(currentDelay));
        }
    }

    private IEnumerator RenderBurstHint(float burstDelayBetweenFirst)
    {
        yield return new WaitForSeconds(burstDelayBetweenFirst);

        NaraCircleBurst burst = ObjectPools.Instance.GetObjectInPools(burstHint.name, GetBurstPos())
            .GetComponent<NaraCircleBurst>();
        burst.data.transform = burst.transform;
        burst.data.delay = burstDelayBetweenFirst;
        burst.data.igniteDuration = igniteDuration;
        burst.GenerateSkill(sourceCaster, naraCircleBurstData);
    }

    private Vector3 GetBurstPos()
    {
        List<Character> targets = detectArea.Detect<Character>(
            sourceCaster.transform.position
            , sourceCaster.tag
            , sourceCaster.gameObject.layer
            , new ColliderDetectControl.CircleDetect(currentSkill.range.Value)
            , false);


        float offsetX = 5f;
        float offsetFrontX = Random.Range(7f, 8.2f);
        float offsetY = 1f;
        float targetX;
        float targetY;
        Transform targetTrans;

        if (targets.Count == 0)
        {
            targetTrans = sourceCaster.transform;
        }
        else
        {
            // Random Choose one target.
            int randomIndex = Random.Range(0, targets.Count - 1);

            targetTrans = targets[randomIndex].transform;
        }

        targetX = targetTrans.position.x;
        targetY = targetTrans.position.y;
        float y = Random.Range(targetY - offsetY, targetY + offsetY);
        float x;

        // Means that target is [right side] of caster.
        if (targetTrans.position.x - sourceCaster.transform.position.x > 0)
        {
            x = Random.Range(targetX - offsetX, targetX + offsetFrontX);
        }
        // Means that target is [left side] of caster.
        else
        {
            x = Random.Range(targetX - offsetFrontX, targetX + offsetX);
        }

        return new Vector3(x, y, 0);
    }
}
