﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Auction_Boxing_2.Boxing.PlayerStates;
using System.Diagnostics;

namespace Auction_Boxing_2
{
    enum PlayerDirection
    {
        Right,
        Left
    }

    public class BoxingPlayer : IComparable<BoxingPlayer>
    {
        

        public int GetWidth
        {
            get { return (int)(sprite.Animation.FrameWidth * scale); }
        }

        public int GetHeight
        {
            get { return (int)(sprite.Animation.FrameHeight * scale); }
        }

        #region stats

        // the health everyone starts with without item mods.
        public float baseHealth = 100;

        // the dimensions of the health bar displayed below the player
        public Rectangle rHealthBar;
        public int healthBarMaxWidth;

        protected float maxhealth;

        public float MaxHealth
        {
            get
            {
                return baseHealth; //+ any mods!;
            }
            set
            {
                baseHealth = value;
            }
        }

        protected float maxstamina;

        public float MaxStamina
        {
            get
            {
                return maxstamina;
            }
            set
            {
                maxstamina = value;
            }
        }

        protected float maxmovement;

        public float MaxMovement
        {
            get
            {
                return maxmovement;
            }
            set
            {
                maxmovement = value;
            }
        }

        protected float maxcooldown;

        public float MaxCoolDown
        {
            get { return maxcooldown; }
            set { maxcooldown = value; }
        }

        protected float currenthealth;

        public float CurrentHealth
        {
            get { return currenthealth; }
            set { currenthealth = value; }
        }

        protected float currentstamina;

        public float CurrentStamina
        {
            get { return currentstamina; }
            set { currentstamina = value; }
        }

        public float CurrentCooldown
        {
            get { return CurrentCooldown; }
            set { CurrentCooldown = value; }
        }

        #endregion

        #region some stuff

        public int playerIndex;
        public bool isHit;

        const int TimerInterval = 190;
        const int WaitTimerInterval = 8;


        #endregion

        #region important stuff
        // List of keys currently down
        List<KeyPressed> keysPressed = new List<KeyPressed>();

        public List<KeyPressed> KeysDown
        {
            get
            {
                return keysPressed;
            }
        }

        //Inputs
        public Input_Handler input;

        //Position Vector
        public Vector2 position;

        //speed vector
        Vector2 speed;

        public Vector2 Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        // if its right, its +1, left is -1
        public int direction;

        //hurtbox
        public Rectangle hurtbox;

        public Rectangle Hurtbox
        {
            get
            {
                return hurtbox;
            }
            set
            {
                hurtbox = value;
            }
        }




        

        



        

        // for flipping the player 
        SpriteEffects spriteEffect;

        public float dbleTapTimer;
        public float dbleTapTime = .4f;
        public KeyPressed prevKey;
        public int dbleTapCounter = 0;

        public GameTimer DblJumpTimer;

        public GameTimer DashTimerRight;

        public GameTimer DashTimerLeft;

        //public List<BoxingItem> Items = new List<BoxingItem>();

        //public Item[] equippedItems = new Item[4];



        //public delegate void UseItemEventHandler(ItemInstance item);

        //public event UseItemEventHandler OnUseItem;

        
        Texture2D blank;

        Color color;

        #endregion

        public float GetGroundLevel
        {
            get { return levellevel; }
        }

        public float currentVerticalSpeed = 0;
        public float currentHorizontalSpeed = 0;

        Vector2 startPosition;

        public Boxing_Manager BoxingManager;

        public bool isFalling = false;
        public float levellevel;

        #region Items

        //public Item[] items = new Item[4];
        public bool isReloadingRevolver = false;

        bool[] items = new bool[4];

        // an array of size 4 containing the animations of the items.
        //public Dictionary<string, Animation>[] itemAnimations = new Dictionary<string, Animation>[4];

        #endregion

        #region Animation

        public string currentAnimationKey;

        public AnimationPlayer sprite = new AnimationPlayer();

        //animations attached to statenames
        public Dictionary<string, Animation> animations = new Dictionary<string, Animation>();

        // Haven't implemented. If player is attacking, they get draw last so they appear ontop of other person.
        public short drawPriority = 0;

