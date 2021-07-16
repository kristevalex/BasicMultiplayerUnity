using UnityEngine;

public class AOEDamage
{
    public void DamageInArea(Vector2 center, float radius, float damage, ulong clientId)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
        foreach (Collider2D collider in colliders)
        {
            var player = collider.gameObject.GetComponent<HelloWorldPlayer>();

            if (player)
            {
                if (player.OwnerClientId != clientId)
                {
                    player.ReciveHPServerOnly(-damage);
                }
            }
        }
    }
}
