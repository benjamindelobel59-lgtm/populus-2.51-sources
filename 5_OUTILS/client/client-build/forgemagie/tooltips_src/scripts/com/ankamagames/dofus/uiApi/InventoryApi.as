package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.internalDatacenter.items.ItemWrapper;
   import com.ankamagames.dofus.internalDatacenter.items.QuantifiedItemWrapper;
   import com.ankamagames.dofus.internalDatacenter.items.SimpleTextureWrapper;
   
   public class InventoryApi
   {
      
      public function InventoryApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function getStorageObjectGID(pObjectGID:uint, quantity:uint = 1) : Object
      {
         return null;
      }
      
      public function getStorageObjectsByType(objectType:uint) : Array
      {
         return null;
      }
      
      public function getItemQty(pObjectGID:uint, pObjectUID:uint = 0) : uint
      {
         return 0;
      }
      
      public function getItemByGID(objectGID:uint) : ItemWrapper
      {
         return null;
      }
      
      public function getItemFromInventoryByUID(objectUID:uint) : ItemWrapper
      {
         return null;
      }
      
      public function getQuantifiedItemByGIDInInventoryOrMakeUpOne(objectGID:uint) : QuantifiedItemWrapper
      {
         return null;
      }
      
      public function getItem(objectUID:uint) : ItemWrapper
      {
         return null;
      }
      
      public function getEquipementItemByPosition(pPosition:uint) : ItemWrapper
      {
         return null;
      }
      
      public function getEquipement() : Object
      {
         return null;
      }
      
      public function getEquipementForPreset() : Array
      {
         return null;
      }
      
      public function getVoidItemForPreset(index:int) : SimpleTextureWrapper
      {
         return null;
      }
      
      public function getMaxItemsCountForPreset() : int
      {
         return 0;
      }
      
      public function getPositionForPresetItemIndex(index:int) : int
      {
         return 0;
      }
      
      public function getInvisibleEquipmentPositions() : Array
      {
         return null;
      }
      
      public function getCurrentWeapon() : ItemWrapper
      {
         return null;
      }
      
      public function getBuilds() : Array
      {
         return null;
      }
      
      public function setBuildId(id:int) : void
      {
      }
      
      public function getBuildId() : int
      {
         return 0;
      }
      
      public function removeSelectedItem() : Boolean
      {
         return false;
      }
   }
}

