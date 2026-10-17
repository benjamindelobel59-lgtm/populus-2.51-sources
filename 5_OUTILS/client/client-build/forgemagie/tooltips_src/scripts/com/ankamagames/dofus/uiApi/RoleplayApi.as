package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.datacenter.almanax.AlmanaxCalendar;
   import com.ankamagames.dofus.internalDatacenter.items.ItemWrapper;
   import com.ankamagames.dofus.network.types.game.context.roleplay.GameRolePlayGroupMonsterInformations;
   import com.ankamagames.dofus.network.types.game.context.roleplay.GroupMonsterStaticInformations;
   
   public class RoleplayApi
   {
      
      public function RoleplayApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function getTotalFightOnCurrentMap() : uint
      {
         return 0;
      }
      
      public function getSpellToForgetList() : Array
      {
         return null;
      }
      
      public function getEmotesList() : Array
      {
         return null;
      }
      
      public function getUsableEmotesList() : Array
      {
         return null;
      }
      
      public function getSpawnMap() : Number
      {
         return 0;
      }
      
      public function getEntitiesOnCell(cellId:int) : Array
      {
         return null;
      }
      
      public function getPlayersIdOnCurrentMap() : Array
      {
         return null;
      }
      
      public function getMerchants() : Array
      {
         return null;
      }
      
      public function getPlayerIsInCurrentMap(playerId:Number) : Boolean
      {
         return false;
      }
      
      public function isUsingInteractive() : Boolean
      {
         return false;
      }
      
      public function getFight(id:int) : Object
      {
         return null;
      }
      
      public function putEntityOnTop(entity:Object) : void
      {
      }
      
      public function playGfx(gfxId:uint, cellId:uint) : void
      {
      }
      
      public function getEntityInfos(entity:Object) : Object
      {
         return null;
      }
      
      public function getEntityByName(name:String) : Object
      {
         return null;
      }
      
      public function switchButtonWrappers(btnWrapper1:Object, btnWrapper2:Object) : void
      {
      }
      
      public function setButtonWrapperActivation(btnWrapper:Object, active:Boolean, description:String = "", title:String = "") : void
      {
      }
      
      public function playEntityAnimation(npcID:int, animationName:String) : void
      {
      }
      
      public function playSpellAnimation(spellId:int, spellLevel:int, targetCellId:int) : void
      {
      }
      
      public function showNpcBubble(npcID:int, text:String) : void
      {
      }
      
      public function getMonsterXpBoostMultiplier(pMonsterId:int) : Number
      {
         return 0;
      }
      
      public function getMonsterDropBoostMultiplier(pMonsterId:int) : Number
      {
         return 0;
      }
      
      public function getRaceXpBoostMultiplier(pRaceId:int) : Number
      {
         return 0;
      }
      
      public function getRaceDropBoostMultiplier(pRaceId:int) : Number
      {
         return 0;
      }
      
      public function getMonsterGroupString(pMonsterGroupInfo:*) : String
      {
         return null;
      }
      
      public function getMonsterGroupFromString(pMonsterGroupString:String) : GameRolePlayGroupMonsterInformations
      {
         return null;
      }
      
      public function getAlmanaxCalendar() : AlmanaxCalendar
      {
         return null;
      }
      
      public function getAlmanaxMonsterXpBonusMultiplier(pMonsterId:int) : Number
      {
         return 0;
      }
      
      public function getAlmanaxMonsterDropChanceBonusMultiplier(pMonsterId:int) : Number
      {
         return 0;
      }
      
      public function getAlmanaxMonsterStarRateBonus() : int
      {
         return 0;
      }
      
      public function isMonsterAttacking(pMonsterId:int, pGrade:int) : Boolean
      {
         return false;
      }
      
      public function isMonstersGroupAttacking(pMonstersInfo:GroupMonsterStaticInformations) : Boolean
      {
         return false;
      }
      
      public function resourceIsFood(resource:ItemWrapper) : Boolean
      {
         return false;
      }
      
      public function canPetEatThisFood(pet:ItemWrapper, food:ItemWrapper) : Boolean
      {
         return false;
      }
   }
}

