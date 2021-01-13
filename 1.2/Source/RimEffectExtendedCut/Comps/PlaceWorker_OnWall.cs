﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimEffectExtendedCut
{
    public class ColorModExtension : DefModExtension
    {
        public List<ColorOption> colorOptions = new List<ColorOption>();
    }
    public class ColorOption
    {
		public float overlightRadius;

		public float glowRadius = 14f;

		public ColorInt glowColor = new ColorInt(255, 255, 255, 0) * 1.45f;

		public string colorLabel = "";
    }
	public class CompProperties_GlowerExtended : CompProperties
    {
        public List<ColorOption> colorOptions;
		public CompProperties_GlowerExtended()
		{
			compClass = typeof(CompGlowerExtended);
		}
	}

	public class CompGlowerExtended : ThingComp
	{
		private ColorOption currentColor;
		private int currentColorInd;
        private CompGlower compGlower;
        public CompProperties_GlowerExtended Props => (CompProperties_GlowerExtended)props;
        public override string TransformLabel(string label)
        {
            return base.TransformLabel(label) + " (" + currentColor.colorLabel + ")";
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			this.currentColor = Props.colorOptions[currentColorInd];
			this.UpdateGlower(currentColorInd);
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (base.parent.Faction == Faction.OfPlayer)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = delegate
                {
                    if (this.currentColorInd == Props.colorOptions.Count - 1)
                    {
                        this.UpdateGlower(0);
                    }
                    else
                    {
                        this.UpdateGlower(this.currentColorInd + 1);
                    }
                };
                command_Action.defaultLabel = "RE.SwitchLightColor".Translate();
                command_Action.defaultDesc = "RE.SwitchLightColorDesc".Translate();
                command_Action.hotKey = KeyBindingDefOf.Misc8;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Icons/LampColourSwitch");
                yield return command_Action;
            }
        }

        public void UpdateGlower(int colorOptionInd)
        {
            if (this.compGlower != null)
            {
                base.parent.Map.glowGrid.DeRegisterGlower(this.compGlower);
            }
            var colorOption = Props.colorOptions[colorOptionInd];
            this.compGlower = new CompGlower();
            this.compGlower.parent = this.parent;
            this.compGlower.Initialize(new CompProperties_Glower()
            {
                glowColor = colorOption.glowColor,
                glowRadius = colorOption.glowRadius,
                overlightRadius = colorOption.overlightRadius
            });
            this.currentColor = colorOption;
            this.currentColorInd = colorOptionInd;
            base.parent.Map.mapDrawer.MapMeshDirty(base.parent.Position, MapMeshFlag.Things);
            base.parent.Map.glowGrid.RegisterGlower(this.compGlower);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Values.Look(ref currentColorInd, "currentColorInd");
        }
    }
}