        #endregion

        #region State Control

        // current state decides what the player is doing at any given time
        // probably the most important part of the player
        public State state;

        public State InternalState
        {
            set
            {
                state = value;
            }
            get
            {
                return state;
            }
        }

        #endregion

        #region PerPixel

        // To handle per pixel collision while scaling sprites, we'll need to use a matrix to hold the position

        public bool isAttacking
        {
            get { return state.isAttack; }//return (state is StatePunch || state is StateCaneBonk); }
        }

        public float scale = 4;
        Vector3 scales = new Vector3(4, 4, 0);
        float rotation = 0;
        public Vector2 Origin
        {
            get { return sprite.Origin; }
        }

        // How to do flipping?!

        public Matrix TransformMatrix
        {
            get
            {
                return 
                    Matrix.CreateTranslation(new Vector3(new Vector2(-1 * Origin.X, -1 * Origin.Y), 0.0f)) *
                    Matrix.CreateScale(scales) *
                    Matrix.CreateRotationZ(rotation) *
                    Matrix.CreateTranslation(new Vector3(position, 0.0f));
            }
        }

        // This rectangle is for preliminary checking. If two players collision rectangles collide,
        // then we check the pixels.
        public Rectangle BoundingRectangle
        {
            get
            {
                int width = (int)(animations[currentAnimationKey].FrameWidth * scale);
                int height = (int)(animations[currentAnimationKey].FrameHeight * scale);

                return new Rectangle((int)position.X - width / 2, (int)position.Y - height,
                    width, height);
            }
        }

