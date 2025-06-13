using System;
using UnityEngine;

public enum HittableType
{
    Enemy,
    Obstacle
}
public interface IHittable
{
    public Action OnHit { get; set; }
    public void DoHit(int damage);

    public HittableType GetType();

}
