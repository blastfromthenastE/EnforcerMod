using EnforcerPlugin;
using EntityStates.Enforcer;
using EntityStates.Enforcer.NeutralSpecial;
using EntityStates.Nemforcer;
using R2API;
using System;

namespace Modules {
    internal static class LanguageAPI {
        public static void Add(string token, string text) {
            //R2API.LanguageAPI.Add(token, text);
            Languages.Add(token, text);
        }
    }

    internal class Tokens {

        public static void GenerateLanguageTokens() {
            //in case ruin's new language system doesn't work
            //which I think it doesn't
            //rip
            RegisterEnforcerTokens();
            Languages.PrintOutput("Enforcer.txt");

            RegisterNemforcerTokens();
            Languages.PrintOutput("Nemforcer.txt");
        }

        private static void RegisterEnforcerTokens() {
            
            #region Enforcer

            const string characterName = "Enforcer";
            const string characterSubtitle = "Unwavering Bastion";
            const string characterOutro = "..and so he left, unsure of his title as protector.";
            const string characterOutroFailure = "..and so he vanished, the planet's minorities finally at peace.";
            const string characterLore = "\n<style=cMono>\"You don't have to do this.\"</style>\r\n\r\nThe words echoed in his head, yet he pushed forward. The pod was only a few steps away — he had a chance to leave — but something in his core kept him moving. He didn't know what it was, but he didn't question it. It was a natural force: the same force that always drove him to follow orders.\n\nThis time, however, it didn't seem so natural. There were no orders. The heavy trigger and its rhythmic thunder were his — and his alone.";

            string outro = characterOutro;
            if (Modules.Config.forceUnlock.Value) outro = "..and so he left, having cheated not only the game, but himself. He didn't grow. He didn't improve. He took a shortcut and gained nothing. He experienced a hollow victory. Nothing was risked and nothing was rained.";

            string desc = "The Enforcer is a defensive juggernaut who can take and give a beating.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
                 + "< ! > Riot Shotgun can pierce through many enemies at once." + Environment.NewLine + Environment.NewLine
                 + "< ! > Batting away enemies with Shield Bash guarantees you will keep enemies at a safe range." + Environment.NewLine + Environment.NewLine
                 + "< ! > Use Tear Gas to weaken large crowds of enemies, then get in close and crush them." + Environment.NewLine + Environment.NewLine
                 + "< ! > When you can, use Protect and Serve against walls to prevent enemies from flanking you." + Environment.NewLine + Environment.NewLine;

            LanguageAPI.Add("ENFORCER_NAME", characterName);
            LanguageAPI.Add("ENFORCER_DESCRIPTION", desc);
            LanguageAPI.Add("ENFORCER_SUBTITLE", characterSubtitle);
            //LanguageAPI.Add("ENFORCER_LORE", "I'M FUCKING INVINCIBLE");
            LanguageAPI.Add("ENFORCER_LORE", characterLore);
            LanguageAPI.Add("ENFORCER_OUTRO_FLAVOR", outro);
            LanguageAPI.Add("ENFORCER_OUTRO_FAILURE", characterOutroFailure);

            #endregion Enforcer

            #region Skills

            #region Primary
            //riot
            string riotDesc = "Fire a short-range blast that <style=cIsUtility>pierces</style> for <style=cIsDamage>" + Config.shotgunBulletCount.Value + "x" + 100f * Modules.Config.shotgunDamage.Value + "% damage.</style>";

            LanguageAPI.Add("ENFORCER_PRIMARY_SHOTGUN_NAME", "Riot Shotgun");
            LanguageAPI.Add("ENFORCER_PRIMARY_SHOTGUN_DESCRIPTION", riotDesc);

            //super
            string superDesc = "Fire up to 2 shotgun blasts for <style=cIsDamage>" + SuperShotgun2.bulletCount / 2 + "x" + 100f * Config.superDamage.Value + "% damage</style>.\nWhile using <style=cIsUtility>Protect and Serve</style>, fire <style=cIsDamage>both barrels at once.</style>";

            LanguageAPI.Add("ENFORCER_PRIMARY_SUPERSHOTGUN_NAME", "Super Shotgun");
            LanguageAPI.Add("ENFORCER_PRIMARY_SUPERSHOTGUN_DESCRIPTION", superDesc);

            //ar
            string arDamage = $"<style=cIsDamage>{100f * FireMachineGun.damageCoefficient}% damage</style>";

            string arDesc = $"Unload a barrage of bullets for {arDamage}.\nWhile using <style=cIsUtility>Protect and Serve</style>, has <style=cIsDamage>increased accuracy</style>, but <style=cIsHealth>slower movement speed</style>.";

            LanguageAPI.Add("ENFORCER_PRIMARY_RIFLE_NAME", "Heavy Machine Gun");
            LanguageAPI.Add("ENFORCER_PRIMARY_RIFLE_DESCRIPTION", arDesc);

            //Hammer
            string damage = $"<style=cIsDamage>{ 100f * HammerSwing.damageCoefficient}% damage</style>";
            string shieldDamage = $"<style=cIsDamage>{ 100f * HammerSwing.shieldDamageCoefficient}% damage</style>";
            string hamDesc = $"Swing your hammer for {damage}.\nWhile using Protect and Serve, swing in a <style=cIsUtility>larger area</style>, for {shieldDamage} instead.";

            LanguageAPI.Add("ENFORCER_PRIMARY_HAMMER_NAME", "Breaching Hammer");
            LanguageAPI.Add("ENFORCER_PRIMARY_HAMMER_DESCRIPTION", hamDesc);
            #endregion Primary

            #region Secondary
            LanguageAPI.Add("KEYWORD_BASH", "<style=cKeywordName>Bash</style><style=cSub>Applies <style=cIsDamage>stun</style> and <style=cIsUtility>heavy knockback</style>.");

            LanguageAPI.Add("KEYWORD_SPRINTBASH", $"<style=cKeywordName>Shoulder Bash</style><style=cSub><style=cIsDamage>Stunning.</style> A short charge that deals <style=cIsDamage>{100f * ShoulderBash.chargeDamageCoefficient}% damage</style>.\nHitting <style=cIsDamage>heavier enemies</style> deals <style=cIsDamage>{ShoulderBash.knockbackDamageCoefficient * 100f}% damage</style>.");

            //string desc = $"<style=cIsDamage>Bash</style> nearby enemies for <style=cIsDamage>{100f * ShieldBash.damageCoefficient}% damage</style>. <style=cIsUtility>Deflects projectiles</style>. Use while <style=cIsUtility>sprinting</style> to perform a <style=cIsDamage>Shoulder Bash</style> for <style=cIsDamage>{100f * ShoulderBash.chargeDamageCoefficient}-{100f * ShoulderBash.knockbackDamageCoefficient}% damage</style> instead.";
            string shieldBashDesc = $"<style=cIsDamage>Stunning</style>. Knock back enemies for <style=cIsDamage>{100f * ShieldBash.damageCoefficient}% damage</style> and <style=cIsUtility>deflect projectiles</style>.";
            shieldBashDesc += $"\nWhile <style=cIsUtility>sprinting</style>, perform a <style=cIsDamage>Shoulder Bash</style> instead.";

            LanguageAPI.Add("ENFORCER_SECONDARY_BASH_NAME", "Shield Bash");
            LanguageAPI.Add("ENFORCER_SECONDARY_BASH_DESCRIPTION", shieldBashDesc);
            #endregion Secondary

            #region Utility
            //gas
            LanguageAPI.Add("KEYWORD_BLINDED", "<style=cKeywordName>Impaired</style><style=cSub>Reduce movement speed by <style=cIsDamage>75%</style>, attack speed by <style=cIsDamage>25%</style> and armor by <style=cIsDamage>30</style>.</style></style>");

            LanguageAPI.Add("ENFORCER_UTILITY_TEARGAS_NAME", "Tear Gas");
            LanguageAPI.Add("ENFORCER_UTILITY_TEARGAS_DESCRIPTION", "Toss a grenade that covers an area in <style=cIsDamage>Impairing</style> gas.");

            //stun
            LanguageAPI.Add("ENFORCER_UTILITY_STUNGRENADE_NAME", "Stun Grenade");
            LanguageAPI.Add("ENFORCER_UTILITY_STUNGRENADE_DESCRIPTION", "<style=cIsDamage>Stunning</style>. Launch a grenade that concusses enemies for <style=cIsDamage>" + 100f * StunGrenade.damageCoefficient + "% damage</style>. Hold up to 3.");

            //gas scepter
            LanguageAPI.Add("ENFORCER_UTILITY_TEARGASSCEPTER_NAME", "Mustard Gas");
            LanguageAPI.Add("ENFORCER_UTILITY_TEARGASSCEPTER_DESCRIPTION", "Toss a grenade that covers an area in <style=cIsDamage>Impairing</style> gas, choking enemies for <style=cIsDamage>200% damage per second</style>.");

            //stun scepter
            LanguageAPI.Add("ENFORCER_UTILITY_SHOCKGRENADE_NAME", "Shock Grenade");
            LanguageAPI.Add("ENFORCER_UTILITY_SHOCKGRENADE_DESCRIPTION", "<style=cIsDamage>Shocking</style>. Launch a grenade that electrocutes enemies for <style=cIsDamage>" + 100f * ShockGrenade.damageCoefficient + "% damage</style>. Hold up to 3.");
            #endregion Utility

            #region Special
            //shield
            LanguageAPI.Add("ENFORCER_SPECIAL_SHIELDUP_NAME", "Protect and Serve");
            LanguageAPI.Add("ENFORCER_SPECIAL_SHIELDUP_DESCRIPTION", "Take a defensive stance, <style=cIsUtility>blocking all damage from the front</style>. <style=cIsUtility>Enhances primary fire</style>, but <style=cIsHealth>prevents sprinting and jumping</style>.");

            LanguageAPI.Add("ENFORCER_SPECIAL_SHIELDDOWN_NAME", "Protect and Serve");
            LanguageAPI.Add("ENFORCER_SPECIAL_SHIELDDOWN_DESCRIPTION", "Take a defensive stance, <style=cIsUtility>blocking all damage from the front</style>. <style=cIsDamage>Increases attack speed</style>, but <style=cIsHealth>prevents sprinting and jumping</style>.");

            //energy shield (still believe)

            LanguageAPI.Add("ENFORCER_SPECIAL_SHIELDON_NAME", "Project and Swerve");
            LanguageAPI.Add("ENFORCER_SPECIAL_SHIELDON_DESCRIPTION", "Take a defensive stance, <style=cIsUtility>projecting an Energy Shield in front of you</style>. <style=cIsDamage>Increases your rate of fire</style>, but <style=cIsUtility>prevents sprinting and jumping</style>.");

            LanguageAPI.Add("ENFORCER_SPECIAL_SHIELDOFF_NAME", "Project and Swerve");
            LanguageAPI.Add("ENFORCER_SPECIAL_SHIELDOFF_DESCRIPTION", "Take a defensive stance, <style=cIsUtility>projecting an Energy Shield in front of you</style>. <style=cIsDamage>Increases your rate of fire</style>, but <style=cIsUtility>prevents sprinting and jumping</style>.");

            LanguageAPI.Add("ENFORCER_SPECIAL_BOARDUP_NAME", "Skateboard");
            LanguageAPI.Add("ENFORCER_SPECIAL_BOARDUP_DESCRIPTION", "Swag.");

            LanguageAPI.Add("ENFORCER_SPECIAL_BOARDDOWN_NAME", "Skateboard");
            LanguageAPI.Add("ENFORCER_SPECIAL_BOARDDOWN_DESCRIPTION", "Unswag.");
            #endregion Special

            #endregion Skills

            #region Skins
            LanguageAPI.Add("ENFORCERBODY_DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add("ENFORCERBODY_MASTERY_SKIN_NAME", "Peacekeeper");
            LanguageAPI.Add("ENFORCERBODY_TYPHOON_SKIN_NAME", "Lawbringer");
            LanguageAPI.Add("ENFORCERBODY_BOT_SKIN_NAME", "N-4CR");
            LanguageAPI.Add("ENFORCERBODY_CLASSIC_SKIN_NAME", "Classic");

            LanguageAPI.Add("ENFORCERBODY_NEMESIS_SKIN_NAME", "Nemesis");

            LanguageAPI.Add("ENFORCERBODY_STORM_SKIN_NAME", "Monsoontrooper");
            LanguageAPI.Add("ENFORCERBODY_ENGI_SKIN_NAME", "Engineer?");
            LanguageAPI.Add("ENFORCERBODY_DOOM_SKIN_NAME", "Doom Slayer");
            LanguageAPI.Add("ENFORCERBODY_DESPERADO_SKIN_NAME", "Desperado");

            LanguageAPI.Add("ENFORCERBODY_FEMFORCER_SKIN_NAME", "Rushed");
            LanguageAPI.Add("ENFORCERBODY_FUCKINGSTEVE_SKIN_NAME", "Blocked");
            LanguageAPI.Add("ENFORCERBODY_FUCKINGFROG_SKIN_NAME", "Zero Suit");
            #endregion Skins

            #region Achievements
            //character
            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_CHARACTERUNLOCKABLE_ACHIEVEMENT_ID"), "Riot");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_CHARACTERUNLOCKABLE_ACHIEVEMENT_ID"), "Kill a Magma Worm, a Wandering Vagrant and a Stone Titan in a single run.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_MASTERYUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Mastery");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_MASTERYUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, beat the game or obliterate on Monsoon.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_GRANDMASTERYUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Grand Mastery");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_GRANDMASTERYUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, beat the game or obliterate on Typhoon or Eclipse.\n<color=#8888>(Counts any difficulty Typhoon or higher)</color>");

