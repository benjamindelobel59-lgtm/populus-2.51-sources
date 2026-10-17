package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.datacenter.abuse.AbuseReasons;
   import com.ankamagames.dofus.datacenter.alignments.AlignmentBalance;
   import com.ankamagames.dofus.datacenter.alignments.AlignmentSide;
   import com.ankamagames.dofus.datacenter.almanax.AlmanaxCalendar;
   import com.ankamagames.dofus.datacenter.appearance.Ornament;
   import com.ankamagames.dofus.datacenter.appearance.Title;
   import com.ankamagames.dofus.datacenter.appearance.TitleCategory;
   import com.ankamagames.dofus.datacenter.arena.ArenaLeague;
   import com.ankamagames.dofus.datacenter.arena.ArenaLeagueReward;
   import com.ankamagames.dofus.datacenter.arena.ArenaLeagueSeason;
   import com.ankamagames.dofus.datacenter.bonus.Bonus;
   import com.ankamagames.dofus.datacenter.breeds.Breed;
   import com.ankamagames.dofus.datacenter.breeds.BreedRole;
   import com.ankamagames.dofus.datacenter.breeds.Head;
   import com.ankamagames.dofus.datacenter.challenges.Challenge;
   import com.ankamagames.dofus.datacenter.characteristics.Characteristic;
   import com.ankamagames.dofus.datacenter.characteristics.CharacteristicCategory;
   import com.ankamagames.dofus.datacenter.communication.ChatChannel;
   import com.ankamagames.dofus.datacenter.communication.Emoticon;
   import com.ankamagames.dofus.datacenter.communication.InfoMessage;
   import com.ankamagames.dofus.datacenter.communication.Smiley;
   import com.ankamagames.dofus.datacenter.communication.SmileyCategory;
   import com.ankamagames.dofus.datacenter.communication.SmileyPack;
   import com.ankamagames.dofus.datacenter.dare.DareCriteria;
   import com.ankamagames.dofus.datacenter.effects.Effect;
   import com.ankamagames.dofus.datacenter.externalnotifications.ExternalNotification;
   import com.ankamagames.dofus.datacenter.guild.EmblemSymbol;
   import com.ankamagames.dofus.datacenter.guild.RankName;
   import com.ankamagames.dofus.datacenter.houses.HavenbagTheme;
   import com.ankamagames.dofus.datacenter.houses.House;
   import com.ankamagames.dofus.datacenter.idols.Idol;
   import com.ankamagames.dofus.datacenter.interactives.Interactive;
   import com.ankamagames.dofus.datacenter.items.Incarnation;
   import com.ankamagames.dofus.datacenter.items.IncarnationLevel;
   import com.ankamagames.dofus.datacenter.items.Item;
   import com.ankamagames.dofus.datacenter.items.ItemSet;
   import com.ankamagames.dofus.datacenter.items.ItemType;
   import com.ankamagames.dofus.datacenter.items.PresetIcon;
   import com.ankamagames.dofus.datacenter.jobs.Skill;
   import com.ankamagames.dofus.datacenter.livingObjects.Pet;
   import com.ankamagames.dofus.datacenter.misc.BreachBoss;
   import com.ankamagames.dofus.datacenter.misc.BreachPrize;
   import com.ankamagames.dofus.datacenter.misc.OptionalFeature;
   import com.ankamagames.dofus.datacenter.misc.Pack;
   import com.ankamagames.dofus.datacenter.monsters.Companion;
   import com.ankamagames.dofus.datacenter.monsters.CompanionCharacteristic;
   import com.ankamagames.dofus.datacenter.monsters.CompanionSpell;
   import com.ankamagames.dofus.datacenter.monsters.Monster;
   import com.ankamagames.dofus.datacenter.monsters.MonsterMiniBoss;
   import com.ankamagames.dofus.datacenter.monsters.MonsterRace;
   import com.ankamagames.dofus.datacenter.monsters.MonsterSuperRace;
   import com.ankamagames.dofus.datacenter.mounts.Mount;
   import com.ankamagames.dofus.datacenter.mounts.MountBehavior;
   import com.ankamagames.dofus.datacenter.npcs.Npc;
   import com.ankamagames.dofus.datacenter.npcs.NpcAction;
   import com.ankamagames.dofus.datacenter.npcs.TaxCollectorFirstname;
   import com.ankamagames.dofus.datacenter.npcs.TaxCollectorName;
   import com.ankamagames.dofus.datacenter.quest.Achievement;
   import com.ankamagames.dofus.datacenter.quest.AchievementCategory;
   import com.ankamagames.dofus.datacenter.quest.AchievementObjective;
   import com.ankamagames.dofus.datacenter.quest.AchievementReward;
   import com.ankamagames.dofus.datacenter.quest.Quest;
   import com.ankamagames.dofus.datacenter.quest.QuestCategory;
   import com.ankamagames.dofus.datacenter.quest.QuestObjective;
   import com.ankamagames.dofus.datacenter.quest.QuestStep;
   import com.ankamagames.dofus.datacenter.quest.treasureHunt.LegendaryTreasureHunt;
   import com.ankamagames.dofus.datacenter.servers.Server;
   import com.ankamagames.dofus.datacenter.servers.ServerCommunity;
   import com.ankamagames.dofus.datacenter.servers.ServerGameType;
   import com.ankamagames.dofus.datacenter.servers.ServerLang;
   import com.ankamagames.dofus.datacenter.servers.ServerPopulation;
   import com.ankamagames.dofus.datacenter.servers.ServerTemporisSeason;
   import com.ankamagames.dofus.datacenter.spells.Spell;
   import com.ankamagames.dofus.datacenter.spells.SpellBomb;
   import com.ankamagames.dofus.datacenter.spells.SpellLevel;
   import com.ankamagames.dofus.datacenter.spells.SpellPair;
   import com.ankamagames.dofus.datacenter.spells.SpellState;
   import com.ankamagames.dofus.datacenter.spells.SpellType;
   import com.ankamagames.dofus.datacenter.spells.SpellVariant;
   import com.ankamagames.dofus.datacenter.world.Area;
   import com.ankamagames.dofus.datacenter.world.Dungeon;
   import com.ankamagames.dofus.datacenter.world.Hint;
   import com.ankamagames.dofus.datacenter.world.HintCategory;
   import com.ankamagames.dofus.datacenter.world.MapPosition;
   import com.ankamagames.dofus.datacenter.world.SubArea;
   import com.ankamagames.dofus.datacenter.world.SuperArea;
   import com.ankamagames.dofus.datacenter.world.WorldMap;
   import com.ankamagames.dofus.internalDatacenter.almanax.AlmanaxEvent;
   import com.ankamagames.dofus.internalDatacenter.almanax.AlmanaxMonth;
   import com.ankamagames.dofus.internalDatacenter.almanax.AlmanaxZodiac;
   import com.ankamagames.dofus.internalDatacenter.appearance.OrnamentWrapper;
   import com.ankamagames.dofus.internalDatacenter.appearance.TitleWrapper;
   import com.ankamagames.dofus.internalDatacenter.communication.EmoteWrapper;
   import com.ankamagames.dofus.internalDatacenter.fight.ChallengeWrapper;
   import com.ankamagames.dofus.internalDatacenter.house.HavenbagFurnitureWrapper;
   import com.ankamagames.dofus.internalDatacenter.house.HouseWrapper;
   import com.ankamagames.dofus.internalDatacenter.items.ItemWrapper;
   import com.ankamagames.dofus.internalDatacenter.items.ShortcutWrapper;
   import com.ankamagames.dofus.internalDatacenter.jobs.JobWrapper;
   import com.ankamagames.dofus.internalDatacenter.spells.SpellWrapper;
   import com.ankamagames.dofus.internalDatacenter.userInterface.ButtonWrapper;
   import com.ankamagames.jerakine.types.positions.WorldPoint;
   
   public class DataApi
   {
      
      public function DataApi()
      {
         super();
      }
      
      public function initStaticCartographyData() : void
      {
      }
      
      public function destroy() : void
      {
      }
      
      public function getNotifications() : Array
      {
         return null;
      }
      
      public function getServer(id:int) : Server
      {
         return null;
      }
      
      public function getServerPopulation(id:int) : ServerPopulation
      {
         return null;
      }
      
      public function getBreed(id:int) : Breed
      {
         return null;
      }
      
      public function getBreeds() : Array
      {
         return null;
      }
      
      public function getBreedRole(id:int) : BreedRole
      {
         return null;
      }
      
      public function getBreedRoles() : Array
      {
         return null;
      }
      
      public function getHead(id:int) : Head
      {
         return null;
      }
      
      public function getHeads() : Array
      {
         return null;
      }
      
      public function getCharacteristic(id:int) : Characteristic
      {
         return null;
      }
      
      public function getCharacteristics() : Array
      {
         return null;
      }
      
      public function getCharacteristicCategory(id:int) : CharacteristicCategory
      {
         return null;
      }
      
      public function getCharacteristicCategories() : Array
      {
         return null;
      }
      
      public function getSpell(id:int) : Spell
      {
         return null;
      }
      
      public function getSpells() : Array
      {
         return null;
      }
      
      public function getSpellWrapper(id:uint, level:uint = 1, useCache:Boolean = false, playerId:Number = 0, variantActivated:Boolean = false) : SpellWrapper
      {
         return null;
      }
      
      public function getSpellVariant(id:int) : SpellVariant
      {
         return null;
      }
      
      public function getSpellVariants() : Array
      {
         return null;
      }
      
      public function getEmoteWrapper(id:uint, position:uint = 0) : EmoteWrapper
      {
         return null;
      }
      
      public function getButtonWrapper(buttonId:uint, position:int, uriName:String, callback:Function, name:String, shortcutName:String = "", description:String = "") : ButtonWrapper
      {
         return null;
      }
      
      public function getShortcutWrapper(slot:uint, id:int, shortcutType:uint = 0, gid:int = 0) : ShortcutWrapper
      {
         return null;
      }
      
      public function getJobs() : Array
      {
         return null;
      }
      
      public function getJobWrapper(id:uint) : JobWrapper
      {
         return null;
      }
      
      public function getTitleWrapper(id:uint) : TitleWrapper
      {
         return null;
      }
      
      public function getOrnamentWrapper(id:uint) : OrnamentWrapper
      {
         return null;
      }
      
      public function getSpellLevel(id:int) : SpellLevel
      {
         return null;
      }
      
      public function getSpellLevelBySpell(spell:Spell, level:int) : SpellLevel
      {
         return null;
      }
      
      public function getSpellType(id:int) : SpellType
      {
         return null;
      }
      
      public function getSpellState(id:int) : SpellState
      {
         return null;
      }
      
      public function getChatChannel(id:int) : ChatChannel
      {
         return null;
      }
      
      public function getAllChatChannels() : Array
      {
         return null;
      }
      
      public function getSubArea(id:int) : SubArea
      {
         return null;
      }
      
      public function getSubAreaFromMap(mapId:Number) : SubArea
      {
         return null;
      }
      
      public function getAllSubAreas() : Array
      {
         return null;
      }
      
      public function getArea(id:int) : Area
      {
         return null;
      }
      
      public function getSuperArea(id:int) : SuperArea
      {
         return null;
      }
      
      public function getAllArea(withHouses:Boolean = false, withPaddocks:Boolean = false) : Array
      {
         return null;
      }
      
      public function getWorldPoint(id:int) : WorldPoint
      {
         return null;
      }
      
      public function getItem(id:int, returnDefaultItemIfNull:Boolean = true) : Item
      {
         return null;
      }
      
      public function getItems() : Array
      {
         return null;
      }
      
      public function getItemsByIds(ids:Object) : Object
      {
         return null;
      }
      
      public function getIncarnationLevel(incarnationId:int, level:int) : IncarnationLevel
      {
         return null;
      }
      
      public function getIncarnation(incarnationId:int) : Incarnation
      {
         return null;
      }
      
      public function getNewGenericSlotData() : Object
      {
         return null;
      }
      
      public function getItemIconUri(iconId:uint) : Object
      {
         return null;
      }
      
      public function getItemName(id:int) : String
      {
         return null;
      }
      
      public function getItemType(id:int) : ItemType
      {
         return null;
      }
      
      public function getItemTypes() : Array
      {
         return null;
      }
      
      public function getItemSet(id:int) : ItemSet
      {
         return null;
      }
      
      public function getPet(id:int) : Pet
      {
         return null;
      }
      
      public function getSetEffects(GIDList:Array, aSetBonus:Array = null) : Array
      {
         return null;
      }
      
      public function getArenaLeagueById(leagueId:uint) : ArenaLeague
      {
         return null;
      }
      
      public function getArenaLeagueSeasonById(seasonId:uint) : ArenaLeagueSeason
      {
         return null;
      }
      
      public function getArenaLeagueRewardsForCurrentRankAndSeason(league:int, season:int, endSeason:Boolean) : ArenaLeagueReward
      {
         return null;
      }
      
      public function getMonsterFromId(monsterId:uint) : Monster
      {
         return null;
      }
      
      public function getMonsters() : Array
      {
         return null;
      }
      
      public function getMonsterMiniBossFromId(monsterId:uint) : MonsterMiniBoss
      {
         return null;
      }
      
      public function getMonsterRaceFromId(raceId:uint) : MonsterRace
      {
         return null;
      }
      
      public function getMonsterRaces() : Array
      {
         return null;
      }
      
      public function getMonsterSuperRaceFromId(raceId:uint) : MonsterSuperRace
      {
         return null;
      }
      
      public function getMonsterSuperRaces() : Array
      {
         return null;
      }
      
      public function getCompanion(companionId:uint) : Companion
      {
         return null;
      }
      
      public function getAllCompanions() : Array
      {
         return null;
      }
      
      public function getCompanionCharacteristic(companionCharacteristicId:uint) : CompanionCharacteristic
      {
         return null;
      }
      
      public function getCompanionSpell(companionSpellId:uint) : CompanionSpell
      {
         return null;
      }
      
      public function getNpc(npcId:uint) : Npc
      {
         return null;
      }
      
      public function getNpcAction(actionId:uint) : NpcAction
      {
         return null;
      }
      
      public function getAlignmentSide(sideId:uint) : AlignmentSide
      {
         return null;
      }
      
      public function getAlignmentBalance(percent:uint) : AlignmentBalance
      {
         return null;
      }
      
      public function getRankName(rankId:uint) : RankName
      {
         return null;
      }
      
      public function getAllRankNames() : Array
      {
         return null;
      }
      
      public function getItemWrapper(itemGID:uint, itemPosition:int = 0, itemUID:uint = 0, itemQuantity:uint = 0, itemEffects:* = null) : ItemWrapper
      {
         return null;
      }
      
      public function getItemFromUId(objectUID:uint) : ItemWrapper
      {
         return null;
      }
      
      public function getSkill(skillId:uint) : Skill
      {
         return null;
      }
      
      public function getSkills() : Array
      {
         return null;
      }
      
      public function getHouseSkills() : Array
      {
         return null;
      }
      
      public function getInfoMessage(infoMsgId:uint) : InfoMessage
      {
         return null;
      }
      
      public function getAllInfoMessages() : Array
      {
         return null;
      }
      
      public function getSmileyWrappers() : Array
      {
         return null;
      }
      
      public function getSmiley(id:uint) : Smiley
      {
         return null;
      }
      
      public function getAllSmiley() : Array
      {
         return null;
      }
      
      public function getSmileyCategory(id:uint) : SmileyCategory
      {
         return null;
      }
      
      public function getAllSmileyCategory() : Array
      {
         return null;
      }
      
      public function getSmileyPack(id:uint) : SmileyPack
      {
         return null;
      }
      
      public function getAllSmileyPack() : Array
      {
         return null;
      }
      
      public function getEmoticon(id:uint) : Emoticon
      {
         return null;
      }
      
      public function getChallenge(id:uint) : Challenge
      {
         return null;
      }
      
      public function getChallengeWrapper(id:uint) : ChallengeWrapper
      {
         return null;
      }
      
      public function getChallenges() : Array
      {
         return null;
      }
      
      public function getDareCriteria(id:uint) : DareCriteria
      {
         return null;
      }
      
      public function getAllDareCriteria() : Array
      {
         return null;
      }
      
      public function getTaxCollectorName(id:uint) : TaxCollectorName
      {
         return null;
      }
      
      public function getTaxCollectorFirstname(id:uint) : TaxCollectorFirstname
      {
         return null;
      }
      
      public function getEmblems() : Array
      {
         return null;
      }
      
      public function getEmblemSymbol(symbolId:int) : EmblemSymbol
      {
         return null;
      }
      
      public function getAllEmblemSymbolCategories() : Array
      {
         return null;
      }
      
      public function getQuest(questId:int) : Quest
      {
         return null;
      }
      
      public function getQuestCategory(questCatId:int) : QuestCategory
      {
         return null;
      }
      
      public function getQuestObjective(questObjectiveId:int) : QuestObjective
      {
         return null;
      }
      
      public function getQuestStep(questStepId:int) : QuestStep
      {
         return null;
      }
      
      public function getAchievement(achievementId:int) : Achievement
      {
         return null;
      }
      
      public function getAchievements() : Array
      {
         return null;
      }
      
      public function getAchievementCategory(achievementCatId:int) : AchievementCategory
      {
         return null;
      }
      
      public function getAchievementCategories() : Array
      {
         return null;
      }
      
      public function getAchievementReward(rewardId:int) : AchievementReward
      {
         return null;
      }
      
      public function getAchievementRewards() : Array
      {
         return null;
      }
      
      public function getAchievementObjective(objectiveId:int) : AchievementObjective
      {
         return null;
      }
      
      public function getAchievementObjectives() : Array
      {
         return null;
      }
      
      public function getHouse(houseId:int) : House
      {
         return null;
      }
      
      public function getLivingObjectSkins(item:ItemWrapper) : Array
      {
         return null;
      }
      
      public function getAbuseReasonName(abuseReasonId:uint) : AbuseReasons
      {
         return null;
      }
      
      public function getAllAbuseReasons() : Array
      {
         return null;
      }
      
      public function getPresetIcons() : Array
      {
         return null;
      }
      
      public function getPresetIcon(iconId:uint) : PresetIcon
      {
         return null;
      }
      
      public function getIdolsPresetIcons() : Array
      {
         return null;
      }
      
      public function getIdolsPresetIcon(iconId:uint) : Object
      {
         return null;
      }
      
      public function getDungeons() : Array
      {
         return null;
      }
      
      public function getDungeon(dungeonId:uint) : Dungeon
      {
         return null;
      }
      
      public function getMapInfo(mapId:Number) : MapPosition
      {
         return null;
      }
      
      public function getWorldMap(mapId:Number) : WorldMap
      {
         return null;
      }
      
      public function getAllWorldMaps() : Array
      {
         return null;
      }
      
      public function getAllWaypoints() : Array
      {
         return null;
      }
      
      public function getHintCategory(hintId:uint) : HintCategory
      {
         return null;
      }
      
      public function getHintCategories() : Array
      {
         return null;
      }
      
      public function getHousesInformations() : Object
      {
         return null;
      }
      
      public function getHouseInformations(doorId:uint) : HouseWrapper
      {
         return null;
      }
      
      public function getSpellPair(pairId:uint) : SpellPair
      {
         return null;
      }
      
      public function getBomb(bombId:uint) : SpellBomb
      {
         return null;
      }
      
      public function getPack(packId:uint) : Pack
      {
         return null;
      }
      
      public function getLegendaryTreasureHunt(huntId:uint) : LegendaryTreasureHunt
      {
         return null;
      }
      
      public function getLegendaryTreasureHunts() : Array
      {
         return null;
      }
      
      public function getTitle(titleId:uint) : Title
      {
         return null;
      }
      
      public function getTitles() : Array
      {
         return null;
      }
      
      public function getTitleCategory(titleCatId:uint) : TitleCategory
      {
         return null;
      }
      
      public function getTitleCategories() : Array
      {
         return null;
      }
      
      public function getOrnament(oId:uint) : Ornament
      {
         return null;
      }
      
      public function getOrnaments() : Array
      {
         return null;
      }
      
      public function getOptionalFeatureByKeyword(key:String) : OptionalFeature
      {
         return null;
      }
      
      public function getEffect(effectId:uint) : Effect
      {
         return null;
      }
      
      public function getEvolutiveEffectInstancesByExperienceBoost(item:ItemWrapper, experienceBoost:int) : Array
      {
         return null;
      }
      
      public function getEvolutiveItemLevelByExperiencePoints(item:ItemWrapper, experiencePoints:int) : int
      {
         return 0;
      }
      
      public function getAlmanaxEvent() : AlmanaxEvent
      {
         return null;
      }
      
      public function getAlmanaxZodiac() : AlmanaxZodiac
      {
         return null;
      }
      
      public function getAlmanaxMonth() : AlmanaxMonth
      {
         return null;
      }
      
      public function getAlmanaxCalendar(calendarId:uint) : AlmanaxCalendar
      {
         return null;
      }
      
      public function getExternalNotification(pExtNotifId:int) : ExternalNotification
      {
         return null;
      }
      
      public function getExternalNotifications() : Array
      {
         return null;
      }
      
      public function queryString(dataClass:Class, field:String, pattern:String) : Object
      {
         return null;
      }
      
      public function queryEquals(dataClass:Class, field:String, value:*) : Object
      {
         return null;
      }
      
      public function queryUnion(... ids) : Object
      {
         return null;
      }
      
      public function queryIntersection(... ids) : Object
      {
         return null;
      }
      
      public function queryGreaterThan(dataClass:Class, field:String, value:*) : Object
      {
         return null;
      }
      
      public function querySmallerThan(dataClass:Class, field:String, value:*) : Object
      {
         return null;
      }
      
      public function queryReturnInstance(dataClass:Class, ids:Object) : Object
      {
         return null;
      }
      
      public function querySort(dataClass:Class, ids:Object, fields:*, ascending:* = true) : Object
      {
         return null;
      }
      
      public function querySortI18nId(data:*, fields:*, ascending:* = true) : *
      {
         return null;
      }
      
      public function getAllZaaps() : Array
      {
         return null;
      }
      
      public function getUnknowZaaps(knwonZaapList:Array) : Array
      {
         return null;
      }
      
      public function getAllVeteranRewards() : Array
      {
         return null;
      }
      
      public function getBreachPrizeById(id:int) : BreachPrize
      {
         return null;
      }
      
      public function getIdol(pIdolId:uint) : Idol
      {
         return null;
      }
      
      public function getIdolByItemId(pItemId:int) : Idol
      {
         return null;
      }
      
      public function getAllIdols() : Array
      {
         return null;
      }
      
      public function getHintById(pHintId:int) : Hint
      {
         return null;
      }
      
      public function getHints() : Array
      {
         return null;
      }
      
      public function getHavenbagFurnitures() : Array
      {
         return null;
      }
      
      public function getHavenbagFurnitureWrapper(furnitureTypeId:int) : HavenbagFurnitureWrapper
      {
         return null;
      }
      
      public function getHavenbagTheme(themeId:int) : HavenbagTheme
      {
         return null;
      }
      
      public function getInteractive(interactiveId:int) : Interactive
      {
         return null;
      }
      
      public function getNullEffectInstance(e:*) : *
      {
         return null;
      }
      
      public function getBonusById(pBonusId:int) : Bonus
      {
         return null;
      }
      
      public function getMountBehaviorById(id:int) : MountBehavior
      {
         return null;
      }
      
      public function getMountFamilyNameById(mountFamilyId:int) : String
      {
         return null;
      }
      
      public function getMountById(mountId:int) : Mount
      {
         return null;
      }
      
      public function getServerGameTypes() : Array
      {
         return null;
      }
      
      public function getServerGameType(id:int) : ServerGameType
      {
         return null;
      }
      
      public function getServerCommunities() : Array
      {
         return null;
      }
      
      public function getServerCommunity(id:int) : ServerCommunity
      {
         return null;
      }
      
      public function getServerLang(langId:int) : ServerLang
      {
         return null;
      }
      
      public function getFinishMoves() : Array
      {
         return null;
      }
      
      public function getBreachBossById(id:int) : BreachBoss
      {
         return null;
      }
      
      public function getBreachBossByMonsterId(id:int) : BreachBoss
      {
         return null;
      }
      
      public function getBreachBossByRewardId(id:int) : BreachBoss
      {
         return null;
      }
      
      public function getCurrentSeason() : ServerTemporisSeason
      {
         return null;
      }
   }
}

