﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Auction_Boxing_2.Boxing.PlayerStates; // Fix this!
using System.Diagnostics;

namespace Auction_Boxing_2
{
    enum boxingstate
    {
        idle,
        roundstart,
        roundend,
        box,
        stats
    }

    // Rounds not yet implemented
    public class Boxing_Manager
    {
        #region fields

        Texture2D background;

        Level level;

        KeyboardState kb;

        Input_Handler[] inputs = new Input_Handler[4];

        BoxingPlayer[] players = new BoxingPlayer[4];

        bool collide = false;

        //List<BoxingPlayer> Players = new List<BoxingPlayer>();

        Rectangle bounds;

        public static Rectangle Battlefield;

        protected PlayerStatDisplay[] displays = new PlayerStatDisplay[4];

        string shit = "hit";
        string smoving = "moving";
        string sstopped = "stopped";
        string sattack1 = "Jump";
        string sattack2 = "Attack";
        string sattack3 = "attack3";
        string sjump = "jumping";
        string spickup = "puckup";
        string sdead = "dead";
        string scharging = "charging";
        string name = "ffsp1";
        string scasting = "casting";

        List<string> StateNames = new List<string>();

        Dictionary<string, Animation> playerAnims = new Dictionary<string, Animation>();
        Dictionary<string, Animation> itemAnims = new Dictionary<string, Animation>();
        Dictionary<string, BitMap> bitmaps = new Dictionary<string, BitMap>();

        SpriteFont font;

        Texture2D blank;

        // When players use items, the item attacks manifest as objects.
        public List<ItemInstance> itemInstances;
        int instanceID = 0;

        public void addBowlerHat(BoxingPlayer p)
        {
            itemInstances.Add(new BowlerHatInstance(p, itemAnims, instanceID++));
        }

        public void removeBowlerHat(BowlerHatInstance hat)
        {
            itemInstances.Remove(hat);
        }

        Vector2[] playerStartPositions = new Vector2[4];

        GraphicsDevice gd;

        #endregion

        #region ItemNames

        string cane = "cane";

        # endregion

        #region ItemField



        List<string> ItemNames = new List<string>();


        //Item[,] Items;
        //List<BoxingItem>[] BoxingItems = { new List<BoxingItem>(), new List<BoxingItem>(), new List<BoxingItem>(), new List<BoxingItem>() };

        Dictionary<string, Texture2D> ItemTextures = new Dictionary<string, Texture2D>();

        #endregion

        Rectangle healthBarDimensions;

        public int NumRounds { get; set; }
        int currentRound = 1;
        float roundStartTimer;
        float roundStartTime = 3f;

        boxingstate state;

        bool drawCollisionBoxes = false;

        int deadCount = 0;
        int winner;
        float winTime = 2f;
        float winTimer = 0;
        bool isRoundOver;

        float restartTime = 3;
        float restartTimer = 0;

        int numberOfPlayers;

        public float GetGroundLevel
        {
            get
            {
                return playerStartPositions[0].Y;
            }
        }

        public Boxing_Manager(ContentManager content, Rectangle ClientBounds, Input_Handler[] inputs,
            GraphicsDevice gd)
        {
            this.gd = gd;

            this.bounds = new Rectangle(0, 0, ClientBounds.Width, ClientBounds.Height);
            this.inputs = inputs;

            background = content.Load<Texture2D>("Boxing/AB Background");
            font = content.Load<SpriteFont>("Menu/menufont");
            blank = content.Load<Texture2D>("White");

            #region Add StateNames
            StateNames.Add(smoving);
            StateNames.Add(sstopped);
            StateNames.Add(sjump);
            StateNames.Add(sattack1);
            StateNames.Add(sattack3);
            StateNames.Add(scasting);
            StateNames.Add(shit);
            StateNames.Add(sdead);
            #endregion

            #region ItemNames
            ItemNames.Add(cane);

            #endregion

            state = boxingstate.idle;

            for (int i = 0; i < 4; i++)
                playerStartPositions[i] = new Vector2(bounds.X + bounds.Width / 5 * (i + 1), 4 * bounds.Height / 5);

            level = new Level(this, ClientBounds, blank, background);
            //level.platforms[level.platforms.Length - 1].Y = (int)playerStartPositions[0].Y;

            NumRounds = 1;
            itemInstances = new List<ItemInstance>();

            healthBarDimensions = new Rectangle(0, 0, ClientBounds.Width / 16, ClientBounds.Height / 80);

            LoadContent(content);
        }

