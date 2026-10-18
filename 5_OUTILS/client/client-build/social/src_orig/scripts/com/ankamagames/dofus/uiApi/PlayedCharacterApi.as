package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.datacenter.appearance.Ornament;
   import com.ankamagames.dofus.datacenter.appearance.Title;
   import com.ankamagames.dofus.datacenter.world.SubArea;
   import com.ankamagames.dofus.internalDatacenter.items.WeaponWrapper;
   import com.ankamagames.dofus.internalDatacenter.spells.SpellWrapper;
   import com.ankamagames.dofus.internalDatacenter.world.WorldPointWrapper;
   import com.ankamagames.dofus.network.types.game.character.characteristic.CharacterCharacteristicsInformations;
   import com.ankamagames.dofus.network.types.game.character.restriction.ActorRestrictionsInformations;
   import com.ankamagames.dofus.network.types.game.context.roleplay.GameRolePlayCharacterInformations;
   
   public class PlayedCharacterApi
   {
      
      public function PlayedCharacterApi()
      {
         super();
      }
      
      public function characteristics() : CharacterCharacteristicsInformations
      {
         return null;
      }
      
      public function getPlayedCharacterInfo() : Object
      {
         return null;
      }
      
      public function getCurrentEntityLook() : Object
      {
         return null;
      }
      
      public function getInventory() : Object
      {
         return null;
      }
      
      public function getEquipment() : Array
      {
         return null;
      }
      
      public function getSpellInventory() : Array
      {
         return null;
      }
      
      public function getSpells(returnBreedSpells:Boolean) : Array
      {
         return null;
      }
      
      public function getBreedSpellActivatedIds() : Array
      {
         return null;
      }
      
      public function getJobs() : Array
      {
         return null;
      }
      
      public function getMount() : Object
      {
         return null;
      }
      
      public function getTitle() : Title
      {
         return null;
      }
      
      public function getOrnament() : Ornament
      {
         return null;
      }
      
      public function getKnownTitles() : Object
      {
         return null;
      }
      
      public function getKnownOrnaments() : Object
      {
         return null;
      }
      
      public function titlesOrnamentsAskedBefore() : Boolean
      {
         return false;
      }
      
      public function getEntityInfos() : GameRolePlayCharacterInformations
      {
         return null;
      }
      
      public function getEntityTooltipInfos() : Object
      {
         return null;
      }
      
      public function getKamasMaxLimit() : Number
      {
         return 0;
      }
      
      public function inventoryWeight() : uint
      {
         return 0;
      }
      
      public function inventoryWeightMax() : uint
      {
         return 0;
      }
      
      public function isIncarnation() : Boolean
      {
         return false;
      }
      
      public function isMutated() : Boolean
      {
         return false;
      }
      
      public function isInHouse() : Boolean
      {
         return false;
      }
      
      public function isInExchange() : Boolean
      {
         return false;
      }
      
      public function isInFight() : Boolean
      {
         return false;
      }
      
      public function isInPreFight() : Boolean
      {
         return false;
      }
      
      public function isSpectator() : Boolean
      {
         return false;
      }
      
      public function isInParty() : Boolean
      {
         return false;
      }
      
      public function isPartyLeader() : Boolean
      {
         return false;
      }
      
      public function isRidding() : Boolean
      {
         return false;
      }
      
      public function isPetsMounting() : Boolean
      {
         return false;
      }
      
      public function hasCompanion() : Boolean
      {
         return false;
      }
      
      public function id() : Number
      {
         return 0;
      }
      
      public function restrictions() : ActorRestrictionsInformations
      {
         return null;
      }
      
      public function isMutant() : Boolean
      {
         return false;
      }
      
      public function publicMode() : Boolean
      {
         return false;
      }
      
      public function artworkId() : int
      {
         return 0;
      }
      
      public function isCreature() : Boolean
      {
         return false;
      }
      
      public function getBone() : uint
      {
         return 0;
      }
      
      public function getSkin() : uint
      {
         return 0;
      }
      
      public function getColors() : Object
      {
         return null;
      }
      
      public function getSubentityColors() : Object
      {
         return null;
      }
      
      public function getAlignmentSide() : int
      {
         return 0;
      }
      
      public function getAlignmentValue() : uint
      {
         return 0;
      }
      
      public function getAlignmentAggressableStatus() : uint
      {
         return 0;
      }
      
      public function getAlignmentGrade() : uint
      {
         return 0;
      }
      
      public function getMaxSummonedCreature() : uint
      {
         return 0;
      }
      
      public function getCurrentSummonedCreature() : uint
      {
         return 0;
      }
      
      public function canSummon() : Boolean
      {
         return false;
      }
      
      public function getSpell(spellId:uint) : SpellWrapper
      {
         return null;
      }
      
      public function canCastThisSpell(spellId:uint, lvl:uint) : Boolean
      {
         return false;
      }
      
      public function canCastThisSpellWithResult(spellId:uint, lvl:uint, target:Number = 0) : String
      {
         return null;
      }
      
      public function canCastThisSpellOnTarget(spellId:uint, lvl:uint, pTargetId:Number) : Boolean
      {
         return false;
      }
      
      public function getSpellModification(spellId:uint, carac:int) : int
      {
         return 0;
      }
      
      public function getSpellModifications(spellId:uint) : Object
      {
         return null;
      }
      
      public function isInHisHouse() : Boolean
      {
         return false;
      }
      
      public function getPlayerHouses() : Object
      {
         return null;
      }
      
      public function currentMap() : WorldPointWrapper
      {
         return null;
      }
      
      public function currentSubArea() : SubArea
      {
         return null;
      }
      
      public function isInTutorialArea() : Boolean
      {
         return false;
      }
      
      public function state() : uint
      {
         return 0;
      }
      
      public function isAlive() : Boolean
      {
         return false;
      }
      
      public function getFollowingPlayerIds() : Object
      {
         return null;
      }
      
      public function getPlayerSet(objectGID:uint) : Object
      {
         return null;
      }
      
      public function getWeapon() : WeaponWrapper
      {
         return null;
      }
      
      public function getExperienceBonusPercent() : int
      {
         return 0;
      }
      
      public function getAchievementPoints() : int
      {
         return 0;
      }
      
      public function getWaitingGifts() : Array
      {
         return null;
      }
      
      public function getSoloIdols() : Object
      {
         return null;
      }
      
      public function getPartyIdols() : Object
      {
         return null;
      }
      
      public function setPartyIdols(pIdols:Object) : void
      {
      }
      
      public function getIdolsPresets() : Object
      {
         return null;
      }
      
      public function knowSpell(pSpellId:uint) : int
      {
         return 0;
      }
      
      public function isInHisHavenbag() : Boolean
      {
         return false;
      }
      
      public function isInHavenbag() : Boolean
      {
         return false;
      }
      
      public function havenbagSharePermissions() : uint
      {
         return 0;
      }
      
      public function isInBreach() : Boolean
      {
         return false;
      }
      
      public function isInAnomaly() : Boolean
      {
         return false;
      }
   }
}

