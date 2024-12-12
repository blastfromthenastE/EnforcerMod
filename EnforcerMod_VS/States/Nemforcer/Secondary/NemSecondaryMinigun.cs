using RoR2;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using RoR2.Projectile;
using System;
using Modules;
using EnforcerPlugin;

namespace EntityStates.Nemforcer {
    public class HammerSlam : BaseSkillState
    {
        public static string hitboxString = "SwingCenter"; //transform where the hitbox is fired
        public static float baseDuration = 1.2f;
        public static float damageCoefficient = 6f;
        public static float procCoefficient = 1f;
        public static float knockbackForce = 0.14f;
        public static float blastRadius = 12f;
        public static float beefDuration = 0.8f;
        public static float recoilAmplitude = 15f;
        public static GameObject slamPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ParentSlamEffect");

        public static float projectileBlastRadius = 7f;
        public static float blastDamageCoefficient = 1f;
        public static float blastProcCoefficient = 1f;
        public GameObject projectileBlastPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFX");

        private float attackStopDuration;
        private float duration;
        private float fireDuration;
        private Ray aimRay;
        private BlastAttack blastAttack;
        private ChildLocator childLocator;
        private bool hasFired;
        private bool hasSwung;

        private BlastAttack projectileBlastAttack;
        private EffectData blastEffectData;

        public static event Action<Run> Bonked = delegate { };

        private List<CharacterBody> victimList = new List<CharacterBody>();

        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = baseDuration / this.attackSpeedStat;
            this.fireDuration = this.duration * 0.456f;
            this.aimRay = base.GetAimRay();
            this.hasFired = false;
            this.hasSwung = false;
            this.childLocator = base.GetModelTransform().GetComponent<ChildLocator>();
            base.StartAimMode(aimRay, 2f, false);
            this.attackStopDuration = HammerSlam.beefDuration / this.attackSpeedStat;

            base.PlayAnimation("FullBody, Override", "HammerSlam", "HammerSlam.playbackRate", this.duration);

            projectileBlastAttack = new BlastAttack {
                radius = HammerSlam.projectileBlastRadius,
                procCoefficient = HammerSlam.blastProcCoefficient,
                position = Vector3.zero,
                attacker = base.gameObject,
                crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master),
                baseDamage = base.characterBody.damage * HammerSlam.blastDamageCoefficient,
                falloffModel = BlastAttack.FalloffModel.None,
                baseForce = 1f,
                damageType = DamageType.Generic,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
            };
            projectileBlastAttack.teamIndex = TeamComponent.GetObjectTeam(projectileBlastAttack.attacker);

