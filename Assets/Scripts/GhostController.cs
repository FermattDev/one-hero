using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GhostController : AvatarController
{
    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionPlayer = new List<KeyValuePair<float, KeyValuePair<string, object>>>();
    private bool actionStart;
    private int index;

    public void SetActionPlayer(List<KeyValuePair<float, KeyValuePair<string, object>>> newActionPlayer)
    {
        actionPlayer = newActionPlayer;
        actionStart = true;
        index = 0;
    }

    protected override void Update()
    {
        base.Update();

        if (!actionStart)
        {
            return;
        }
        if(actionPlayer.Count > index)
        {
            if ((actionPlayer[index].Key) < Time.time - TimeStart)
            {
                switch(actionPlayer[index].Value.Key)
                {
                    case "Move":
                        AvatarMove(actionPlayer[index].Value.Value);
                        break;
                    case "Fire":
                        AvatarFire(actionPlayer[index].Value.Value);
                        break;
                    case "Dash":
                        AvatarDash(actionPlayer[index].Value.Value);
                        break;
                    case "Dead":
                        AvatarDead();
                        break;
                }

                index++;
            }
        }
    }
}
