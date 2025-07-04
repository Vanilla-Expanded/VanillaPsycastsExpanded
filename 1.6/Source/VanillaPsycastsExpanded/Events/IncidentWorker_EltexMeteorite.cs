﻿namespace VanillaPsycastsExpanded
{
    using System.Collections.Generic;
    using RimWorld;
    using UnityEngine;
    using Verse;
    public class IncidentWorker_EltexMeteorite : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            IntVec3 cell;
            return TryFindCell(out cell, map);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!TryFindCell(out var cell, map))
            {
                return false;
            }
            List<Thing> list = new List<Thing>();
            for (var i = 0; i < 5; i++)
            {
                Building building = (Building)ThingMaker.MakeThing(VPE_DefOf.VPE_EltexOre);
                building.canChangeTerrainOnDestroyed = false;
                list.Add(building);
            }
            SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, cell, map);
            LetterDef baseLetterDef = (list[0].def.building.isResourceRock ? LetterDefOf.PositiveEvent : LetterDefOf.NeutralEvent);
            string text = string.Format(def.letterText, list[0].def.label).CapitalizeFirst();
            SendStandardLetter(def.letterLabel, text, baseLetterDef, parms, new TargetInfo(cell, map));
            return true;
        }

        private bool TryFindCell(out IntVec3 cell, Map map)
        {
            int maxMineables = 5;
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map,TerrainAffordanceDefOf.Walkable, out cell, 10, default(IntVec3), -1, allowRoofedCells: true, allowCellsWithItems: false, allowCellsWithBuildings: false, colonyReachable: false, avoidColonistsIfExplosive: true, alwaysAvoidColonists: true, delegate (IntVec3 x)
            {
                int num = Mathf.CeilToInt(Mathf.Sqrt(maxMineables)) + 2;
                CellRect cellRect = CellRect.CenteredOn(x, num, num);
                int num2 = 0;
                foreach (IntVec3 item in cellRect)
                {
                    if (item.InBounds(map) && item.Standable(map))
                    {
                        num2++;
                    }
                }
                return num2 >= maxMineables;
            });
        }
    }
}