using System;

[Serializable]
public class EntityCreationData
{
    public Player PlayerPrefab;
    public Bomb BombPrefab;
    public Monster MonsterPrefab;

    public BombPickableItem LootItemPrefab;
}
