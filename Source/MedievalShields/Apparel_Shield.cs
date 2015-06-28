using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
namespace MedievalShields
{
    public class Apparel_MedievalShield : Apparel
    {
        private Color color;
        private Material shieldMat;
        private static readonly SoundDef SoundAbsorbDamage = SoundDef.Named("PersonalShieldAbsorbDamage");
        private static readonly SoundDef SoundBreak = SoundDef.Named("PersonalShieldBroken");
        private Vector3 impactAngleVect;
        public override void SpawnSetup()
        {
 	        base.SpawnSetup();
            color = this.Stuff.stuffProps.color;
            shieldMat = MaterialPool.MatFrom("Things/Item/Equipment/Apparel/Accessory/Shield", ShaderDatabase.Cutout, color);
        }
        private bool ShouldDisplay
        {
            get
            {
                return !this.wearer.Dead && !this.wearer.Downed && (!this.wearer.IsPrisonerOfColony || (this.wearer.BrokenStateDef != null && this.wearer.BrokenStateDef == BrokenStateDefOf.Berserk)) && ((this.wearer.playerController != null && this.wearer.playerController.Drafted) || this.wearer.Faction.HostileTo(Faction.OfColony));
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
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
                    if (chance1 == 1)
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
                    if (chance2 >= 3)
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
        private void AbsorbedDamage(DamageInfo dinfo)
        {
            Apparel_MedievalShield.SoundAbsorbDamage.PlayOneShot(this.wearer.Position);
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = this.wearer.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            MoteThrower.ThrowStatic(loc, ThingDefOf.Mote_ShotHit_Spark, 1f);
        }
        private void Break()
        {
            Apparel_MedievalShield.SoundBreak.PlayOneShot(this.wearer.Position);
            this.Destroy();
        }
        public override bool AllowVerbCast(IntVec3 root, TargetInfo targ)
        {
            return true;
        }
        public override void DrawWornExtras()
        {
            float num = 0f;
            Vector3 vector = this.wearer.drawer.DrawPos;
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
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(vector, Quaternion.AngleAxis(num, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, shieldMat, 0);
        }
    }
}