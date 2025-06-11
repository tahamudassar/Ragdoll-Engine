using UnityEngine;

public enum HittableType
{
    Player,
    Enemy,
    Obstacle
}
public interface Hittable
{

    public void DoHit(int damage);

    public HittableType GetType();
    
}
