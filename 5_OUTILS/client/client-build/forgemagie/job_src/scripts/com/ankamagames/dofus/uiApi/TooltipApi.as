package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.berilia.types.tooltip.TooltipRectangle;
   import com.ankamagames.dofus.internalDatacenter.items.ItemWrapper;
   import com.ankamagames.dofus.internalDatacenter.items.WeaponWrapper;
   import com.ankamagames.dofus.internalDatacenter.spells.SpellWrapper;
   import com.ankamagames.dofus.modules.utils.ItemTooltipSettings;
   import com.ankamagames.dofus.modules.utils.SpellTooltipSettings;
   
   public class TooltipApi
   {
      
      public function TooltipApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function setDefaultTooltipUiScript(module:String, ui:String) : void
      {
      }
      
      public function createTooltip(baseUri:String, containerUri:String, separatorUri:String = null) : Object
      {
         return null;
      }
      
      public function createTooltipBlock(onAllChunkLoadedCallback:Function, contentGetter:Function, chunkType:String = "chunks") : Object
      {
         return null;
      }
      
      public function registerTooltipAssoc(targetClass:*, makerName:String) : void
      {
      }
      
      public function registerTooltipMaker(makerName:String, makerClass:Class, scriptClass:Class = null) : void
      {
      }
      
      public function createChunkData(name:String, uri:String) : Object
      {
         return null;
      }
      
      public function place(target:*, showDirectionalArrow:Object, point:uint = 6, relativePoint:uint = 0, offset:* = 3, checkSuperposition:Boolean = false, cellId:int = -1, offsetRect:Object = null, alwaysDisplayed:Boolean = true) : void
      {
      }
      
      public function placeArrow(target:*) : Object
      {
         return null;
      }
      
      public function getSpellTooltipInfo(spellWrapper:SpellWrapper, shortcutKey:String = null) : Object
      {
         return null;
      }
      
      public function getItemTooltipInfo(itemWrapper:ItemWrapper, shortcutKey:String = null) : Object
      {
         return null;
      }
      
      public function getWeaponTooltipInfo(spellWrapper:SpellWrapper, shortcutKey:String = null, makerParams:Object = null, weapon:WeaponWrapper = null) : Object
      {
         return null;
      }
      
      public function getSpellTooltipCache() : int
      {
         return 0;
      }
      
      public function resetSpellTooltipCache() : void
      {
      }
      
      public function createTooltipRectangle(x:Number = 0, y:Number = 0, width:Number = 0, height:Number = 0) : TooltipRectangle
      {
         return null;
      }
      
      public function createSpellSettings() : SpellTooltipSettings
      {
         return null;
      }
      
      public function createItemSettings() : ItemTooltipSettings
      {
         return null;
      }
      
      public function update(ttCacheName:String, ... params) : void
      {
      }
      
      public function adjustTooltipPositions(tooltipNames:Array, sourceName:String, offset:int = 0) : void
      {
      }
   }
}

