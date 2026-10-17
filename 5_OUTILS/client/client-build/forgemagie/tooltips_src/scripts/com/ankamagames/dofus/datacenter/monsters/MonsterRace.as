package com.ankamagames.dofus.datacenter.monsters
{
   public class MonsterRace
   {
      
      public function MonsterRace()
      {
         super();
      }
      
      public function get id() : int
      {
         return new int();
      }
      
      public function get superRaceId() : int
      {
         return new int();
      }
      
      public function get nameId() : uint
      {
         return new uint();
      }
      
      public function get monsters() : Vector.<uint>
      {
         return new Vector.<uint>();
      }
      
      public function get aggressiveZoneSize() : int
      {
         return new int();
      }
      
      public function get aggressiveLevelDiff() : int
      {
         return new int();
      }
      
      public function get aggressiveImmunityCriterion() : String
      {
         return new String();
      }
      
      public function get aggressiveAttackDelay() : int
      {
         return new int();
      }
      
      public function get name() : String
      {
         return null;
      }
      
      public function get canAttack() : Boolean
      {
         return false;
      }
   }
}