        public void LoadContent(ContentManager Content)
        {
            // Create animation library to pass to players.

            #region set frame width

            // frame widths
            int wIdle = 15;
            int wWalk = 12;
            int wRun = 27;
            int wJump = 20;
            int wLand = 18;
            int wPunch = 37;
            int wPunchHit = 34;
            int wDodge = 17;
            int wBlock = 15;
            int wDown = 43;
            int wDuck = 16;

            // item frame widths
            int wCaneBonk = 54;
            int wCaneHit = 19;
            int wCanePull = 76;
            int wCaneBalance = 28;

            int wRevolverShoot = 56;
            int wRevolverHit = 15;
            int wRevolverReload = 56;

            int wBowlerThrow = 34;
            int wBowlerCatch = 37;
            int wBowlerReThrow = 34;

            int wBowlerHat = 6;
            
            #endregion

            #region set frame times

            // frame times
            float fIdle = 0.1f;
            float fWalk = 0.1f;
            float fRun = 0.05f;
            float fJump = 0.1f;
            float fLand = 0.1f;
            float fPunch = 0.09f;
            float fPunchHit = 0.1f;
            float fDodge = 0.05f;
            float fBlock = 0.1f;
            float fDown = 0.08f;
            float fDuck = 0.08f;

            // item frame times
            float fCaneBonk = 0.1f;
            float fCaneHit = 0.1f;
            float fCanePull = 0.1f;
            float fCaneBalance = 0.1f;

            float fRevolverShoot = 0.1f;
            float fRevolverHit = 0.05f;
            float fRevolverReload = 0.1f;

            float fBowlerThrow = 0.1f;
            float fBowlerCatch = 0.1f;
            float fBowlerReThrow = 0.1f;

            #endregion

            #region load textures

            // Load textures
            Texture2D idle = Content.Load<Texture2D>("Boxing/Player_Idle_Side");
            Texture2D walk = Content.Load<Texture2D>("Boxing/Player_Walking_Side");
            Texture2D run = Content.Load<Texture2D>("Boxing/Player_Running_Side");
            Texture2D jump = Content.Load<Texture2D>("Boxing/Player_Jump");
            Texture2D land = Content.Load<Texture2D>("Boxing/Player_Land");
            Texture2D punch = Content.Load<Texture2D>("Boxing/Player_Punch");
            Texture2D punchHit = Content.Load<Texture2D>("Boxing/Player_Punch_Hit");
            Texture2D dodge = Content.Load<Texture2D>("Boxing/Player_Dodge");
            Texture2D block = Content.Load<Texture2D>("Boxing/Player_Block");
            Texture2D down = Content.Load<Texture2D>("Boxing/Player_Knocked_Down");
            Texture2D duck = Content.Load<Texture2D>("Boxing/Player_Duck");

            // player item use textures
            Texture2D caneBonk = Content.Load<Texture2D>("BoxingItems/Player_Cane");
            Texture2D caneHit = Content.Load<Texture2D>("BoxingItems/Player_Cane_Hit");
            Texture2D canePull = Content.Load<Texture2D>("BoxingItems/Player_Cane_Pull");
            Texture2D caneBalance = Content.Load<Texture2D>("BoxingItems/Player_Cane_Balance");

            Texture2D revolverShoot = Content.Load<Texture2D>("BoxingItems/Player_Revolver");
            Texture2D revolverHit = Content.Load<Texture2D>("BoxingItems/Player_Revolver_Hit");
            Texture2D revolverReload = Content.Load<Texture2D>("BoxingItems/Player_Revolver_Reload");

            Texture2D bowlerThrow = Content.Load<Texture2D>("BoxingItems/Player_BowlerHat");
            Texture2D bowlerCatch = Content.Load<Texture2D>("BoxingItems/Player_BowlerHat_Catch");
            Texture2D bowlerReThrow = Content.Load<Texture2D>("BoxingItems/Player_BowlerHat_ReThrow");

            // item textures
            
            Texture2D bowlerHat = Content.Load<Texture2D>("BoxingItems/BowlerHat_Instance");

            #endregion

            #region initialize animations

            // Initialize animations;
            playerAnims.Add("Idle", new Animation(idle, fIdle, true, wIdle));
            playerAnims.Add("Walk", new Animation(walk, fWalk, true, wWalk));
            playerAnims.Add("Run", new Animation(run, fRun, true, wRun));
            playerAnims.Add("Jump", new Animation(jump, fJump, false, wJump));
            playerAnims.Add("Land", new Animation(land, fLand, true, wLand));
            playerAnims.Add("Punch", new Animation(punch, fPunch, false, wPunch));
            playerAnims.Add("PunchHit", new Animation(punchHit, fPunchHit, true, wPunchHit));
            playerAnims.Add("Dodge", new Animation(dodge, fDodge, true, wDodge));
            playerAnims.Add("Block", new Animation(block, fBlock, true, wBlock));
            playerAnims.Add("Down", new Animation(down, fDown, false, wDown));
            playerAnims.Add("Duck", new Animation(duck, fDuck, false, wDuck));

            // player item use animations
            playerAnims.Add("CaneBonk", new Animation(caneBonk, fCaneBonk, false, wCaneBonk));
            playerAnims.Add("CaneHit", new Animation(caneHit, fCaneHit, false, wCaneHit));
            playerAnims.Add("CanePull", new Animation(canePull, fCanePull, false, wCanePull));
            playerAnims.Add("CaneBalance", new Animation(caneBalance, fCaneBalance, false, wCaneBalance));

            playerAnims.Add("RevolverShoot", new Animation(revolverShoot, fRevolverShoot, false, wRevolverShoot));
            playerAnims.Add("RevolverHit", new Animation(revolverHit, fRevolverHit, false, wRevolverHit));
            playerAnims.Add("RevolverReload", new Animation(revolverReload, fRevolverReload, false, wRevolverReload));

            playerAnims.Add("bowlerThrow", new Animation(bowlerThrow, fBowlerThrow, false, wBowlerThrow));
            playerAnims.Add("bowlerCatch", new Animation(bowlerCatch, fBowlerCatch, false, wBowlerCatch));
            playerAnims.Add("bowlerReThrow", new Animation(bowlerReThrow, fBowlerReThrow, false, wBowlerReThrow));

            // item animations

            itemAnims.Add("bowlerHat", new Animation(bowlerHat, 1f, true, wBowlerHat));

            #endregion

            // Initialize bitmaps
            bitmaps.Add("Punch", new BitMap(Content.Load<Texture2D>("Boxing/Bitmaps/Player_Punch_Bitmap")));
            
            // item animations
            bitmaps.Add("CaneBonk", new BitMap(Content.Load<Texture2D>("Boxing/Bitmaps/Player_Cane_Bitmap")));
            bitmaps.Add("CanePull", new BitMap(Content.Load<Texture2D>("Boxing/Bitmaps/Player_Cane_Pull_Bitmap")));
        }

