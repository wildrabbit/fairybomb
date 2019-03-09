using System.Collections.Generic;
using UnityEngine;

public class ActionPhaseContext : IPlayContext
{
    public PlayContext Update(BaseContextData contextData, out bool timeWillPass)
    {
        ActionPhaseData actionData = contextData as ActionPhaseData;
        GameInput input = actionData.input;
        Player player = actionData.Player;
        FairyBombMap map = actionData.Map;
        EntityController entityController = actionData.EntityController as EntityController;
        GameEventLog log = actionData.Log;

        timeWillPass = false;

        if(input.IdleTurn)
        {
            timeWillPass = true;
            PlayerActionEvent evt = new PlayerActionEvent(actionData.Turns, actionData.TimeUnits);
            evt.SetIdle();
            log.AddEvent(evt);
            return PlayContext.Action;
        }

        Vector2Int playerCoords = map.CoordsFromWorld(player.transform.position);

        if (input.BombPlaced)
        {
            if(player.Frozen)
            {
                Debug.Log("Player is frozen and can't lay bombs");
            }
            else
            {
                BaseEntity[] blackList = new BaseEntity[] { player };
                if (map.TileAt(playerCoords).Walkable && !entityController.ExistsEntitiesAt(playerCoords, blackList) && player.BomberTrait.HasBombAvailable())
                {
                    Bomb bomb = entityController.CreateBomb(player.BomberTrait.SelectedBomb, playerCoords, player);
                    player.BomberTrait.UseInventoryItem(player.BomberTrait.SelectedIdx);
                    //PlayerActionEvent evt = new PlayerActionEvent(actionData.Turns, actionData.TimeUnits);
                    //evt.SetBomb(bomb.Coords);
                    //log.AddEvent(evt);
                }

            }
            timeWillPass = true;
            return PlayContext.Action;
        }

        bool evenColumn = playerCoords.y % 2 == 0;
        MoveDirection moveDir = input.MoveDir;
        Vector2Int offset = map.GetOffset(moveDir, evenColumn);
        if (offset != Vector2Int.zero)
        {
            playerCoords += offset;
            
            if (map.IsWalkableTile(playerCoords))
            {
                List<BaseEntity> otherEntities = entityController.GetEntitiesAt(playerCoords);
                List<BombPickableItem> itemsToPick = new List<BombPickableItem>();
                Monster collidingMonster = null;
                bool canMove = true;
                foreach(var other in otherEntities)
                {
                    if (other is Bomb)
                    {
                        BombWalkabilityType walksOverBombs = player.BombWalkability;
                        bool isOwnBomb = ((Bomb)other).Owner == player;
                        canMove = (walksOverBombs == BombWalkabilityType.CrossAny || walksOverBombs == BombWalkabilityType.CrossOwnBombs && isOwnBomb);
                    }
                    else if (other is Monster)
                    {
                        collidingMonster = ((Monster)other);
                        canMove = player.CanMoveIntoMonsterCoords;
                    }
                    else if (other is BombPickableItem)
                    {
                        BombPickableItem ground = ((BombPickableItem)other);
                        if(player.BomberTrait.HasItem(ground) || player.BomberTrait.HasFreeSlot(itemsToPick))
                        {
                            itemsToPick.Add(ground);
                        }
                    }
                    
                    if(!canMove)
                    {
                        break;
                    }
                }

                if(canMove)
                {
                    player.Coords = playerCoords;
                    player.PaintableTrait.OwnerChangedPos(playerCoords);

                    foreach(var item in itemsToPick)
                    {
                        entityController.DestroyEntity(item);
                        player.BomberTrait.AddToInventory(item);
                    }

                    if(collidingMonster != null)
                    {
                        int playerDmg = player.MonsterCollided(collidingMonster);
                        int monsterDmg = collidingMonster.PlayerCollided(player);
                        entityController.CollisionMonsterPlayer(player, collidingMonster, playerDmg, monsterDmg);
                    }

                    //PlayerActionEvent evt = new PlayerActionEvent(actionData.Turns, actionData.TimeUnits);
                    //evt.SetMovement(moveDir, player.Coords);
                    //log.AddEvent(evt);
                    timeWillPass = true;
                }                
            }
            else
            {
                timeWillPass = actionData.BumpingWallsWillSpendTurn;
            }
        }

        // Inventory actions:
        int inventoryIdx = System.Array.FindIndex(input.NumbersPressed, x => x);
        if(inventoryIdx != -1)
        {
            bool dropModifier = input.ShiftPressed;

            if(player.BomberTrait.HasItemAt(inventoryIdx))
            {
                if (dropModifier)
                {
                    if((inventoryIdx > 0 || !player.BomberTrait.FirstItemFixed)  && player.BomberTrait.HasItemAt(inventoryIdx))
                    {
                        BombInventoryEntry dropItem = player.BomberTrait.DropFromInventory(inventoryIdx);
                        entityController.CreatePickable(actionData.LootData, dropItem.Bomb, player.Coords, dropItem.Amount, dropItem.Unlimited);
                    }
                    
                }
                else
                {
                    player.BomberTrait.SelectBomb(inventoryIdx);
                }
            }            
        }

        return PlayContext.Action;
    }
}
