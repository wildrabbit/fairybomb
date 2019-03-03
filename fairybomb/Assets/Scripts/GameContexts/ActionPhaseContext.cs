using UnityEngine;

public class ActionPhaseContext : IPlayContext
{
    public PlayContext Update(BaseContextData contextData, out bool timeWillPass)
    {
        ActionPhaseData actionData = contextData as ActionPhaseData;
        GameInput input = actionData.input;
        Player player = actionData.Player;
        FairyBombMap map = actionData.Map;


        timeWillPass = false;

        if(input.IdleTurn)
        {
            timeWillPass = true;
            return PlayContext.Action;
        }

        Vector2Int playerCoords = map.CoordsFromWorld(player.transform.position);
        Vector2Int offset = Vector2Int.zero;
        bool evenColumn = playerCoords.y % 2 == 0;
        MoveDirection moveDir = input.MoveDir;
        switch (moveDir)
        {
            case MoveDirection.None:
                {
                    break;
                }
            case MoveDirection.NW:
                {
                    offset.Set(evenColumn ? 0 : 1, -1);
                    break;
                }
            case MoveDirection.N:
                {
                    offset.Set(1, 0);
                    break;
                }
            case MoveDirection.NE:
                {
                    offset.Set(evenColumn ? 0 : 1, 1);
                    break;
                }
            case MoveDirection.SW:
                {
                    offset.Set(evenColumn ? -1 : 0, -1);
                    break;
                }
            case MoveDirection.SE:
                {
                    offset.Set(evenColumn ? -1 : 0, 1);
                    break;
                }
            case MoveDirection.S:
                {
                    offset.Set(-1, 0);
                    break;
                }
        }

        if(offset != Vector2Int.zero)
        {
            playerCoords += offset;
            map.ConstrainCoords(ref playerCoords);
            Vector2 playerTargetPos = map.WorldFromCoords(playerCoords);

            if (map.IsWalkableTile(playerCoords))
            {
                player.transform.position = playerTargetPos;
                timeWillPass = true;
            }
            else
            {
                timeWillPass = actionData.BumpingWallsWillSpendMoves;
            }
        }    
        

        return PlayContext.Action;
    }
}