        // Apply's settings gathered before the boxing begins.
        public void ApplySettings(Color[] colors)
        {
            for(int i = 0; i < 4; i++)
            {
                if(colors[i] != Color.Transparent)
                    players[i] = new BoxingPlayer(this, i, playerStartPositions[i], playerAnims, inputs[i], colors[i], blank,
                        healthBarDimensions, level.platforms[level.platforms.Length - 1]); // Figure out the boxing players.
            }
        }

        // Resets the game for another round.
        public void Reset()
        {
            // Reset the state
            state = boxingstate.roundstart;
            roundStartTimer = roundStartTime;

            numberOfPlayers = 0;

            // Reset the players
            for (int i = 0; i < 4; i++)
            {
                if (players[i] != null)
                {
                    players[i].Reset(playerStartPositions[i]);
                    numberOfPlayers++;
                }
            }

            Debug.WriteLine("num of players = " + numberOfPlayers);

            // reset item instances
            itemInstances.Clear();

            deadCount = 0;
            isRoundOver = false;
        }

        public void Activate(ContentManager Content)
        {
            /*List<Item>[] equippedItems = { new List<Item>(), new List<Item>(), new List<Item>(), new List<Item>() };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    equippedItems[i].Add(new Cane(Content.Load<Texture2D>("BoxingItems/cane"),
                    Content.Load<Texture2D>("BoxingItems/Cane_Attack"),
                    Content.Load<Texture2D>("LoadOut/cane_icon")));
                    equippedItems[i].Add(new Bowler_Hat(Content.Load<Texture2D>("Items/bowlerhat_image"),
                        Content.Load<Texture2D>("BoxingItems/Bowler_Attack"),
                        Content.Load<Texture2D>("LoadOut/bowlerhat_icon")));
                    equippedItems[i].Add(new Revolver(Content.Load<Texture2D>("Items/revolver_image"),
                        Content.Load<Texture2D>("BoxingItems/Revolver_Attack"),
                        Content.Load<Texture2D>("LoadOut/revolver_icon")));
                    equippedItems[i].Add(new Boots(Content.Load<Texture2D>("Items/Boots_Image"),
                        Content.Load<Texture2D>("BoxingItems/gust_attack"),
                        Content.Load<Texture2D>("LoadOut/boots_icon"),
                        Content.Load<Texture2D>("Boxing/ffsp1charge"),
                        Content.Load<Texture2D>("Boxing/ffsp1jumping")));
                }

            }
            this.Items = equippedItems;

            Battlefield = new Rectangle(0, 140, bounds.Width, 55);

            List<BoxingPlayer> activePlayers = new List<BoxingPlayer>();

            for (int i = 0; i < 4; i++)
            {
                activePlayers.Add(new BoxingPlayer(bounds.Width * (1 - .9f), bounds.Height * (1 - i * .1f) - 100, "player" + i, i, Tools.WIDTH, Tools.HEIGHT, inputs[i]));
                activePlayers[i].OnUseItem += CreateInstance;
            }

            Players = activePlayers;
            Item[] equipment = new Item[4];
            for (int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                     equipment[j] = state_manager.equipment[i,j];
                
                Players[i].LoadContent(Content, ATextures, equipment);
            }

            for (int i = 0; i < 4; i++)
            {

                displays[i] = new PlayerStatDisplay(font, i + 1, Players[i], inputs[i], new Rectangle((i * bounds.Width / 4) + 1, 1, (bounds.Width / 4) - 2, bounds.Height / 4),
                    Content.Load<Texture2D>("White"), Content.Load<Texture2D>("White"));
            }*/
        }

