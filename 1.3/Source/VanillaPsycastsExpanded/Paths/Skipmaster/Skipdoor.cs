namespace VanillaPsycastsExpanded.Skipmaster;

using System.Collections.Generic;
using System.IO;
using Graphics;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

[StaticConstructorOnStartup]
public class Skipdoor : ThingWithComps, IMinHeatGiver
{
    private const           string    SKIPGATE_TEXPATH = "Textures/Effects/Skipmaster/Skipgate";
    private const           float     MASK_THRESHOLD   = 0.13f;
    private static readonly Texture2D DestroyIcon      = ContentFinder<Texture2D>.Get("Effects/Skipmaster/Skipgate/SkipdoorDestroy");
    private static readonly Texture2D RenameIcon       = ContentFinder<Texture2D>.Get("Effects/Skipmaster/Skipgate/SkipdoorRename");
    private static readonly Material  MainMat          = MaterialPool.MatFrom("Effects/Skipmaster/Skipgate/Skipgate", ShaderDatabase.TransparentPostLight);
    private static          Material  maskMat;
    private static          Texture2D backgroundTex;


    private static readonly Material DistortionMat =
        DistortedMaterialsPool.DistortedMaterial("Things/Mote/Black", "Things/Mote/PsycastDistortionMask", 0.02f, 1.1f);

    private Material      backgroundMat;
    private RenderTexture background1;
    private RenderTexture background2;
    public  Pawn          Pawn;
    private float         rotation;
    private float         distortAmount = 1.5f;
    private Vector2       backgroundOffset;
    private Sustainer     sustainer;

    public string Name { get; set; }

    public bool IsActive => this.Spawned;
    public int  MinHeat  => 50;

    private static Pair<Texture2D, Texture2D> GetBackgroundTextures(ModContentPack content)
    {
        Texture2D bg = null, mask = null;
        foreach ((string path, FileInfo file) in ModContentPack.GetAllFilesForMod(
                     content, SKIPGATE_TEXPATH, ModContentLoader<Texture2D>.IsAcceptableExtension))
        {
            string key  = path.Replace("\\", "/");
            string name = key.Replace(SKIPGATE_TEXPATH + "/", "").Replace(Path.GetExtension(key), "");
            switch (name)
            {
                case "SkipgateBackground":
                    bg = LoadTextureReadable(file.FullName);
                    break;
                case "SkipgateMask":
                    mask = LoadTextureReadable(file.FullName);
                    break;
            }
        }

        return new Pair<Texture2D, Texture2D>(bg, mask);
    }

    private static Texture2D LoadTextureReadable(string file)
    {
        Texture2D texture2D = null;
        if (File.Exists(file))
        {
            byte[] data = File.ReadAllBytes(file);
            texture2D = new Texture2D(2, 2, TextureFormat.Alpha8, true);
            texture2D.LoadImage(data);
            texture2D.name       = Path.GetFileNameWithoutExtension(file);
            texture2D.filterMode = FilterMode.Trilinear;
            texture2D.anisoLevel = 2;
            texture2D.Apply(true, false);
        }

        return texture2D;
    }

    private static void CacheBackground(Texture2D bg, Texture2D mask)
    {
        backgroundTex = bg;
        Texture2D invertedMask = new(bg.width, bg.height);

        for (int x = 0; x < bg.width; x++)
        for (int y = 0; y < bg.height; y++)
        {
            Color maskColor = mask.GetPixel(x, y);
            Color result = maskColor.IndistinguishableFromFast(Color.black) || maskColor.r <= MASK_THRESHOLD
                ? Color.red
                : Color.black;
            invertedMask.SetPixel(x, y, result);
        }

        invertedMask.Apply();
        maskMat = new Material(ShaderDatabase.CutoutComplex) { name = "Skipdoor_Static_BackgroundMask", color = Color.clear };
        maskMat.SetTexture(ShaderPropertyIDs.MaskTex, invertedMask);
        maskMat.SetColor(ShaderPropertyIDs.ColorTwo, Color.clear);
    }