            blastEffectData = new EffectData();
            blastEffectData.scale = 6f;
            blastEffectData.color = new Color32(234, 200, 127, 100);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge < this.attackStopDuration)
            {
                if (base.characterMotor)
                {
                    base.characterMotor.moveDirection = Vector3.zero;
                }
            }

            if (base.fixedAge >= 0.35f * this.duration)
            {
                this.SwingEffect();
            }

            if (base.fixedAge >= this.fireDuration)
            {
                this.FireBlast();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void FireBlast()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                this.DestroyProjectiles();
                this.Bonk();

                var soundGO = EnforcerPlugin.VRAPICompat.IsLocalVRPlayer(characterBody) ? EnforcerPlugin.VRAPICompat.GetPrimaryMuzzleObject() : base.gameObject;

                AkSoundEngine.SetRTPCValue("M2_Charge", 100f);
                Util.PlaySound(Sounds.NemesisSmash, soundGO);
                //Util.PlaySound("Play_parent_attack1_slam", base.gameObject);

                if (base.isAuthority)
                {
                    Vector3 sex = this.childLocator.FindChild("SwingCenter").transform.position;

                    EffectData effectData = new EffectData();
                    effectData.origin = sex + Vector3.up * 0.5f;
                    effectData.scale = 1;

                    EffectManager.SpawnEffect(slamPrefab, effectData, true); 

                    base.AddRecoil(-0.5f * HammerSlam.recoilAmplitude * 3f, -0.5f * HammerSlam.recoilAmplitude * 3f, -0.5f * HammerSlam.recoilAmplitude * 5f, 0.5f * HammerSlam.recoilAmplitude * 3f);

                    Vector3 center = childLocator.FindChild(hitboxString).position;

                    blastAttack = new BlastAttack();
                    blastAttack.radius = HammerSlam.blastRadius;
                    blastAttack.procCoefficient = HammerSlam.procCoefficient;
                    blastAttack.position = center;
                    blastAttack.attacker = base.gameObject;
                    blastAttack.inflictor = base.gameObject;
                    blastAttack.crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);
                    blastAttack.baseDamage = base.characterBody.damage * HammerSlam.damageCoefficient;
                    blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                    blastAttack.baseForce = 0f;
                    blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                    blastAttack.damageType = DamageType.Stun1s;
                    blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                    blastAttack.impactEffect = BeetleGuardMonster.GroundSlam.hitEffectPrefab.GetComponent<EffectComponent>().effectIndex;
                    var result = blastAttack.Fire();
                    if (result.hitCount > 0 && NemforcerPlugin.reworkPassive.Value)  base.characterBody.AddTimedBuffAuthority(Modules.Buffs.nemforcerRegenBuff.buffIndex, NemforcerPlugin.nemforcerRegenBuffDuration);
                }

                if (NetworkServer.active)
                {
                    Vector3 pushForce = ((aimRay.origin + 200 * aimRay.direction) - childLocator.FindChild(hitboxString).position + (75 * Vector3.up)) * HammerSlam.knockbackForce;

                    Collider[] array = Physics.OverlapSphere(childLocator.FindChild(hitboxString).position, HammerSlam.blastRadius, LayerIndex.defaultLayer.mask);
                    for (int i = 0; i < array.Length; i++)
                    {
                        HealthComponent healthComponent = array[i].GetComponent<HealthComponent>();
                        if (healthComponent)
                        {
                            TeamComponent component2 = healthComponent.GetComponent<TeamComponent>();
                            if (component2.teamIndex != base.teamComponent.teamIndex)
                            {
                                Util.PlaySound(Sounds.NemesisImpact, healthComponent.gameObject);

                                var charb = healthComponent.body;
                                if (charb)
                                {
                                    var motor = charb.GetComponent<CharacterMotor>();
                                    var rb = charb.GetComponent<Rigidbody>();
                                    Vector3 force = pushForce;

                                    float mass = 0f;

                                    if (motor) mass = motor.mass;
                                    else if (rb) mass = rb.mass;

                                    if (mass <= 100f) mass = 100f;
                                    if (Config.uncappedShieldBash.Value && mass > 500f) mass = 500f;

                                    force *= mass;

                                    DamageInfo info = new DamageInfo
                                    {
                                        attacker = base.gameObject,
                                        inflictor = base.gameObject,
                                        damage = 0,
                                        damageColorIndex = DamageColorIndex.Default,
                                        damageType = DamageType.Generic,
                                        crit = false,
                                        dotIndex = DotController.DotIndex.None,
                                        force = force,
                                        position = base.transform.position,
                                        procChainMask = default(ProcChainMask),
                                        procCoefficient = 0
                                    };

                                    charb.healthComponent.TakeDamageForce(info, true, true);
                                }

                            }
                        }
                    }
                }
            }
        }

        private void Bonk()
        {
            if (base.teamComponent.teamIndex == TeamIndex.Player)
            {
                Collider[] array = Physics.OverlapSphere(childLocator.FindChild(hitboxString).position, HammerSlam.blastRadius, LayerIndex.defaultLayer.mask);
                for (int i = 0; i < array.Length; i++)
                {
                    HealthComponent healthComponent = array[i].GetComponent<HealthComponent>();
                    if (healthComponent)
                    {
                        TeamComponent component2 = healthComponent.GetComponent<TeamComponent>();
                        if (component2.teamIndex == base.teamComponent.teamIndex)
                        {
                            var charb = healthComponent.body;
                            if (charb && charb.modelLocator && charb != base.characterBody)
                            {
                                if (!charb.modelLocator.modelTransform.gameObject.GetComponent<EnforcerPlugin.SquashedComponent>())
                                {
                                    charb.modelLocator.modelTransform.gameObject.AddComponent<EnforcerPlugin.SquashedComponent>().speed = 5f;
                                    Util.PlaySound(Sounds.Bonk, charb.gameObject);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DestroyProjectiles()
        {
            Collider[] array = Physics.OverlapSphere(this.childLocator.FindChild(hitboxString).position, 1.5f * HammerSlam.blastRadius, LayerIndex.projectile.mask);

            int projectileCount = 0;

            for (int i = 0; i < array.Length; i++)
            {
                ProjectileController pc = array[i].GetComponentInParent<ProjectileController>();
                if (pc)
                {
                    if (pc.owner != base.gameObject)
                    {
                        projectileCount++;
                        Destroy(pc.gameObject);

                        projectileBlastAttack.position = pc.transform.position;
                        projectileBlastAttack.Fire();

                        blastEffectData.origin = pc.transform.position;
                        blastEffectData.scale = HammerSlam.projectileBlastRadius * 1.15f;
                        EffectManager.SpawnEffect(projectileBlastPrefab, blastEffectData, true);
                    }
                }

                if (projectileCount >= 5)
                {
                    Bonked?.Invoke(Run.instance);
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void SwingEffect()
        {
            if (!this.hasSwung)
            {
                this.hasSwung = true;
                Util.PlayAttackSpeedSound(Sounds.NemesisSwingL, base.gameObject, this.attackSpeedStat);

                if (base.isAuthority)
                {
                    EffectManager.SimpleMuzzleFlash(Asset.nemSlamSwingFX, base.gameObject, "SwingUppercut", true);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}