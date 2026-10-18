package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.internalDatacenter.spells.SpellWrapper;
   import com.ankamagames.dofus.network.types.game.context.GameContextActorInformations;
   
   public class UtilApi
   {
      
      public function UtilApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function callWithParameters(method:Function, parameters:Array) : void
      {
      }
      
      public function callConstructorWithParameters(callClass:Class, parameters:Array) : *
      {
         return null;
      }
      
      public function callRWithParameters(method:Function, parameters:Array) : *
      {
         return null;
      }
      
      public function kamasToString(kamas:Number, unit:String = "-") : String
      {
         return null;
      }
      
      public function formateIntToString(val:Number, precision:int = 2) : String
      {
         return null;
      }
      
      public function stringToKamas(string:String, unit:String = "-") : Number
      {
         return 0;
      }
      
      public function getTextWithParams(textId:int, params:Array, replace:String = "%") : String
      {
         return null;
      }
      
      public function applyTextParams(pText:String, pParams:Array, pReplace:String = "%") : String
      {
         return null;
      }
      
      public function noAccent(str:String) : String
      {
         return null;
      }
      
      public function getAllIndexOf(pStringLookFor:String, pWholeString:String) : Array
      {
         return null;
      }
      
      public function changeColor(obj:Object, color:Number, depth:int, unColor:Boolean = false) : void
      {
      }
      
      public function sortOnString(list:*, field:String = "", ascending:Boolean = true) : void
      {
      }
      
      public function sort(target:*, field:String, ascendand:Boolean = true, isNumeric:Boolean = false) : *
      {
         return null;
      }
      
      public function filter(target:*, pattern:*, field:String) : *
      {
         return null;
      }
      
      public function getTiphonEntityLook(pEntityId:Number) : Object
      {
         return null;
      }
      
      public function getRealTiphonEntityLook(pEntityId:Number, pWithoutMount:Boolean = false) : Object
      {
         return null;
      }
      
      public function getLookFromContext(pEntityId:Number, pForceCreature:Boolean = false) : Object
      {
         return null;
      }
      
      public function getLookFromContextInfos(pInfos:GameContextActorInformations, pForceCreature:Boolean = false) : Object
      {
         return null;
      }
      
      public function isCreature(pEntityId:Number) : Boolean
      {
         return false;
      }
      
      public function isCreatureFromLook(pLook:Object) : Boolean
      {
         return false;
      }
      
      public function isIncarnation(pEntityId:Number) : Boolean
      {
         return false;
      }
      
      public function isIncarnationFromLook(pLook:Object) : Boolean
      {
         return false;
      }
      
      public function isCreatureMode() : Boolean
      {
         return false;
      }
      
      public function getCreatureLook(pEntityId:Number) : Object
      {
         return null;
      }
      
      public function getGfxUri(pGfxId:int) : String
      {
         return null;
      }
      
      public function encodeToJson(value:*) : String
      {
         return null;
      }
      
      public function decodeJson(value:String) : *
      {
         return null;
      }
      
      public function getObjectsUnderPoint() : Array
      {
         return null;
      }
      
      public function isCharacteristicSpell(pSpellWrapper:SpellWrapper, pCharacteristicId:int, pRecursive:Boolean = false) : Boolean
      {
         return false;
      }
      
      public function getCurrentMonsterXp(monsterLvl:uint, isBoss:Boolean) : uint
      {
         return 0;
      }
   }
}

