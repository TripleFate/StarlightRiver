﻿using Terraria;
using Terraria.Graphics;

namespace StarlightRiver.Content.CustomHooks
{
	class MatrixUpdateCancel : HookGroup
    {
        //Really risky matrix reset cancellation which may or may not kill zoom
        public override SafetyLevel Safety => SafetyLevel.Fragile;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On.Terraria.Graphics.SpriteViewMatrix.ShouldRebuild += UpdateMatrixFirst;
        }

        private bool UpdateMatrixFirst(On.Terraria.Graphics.SpriteViewMatrix.orig_ShouldRebuild orig, SpriteViewMatrix self) => orig(self);
    }
}