using System;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public interface BaseEnemy : IHittable
{
    public Action OnDeath { get; set; }
    public float GetHealthNormalized();

}
