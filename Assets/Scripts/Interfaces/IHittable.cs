using UnityEngine;

public enum HittableType
{
    Player,
    Enemy,
    Obstacle
}
public interface IHittable
{

    public void DoHit(int damage);

    public HittableType GetType();
    
}
