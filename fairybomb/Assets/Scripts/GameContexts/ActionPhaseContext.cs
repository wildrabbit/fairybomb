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
            BaseEntity[] blackList = new BaseEntity[] { player };
            if(map.TileAt(playerCoords).Walkable && !entityController.ExistsEntitiesAt(playerCoords, blackList) && player.BomberTrait.HasBombAvailable())
            {
                Bomb bomb = entityController.CreateBomb(player.BomberTrait.SelectedBomb, playerCoords, player);
                PlayerActionEvent evt = new PlayerActionEvent(actionData.Turns, actionData.TimeUnits);
                evt.SetBomb(bomb.Coords);
                log.AddEvent(evt);
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
                        canMove = false; // TODO: Can we suicide? Can we melee?                       
                    }

                    if(!canMove)
                    {
                        break;
                    }
                }

                if(canMove)
                {
                    player.Coords = playerCoords;
                    PlayerActionEvent evt = new PlayerActionEvent(actionData.Turns, actionData.TimeUnits);
                    evt.SetMovement(moveDir, player.Coords);
                    log.AddEvent(evt);
                    timeWillPass = true;
                }                
            }
            else
            {
                timeWillPass = actionData.BumpingWallsWillSpendTurn;
            }
        }    
        

        return PlayContext.Action;
    }
}
