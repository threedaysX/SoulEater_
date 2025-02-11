﻿using System;
using System.Collections;
using UnityEngine;

public class DieController : MonoBehaviour
{
    private Character character;
    public Transform soulPrefab;
    public ParticleSystem dieParticle;
    public AudioClip dieSound;
    public string soulName;
    public float dieDuration;
    public float dieDissolveDuration;

    private void Start()
    {
        character = GetComponent<Character>();
        if (dieDissolveDuration > dieDuration)
        {
            dieDissolveDuration = dieDuration;
        }
    }

    public void StartDie()
    {
        character.StopAllCoroutines();
        character.GetIntoImmune(true);
        character.LockOperation(LockType.Die, true);
        character.opc.InterruptAnimOperation();
        character.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        character.CurrentHealth = 0;
        character.opsc.PlaySound(dieSound);
        TriggerDieParticle();
        DieDissolve(dieDissolveDuration);
        StartCoroutine(DieCoroutine(dieDuration));
        GenerateSoul();
    }

    /// <summary>
    /// Destory object
    /// </summary>
    private IEnumerator DieCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        this.gameObject.SetActive(false);
    }

    public void DieDissolve(float duration, params Action[] callBacks)
    {
        // Dissolve Shader Graph Effect
        Material material = GetComponent<SpriteRenderer>().material;
        Counter.Instance.StartCountDownInTimes(1f, 0f, duration, false, (x) => material.SetFloat("_Fade", x), callBacks);
    }

    public void TriggerDieParticle()
    {
        string layerName = character.GetComponent<SpriteRenderer>().sortingLayerName;
        int layerOrder = character.GetComponent<SpriteRenderer>().sortingOrder + 1;
        dieParticle.Play(true);
        dieParticle.GetComponent<Renderer>().sortingLayerName = layerName;
        dieParticle.GetComponent<Renderer>().sortingOrder = layerOrder;
        foreach (Transform item in dieParticle.transform)
        {
            item.GetComponent<ParticleSystemRenderer>().sortingLayerName = layerName;
            item.GetComponent<ParticleSystemRenderer>().sortingOrder = layerOrder;
        }
    }

    public void GenerateSoul()
    {
        GameObject soul = PrefabRenderer.Instance.RenderPrefab(soulPrefab, soulName, true);
        soul.GetComponent<Character>().GetIntoImmune(dieDuration + 0.1f);
        soul.GetComponent<Character>().characterName = soulName;
        soul.GetComponent<SpriteRenderer>().sortingLayerName = character.GetComponent<SpriteRenderer>().sortingLayerName;
        soul.GetComponent<SpriteRenderer>().sortingOrder = character.GetComponent<SpriteRenderer>().sortingOrder;
        soul.tag = character.tag;
        soul.gameObject.layer = character.gameObject.layer;
        soul.name = soulName;
        StartCoroutine(FollowMaster(transform, soul.transform, dieDuration));
    }

    public virtual IEnumerator FollowMaster(Transform master, Transform follower, float duration)
    {
        float timeleft = duration;
        while (timeleft > 0)
        {
            if (master == null)
                yield break;

            follower.position = master.position;
            timeleft -= Time.deltaTime;
            yield return null;
        }
    }
}