        public Rectangle NonScaledBoundingRectangle
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y,
                    animations[currentAnimationKey].FrameWidth, animations[currentAnimationKey].FrameHeight);

            }

        }

        public Rectangle CollisionRectangle;

        // Get the color data of the current frame of the current animation
        public Color[] Get1DColorData
        {
            get { return sprite.Get1DColorDataArray(); }
        }

        #endregion

        #region ComboControl

        List<KeyPressed> comboKeys = new List<KeyPressed>();
        int comboCounter;

        float comboTimer = 0.0f;
        float comboTime = .75f; // the time between keys

        KeyPressed[,] combinations = new KeyPressed[4,3];

        #endregion

        public bool isDead;

        public BoxingPlayer(Boxing_Manager bm, int playerIndex, Vector2 startPosition, Dictionary<string, Animation> animations, Input_Handler input, Color color,
            Texture2D blank, Rectangle healthBar)//items)
        {
            this.BoxingManager = bm;

            // To test the accuracy of the collision rect
            this.blank = blank;

            this.playerIndex = playerIndex;

            this.animations = animations;

            this.input = input;

            this.color = color; 

            this.startPosition = startPosition;
            levellevel = startPosition.Y; // start on ground

            this.rHealthBar = healthBar;
            healthBarMaxWidth = healthBar.Width;

            // Listen for input
            input.OnKeyDown += HandleKeyDown;
            input.OnKeyRelease += HandleKeyRelease;

            // 
            items[0] = true;
            items[1] = true;
            items[2] = true;
            items[3] = true;

            combinations[0, 0] = KeyPressed.Defend;
            combinations[0, 1] = KeyPressed.Up;
            combinations[0, 2] = KeyPressed.Attack;

            combinations[1, 0] = KeyPressed.Defend;
            combinations[1, 1] = KeyPressed.Right;
            combinations[1, 2] = KeyPressed.Attack;

            combinations[2, 0] = KeyPressed.Defend;
            combinations[2, 1] = KeyPressed.Down;
            combinations[2, 2] = KeyPressed.Attack;

            combinations[3, 0] = KeyPressed.Defend;
            combinations[3, 1] = KeyPressed.Left;
            combinations[3, 2] = KeyPressed.Attack;

            

            // Set players for first round.
            Reset(startPosition);
        }

        public void LoadContent(ContentManager Content, Dictionary<string, Animation> animations)//, Item[] Items)
        {
        }

        // Reset stats for next round
        public void Reset(Vector2 startPosition)
        {
            currentAnimationKey = "Idle";

            position = startPosition;
            levellevel = startPosition.Y;
            sprite.PlayAnimation(animations[currentAnimationKey]);
            
            // Set our collision rect. The position represents the bottom center of the sprite.
            ChangeAnimation(currentAnimationKey);
            CollisionRectangle = BoundingRectangle;

            // initial state
            this.state = new StateStopped(this);

            // Set the direction of the player based on which player they are.
            switch (playerIndex)
            {
                case (0):
                case (2):
                    direction = 1;
                    spriteEffect = SpriteEffects.None;
                    break;
                case (1):
                case (3):
                    direction = -1;
                    spriteEffect = SpriteEffects.FlipHorizontally;
                    break;
            }

            spriteEffect = SpriteEffects.None;

            // Set base stats
            CurrentHealth = MaxHealth;

            isDead = false;
            //CurrentStamina = MaxStamina;

            //maxcooldown = Tools.BASE_COOLDOWN;
            //maxmovement = Tools.BASE_MOVEMENT;
        }

        public void Update(GameTime gameTime)
        {
            if (dbleTapTimer > 0)
                dbleTapTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
                dbleTapCounter = 0;

            if (comboTimer > 0)
                comboTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            state.Update(gameTime);

            rHealthBar.Width = (int)(healthBarMaxWidth * (CurrentHealth / MaxHealth));

            if (CurrentHealth <= 0)
                isDead = true;

            if (!(state is StateJump ))//|| state is StateFall))
            {
                isFalling = false;
            }

            // keep the collision rect current to the position.
            //CollisionRect.X = (int)position.X - CollisionRect.Width / 2;
            //CollisionRect.Y = (int)position.Y - CollisionRect.Height;

            //HandleState();

            //handleMovement();

            //handleDirection();


        }

        public void ChangeAnimation(string index)
        {
            currentAnimationKey = index;

            sprite.PlayAnimation(animations[currentAnimationKey]);

            //Origin = sprite.Origin;
        }

        public void ChangeDirection(int i)
        {
            direction = i;
            if (i == -1)
            {
                spriteEffect = SpriteEffects.FlipHorizontally;

                scales.X = -1 * scale;
                
                // We have to flip the matrix as well!
                //_scaleMatrix *= (-1,1); How?!
            }
            else if (i == 1)
            {
                spriteEffect = SpriteEffects.None;

                scales.X = scale;
            }

            //scales.X *= -1;
        }



        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            

            //spriteBatch.Draw(blank, CollisionRect, Color.Red);
            //spriteBatch.Draw(blank, BoundingRectangle, Color.Red);
            // Draw sprite
            //sprite.Draw(gameTime, spriteBatch, color, position, 0, Vector2.Zero, 0f, spriteEffect);
            rHealthBar.X = (int)position.X - healthBarMaxWidth / 2;
            rHealthBar.Y = (int)position.Y;// +GetHeight;
            spriteBatch.Draw(blank, rHealthBar, Color.Red);
            sprite.Draw(gameTime, spriteBatch, BoundingRectangle, 0, color, spriteEffect);
           
        }

        // Add a key to the list
        public void HandleKeyDown(int player_index, KeyPressed key)
        {
            if (!KeysDown.Contains(key))
                KeysDown.Add(key);

            if (dbleTapCounter == 2)
            {
                dbleTapCounter = 0;

            }

            if(dbleTapCounter == 0)
                dbleTapTimer = dbleTapTime;

            dbleTapCounter++;

            // Add the new key, there will always be 3 keys in comboKeys.
            comboKeys.Add(key);
            if (comboKeys.Count > 3)
                comboKeys.RemoveAt(0);

            if (comboTimer > 0 && comboCounter == 1)
            {
                //Debug.WriteLine("Check combo!");
                CheckForCombo();
                comboTimer = comboTime; // reset the combo timer.
                comboCounter = 0; // reset the combo counter
            }
            else if (comboCounter > 1)
            {
                comboCounter = 0; // reset the combo counter
            }

            string s = "";
            for(int i = 0; i < comboKeys.Count; i++)
            {
                s +=  comboKeys[i].ToString() + " ";
            }
            Debug.WriteLine("Keys: " + s);
        }

        // Remove the key from the list
        public void HandleKeyRelease(int player_index, KeyPressed key)
        {
            if (KeysDown.Contains(key))
                KeysDown.Remove(key);


            prevKey = key;

            


            if (comboTimer > 0)
            {
                comboCounter++;

                //Debug.WriteLine("Combo Counter = " + comboCounter);
                comboTimer = comboTime; // reset the combo timer.
            }
            else
            {
                comboTimer = comboTime; // reset the combo timer.
                comboCounter = 0; // reset the combo counter
            }
        }

        public void CheckForCombo()
        {
            int c = 0;
            for (int i = 0; i < 4; i++)
            {
                c = 0;
                for (int j = 0; j < 3; j++)
                {
                    if (items[i] && comboKeys[j] == combinations[i,j])
                    {
                        c++;

                    }
                    // We have a combo if all keys match!
                    if (c == 3)
                    {
                        Debug.WriteLine("Combo detected!");
                        state.OnCombo(i);
                    }
                }
            }

        }


        public void HandleState()
        {
            state.HandleState();
        }

        public void handleDirection()
        {
            state.HandleDirection();
            /*foreach (BoxingItem item in Items)
            {
                item.HandleDirection(PlayerEffect);
            }*/
        }

        public void handleMovement()
        {
            state.HandleMovement();
        }

        public void handleCollision(List<BoxingPlayer> Players)
        {
            state.HandleCollision(Players);
        }

        //public void isHit(BoxingPlayer player)
        //{
            //state.
        //}

        /// <summary>
        /// Calls the state to handle any effects to a hit player and this player
        /// </summary>
        /// <param name="hitPlayer">The player hit by this player</param>
        public void HitOtherPlayer(BoxingPlayer hitPlayer)
        {
            state.HitOtherPlayer(hitPlayer);
        }

        /*public void Hit(Item item)
        {
            State state = InternalState;
            InternalState = new StateHit(state, item);
            currenthealth -= item.attack;
        }*/

        public bool IsKeyDown(KeyPressed key)
        {
            return this.KeysDown.Contains(key);
        }

        /*public void UseItemEvent(int itemindex)
        {
            if (equippedItems[itemindex] != null && OnUseItem != null)
            {

                State state = InternalState;
                InternalState = equippedItems[itemindex].GenerateState(itemindex, direction, state);

            }
        }*/

        /*public void CreateInstance(int itemindex)
        {
            if (equippedItems[itemindex] != null && OnUseItem != null)
            {
                SpriteEffects effect = SpriteEffects.None;

                if (direction == DirectionType.Right)
                {
                    effect = SpriteEffects.FlipHorizontally;
                }

                OnUseItem(equippedItems[itemindex].GenerateInstance(
                        new Vector3(position.X, position.Y, position.Z), playerindex, effect));
            }
        }*/
        
        public int CompareTo(BoxingPlayer player)
        {
            return this.position.Y.CompareTo(player.position.Y);
        }

       

        /// <summary>
        /// Calculates an axis aligned rectangle which fully contains an arbitrarily
        /// transformed axis aligned rectangle.
        /// </summary>
        /// <param name="rectangle">Original bounding rectangle.</param>
        /// <param name="transform">World transform of the rectangle.</param>
        /// <returns>A new rectangle which contains the trasnformed rectangle.</returns>
        public Rectangle CalculateCollisionRectangle()
        {

            // whoops! Just using htis for now.
            return BoundingRectangle;

            Rectangle rectangle = NonScaledBoundingRectangle;
            Matrix transform = TransformMatrix;

            // Get all four corners in local space
            Vector2 leftTop = new Vector2(rectangle.Left, rectangle.Top);
            Vector2 rightTop = new Vector2(rectangle.Right, rectangle.Top);
            Vector2 leftBottom = new Vector2(rectangle.Left, rectangle.Bottom);
            Vector2 rightBottom = new Vector2(rectangle.Right, rectangle.Bottom);

            // Transform all four corners into work space
            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            // Find the minimum and maximum extents of the rectangle in world space
            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            // Return that as a rectangle
            return new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

    }
}