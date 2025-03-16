using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;
using UnityEngine;

namespace BiggerTooltips
{
	[StaticConstructorOnStartup]
	public class Main
	{
		static Main()
		{
			// Use unique harmony ID based on mod name (or location), so that assembly can be shipped in 2 separate mods being active together
			if (BiggerTooltips.harmonyID == "")
			{
				Assembly assembly = typeof(Main).Assembly;
				BiggerTooltips.harmonyID = assembly.Location;
			}
			Harmony pat = new Harmony(BiggerTooltips.harmonyID);
			pat.PatchAll();
		}
	}
	public class BiggerTooltips : Mod
	{
		public static string harmonyID = "";
		public BiggerTooltips(ModContentPack content) : base(content)
		{
			harmonyID = content.Name;
		}
	}
	[HarmonyPatch(typeof(ActiveTip), "get_TipRect")]
	public static class LargeTip
	{
		public static void Postfix(ActiveTip __instance, ref Rect __result)
		{
			string text = (string)typeof(ActiveTip).GetProperty("FinalText", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
			// Optimization for many many normal UI tooltips being short
			const int expectedNonFittingTextLength = 240;
			if (text.Length < expectedNonFittingTextLength)
			{
				return;
			}
			// In Vanilla, rect is expanded by 4 at the end of calculation
			const float doubleSpacing = 8f;
			const int vanillaWidthLimit = 260;
			Vector2 newMaxTextSize = Text.CalcSize(text);
			float newWidth = 0f; // 0 means leave unchanged
			int estimateNumberOfLines = (int)newMaxTextSize.x / vanillaWidthLimit;
			// Number thresold picked manually by viewing actual tooltips. After using bigger tooltips for a while,
			// Vanilla limit looks really small, so the threshold to increase size to 1.5x should be low.
			// Threshold for huge tooltip can be larger, however.
			if (estimateNumberOfLines > 12)
			{
				newWidth = (__result.width - doubleSpacing) * 2.25f;
			}
			else if (estimateNumberOfLines > 5)
			{
				newWidth = (__result.width - doubleSpacing) * 1.5f;
			}
			if (newWidth != 0f)
			{
				__result.width = newWidth + doubleSpacing;
				__result.height = Text.CalcHeight(text, newWidth) + doubleSpacing;
				__result = __result.RoundedCeil();
			}
		}
	}
}

