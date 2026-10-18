package com.ankamagames.dofus.datacenter.monsters
{
   import com.ankamagames.dofus.datacenter.items.criterion.GroupItemCriterion;
   
   public class MonsterDrop
   {
      
      public function MonsterDrop()
      {
         super();
      }
      
      public function get dropId() : uint
      {
         return new uint();
      }
      
      public function get monsterId() : int
      {
         return new int();
      }
      
      public function get objectId() : int
      {
         return new int();
      }
      
      public function get percentDropForGrade1() : Number
      {
         return new Number();
      }
      
      public function get percentDropForGrade2() : Number
      {
         return new Number();
      }
      
      public function get percentDropForGrade3() : Number
      {
         return new Number();
      }
      
      public function get percentDropForGrade4() : Number
      {
         return new Number();
      }
      
      public function get percentDropForGrade5() : Number
      {
         return new Number();
      }
      
      public function get count() : int
      {
         return new int();
      }
      
      public function get criteria() : String
      {
         return new String();
      }
      
      public function get hasCriteria() : Boolean
      {
         return new Boolean();
      }
      
      public function get specificDropCoefficient() : Vector.<MonsterDropCoefficient>
      {
         return new Vector.<MonsterDropCoefficient>();
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

