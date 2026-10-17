package com.ankamagames.dofus.datacenter.quest
{
   import com.ankamagames.dofus.datacenter.items.criterion.GroupItemCriterion;
   
   public class AchievementReward
   {
      
      public function AchievementReward()
      {
         super();
      }
      
      public function get id() : uint
      {
         return new uint();
      }
      
      public function get achievementId() : uint
      {
         return new uint();
      }
      
      public function get criteria() : String
      {
         return new String();
      }
      
      public function get kamasRatio() : Number
      {
         return new Number();
      }
      
      public function get experienceRatio() : Number
      {
         return new Number();
      }
      
      public function get kamasScaleWithPlayerLevel() : Boolean
      {
         return new Boolean();
      }
      
      public function get itemsReward() : Vector.<uint>
      {
         return new Vector.<uint>();
      }
      
      public function get itemsQuantityReward() : Vector.<uint>
      {
         return new Vector.<uint>();
      }
      
      public function get emotesReward() : Vector.<uint>
      {
         return new Vector.<uint>();
      }
      
      public function get spellsReward() : Vector.<uint>
      {
         return new Vector.<uint>();
      }
      
      public function get titlesReward() : Vector.<uint>
      {
         return new Vector.<uint>();
      }
      
      public function get ornamentsReward() : Vector.<uint>
      {
         return new Vector.<uint>();
      }
      
      public function get achievement() : Achievement
      {
         return null;
      }
      
      public function get conditions() : GroupItemCriterion
      {
         return null;
      }
   }
}

