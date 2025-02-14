﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Herbology
{
    //todo finish this (hover item, etc) and abstract it for other crops
    public abstract class Crop : ModTile
    {
        protected readonly int Height;

        protected readonly string TexturePath;

        protected readonly int FrameCount;
        protected readonly int DustType;
        protected readonly int SoundType;
        protected readonly string DropType;

        protected readonly string MapName;
        protected readonly Color MapColor;

        protected readonly bool CustomGroundOnly;

        protected readonly int[] ExtraGroundTypes;
        protected readonly string[] ExtraGroundNames;
        protected readonly AnchorData TopAnchorOverride;
        protected readonly AnchorData BottomAnchorOverride;

        public int FrameHeight;
        public int LastFrame;

        public Crop(string dropType, string mapName, string texturePath, int frameCount, int height = 3, int dustType = DustID.Grass, int soundType = SoundID.Grass, Color mapColor = default, AnchorData topAnchorOverride = default, AnchorData bottomAnchorOverride = default, int[] extraGroundTypes = null, string[] extraGroundNames = null, bool customGroundOnly = false)
        {
            DropType = dropType;

            TexturePath = texturePath;

            FrameCount = frameCount;
            Height = height;
            DustType = dustType;
            SoundType = soundType;

            MapName = mapName;
            MapColor = mapColor;

            TopAnchorOverride = topAnchorOverride;
            BottomAnchorOverride = bottomAnchorOverride;

            ExtraGroundTypes = extraGroundTypes;
            ExtraGroundNames = extraGroundNames;
            CustomGroundOnly = customGroundOnly;
        }

        public override bool Autoload(ref string name, ref string texture)
        {
            if (!string.IsNullOrEmpty(TexturePath))
                texture = TexturePath + name;

            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileNoFail[Type] = true;
            //TileObjectData.newTile.DrawYOffset = 2;

            AnchorData anchor = TopAnchorOverride == default ?
                new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0) :
                TopAnchorOverride;

            List<int> valid = CustomGroundOnly ? 
                new List<int>() : 
                new List<int>() { 
                    TileType<Soil>(), TileType<Trellis>(), TileType<GardenPot>(),
                    TileID.Dirt, TileID.Sand, TileID.Mud, TileID.Grass,
                    TileID.HallowedGrass, TileID.CorruptGrass, TileID.FleshGrass, TileID.JungleGrass 
                };

            if (ExtraGroundTypes != null)
                valid.AddRange(ExtraGroundTypes);

            if(ExtraGroundNames != null)
                foreach (string name in ExtraGroundNames)
                    valid.Add(mod.TileType(name));

            QuickBlock.QuickSetFurniture(this, 1, Height, DustType, SoundType, false, 
                MapColor == default ? new Color(200, 255, 220) : MapColor,
                false, false, MapName, anchor, TopAnchorOverride, valid.ToArray());

            drop = mod.ItemType(DropType);

            FrameHeight = 16 * Height;
            LastFrame = (FrameCount - 1) * FrameHeight;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => 
            num = 1;

        public override bool Drop(int i, int j)
        {
            Tile thisTile = Main.tile[i, j];
            if (thisTile.frameY == ((Height - 1) * 18))
                Item.NewItem(i * 16, j * 16, 0, 0, drop, (thisTile.frameX >= LastFrame ? Main.rand.Next(2, 4) : 1));
            return false;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].frameY == 36)
            {
                Texture2D tex;
                if (Main.canDrawColorTile(i, j))
                    tex = Main.tileAltTexture[Type, Main.tile[i, j].color()];
                else
                    tex = Main.tileTexture[Type];

                SpriteEffects effect = i % 2 == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                float scale = Math.Min(0.01f + (Math.Abs(Main.windSpeed) * 0.07f), 0.12f);
                spriteBatch.Draw(tex, 
                    (new Vector2(i + 11.5f, j + 10) * 16) + new Vector2(tex.Width / 2, FrameHeight + 3) - Main.screenPosition, 
                    new Rectangle(0, Main.tile[i, j].frameX + 1, 32, 48), Lighting.GetColor(i, j), 
                    ((float)Math.Sin(Main.GameUpdateCount / 30f + (i * 0.8333f) + (j * 2.3333f)) + (float)Math.Sin(Main.GameUpdateCount / 23.3333)) * scale, //0.07f, 
                    new Vector2(16, 48), 1f, effect, default);
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            int tile = Main.tile[i, j].frameY / 18;
            int off = Math.Abs(tile - (Height - 1));

            if (Main.tile[i, j + off].frameX >= LastFrame)
            {
                Main.LocalPlayer.noThrow = 2;
                Main.LocalPlayer.showItemIcon = true;
                Main.LocalPlayer.showItemIcon2 = drop;
            }
        }

        //public int LastFrame() => 
        //    (FrameCount - 1) * 16 * Height;

        //public int FrameHeight() =>

        public override void RandomUpdate(int i, int j)
        {
            if (Main.tile[i, j].frameY == 0)
            {
                int bottomY = j + (Height - 1);
                Tile bottomTile = Main.tile[i, bottomY];
                if (bottomTile.type == Type && bottomTile.frameY == ((Height - 1) * 18) && bottomTile.frameX < LastFrame)
                {
                    if (Main.rand.Next(1, 101) <= GrowthPercentChance(i, bottomY))
                    {
                        bottomTile.frameX += (short)(16 * Height);
                        NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                    }
                }
            }
        }

        public virtual int GrowthPercentChance(int i, int j)
        {
            return 100 / Height;
        }

        public override bool NewRightClick(int i, int j)
        {
            int tile = Main.tile[i, j].frameY / 18;
            int off = Math.Abs(tile - 2);

            if (Main.tile[i, j + off].frameX >= LastFrame)
            {
                Main.tile[i, j + off].frameX -= (short)FrameHeight;
                Item.NewItem(new Vector2(i, j) * 16, drop, Main.rand.Next(1, 3));
                return true;
            }
            return base.NewRightClick(i, j);
        }
    }

    public class TallRice : Crop
    {
        public TallRice() : base("Rice", "Rice", AssetDirectory.HerbologyCropTile, 4) { }

        public override int GrowthPercentChance(int i, int j)
        {
            Tile thisTile = Main.tile[i, j];
            Tile aboveTile = Main.tile[i, j - 1];
            //Main.NewText(aboveTile.liquid <= 128 ? (((thisTile.liquid - (aboveTile.liquid * 2)) / 255f) * 80) + 10 : 0);
            if (aboveTile.liquid < 127)//the growth chance gets higher as the water level does, but it quickly drops off if the second block has any water
                return (int)((((thisTile.liquid - (aboveTile.liquid * 2)) / 255f) * 60) + 8); //68 max //last number is (min chance), second to last number is (max chance) minus (min chance)

            return 0;
        }
    }
    public class Rice : QuickTileItem
    {
        public Rice() : base("Rice", " ", "TallRice", ItemRarityID.White, AssetDirectory.HerbologyCropTile) { }
    }


    public class TallGoldenRice : Crop
    {
        public TallGoldenRice() : base("GoldenRice", "Golden Rice", AssetDirectory.HerbologyCropTile, 4) { }
    }
    public class GoldenRice : QuickTileItem
    {
        public GoldenRice() : base("Golden Rice", " ", "TallGoldenRice", ItemRarityID.Orange, AssetDirectory.HerbologyCropTile) { }
    }


    public class TallAncientFruit : Crop
    {
        public TallAncientFruit() : base("AncientFruit", "Ancient Fruit", AssetDirectory.HerbologyCropTile, 6) { }
    }
    public class AncientFruit : QuickTileItem
    {
        public AncientFruit() : base("Ancient Fruit", " ", "TallAncientFruit", ItemRarityID.Orange, AssetDirectory.HerbologyCropTile) { }
    }
}