using JadePhoenix.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintProjectile : Projectile
{
    public GameObject PaintSplatterPrefab;

    protected override void OnDeath()
    {
        base.OnDeath();
        //Debug.Log($"{this.GetType()}.OnDeath: OnDeath triggered, attempting to spawn splatter object of {PaintSplatterPrefab.name}.");
        SpawnManager.Instance.SpawnAtPosition(this.transform.position, PaintSplatterPrefab.name);
    }
}