        /*public void CreateInstance(ItemInstance instance)
        {
            /*itemInstances.Add(instance);
            Debug.WriteLine("Item instance = " + (instance is BowlerHatInstance).ToString());
        }*/

        // Find the first player in front of player
        public BoxingPlayer GetPlayerInFront(BoxingPlayer p, float y)
        {
            BoxingPlayer f = null;

            float min = 0;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null && players[i].playerIndex != p.playerIndex && !players[i].isDead)
                {
                    if (p.direction == 1
                        && players[i].position.X > p.position.X 
                        && y > players[i].position.Y - players[i].GetHeight
                        && y < players[i].position.Y)
                    {
                        float d = players[i].position.X - p.position.X;
                        if (min == 0 || d < min)
                        {
                            min = d;
                            f = players[i];
                        }
                    }
                    else if (p.direction == -1
                        && players[i].position.X < p.position.X
                        && y > players[i].position.Y - players[i].GetHeight
                        && y < players[i].position.Y)
                    {
                        float d = p.position.X - players[i].position.X;
                        if (min == 0 || d < min)
                        {
                            min = d;
                            f = players[i];
                        }
                    }
                }
            }
            return f;
        }

        public bool Update(GameTime gameTime)
        {
            switch (state)
            {
                // The idle state is just to display the background while the settings are configured.
                case(boxingstate.idle):
                    // if animated background, update it.
                    break;
                // Will display the "Round X, Begin!" animation.
                case(boxingstate.roundstart):
                    if (roundStartTimer > 0)
                        roundStartTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    else
                        state = boxingstate.box;
                    break;
                // Will handle all the logic for the boxing; player updates, collision, etc.
                case(boxingstate.box):
                    foreach (ItemInstance item in itemInstances)
                    {
                        item.Update(gameTime);
                        HandleCollisions(item);
                    }
                    for(int i = 0; i < 4; i++)
                    {
                        BoxingPlayer player = players[i];
                        if(player != null)
                        {
                            player.Update(gameTime);

                            // keep them from going off the screen
                            player.position.X = MathHelper.Clamp(player.position.X,
                                bounds.Left + player.GetWidth / 2, bounds.Right - player.GetWidth / 2);//- (player.GetWidth / 2 - 15 * player.scale), bounds.Right - player.GetWidth / 2 - (player.GetWidth / 2 - 15 * player.scale));

                            HandleCollisions(i);
                        }

                    }

                    // Handle the winning
                    if (winTimer > 0)
                        winTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (deadCount >= numberOfPlayers - 1)
                    {
                        isRoundOver = true;
                        winTimer = winTime;


                        // Who's the winner?
                        for (int i = 0; i < 4; i++)
                        {
                            if (players[i] != null && !players[i].isDead)
                            {
                                winner = i+1;
                            }
                        }

                        state = boxingstate.roundend;
                        restartTimer = restartTime;
                    }

                    break;
                case (boxingstate.roundend):
                    if (currentRound == NumRounds) // this was the last round, show stats
                    {
                        Debug.WriteLine("Round {0} Complete!", currentRound);
                        state = boxingstate.stats;
                    }
                    else // reset for next round
                    {
                        if (restartTimer > 0)
                        {
                            restartTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        else
                        {
                            Debug.WriteLine("Round {0} Complete!", currentRound);
                            currentRound++;
                            Reset();
                        }
                    }
                    break;
                case (boxingstate.stats):
                    Debug.WriteLine("A winner is you!");
                    return false;
                    break;
            }

            return true;

            /*
            kb = Keyboard.GetState();

            Players.Sort();

            HandleCollisions();

            foreach (BoxingPlayer player in Players)
            {
                //player.handleCollision(Players);
                if (player.InternalState is StateCharging)
                {
                    player.handleCollision(Players);
                }
                player.update(gameTime);
            }

            List<ItemInstance> instancesToRemove = new List<ItemInstance>();

            foreach (ItemInstance item in itemInstances)
            {
                item.Update(gameTime);
                if (item.end)
                    instancesToRemove.Add(item);
            }

            for (int i = 0; i < instancesToRemove.Count; i++)
                itemInstances.Remove(instancesToRemove[i]);

            foreach (PlayerStatDisplay display in displays)
            {
                display.Update();
            }

            //Switch to Menu by pressing R
            if (kb.IsKeyDown(Keys.R))
                return true;
            else
                return false;*/
        }

        public float GetLowerPlatformLevel(float platformlevel)
        {
            float l = platformlevel;
            for (int i = 0; i < level.platforms.Length; i++)
            {
                if (level.platforms[i].Y > l)
                {
                    l = level.platforms[i].Y;
                    return l;// 
                }

            }
            return l;
        }

        public Rectangle GetLowerPlatform(Vector2 pos)
        {
            //float l = platformlevel;
            for (int i = 0; i < level.platforms.Length; i++)
            {
                if (pos.X + 15 > level.platforms[i].X && pos.X - 15 < level.platforms[i].X + level.platforms[i].Width && level.platforms[i].Y > pos.Y)
                {
                   // l = level.platforms[i].Y;
                    return level.platforms[i];// 
                }

            }
            return level.platforms[level.platforms.Length - 1];
        }

        /*public bool TexturesCollide(Color[,] image1, Matrix matrix1, Color[,] image2, Matrix matrix2)
        {
            Matrix mat1to2 = matrix1 * Matrix.Invert(matrix2);
            int width1 = image1.GetLength(0);
            int height1 = image1.GetLength(1);
            int width2 = image2.GetLength(0);
            int height2 = image2.GetLength(1);

            for (int x1 = 0; x1 < width1; x1++)
            {
                for (int y1 = 0; y1 < height1; y1++)
                {
                    Vector2 pos1 = new Vector2(x1, y1);
                    Vector2 pos2 = Vector2.Transform(pos1, mat1to2);

                    int x2 = (int)pos2.X;
                    int y2 = (int)pos2.Y;
                    if ((x2 >= 0) && (x2 < width2))
                    {
                        if ((y2 >= 0) && (y2 < height2))
                        {
                            if (image1[x1, y1].A != 0)
                            {
                                if (image2[x2, y2].A != 0)
                                {
                                    
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }*/

        public void HandleCollisions(ItemInstance item)
        {
            // For attacking player-on-player collision (Uses per pixel)
            for (int i = 0; i < 4; i++)
            {
                if (players[i] != null && !players[i].isDead)
                {
                    if (players[i] != item.player) // collision with unfriendly player
                    {
                        // TODO : check for collision with player
                    }
                }
            }
        }

        public void HandleCollisions(int player)
        {
            // level collision
            if (players[player].currentVerticalSpeed > 0 && !(players[player].state is StateFall))
            {
                Vector2 bottomLeft = new Vector2(players[player].position.X - players[player].GetWidth / 2,
                    players[player].position.Y);// + players[player].GetHeight - players[player].GetHeight / 2);
                Vector2 bottomRight = new Vector2(players[player].position.X + players[player].GetWidth - players[player].GetWidth / 2,
                    players[player].position.Y);// + players[player].GetHeight - players[player].GetHeight / 2);

                for (int j = level.platforms.Length - 1; j >= 0; j--)//(int j = 0; j < level.platforms.Length; j++)
                {
                    if (j != 0)
                    {
                        if ((bottomLeft.X > level.platforms[j].X
                            && bottomLeft.X < level.platforms[j].X + level.platforms[j].Width
                            && bottomLeft.Y < level.platforms[j].Y
                            && bottomLeft.Y > level.platforms[j - 1].Y + level.platforms[j - 1].Height) ||
                            //&& bottomLeft.Y > level.platforms[j].Y
                            //&& bottomLeft.Y < level.platforms[j].Y + level.platforms[j].Height) ||
                            (bottomRight.X > level.platforms[j].X
                            && bottomRight.X < level.platforms[j].X + level.platforms[j].Width
                             && bottomLeft.Y < level.platforms[j].Y
                             && bottomLeft.Y > level.platforms[j - 1].Y + level.platforms[j - 1].Height))
                        //&& bottomRight.Y > level.platforms[j].Y
                        //&& bottomRight.Y < level.platforms[j].Y + level.platforms[j].Height))
                        {

                            players[player].platform = level.platforms[j];
                            players[player].levellevel = level.platforms[j].Y;//position.Y = level.platforms[j].Y;
                        }
                    }
                    else // top level
                    {
                        if ((bottomLeft.X > level.platforms[j].X
                            && bottomLeft.X < level.platforms[j].X + level.platforms[j].Width
                            && bottomLeft.Y < level.platforms[j].Y) ||
                            (bottomRight.X > level.platforms[j].X
                            && bottomRight.X < level.platforms[j].X + level.platforms[j].Width
                             && bottomLeft.Y < level.platforms[j].Y))
                        {
                            players[player].levellevel = level.platforms[j].Y;
                        }
                    }
                }
            }

            // For attacking player-on-player collision (Uses per pixel)
            for(int i = 0; i < 4; i++)
            {
                // pvp
                if(i != player && players[i] != null && !players[i].isDead)
                {

                    BoxingPlayer thisPlayer = players[player];
                    BoxingPlayer otherPlayer = players[i];

                    if (thisPlayer.BoundingRectangle.Intersects(otherPlayer.BoundingRectangle))
                    {
                        if (thisPlayer.isAttacking && (thisPlayer.GetGroundLevel == otherPlayer.GetGroundLevel))
                        {
                            collide = thisPlayer.IntersectPixels(otherPlayer);
                            if (collide)
                                thisPlayer.HitOtherPlayer(otherPlayer);
                        }
                    }
                }
            }
            
            /*
            foreach (ItemInstance instance in itemInstances)
            {
                if (!instance.isEffect)
                {
                    foreach (BoxingPlayer player in Players)
                    {
                        if (player.Hurtbox.Intersects(instance.hitbox) && player.playerindex != instance.playerId)
                        {
                            if (Math.Abs(player.Position.Y - instance.position.Y) <= 20 && !(player.InternalState is StateHit))
                    /            player.Hit(instance.item);
                            Debug.WriteLine(player.Position.Y - instance.position.Y);
                            //if(item is BowlerHatInstance && Math.Abs(player.Position.Y - item.position.Y 
                        }
                    }
                }
            }
             * */
        }

        public Color[] GetBitmapData(string key, int index, int framew, int frameh)
        {
            return bitmaps[key].GetData(index, framew, frameh);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            switch (state)
            {
                case (boxingstate.idle):
                    spriteBatch.Draw(background, bounds, Color.White);
                    break;
                case (boxingstate.roundstart):
                    spriteBatch.Draw(background, bounds, Color.White);

                    level.Draw(spriteBatch);

                    // Draw the round start count down
                    string s = "Round Starting in\n" + (int)roundStartTimer;
                    spriteBatch.DrawString(font, s, new Vector2(bounds.Width / 2 - font.MeasureString(s).X,
                        bounds.Height / 2 - font.MeasureString(s).Y), Color.Yellow);

                    // Draw the players
                    foreach (BoxingPlayer player in players)
                    {
                        if (player != null)
                            player.Draw(gameTime, spriteBatch);
                    }

                    break;
                case (boxingstate.box):
                    //if (!collide)
                    //    spriteBatch.Draw(background, bounds, Color.White);
                    //else
                    //    spriteBatch.Draw(background, bounds, Color.White);

                    level.Draw(spriteBatch);

                    foreach (ItemInstance item in itemInstances)
                    {
                        item.Draw(gameTime, spriteBatch);
                    }

                    // Draw the players
                    foreach (BoxingPlayer player in players)
                    {
                        if (player != null)
                        {
                            Vector2 bottomLeft = new Vector2(player.position.X - player.GetWidth / 2,
                                player.position.Y);// + player.GetHeight - player.GetHeight / 2);
                            Vector2 bottomRight = new Vector2(player.position.X + player.GetWidth - player.GetWidth / 2,
                                player.position.Y);// + player.GetHeight - player.GetHeight / 2);

                            spriteBatch.Draw(blank, new Rectangle((int)bottomLeft.X, (int)bottomLeft.Y, 3, 3), Color.Red);
                            spriteBatch.Draw(blank, new Rectangle((int)bottomRight.X, (int)bottomRight.Y, 3, 3), Color.Red);

                            if (drawCollisionBoxes)
                            {
                                Rectangle playerRectangle = player.CalculateCollisionRectangle();
                                //new Rectangle(0, 0, player.GetWidth / 4, player.GetHeight / 4), player.TransformMatrix);
                                //Debug.WriteLine("DRAWING BOUND");
                                spriteBatch.Draw(blank, playerRectangle, Color.Blue);

                                // Draw the current color data in the top corner

                                // Create a new texture and assign the data to it
                                //Texture2D texture = 

                                //spriteBatch.Draw(player.sprite.GetDataAsTexture(gd), 
                                //new Rectangle(0,0,(int)(player.GetWidth / player.scale), (int)(player.GetHeight / player.scale)), Color.White);
                            }

                            player.Draw(gameTime, spriteBatch);
                        }
                    }
                    break;
                case (boxingstate.roundend):
                    spriteBatch.Draw(background, bounds, Color.White);
                    // Draw the players
                    foreach (BoxingPlayer player in players)
                    {
                        if (player != null)
                        {
                            if (drawCollisionBoxes)
                            {
                                Rectangle playerRectangle = player.CalculateCollisionRectangle();
                                //new Rectangle(0, 0, player.GetWidth / 4, player.GetHeight / 4), player.TransformMatrix);

                                spriteBatch.Draw(blank, playerRectangle, Color.Blue);
                            }

                            player.Draw(gameTime, spriteBatch);


                        }
                    }
                    // Draw the winner!
                    string w = "Player " + winner + " Takes the Round!";
                    spriteBatch.DrawString(font, w,
                        new Vector2(bounds.X + bounds.Width / 2 - font.MeasureString(w).X / 2,
                            bounds.Y + bounds.Height / 2 - font.MeasureString(w).Y / 2), Color.Goldenrod);
                    break;
                case (boxingstate.stats):
                    spriteBatch.Draw(background, bounds, Color.White);
                    // Draw the players
                    foreach (BoxingPlayer player in players)
                    {
                        if (player != null)
                        {
                            if (drawCollisionBoxes)
                            {
                                Rectangle playerRectangle = player.CalculateCollisionRectangle();
                                //new Rectangle(0, 0, player.GetWidth / 4, player.GetHeight / 4), player.TransformMatrix);

                                spriteBatch.Draw(blank, playerRectangle, Color.Blue);
                            }

                            player.Draw(gameTime, spriteBatch);
                        }
                    }
                    // Draw the winner!
                    string win = "Player " + winner + "Wins!";
                    spriteBatch.DrawString(font, win,
                        new Vector2(bounds.X + bounds.Width / 2 - font.MeasureString(win).X / 2,
                            bounds.Y + bounds.Height / 2 - font.MeasureString(win).Y / 2), Color.Goldenrod);
                    break;
            }
        }

        public void NotifyPlayerDeath(int playerIndex)
        {
            deadCount++;
        }
    }
}
