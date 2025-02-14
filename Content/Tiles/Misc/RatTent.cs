﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Misc
{
    public class RatTent : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.MiscTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() 
        {
            TileObjectData.newTile.DrawYOffset = 2;
            this.QuickSetFurniture(5, 4, 26, SoundID.Dig, false, new Color(163, 161, 96), false, false, "Strange Tent"); 
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = 1;

        public override bool NewRightClick(int i, int j)
        {
            Vector2 vel = (-Vector2.UnitY * 8).RotatedBy(Main.rand.NextFloat(-1f, 1f));
            Main.npc[NPC.NewNPC(i * 16, j * 16, Main.rand.Next(350) == 0 ? NPCID.GoldMouse : NPCID.Mouse)].velocity = vel;

            Helper.PlayPitched(SoundID.NPCDeath4, 0.3f, Main.rand.NextFloat(-0.1f, 0.1f));
            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<RatTentItem>());
    }

    public class RatTentItem : QuickTileItem
    {
        public RatTentItem() : base("Strange Tent", "Whats inside?...", TileType<RatTent>(), 1, AssetDirectory.MiscTile) { }
    }
}