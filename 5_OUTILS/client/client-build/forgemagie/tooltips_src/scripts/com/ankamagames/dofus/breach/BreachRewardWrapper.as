package com.ankamagames.dofus.breach
{
   import com.ankamagames.dofus.datacenter.items.criterion.GroupItemCriterion;
   
   public class BreachRewardWrapper
   {
      
      public function BreachRewardWrapper()
      {
         super();
      }
      
      public function get id() : uint
      {
         return new uint();
      }
      
      public function get buyLocks() : Vector.<uint>
      {
         return new Vector.<uint>();
      }
      
      public function get bought() : Boolean
      {
         return new Boolean();
      }
      
      public function get buyCriterion() : GroupItemCriterion
      {
         return new GroupItemCriterion();
      }
      
      public function get price() : uint
      {
         return new uint();
      }
   }
}

