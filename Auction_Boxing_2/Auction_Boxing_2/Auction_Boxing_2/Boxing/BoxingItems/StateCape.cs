using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Auction_Boxing_2.Boxing.PlayerStates
{
    enum CapeState
    {
        draw,
        hide,
        travel,
        reveal
    }
    class StateCape : State
    {
        CapeState state;
        bool exiting = false;

        float speed = 500;

        bool traveled = false;

        BoxingPlayer targetPlayer;
        float target;
        int direction; // moves backwards towards target
        int itemIndex;

        KeyPressed itemButton; // the button mapped to this item

        public StateCape(int itemIndex, BoxingPlayer player, KeyPressed key)
            : base(player, "Cape")
        {
            this.itemIndex = itemIndex;
            state = CapeState.draw;

            this.itemButton = key;
            
        }

        public override void Update(GameTime gameTime)
        {
            if (player.isFreeingCape)
                ChangeState(new StateCapeStuck(itemIndex, player));

            switch (state)
            {
                case(CapeState.draw):
                    if (!hasPlayedSound)
                        PlaySound(player.soundEffects["CapeDraw"], .5f);
                    if (player.sprite.FrameIndex == 9)
                        state = CapeState.hide;
                    break;
                case(CapeState.hide):
                     // Hold to continue hiding
                    if (player.IsKeyDown(itemButton))
                    {
                        player.sprite.FrameIndex = 9;

                        bool behind = false;

                        if (player.IsKeyDown(KeyPressed.Left))
                        {
                            int dir = player.direction;
                            if(player.direction == 1)
                                dir = -1;
                            targetPlayer = player.BoxingManager.GetPlayerInFront(player, player.position.Y - 2 * player.GetHeight / 3, dir);
                            if(targetPlayer != null)
                                target = targetPlayer.position.X - 18 * BoxingPlayer.Scale;

                            direction = dir;
                        }
                        else if(player.IsKeyDown(KeyPressed.Right))
                        {
                            int dir = player.direction;
                            if(player.direction == -1)
                                dir = 1;
                            targetPlayer = player.BoxingManager.GetPlayerInFront(player, player.position.Y - 2 * player.GetHeight / 3, dir);
                            if (targetPlayer != null)
                                target = targetPlayer.position.X + 18 * BoxingPlayer.Scale;

                            direction = dir;
                        }
                        if (targetPlayer != null)
                        {
                            player.soundEffects["Cape"].Play(.5f, 0,0); // play the sound effect!
                            state = CapeState.travel;
                            player.ChangeDirection(direction * -1);
                        }
                    }
                    else if (!exiting)
                    {
                        player.sprite.FrameIndex = 16;
                        state = CapeState.reveal;
                    }
                    break;
                case(CapeState.reveal):
                    if (player.sprite.FrameIndex == player.animations[key].FrameCount - 1)
                    {
                        if(traveled)
                            player.isFreeingCape = true;
                        ChangeState(new StateStopped(player));
                        
                    }
                    break;
                case(CapeState.travel):

                    if (player.sprite.FrameIndex == 15)
                        player.sprite.FrameIndex = 13;

                    

                    player.position.X += (float)(direction * speed * gameTime.ElapsedGameTime.TotalSeconds);
                    float dif = Math.Abs(player.position.X - target);
                    // You're behind'm!
                    if (dif < 5 && dif > 0
                        || player.position.X - player.GetWidth / 2 <= player.BoxingManager.bounds.X
                        || player.position.X + player.GetWidth / 2 >= player.BoxingManager.bounds.X + player.BoxingManager.bounds.Width
                        )
                    {
                        state = CapeState.reveal;
                        traveled = true;
                    }

                    break;



            }

            base.Update(gameTime);
        }

        public override void isHit(BoxingPlayer attackingPlayer, State expectedHitState, int damage)
        {
            if(!(state == CapeState.travel))
                base.isHit(attackingPlayer, expectedHitState, damage);
        }
        public override void isHitByItem(ItemInstance item, State expectedHitState)
        {
            if (!(state == CapeState.travel))
                base.isHitByItem(item, expectedHitState);
        }

    }
}
