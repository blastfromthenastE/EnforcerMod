﻿using System;
using System.Collections.Generic;
using BepInEx;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using EntityStates.Nemforcer;
using EntityStates.Enforcer;
using RoR2.Projectile;
using RoR2.CharacterAI;
using RoR2.Navigation;
using EnforcerPlugin.Modules;

namespace EnforcerPlugin {
    public class NemforcerPlugin
    {
        public const string characterName = "Nemesis Enforcer";
        public const string characterSubtitle = "Incorruptible Shadow";
        public const string bossSubtitle = "End of the Line";
        public const string characterOutro = "..and so he left, with newfound might to honor.";
        public const string characterOutroFailure = "..and so he returned, infallible bastion truly immortalized.";
        public const string characterLore = "\nheavy tf2\n\n";

        public static GameObject characterPrefab;
        public static GameObject characterDisplay;
        public static GameObject doppelganger;
        public static GameObject bossPrefab;
        public static GameObject bossMaster;
        public static GameObject minibossPrefab;
        public static GameObject minibossMaster;

        public static GameObject dededePrefab;
        public static GameObject dededeMaster;

        public static GameObject nemGasGrenade;
        public static GameObject nemGas;

        public static GameObject hammerProjectile;
        public static GameObject gordoProjectileGhost;

        public static readonly Color characterColor = new Color(1, 0.7176f, 0.1725f);

        public static SkillDef hammerSwingDef;
        public static SkillDef hammerChargeDef;//m2
        public static SkillDef minigunFireDef;//skilldef for actually firing the minigun
        public static SkillDef hammerSlamDef;//skilldef for m2 during minigun
        public static SkillDef minigunDownDef;//skilldef used while gun is down
        public static SkillDef minigunUpDef;//skilldef used while gun is up
        public static SkillDef jumpDef;

        public const float passiveRegenBonus = 0.025f;

        public SkillLocator skillLocator;

        public void Init()
        {
            CreatePrefab();
            CreateDisplayPrefab();
            RegisterCharacter();
            //NemItemDisplays.RegisterDisplays();
            RegisterProjectiles();
            NemforcerSkins.RegisterSkins();
            CreateDoppelganger();
            CreateBossPrefab();
            CreateMiniBossPrefab();

            if (Modules.Config.kingDededeBoss.Value) CreateDededeBoss();

            if (EnforcerModPlugin.starstormInstalled) StarstormCompat();
        }

        private static void StarstormCompat()
        {
            Starstorm2.Cores.VoidCore.nemesisSpawns.Add(new Starstorm2.Cores.VoidCore.NemesisSpawnData
            {
                masterPrefab = NemforcerPlugin.minibossMaster,
                itemDrop = RoR2Content.Items.Knurl,
                musicString = ""
            });
        }

        private static GameObject CreateModel(GameObject main, string mdlName)
        {
            EnforcerModPlugin.Destroy(main.transform.Find("ModelBase").gameObject);
            EnforcerModPlugin.Destroy(main.transform.Find("CameraPivot").gameObject);
            EnforcerModPlugin.Destroy(main.transform.Find("AimOrigin").gameObject);

            GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>(mdlName);

            return GameObject.Instantiate(model);
        }

        private static void CreateDisplayPrefab()
        {
            GameObject tempDisplay = Assets.MainAssetBundle.LoadAsset<GameObject>("NemforcerDisplay");

            ChildLocator childLocator = tempDisplay.GetComponent<ChildLocator>();

            CharacterModel characterModel = tempDisplay.AddComponent<CharacterModel>();
            characterModel.body = null;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = Assets.CreateNemMaterial("matNemforcer", 5f, Color.white, 0),
                    renderer = childLocator.FindChild("HammerModel").GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("AltHammer").GetComponentInChildren<MeshRenderer>().material,
                    renderer = childLocator.FindChild("AltHammer").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = true
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("GrenadeL").GetComponentInChildren<MeshRenderer>().material,
                    renderer = childLocator.FindChild("GrenadeL").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = true
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = Assets.CreateNemMaterial("matNemforcer", 5f, Color.white, 0),
                    renderer = childLocator.FindChild("GrenadeR").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = true
                },
                //keep body last for teleporter particles
                // god can i just say i fucking hate this
                // fix your shit hopoo
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("Model").GetComponentInChildren<SkinnedMeshRenderer>().material,
                    renderer = childLocator.FindChild("Model").GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

            characterModel.mainSkinnedMeshRenderer = characterModel.baseRendererInfos[characterModel.baseRendererInfos.Length - 1].renderer.gameObject.GetComponent<SkinnedMeshRenderer>();

            characterDisplay = tempDisplay;
        }

        private static void CreatePrefab()
        {
            characterPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "NemforcerBody");

            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            GameObject model = CreateModel(characterPrefab, "mdlNemforcer");

