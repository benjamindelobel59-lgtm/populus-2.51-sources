package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.datacenter.mounts.Mount;
   import com.ankamagames.dofus.internalDatacenter.items.ItemWrapper;
   
   public class MountApi
   {
      
      public function MountApi()
      {
         super();
      }
      
      public function getRiderEntityLook(look:*) : Object
      {
         return null;
      }
      
      public function getMount(modelId:uint) : Mount
      {
         return null;
      }
      
      public function getStableList() : Array
      {
         return null;
      }
      
      public function getPaddockList() : Array
      {
         return null;
      }
      
      public function getInventoryList() : Object
      {
         return null;
      }
      
      public function getCurrentPaddock() : Object
      {
         return null;
      }
      
      public function isCertificateValid(certificate:ItemWrapper) : Boolean
      {
         return false;
      }
   }
}