            //skills
            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_DOOMUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Rip and Tear...");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_DOOMUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, kill 20 imps in a single run.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_RIFLEUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Rapidfire");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_RIFLEUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, reach +400% attack speed.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_STUNGRENADEUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Crowd Control");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_STUNGRENADEUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, have 20 enemies under the effects of Tear Gas at once.");

            //skans                                                       //technically not robocop quote but a lot more understandable in general
            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_ROBITUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: We Have the Technology");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_ROBITUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, fall and be brought back to life.");


            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_NEMESISSKINUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Clearance");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_NEMESISSKINUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, stabilize the Cell in the Void Fields.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_DOOMINTERNALUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: ...Until it is done");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_DOOMINTERNALUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, kill 10 imp overlords in a single run.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_CLASSICUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Schmoovin'");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_CLASSICUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, show off your dance moves.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_DESPERADOUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Rules of Nature");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_DESPERADOUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, Defeat the unique guardian of Gilded Coast by pushing it off the edge of the map.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_BUNGUSUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Enforcing Perfection");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_BUNGUSUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, become one with the Bungus.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_STORMTROOPERUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Long Live the Empire");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_STORMTROOPERUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, defeat an elite Solus Control Unit.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_FROGUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Through Thick and Thin");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_FROGUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, make a friend on the moon.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_STEVEUNLOCKABLE_ACHIEVEMENT_ID"), "Enforcer: Blocks");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_STEVEUNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, block an attack with your shield.");

            #endregion Achievements

            #region skillsplus
            LanguageAPI.Add("ENFORCER_PRIMARY_SHOTGUN_NAME_UPGRADE_DESCRIPTION", "<style=cIsUtility>+5%</style> damage, <style=cIsUtility>+10%</style> bullet thickness, and <style=cIsUtility>+0.1</style> bullet spread");
            #endregion

            #region betterui
            LanguageAPI.Add("ENFORCER_PROC_BULLET", "Bullet");
            LanguageAPI.Add("ENFORCER_PROC_HAMMER", "Hammer");
            LanguageAPI.Add("ENFORCER_PROC_BASH", "Bash");
            LanguageAPI.Add("ENFORCER_PROC_GRENADE", "Grenade");
            LanguageAPI.Add("ENFORCER_PROC_DAMAGING_GAS", "Damaging Gas");
            LanguageAPI.Add("ENFORCER_PROC_SKATEBOARD", "Skateboard");
            #endregion
        }

        private static void RegisterNemforcerTokens() {

            #region Unforcer
            string characterName = "Nemesis Enforcer";
            string characterSubtitle = "Incorruptible Shadow";
            string bossSubtitle = "End of the Line";
            string characterOutro = "..and so he left, with newfound might to honor.";
            string characterOutroFailure = "..and so he returned, infallible bastion truly immortalized.";
            string characterLore = "\nheavy tf2\n\n";
            string characterDescription = "Nemesis Enforcer is an incarnation of valiance and strength, a supernatural kind who is nobody to take lightly.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            characterDescription = characterDescription + "< ! > Golden Hammer can hit many enemies at once." + Environment.NewLine + Environment.NewLine;
            characterDescription = characterDescription + "< ! > Use Dominance from high up to perform a devastating slam." + Environment.NewLine + Environment.NewLine;
            characterDescription = characterDescription + "< ! > Strafing with Golden Minigun is key to taking down powerful bosses." + Environment.NewLine + Environment.NewLine;
            characterDescription = characterDescription + "< ! > Shields are for pussies." + Environment.NewLine + Environment.NewLine;

            LanguageAPI.Add("NEMFORCER_NAME", characterName);
            LanguageAPI.Add("NEMFORCER_DESCRIPTION", characterDescription);
            LanguageAPI.Add("NEMFORCER_SUBTITLE", characterSubtitle);
            LanguageAPI.Add("NEMFORCER_LORE", characterLore);
            LanguageAPI.Add("NEMFORCER_OUTRO_FLAVOR", characterOutro);
            LanguageAPI.Add("NEMFORCER_OUTRO_FAILURE", characterOutroFailure);

            LanguageAPI.Add("NEMFORCER_BOSS_NAME", "Ultra Nemesis Enforcer");
            LanguageAPI.Add("NEMFORCER_BOSS_SUBTITLE", bossSubtitle);

            LanguageAPI.Add("DEDEDE_NAME", "King Dedede");
            LanguageAPI.Add("DEDEDE_BOSS_SUBTITLE", "King of Dreamland");

            LanguageAPI.Add("ENFORCER_EVENT_NEMFORCERBOSS_START", "An unnatural force emanates from the void... </style>");
            LanguageAPI.Add("ENFORCER_EVENT_NEMFORCERBOSS_END", "The void's influence fades...</style>");
            #endregion Unforcer

            #region Skills

            //passive
            LanguageAPI.Add("NEMFORCER_PASSIVE_NAME", "Colossus");
            LanguageAPI.Add("NEMFORCER_PASSIVE_DESCRIPTION", $"Nemesis Enforcer gains <style=cIsHealing>bonus health regen</style>, based on his current <style=cIsHealth>missing health</style>, up to <style=cIsHealth>{100 * NemforcerPlugin.passiveRegenBonus}% max health</style>.");
            LanguageAPI.Add("NEMFORCER_PASSIVE_REWORK_DESCRIPTION", $"Nemesis Enforcer regenerates <style=cIsHealing>{NemforcerPlugin.nemforcerRegenBuffPercent}%</style> of his <style=cIsHealing>maximum health</style> over {NemforcerPlugin.nemforcerRegenBuffDuration}s when striking enemies with <style=cIsDamage>melee attacks</style>.");

            #region Primary
            LanguageAPI.Add("NEMFORCER_PRIMARY_HAMMER_NAME", "Golden Hammer");
            LanguageAPI.Add("NEMFORCER_PRIMARY_HAMMER_DESCRIPTION", "Swing your hammer for <style=cIsDamage>" + 100f * NemHammerSwing.damageCoefficient + "%</style> damage.");

            LanguageAPI.Add("NEMFORCER_PRIMARY_THROWHAMMER_NAME", "Throwing Hammer");
            LanguageAPI.Add("NEMFORCER_PRIMARY_THROWHAMMER_DESCRIPTION", "Throw a hammer for <style=cIsDamage>" + 100f * ThrowHammer.damageCoefficient + "%</style> damage.");

            LanguageAPI.Add("NEMFORCER_PRIMARY_MINIGUN_NAME", "Golden Minigun");
            LanguageAPI.Add("NEMFORCER_PRIMARY_MINIGUN_DESCRIPTION", "Rev up and fire a hail of bullets dealing <style=cIsDamage>" + NemMinigunFire.baseDamageCoefficient * 100f + "% damage</style> per bullet. <style=cIsUtility>Slows your movement while shooting.</style>");
            #endregion Primary

            #region Secondary
            LanguageAPI.Add("KEYWORD_SLAM", $"<style=cKeywordName>Downward Slam</style><style=cIsDamage>Stunning.</style> Viciously <style=cIsHealth>crash down</style> with your hammer, dealing <style=cIsDamage>{100f * HammerAirSlam.minDamageCoefficient}%-{100f * HammerAirSlam.maxDamageCoefficient}% damage</style> and dealing an extra <style=cIsDamage>30%</style> of that on impact. <style=cIsUtility>Impact radius scales with speed.</style>");

            LanguageAPI.Add("NEMFORCER_SECONDARY_BASH_NAME", "Dominance");
            LanguageAPI.Add("NEMFORCER_SECONDARY_BASH_DESCRIPTION", $"<style=cIsUtility>Charge up</style>, then lunge forward and unleash a <style=cIsDamage>rising uppercut</style> for <style=cIsDamage>{100f * HammerUppercut.minDamageCoefficient}%-{100f * HammerUppercut.maxDamageCoefficient}% damage</style>. Use while <style=cIsUtility>falling and looking down</style> to perform a <style=cIsUtility>Downward Slam</style> instead.");

            LanguageAPI.Add("NEMFORCER_SECONDARY_SLAM_NAME", "Dominance (Minigun)");
            LanguageAPI.Add("NEMFORCER_SECONDARY_SLAM_DESCRIPTION", $"<style=cIsDamage>Stunning.</style> While in minigun stance, violently <style=cIsHealth>slam</style> down your hammer, dealing <style=cIsDamage>{100f * HammerSlam.damageCoefficient}% damage</style> and <style=cIsDamage>knocking back</style> enemies hit. <style=cIsUtility>Explodes projectiles.</style>");

            #endregion Secondary

            #region Utility
            LanguageAPI.Add("NEMFORCER_UTILITY_GAS_NAME", "XM47 Grenade");
            LanguageAPI.Add("NEMFORCER_UTILITY_GAS_DESCRIPTION", "Throw a grenade that explodes into a cloud of <style=cIsUtility>corrosive gas</style> that <style=cIsUtility>slows</style> and deals <style=cIsDamage>200% damage per second</style> and lasts for <style=cIsDamage>16 seconds</style>.");

            LanguageAPI.Add("NEMFORCER_UTILITY_JUMP_NAME", "Super Dedede Jump");
            LanguageAPI.Add("NEMFORCER_UTILITY_JUMP_DESCRIPTION", "Jump into the air, then slam down for <style=cIsDamage>" + 100f * SuperDededeJump.slamDamageCoefficient + "% damage</style>. <style=cIsUtility>Deals reduced damage outside the center of the impact.</style>");


            LanguageAPI.Add("KEYWORD_GRAPPLE", "<style=cKeywordName>Grappling</style><style=cSub>Applies <style=cIsDamage>stun</style> and attempts to <style=cIsUtility>grab</style> a nearby enemy.");

            LanguageAPI.Add("NEMFORCER_UTILITY_CRASH_NAME", "Heat Crash");
            LanguageAPI.Add("NEMFORCER_UTILITY_CRASH_DESCRIPTION", "<style=cIsUtility>Grappling.</style> Jump into the air, then slam down for <style=cIsDamage>" + 100f * HeatCrash.slamDamageCoefficient + "% damage</style>. <style=cIsUtility>Deals reduced damage outside the center of the impact.</style>");
            #endregion Utility

            #region Special
            LanguageAPI.Add("NEMFORCER_SPECIAL_MINIGUNUP_NAME", "Suppression Stance");
            LanguageAPI.Add("NEMFORCER_SPECIAL_MINIGUNUP_DESCRIPTION", "Take an offensive stance, <style=cIsDamage>readying your minigun</style>. <style=cIsHealth>Prevents sprinting</style>.");

            LanguageAPI.Add("NEMFORCER_SPECIAL_MINIGUNDOWN_NAME", "Endurance Stance");
            LanguageAPI.Add("NEMFORCER_SPECIAL_MINIGUNDOWN_DESCRIPTION", "Take a neutral stance, <style=cIsUtility>un-readying your minigun</style>.");
            #endregion Special

            #endregion SKills

            #region Skins
            LanguageAPI.Add("NEMFORCERBODY_DEFAULT_SKIN_NAME", "Nemesis");
            LanguageAPI.Add("NEMFORCERBODY_ENFORCER_SKIN_NAME", "Enforcer");
            LanguageAPI.Add("NEMFORCERBODY_CLASSIC_SKIN_NAME", "Classic");
            LanguageAPI.Add("NEMFORCERBODY_TYPHOON_SKIN_NAME", "Champion");
            LanguageAPI.Add("NEMFORCERBODY_DRIP_SKIN_NAME", "Dripforcer");
            LanguageAPI.Add("NEMFORCERBODY_DEDEDE_SKIN_NAME", "King Dedede");
            LanguageAPI.Add("NEMFORCERBODY_MINECRAFT_SKIN_NAME", "Minecraft");
            LanguageAPI.Add("NEMFORCERBODY_FEM_SKIN_NAME", "Spiked");
            #endregion Skins

            #region Achievements
            //character
            LanguageAPI.Add(Tokens.GetAchievementNameToken("ENFORCER_NEMESIS2UNLOCKABLE_ACHIEVEMENT_ID"), "???");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("ENFORCER_NEMESIS2UNLOCKABLE_ACHIEVEMENT_ID"), "As Enforcer, on monsoon or higher, stabilize the Void Fields and defeat Enforcer's Vestige.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("NEMFORCER_MASTERYUNLOCKABLE_ACHIEVEMENT_ID"), "Nemesis Enforcer: Mastery");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("NEMFORCER_MASTERYUNLOCKABLE_ACHIEVEMENT_ID"), "As Nemesis Enforcer, beat the game or obliterate on Monsoon.");

            LanguageAPI.Add(Tokens.GetAchievementNameToken("NEMFORCER_TYPHOONUNLOCKABLE_ACHIEVEMENT_ID"), "Nemesis Enforcer: Grand Mastery");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("NEMFORCER_TYPHOONUNLOCKABLE_ACHIEVEMENT_ID"), "As Nemesis Enforcer, beat the game or obliterate on Typhoon or Eclipse.\n<color=#8888>(Counts any difficulty Typhoon or higher)</color>");

            //sken
            LanguageAPI.Add(Tokens.GetAchievementNameToken("NEMFORCER_DOMINANCEUNLOCKABLE_ACHIEVEMENT_ID"), "Nemesis Enforcer: Demolition");
            LanguageAPI.Add(Tokens.GetAchievementDescToken("NEMFORCER_DOMINANCEUNLOCKABLE_ACHIEVEMENT_ID"), "As Nemesis Enforcer, destroy 5 projectiles at once with Dominance.");
            #endregion

            #region betterui
            LanguageAPI.Add("NEMFORCER_PROC_UPPERCUT", "Uppercut");
            LanguageAPI.Add("NEMFORCER_PROC_SLAM", "Slam");
            LanguageAPI.Add("NEMFORCER_PROC_JAM", "Jam");
            #endregion
        }

        /// <summary>
        /// gets langauge token from achievement's registered identifier
        /// </summary>
        public static string GetAchievementNameToken(string identifier)
        {
            return $"ACHIEVEMENT_{identifier.ToUpperInvariant()}_NAME";
        }
        /// <summary>
        /// gets langauge token from achievement's registered identifier
        /// </summary>
        public static string GetAchievementDescToken(string identifier)
        {
            return $"ACHIEVEMENT_{identifier.ToUpperInvariant()}_DESCRIPTION";
        }
    }
}