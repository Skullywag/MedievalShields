using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
namespace MedievalShields
{
    public class Apparel_MedievalShield : Apparel
    {
        public Material shieldMat;
        public static readonly SoundDef SoundAbsorbDamage = SoundDef.Named("PersonalShieldAbsorbDamage");
        public static readonly SoundDef SoundBreak = SoundDef.Named("PersonalShieldBroken");
        public Vector3 impactAngleVect;
        public ThingDef equippedDef;
        public Material kiteMat = MaterialPool.MatFrom("Things/Item/Equipment/Apparel/Accessory/KiteShield");
        public Material bucklerMat = MaterialPool.MatFrom("Things/Item/Equipment/Apparel/Accessory/BucklerSingle/buckler1");

    public bool ShouldDisplay
        {
            get
            {
                return !this.wearer.Dead && !this.wearer.InBed() && !this.wearer.Downed && (!this.wearer.IsPrisonerOfColony || (this.wearer.MentalStateDef != null && this.wearer.MentalStateDef == MentalStateDefOf.Berserk)) || this.wearer.Faction.HostileTo(Faction.OfColony);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.LookValue<Color>(ref this.color, "color", new Color(0.8f, 0.8f, 0.8f), false);
        }
        public override void Tick()
        {
            base.Tick();
        }
        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (dinfo.Instigator != null)
            {
                SkillRecord melee = this.wearer.skills.GetSkill(SkillDefOf.Melee);
                System.Random random = new System.Random();
                int chance = random.Next(0, 21);
                if (chance >= melee.level)
                {
                    System.Random random1 = new System.Random();
                    int chance1 = random1.Next(0, 5);
                    if (chance1 >= 3)
                    {
                        this.HitPoints -= dinfo.Amount / 2;
                        if (this.HitPoints <= 0)
                        {
                            this.Break();
                        }
                        else
                        {
                            this.AbsorbedDamage(dinfo);
                        }
                        return true;
                    }
                }
                else
                {
                    System.Random random2 = new System.Random();
                    int chance2 = random2.Next(0, 5);
                    if (chance2 >= 2)
                    {
                        this.HitPoints -= dinfo.Amount / 4;
                        if (this.HitPoints <= 0)
                        {
                            this.Break();
                        }
                        else
                        {
                            this.AbsorbedDamage(dinfo);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public void AbsorbedDamage(DamageInfo dinfo)
        {
            Apparel_MedievalShield.SoundAbsorbDamage.PlayOneShot(this.wearer.Position);
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = this.wearer.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            MoteThrower.ThrowStatic(loc, ThingDefOf.Mote_ShotHit_Spark, 1f);
        }
        public void Break()
        {
            Apparel_MedievalShield.SoundBreak.PlayOneShot(this.wearer.Position);
            this.Destroy();
        }
        public override bool AllowVerbCast(IntVec3 root, TargetInfo targ)
        {
            if (this.wearer.equipment.Primary != null)
            {
                if (this.wearer.equipment.Primary.def.IsMeleeWeapon)
                {
                    return true;
                }
                if (this.wearer.equipment.Primary.def.defName.Equals("Gun_Pistol"))
                {
                    return true;
                }
                if (this.wearer.equipment.Primary.def.defName.Equals("Gun_PDW"))
                {
                    return true;
                }
                if (this.wearer.equipment.Primary.def.defName.Equals("Gun_HeavySMG"))
                {
                    return true;
                }
                if (this.wearer.equipment.Primary.def.weaponTags.Contains("MedievalShields_ValidSidearm"))
                {
                    return true;
                }
            }
            return root.AdjacentTo8Way(targ.Cell);
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            yield return new Apparel_MedievalShield.Gizmo_ShieldStatus
            {
                shield = this
            };
            yield break;
        }

        internal class Gizmo_ShieldStatus : Gizmo
        {
            public Apparel_MedievalShield shield;
            private static readonly Texture2D FullTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
            private static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
            public override float Width
            {
                get
                {
                    return 140f;
                }
            }
            public override GizmoResult GizmoOnGUI(Vector2 topLeft)
            {
                Rect rect = new Rect(topLeft.x, topLeft.y, this.Width, 75f);
                Widgets.DrawWindowBackground(rect);
                Rect rect2 = GenUI.ContractedBy(rect, 6f);
                Rect rect3 = rect2;
                rect3.height = rect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect3, this.shield.LabelCap);
                Rect rect4 = rect2;
                rect4.yMin = rect.y + rect.height / 2f;
                float num = this.shield.MaxHitPoints / this.shield.HitPoints;
                Widgets.FillableBar(rect4, num, Apparel_MedievalShield.Gizmo_ShieldStatus.FullTex, Apparel_MedievalShield.Gizmo_ShieldStatus.EmptyTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect4, (this.shield.MaxHitPoints).ToString("F0") + " / " + (this.shield.HitPoints).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
                return new GizmoResult(0);
            }
        }

        public override void DrawWornExtras()
        {
            if (this.ShouldDisplay)
            {
                float num = 0f;
                Vector3 vector = this.wearer.Drawer.DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
                Vector3 s = new Vector3(1f, 1f, 1f);
                if (this.wearer.Rotation == Rot4.North)
                {
                    vector.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
                    vector.x -= 0.2f;
                    vector.z -= 0.2f;
                }
                else
                {
                    if (this.wearer.Rotation == Rot4.South)
                    {
                        vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                        vector.x += 0.2f;
                        vector.z -= 0.2f;
                    }
                    else
                    {
                        if (this.wearer.Rotation == Rot4.East)
                        {
                            vector.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
                            vector.z -= 0.2f;
                            num = 90f;
                        }
                        else
                        {
                            if (this.wearer.Rotation == Rot4.West)
                            {
                                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                                vector.z -= 0.2f;
                                num = 270f;
                            }
                        }
                    }
                }
                if (this.def.defName == "KiteShield")
                {
                    kiteMat.shader = ShaderDatabase.Cutout;
                    kiteMat.color = Stuff.stuffProps.color;
                    Matrix4x4 matrix = default(Matrix4x4);
                    matrix.SetTRS(vector, Quaternion.AngleAxis(num, Vector3.up), s);
                    Graphics.DrawMesh(MeshPool.plane10, matrix, kiteMat, 0);
                }
                else
                {
                    bucklerMat.shader = ShaderDatabase.Cutout;
                    bucklerMat.color = Stuff.stuffProps.color;
                    Matrix4x4 matrix = default(Matrix4x4);
                    matrix.SetTRS(vector, Quaternion.AngleAxis(num, Vector3.up), s);
                    Graphics.DrawMesh(MeshPool.plane10, matrix, bucklerMat, 0);
                }
            }
        }
    }
}