    public static void Init(ModContentPack content)
    {
        Pair<Texture2D, Texture2D> pair = GetBackgroundTextures(content);
        CacheBackground(pair.First, pair.Second);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            this.background1   = new RenderTexture(backgroundTex.width, backgroundTex.height, 0);
            this.background2   = new RenderTexture(backgroundTex.width, backgroundTex.height, 0);
            this.backgroundMat = new Material(ShaderDatabase.TransparentPostLight);
        });
        this.RecacheBackground();
        this.Pawn.Psycasts().AddMinHeatGiver(this);
        if (respawningAfterLoad) return;
        WorldComponent_SkipdoorManager.Instance.Skipdoors.Add(this);
        this.Pawn.psychicEntropy.TryAddEntropy(50f, this, true, true);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        this.sustainer?.End();
        this.sustainer = null;
        WorldComponent_SkipdoorManager.Instance.Skipdoors.Remove(this);
        base.DeSpawn(mode);
        Object.Destroy(this.background1);
        Object.Destroy(this.background2);
        Object.Destroy(this.backgroundMat);
    }

    private void RecacheBackground()
    {
        if (this.backgroundMat == null) return;
        Graphics.Blit(backgroundTex,    this.background1, Vector2.one, this.backgroundOffset, 0, 0);
        Graphics.Blit(this.background1, this.background2, maskMat);
        this.backgroundMat.mainTexture = this.background2;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos()) yield return gizmo;
        yield return new Command_Action
        {
            defaultLabel = "VPE.DestroySkipdoor".Translate(),
            defaultDesc  = "VPE.DestroySkipdoor.Desc".Translate(this.Pawn.NameFullColored),
            icon         = DestroyIcon,
            action       = () => this.Destroy()
        };
        yield return new Command_Action
        {
            defaultLabel = "VPE.RenameSkipdoor".Translate(),
            defaultDesc  = "VPE.RenameSkipdoor.Desc".Translate(),
            icon         = RenameIcon,
            action       = () => Find.WindowStack.Add(new Dialog_RenameSkipdoor(this))
        };
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref this.Pawn, "pawn");
        string name = this.Name;
        Scribe_Values.Look(ref name, nameof(name));
        this.Name = name;
    }

    public override void Tick()
    {
        base.Tick();
        this.rotation      =  (this.rotation + 0.5f) % 360f;
        this.distortAmount += 0.01f;
        if (this.distortAmount >= 3f) this.distortAmount = 1.5f;
        this.backgroundOffset += Vector2.one * 0.001f;
        this.RecacheBackground();
        if (PsycastsMod.Settings.muteSkipdoor)
        {
            this.sustainer?.End();
            this.sustainer = null;
        }
        else
        {
            this.sustainer ??= VPE_DefOf.VPE_Skipdoor_Sustainer.TrySpawnSustainer(this);
            this.sustainer.Maintain();
        }

        if (this.IsHashIntervalTick(30) && this.HitPoints < this.MaxHitPoints) this.HitPoints += 1;
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(this.rotation, Vector3.up), Vector3.one * 1.5f), MainMat, 0);
        if (this.backgroundMat != null)
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawLoc - Altitudes.AltIncVect / 2, Quaternion.identity, Vector3.one * 1.5f),
                              this.backgroundMat, 0);
        Graphics.DrawMesh(MeshPool.plane10,
                          Matrix4x4.TRS(drawLoc.Yto0() + Vector3.up * AltitudeLayer.MoteOverhead.AltitudeFor(), Quaternion.identity,
                                        Vector3.one * this.distortAmount * 2f),
                          DistortionMat, 0);
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    {
        foreach (FloatMenuOption option in base.GetFloatMenuOptions(selPawn)) yield return option;

        foreach (Skipdoor skipdoor in WorldComponent_SkipdoorManager.Instance.Skipdoors.Except(this))
            yield return new FloatMenuOption("VPE.TeleportTo".Translate(skipdoor.Name), () =>
            {
                Job job = JobMaker.MakeJob(VPE_DefOf.VPE_UseSkipdoor, this);
                job.globalTarget = skipdoor;
                selPawn.jobs.StartJob(job, JobCondition.InterruptForced, canReturnCurJobToPool: true);
            });
    }
}