            GameObject gameObject = new GameObject("ModelBase");
            gameObject.transform.parent = characterPrefab.transform;
            gameObject.transform.localPosition = new Vector3(0f, -0.92f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject2 = new GameObject("CameraPivot");
            gameObject2.transform.parent = gameObject.transform;
            gameObject2.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            gameObject2.transform.localRotation = Quaternion.identity;
            gameObject2.transform.localScale = Vector3.one;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = gameObject.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = characterPrefab.GetComponent<CharacterDirection>();
            characterDirection.moveVector = Vector3.zero;
            characterDirection.targetTransform = gameObject.transform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            CharacterBody bodyComponent = characterPrefab.GetComponent<CharacterBody>();
            bodyComponent.name = "NemesisEnforcerBody";
            bodyComponent.baseNameToken = "NEMFORCER_NAME";
            bodyComponent.subtitleNameToken = "NEMFORCER_SUBTITLE";
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 224;
            bodyComponent.levelMaxHealth = 56;
            bodyComponent.baseRegen = 0.5f;
            bodyComponent.levelRegen = 0.25f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0;
            bodyComponent.baseMoveSpeed = 7;
            bodyComponent.levelMoveSpeed = 0;
            bodyComponent.baseAcceleration = 80;
            bodyComponent.baseJumpPower = 15;
            bodyComponent.levelJumpPower = 0;
            bodyComponent.baseDamage = 12;
            bodyComponent.levelDamage = 2.4f;
            bodyComponent.baseAttackSpeed = 1;
            bodyComponent.levelAttackSpeed = 0;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0;
            bodyComponent.baseArmor = 20;
            bodyComponent.levelArmor = 0;
            bodyComponent.baseJumpCount = 1;
            bodyComponent.sprintingSpeedMultiplier = 1.45f;
            bodyComponent.wasLucky = false;
            bodyComponent.hideCrosshair = false;
            bodyComponent.crosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/SimpleDotCrosshair");
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.nemCharPortrait;
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.skinIndex = 0U;
            bodyComponent.preferredPodPrefab = null;
            bodyComponent.bodyColor = characterColor;

            Modules.States.AddSkill(typeof(EntityStates.Nemforcer.NemforcerMain));
            Modules.States.AddSkill(typeof(EntityStates.Nemforcer.SpawnState));

            var stateMachine = bodyComponent.GetComponent<EntityStateMachine>();
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Nemforcer.NemforcerMain));
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.Nemforcer.SpawnState));

            CharacterMotor characterMotor = characterPrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 200f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;

            CameraTargetParams cameraTargetParams = characterPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();
            cameraTargetParams.cameraParams.maxPitch = 70;
            cameraTargetParams.cameraParams.minPitch = -70;
            cameraTargetParams.cameraParams.wallCushion = 0.1f;
            cameraTargetParams.cameraParams.pivotVerticalOffset = 1.37f;
            cameraTargetParams.cameraParams.standardLocalCameraPos = new Vector3(0, 0.5f, -12);

            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.idealLocalCameraPos = Vector3.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            ModelLocator modelLocator = characterPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = gameObject.transform;

            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = Assets.CreateNemMaterial("matNemforcer", 5f, Color.white, 0),
                    renderer = childLocator.FindChild("HammerModel").GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("AltHammer").GetComponentInChildren<MeshRenderer>().material,
                    renderer = childLocator.FindChild("AltHammer").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = true
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("GrenadeL").GetComponentInChildren<MeshRenderer>().material,
                    renderer = childLocator.FindChild("GrenadeL").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = true
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("GrenadeR").GetComponentInChildren<MeshRenderer>().material,
                    renderer = childLocator.FindChild("GrenadeR").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = true
                },
                //keep body last for teleporter particles
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = Assets.CreateNemMaterial("matNemforcer", 5f, Color.white, 0),
                    renderer = childLocator.FindChild("Model").GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };

            Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/hgstandard");

            foreach (CharacterModel.RendererInfo i in characterModel.baseRendererInfos) 
            {
                if (i.defaultMaterial) i.defaultMaterial.shader = hotpoo;
            }

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

            characterModel.mainSkinnedMeshRenderer = characterModel.baseRendererInfos[characterModel.baseRendererInfos.Length - 1].renderer.gameObject.GetComponent<SkinnedMeshRenderer>();

            TeamComponent teamComponent = null;
            if (characterPrefab.GetComponent<TeamComponent>() != null) teamComponent = characterPrefab.GetComponent<TeamComponent>();
            else teamComponent = characterPrefab.GetComponent<TeamComponent>();
            teamComponent.hideAllyCardDisplay = false;
            teamComponent.teamIndex = TeamIndex.None;

            characterPrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
            characterPrefab.GetComponent<InteractionDriver>().highlightInteractor = true;

            CharacterDeathBehavior characterDeathBehavior = characterPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
            //characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            SfxLocator sfxLocator = characterPrefab.GetComponent<SfxLocator>();
            //sfxLocator.deathSound = Sounds.DeathSound;
            sfxLocator.barkSound = "";
            sfxLocator.openSound = "";
            sfxLocator.landingSound = "Play_char_land";
            sfxLocator.fallDamageSound = "Play_char_land_fall_damage";
            sfxLocator.aliveLoopStart = "";
            sfxLocator.aliveLoopStop = "";

            Rigidbody rigidbody = characterPrefab.GetComponent<Rigidbody>();
            rigidbody.mass = 200f;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;

            CapsuleCollider capsuleCollider = characterPrefab.GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = false;
            capsuleCollider.material = null;
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;

            KinematicCharacterMotor kinematicCharacterMotor = characterPrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            kinematicCharacterMotor.Capsule = capsuleCollider;
            kinematicCharacterMotor.Rigidbody = rigidbody;

            kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 0.2f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;
            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;
            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = Vector3.up;
            kinematicCharacterMotor.StepHandling = StepHandlingMethod.None;
            kinematicCharacterMotor.LedgeHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.SafeMovement = false;

            HealthComponent healthComponent = characterPrefab.GetComponent<HealthComponent>();

            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();

            HurtBox mainHurtbox = model.transform.Find("MainHurtbox").GetComponent<CapsuleCollider>().gameObject.AddComponent<HurtBox>();
            mainHurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            mainHurtbox.healthComponent = healthComponent;
            mainHurtbox.isBullseye = true;
            mainHurtbox.damageModifier = HurtBox.DamageModifier.Normal;
            mainHurtbox.hurtBoxGroup = hurtBoxGroup;
            mainHurtbox.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                mainHurtbox
            };

            hurtBoxGroup.mainHurtBox = mainHurtbox;
            hurtBoxGroup.bullseyeCount = 1;

            HitBoxGroup hitBoxGroup = model.AddComponent<HitBoxGroup>();
            #region legacyHitboxes
            ////make a hitbox for hammer (old)
            //GameObject hammerHitbox = childLocator.FindChild("HammerHitbox").gameObject;
            //hammerHitbox.transform.localScale = new Vector3(0.155f, 0.17f, 0.08f);
            //hammerHitbox.transform.localPosition = Vector3.up * 0.02f;
            //hammerHitbox.layer = LayerIndex.projectile.intVal;

            //HitBox hitBox0 = hammerHitbox.AddComponent<HitBox>();

            //hitBoxGroup.hitBoxes = new HitBox[]
            //{
            //    hitBox0,
            //};

            ////make hitboxes for hammer (old)
            //GameObject hammerHitbox1 = childLocator.FindChild("HammerHitboxHead").gameObject;
            //hammerHitbox1.transform.localScale = new Vector3(0.155f, 0.102f, 0.08f);
            //hammerHitbox1.transform.localPosition = Vector3.up * 0.0518f;
            //hammerHitbox1.layer = LayerIndex.projectile.intVal;

            //GameObject hammerHitbox2 = childLocator.FindChild("HammerHitboxShaft").gameObject;
            //hammerHitbox2.transform.localScale = new Vector3(0.155f, 0.102f, 0.043f);
            //hammerHitbox2.transform.localPosition = Vector3.up * -0.0144f;
            //hammerHitbox2.layer = LayerIndex.projectile.intVal;

            //HitBox hitBox1 = hammerHitbox1.AddComponent<HitBox>();
            //HitBox hitBox11 = hammerHitbox2.AddComponent<HitBox>();

            //hitBoxGroup.hitBoxes = new HitBox[]
            //{
            //    hitBox1,
            //    hitBox11,
            //};
            #endregion

            //make hitboxes for hammer (old)
            GameObject hammerHitbox1 = childLocator.FindChild("HammerHitboxFront").gameObject;
            hammerHitbox1.layer = LayerIndex.projectile.intVal;

            GameObject hammerHitbox2 = childLocator.FindChild("HammerHitboxBack").gameObject;
            hammerHitbox2.layer = LayerIndex.projectile.intVal;

            HitBox hitBox1 = hammerHitbox1.AddComponent<HitBox>();
            HitBox hitBox11 = hammerHitbox2.AddComponent<HitBox>();

            hitBoxGroup.hitBoxes = new HitBox[]
            {
                hitBox1,
                hitBox11,
            };
            hitBoxGroup.groupName = "Hammer";

            //uppercut hitbox

            HitBoxGroup hitBoxGroup2 = model.AddComponent<HitBoxGroup>();

            GameObject uppercutHitbox = childLocator.FindChild("UppercutHitbox").gameObject;
            uppercutHitbox.transform.localScale = Vector3.one * 10f;

            HitBox hitBox2 = uppercutHitbox.AddComponent<HitBox>();
            uppercutHitbox.layer = LayerIndex.projectile.intVal;

            hitBoxGroup2.hitBoxes = new HitBox[]
            {
                hitBox2
            };

            hitBoxGroup2.groupName = "Uppercut";

            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

            RagdollController ragdollController = model.GetComponent<RagdollController>();

            PhysicMaterial physicMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RagdollController>().bones[1].GetComponent<Collider>().material;

            foreach (Transform i in ragdollController.bones)
            {
                if (i)
                {
                    i.gameObject.layer = LayerIndex.ragdoll.intVal;
                    Collider j = i.GetComponent<Collider>();
                    if (j)
                    {
                        j.material = physicMat;
                        j.sharedMaterial = physicMat;
                    }
                }
            }

            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMax = 60f;
            aimAnimator.pitchRangeMin = -60f;
            aimAnimator.yawRangeMin = -90f;
            aimAnimator.yawRangeMax = 90f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 3f;
            aimAnimator.inputBank = characterPrefab.GetComponent<InputBankTest>();

            characterPrefab.AddComponent<NemforcerController>();
        }

        private void RegisterCharacter()
        {
            string desc = "Nemesis Enforcer is an incarnation of valiance and strength, a supernatural kind who is nobody to take lightly.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Golden Hammer can hit many enemies at once." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Use Dominance from high up to perform a devastating slam." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Strafing with Golden Minigun is key to taking down powerful bosses." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Shields are for pussies." + Environment.NewLine + Environment.NewLine;

            LanguageAPI.Add("NEMFORCER_NAME", characterName);
            LanguageAPI.Add("NEMFORCER_DESCRIPTION", desc);
            LanguageAPI.Add("NEMFORCER_SUBTITLE", characterSubtitle);
            //LanguageAPI.Add("ENFORCER_LORE", "I'M FUCKING INVINCIBLE");
            LanguageAPI.Add("NEMFORCER_LORE", characterLore);
            LanguageAPI.Add("NEMFORCER_OUTRO_FLAVOR", characterOutro);
            LanguageAPI.Add("NEMFORCER_OUTRO_FAILURE", characterOutroFailure);

            characterDisplay.AddComponent<NetworkIdentity>();

            Modules.Survivors.RegisterNewSurvivor(characterPrefab, characterDisplay, "NEMFORCER", EnforcerUnlockables.nemesisUnlockableDef, 4.01f);

            SkillSetup();

            EnforcerModPlugin.bodyPrefabs.Add(characterPrefab);
        }

        private void RegisterProjectiles()
        {
            hammerProjectile = Resources.Load<GameObject>("Prefabs/Projectiles/EngiGrenadeProjectile").InstantiateClone("NemHammerProjectile", true);

            ProjectileController hammerController = hammerProjectile.GetComponent<ProjectileController>();
            ProjectileImpactExplosion hammerImpact = hammerProjectile.GetComponent<ProjectileImpactExplosion>();

            GameObject hammerModel = Assets.hammerProjectileModel.InstantiateClone("HammerProjectileGhost", true);
            hammerModel.AddComponent<NetworkIdentity>();
            hammerModel.AddComponent<ProjectileGhostController>();
            hammerController.transform.localScale *= 1.75f;

            hammerController.ghostPrefab = hammerModel;

            hammerImpact.lifetimeExpiredSoundString = "";
            hammerImpact.explosionSoundString = "";
            hammerImpact.offsetForLifetimeExpiredSound = 1;
            hammerImpact.destroyOnEnemy = true;
            hammerImpact.destroyOnWorld = true;
            hammerImpact.timerAfterImpact = false;
            hammerImpact.falloffModel = BlastAttack.FalloffModel.None;
            hammerImpact.lifetime = 18;
            hammerImpact.lifetimeAfterImpact = 0f;
            hammerImpact.lifetimeRandomOffset = 0f;
            hammerImpact.blastRadius = 2.0f;
            hammerImpact.blastDamageCoefficient = 1;
            hammerImpact.blastProcCoefficient = 1;
            hammerImpact.fireChildren = false;
            hammerImpact.childrenCount = 0;
            hammerImpact.childrenProjectilePrefab = null;
            hammerImpact.childrenDamageCoefficient = 0f;
            hammerImpact.impactEffect = null;

            hammerController.startSound = "";
            hammerController.procCoefficient = 1;

            GameObject gordoModel = Assets.gordoProjectileModel.InstantiateClone("HammerProjectileGhostGordo", true);
            gordoModel.AddComponent<NetworkIdentity>();
            gordoModel.AddComponent<ProjectileGhostController>();
            gordoProjectileGhost = gordoModel;

            nemGasGrenade = Resources.Load<GameObject>("Prefabs/Projectiles/CommandoGrenadeProjectile").InstantiateClone("NemGasGrenade", true);
            nemGas = Resources.Load<GameObject>("Prefabs/Projectiles/SporeGrenadeProjectileDotZone").InstantiateClone("NemGasDotZone", true);

            ProjectileController grenadeController = nemGasGrenade.GetComponent<ProjectileController>();
            ProjectileController nemGasController = nemGas.GetComponent<ProjectileController>();
            ProjectileDamage nemGrenadeDamage = nemGasGrenade.GetComponent<ProjectileDamage>();
            ProjectileDamage nemGasDamage = nemGas.GetComponent<ProjectileDamage>();
            ProjectileImpactExplosion grenadeImpact = nemGasGrenade.GetComponent<ProjectileImpactExplosion>();
            ProjectileDotZone dotZone = nemGas.GetComponent<ProjectileDotZone>();

            dotZone.damageCoefficient = 2f;
            dotZone.fireFrequency = 4f;
            dotZone.forceVector = Vector3.zero;
            dotZone.impactEffect = null;
            dotZone.lifetime = 18f;
            dotZone.overlapProcCoefficient = 0.05f;
            dotZone.transform.localScale = Vector3.one * 32;

            HitBoxGroup gasHitboxGroup = nemGas.GetComponent<HitBoxGroup>();

            gasHitboxGroup.hitBoxes = new HitBox[] { gasHitboxGroup.gameObject.AddComponent<HitBox>() };

            nemGasDamage.damageType = DamageType.BlightOnHit;

            BuffWard buffWard2 = nemGas.AddComponent<BuffWard>();

            buffWard2.radius = 18;
            buffWard2.interval = 1;
            buffWard2.rangeIndicator = null;
            buffWard2.buffDef = Modules.Buffs.nemImpairedBuff;
            buffWard2.buffDuration = 1.5f;
            buffWard2.floorWard = false;
            buffWard2.expires = false;
            buffWard2.invertTeamFilter = true;
            buffWard2.expireDuration = 0;
            buffWard2.animateRadius = false;

            GameObject nemGrenadeModel = Assets.nemGasGrenadeModel.InstantiateClone("NemGasGrenadeGhost", true);
            nemGrenadeModel.AddComponent<NetworkIdentity>();
            nemGrenadeModel.AddComponent<ProjectileGhostController>();

            grenadeController.ghostPrefab = nemGrenadeModel;
            //tearGasController.ghostPrefab = Assets.tearGasEffectPrefab;

            grenadeImpact.lifetimeExpiredSoundString = "";
            grenadeImpact.explosionSoundString = Sounds.GasExplosion;
            grenadeImpact.offsetForLifetimeExpiredSound = 1;
            grenadeImpact.destroyOnEnemy = false;
            grenadeImpact.destroyOnWorld = false;
            grenadeImpact.timerAfterImpact = true;
            grenadeImpact.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            grenadeImpact.lifetime = 18;
            grenadeImpact.lifetimeAfterImpact = 0.5f;
            grenadeImpact.lifetimeRandomOffset = 0;
            grenadeImpact.blastRadius = 6;
            grenadeImpact.blastDamageCoefficient = 1;
            grenadeImpact.blastProcCoefficient = 1;
            grenadeImpact.fireChildren = true;
            grenadeImpact.childrenCount = 1;
            grenadeImpact.childrenProjectilePrefab = nemGas;
            grenadeImpact.childrenDamageCoefficient = 0.25f;
            grenadeImpact.impactEffect = null;

            grenadeController.startSound = "";
            grenadeController.procCoefficient = 1;
            nemGasController.procCoefficient = 0;

            nemGrenadeDamage.crit = false;
            nemGrenadeDamage.damage = 0f;
            nemGrenadeDamage.damageColorIndex = DamageColorIndex.Default;
            nemGrenadeDamage.damageType = DamageType.Stun1s;
            nemGrenadeDamage.force = 0;

            nemGasDamage.crit = false;
            nemGasDamage.damage = 1f;
            nemGasDamage.damageColorIndex = DamageColorIndex.WeakPoint;
            nemGasDamage.damageType = DamageType.Generic;
            nemGasDamage.force = -10;

            EnforcerModPlugin.Destroy(nemGas.transform.GetChild(0).gameObject);
            GameObject gasFX = Assets.nemGasEffectPrefab.InstantiateClone("FX", false);
            gasFX.AddComponent<TearGasComponent>();
            gasFX.AddComponent<DestroyOnTimer>().duration = 18f;
            gasFX.transform.parent = nemGas.transform;
            gasFX.transform.localPosition = Vector3.zero;

            nemGasGrenade.AddComponent<DestroyOnTimer>().duration = 32;
            nemGas.AddComponent<DestroyOnTimer>().duration = 18;

            EnforcerModPlugin.projectilePrefabs.Add(nemGasGrenade);
            EnforcerModPlugin.projectilePrefabs.Add(nemGas);
            EnforcerModPlugin.projectilePrefabs.Add(hammerProjectile);
        }

        private void SkillSetup()
        {
            foreach (GenericSkill obj in characterPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }

            skillLocator = characterPrefab.GetComponent<SkillLocator>();

            PassiveSetup();
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }

        private void PassiveSetup()
        {
            LanguageAPI.Add("NEMFORCER_PASSIVE_NAME", "Colossus");
            LanguageAPI.Add("NEMFORCER_PASSIVE_DESCRIPTION", $"Nemesis Enforcer gains <style=cIsHealing>bonus health regen</style>, based on his current <style=cIsHealth>missing health</style>, up to <style=cIsHealth>{100 * NemforcerPlugin.passiveRegenBonus}% max health</style>.");

            skillLocator.passiveSkill.enabled = true;
            skillLocator.passiveSkill.skillNameToken = "NEMFORCER_PASSIVE_NAME";
            skillLocator.passiveSkill.skillDescriptionToken = "NEMFORCER_PASSIVE_DESCRIPTION";
            skillLocator.passiveSkill.icon = Assets.nemIconPassive;
        }

        private void PrimarySetup()
        {
            SkillDef primaryDef1 = PrimarySkillDef_Hammer();
            Modules.Skills.RegisterSkillDef(primaryDef1, typeof(EntityStates.Nemforcer.HammerSwing));
            SkillFamily.Variant primaryVariant1 = Modules.Skills.SetupSkillVariant(primaryDef1);

            SkillDef primaryDef2 = PrimarySkillDef_Throw();
            Modules.Skills.RegisterSkillDef(primaryDef2, typeof(ThrowHammer));
            SkillFamily.Variant primaryVariant2 = Modules.Skills.SetupSkillVariant(primaryDef2);

            skillLocator.primary = Modules.Skills.RegisterSkillsToFamily(characterPrefab, primaryVariant1);

            if (Config.cursed.Value) Modules.Skills.RegisterAdditionalSkills(skillLocator.primary, primaryVariant2);

            SkillDef primaryDefMinigun = PrimarySkillDef_FireMinigun();
            Modules.Skills.RegisterSkillDef(primaryDefMinigun,
                                         typeof(NemMinigunFire),
                                         typeof(NemMinigunSpinDown),
                                         typeof(NemMinigunSpinUp),
                                         typeof(NemMinigunState));

            minigunFireDef = primaryDefMinigun;
        }

        private void SecondarySetup()
        {
            SkillDef secondaryDef1 = SecondarySkillDef_HammerUppercut();
            Modules.Skills.RegisterSkillDef(secondaryDef1, typeof(HammerCharge), typeof(HammerUppercut), typeof(HammerAirSlam));
            SkillFamily.Variant secondaryVariant1 = Modules.Skills.SetupSkillVariant(secondaryDef1);

            SkillDef secondaryGunDef1 = SecondarySkillDef_HammerSlam();
            Modules.Skills.RegisterSkillDef(secondaryGunDef1, typeof(HammerSlam));
            SkillFamily.Variant secondaryGunVariant1 = Modules.Skills.SetupSkillVariant(secondaryGunDef1);

            skillLocator.secondary = Modules.Skills.RegisterSkillsToFamily(characterPrefab, "nemSecondary", secondaryVariant1);

            GenericSkill secondaryAlt = Modules.Skills.RegisterSkillsToFamily(characterPrefab, "nemSecondaryMinigun", secondaryGunVariant1);

            hammerChargeDef = secondaryDef1;
            hammerSlamDef = secondaryGunDef1;
        }

        private void UtilitySetup()
        {
            SkillDef utilityDef1 = UtilitySkillDef_Gas();
            Modules.Skills.RegisterSkillDef(utilityDef1, typeof(AimNemGas));
            SkillFamily.Variant utilityVariant1 = Modules.Skills.SetupSkillVariant(utilityDef1);

            SkillDef utilityDef2 = UtilitySkillDef_Grenade();
            Modules.Skills.RegisterSkillDef(utilityDef2, typeof(StunGrenade));
            SkillFamily.Variant utilityVariant2 = Modules.Skills.SetupSkillVariant(utilityDef2);

            SkillDef utilityDef3 = UtilitySkillDef_Jump();
            Modules.Skills.RegisterSkillDef(utilityDef3, typeof(SuperDededeJump));
            SkillFamily.Variant utilityVariant3 = Modules.Skills.SetupSkillVariant(utilityDef3);

            SkillDef utilityDef4 = UtilitySkillDef_HeatCrash();
            Modules.Skills.RegisterSkillDef(utilityDef4, typeof(HeatCrash));
            SkillFamily.Variant utilityVariant4 = Modules.Skills.SetupSkillVariant(utilityDef4);

            skillLocator.utility = Modules.Skills.RegisterSkillsToFamily(characterPrefab, utilityVariant4, utilityVariant1, utilityVariant2);

            if (Config.cursed.Value) Modules.Skills.RegisterAdditionalSkills(skillLocator.utility, utilityVariant3);
        }

        private void SpecialSetup()
        {
            SkillDef specialDef1 = SpecialSkillDef_MinigunUp();
            Modules.Skills.RegisterSkillDef(specialDef1, typeof(MinigunToggle));
            SkillFamily.Variant specialVariant1 = Modules.Skills.SetupSkillVariant(specialDef1);

            skillLocator.special = Modules.Skills.RegisterSkillsToFamily(characterPrefab, specialVariant1);

            SkillDef specialDef2 = SpecialSkillDef_MinigunDown();
            Modules.Skills.RegisterSkillDef(specialDef2);

            minigunDownDef = specialDef1;
            minigunUpDef = specialDef2;
        }

        #region skilldefs
        private static SkillDef PrimarySkillDef_Hammer()
        {
            string desc = "Swing your hammer for <style=cIsDamage>" + 100f * EntityStates.Nemforcer.HammerSwing.damageCoefficient + "%</style> damage.";

            LanguageAPI.Add("NEMFORCER_PRIMARY_HAMMER_NAME", "Golden Hammer");
            LanguageAPI.Add("NEMFORCER_PRIMARY_HAMMER_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.Nemforcer.HammerSwing));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.resetCooldownTimerOnUse = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.nIcon1;
            mySkillDef.skillDescriptionToken = "NEMFORCER_PRIMARY_HAMMER_DESCRIPTION";
            mySkillDef.skillName = "NEMFORCER_PRIMARY_HAMMER_NAME";
            mySkillDef.skillNameToken = "NEMFORCER_PRIMARY_HAMMER_NAME";

            hammerSwingDef = mySkillDef;

            return mySkillDef;
        }

        private static SkillDef PrimarySkillDef_Throw()
        {
            string desc = "Throw a hammer for <style=cIsDamage>" + 100f * EntityStates.Nemforcer.ThrowHammer.damageCoefficient + "%</style> damage.";

            LanguageAPI.Add("NEMFORCER_PRIMARY_THROWHAMMER_NAME", "Throwing Hammer");
            LanguageAPI.Add("NEMFORCER_PRIMARY_THROWHAMMER_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.Nemforcer.ThrowHammer));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.resetCooldownTimerOnUse = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.nIcon1C;
            mySkillDef.skillDescriptionToken = "NEMFORCER_PRIMARY_THROWHAMMER_DESCRIPTION";
            mySkillDef.skillName = "NEMFORCER_PRIMARY_THROWHAMMER_NAME";
            mySkillDef.skillNameToken = "NEMFORCER_PRIMARY_THROWHAMMER_NAME";

            return mySkillDef;
        }

        private static SkillDef PrimarySkillDef_FireMinigun()
        {
            string desc = "Rev up and fire a hail of bullets dealing <style=cIsDamage>" + NemMinigunFire.baseDamageCoefficient * 100f + "% damage</style> per bullet. <style=cIsUtility>Slows your movement while shooting.</style>";

            LanguageAPI.Add("NEMFORCER_PRIMARY_MINIGUN_NAME", "Golden Minigun");
            LanguageAPI.Add("NEMFORCER_PRIMARY_MINIGUN_DESCRIPTION", desc);

            SkillDef mySkillDef2 = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef2.activationState = new SerializableEntityStateType(typeof(NemMinigunSpinUp));
            mySkillDef2.activationStateMachineName = "Weapon";
            mySkillDef2.baseMaxStock = 1;
            mySkillDef2.baseRechargeInterval = 0f;
            mySkillDef2.beginSkillCooldownOnSkillEnd = false;
            mySkillDef2.canceledFromSprinting = false;
            mySkillDef2.fullRestockOnAssign = true;
            mySkillDef2.interruptPriority = InterruptPriority.Any;
            mySkillDef2.resetCooldownTimerOnUse = false;
            mySkillDef2.isCombatSkill = true;
            mySkillDef2.mustKeyPress = false;
            mySkillDef2.cancelSprintingOnActivation = true;
            mySkillDef2.rechargeStock = 1;
            mySkillDef2.requiredStock = 1;
            mySkillDef2.stockToConsume = 1;
            mySkillDef2.icon = Assets.nIcon1B;
            mySkillDef2.skillDescriptionToken = "NEMFORCER_PRIMARY_MINIGUN_DESCRIPTION";
            mySkillDef2.skillName = "NEMFORCER_PRIMARY_MINIGUN_NAME";
            mySkillDef2.skillNameToken = "NEMFORCER_PRIMARY_MINIGUN_NAME";
            return mySkillDef2;
        }

        private static SkillDef SecondarySkillDef_HammerUppercut()
        {
            LanguageAPI.Add("KEYWORD_SLAM", $"<style=cKeywordName>Downward Slam</style><style=cIsDamage>Stunning.</style> Viciously <style=cIsHealth>crash down</style> with your hammer, dealing <style=cIsDamage>{100f * HammerAirSlam.minDamageCoefficient}%-{100f * HammerAirSlam.maxDamageCoefficient}% damage</style> and dealing an extra <style=cIsDamage>30%</style> of that on impact. <style=cIsUtility>Impact radius scales with speed.</style>");

            string desc = $"<style=cIsUtility>Charge up</style>, then lunge forward and unleash a <style=cIsDamage>rising uppercut</style> for <style=cIsDamage>{100f * HammerUppercut.minDamageCoefficient}%-{100f * HammerUppercut.maxDamageCoefficient}% damage</style>. Use while <style=cIsUtility>falling and looking down</style> to perform a <style=cIsUtility>Downward Slam</style> instead.";

            LanguageAPI.Add("NEMFORCER_SECONDARY_BASH_NAME", "Dominance");
            LanguageAPI.Add("NEMFORCER_SECONDARY_BASH_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(HammerCharge));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 6f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.resetCooldownTimerOnUse = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.nIcon2;
            mySkillDef.skillDescriptionToken = "NEMFORCER_SECONDARY_BASH_DESCRIPTION";
            mySkillDef.skillName = "NEMFORCER_SECONDARY_BASH_NAME";
            mySkillDef.skillNameToken = "NEMFORCER_SECONDARY_BASH_NAME";
            mySkillDef.keywordTokens = new string[] {
                "KEYWORD_SLAM",
            };

            return mySkillDef;
        }
        private static SkillDef SecondarySkillDef_HammerSlam()
        {
            string desc = $"<style=cIsDamage>Stunning.</style> While in minigun stance, violently <style=cIsHealth>slam</style> down your hammer, dealing <style=cIsDamage>{100f * HammerSlam.damageCoefficient}% damage</style> and <style=cIsDamage>knocking back</style> enemies hit. <style=cIsUtility>Explodes projectiles.</style>";

            LanguageAPI.Add("NEMFORCER_SECONDARY_SLAM_NAME", "Dominance (Minigun)");
            LanguageAPI.Add("NEMFORCER_SECONDARY_SLAM_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(HammerSlam));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 6f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.resetCooldownTimerOnUse = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.nIcon2B;
            mySkillDef.skillDescriptionToken = "NEMFORCER_SECONDARY_SLAM_DESCRIPTION";
            mySkillDef.skillName = "NEMFORCER_SECONDARY_SLAM_NAME";
            mySkillDef.skillNameToken = "NEMFORCER_SECONDARY_SLAM_NAME";
            mySkillDef.keywordTokens = new string[] {
                "KEYWORD_STUNNING"
            };

            return mySkillDef;
        }

        private static SkillDef UtilitySkillDef_Grenade()
        {
            SkillDef utilityDef1;
            //LanguageAPI.Add("ENFORCER_UTILITY_STUNGRENADE_NAME", "Stun Grenade");
            //LanguageAPI.Add("ENFORCER_UTILITY_STUNGRENADE_DESCRIPTION", "<style=cIsDamage>Stunning</style>. Launch a stun grenade, dealing <style=cIsDamage>" + 100f * StunGrenade.damageCoefficient + "% damage</style>. <style=cIsUtility>Store up to 3 grenades</style>.");

            utilityDef1 = ScriptableObject.CreateInstance<SkillDef>();
            utilityDef1.activationState = new SerializableEntityStateType(typeof(StunGrenade));
            utilityDef1.activationStateMachineName = "Weapon";
            utilityDef1.baseMaxStock = 3;
            utilityDef1.baseRechargeInterval = 8f;
            utilityDef1.beginSkillCooldownOnSkillEnd = false;
            utilityDef1.canceledFromSprinting = false;
            utilityDef1.fullRestockOnAssign = true;
            utilityDef1.interruptPriority = InterruptPriority.Skill;
            utilityDef1.resetCooldownTimerOnUse = false;
            utilityDef1.isCombatSkill = true;
            utilityDef1.mustKeyPress = false;
            utilityDef1.cancelSprintingOnActivation = true;
            utilityDef1.rechargeStock = 1;
            utilityDef1.requiredStock = 1;
            utilityDef1.stockToConsume = 1;
            utilityDef1.icon = Assets.nIcon3B;
            utilityDef1.skillDescriptionToken = "ENFORCER_UTILITY_STUNGRENADE_DESCRIPTION";
            utilityDef1.skillName = "ENFORCER_UTILITY_STUNGRENADE_NAME";
            utilityDef1.skillNameToken = "ENFORCER_UTILITY_STUNGRENADE_NAME";
            utilityDef1.keywordTokens = new string[] {
                "KEYWORD_STUNNING"
            };

            return utilityDef1;
        }

        private static SkillDef UtilitySkillDef_Gas()
        {
            LanguageAPI.Add("NEMFORCER_UTILITY_GAS_NAME", "XM47 Grenade");
            LanguageAPI.Add("NEMFORCER_UTILITY_GAS_DESCRIPTION", "Throw a grenade that explodes into a cloud of <style=cIsUtility>corrosive gas</style> that <style=cIsUtility>slows</style> and deals <style=cIsDamage>200% damage per second</style> and lasts for <style=cIsDamage>16 seconds</style>.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(AimNemGas));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 24;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.resetCooldownTimerOnUse = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.nIcon3;
            mySkillDef.skillDescriptionToken = "NEMFORCER_UTILITY_GAS_DESCRIPTION";
            mySkillDef.skillName = "NEMFORCER_UTILITY_GAS_NAME";
            mySkillDef.skillNameToken = "NEMFORCER_UTILITY_GAS_NAME";

            return mySkillDef;
        }

        private static SkillDef UtilitySkillDef_Jump()
        {
            LanguageAPI.Add("NEMFORCER_UTILITY_JUMP_NAME", "Super Dedede Jump");
            LanguageAPI.Add("NEMFORCER_UTILITY_JUMP_DESCRIPTION", "Jump into the air, then slam down for <style=cIsDamage>" + 100f * SuperDededeJump.slamDamageCoefficient + "% damage</style>. <style=cIsUtility>Deals reduced damage outside the center of the impact.</style>");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(SuperDededeJump));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 24;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.resetCooldownTimerOnUse = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.testIcon;
            mySkillDef.skillDescriptionToken = "NEMFORCER_UTILITY_JUMP_DESCRIPTION";
            mySkillDef.skillName = "NEMFORCER_UTILITY_JUMP_NAME";
            mySkillDef.skillNameToken = "NEMFORCER_UTILITY_JUMP_NAME";

            jumpDef = mySkillDef;

            return mySkillDef;
        }

        private static SkillDef UtilitySkillDef_HeatCrash()
        {
            LanguageAPI.Add("KEYWORD_GRAPPLE", "<style=cKeywordName>Grappling</style><style=cSub>Applies <style=cIsDamage>stun</style> and attempts to <style=cIsUtility>grab</style> a nearby enemy.");

            LanguageAPI.Add("NEMFORCER_UTILITY_CRASH_NAME", "Heat Crash");
            LanguageAPI.Add("NEMFORCER_UTILITY_CRASH_DESCRIPTION", "<style=cIsUtility>Grappling.</style> Jump into the air, then slam down for <style=cIsDamage>" + 100f * HeatCrash.slamDamageCoefficient + "% damage</style>. <style=cIsUtility>Deals reduced damage outside the center of the impact.</style>");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(HeatCrash));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 12;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.resetCooldownTimerOnUse = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.nIcon3C;
            mySkillDef.skillDescriptionToken = "NEMFORCER_UTILITY_CRASH_DESCRIPTION";
            mySkillDef.skillName = "NEMFORCER_UTILITY_CRASH_NAME";
            mySkillDef.skillNameToken = "NEMFORCER_UTILITY_CRASH_NAME";
            mySkillDef.keywordTokens = new string[]
            {
                "KEYWORD_GRAPPLE"
            };

            return mySkillDef;
        }

        private static SkillDef SpecialSkillDef_MinigunUp()
        {
            LanguageAPI.Add("NEMFORCER_SPECIAL_MINIGUNUP_NAME", "Suppression Stance");
            LanguageAPI.Add("NEMFORCER_SPECIAL_MINIGUNUP_DESCRIPTION", "Take an offensive stance, <style=cIsDamage>readying your minigun</style>. <style=cIsHealth>Prevents sprinting</style>.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(MinigunToggle));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.resetCooldownTimerOnUse = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.nIcon4;
            mySkillDef.skillDescriptionToken = "NEMFORCER_SPECIAL_MINIGUNUP_DESCRIPTION";
            mySkillDef.skillName = "NEMFORCER_SPECIAL_MINIGUNUP_NAME";
            mySkillDef.skillNameToken = "NEMFORCER_SPECIAL_MINIGUNUP_NAME";

            return mySkillDef;
        }
        private static SkillDef SpecialSkillDef_MinigunDown()
        {
            LanguageAPI.Add("NEMFORCER_SPECIAL_MINIGUNDOWN_NAME", "Destruction Stance");
            LanguageAPI.Add("NEMFORCER_SPECIAL_MINIGUNDOWN_DESCRIPTION", "<style=cIsUtility>Sheathe your minigun</style>.");

            SkillDef mySkillDef2 = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef2.activationState = new SerializableEntityStateType(typeof(MinigunToggle));
            mySkillDef2.activationStateMachineName = "Weapon";
            mySkillDef2.baseMaxStock = 1;
            mySkillDef2.baseRechargeInterval = 0f;
            mySkillDef2.beginSkillCooldownOnSkillEnd = false;
            mySkillDef2.canceledFromSprinting = false;
            mySkillDef2.fullRestockOnAssign = true;
            mySkillDef2.interruptPriority = InterruptPriority.Skill;
            mySkillDef2.resetCooldownTimerOnUse = false;
            mySkillDef2.isCombatSkill = true;
            mySkillDef2.mustKeyPress = true;
            mySkillDef2.cancelSprintingOnActivation = false;
            mySkillDef2.rechargeStock = 1;
            mySkillDef2.requiredStock = 1;
            mySkillDef2.stockToConsume = 1;
            mySkillDef2.icon = Assets.nIcon4B;
            mySkillDef2.skillDescriptionToken = "NEMFORCER_SPECIAL_MINIGUNDOWN_DESCRIPTION";
            mySkillDef2.skillName = "NEMFORCER_SPECIAL_MINIGUNDOWN_NAME";
            mySkillDef2.skillNameToken = "NEMFORCER_SPECIAL_MINIGUNDOWN_NAME";

            return mySkillDef2;
        }
        #endregion

        private void CreateDoppelganger()
        {
            doppelganger = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/LemurianMaster"), "NemesisEnforcerMonsterMaster", true);
            doppelganger.GetComponent<CharacterMaster>().bodyPrefab = characterPrefab;

            CreateUmbraAI();

            EnforcerModPlugin.masterPrefabs.Add(doppelganger);
        }

        private void CreateBossPrefab()
        {
            bossPrefab = PrefabAPI.InstantiateClone(characterPrefab, "NemesisEnforcerBossBody");

            bossPrefab.GetComponent<ModelLocator>().modelBaseTransform.localScale *= 1.75f;

            EnforcerModPlugin.Destroy(bossPrefab.transform.Find("ModelBase").gameObject);
            EnforcerModPlugin.Destroy(bossPrefab.transform.Find("CameraPivot").gameObject);
            EnforcerModPlugin.Destroy(bossPrefab.transform.Find("AimOrigin").gameObject);

            CharacterBody charBody = bossPrefab.GetComponent<CharacterBody>();

            LanguageAPI.Add("NEMFORCER_BOSS_NAME", "Ultra Nemesis Enforcer");
            LanguageAPI.Add("NEMFORCER_BOSS_SUBTITLE", bossSubtitle);

            charBody.name = "NemesisEnforcerBossBody";
            charBody.baseNameToken = "NEMFORCER_NAME";
            if (EnforcerModPlugin.starstormInstalled) charBody.baseNameToken = "NEMFORCER_BOSS_NAME";
            charBody.subtitleNameToken = "NEMFORCER_BOSS_SUBTITLE";
            charBody.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            charBody.rootMotionInMainState = false;
            charBody.mainRootSpeed = 0;
            charBody.baseMaxHealth = 1000;
            charBody.levelMaxHealth = 300;
            charBody.baseRegen = 0f;
            charBody.levelRegen = 0f;
            charBody.baseMaxShield = 0;
            charBody.levelMaxShield = 0;
            charBody.baseMoveSpeed = 7;
            charBody.levelMoveSpeed = 0;
            charBody.baseAcceleration = 80;
            charBody.baseJumpPower = 15;
            charBody.levelJumpPower = 0;
            charBody.baseDamage = 16;
            charBody.levelDamage = 3.2f;
            charBody.baseAttackSpeed = 1;
            charBody.levelAttackSpeed = 0;
            charBody.baseCrit = 0;
            charBody.levelCrit = 0;
            charBody.baseArmor = 20;
            charBody.levelArmor = 0;
            charBody.baseJumpCount = 1;
            charBody.portraitIcon = Assets.nemBossPortrait;
            charBody.isChampion = true;
            charBody.skinIndex = 0U;
            charBody.bodyColor = characterColor;

            charBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            EnforcerModPlugin.bodyPrefabs.Add(bossPrefab);

            bossMaster = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/LemurianMaster"), "NemesisEnforcerBossMaster", true);
            bossMaster.GetComponent<CharacterMaster>().bodyPrefab = bossPrefab;

            bossPrefab.AddComponent<NemesisUnlockComponent>();

            CreateNemesisAI();

            EnforcerModPlugin.masterPrefabs.Add(bossMaster);
        }

        private void CreateMiniBossPrefab()
        {
            minibossPrefab = PrefabAPI.InstantiateClone(characterPrefab, "NemesisEnforcerMiniBossBody");

            minibossPrefab.GetComponent<ModelLocator>().modelBaseTransform.localScale *= 1.5f;

            EnforcerModPlugin.Destroy(minibossPrefab.transform.Find("ModelBase").gameObject);
            EnforcerModPlugin.Destroy(minibossPrefab.transform.Find("CameraPivot").gameObject);
            EnforcerModPlugin.Destroy(minibossPrefab.transform.Find("AimOrigin").gameObject);

            CharacterBody charBody = minibossPrefab.GetComponent<CharacterBody>();

            charBody.name = "NemesisEnforcerMiniBossBody";
            charBody.baseNameToken = "NEMFORCER_NAME";
            charBody.subtitleNameToken = "NEMFORCER_SUBTITLE";
            charBody.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            charBody.rootMotionInMainState = false;
            charBody.mainRootSpeed = 0;
            charBody.baseMaxHealth = 1000;
            charBody.levelMaxHealth = 300;
            charBody.baseRegen = 0f;
            charBody.levelRegen = 0f;
            charBody.baseMaxShield = 0;
            charBody.levelMaxShield = 0;
            charBody.baseMoveSpeed = 7;
            charBody.levelMoveSpeed = 0;
            charBody.baseAcceleration = 80;
            charBody.baseJumpPower = 15;
            charBody.levelJumpPower = 0;
            charBody.baseDamage = 16;
            charBody.levelDamage = 3.2f;
            charBody.baseAttackSpeed = 1;
            charBody.levelAttackSpeed = 0;
            charBody.baseCrit = 0;
            charBody.levelCrit = 0;
            charBody.baseArmor = 20;
            charBody.levelArmor = 0;
            charBody.baseJumpCount = 1;
            charBody.portraitIcon = Assets.nemBossPortrait;
            charBody.isChampion = true;
            charBody.skinIndex = 0U;
            charBody.bodyColor = characterColor;

            charBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            EnforcerModPlugin.bodyPrefabs.Add(minibossPrefab);

            minibossMaster = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/LemurianMaster"), "NemesisEnforcerMiniBossMaster", true);
            minibossMaster.GetComponent<CharacterMaster>().bodyPrefab = minibossPrefab;

            minibossPrefab.AddComponent<NemesisUnlockComponent>();

            CreateMiniNemesisAI();

            EnforcerModPlugin.masterPrefabs.Add(minibossMaster);
        }

        private void CreateUmbraAI()
        {
            foreach (AISkillDriver ai in doppelganger.GetComponentsInChildren<AISkillDriver>())
            {
                BaseUnityPlugin.DestroyImmediate(ai);
            }

            AISkillDriver grenadeDriver = doppelganger.AddComponent<AISkillDriver>();
            grenadeDriver.customName = "ThrowGrenade";
            grenadeDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            grenadeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            grenadeDriver.activationRequiresAimConfirmation = true;
            grenadeDriver.activationRequiresTargetLoS = false;
            grenadeDriver.selectionRequiresTargetLoS = true;
            grenadeDriver.requireSkillReady = true;
            grenadeDriver.maxDistance = 64f;
            grenadeDriver.minDistance = 0f;
            grenadeDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            grenadeDriver.ignoreNodeGraph = false;
            grenadeDriver.moveInputScale = 1f;
            grenadeDriver.driverUpdateTimerOverride = 0.5f;
            grenadeDriver.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            grenadeDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            grenadeDriver.maxTargetHealthFraction = Mathf.Infinity;
            grenadeDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            grenadeDriver.maxUserHealthFraction = Mathf.Infinity;
            grenadeDriver.skillSlot = SkillSlot.Utility;

            AISkillDriver hammerChargeDriver = doppelganger.AddComponent<AISkillDriver>();
            hammerChargeDriver.customName = "HammerCharge";
            hammerChargeDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerChargeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerChargeDriver.activationRequiresAimConfirmation = true;
            hammerChargeDriver.activationRequiresTargetLoS = false;
            hammerChargeDriver.selectionRequiresTargetLoS = true;
            hammerChargeDriver.maxDistance = 24f;
            hammerChargeDriver.minDistance = 12f;
            hammerChargeDriver.requireSkillReady = true;
            hammerChargeDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerChargeDriver.ignoreNodeGraph = true;
            hammerChargeDriver.moveInputScale = 1f;
            hammerChargeDriver.driverUpdateTimerOverride = 2f;
            hammerChargeDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerChargeDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerChargeDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerChargeDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerChargeDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerChargeDriver.skillSlot = SkillSlot.Secondary;
            hammerChargeDriver.requiredSkill = hammerChargeDef;
            hammerChargeDriver.noRepeat = true;

            AISkillDriver hammerSwingCloseDriver = doppelganger.AddComponent<AISkillDriver>();
            hammerSwingCloseDriver.customName = "HammerCloseRange";
            hammerSwingCloseDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            hammerSwingCloseDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerSwingCloseDriver.activationRequiresAimConfirmation = true;
            hammerSwingCloseDriver.activationRequiresTargetLoS = false;
            hammerSwingCloseDriver.selectionRequiresTargetLoS = true;
            hammerSwingCloseDriver.maxDistance = 3f;
            hammerSwingCloseDriver.minDistance = 0f;
            hammerSwingCloseDriver.requireSkillReady = true;
            hammerSwingCloseDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerSwingCloseDriver.ignoreNodeGraph = true;
            hammerSwingCloseDriver.moveInputScale = 0.4f;
            hammerSwingCloseDriver.driverUpdateTimerOverride = 0.5f;
            hammerSwingCloseDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerSwingCloseDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerSwingCloseDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerSwingCloseDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerSwingCloseDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerSwingCloseDriver.skillSlot = SkillSlot.Primary;
            hammerSwingCloseDriver.requiredSkill = hammerSwingDef;

            AISkillDriver hammerSwingDriver = doppelganger.AddComponent<AISkillDriver>();
            hammerSwingDriver.customName = "WalkAndHammer";
            hammerSwingDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerSwingDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerSwingDriver.activationRequiresAimConfirmation = true;
            hammerSwingDriver.activationRequiresTargetLoS = false;
            hammerSwingDriver.selectionRequiresTargetLoS = true;
            hammerSwingDriver.maxDistance = 12f;
            hammerSwingDriver.minDistance = 0f;
            hammerSwingDriver.requireSkillReady = true;
            hammerSwingDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerSwingDriver.ignoreNodeGraph = true;
            hammerSwingDriver.moveInputScale = 1f;
            hammerSwingDriver.driverUpdateTimerOverride = 0.5f;
            hammerSwingDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerSwingDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerSwingDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerSwingDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerSwingDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerSwingDriver.skillSlot = SkillSlot.Primary;
            hammerSwingDriver.requiredSkill = hammerSwingDef;

            AISkillDriver minigunSlamDriver = doppelganger.AddComponent<AISkillDriver>();
            minigunSlamDriver.customName = "MinigunSecondary";
            minigunSlamDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            minigunSlamDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            minigunSlamDriver.activationRequiresAimConfirmation = true;
            minigunSlamDriver.activationRequiresTargetLoS = false;
            minigunSlamDriver.selectionRequiresTargetLoS = true;
            minigunSlamDriver.maxDistance = 14f;
            minigunSlamDriver.minDistance = 0f;
            minigunSlamDriver.requireSkillReady = true;
            minigunSlamDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            minigunSlamDriver.ignoreNodeGraph = false;
            minigunSlamDriver.moveInputScale = 1f;
            minigunSlamDriver.driverUpdateTimerOverride = -1f;
            minigunSlamDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            minigunSlamDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            minigunSlamDriver.maxTargetHealthFraction = Mathf.Infinity;
            minigunSlamDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            minigunSlamDriver.maxUserHealthFraction = Mathf.Infinity;
            minigunSlamDriver.skillSlot = SkillSlot.Secondary;
            minigunSlamDriver.requiredSkill = hammerSlamDef;

            AISkillDriver swapToHammerDriver = doppelganger.AddComponent<AISkillDriver>();
            swapToHammerDriver.customName = "SwapToHammer";
            swapToHammerDriver.movementType = AISkillDriver.MovementType.Stop;
            swapToHammerDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            swapToHammerDriver.activationRequiresAimConfirmation = false;
            swapToHammerDriver.activationRequiresTargetLoS = false;
            swapToHammerDriver.selectionRequiresTargetLoS = false;
            swapToHammerDriver.maxDistance = 12f;
            swapToHammerDriver.minDistance = 0f;
            swapToHammerDriver.requireSkillReady = true;
            swapToHammerDriver.aimType = AISkillDriver.AimType.MoveDirection;
            swapToHammerDriver.ignoreNodeGraph = false;
            swapToHammerDriver.moveInputScale = 1f;
            swapToHammerDriver.driverUpdateTimerOverride = 0.5f;
            swapToHammerDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            swapToHammerDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            swapToHammerDriver.maxTargetHealthFraction = Mathf.Infinity;
            swapToHammerDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            swapToHammerDriver.maxUserHealthFraction = Mathf.Infinity;
            swapToHammerDriver.skillSlot = SkillSlot.Special;
            swapToHammerDriver.requiredSkill = minigunUpDef;

            AISkillDriver minigunFireDriver = doppelganger.AddComponent<AISkillDriver>();
            minigunFireDriver.customName = "StrafeAndShoot";
            minigunFireDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            minigunFireDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            minigunFireDriver.activationRequiresAimConfirmation = true;
            minigunFireDriver.activationRequiresTargetLoS = false;
            minigunFireDriver.selectionRequiresTargetLoS = true;
            minigunFireDriver.maxDistance = 80f;
            minigunFireDriver.minDistance = 8f;
            minigunFireDriver.requireSkillReady = true;
            minigunFireDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            minigunFireDriver.ignoreNodeGraph = false;
            minigunFireDriver.moveInputScale = 1f;
            minigunFireDriver.driverUpdateTimerOverride = -1f;
            minigunFireDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            minigunFireDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            minigunFireDriver.maxTargetHealthFraction = Mathf.Infinity;
            minigunFireDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            minigunFireDriver.maxUserHealthFraction = Mathf.Infinity;
            minigunFireDriver.skillSlot = SkillSlot.Primary;
            minigunFireDriver.requiredSkill = minigunFireDef;

            AISkillDriver swapToMinigunDriver = doppelganger.AddComponent<AISkillDriver>();
            swapToMinigunDriver.customName = "SwapToMinigun";
            swapToMinigunDriver.movementType = AISkillDriver.MovementType.Stop;
            swapToMinigunDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            swapToMinigunDriver.activationRequiresAimConfirmation = false;
            swapToMinigunDriver.activationRequiresTargetLoS = false;
            swapToMinigunDriver.selectionRequiresTargetLoS = false;
            swapToMinigunDriver.maxDistance = 90f;
            swapToMinigunDriver.minDistance = 30f;
            swapToMinigunDriver.requireSkillReady = true;
            swapToMinigunDriver.aimType = AISkillDriver.AimType.MoveDirection;
            swapToMinigunDriver.ignoreNodeGraph = false;
            swapToMinigunDriver.moveInputScale = 1f;
            swapToMinigunDriver.driverUpdateTimerOverride = -1f;
            swapToMinigunDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            swapToMinigunDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            swapToMinigunDriver.maxTargetHealthFraction = Mathf.Infinity;
            swapToMinigunDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            swapToMinigunDriver.maxUserHealthFraction = Mathf.Infinity;
            swapToMinigunDriver.skillSlot = SkillSlot.Special;
            swapToMinigunDriver.requiredSkill = minigunDownDef;

            /*AISkillDriver strafeIdleDriver = bossMaster.AddComponent<AISkillDriver>();
            strafeIdleDriver.customName = "StrafeIdle";
            strafeIdleDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            strafeIdleDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            strafeIdleDriver.activationRequiresAimConfirmation = false;
            strafeIdleDriver.activationRequiresTargetLoS = false;
            strafeIdleDriver.selectionRequiresTargetLoS = true;
            strafeIdleDriver.maxDistance = 80f;
            strafeIdleDriver.minDistance = 8f;
            strafeIdleDriver.requireSkillReady = true;
            strafeIdleDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            strafeIdleDriver.ignoreNodeGraph = false;
            strafeIdleDriver.moveInputScale = 1f;
            strafeIdleDriver.driverUpdateTimerOverride = -1f;
            strafeIdleDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            strafeIdleDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            strafeIdleDriver.maxTargetHealthFraction = Mathf.Infinity;
            strafeIdleDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            strafeIdleDriver.maxUserHealthFraction = Mathf.Infinity;
            strafeIdleDriver.skillSlot = SkillSlot.None;
            strafeIdleDriver.requiredSkill = minigunUpDef;*/

            AISkillDriver followDriver = doppelganger.AddComponent<AISkillDriver>();
            followDriver.customName = "Chase";
            followDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            followDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            followDriver.activationRequiresAimConfirmation = false;
            followDriver.activationRequiresTargetLoS = false;
            followDriver.maxDistance = Mathf.Infinity;
            followDriver.minDistance = 0f;
            followDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            followDriver.ignoreNodeGraph = false;
            followDriver.moveInputScale = 1f;
            followDriver.driverUpdateTimerOverride = -1f;
            followDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            followDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            followDriver.maxTargetHealthFraction = Mathf.Infinity;
            followDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            followDriver.maxUserHealthFraction = Mathf.Infinity;
            followDriver.skillSlot = SkillSlot.None;
        }

        private void CreateNemesisAI()
        {
            foreach (AISkillDriver ai in bossMaster.GetComponentsInChildren<AISkillDriver>())
            {
                BaseUnityPlugin.DestroyImmediate(ai);
            }

            bossMaster.GetComponent<BaseAI>().fullVision = true;

            /*AISkillDriver grenadeDriver = bossMaster.AddComponent<AISkillDriver>();
            grenadeDriver.customName = "ThrowGrenade";
            grenadeDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            grenadeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            grenadeDriver.activationRequiresAimConfirmation = true;
            grenadeDriver.activationRequiresTargetLoS = false;
            grenadeDriver.selectionRequiresTargetLoS = true;
            grenadeDriver.requireSkillReady = true;
            grenadeDriver.maxDistance = 64f;
            grenadeDriver.minDistance = 0f;
            grenadeDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            grenadeDriver.ignoreNodeGraph = false;
            grenadeDriver.moveInputScale = 1f;
            grenadeDriver.driverUpdateTimerOverride = 0.5f;
            grenadeDriver.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            grenadeDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            grenadeDriver.maxTargetHealthFraction = Mathf.Infinity;
            grenadeDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            grenadeDriver.maxUserHealthFraction = Mathf.Infinity;
            grenadeDriver.skillSlot = SkillSlot.Utility;*/

            AISkillDriver hammerTapDriver = bossMaster.AddComponent<AISkillDriver>();
            hammerTapDriver.customName = "HammerTap";
            hammerTapDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerTapDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerTapDriver.activationRequiresAimConfirmation = true;
            hammerTapDriver.activationRequiresTargetLoS = false;
            hammerTapDriver.selectionRequiresTargetLoS = true;
            hammerTapDriver.maxDistance = 8f;
            hammerTapDriver.minDistance = 2f;
            hammerTapDriver.requireSkillReady = true;
            hammerTapDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerTapDriver.ignoreNodeGraph = true;
            hammerTapDriver.moveInputScale = 1f;
            hammerTapDriver.driverUpdateTimerOverride = 1.5f;
            hammerTapDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerTapDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerTapDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerTapDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerTapDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerTapDriver.skillSlot = SkillSlot.Secondary;
            hammerTapDriver.requiredSkill = hammerChargeDef;
            hammerTapDriver.noRepeat = true;

            AISkillDriver hammerChargeDriver = bossMaster.AddComponent<AISkillDriver>();
            hammerChargeDriver.customName = "HammerCharge";
            hammerChargeDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerChargeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerChargeDriver.activationRequiresAimConfirmation = true;
            hammerChargeDriver.activationRequiresTargetLoS = false;
            hammerChargeDriver.selectionRequiresTargetLoS = true;
            hammerChargeDriver.maxDistance = 24f;
            hammerChargeDriver.minDistance = 12f;
            hammerChargeDriver.requireSkillReady = true;
            hammerChargeDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerChargeDriver.ignoreNodeGraph = true;
            hammerChargeDriver.moveInputScale = 1f;
            hammerChargeDriver.driverUpdateTimerOverride = 2.5f;
            hammerChargeDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerChargeDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerChargeDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerChargeDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerChargeDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerChargeDriver.skillSlot = SkillSlot.Secondary;
            hammerChargeDriver.requiredSkill = hammerChargeDef;
            hammerChargeDriver.noRepeat = true;

            AISkillDriver hammerSwingCloseDriver = bossMaster.AddComponent<AISkillDriver>();
            hammerSwingCloseDriver.customName = "HammerCloseRange";
            hammerSwingCloseDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            hammerSwingCloseDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerSwingCloseDriver.activationRequiresAimConfirmation = true;
            hammerSwingCloseDriver.activationRequiresTargetLoS = false;
            hammerSwingCloseDriver.selectionRequiresTargetLoS = true;
            hammerSwingCloseDriver.maxDistance = 3f;
            hammerSwingCloseDriver.minDistance = 0f;
            hammerSwingCloseDriver.requireSkillReady = true;
            hammerSwingCloseDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerSwingCloseDriver.ignoreNodeGraph = true;
            hammerSwingCloseDriver.moveInputScale = 0.4f;
            hammerSwingCloseDriver.driverUpdateTimerOverride = 0.5f;
            hammerSwingCloseDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerSwingCloseDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerSwingCloseDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerSwingCloseDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerSwingCloseDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerSwingCloseDriver.skillSlot = SkillSlot.Primary;
            hammerSwingCloseDriver.requiredSkill = hammerSwingDef;

            AISkillDriver hammerSwingDriver = bossMaster.AddComponent<AISkillDriver>();
            hammerSwingDriver.customName = "WalkAndHammer";
            hammerSwingDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerSwingDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerSwingDriver.activationRequiresAimConfirmation = true;
            hammerSwingDriver.activationRequiresTargetLoS = false;
            hammerSwingDriver.selectionRequiresTargetLoS = true;
            hammerSwingDriver.maxDistance = 12f;
            hammerSwingDriver.minDistance = 0f;
            hammerSwingDriver.requireSkillReady = true;
            hammerSwingDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerSwingDriver.ignoreNodeGraph = true;
            hammerSwingDriver.moveInputScale = 1f;
            hammerSwingDriver.driverUpdateTimerOverride = 0.5f;
            hammerSwingDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerSwingDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerSwingDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerSwingDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerSwingDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerSwingDriver.skillSlot = SkillSlot.Primary;
            hammerSwingDriver.requiredSkill = hammerSwingDef;

            AISkillDriver minigunSlamDriver = bossMaster.AddComponent<AISkillDriver>();
            minigunSlamDriver.customName = "MinigunSecondary";
            minigunSlamDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            minigunSlamDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            minigunSlamDriver.activationRequiresAimConfirmation = true;
            minigunSlamDriver.activationRequiresTargetLoS = false;
            minigunSlamDriver.selectionRequiresTargetLoS = true;
            minigunSlamDriver.maxDistance = 14f;
            minigunSlamDriver.minDistance = 0f;
            minigunSlamDriver.requireSkillReady = true;
            minigunSlamDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            minigunSlamDriver.ignoreNodeGraph = false;
            minigunSlamDriver.moveInputScale = 1f;
            minigunSlamDriver.driverUpdateTimerOverride = -1f;
            minigunSlamDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            minigunSlamDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            minigunSlamDriver.maxTargetHealthFraction = Mathf.Infinity;
            minigunSlamDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            minigunSlamDriver.maxUserHealthFraction = Mathf.Infinity;
            minigunSlamDriver.skillSlot = SkillSlot.Secondary;
            minigunSlamDriver.requiredSkill = hammerSlamDef;

            AISkillDriver swapToHammerDriver = bossMaster.AddComponent<AISkillDriver>();
            swapToHammerDriver.customName = "SwapToHammer";
            swapToHammerDriver.movementType = AISkillDriver.MovementType.Stop;
            swapToHammerDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            swapToHammerDriver.activationRequiresAimConfirmation = false;
            swapToHammerDriver.activationRequiresTargetLoS = false;
            swapToHammerDriver.selectionRequiresTargetLoS = false;
            swapToHammerDriver.maxDistance = 12f;
            swapToHammerDriver.minDistance = 0f;
            swapToHammerDriver.requireSkillReady = true;
            swapToHammerDriver.aimType = AISkillDriver.AimType.MoveDirection;
            swapToHammerDriver.ignoreNodeGraph = false;
            swapToHammerDriver.moveInputScale = 1f;
            swapToHammerDriver.driverUpdateTimerOverride = 0.5f;
            swapToHammerDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            swapToHammerDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            swapToHammerDriver.maxTargetHealthFraction = Mathf.Infinity;
            swapToHammerDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            swapToHammerDriver.maxUserHealthFraction = Mathf.Infinity;
            swapToHammerDriver.skillSlot = SkillSlot.Special;
            swapToHammerDriver.requiredSkill = minigunUpDef;

            AISkillDriver minigunFireDriver = bossMaster.AddComponent<AISkillDriver>();
            minigunFireDriver.customName = "StrafeAndShoot";
            minigunFireDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            minigunFireDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            minigunFireDriver.activationRequiresAimConfirmation = true;
            minigunFireDriver.activationRequiresTargetLoS = false;
            minigunFireDriver.selectionRequiresTargetLoS = true;
            minigunFireDriver.maxDistance = 80f;
            minigunFireDriver.minDistance = 8f;
            minigunFireDriver.requireSkillReady = true;
            minigunFireDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            minigunFireDriver.ignoreNodeGraph = false;
            minigunFireDriver.moveInputScale = 1f;
            minigunFireDriver.driverUpdateTimerOverride = -1f;
            minigunFireDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            minigunFireDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            minigunFireDriver.maxTargetHealthFraction = Mathf.Infinity;
            minigunFireDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            minigunFireDriver.maxUserHealthFraction = Mathf.Infinity;
            minigunFireDriver.skillSlot = SkillSlot.Primary;
            minigunFireDriver.requiredSkill = minigunFireDef;

            AISkillDriver swapToMinigunDriver = bossMaster.AddComponent<AISkillDriver>();
            swapToMinigunDriver.customName = "SwapToMinigun";
            swapToMinigunDriver.movementType = AISkillDriver.MovementType.Stop;
            swapToMinigunDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            swapToMinigunDriver.activationRequiresAimConfirmation = false;
            swapToMinigunDriver.activationRequiresTargetLoS = false;
            swapToMinigunDriver.selectionRequiresTargetLoS = false;
            swapToMinigunDriver.maxDistance = 90f;
            swapToMinigunDriver.minDistance = 30f;
            swapToMinigunDriver.requireSkillReady = true;
            swapToMinigunDriver.aimType = AISkillDriver.AimType.MoveDirection;
            swapToMinigunDriver.ignoreNodeGraph = false;
            swapToMinigunDriver.moveInputScale = 1f;
            swapToMinigunDriver.driverUpdateTimerOverride = -1f;
            swapToMinigunDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            swapToMinigunDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            swapToMinigunDriver.maxTargetHealthFraction = Mathf.Infinity;
            swapToMinigunDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            swapToMinigunDriver.maxUserHealthFraction = Mathf.Infinity;
            swapToMinigunDriver.skillSlot = SkillSlot.Special;
            swapToMinigunDriver.requiredSkill = minigunDownDef;

            /*AISkillDriver strafeIdleDriver = bossMaster.AddComponent<AISkillDriver>();
            strafeIdleDriver.customName = "StrafeIdle";
            strafeIdleDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            strafeIdleDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            strafeIdleDriver.activationRequiresAimConfirmation = false;
            strafeIdleDriver.activationRequiresTargetLoS = false;
            strafeIdleDriver.selectionRequiresTargetLoS = true;
            strafeIdleDriver.maxDistance = 80f;
            strafeIdleDriver.minDistance = 8f;
            strafeIdleDriver.requireSkillReady = true;
            strafeIdleDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            strafeIdleDriver.ignoreNodeGraph = false;
            strafeIdleDriver.moveInputScale = 1f;
            strafeIdleDriver.driverUpdateTimerOverride = -1f;
            strafeIdleDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            strafeIdleDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            strafeIdleDriver.maxTargetHealthFraction = Mathf.Infinity;
            strafeIdleDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            strafeIdleDriver.maxUserHealthFraction = Mathf.Infinity;
            strafeIdleDriver.skillSlot = SkillSlot.None;
            strafeIdleDriver.requiredSkill = minigunUpDef;*/

            AISkillDriver followDriver = bossMaster.AddComponent<AISkillDriver>();
            followDriver.customName = "Chase";
            followDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            followDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            followDriver.activationRequiresAimConfirmation = false;
            followDriver.activationRequiresTargetLoS = false;
            followDriver.maxDistance = Mathf.Infinity;
            followDriver.minDistance = 0f;
            followDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            followDriver.ignoreNodeGraph = false;
            followDriver.moveInputScale = 1f;
            followDriver.driverUpdateTimerOverride = -1f;
            followDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            followDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            followDriver.maxTargetHealthFraction = Mathf.Infinity;
            followDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            followDriver.maxUserHealthFraction = Mathf.Infinity;
            followDriver.skillSlot = SkillSlot.None;
        }

        private void CreateMiniNemesisAI()
        {
            foreach (AISkillDriver ai in minibossMaster.GetComponentsInChildren<AISkillDriver>())
            {
                BaseUnityPlugin.DestroyImmediate(ai);
            }

            minibossMaster.GetComponent<BaseAI>().fullVision = true;

            /*AISkillDriver grenadeDriver = bossMaster.AddComponent<AISkillDriver>();
            grenadeDriver.customName = "ThrowGrenade";
            grenadeDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            grenadeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            grenadeDriver.activationRequiresAimConfirmation = true;
            grenadeDriver.activationRequiresTargetLoS = false;
            grenadeDriver.selectionRequiresTargetLoS = true;
            grenadeDriver.requireSkillReady = true;
            grenadeDriver.maxDistance = 64f;
            grenadeDriver.minDistance = 0f;
            grenadeDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            grenadeDriver.ignoreNodeGraph = false;
            grenadeDriver.moveInputScale = 1f;
            grenadeDriver.driverUpdateTimerOverride = 0.5f;
            grenadeDriver.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            grenadeDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            grenadeDriver.maxTargetHealthFraction = Mathf.Infinity;
            grenadeDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            grenadeDriver.maxUserHealthFraction = Mathf.Infinity;
            grenadeDriver.skillSlot = SkillSlot.Utility;*/

            AISkillDriver hammerTapDriver = minibossMaster.AddComponent<AISkillDriver>();
            hammerTapDriver.customName = "HammerTap";
            hammerTapDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerTapDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerTapDriver.activationRequiresAimConfirmation = true;
            hammerTapDriver.activationRequiresTargetLoS = false;
            hammerTapDriver.selectionRequiresTargetLoS = true;
            hammerTapDriver.maxDistance = 8f;
            hammerTapDriver.minDistance = 2f;
            hammerTapDriver.requireSkillReady = true;
            hammerTapDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerTapDriver.ignoreNodeGraph = true;
            hammerTapDriver.moveInputScale = 1f;
            hammerTapDriver.driverUpdateTimerOverride = 1.5f;
            hammerTapDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerTapDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerTapDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerTapDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerTapDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerTapDriver.skillSlot = SkillSlot.Secondary;
            hammerTapDriver.requiredSkill = hammerChargeDef;
            hammerTapDriver.noRepeat = true;

            AISkillDriver hammerChargeDriver = minibossMaster.AddComponent<AISkillDriver>();
            hammerChargeDriver.customName = "HammerCharge";
            hammerChargeDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerChargeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerChargeDriver.activationRequiresAimConfirmation = true;
            hammerChargeDriver.activationRequiresTargetLoS = false;
            hammerChargeDriver.selectionRequiresTargetLoS = true;
            hammerChargeDriver.maxDistance = 24f;
            hammerChargeDriver.minDistance = 12f;
            hammerChargeDriver.requireSkillReady = true;
            hammerChargeDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerChargeDriver.ignoreNodeGraph = true;
            hammerChargeDriver.moveInputScale = 1f;
            hammerChargeDriver.driverUpdateTimerOverride = 2.5f;
            hammerChargeDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerChargeDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerChargeDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerChargeDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerChargeDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerChargeDriver.skillSlot = SkillSlot.Secondary;
            hammerChargeDriver.requiredSkill = hammerChargeDef;
            hammerChargeDriver.noRepeat = true;

            AISkillDriver hammerSwingCloseDriver = minibossMaster.AddComponent<AISkillDriver>();
            hammerSwingCloseDriver.customName = "HammerCloseRange";
            hammerSwingCloseDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            hammerSwingCloseDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerSwingCloseDriver.activationRequiresAimConfirmation = true;
            hammerSwingCloseDriver.activationRequiresTargetLoS = false;
            hammerSwingCloseDriver.selectionRequiresTargetLoS = true;
            hammerSwingCloseDriver.maxDistance = 3f;
            hammerSwingCloseDriver.minDistance = 0f;
            hammerSwingCloseDriver.requireSkillReady = true;
            hammerSwingCloseDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerSwingCloseDriver.ignoreNodeGraph = true;
            hammerSwingCloseDriver.moveInputScale = 0.4f;
            hammerSwingCloseDriver.driverUpdateTimerOverride = 0.5f;
            hammerSwingCloseDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerSwingCloseDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerSwingCloseDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerSwingCloseDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerSwingCloseDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerSwingCloseDriver.skillSlot = SkillSlot.Primary;
            hammerSwingCloseDriver.requiredSkill = hammerSwingDef;

            AISkillDriver hammerSwingDriver = minibossMaster.AddComponent<AISkillDriver>();
            hammerSwingDriver.customName = "WalkAndHammer";
            hammerSwingDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerSwingDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerSwingDriver.activationRequiresAimConfirmation = true;
            hammerSwingDriver.activationRequiresTargetLoS = false;
            hammerSwingDriver.selectionRequiresTargetLoS = true;
            hammerSwingDriver.maxDistance = 12f;
            hammerSwingDriver.minDistance = 0f;
            hammerSwingDriver.requireSkillReady = true;
            hammerSwingDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerSwingDriver.ignoreNodeGraph = true;
            hammerSwingDriver.moveInputScale = 1f;
            hammerSwingDriver.driverUpdateTimerOverride = 0.5f;
            hammerSwingDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerSwingDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerSwingDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerSwingDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerSwingDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerSwingDriver.skillSlot = SkillSlot.Primary;
            hammerSwingDriver.requiredSkill = hammerSwingDef;

            AISkillDriver minigunSlamDriver = minibossMaster.AddComponent<AISkillDriver>();
            minigunSlamDriver.customName = "MinigunSecondary";
            minigunSlamDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            minigunSlamDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            minigunSlamDriver.activationRequiresAimConfirmation = true;
            minigunSlamDriver.activationRequiresTargetLoS = false;
            minigunSlamDriver.selectionRequiresTargetLoS = true;
            minigunSlamDriver.maxDistance = 14f;
            minigunSlamDriver.minDistance = 0f;
            minigunSlamDriver.requireSkillReady = true;
            minigunSlamDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            minigunSlamDriver.ignoreNodeGraph = false;
            minigunSlamDriver.moveInputScale = 1f;
            minigunSlamDriver.driverUpdateTimerOverride = -1f;
            minigunSlamDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            minigunSlamDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            minigunSlamDriver.maxTargetHealthFraction = Mathf.Infinity;
            minigunSlamDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            minigunSlamDriver.maxUserHealthFraction = Mathf.Infinity;
            minigunSlamDriver.skillSlot = SkillSlot.Secondary;
            minigunSlamDriver.requiredSkill = hammerSlamDef;

            AISkillDriver swapToHammerDriver = minibossMaster.AddComponent<AISkillDriver>();
            swapToHammerDriver.customName = "SwapToHammer";
            swapToHammerDriver.movementType = AISkillDriver.MovementType.Stop;
            swapToHammerDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            swapToHammerDriver.activationRequiresAimConfirmation = false;
            swapToHammerDriver.activationRequiresTargetLoS = false;
            swapToHammerDriver.selectionRequiresTargetLoS = false;
            swapToHammerDriver.maxDistance = 12f;
            swapToHammerDriver.minDistance = 0f;
            swapToHammerDriver.requireSkillReady = true;
            swapToHammerDriver.aimType = AISkillDriver.AimType.MoveDirection;
            swapToHammerDriver.ignoreNodeGraph = false;
            swapToHammerDriver.moveInputScale = 1f;
            swapToHammerDriver.driverUpdateTimerOverride = 0.5f;
            swapToHammerDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            swapToHammerDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            swapToHammerDriver.maxTargetHealthFraction = Mathf.Infinity;
            swapToHammerDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            swapToHammerDriver.maxUserHealthFraction = Mathf.Infinity;
            swapToHammerDriver.skillSlot = SkillSlot.Special;
            swapToHammerDriver.requiredSkill = minigunUpDef;

            AISkillDriver minigunFireDriver = minibossMaster.AddComponent<AISkillDriver>();
            minigunFireDriver.customName = "StrafeAndShoot";
            minigunFireDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            minigunFireDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            minigunFireDriver.activationRequiresAimConfirmation = true;
            minigunFireDriver.activationRequiresTargetLoS = false;
            minigunFireDriver.selectionRequiresTargetLoS = true;
            minigunFireDriver.maxDistance = 80f;
            minigunFireDriver.minDistance = 8f;
            minigunFireDriver.requireSkillReady = true;
            minigunFireDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            minigunFireDriver.ignoreNodeGraph = false;
            minigunFireDriver.moveInputScale = 1f;
            minigunFireDriver.driverUpdateTimerOverride = -1f;
            minigunFireDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            minigunFireDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            minigunFireDriver.maxTargetHealthFraction = Mathf.Infinity;
            minigunFireDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            minigunFireDriver.maxUserHealthFraction = Mathf.Infinity;
            minigunFireDriver.skillSlot = SkillSlot.Primary;
            minigunFireDriver.requiredSkill = minigunFireDef;

            AISkillDriver swapToMinigunDriver = minibossMaster.AddComponent<AISkillDriver>();
            swapToMinigunDriver.customName = "SwapToMinigun";
            swapToMinigunDriver.movementType = AISkillDriver.MovementType.Stop;
            swapToMinigunDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            swapToMinigunDriver.activationRequiresAimConfirmation = false;
            swapToMinigunDriver.activationRequiresTargetLoS = false;
            swapToMinigunDriver.selectionRequiresTargetLoS = false;
            swapToMinigunDriver.maxDistance = 90f;
            swapToMinigunDriver.minDistance = 30f;
            swapToMinigunDriver.requireSkillReady = true;
            swapToMinigunDriver.aimType = AISkillDriver.AimType.MoveDirection;
            swapToMinigunDriver.ignoreNodeGraph = false;
            swapToMinigunDriver.moveInputScale = 1f;
            swapToMinigunDriver.driverUpdateTimerOverride = -1f;
            swapToMinigunDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            swapToMinigunDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            swapToMinigunDriver.maxTargetHealthFraction = Mathf.Infinity;
            swapToMinigunDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            swapToMinigunDriver.maxUserHealthFraction = Mathf.Infinity;
            swapToMinigunDriver.skillSlot = SkillSlot.Special;
            swapToMinigunDriver.requiredSkill = minigunDownDef;

            /*AISkillDriver strafeIdleDriver = bossMaster.AddComponent<AISkillDriver>();
            strafeIdleDriver.customName = "StrafeIdle";
            strafeIdleDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            strafeIdleDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            strafeIdleDriver.activationRequiresAimConfirmation = false;
            strafeIdleDriver.activationRequiresTargetLoS = false;
            strafeIdleDriver.selectionRequiresTargetLoS = true;
            strafeIdleDriver.maxDistance = 80f;
            strafeIdleDriver.minDistance = 8f;
            strafeIdleDriver.requireSkillReady = true;
            strafeIdleDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            strafeIdleDriver.ignoreNodeGraph = false;
            strafeIdleDriver.moveInputScale = 1f;
            strafeIdleDriver.driverUpdateTimerOverride = -1f;
            strafeIdleDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            strafeIdleDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            strafeIdleDriver.maxTargetHealthFraction = Mathf.Infinity;
            strafeIdleDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            strafeIdleDriver.maxUserHealthFraction = Mathf.Infinity;
            strafeIdleDriver.skillSlot = SkillSlot.None;
            strafeIdleDriver.requiredSkill = minigunUpDef;*/

            AISkillDriver followDriver = minibossMaster.AddComponent<AISkillDriver>();
            followDriver.customName = "Chase";
            followDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            followDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            followDriver.activationRequiresAimConfirmation = false;
            followDriver.activationRequiresTargetLoS = false;
            followDriver.maxDistance = Mathf.Infinity;
            followDriver.minDistance = 0f;
            followDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            followDriver.ignoreNodeGraph = false;
            followDriver.moveInputScale = 1f;
            followDriver.driverUpdateTimerOverride = -1f;
            followDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            followDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            followDriver.maxTargetHealthFraction = Mathf.Infinity;
            followDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            followDriver.maxUserHealthFraction = Mathf.Infinity;
            followDriver.skillSlot = SkillSlot.None;
        }

        private static void CreateDededePrefab()
        {
            dededePrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "KingDededeBody");

            dededePrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            GameObject model = CreateModel(dededePrefab, "mdlNemforcer");

            GameObject gameObject = new GameObject("ModelBase");
            gameObject.transform.parent = dededePrefab.transform;
            gameObject.transform.localPosition = new Vector3(0f, -0.92f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject2 = new GameObject("CameraPivot");
            gameObject2.transform.parent = gameObject.transform;
            gameObject2.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            gameObject2.transform.localRotation = Quaternion.identity;
            gameObject2.transform.localScale = Vector3.one;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = gameObject.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = dededePrefab.GetComponent<CharacterDirection>();
            characterDirection.moveVector = Vector3.zero;
            characterDirection.targetTransform = gameObject.transform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            CharacterBody bodyComponent = dededePrefab.GetComponent<CharacterBody>();
            bodyComponent.name = "KingDededeBody";
            bodyComponent.baseNameToken = "DEDEDE_NAME";
            bodyComponent.subtitleNameToken = "DEDEDE_SUBTITLE";
            bodyComponent.baseAcceleration = 80;
            bodyComponent.baseJumpCount = 1;
            bodyComponent.sprintingSpeedMultiplier = 1.45f;
            bodyComponent.wasLucky = false;
            bodyComponent.hideCrosshair = false;
            bodyComponent.crosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/SimpleDotCrosshair");
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.nemCharPortrait;
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.skinIndex = 0U;
            bodyComponent.preferredPodPrefab = null;

            var stateMachine = bodyComponent.GetComponent<EntityStateMachine>();
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Nemforcer.NemforcerMain));
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.Bison.SpawnState));

            CharacterMotor characterMotor = dededePrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 2000f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;

            CameraTargetParams cameraTargetParams = dededePrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();
            cameraTargetParams.cameraParams.maxPitch = 70;
            cameraTargetParams.cameraParams.minPitch = -70;
            cameraTargetParams.cameraParams.wallCushion = 0.1f;
            cameraTargetParams.cameraParams.pivotVerticalOffset = 1.37f;
            cameraTargetParams.cameraParams.standardLocalCameraPos = new Vector3(0, 0.5f, -12);

            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.idealLocalCameraPos = Vector3.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            ModelLocator modelLocator = dededePrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = gameObject.transform;

            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = Assets.CreateNemMaterial("matDedede"),
                    renderer = childLocator.FindChild("HammerModel").GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("AltHammer").GetComponentInChildren<MeshRenderer>().material,
                    renderer = childLocator.FindChild("AltHammer").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = true
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("GrenadeL").GetComponentInChildren<MeshRenderer>().material,
                    renderer = childLocator.FindChild("GrenadeL").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = true
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("GrenadeR").GetComponentInChildren<MeshRenderer>().material,
                    renderer = childLocator.FindChild("GrenadeR").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = true
                },
                //keep body last for teleporter particles
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = Assets.CreateNemMaterial("matDedede"),
                    renderer = childLocator.FindChild("Model").GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };

            childLocator.FindChild("Model").GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = Assets.dededeBossMesh;
            childLocator.FindChild("HammerModel").GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = Assets.dededeHammerMesh;

            Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/hgstandard");

            foreach (CharacterModel.RendererInfo i in characterModel.baseRendererInfos)
            {
                if (i.defaultMaterial) i.defaultMaterial.shader = hotpoo;
            }

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

            characterModel.mainSkinnedMeshRenderer = characterModel.baseRendererInfos[characterModel.baseRendererInfos.Length - 1].renderer.gameObject.GetComponent<SkinnedMeshRenderer>();

            TeamComponent teamComponent = null;
            if (dededePrefab.GetComponent<TeamComponent>() != null) teamComponent = dededePrefab.GetComponent<TeamComponent>();
            else teamComponent = dededePrefab.GetComponent<TeamComponent>();
            teamComponent.hideAllyCardDisplay = false;
            teamComponent.teamIndex = TeamIndex.None;

            dededePrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
            dededePrefab.GetComponent<InteractionDriver>().highlightInteractor = true;

            CharacterDeathBehavior characterDeathBehavior = dededePrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = dededePrefab.GetComponent<EntityStateMachine>();
            //characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            SfxLocator sfxLocator = dededePrefab.GetComponent<SfxLocator>();
            sfxLocator.deathSound = "Play_bison_death";
            sfxLocator.barkSound = "";
            sfxLocator.openSound = "";
            sfxLocator.landingSound = "Play_char_land";
            sfxLocator.fallDamageSound = "";
            sfxLocator.aliveLoopStart = "";
            sfxLocator.aliveLoopStop = "";

            Rigidbody rigidbody = dededePrefab.GetComponent<Rigidbody>();
            rigidbody.mass = 2000f;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;

            CapsuleCollider capsuleCollider = dededePrefab.GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = false;
            capsuleCollider.material = null;
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;

            KinematicCharacterMotor kinematicCharacterMotor = dededePrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            kinematicCharacterMotor.Capsule = capsuleCollider;
            kinematicCharacterMotor.Rigidbody = rigidbody;

            kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 0.2f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;
            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;
            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = Vector3.up;
            kinematicCharacterMotor.StepHandling = StepHandlingMethod.None;
            kinematicCharacterMotor.LedgeHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.SafeMovement = false;

            HealthComponent healthComponent = dededePrefab.GetComponent<HealthComponent>();

            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();

            HurtBox mainHurtbox = model.transform.Find("MainHurtbox").GetComponent<CapsuleCollider>().gameObject.AddComponent<HurtBox>();
            mainHurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
            mainHurtbox.healthComponent = healthComponent;
            mainHurtbox.isBullseye = true;
            mainHurtbox.damageModifier = HurtBox.DamageModifier.Normal;
            mainHurtbox.hurtBoxGroup = hurtBoxGroup;
            mainHurtbox.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                mainHurtbox
            };

            hurtBoxGroup.mainHurtBox = mainHurtbox;
            hurtBoxGroup.bullseyeCount = 1;

            HitBoxGroup hitBoxGroup = model.AddComponent<HitBoxGroup>();
            #region legacyHitboxes
            ////make a hitbox for hammer (old)
            //GameObject hammerHitbox = childLocator.FindChild("HammerHitbox").gameObject;
            //hammerHitbox.transform.localScale = new Vector3(0.155f, 0.17f, 0.08f);
            //hammerHitbox.transform.localPosition = Vector3.up * 0.02f;
            //hammerHitbox.layer = LayerIndex.projectile.intVal;

            //HitBox hitBox0 = hammerHitbox.AddComponent<HitBox>();

            //hitBoxGroup.hitBoxes = new HitBox[]
            //{
            //    hitBox0,
            //};

            ////make hitboxes for hammer (old)
            //GameObject hammerHitbox1 = childLocator.FindChild("HammerHitboxHead").gameObject;
            //hammerHitbox1.transform.localScale = new Vector3(0.155f, 0.102f, 0.08f);
            //hammerHitbox1.transform.localPosition = Vector3.up * 0.0518f;
            //hammerHitbox1.layer = LayerIndex.projectile.intVal;

            //GameObject hammerHitbox2 = childLocator.FindChild("HammerHitboxShaft").gameObject;
            //hammerHitbox2.transform.localScale = new Vector3(0.155f, 0.102f, 0.043f);
            //hammerHitbox2.transform.localPosition = Vector3.up * -0.0144f;
            //hammerHitbox2.layer = LayerIndex.projectile.intVal;

            //HitBox hitBox1 = hammerHitbox1.AddComponent<HitBox>();
            //HitBox hitBox11 = hammerHitbox2.AddComponent<HitBox>();

            //hitBoxGroup.hitBoxes = new HitBox[]
            //{
            //    hitBox1,
            //    hitBox11,
            //};
            #endregion

            //make hitboxes for hammer (old)
            GameObject hammerHitbox1 = childLocator.FindChild("HammerHitboxFront").gameObject;
            hammerHitbox1.layer = LayerIndex.projectile.intVal;

            GameObject hammerHitbox2 = childLocator.FindChild("HammerHitboxBack").gameObject;
            hammerHitbox2.layer = LayerIndex.projectile.intVal;

            HitBox hitBox1 = hammerHitbox1.AddComponent<HitBox>();
            HitBox hitBox11 = hammerHitbox2.AddComponent<HitBox>();

            hitBoxGroup.hitBoxes = new HitBox[]
            {
                hitBox1,
                hitBox11,
            };
            hitBoxGroup.groupName = "Hammer";

            //uppercut hitbox

            HitBoxGroup hitBoxGroup2 = model.AddComponent<HitBoxGroup>();

            GameObject uppercutHitbox = childLocator.FindChild("UppercutHitbox").gameObject;
            uppercutHitbox.transform.localScale = Vector3.one * 10f;
            uppercutHitbox.transform.localPosition = new Vector3(0, 2.2f, 2.32f);

            HitBox hitBox2 = uppercutHitbox.AddComponent<HitBox>();
            uppercutHitbox.layer = LayerIndex.projectile.intVal;

            hitBoxGroup2.hitBoxes = new HitBox[]
            {
                hitBox2
            };

            hitBoxGroup2.groupName = "Uppercut";

            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_scav_step";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericHugeFootstepDust");

            RagdollController ragdollController = model.GetComponent<RagdollController>();

            PhysicMaterial physicMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RagdollController>().bones[1].GetComponent<Collider>().material;

            foreach (Transform i in ragdollController.bones)
            {
                if (i)
                {
                    i.gameObject.layer = LayerIndex.ragdoll.intVal;
                    Collider j = i.GetComponent<Collider>();
                    if (j)
                    {
                        j.material = physicMat;
                        j.sharedMaterial = physicMat;
                    }
                }
            }

            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMax = 60f;
            aimAnimator.pitchRangeMin = -60f;
            aimAnimator.yawRangeMin = -90f;
            aimAnimator.yawRangeMax = 90f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 3f;
            aimAnimator.inputBank = dededePrefab.GetComponent<InputBankTest>();

            dededePrefab.AddComponent<NemforcerController>();
            dededePrefab.AddComponent<DeathRewards>();
        }

        private void CreateDededeBoss()
        {
            CreateDededePrefab();

            dededePrefab.GetComponent<ModelLocator>().modelBaseTransform.localScale *= 2f;

            EnforcerModPlugin.Destroy(dededePrefab.GetComponentInChildren<ModelSkinController>());

            CharacterBody charBody = dededePrefab.GetComponent<CharacterBody>();

            LanguageAPI.Add("DEDEDE_NAME", "King Dedede");
            LanguageAPI.Add("DEDEDE_BOSS_SUBTITLE", "King of Dreamland");

            charBody.name = "KingDededeBody";
            charBody.baseNameToken = "DEDEDE_NAME";
            charBody.subtitleNameToken = "DEDEDE_BOSS_SUBTITLE";
            charBody.bodyFlags = CharacterBody.BodyFlags.None;
            charBody.rootMotionInMainState = false;
            charBody.mainRootSpeed = 0;
            charBody.baseMaxHealth = 2800;
            charBody.levelMaxHealth = 840;
            charBody.baseRegen = 0f;
            charBody.levelRegen = 0f;
            charBody.baseMaxShield = 0;
            charBody.levelMaxShield = 0;
            charBody.baseMoveSpeed = 8;
            charBody.levelMoveSpeed = 0;
            charBody.baseAcceleration = 80;
            charBody.baseJumpPower = 15;
            charBody.levelJumpPower = 0;
            charBody.baseDamage = 8;
            charBody.levelDamage = charBody.baseDamage * 0.2f;
            charBody.baseAttackSpeed = 1;
            charBody.levelAttackSpeed = 0;
            charBody.baseCrit = 0;
            charBody.levelCrit = 0;
            charBody.baseArmor = 20;
            charBody.levelArmor = 0;
            charBody.baseJumpCount = 6;
            charBody.portraitIcon = Assets.MainAssetBundle.LoadAsset<Sprite>("texDededeIcon").texture;
            charBody.isChampion = true;
            charBody.skinIndex = 0U;
            charBody.bodyColor = Color.red;

            foreach (GenericSkill obj in dededePrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }

            SkillLocator skillLocator = dededePrefab.GetComponent<SkillLocator>();

            skillLocator.primary = dededePrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            Modules.States.AddSkillFamily(newFamily);
            skillLocator.primary._skillFamily = newFamily;
            SkillFamily skillFamily = skillLocator.primary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = hammerSwingDef,
                viewableNode = new ViewablesCatalog.Node(hammerSwingDef.skillNameToken, false, null)
            };

            skillLocator.secondary = dededePrefab.AddComponent<GenericSkill>();
            newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            Modules.States.AddSkillFamily(newFamily);
            skillLocator.secondary._skillFamily = newFamily;
            skillFamily = skillLocator.secondary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = hammerChargeDef,
                viewableNode = new ViewablesCatalog.Node(hammerChargeDef.skillNameToken, false, null)
            };

            skillLocator.utility = dededePrefab.AddComponent<GenericSkill>();
            newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            Modules.States.AddSkillFamily(newFamily);
            skillLocator.utility._skillFamily = newFamily;
            skillFamily = skillLocator.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = jumpDef,
                viewableNode = new ViewablesCatalog.Node(jumpDef.skillNameToken, false, null)
            };

            EnforcerModPlugin.bodyPrefabs.Add(dededePrefab);

            dededeMaster = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/LemurianMaster"), "KingDededeMaster", true);
            dededeMaster.GetComponent<CharacterMaster>().bodyPrefab = dededePrefab;

            CreateDededeAI();

            EnforcerModPlugin.masterPrefabs.Add(dededeMaster);

            CreateDededeSpawnCard();
        }

        private void CreateDededeSpawnCard()
        {
            CharacterSpawnCard characterSpawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
            characterSpawnCard.name = "cscDedede";
            characterSpawnCard.prefab = dededeMaster;
            characterSpawnCard.sendOverNetwork = true;
            characterSpawnCard.hullSize = HullClassification.BeetleQueen;
            characterSpawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
            characterSpawnCard.requiredFlags = NodeFlags.None;
            characterSpawnCard.forbiddenFlags = NodeFlags.TeleporterOK;
            characterSpawnCard.directorCreditCost = 2000;
            characterSpawnCard.occupyPosition = false;
            characterSpawnCard.loadout = new SerializableLoadout();
            characterSpawnCard.noElites = false;
            characterSpawnCard.forbiddenAsBoss = false;

            DirectorCard card = new DirectorCard
            {
                spawnCard = characterSpawnCard,
                selectionWeight = 1,
                allowAmbushSpawn = false,
                preventOverhead = false,
                minimumStageCompletions = 3,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Close
            };

            DirectorAPI.DirectorCardHolder dededeCard = new DirectorAPI.DirectorCardHolder
            {
                Card = card,
                MonsterCategory = DirectorAPI.MonsterCategory.Champions,
                InteractableCategory = DirectorAPI.InteractableCategory.None
            };

            DirectorAPI.MonsterActions += delegate (List<DirectorAPI.DirectorCardHolder> list, DirectorAPI.StageInfo stage)
            {
                if (stage.stage == DirectorAPI.Stage.SkyMeadow || stage.stage == DirectorAPI.Stage.GildedCoast || stage.stage == DirectorAPI.Stage.TitanicPlains || stage.stage == DirectorAPI.Stage.VoidCell)
                {
                    if (!list.Contains(dededeCard))
                    {
                        list.Add(dededeCard);
                    }
                }
            };
        }

        private void CreateDededeAI()
        {
            foreach (AISkillDriver ai in dededeMaster.GetComponentsInChildren<AISkillDriver>())
            {
                BaseUnityPlugin.DestroyImmediate(ai);
            }

            dededeMaster.GetComponent<BaseAI>().fullVision = true;

            AISkillDriver slamDriver = dededeMaster.AddComponent<AISkillDriver>();
            slamDriver.customName = "Slam";
            slamDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            slamDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            slamDriver.activationRequiresAimConfirmation = true;
            slamDriver.activationRequiresTargetLoS = false;
            slamDriver.selectionRequiresTargetLoS = true;
            slamDriver.requireSkillReady = true;
            slamDriver.maxDistance = 12f;
            slamDriver.minDistance = 0f;
            slamDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            slamDriver.ignoreNodeGraph = false;
            slamDriver.moveInputScale = 1f;
            slamDriver.driverUpdateTimerOverride = 0.5f;
            slamDriver.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
            slamDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            slamDriver.maxTargetHealthFraction = Mathf.Infinity;
            slamDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            slamDriver.maxUserHealthFraction = 0.5f;
            slamDriver.skillSlot = SkillSlot.Utility;

            AISkillDriver hammerTapDriver = dededeMaster.AddComponent<AISkillDriver>();
            hammerTapDriver.customName = "HammerTap";
            hammerTapDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerTapDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerTapDriver.activationRequiresAimConfirmation = true;
            hammerTapDriver.activationRequiresTargetLoS = false;
            hammerTapDriver.selectionRequiresTargetLoS = true;
            hammerTapDriver.maxDistance = 8f;
            hammerTapDriver.minDistance = 2f;
            hammerTapDriver.requireSkillReady = true;
            hammerTapDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerTapDriver.ignoreNodeGraph = true;
            hammerTapDriver.moveInputScale = 1f;
            hammerTapDriver.driverUpdateTimerOverride = 1.5f;
            hammerTapDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerTapDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerTapDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerTapDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerTapDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerTapDriver.skillSlot = SkillSlot.Secondary;
            hammerTapDriver.noRepeat = true;

            AISkillDriver hammerChargeDriver = dededeMaster.AddComponent<AISkillDriver>();
            hammerChargeDriver.customName = "HammerCharge";
            hammerChargeDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerChargeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerChargeDriver.activationRequiresAimConfirmation = true;
            hammerChargeDriver.activationRequiresTargetLoS = false;
            hammerChargeDriver.selectionRequiresTargetLoS = true;
            hammerChargeDriver.maxDistance = 24f;
            hammerChargeDriver.minDistance = 12f;
            hammerChargeDriver.requireSkillReady = true;
            hammerChargeDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerChargeDriver.ignoreNodeGraph = true;
            hammerChargeDriver.moveInputScale = 1f;
            hammerChargeDriver.driverUpdateTimerOverride = 2.5f;
            hammerChargeDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerChargeDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerChargeDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerChargeDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerChargeDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerChargeDriver.skillSlot = SkillSlot.Secondary;
            hammerChargeDriver.noRepeat = true;

            AISkillDriver hammerSwingCloseDriver = dededeMaster.AddComponent<AISkillDriver>();
            hammerSwingCloseDriver.customName = "HammerCloseRange";
            hammerSwingCloseDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            hammerSwingCloseDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerSwingCloseDriver.activationRequiresAimConfirmation = true;
            hammerSwingCloseDriver.activationRequiresTargetLoS = false;
            hammerSwingCloseDriver.selectionRequiresTargetLoS = true;
            hammerSwingCloseDriver.maxDistance = 3f;
            hammerSwingCloseDriver.minDistance = 0f;
            hammerSwingCloseDriver.requireSkillReady = true;
            hammerSwingCloseDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerSwingCloseDriver.ignoreNodeGraph = true;
            hammerSwingCloseDriver.moveInputScale = 0.4f;
            hammerSwingCloseDriver.driverUpdateTimerOverride = 0.5f;
            hammerSwingCloseDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerSwingCloseDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerSwingCloseDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerSwingCloseDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerSwingCloseDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerSwingCloseDriver.skillSlot = SkillSlot.Primary;

            AISkillDriver hammerSwingDriver = dededeMaster.AddComponent<AISkillDriver>();
            hammerSwingDriver.customName = "WalkAndHammer";
            hammerSwingDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hammerSwingDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            hammerSwingDriver.activationRequiresAimConfirmation = true;
            hammerSwingDriver.activationRequiresTargetLoS = false;
            hammerSwingDriver.selectionRequiresTargetLoS = true;
            hammerSwingDriver.maxDistance = 12f;
            hammerSwingDriver.minDistance = 0f;
            hammerSwingDriver.requireSkillReady = true;
            hammerSwingDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            hammerSwingDriver.ignoreNodeGraph = true;
            hammerSwingDriver.moveInputScale = 1f;
            hammerSwingDriver.driverUpdateTimerOverride = 0.5f;
            hammerSwingDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hammerSwingDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            hammerSwingDriver.maxTargetHealthFraction = Mathf.Infinity;
            hammerSwingDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            hammerSwingDriver.maxUserHealthFraction = Mathf.Infinity;
            hammerSwingDriver.skillSlot = SkillSlot.Primary;

            AISkillDriver followDriver = dededeMaster.AddComponent<AISkillDriver>();
            followDriver.customName = "Chase";
            followDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            followDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            followDriver.activationRequiresAimConfirmation = false;
            followDriver.activationRequiresTargetLoS = false;
            followDriver.maxDistance = Mathf.Infinity;
            followDriver.minDistance = 0f;
            followDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            followDriver.ignoreNodeGraph = false;
            followDriver.moveInputScale = 1f;
            followDriver.driverUpdateTimerOverride = -1f;
            followDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            followDriver.minTargetHealthFraction = Mathf.NegativeInfinity;
            followDriver.maxTargetHealthFraction = Mathf.Infinity;
            followDriver.minUserHealthFraction = Mathf.NegativeInfinity;
            followDriver.maxUserHealthFraction = Mathf.Infinity;
            followDriver.skillSlot = SkillSlot.None;
        }
    }
}