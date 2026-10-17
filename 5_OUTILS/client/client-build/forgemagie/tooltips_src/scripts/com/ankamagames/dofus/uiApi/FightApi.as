package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.datacenter.spells.Spell;
   import com.ankamagames.dofus.internalDatacenter.fight.FighterInformations;
   import com.ankamagames.dofus.internalDatacenter.spells.EffectsListWrapper;
   import com.ankamagames.dofus.internalDatacenter.spells.EffectsWrapper;
   import com.ankamagames.dofus.network.types.game.character.characteristic.CharacterCharacteristicsInformations;
   
   public class FightApi
   {
      
      public function FightApi()
      {
         super();
      }
      
      public function isSlaveContext() : Boolean
      {
         return false;
      }
      
      public function destroy() : void
      {
      }
      
      public function getFighterInformations(fighterId:Number) : FighterInformations
      {
         return null;
      }
      
      public function getFighterName(fighterId:Number) : String
      {
         return null;
      }
      
      public function getFighterLevel(fighterId:Number) : uint
      {
         return 0;
      }
      
      public function getFighters() : Object
      {
         return null;
      }
      
      public function getMonsterId(id:Number) : int
      {
         return 0;
      }
      
      public function getDeadFighters() : Object
      {
         return null;
      }
      
      public function getFightType() : uint
      {
         return 0;
      }
      
      public function getBuffList(targetId:Number) : Array
      {
         return null;
      }
      
      public function getBuffById(buffId:uint, playerId:Number) : Object
      {
         return null;
      }
      
      public function createEffectsWrapper(spell:Spell, effects:Array, name:String) : EffectsWrapper
      {
         return null;
      }
      
      public function getCastingSpellBuffEffects(targetId:Number, castingSpellId:uint) : EffectsWrapper
      {
         return null;
      }
      
      public function getAllBuffEffects(targetId:Number) : EffectsListWrapper
      {
         return null;
      }
      
      public function isCastingSpell() : Boolean
      {
         return false;
      }
      
      public function cancelSpell() : void
      {
      }
      
      public function getChallengeList() : Array
      {
         return null;
      }
      
      public function getCurrentPlayedFighterId() : Number
      {
         return 0;
      }
      
      public function getPlayingFighterId() : Number
      {
         return 0;
      }
      
      public function isCompanion(pFighterId:Number) : Boolean
      {
         return false;
      }
      
      public function getCurrentPlayedCharacteristicsInformations() : CharacterCharacteristicsInformations
      {
         return null;
      }
      
      public function preFightIsActive() : Boolean
      {
         return false;
      }
      
      public function isWaitingBeforeFight() : Boolean
      {
         return false;
      }
      
      public function isFightLeader() : Boolean
      {
         return false;
      }
      
      public function isSpectator() : Boolean
      {
         return false;
      }
      
      public function isDematerializated() : Boolean
      {
         return false;
      }
      
      public function getTurnsCount() : int
      {
         return 0;
      }
      
      public function getFighterStatus(fighterId:Number) : int
      {
         return 0;
      }
      
      public function isMouseOverFighter(fighterId:Number) : Boolean
      {
         return false;
      }
      
      public function updateSwapPositionRequestsIcons() : void
      {
      }
      
      public function setSwapPositionRequestsIconsVisibility(pVisible:Boolean) : void
      {
      }
   }
}

