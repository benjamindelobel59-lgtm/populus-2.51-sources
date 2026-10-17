package com.ankamagames.dofus.datacenter.monsters
{
   import com.ankamagames.dofus.datacenter.items.criterion.GroupItemCriterion;
   
   public class MonsterDropCoefficient
   {
      
      public function MonsterDropCoefficient()
      {
         super();
      }
      
      public function get monsterId() : uint
      {
         return new uint();
      }
      
      public function get monsterGrade() : uint
      {
         return new uint();
      }
      
      public function get dropCoefficient() : Number
      {
         return new Number();
      }
      
      public function get criteria() : String
      {
         return new String();
      }
      
      public function get monster() : Monster
      {
         return null;
      }
      
      public function get conditions() : GroupItemCriterion
      {
         return null;
      }
   }
}

